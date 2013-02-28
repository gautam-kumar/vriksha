using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class GlobalVariables
{

	public static HLA hla;
	public static List<MLA> mlas;
	public static List<Worker> workers;
	public static Dictionary<int, double> waitTimeGlobal = new Dictionary<int,double> ();
	//public static Dictionary<int, List<double>> optimalWaitTime = new Dictionary<int, List<double>>();
	public static Dictionary<int, List<Task>> secondStageTasks = new Dictionary<int, List<Task>> ();
	public static Dictionary<int, List<double>> optimalWaitTimes = new Dictionary<int, List<double>> ();

	public static void UpdateGlobalWaitTime (int id)
	{
		double sum = 0;
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
	}

	public static List<Request> requests;
	public static List<double> flowCompletionTimesMla;
	public static List<double> flowCompletionTimesTla;

	// Initialization Variables
	public static double currentTimeSec = 0;
	public static int numFlowsMissed = 0;


	/********************************************/
	// Simulation Parameters
	/********************************************/
	// Request parameters
	public static int NumRequestsToSimulate = 1;
	public static int NumEdgeRequestsToDelete = 0;
	public static double timeIncrementSec = 0.001;
	public static double requestGenerationIntervalSec = 0.01; // TODO: 1 every 5 ms?

	// Topology and machine parameters
	public static int NumWorkerPerMla = 40;
	public static int NumMlaPerTla = 40;
	public static int NumWorkers = 1600;
	public static int NumQueuesInWorker = 30;
	public static int NumQueuesInMla = 30;

	// Network parameters
	public static double capacityBps = 1000000000.0;
	public static double flowSizeBytes = 50000;
	public static double flowSizeMinBytes = 10;
	public static double flowSizeMaxBytes = 10;
	public static double workerMlaFlowDeadlineSec = 0.019;
	public static double mlaTlaFlowDeadlineSec = 0.015;


	// Processing distributions type
	public static DistType workerDistType = DistType.LogNormal;
	public static DistType mlaDistType = DistType.LogNormal;
	// Worker
	public static double workerComputationTimeMeanSec = 0.100; // TODO: Fix!! Assume 10 threads at Workers
	public static double workerComputationTimeStdevSec = 0.01;
	public static double workerComputationTimeSigmaSecPerRequest = 0.01;
	public static double workerComputationTimeMaxPerRequest = 5;
	public static double workerLogNormalTimeMean = 2.94;
	public static double workerLogNormalTimeSigma = 0.55;
	// MLA
	public static double mlaComputationTimeMeanSec = 0.100; // TODO: Fix!! Assume 20 threads at MLA
	public static double mlaComputationTimeStdevSec = 0.01;
	public static double mlaComputationTimeSigmaSecPerRequest = 0.02;
	public static double mlaComputationTimeMaxPerRequest = 5;
	public static double mlaLogNormalTimeMean = 2.94;
	public static double mlaLogNormalTimeSigma = 0.55;

	// Wait times
	// Wait times before sending partial responses; Set to very high value if 
	// wait for all responses
	public static double mlaWaitTimeSec = .06;
	public static double tlaWaitTimeSec = .90;


	// Misc
	public static double startCountingUtilization = 8.0;
	public static double endCountingUtilization = 10.0;
	public static Randomness rnd;
	public static Dictionary<Tuple<int, int>, int> computeToWaitTime;

	public static void init (int seed)
	{
		currentTimeSec = 0.0001;
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
        
	public static double GetWaitTime (double w, double m)
	{
		return GlobalVariables.tlaWaitTimeSec / 2;
		/*
            int wInt = (int) (w * 1000.0);
            int mInt = (int) (m * 1000.0);
            int wTR = (int) (Math.Round(wInt / 5.0) * 5);
            int mTR = (int) (Math.Round(mInt / 10.0) * 10);
            if (wTR <= 30) wTR = 30;
            if (wTR >= 70) wTR = 70;
            if (mTR <= 60) mTR = 60;
            if (mTR >= 140) mTR = 140;
            Tuple<int, int> t = new Tuple<int, int>(wTR, mTR);
            if (!computeToWaitTime.ContainsKey(t))
            {
                Console.Out.WriteLine("Key Problem" +
                    t.Item1 + " " + t.Item2);
            }
            return 0.101 * 0.001 * computeToWaitTime[t];
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

	public static double GetFacebookSample (int jobId)
	{
		jobId = jobId % taskPerJob.Keys.Count;
		//jobId += 400;
		int i = facebookRandom.Next (taskPerJob [jobId].Count);
		return 0.001 * taskPerJob [jobId] [i];
	}

	public static double facebookLogNormalMean = 4.4;
	public static double facebookLogNormalSigma = 1.15;
	public static double optimalWaitTime = 0;

	public static double FacebookCdf (double x)
	{
		return GetStandardNormalCdf ((Math.Log (x) - 4.4) / 1.15);
	}

	public static double GoogleCdf (double x)
	{
		return GetStandardNormalCdf ((Math.Log (x) - 2.94) / 0.55);
	}

	public static double GetLogNormalCdf (double x, double mean, double sigma)
	{
		return GetStandardNormalCdf ((Math.Log (x) - mean) / sigma);
	}

	public static double GetNormalCdf (double x, double mean, double sigma)
	{
		return GetStandardNormalCdf ((x - mean) / sigma);
	}

	public static double GetStandardNormalCdf (double x)
	{
		double a1 = 0.254829592;
		double a2 = -0.284496736;
		double a3 = 1.421413741;
		double a4 = -1.453152027;
		double a5 = 1.061405429;
		double p = 0.3275911;

		// Save the sign of x
		double sign = 1;
		if (x < 0)
			sign = -1;
		x = Math.Abs (x) / Math.Sqrt (2.0);

		// A&S formula 7.1.26
		double t = 1.0 / (1.0 + p * x);
		double y = 1.0 - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Math.Exp (-x * x);

		return 0.5 * (1.0 + sign * y);
	}

	public static double GetOptimalWaitTimeNormal (
            double mean1, double sigma1, double mean2, double sigma2)
	{
		double deadline = GlobalVariables.tlaWaitTimeSec;
		double k = NumWorkerPerMla;
		double time = 0.0001;
		double utility = 0.0;
		double maxUtility = 0;
		double maxTime = 0;
		double increment = 0.001;
		while (time < deadline) {
			double gain = (GlobalVariables.GetNormalCdf (time + increment,
                    mean1, sigma1)
				- GlobalVariables.GetNormalCdf (time, mean1, sigma1))
				* GlobalVariables.GetNormalCdf (deadline - time - increment, mean2, sigma2);
			double loss = (GlobalVariables.GetNormalCdf (time, mean1, sigma1)
				- Math.Pow (GlobalVariables.GetNormalCdf (time, mean1, sigma1), k))
				* (GlobalVariables.GetNormalCdf (deadline - time, mean2, sigma2) - GlobalVariables.GetNormalCdf (deadline - time - increment, mean2, sigma2));
			utility += gain - loss;
			if (utility > maxUtility) {
				maxUtility = utility;
				maxTime = time;
			}
			time += increment;
		}
		//Console.Error.WriteLine("MaxUtility: " + maxUtility);
		//Console.Error.WriteLine("MaxTime: " + maxTime);
		return maxTime;
	}

	public static double GetOptimalWaitTimeFacebookGoogle (
            double mean, double sigma)
	{
		double deadline = GlobalVariables.tlaWaitTimeSec * 1000.0;
		double k = NumWorkerPerMla;
		double time = 1.0;
		double utility = 0.0;
		double maxUtility = 0;
		double maxTime = 0;
		double increment = 1.0;
		while (time < deadline) {
			double gain = (GlobalVariables.GetLogNormalCdf (time + increment, 
                    mean, sigma)
				- GlobalVariables.GetLogNormalCdf (time, mean, sigma))
				* GlobalVariables.GoogleCdf (deadline - time - increment);
			double loss = (GlobalVariables.GetLogNormalCdf (time, mean, sigma)
				- Math.Pow (GlobalVariables.GetLogNormalCdf (time, mean, sigma), k))
				* (GlobalVariables.GoogleCdf (deadline - time) - GlobalVariables.GoogleCdf (deadline - time - increment));
			utility += gain - loss;
			if (utility > maxUtility) {
				maxUtility = utility;
				maxTime = time;
			}
			time += increment;
		}
		//Console.Error.WriteLine("MaxUtility: " + maxUtility);
		//Console.Error.WriteLine("MaxTime: " + maxTime);
		return maxTime;
	}

	public static double GetOptimalWaitTimeLogNormal (
            double mean1, double sigma1,
            double mean2, double sigma2)
	{
		double deadline = GlobalVariables.tlaWaitTimeSec * 1000.0;
		double k = NumWorkerPerMla;
		double time = 0.001;
		double utility = 0.0;
		double maxUtility = 0;
		double maxTime = 0;
		double increment = 1;
		while (time < deadline) {
			double gain = (GlobalVariables.GetLogNormalCdf (time + increment,
                    mean1, sigma1)
				- GlobalVariables.GetLogNormalCdf (time, mean1, sigma1))
				* GlobalVariables.GetLogNormalCdf (deadline - time - increment, mean2, sigma2);
			double loss = (GlobalVariables.GetLogNormalCdf (time, mean1, sigma1)
				- Math.Pow (GlobalVariables.GetLogNormalCdf (time, mean1, sigma1), k))
				* (GlobalVariables.GetLogNormalCdf (deadline - time, mean2, sigma2) - GlobalVariables.GetLogNormalCdf (deadline - time - increment, mean2, sigma2));
			utility += gain - loss;
			if (utility > maxUtility) {
				maxUtility = utility;
				maxTime = time;
			}
			time += increment;
		}
		//Console.Error.WriteLine("MaxUtility: " + maxUtility);
		//Console.Error.WriteLine("MaxTime: " + maxTime);
		return maxTime;
	}

	public static double GetExponentialCdf (double t, double mean)
	{
		return 1 - Math.Exp (-1.0 * t / mean);
	}

	public static double GetOptimalWaitTimeExponential (
            double mean1, double mean2)
	{
		double deadline = GlobalVariables.tlaWaitTimeSec;
		double k = NumWorkerPerMla;
		double time = 0.0001;
		double utility = 0.0;
		double maxUtility = 0;
		double maxTime = 0;
		double increment = 0.001;
		while (time < deadline) {
			double gain = (GlobalVariables.GetExponentialCdf (time + increment,
                    mean1)
				- GlobalVariables.GetExponentialCdf (time, mean1))
				* GlobalVariables.GetExponentialCdf (deadline - time - increment, mean2);
			double loss = (GlobalVariables.GetExponentialCdf (time, mean1)
				- Math.Pow (GlobalVariables.GetExponentialCdf (time, mean1), k))
				* (GlobalVariables.GetExponentialCdf (deadline - time, mean2) - GlobalVariables.GetExponentialCdf (deadline - time - increment, mean2));
			utility += gain - loss;
			if (utility > maxUtility) {
				maxUtility = utility;
				maxTime = time;
			}
			time += increment;
		}
		//Console.Error.WriteLine("MaxUtility: " + maxUtility);
		//Console.Error.WriteLine("MaxTime: " + maxTime);
		return maxTime;
	}

	public static Dictionary<int, double> orderStats;

	public static void ReadLogNormalOrderStats ()
	{
		orderStats = new Dictionary<int, double> ();
		string[] lines = System.IO.File.ReadAllLines (@"OrderStatisticsLogNormal" + NumWorkerPerMla + ".txt");
		//Console.Error.WriteLine(lines.Length);
		int i = 1;
		foreach (string l in lines) {
			//Console.Error.WriteLine(l);
			orderStats [i++] = double.Parse (l);
		}
		Console.Error.WriteLine ("Length of Order Stats Read: " + orderStats.Count);
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