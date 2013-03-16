using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class GlobalVariables
{

	public static HLA hla;
	public static List<MLA>[] mlas;
	public static List<Worker> workers;
//	public static Dictionary<int, double> waitTimeGlobal = new Dictionary<int,double> ();
	public static Dictionary<int, List<Task>>[] mlaTasks;
	// TODO Removed for the Ideal
	/*
	public static void UpdateGlobalWaitTime (int id)
	{
		TimeSpan sum = 0;
		foreach (MLA mla in mlas) {
			if (mla.waitTime.ContainsKey (id)) {
				sum += mla.waitTime [id];
			} else {
				sum += GlobalVariables.mlaWaitTimeSec;
			}
		}
		if (waitTimeGlobal.ContainsKey (id)) {
			waitTimeGlobal [id] = sum / mlas.Count;
		} else {
			waitTimeGlobal.Add (id, sum / mlas.Count);
		}
	}*/

	public static List<Request> requests;
	public static List<double> flowCompletionTimesMla;
	public static List<double> flowCompletionTimesTla;

	// Initialization Variables
	public static TimeSpan currentTime = new TimeSpan(0);
	public static int numFlowsMissed = 0;


	/********************************************/
	// Simulation Parameters
	/********************************************/
	// Request parameters
	public static int NumRequestsToSimulate = 1;
	public static int NumEdgeRequestsToDelete = 0;
	public static TimeSpan timeIncrement = new TimeSpan (0, 0, 0, 0, 1);

	// Topology and machine parameters
	public static int[] Fanouts;
	public static int NumWorkers; // TODO: Should also go
	public static int NumQueuesInWorker = 30;
	public static int NumQueuesInMla = 30;

	// Network parameters
	public static double capacityBps = 1000000000.0;
	public static double flowSizeBytes = 50000;
	public static double flowSizeMinBytes = 10;
	public static double flowSizeMaxBytes = 10;


	// Processing distributions type
	public static DistType workerDistType = DistType.LogNormal;
	public static DistType mlaDistType = DistType.LogNormal;
	// Worker
	public static double workerComputationTimeMeanMs = 100; // TODO: Fix!! Assume 10 threads at Workers
	public static double workerComputationTimeStdevMs = 10;
	public static double workerComputationTimeSigmaPerRequestMs = 10;
	public static double workerComputationTimeMaxPerRequest = 5000;
	public static double workerLogNormalTimeMean = 2.94;
	public static double workerLogNormalTimeSigma = 0.55;
	// MLA
	public static double mlaComputationTimeMeanMs = 100; // TODO: Fix!! Assume 20 threads at MLA
	public static double mlaComputationTimeStdevMs = 10;
	public static double mlaComputationTimeSigmaPerRequestMs = 20;
	public static double mlaComputationTimeMaxPerRequest = 5000;
	public static double mlaLogNormalTimeMean = 2.94;
	public static double mlaLogNormalTimeSigma = 0.55;

	// Wait times
	// Wait times before sending partial responses; Set to very high value if 
	// wait for all responses
	public static TimeSpan[] mlaWaitTimes;
	public static TimeSpan tlaWaitTime;


	// Misc
	public static double startCountingUtilization = 8.0;
	public static double endCountingUtilization = 10.0;
	public static Randomness rnd;
	public static Dictionary<Tuple<int, int>, int> computeToWaitTime;

	public static void init (int seed)
	{
		currentTime = new TimeSpan(0, 0, 0, 0, 1);
		requests = new List<Request> ();
		flowCompletionTimesMla = new List<double> ();
		flowCompletionTimesTla = new List<double> ();
		rnd = new Randomness (seed);
		numFlowsMissed = 0;
		ReadFacebookTaskDurations (seed + 5);
		ReadDcTcpNumbers ();
		ReadLogNormalOrderStats ();
		/*
            string[] lines = System.IO.File.ReadAllLines(@"Optimal.txt");
            
            computeToWaitTime = new Dictionary<Tuple<int, int>, int>();
            fore  ach (string l in lines)
            {
                string[] split = l.Split(new char[] { ' ' });
                double mlaT = double.Parse(split[0]);
                double wlaT = double.Parse(split[1]);
                int waitTime = int.Parse(split[2]);
                int mlaI = (int)(1000.0 * mlaT);
                int wlaI = (int)(1000.0 * wlaT);
                //Console.Error.WriteLine("Adding " + mlaI + " " +
                //    wlaI + " " + waitTime);
                computeToWaitTime.Add(new Tuple<int, int>(mlaI, wlaI), waitTime);

            }
            */
	}
        

	static List<double> rttMeasurements;
	static Random rttRandom;

	public static void ReadDcTcpNumbers ()
	{
		string[] lines = System.IO.File.ReadAllLines (@"rtt_measurement.txt");
		rttMeasurements = new List<double> ();
		rttRandom = new Random ();
		int i = 0;
		foreach (string l in lines) {
			string[] split = l.Split (new char[] { ' ' });
			//Console.Error.WriteLine(">>> " + split[1]); ;
			rttMeasurements.Add (double.Parse (split [1]));
			i++;
		}
		//Console.Error.Write(rttMeasurements);
		//Console.Error.WriteLine(rttMeasurements.Sum() / rttMeasurements.Count + " " + rttMeasurements.Max());
	}

	public static double GetDcTcpSample ()
	{

		int i = rttRandom.Next (rttMeasurements.Count);
		return rttMeasurements [i];
	}

	static Dictionary<int, List<Double>> taskPerJob;
	static Random facebookRandom;

	public static void ReadFacebookTaskDurations (int seed)
	{
		facebookRandom = new Random (seed);
		taskPerJob = new Dictionary<int, List<double>> ();
		string[] lines = System.IO.File.ReadAllLines (@"MapDurationsPruned.txt");
		int numJobs = int.Parse (lines [0]);

		// Console.Error.WriteLine("NumJobs " + numJobs);
            
		int lC = 1;
		for (int jobId = 0; jobId < numJobs; jobId++) {
			int numTasks = int.Parse (lines [lC++]);
			taskPerJob [jobId] = new List<double> ();
			for (int i = 0; i < numTasks; i++) {
				taskPerJob [jobId].Add (double.Parse (lines [lC++]));
			}
		}
	}

	public static double GetFacebookSampleMs (int jobId)
	{
		jobId = jobId % taskPerJob.Keys.Count;
		//jobId += 400;
		int i = facebookRandom.Next (taskPerJob [jobId].Count);
		return taskPerJob [jobId] [i];
	}

	public static double facebookLogNormalMean = 4.4;
	public static double facebookLogNormalSigma = 1.15;
	public static double optimalWaitTime = 0;






	public static Dictionary<int, double> orderStats;

	public static void ReadLogNormalOrderStats ()
	{
		orderStats = new Dictionary<int, double> ();
		string[] lines = System.IO.File.ReadAllLines (@"OrderStatisticsLogNormal" + Fanouts[Fanouts.Length - 1] + ".txt");
		//Console.Error.WriteLine(lines.Length);
		int i = 1;
		foreach (string l in lines) {
			//Console.Error.WriteLine(l);
			orderStats [i++] = double.Parse (l);
		}
		//Console.Error.WriteLine ("Length of Order Stats Read: " + orderStats.Count);
	}

}

public enum DistType
{
	Normal,
	Exponential,
	LogNormal,
	DcTcp,
	Facebook,
	Google,
	LogNormalPerJob
}

public enum SchedulerType
{
	TCP,
	VrikshaNonKilling,
	EDFNonPreEmptive,
	EDFPreEmptive,
	D3,
	SJFPreEmptive
}

public enum TaskType
{
	WorkerTask,
	MlaTask,
	TlaTask
}  

public enum ExperimentType
{
	Cedar,
	PropSplit
}
