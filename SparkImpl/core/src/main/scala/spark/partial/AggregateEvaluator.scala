package spark.partial

import cern.jet.stat.Probability
import spark.Logging
/**
 * An ApproximateEvaluator for aggregation.
 *
 * TODO: There's currently a lot of shared code between this and GroupedCountEvaluator. It might
 * be best to make this a special case of GroupedCountEvaluator with one group.
 */
private[spark] class AggregateEvaluator[T : ClassManifest](totalOutputs: Int)
  extends ApproximateEvaluator[Array[T], Array[T]] with Logging {

  var outputsMerged = 0
  var sum: Array[T] = new Array[T](0) 

  override def merge(outputId: Int, taskResult: Array[T]) {
    logInfo("<G> AggregateEvaluator's merge called")
    outputsMerged += 1
    sum = sum ++ taskResult
  }

  override def currentResult(): Array[T] = {
    sum
    /*
    if (outputsMerged == totalOutputs) {
      new BoundedDouble(sum, 1.0, sum, sum)
    } else if (outputsMerged == 0) {
      new BoundedDouble(0, 0.0, Double.NegativeInfinity, Double.PositiveInfinity)
    } else {
      val p = outputsMerged.toDouble / totalOutputs
      val mean = (sum + 1 - p) / p
      val variance = (sum + 1) * (1 - p) / (p * p)
      val stdev = math.sqrt(variance)
      val confFactor = Probability.normalInverse(1 - (1 - confidence) / 2)
      val low = mean - confFactor * stdev
      val high = mean + confFactor * stdev
      new BoundedDouble(mean, confidence, low, high)
    }
    */
  }
}
