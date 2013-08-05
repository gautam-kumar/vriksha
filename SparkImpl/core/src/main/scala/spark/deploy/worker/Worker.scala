package spark.deploy.worker

import scala.collection.mutable.{ArrayBuffer, HashMap}
import akka.actor._
import akka.util.duration._
import spark.{Logging, Utils}
import spark.util.AkkaUtils
import spark.deploy._
import akka.remote.{RemoteClientLifeCycleEvent, RemoteClientShutdown, RemoteClientDisconnected}
import java.text.SimpleDateFormat
import java.util.Date
import spark.deploy.RegisterWorker
import spark.deploy.LaunchExecutor
import spark.deploy.RegisterWorkerFailed
import spark.deploy.master.Master
import java.io.File

private[spark] class Worker(
    ip: String,
    port: Int,
    webUiPort: Int,
    cores: Int,
    memory: Int,
    masterUrl: String,
    actorSystemUrl: String,
    workDirPath: String)
  extends Actor with Logging {

  val DATE_FORMAT = new SimpleDateFormat("yyyyMMddHHmmss")  // For worker and executor IDs

  // Send a heartbeat every (heartbeat timeout) / 4 milliseconds
  val HEARTBEAT_MILLIS = System.getProperty("spark.worker.timeout", "60").toLong * 1000 / 4

  var master: ActorRef = null
  var masterWebUiUrl : String = ""
  var masterAddress: Address = null
  val workerId = generateWorkerId()
  var sparkHome: File = null
  var workDir: File = null
  val executors = new HashMap[String, ExecutorRunner]
  val finishedExecutors = new HashMap[String, ExecutorRunner]
  val publicAddress = {
    val envVar = System.getenv("SPARK_PUBLIC_DNS")
    if (envVar != null) envVar else ip
  }

  var coresUsed = 0
  var memoryUsed = 0

  def coresFree: Int = cores - coresUsed
  def memoryFree: Int = memory - memoryUsed

  def createWorkDir() {
    workDir = Option(workDirPath).map(new File(_)).getOrElse(new File(sparkHome, "work"))
    try {
      if (!workDir.exists() && !workDir.mkdirs()) {
        logError("Failed to create work directory " + workDir)
        System.exit(1)
      }
    } catch {
      case e: Exception =>
        logError("Failed to create work directory " + workDir, e)
        System.exit(1)
    }
  }

  override def preStart() {
    logInfo("Starting Spark worker %s:%d with %d cores, %s RAM".format(
      ip, port, cores, Utils.memoryMegabytesToString(memory)))
    sparkHome = new File(Option(System.getenv("SPARK_HOME")).getOrElse("."))
    logInfo("Spark home: " + sparkHome)
    createWorkDir()
    connectToMaster()
    startWebUi()
  }

  def connectToMaster() {
    logInfo("Connecting to master " + masterUrl)
    master = context.actorFor(Master.toAkkaUrl(masterUrl))
    master ! RegisterWorker(workerId, ip, port, cores, memory, webUiPort, publicAddress)
    context.system.eventStream.subscribe(self, classOf[RemoteClientLifeCycleEvent])
    context.watch(master) // Doesn't work with remote actors, but useful for testing
  }

  def startWebUi() {
    val webUi = new WorkerWebUI(context.system, self, workDir)
    try {
      AkkaUtils.startSprayServer(context.system, "0.0.0.0", webUiPort, webUi.handler)
    } catch {
      case e: Exception =>
        logError("Failed to create web UI", e)
        System.exit(1)
    }
  }

  override def receive = {
    case RegisteredWorker(url) =>
      masterWebUiUrl = url
      masterAddress = sender.path.address
      logInfo("Successfully registered with master")
      context.system.scheduler.schedule(0 millis, HEARTBEAT_MILLIS millis) {
        master ! Heartbeat(workerId)
      }

    case RegisterWorkerFailed(message) =>
      logError("Worker registration failed: " + message)
      System.exit(1)

    case LaunchExecutor(appId, execId, appDesc, cores_, memory_, execSparkHome_) =>
      logInfo("Asked to launch executor %s/%d for %s".format(appId, execId, appDesc.name))
      val manager = new ExecutorRunner(
        appId, execId, appDesc, cores_, memory_, self, workerId, ip, new File(execSparkHome_),
        workDir, context.system, actorSystemUrl)
      executors(appId + "/" + execId) = manager
      manager.start()
      coresUsed += cores_
      memoryUsed += memory_
      master ! ExecutorStateChanged(appId, execId, ExecutorState.RUNNING, None, None)

    case ExecutorStateChanged(appId, execId, state, message, exitStatus) =>
      master ! ExecutorStateChanged(appId, execId, state, message, exitStatus)
      val fullId = appId + "/" + execId
      if (ExecutorState.isFinished(state)) {
        val executor = executors(fullId)
        logInfo("Executor " + fullId + " finished with state " + state +
          message.map(" message " + _).getOrElse("") +
          exitStatus.map(" exitStatus " + _).getOrElse(""))
        finishedExecutors(fullId) = executor
        executors -= fullId
        coresUsed -= executor.cores
        memoryUsed -= executor.memory
      }

    case KillExecutor(appId, execId) =>
      val fullId = appId + "/" + execId
      executors.get(fullId) match {
        case Some(executor) =>
          logInfo("Asked to kill executor " + fullId)
          executor.kill()
        case None =>
          logInfo("Asked to kill unknown executor " + fullId)
      }

    case Terminated(actorRef) if actorRef == master =>
      masterDisconnected()

    case RemoteClientDisconnected(transport, address) =>
      if (address == masterAddress || masterAddress == null) {
        masterDisconnected()
      }

    case RemoteClientShutdown(transport, address) =>
      if (address == masterAddress || masterAddress == null) {
        masterDisconnected()
      }
      
    case RequestWorkerState => {
      sender ! WorkerState(ip, port, workerId, executors.values.toList,
        finishedExecutors.values.toList, masterUrl, cores, memory, 
        coresUsed, memoryUsed, masterWebUiUrl)
    }
  }

  def masterDisconnected() {
    // TODO: It would be nice to try to reconnect to the master, but just shut down for now.
    // (Note that if reconnecting we would also need to assign IDs differently.)
    logError("Connection to master failed! Shutting down.")
    executors.values.foreach(_.kill())
    System.exit(1)
  }

  def generateWorkerId(): String = {
    "worker-%s-%s-%d".format(DATE_FORMAT.format(new Date), ip, port)
  }

  override def postStop() {
    executors.values.foreach(_.kill())
  }
}

private[spark] object Worker {
  def main(argStrings: Array[String]) {
    val args = new WorkerArguments(argStrings)
    val (actorSystem, _) = startSystemAndActor(args.ip, args.port, args.webUiPort, args.cores,
      args.memory, args.master, args.workDir)
    actorSystem.awaitTermination()
  }

  def startSystemAndActor(host: String, port: Int, webUiPort: Int, cores: Int, memory: Int,
    masterUrl: String, workDir: String, workerNumber: Option[Int] = None): (ActorSystem, Int) = {
    // The LocalSparkCluster runs multiple local sparkWorkerX actor systems
    val systemName = "sparkWorker" + workerNumber.map(_.toString).getOrElse("")
    val (actorSystem, boundPort) = AkkaUtils.createActorSystem(systemName, host, port)
    val systemUrl = "akka://%s@%s:%s".format(systemName, host, boundPort)
    val actor = actorSystem.actorOf(
      Props(new Worker(host, boundPort, webUiPort, cores, memory, masterUrl, systemUrl, workDir)),
      name = "Worker")
    (actorSystem, boundPort)
  }

}
