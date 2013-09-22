package spark.examples

import spark._
import SparkContext._

/** Computes an approximation to pi */
object CedarExp {
  def main(args: Array[String]) {
    if (args.length == 0) {
      System.err.println("Usage: CedarExp <master> <deadline> <useCedar>[<slices>]")
      System.exit(1)
    }
    val spark = new SparkContext(args(0), "CedarExp",
      System.getenv("SPARK_HOME"), Seq(System.getenv("SPARK_EXAMPLES_JAR")))
    val deadline = args(1).toInt
    val u = args(2).toInt
    var useCedar = true
    if (u == 0) useCedar = false
    val pageCount = spark.parallelize(1 to 800, 800)
    val a = pageCount.cedar(16, deadline, 4.4, 1.15, true, 2.94, 0.52, useCedar).initialValue.size
    println(deadline + ": " + a + " with useCedar: " + useCedar)
    System.exit(0)
  }
}
