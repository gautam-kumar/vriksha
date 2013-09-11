package spark.rdd

import spark.{RDD, Partition, TaskContext}
import org.apache.commons.math.distribution.NormalDistributionImpl
import scala.math

private[spark]
class MapAndWaitPartitionsRDD[T: ClassManifest, U: ClassManifest](
    prev: RDD[T],
    f: Iterator[T] => Iterator[U],
    logDistMean: Double,
    logDistSigma: Double,
    preservesPartitioning: Boolean = false)
  extends RDD[U](prev) {
  
  val rng = new NormalDistributionImpl(logDistMean, logDistSigma)
  override val partitioner =
    if (preservesPartitioning) firstParent[T].partitioner else None

  override def getPartitions: Array[Partition] = firstParent[T].partitions

  override def compute(split: Partition, context: TaskContext) = {
    val a = f(firstParent[T].iterator(split, context))
    val s = (math.exp(rng.sample()) * 1000).toLong
    logInfo("Sleeping for " + s)
    Thread.sleep(s)
    a
  }
}