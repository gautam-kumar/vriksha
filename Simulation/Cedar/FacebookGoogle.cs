using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

class FacebookGoogle
{
	public static void FacebookGoogleExperiment (string[] args)
	{
		int[] T = new int[1] {50};
		foreach (int t in T) {
			int startDeadline = 260;
			int numDeadlines = 1;
			int increment = 5;//0.195, 0.19, 0.20, 0.21, 0.22};
			for (int i = 0; i < numDeadlines; i++) {
				int d = startDeadline + i * increment;
				GlobalVariables.tlaWaitTime = new TimeSpan(0, 0, 0, 0, d);
				Console.Error.WriteLine ("Deadline is : " + GlobalVariables.tlaWaitTime);
				SetupParameters(t);
				Simulate();
				LogResults();
			}
		}
	}

	public static void SetupParameters(int fanout)
	{
		/* Set up Global parameters */
		GlobalVariables.Fanouts = new int[4] {1, 25, 50};
		GlobalVariables.NumWorkers = 50;

		Console.Error.WriteLine ("Topology: " + fanout);
		GlobalVariables.ReadDcTcpNumbers ();
		GlobalVariables.workerDistType = DistType.LogNormal;
		GlobalVariables.workerLogNormalTimeMean = 4.4;
		GlobalVariables.workerLogNormalTimeSigma = 1.15;
		GlobalVariables.mlaDistType = DistType.LogNormal;
		GlobalVariables.mlaLogNormalTimeMean = 2.94;
		GlobalVariables.mlaLogNormalTimeSigma = 0.55;

		
		//GlobalVariables.timeIncrementSec = 0.001;
		GlobalVariables.NumRequestsToSimulate = 200;

		TimeSpan waitTime1 = Algorithms.GetOptimalWaitTime4(GlobalVariables.tlaWaitTime, 
		                                                    GlobalVariables.Fanouts[3], GlobalVariables.Fanouts[2], GlobalVariables.Fanouts[1],
		                                                    DistType.LogNormal, GlobalVariables.workerLogNormalTimeMean, GlobalVariables.workerLogNormalTimeSigma,
		                                                    DistType.LogNormal, GlobalVariables.mlaLogNormalTimeMean, GlobalVariables.mlaLogNormalTimeSigma,
		                                                    DistType.LogNormal, GlobalVariables.mlaLogNormalTimeMean, GlobalVariables.mlaLogNormalTimeSigma,
		                                                    DistType.LogNormal, GlobalVariables.mlaLogNormalTimeMean, GlobalVariables.mlaLogNormalTimeSigma);
		Console.Error.WriteLine ("MLA OPT4 is: " + waitTime1);
		TimeSpan waitTime2 = waitTime1 + Algorithms.GetOptimalWaitTime3(GlobalVariables.tlaWaitTime - waitTime1, 
		                                                    GlobalVariables.Fanouts[2], GlobalVariables.Fanouts[1],
		                                                    DistType.LogNormal, GlobalVariables.mlaLogNormalTimeMean, GlobalVariables.mlaLogNormalTimeSigma,
		                                                    DistType.LogNormal, GlobalVariables.mlaLogNormalTimeMean, GlobalVariables.mlaLogNormalTimeSigma,
		                                                    DistType.LogNormal, GlobalVariables.mlaLogNormalTimeMean, GlobalVariables.mlaLogNormalTimeSigma);
		Console.Error.WriteLine ("MLA OPT3 is: " + waitTime2);

		TimeSpan waitTime3 = waitTime2 + Algorithms.GetOptimalWaitTime2(GlobalVariables.tlaWaitTime - waitTime2,
		                                                                            GlobalVariables.Fanouts[1],
		                                                       DistType.LogNormal, GlobalVariables.mlaLogNormalTimeMean, GlobalVariables.mlaLogNormalTimeSigma,
		                                                       DistType.LogNormal, GlobalVariables.mlaLogNormalTimeMean, GlobalVariables.mlaLogNormalTimeSigma);
		Console.Error.WriteLine ("MLA OPT2 is: " + waitTime3);

		GlobalVariables.mlaWaitTimes = new TimeSpan[GlobalVariables.Fanouts.Length - 1];
		GlobalVariables.mlaWaitTimes[2] = 
			//new TimeSpan(0, 0, 0, 0, (int) (158 * GlobalVariables.tlaWaitTime.TotalMilliseconds / 220));
			new TimeSpan(0, 0, 0, 0, 140);
			//waitTime1;
		GlobalVariables.mlaWaitTimes[1] = 
			//new TimeSpan(0, 0, 0, 0, (int) (179 * GlobalVariables.tlaWaitTime.TotalMilliseconds / 220));
			new TimeSpan(0, 0, 0, 0, 180);
			//waitTime2;
		GlobalVariables.mlaWaitTimes [0] = 
			//new TimeSpan(0, 0, 0, 0, (int) (200 * GlobalVariables.tlaWaitTime.TotalMilliseconds / 220));
			new TimeSpan(0, 0, 0, 0, 220);
			//waitTime3;


		GlobalVariables.init (2000);




		// Initialize System Components
		GlobalVariables.hla = new HLA ();
		int currentLevelNodes = 1;
		GlobalVariables.mlas = new List<MLA>[GlobalVariables.Fanouts.Length - 1];
		for (int i = 0; i < GlobalVariables.Fanouts.Length - 1; i++) {
			currentLevelNodes *= GlobalVariables.Fanouts[i];
			GlobalVariables.mlas[i] = new List<MLA> ();
			for (int j = 0; j < currentLevelNodes; j++) {
				Machine aggregator;
				if (i == 0) {
					aggregator = GlobalVariables.hla;
				} else {
					aggregator = GlobalVariables.mlas[i - 1][j / GlobalVariables.Fanouts[i]];
				}
				GlobalVariables.mlas[i].Add (new MLA (j, i, GlobalVariables.mlaWaitTimes[i],
				                             GlobalVariables.Fanouts[i + 1], 
			                                 aggregator));
			}
		}

		GlobalVariables.workers = new List<Worker> ();
		for (int i = 0; i < GlobalVariables.NumWorkers; i++) {
			int numWorkerPerAggregator = GlobalVariables.Fanouts[GlobalVariables.Fanouts.Length - 1];
			int layer = GlobalVariables.Fanouts.Length - 2;
			GlobalVariables.workers.Add (new Worker (GlobalVariables.NumQueuesInWorker,
			                                         GlobalVariables.mlas[layer][i / numWorkerPerAggregator]));
		}


		// Initialize the stage tasks
		GlobalVariables.mlaTasks = new Dictionary<int, List<Task>>[GlobalVariables.Fanouts.Length - 1];
		for (int i = 0; i < GlobalVariables.mlaTasks.Length; i++) {
			GlobalVariables.mlaTasks [i] = new Dictionary<int, List<Task>> ();
		}


		// Insert the requests
		int requestNumber = 0;
		while (requestNumber < GlobalVariables.NumRequestsToSimulate) {
			GlobalVariables.requests.Add (Request.GetNewRequest (requestNumber));
			List<Double> durations = new List<Double> ();
			foreach (Worker w in GlobalVariables.workers) {
				Task t = Task.CreateNewTask (requestNumber, TaskType.WorkerTask);
				w.InsertTask (t);
				durations.Add (t.processingTimeSec.TotalMilliseconds);
			}

			// For each of the layers, initialize the tasks
			int layer = 0;
			foreach (List<MLA> mlas in GlobalVariables.mlas) {
				GlobalVariables.mlaTasks[layer] [requestNumber] = new List<Task> ();
				foreach (MLA m in mlas) {
					GlobalVariables.mlaTasks[layer][requestNumber].Add (Task.CreateNewTask (requestNumber, TaskType.MlaTask));
				}
				layer += 1;
			}

			requestNumber += 1;
		}
	}



