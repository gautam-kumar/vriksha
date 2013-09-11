package spark

import java.io._
import java.net._
import java.util.{Locale, Random, UUID}
import java.util.concurrent.{Executors, ThreadFactory, ThreadPoolExecutor}
import org.apache.hadoop.conf.Configuration
import org.apache.hadoop.fs.{Path, FileSystem, FileUtil}
import scala.collection.mutable.ArrayBuffer
import scala.collection.JavaConversions._
import scala.io.Source
import com.google.common.io.Files
import com.google.common.util.concurrent.ThreadFactoryBuilder
import scala.Some
import spark.serializer.SerializerInstance

/**
 * Various utility methods used by Spark.
 */
object CedarUtils extends Logging {
  /** Serialize an object using Java serialization */
  def ReadFacebookTaskDistribution(): Array[Array[Int]] = {
    val lines = scala.io.Source.fromFile("MapDurationsPruned.txt").getLines
    val numJobs = lines.next().toInt
    var a = new Array[Array[Int]](numJobs)
    var lN = 1
    for (i <- 0 until numJobs) {
      var numTasks = lines.next().toInt; lN = lN + 1
      a(i) = new Array[Int](numTasks)
      for (j <- 0 until numTasks) {
        a(i)(j) = lines.next().toInt; lN = lN + 1 
      }
    }
    a    
  }
}
