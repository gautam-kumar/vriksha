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
    numPartitions : Int)
  extends RDD[T](sc, Nil)
  with Serializable {
 
  val numPartitionsInRdd1 = rdd1.partitions.size
  val numParentsPerPartition = rdd1.partitions.size / numPartitions

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
    logInfo("<G> Here it is ")
    for (i <- 1 until currSplit.s1.length) {
        result ++= rdd1.preferredLocations(currSplit.s1(i))
    }
    logInfo("<G> GetPreferredLocations Result is " + result)
    result
  }

  override def compute(split: Partition, context: TaskContext) = {
    val currSplit = split.asInstanceOf[AggregatePartition]
    logInfo("<G> AggregateRDD's compute with " + rdd1 + ", " + currSplit.s1.size)

    val sdf = new SimpleDateFormat("yyyy/MM/dd HH:mm:ss");
     
    var a = new Array[Array[T]](currSplit.s1.size)
    var numTasksCompleted = 0
    implicit val ec = ExecutionContext.fromExecutorService(Executors.newFixedThreadPool(50))
    logInfo("Starting execution at " + System.currentTimeMillis());
    val tasks: IndexedSeq[Future[Tuple2[Int, Array[T]]]] = for (i <- 0 until currSplit.s1.size) yield Future {
    	logInfo("Executing task " + i + " for index " + split.index + " at " + System.currentTimeMillis());
    	val t = rdd1.iterator(currSplit.s1(i), context)
        val r = t.toArray
        logInfo("Result size for " + i + " is " + r.size + " at " + System.currentTimeMillis());
        (i, r)
    } 
    for (f <- tasks) {
    	f onSuccess {
            case res => {
		this.synchronized {
		  logInfo("Result 1: " + res._1); 
		  a(res._1) = res._2; 
		  numTasksCompleted += 1; 
		}
		logInfo("<G> NumCompleted: " + numTasksCompleted);
	    } 
        }    
        f onFailure {
            case t => println("An error has occured: " + t.getMessage)
        }
    }
    
    while (numTasksCompleted < (currSplit.s1.size)) {
	logInfo("NumCompleted " + numTasksCompleted + " is less than one so sleeping more")
    	Thread.sleep(1000L) 
    }
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