	public static void Simulate() {
		while (GlobalVariables.currentTime <= 
		       GlobalVariables.tlaWaitTime + GlobalVariables.timeIncrement.Add (GlobalVariables.timeIncrement)) { 
			//Console.Error.WriteLine(GlobalVariables.currentTime);
			foreach (Worker w in GlobalVariables.workers) {
				w.AdvanceTime (GlobalVariables.timeIncrement);
			}
			for (int i = GlobalVariables.Fanouts.Length - 2; i >= 0; i--) {
				foreach (MLA m in GlobalVariables.mlas[i]) {
					m.AdvanceTime (GlobalVariables.timeIncrement);
				}
			}
			GlobalVariables.hla.AdvanceTime (GlobalVariables.timeIncrement);
			
			GlobalVariables.currentTime += GlobalVariables.timeIncrement;
		}
	}



	public static void LogResults() 
	{
		// Remove the first edge requests
		List<Request> edgeRequests = new List<Request> ();
		List<double> informationContents = new List<double> ();
		List<double> responseTimes = new List<double> ();
		Dictionary<int, double> icMap = new Dictionary<int, double> ();
		foreach (Request r in GlobalVariables.hla.completedRequests) {
			//Console.WriteLine(r.requestId);
			if (r.requestId < GlobalVariables.NumEdgeRequestsToDelete || r.requestId >= (GlobalVariables.NumRequestsToSimulate - GlobalVariables.NumEdgeRequestsToDelete)) {
				edgeRequests.Add (r);
			} else {
				icMap [r.requestId] = r.informationContent;
				informationContents.Add (r.informationContent * 1.0 / GlobalVariables.NumWorkers);
				responseTimes.Add ((r.endSec - r.beginSec).TotalMilliseconds);
			}
		}
		
		foreach (Request r in edgeRequests) {
			GlobalVariables.hla.completedRequests.Remove (r);
		}
		int numToFill = GlobalVariables.NumRequestsToSimulate - GlobalVariables.hla.completedRequests.Count;
		for (int i = 0; i < numToFill; i++) {
			informationContents.Add (0.0);
		}
		
		//Console.Error.Write(GlobalVariables.mlaWaitTimeSec + " Completed Requests: {0}, ", tla.completedRequests.Count);
		informationContents.Sort ();
		double averageIc = informationContents.Sum () / GlobalVariables.NumRequestsToSimulate;
		int index50 = Math.Max(0, ((int)(50 * informationContents.Count / 100.0)) - 1);
		double percentile50 = informationContents [index50];
		
		int index60 = Math.Max(0, ((int)(40 * informationContents.Count / 100.0)) - 1);
		double percentile60 = informationContents [index60];

		int index75 = Math.Max(0, ((int)(25 * informationContents.Count / 100.0)) - 1);
		double percentile75 = informationContents [index75];
		
		int index80 = Math.Max(0, ((int)(20 * informationContents.Count / 100.0)) - 1);
		double percentile80 = informationContents [index80];

		int index90 = Math.Max(0, ((int)(10 * informationContents.Count / 100.0)) - 1);
		double percentile90 = informationContents [index90];
		
		int index95 = Math.Max(0, ((int)(5 * informationContents.Count / 100.0)) - 1);
		double percentile95 = informationContents [index95];

		int index98 = Math.Max(0, ((int)(2 * informationContents.Count / 100.0)) - 1);
		double percentile98 = informationContents [index98];
		
		int index1 = Math.Max(0, ((int)(1.0 * informationContents.Count / 100.0)) - 1);
		double percentile99 = informationContents [index1];
		
		int index2 = Math.Max(0, ((int)(informationContents.Count / 1000.0)) - 1);
		double percentile99_9 = informationContents [index2];
		
		// #Requests with IC > 0.9, 0.95 and 0.99
		int num80 = 0, num90 = 0, num95 = 0, num99 = 0;
		for (int i = 0; i < informationContents.Count; i++) {
			if (informationContents [i] >= 0.8) {
				if (num80 == 0) {
					num80 = informationContents.Count - i;
				}
			}
			
			if (informationContents [i] >= 0.9) {
				if (num90 == 0) {
					num90 = informationContents.Count - i;
				}
			}
			if (informationContents [i] >= 0.95) {
				if (num95 == 0) {
					num95 = informationContents.Count - i;
				}
			}
			if (informationContents [i] >= 0.99) {
				if (num99 == 0) {
					num99 = informationContents.Count - i;
				}
			}
		}
		
		Console.Error.WriteLine (GlobalVariables.tlaWaitTime.TotalMilliseconds + " " +
		                         GlobalVariables.mlaWaitTimes[0].TotalMilliseconds +
		                         " ICMean " + averageIc +
		                         " IC50 " + percentile50 +
		                         " IC60 " + percentile60 +
		                         " IC75 " + percentile75 +
		                         " IC80 " + percentile80 +
		                         " IC90 " + percentile90 +
		                         " IC95 " + percentile95 +
		                         " IC98 " + percentile98 +
		                         " IC99 " + percentile99 +
		                         " IC99.9 " + percentile99_9 +
		                         " Num80 " + num80 +
		                         " Num90 " + num90 +
		                         " Num95 " + num95 +
		                         " Num99 " + num99 +
		                         " NumCompleted " + informationContents.Count);
	}
}