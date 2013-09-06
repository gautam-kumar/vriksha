package spark.rdd

import spark.{RDD, Partition, TaskContext}
import org.apache.commons.math.distribution.NormalDistributionImpl
import scala.math

private[spark]
class WaitPartitionsRDD[T: ClassManifest](
    prev: RDD[T],
    logDistMean: Double,
    logDistSigma: Double,
    preservesPartitioning: Boolean = false)
  extends RDD[T](prev) {
  
  val rng = new NormalDistributionImpl(logDistMean, logDistSigma)
  override val partitioner =
    if (preservesPartitioning) firstParent[T].partitioner else None

  override def getPartitions: Array[Partition] = firstParent[T].partitions

  override def compute(split: Partition, context: TaskContext) = {
    val a = firstParent[T].iterator(split, context)
    val s = (math.exp(rng.sample()) * 1000).toLong
    logInfo("Sleeping for " + s)
    Thread.sleep(s)
    a
  }
}
