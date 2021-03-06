package spark.rdd

import java.io.{ObjectOutputStream, IOException}
// import scala.actors.Futures._
import java.util.concurrent.Executors
import akka.dispatch.Future
import akka.dispatch.{Future, ExecutionContext, Promise }
import org.apache.commons.math.distribution.NormalDistributionImpl
import spark._
import java.text.SimpleDateFormat
import java.util.Date
import java.util.concurrent.atomic.AtomicInteger
import scala.math.{log => natLog, pow}

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
    deadline : Int, // in seconds
    initialMeanEstimate: Double,
    initialSigmaEstimate: Double,
    aboveMean: Double,
    aboveSigma: Double,
    useCedar: Boolean = true,
    empirical: Boolean = false) // Deadline in milliseconds
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
  var mean = initialMeanEstimate
  var sigma = initialSigmaEstimate
  var rPrev = 0.0
  var sumSquares = 0.0
  var beginTime = 0

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
    var maxQuality = 0.0
    var time = 1
    var maxTime = 1
    val increment = 1
    val k = 50
    val nDist1 = new NormalDistributionImpl(m1, s1)
    val nDist2 = new NormalDistributionImpl(m2, s2)
    while (time < deadline - increment) {
    	val aboveQualityWithoutWait = nDist2.cumulativeProbability(natLog(deadline - time))
    	val aboveQualityWithWait = nDist2.cumulativeProbability(natLog(deadline - time - increment))
    	val c1 = nDist1.cumulativeProbability(natLog(time))
    	val c1t = nDist1.cumulativeProbability(natLog(time + increment))
    	val gain = (c1t - c1) * aboveQualityWithoutWait
    	val loss = (c1 - pow(c1, k)) *  (aboveQualityWithoutWait - aboveQualityWithWait)
    	quality += (gain - loss)
    	if (quality > maxQuality) {
          maxQuality = quality
          maxTime = time;
        }

    	time += increment
    } 

    logInfo("<G> MaxQuality is " + maxQuality + " at time " + maxTime)
    maxTime
  }

  private def updateMeanAndSigma(n: Int, curTaskCompletionTime: Double) = {
    // TODO: verify this code 
    val r = natLog(curTaskCompletionTime)
    // If this is the first task completes
    if (n != 1) {
        val ePrev = orderStats(id - 1)
        val e = orderStats(id)
    
        val newSigma = (r - rPrev) / (e - ePrev)
        val newMean = r - e * newSigma

        mean = (mean * (n - 1) + newMean) / n
        sigma = (sigma * (n - 1) + newSigma) / n
    } 
    rPrev = r
  }

  private def updateMeanAndSigmaEmpirical(n: Int, curTaskCompletionTime: Double) = {
      val r = natLog(curTaskCompletionTime)
      sumSquares += r * r
      mean = (mean * (n - 1) + r) / n
      val eX2 = (1.0 * sumSquares) / n
      val sigmaT = math.sqrt(eX2 - mean * mean)
      logInfo("<G> update: " + sumSquares + ": ex2: " + eX2 + " : mean2: " + mean * mean + " sigmaT : " + sigmaT)
      if (sigmaT > 0.1) sigma = sigmaT
  }

  override def compute(split: Partition, context: TaskContext) = {
    val currSplit = split.asInstanceOf[AggregatePartition]
    logInfo("<G> AggregateRDD's compute with " + rdd1 + ", " + currSplit.s1.size)

    // Initialize an array to store the individual task responses 
    var a = new Array[Array[T]](currSplit.s1.size)
    var numTasksCompleted = 0
    implicit val ec = ExecutionContext.fromExecutorService(Executors.newFixedThreadPool(50))
    logInfo("Starting execution at " + System.currentTimeMillis());
    val env = SparkEnv.get
    val beginTime = System.nanoTime
    val tasks: IndexedSeq[Future[Tuple2[Int, Array[T]]]] = for (i <- 0 until currSplit.s1.size) yield Future {
    	// SparkEnv is a thread-local variable -- need to copy it
    	SparkEnv.set(env)
    	logInfo("Executing task " + i + " for split " + split.index + " at " + System.currentTimeMillis());
    	val t = rdd1.iterator(currSplit.s1(i), context)
        val r = t.toArray
    	logInfo("Finished task " + i + " for split " + split.index + " at " + System.currentTimeMillis());
        (i, r)
    } 
    // TODO: Must learn distribution online
    var timeOut = 0.0
    if (useCedar) timeOut = getOptimalWaitTime(deadline / 1000, mean, sigma, aboveMean, aboveSigma) * 1000 // Conversion to ms
    else timeOut = deadline.toDouble * (157.78 / (157.78 + 22))
    logInfo("<G> TimeOut Computed as: " + timeOut)
    
    // Add Listeners when each of the tasks complete 
    for (f <- tasks) {
    	f onSuccess {
        case res => {
		      this.synchronized {
		        a(res._1) = res._2;
		        numTasksCompleted += 1; 
		        // Code to update wait time
            /*if (useCedar) {
              if (empirical) {
		            updateMeanAndSigmaEmpirical(numTasksCompleted, (System.nanoTime - beginTime) / 1000000000.0);
              } else {
                updateMeanAndSigma(numTasksCompleted, (System.nanoTime - beginTime) / 1000000000.0);
              }
              timeOut = getOptimalWaitTime(deadline / 1000, mean, sigma, aboveMean, aboveSigma) * 1000
            }*/
          } 
          logInfo("<G> " + numTasksCompleted + " Sleep time updated to " + timeOut + " with mean: " + mean + " sigma: " + sigma)
	      } 
      }    
      f onFailure {
            case t => println("An error has occured: " + t.getMessage)
      }
    }


   while ((((System.nanoTime - beginTime)/1000000 < timeOut) || (numTasksCompleted < 1)) && (numTasksCompleted < currSplit.s1.size)) { 
    	Thread.sleep(1000L)
    //	logInfo("<G> Sleeping more: " + numTasksCompleted + ", " +
    	//currSplit.s1.size + "--" + (numTasksCompleted < currSplit.s1.size))
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
