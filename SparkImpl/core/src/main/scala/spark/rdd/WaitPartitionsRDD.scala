package spark.rdd

import spark.{RDD, Partition, TaskContext}

private[spark]
class WaitPartitionsRDD[T: ClassManifest](
    prev: RDD[T],
    preservesPartitioning: Boolean = false)
  extends RDD[T](prev) {

  override val partitioner =
    if (preservesPartitioning) firstParent[T].partitioner else None

  override def getPartitions: Array[Partition] = firstParent[T].partitions

  override def compute(split: Partition, context: TaskContext) = {
      val a = firstParent[T].iterator(split, context).take(20)
      logInfo("Sleeping now")
      Thread.sleep(10000)
      logInfo("Sleeping Done")
      a
    }
}
