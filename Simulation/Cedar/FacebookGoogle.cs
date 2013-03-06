﻿using System;
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
			int [] deadline = new int[6] {140, 145, 150, 155, 160, 165}; //0.195, 0.19, 0.20, 0.21, 0.22};
			foreach (int d in deadline) {
				
				GlobalVariables.tlaWaitTimeSec = new TimeSpan(0, 0, 0, 0, d + 30);
				Console.Error.WriteLine ("Deadline is : " + GlobalVariables.tlaWaitTimeSec);
				SetupParameters(t); 
				Simulate();
				LogResults();
			}
		}
	}

	public static void SetupParameters(int fanout)
	{
		/* Set up Global parameters */
		GlobalVariables.NumWorkerPerMla = 50;
		GlobalVariables.NumMlaPerSmla = 10;
		GlobalVariables.NumSmlaPerTla = 5;
		GlobalVariables.NumMlas = 50;
		GlobalVariables.NumWorkers = 2500;

		Console.Error.WriteLine ("Topology: " + fanout);
		GlobalVariables.ReadDcTcpNumbers ();
		GlobalVariables.workerDistType = DistType.LogNormal;
		GlobalVariables.workerLogNormalTimeMean = 4.4;
		GlobalVariables.workerLogNormalTimeSigma = 1.15;
		GlobalVariables.mlaDistType = DistType.LogNormal;
		GlobalVariables.mlaLogNormalTimeMean = 2.94;
		GlobalVariables.mlaLogNormalTimeSigma = 0.55;

		
		//GlobalVariables.timeIncrementSec = 0.001;
		GlobalVariables.NumRequestsToSimulate = 100;
		
		double mu = GlobalVariables.workerLogNormalTimeMean;
		double g = GlobalVariables.workerLogNormalTimeSigma;
		//double a = Math.Exp (mu + g * g / 2);
		mu = GlobalVariables.mlaLogNormalTimeMean;
		g = GlobalVariables.mlaLogNormalTimeSigma;
		//double b = Math.Exp (mu + g * g / 2);
		//double tmp = 0;
		//tmp = 0.001 * GlobalVariables.GetOptimalWaitTimeLogNormal (GlobalVariables.workerLogNormalTimeMean, GlobalVariables.workerLogNormalTimeSigma,
		//                                                           GlobalVariables.mlaLogNormalTimeMean, GlobalVariables.mlaLogNormalTimeSigma);
		//tmp = a / (a + 2 * b) * GlobalVariables.tlaWaitTimeSec;


		// TODO: Fix -- Mla Wait Time is OPT3, Smal is OPT but subtract time apportioned to MLA
		TimeSpan mlaWaitTime = Algorithms.GetOptimalWaitTime3(GlobalVariables.tlaWaitTimeSec,
		                                                       DistType.LogNormal, GlobalVariables.workerLogNormalTimeMean, GlobalVariables.workerLogNormalTimeSigma,
		                                                       DistType.LogNormal, GlobalVariables.mlaLogNormalTimeMean, GlobalVariables.mlaLogNormalTimeSigma,
		                                                       DistType.LogNormal, GlobalVariables.mlaLogNormalTimeMean, GlobalVariables.mlaLogNormalTimeSigma);
		TimeSpan smlaWaitTime = Algorithms.GetOptimalWaitTime2(GlobalVariables.tlaWaitTimeSec - mlaWaitTime, 
		                                                       DistType.LogNormal, GlobalVariables.mlaWaitTimeSec, GlobalVariables.mlaWaitTimeSec,
		                                                      DistType.LogNormal, GlobalVariables.mlaLogNormalTimeMean, GlobalVariables.mlaLogNormalTimeSigma);

		GlobalVariables.mlaWaitTimeSec = new TimeSpan(0, 0, 0, 0, (int) (158 * GlobalVariables.tlaWaitTimeSec.TotalMilliseconds / 200));
			//(a) / (a + 2 * b) * GlobalVariables.tlaWaitTimeSec;;
		GlobalVariables.smlaWaitTimeSec = new TimeSpan(0, 0, 0, 0, (int) (179 * GlobalVariables.tlaWaitTimeSec.TotalMilliseconds / 200));
			//(a + b) / (a + 2 * b) * GlobalVariables.tlaWaitTimeSec;;
		
		GlobalVariables.init (2000);
		// Initialize System Components
		GlobalVariables.hla = new HLA ();
		GlobalVariables.smlas = new List<SMLA> ();
		for (int i = 0; i < GlobalVariables.NumSmlaPerTla; i++) {
			GlobalVariables.smlas.Add (new SMLA (i, GlobalVariables.NumQueuesInMla,
			                                   GlobalVariables.hla));
		}

		GlobalVariables.mlas = new List<MLA> ();
		for (int i = 0; i < GlobalVariables.NumMlas; i++) {
			GlobalVariables.mlas.Add (new MLA (i, GlobalVariables.NumQueuesInMla,
			                                  GlobalVariables.smlas[i / GlobalVariables.NumMlaPerSmla]
			                                   ));
		}
		GlobalVariables.workers = new List<Worker> ();
		for (int i = 0; i < GlobalVariables.NumWorkers; i++) {
			GlobalVariables.workers.Add (new Worker (GlobalVariables.NumQueuesInWorker,
			                                         GlobalVariables.mlas [i / GlobalVariables.NumWorkerPerMla]));
		}
		
		int requestNumber = 0;
		while (requestNumber < GlobalVariables.NumRequestsToSimulate) {
			GlobalVariables.requests.Add (Request.GetNewRequest (requestNumber));
			List<Double> durations = new List<Double> ();
			foreach (Worker w in GlobalVariables.workers) {
				Task t = Task.CreateNewTask (requestNumber, TaskType.WorkerTask);
				w.InsertTask (t);
				durations.Add (t.processingTimeSec.TotalMilliseconds);
			}
			
			GlobalVariables.secondStageTasks [requestNumber] = new List<Task> ();
			//TODO: ReEnable for Ideal scheme computation: 
			//GlobalVariables.optimalWaitTimes [requestNumber] = new List<double> ();
			int index = 0;
			foreach (MLA m in GlobalVariables.mlas) {
				GlobalVariables.secondStageTasks [requestNumber].Add (Task.CreateNewTask (requestNumber, TaskType.MlaTask));
				//TODO: ReEnable for Ideal scheme computation:
				//GlobalVariables.optimalWaitTimes [requestNumber].Add (OptimalMLA.OptimalWaitTimeLogNormal (durations.GetRange (index, GlobalVariables.NumWorkerPerMla)));
				index += GlobalVariables.NumWorkerPerMla;
			}

			GlobalVariables.thirdStageTasks [requestNumber] = new List<Task> ();
			foreach (SMLA s in GlobalVariables.smlas) {
				GlobalVariables.thirdStageTasks [requestNumber].Add (Task.CreateNewTask (requestNumber, TaskType.MlaTask));
			}
			requestNumber += 1;
		}
	}



	public static void Simulate() {
		while (GlobalVariables.currentTime <= 
		       GlobalVariables.tlaWaitTimeSec + GlobalVariables.timeIncrement.Add (GlobalVariables.timeIncrement)) { 
			//Console.Error.WriteLine(GlobalVariables.currentTime);
			foreach (Worker w in GlobalVariables.workers) {
				w.AdvanceTime (GlobalVariables.timeIncrement);
			}
			foreach (MLA m in GlobalVariables.mlas) {
				m.AdvanceTime (GlobalVariables.timeIncrement);
			}
			foreach (SMLA s in GlobalVariables.smlas) {
				s.AdvanceTime (GlobalVariables.timeIncrement);
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
		
		Console.Error.WriteLine (GlobalVariables.tlaWaitTimeSec + " " +
		                         GlobalVariables.mlaWaitTimeSec +
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