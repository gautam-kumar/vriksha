package spark.rdd

import java.io.{ObjectOutputStream, IOException}
// import scala.actors.Futures._
import java.util.concurrent.Executors
import akka.dispatch.Future
import akka.dispatch.{Future, ExecutionContext, Promise }

import scala.math
import spark._
import java.text.SimpleDateFormat
import java.util.Date
import java.util.concurrent.atomic.AtomicInteger

private[spark]
class AggregatePartition(
    idx: Int,
    @transient rdd1: RDD[_],
    startIndex: Int,
    endIndex: Int
  ) extends Partition {
  var s1 = rdd1.partitions.slice(startIndex, endIndex) 

  override val index: Int = idx

  @throws(classOf[IOException])
  private def writeObject(oos: ObjectOutputStream) {
    // Update the reference to parent split at the time of task serialization
    s1 = rdd1.partitions.slice(startIndex, endIndex)
    oos.defaultWriteObject()
  }
}

private[spark]
class AggregateRDD[T: ClassManifest](
    sc: SparkContext,
    var rdd1 : RDD[T],
    numPartitions : Int,
    deadline : Int) // Deadline in milliseconds
  extends RDD[T](sc, Nil)
  with Serializable {

  def readOrderStats(): Array[Double] = {
    var orderStats = new Array[Double](0)
    val orderStatsStrings = scala.io.Source.fromFile("OrderStatisticsLogNormal40.txt").getLines

    for (x <- orderStatsStrings) {
        orderStats = orderStats :+ x.toDouble 
    }
    orderStats
  }

  val numPartitionsInRdd1 = rdd1.partitions.size
  val numParentsPerPartition = rdd1.partitions.size / numPartitions
  val orderStats = readOrderStats()

  logInfo("<G> OrderStats: " + orderStats)

  override def getPartitions: Array[Partition] = {
    // create the cross product split
    logInfo("<G> AggregateRDD's getPartitions parents " + numParentsPerPartition)
    val array = new Array[Partition](numPartitions)
    logInfo("<G> Must assert that division is perfect " + numPartitions)

    for (idx <- 0 until numPartitions) {
    	val startIndex = idx * numParentsPerPartition
    	val endIndex = (idx + 1) * numParentsPerPartition
    	array(idx) = new AggregatePartition(idx, rdd1, startIndex, endIndex) 
    }
    array
  }

  override def getPreferredLocations(split: Partition): Seq[String] = {
    val currSplit = split.asInstanceOf[AggregatePartition]
    var result = rdd1.preferredLocations(currSplit.s1(0))
    for (i <- 1 until currSplit.s1.length) {
        result ++= rdd1.preferredLocations(currSplit.s1(i))
    }
    result
  }

  private def getOptimalWaitTime(deadline: Int, m1: Double, s1: Double,
                                                    m2: Double, s2: Double) = {
    var quality = 0.0
    var maxQuality = 0.0;
    var time = 1;
    while (time < deadline) {
    	val logTime = math.log(time)
    	val aboveQualityWithoutWait = 1.0;
    	time += 2
    } 
    40000
  }

  private def updateMeanAndSigma(id: Int) = {
    
  }

  override def compute(split: Partition, context: TaskContext) = {
    val currSplit = split.asInstanceOf[AggregatePartition]
    logInfo("<G> AggregateRDD's compute with " + rdd1 + ", " + currSplit.s1.size)

    val sdf = new SimpleDateFormat("yyyy/MM/dd HH:mm:ss");
     
    var a = new Array[Array[T]](currSplit.s1.size)
    var numTasksCompleted = 0
    implicit val ec = ExecutionContext.fromExecutorService(Executors.newFixedThreadPool(50))
    logInfo("Starting execution at " + System.currentTimeMillis());
    val env = SparkEnv.get
    val tasks: IndexedSeq[Future[Tuple2[Int, Array[T]]]] = for (i <- 0 until currSplit.s1.size) yield Future {
    	SparkEnv.set(env)
    	logInfo("Executing task " + i + " for split " + split.index + " at " + System.currentTimeMillis());
    	val t = rdd1.iterator(currSplit.s1(i), context)
        val r = t.toArray
    	logInfo("Finished task " + i + " for split " + split.index + " at " + System.currentTimeMillis());
        (i, r)
    } 
    for (f <- tasks) {
    	f onSuccess {
            case res => {
		this.synchronized {
		  a(res._1) = res._2;
		  numTasksCompleted += 1; 
		  // Code to update wait time
		  updateMeanAndSigma(numTasksCompleted);
		}
		logInfo("<G> NumCompleted: " + numTasksCompleted);
	    } 
        }    
        f onFailure {
            case t => println("An error has occured: " + t.getMessage)
        }
    }
    val beginTime = System.nanoTime 
    val timeOut = getOptimalWaitTime(20, 4.4, 1.15, 2.94, 0.52)
    while (((System.nanoTime - beginTime)/1000000 < timeOut) &&
    	(numTasksCompleted < currSplit.s1.size)) {
    	Thread.sleep(1000L) 
    }
    logInfo("<G> Out of sleep loop at " + (System.nanoTime - beginTime)/1000000)
    //val squares = awaitAll(2000L, tasks: _*)
    var b = Iterator[T]()
    for (x <- a) {
    	if (x != null)
    	    b = b ++ x.iterator
    }
    logInfo("Returning result at " + System.currentTimeMillis());
    b
    
  }

  override def getDependencies: Seq[Dependency[_]] = List(
    new NarrowDependency(rdd1) {
      def getParents(id: Int): Seq[Int] = {
      	logDebug("<G> GetParents Call in AggregateRDD")
      	((id * numParentsPerPartition) until ((id + 1) * numParentsPerPartition)).toList
      }
    }
  )

  override def clearDependencies() {
    logInfo("<G> AggregateRDD's clearDependencies")
    super.clearDependencies()
    rdd1 = null
  }
}
