using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/* Abstract class Machine */
public abstract class Machine
{
	public List<Task> tasksToSchedule;

	public Machine ()
	{
		tasksToSchedule = new List<Task> ();
	}

	public void InsertTask (Task t)
	{
		t.progressSec = new TimeSpan(0);
		tasksToSchedule.Add (t);
	}

	public abstract void AdvanceTime (TimeSpan timeToSimulate);

	public abstract void InsertFlow (Flow f);

}

public class Worker : Machine
{
	public Machine mla;

	public Worker (int numQueues, Machine mla) : base()
	{
		// Denotes where to insert a flow to
		this.mla = mla;
	}

	public override void InsertFlow (Flow f)
	{
	}

	public override void AdvanceTime (TimeSpan timeToSimulate)
	{
		List<Task> finished = new List<Task> ();
		foreach (Task t in tasksToSchedule) {
			t.progressSec += timeToSimulate;
			if (t.progressSec >= t.processingTimeSec) {
				mla.InsertFlow (Flow.CreateNewFlow (t, 1.0));

				finished.Add (t);
			}
		}
		foreach (Task toRemove in finished) {
			tasksToSchedule.Remove (toRemove);
		}
	}

}

public class MLA : Machine
{
	public int id;
	public Machine hla;
	public Dictionary<int, int> numFlowsPerRequest;
	public Dictionary<int, TimeSpan> firstFlowArrivalTimePerRequest;
	public Dictionary<int, bool> isRequestProcessed;
	public Dictionary<int, double> informationContentPerRequest;
	public Dictionary<int, double> meanEstimate;
	public Dictionary<int, double> sigmaEstimate;
	public Dictionary<int, TimeSpan> waitTime;
	public Dictionary<int, TimeSpan> prevFlowArrivalTime;

	public MLA (int id, int numQueues, Machine tla) : base()
	{
		this.id = id;
		this.hla = tla;
		numFlowsPerRequest = new Dictionary<int, int> ();
		firstFlowArrivalTimePerRequest = new Dictionary<int, TimeSpan> ();
		isRequestProcessed = new Dictionary<int, bool> ();
		informationContentPerRequest = new Dictionary<int, double> ();
		meanEstimate = new Dictionary<int, double> ();
		sigmaEstimate = new Dictionary<int, double> ();
		waitTime = new Dictionary<int, TimeSpan> ();
		prevFlowArrivalTime = new Dictionary<int,TimeSpan> ();
	}

	public override void InsertFlow (Flow f)
	{
		//Console.WriteLine(GlobalVariables.currentTime + ": Adding f");
		int requestId = f.task.requestId;
		// If this is a flow coming in late, ignore
		if (isRequestProcessed.ContainsKey (requestId)) {
			return;
		}
		if (numFlowsPerRequest.ContainsKey (requestId)) {
			numFlowsPerRequest [requestId] += 1;
			informationContentPerRequest [requestId] += f.informationContent;
                
			//Console.Error.WriteLine("{0} New Flow: {1} {2}", requestId, numFlowsPerRequest[requestId], GlobalVariables.currentTimeSec);
		} else {   // first flow
			numFlowsPerRequest.Add (requestId, 1); 
			firstFlowArrivalTimePerRequest.Add (requestId, GlobalVariables.currentTime);
			informationContentPerRequest.Add (requestId, f.informationContent);
			meanEstimate.Add (requestId, GlobalVariables.facebookLogNormalMean);
			sigmaEstimate.Add (requestId, GlobalVariables.facebookLogNormalSigma);
			waitTime.Add (requestId, GlobalVariables.mlaWaitTimeSec);
			prevFlowArrivalTime.Add (requestId, GlobalVariables.currentTime);
		}
	}

	public override void AdvanceTime (TimeSpan timeToSimulate)
	{
		List<int> toRemove = new List<int> ();
		// First check if some new task needs to be added
		foreach (KeyValuePair<int, TimeSpan> req in firstFlowArrivalTimePerRequest) { 
			// If time expired, send partial results, or if all flows received
			TimeSpan beginTime = GlobalVariables.requests[req.Key].beginSec;
			TimeSpan w = GlobalVariables.mlaWaitTimeSec;
			//Console.Error.WriteLine(GlobalVariables.currentTime + ": " + (beginTime + w));
			if ((GlobalVariables.currentTime >= beginTime + w)
				|| (numFlowsPerRequest [req.Key] == GlobalVariables.NumWorkerPerMla)) {
				//GlobalVariables.
				//Console.Error.WriteLine("Timed out: " + w);
				//Console.Error.WriteLine(GlobalVariables.currentTime + ": MLA adding task");
				GlobalVariables.requests [req.Key].numFlowsCompleted += numFlowsPerRequest [req.Key];
				InsertTask (GlobalVariables.secondStageTasks [req.Key] [id]);
				toRemove.Add (req.Key);
				isRequestProcessed [req.Key] = true;
			}
		}
		foreach (int i in toRemove) {
			numFlowsPerRequest.Remove (i);
			firstFlowArrivalTimePerRequest.Remove (i);
		}

		List<Task> finished = new List<Task> ();
		foreach (Task t in tasksToSchedule) {
			t.progressSec += timeToSimulate;
			//Console.Error.WriteLine(GlobalVariables.currentTime + ": Progress " + t.progressSec);
			if (t.progressSec >= t.processingTimeSec) {
				hla.InsertFlow (Flow.CreateNewFlow (t, 
                        informationContentPerRequest [t.requestId]));
				finished.Add (t);
			}
		}
		foreach (Task t in finished) {
			tasksToSchedule.Remove (t);
		}
	}

}



public class SMLA : Machine
{
	public int id;
	public Machine hla;
	public Dictionary<int, int> numFlowsPerRequest;
	public Dictionary<int, bool> isRequestProcessed;
	public Dictionary<int, double> informationContentPerRequest;
	public Dictionary<int, double> meanEstimate;
	public Dictionary<int, double> sigmaEstimate;
	public Dictionary<int, TimeSpan> waitTime;
	public Dictionary<int, TimeSpan> firstFlowArrivalTimePerRequest;
	public Dictionary<int, TimeSpan> prevFlowArrivalTime;
	
	public SMLA (int id, int numQueues, Machine tla) : base()
	{
		this.id = id;
		this.hla = tla;
		numFlowsPerRequest = new Dictionary<int, int> ();
		firstFlowArrivalTimePerRequest = new Dictionary<int, TimeSpan> ();
		isRequestProcessed = new Dictionary<int, bool> ();
		informationContentPerRequest = new Dictionary<int, double> ();
		meanEstimate = new Dictionary<int, double> ();
		sigmaEstimate = new Dictionary<int, double> ();
		waitTime = new Dictionary<int, TimeSpan> ();
		prevFlowArrivalTime = new Dictionary<int,TimeSpan> ();
	}
	
	public override void InsertFlow (Flow f)
	{
		
		//Console.WriteLine("Adding f");
		int requestId = f.task.requestId;
		// If this is a flow coming in late, ignore
		if (isRequestProcessed.ContainsKey (requestId)) {
			return;
		}

		if (numFlowsPerRequest.ContainsKey (requestId)) {
			numFlowsPerRequest [requestId] += 1;
			informationContentPerRequest [requestId] += f.informationContent;
			
			//Console.Error.WriteLine("{0} New Flow: {1} {2}", requestId, numFlowsPerRequest[requestId], GlobalVariables.currentTimeSec);
		} else {   // first flow
			numFlowsPerRequest.Add (requestId, 1); 

			firstFlowArrivalTimePerRequest.Add (requestId, GlobalVariables.currentTime);
			informationContentPerRequest.Add (requestId, f.informationContent);
			meanEstimate.Add (requestId, GlobalVariables.facebookLogNormalMean);
			sigmaEstimate.Add (requestId, GlobalVariables.facebookLogNormalSigma);
			waitTime.Add (requestId, GlobalVariables.mlaWaitTimeSec);
			prevFlowArrivalTime.Add (requestId, GlobalVariables.currentTime);
		}
	}
	
	public override void AdvanceTime (TimeSpan timeToSimulate)
	{
		List<int> toRemove = new List<int> ();
		// First check if some new task needs to be added
		foreach (KeyValuePair<int, TimeSpan> req in firstFlowArrivalTimePerRequest) { 
			// If time expired, send partial results, or if all flows received
			TimeSpan beginTime = GlobalVariables.requests [req.Key].beginSec;
		
			TimeSpan w = GlobalVariables.smlaWaitTimeSec;
			if ((GlobalVariables.currentTime >= beginTime + w)
			    || (numFlowsPerRequest [req.Key] == GlobalVariables.NumMlaPerSmla)) {

				//GlobalVariables.
				//Console.Error.WriteLine("Timed out: " + w);
				GlobalVariables.requests [req.Key].numFlowsCompleted += numFlowsPerRequest [req.Key];
				InsertTask (GlobalVariables.thirdStageTasks [req.Key] [id]);
				toRemove.Add (req.Key);
				isRequestProcessed [req.Key] = true;
			}
		}
		foreach (int i in toRemove) {
			numFlowsPerRequest.Remove (i);
			firstFlowArrivalTimePerRequest.Remove (i);
		}
		
		List<Task> finished = new List<Task> ();
		foreach (Task t in tasksToSchedule) {
			t.progressSec += timeToSimulate;
			if (t.progressSec >= t.processingTimeSec) {
				hla.InsertFlow (Flow.CreateNewFlow (t, 
				                                    informationContentPerRequest [t.requestId]));
				finished.Add (t);
			}
		}
		foreach (Task t in finished) {
			tasksToSchedule.Remove (t);
		}
	}
}


public class HLA : Machine
{
	public Dictionary<int, int> numFlowsPerRequest;
	public List<Request> completedRequests;
	public Dictionary<int, double> firstFlowArrivalTimePerRequest;
	public Dictionary<int, bool> isRequestProcessed;
	public Dictionary<int, double> informationContentPerRequest;
	
	public HLA () : base ()
	{
		tasksToSchedule = new List<Task> ();
		numFlowsPerRequest = new Dictionary<int, int> ();
		completedRequests = new List<Request> ();
		firstFlowArrivalTimePerRequest = new Dictionary<int, double> ();
		isRequestProcessed = new Dictionary<int, bool> ();
		informationContentPerRequest = new Dictionary<int, double> ();
	}
	
	public override void InsertFlow (Flow f)
	{
		
		int requestId = f.task.requestId;
		// Flow coming in late
		
		if (isRequestProcessed.ContainsKey (requestId)) {
			return;
		}
		
		if (numFlowsPerRequest.ContainsKey (requestId)) {
			numFlowsPerRequest [requestId] += 1;
			informationContentPerRequest [requestId] += f.informationContent;
		} else {
			numFlowsPerRequest.Add (requestId, 1);
			firstFlowArrivalTimePerRequest.Add (requestId, GlobalVariables.currentTime.TotalMilliseconds);
			informationContentPerRequest.Add (requestId, f.informationContent);
			//Console.Out.WriteLine("Added " + f.informationContent);
		}
	}
	
	public override void AdvanceTime (TimeSpan timeToSimulate)
	{
		// First check if some new task needs to be added
		List<int> toRemove = new List<int> ();
		foreach (KeyValuePair<int, double> req in firstFlowArrivalTimePerRequest) {
			// If time expired, send partial results, or if all flows received
			TimeSpan beginTime = GlobalVariables.requests [req.Key].beginSec;
			if ((GlobalVariables.currentTime >= beginTime + GlobalVariables.tlaWaitTimeSec)
			    || (numFlowsPerRequest [req.Key] == GlobalVariables.NumSmlaPerTla)) {
				GlobalVariables.requests [req.Key].numFlowsCompleted += numFlowsPerRequest [req.Key];
				if (GlobalVariables.currentTime <= beginTime + GlobalVariables.tlaWaitTimeSec + GlobalVariables.timeIncrement) {
					InsertTask (Task.CreateNewTask (req.Key, TaskType.TlaTask));
				}
				toRemove.Add (req.Key);
				isRequestProcessed [req.Key] = true;
			}
		}
		foreach (int i in toRemove) {
			numFlowsPerRequest.Remove (i);
			firstFlowArrivalTimePerRequest.Remove (i);
		}
		
		
		List<Task> finished = new List<Task> ();
		foreach (Task t in tasksToSchedule) {
			int requestId = t.requestId;
			Request r = GlobalVariables.requests [requestId];
			r.informationContent = informationContentPerRequest [r.requestId];
			r.endSec = GlobalVariables.currentTime;
			t.progressSec += timeToSimulate;
			completedRequests.Add (GlobalVariables.requests [requestId]);
			finished.Add (t);
			
		}
		foreach (Task t in finished) {
			tasksToSchedule.Remove (t);
		}
	}

}

// TODO Commented out for now, need to fix TimeSpan and other issues
/*
public class CedarMLA : MLA
{
	public CedarMLA (int id, int numQueues, Machine tla) : base(id, numQueues, tla)
	{
	}

	public void UpdateMeanAndSigmaEmpirical (int requestId)
	{
		int n = numFlowsPerRequest [requestId];
		double rPrev = Math.Log (prevFlowArrivalTime [requestId] * 1000);
		double ePrev = Math.Log (GlobalVariables.orderStats [n - 1]);
		double r = Math.Log (GlobalVariables.currentTime * 1000);
		double e = Math.Log (GlobalVariables.orderStats [n]);
		double newSigma = (r - rPrev) / (e - ePrev);
		double newMean = r - e * newSigma;
		if ((r == rPrev)) {
			newMean = meanEstimate [requestId];
			newSigma = sigmaEstimate [requestId];
		}
		double prevMean = meanEstimate [requestId];
		double prevSigma = sigmaEstimate [requestId];
		meanEstimate [requestId] = (prevMean * (n - 1) + newMean) / n;
		sigmaEstimate [requestId] = (prevSigma * (n - 1) + newSigma) / n;
		// Set rprev, eprev
		prevFlowArrivalTime [requestId] = GlobalVariables.currentTime;
	}

	public void UpdateMeanAndSigma (int requestId)
	{
		int n = numFlowsPerRequest [requestId];
		double rPrev = Math.Log (prevFlowArrivalTime [requestId] * 1000);
		double ePrev = Math.Log (GlobalVariables.orderStats [n - 1]);
		double r = Math.Log (GlobalVariables.currentTime * 1000);
		double e = Math.Log (GlobalVariables.orderStats [n]);
		double newSigma = (r - rPrev) / (e - ePrev);
		double newMean = r - e * newSigma;
		if ((r == rPrev)) {
			newMean = meanEstimate [requestId];
			newSigma = sigmaEstimate [requestId];
		}
		double prevMean = meanEstimate [requestId];
		double prevSigma = sigmaEstimate [requestId];
		meanEstimate [requestId] = (prevMean * (n - 1) + newMean) / n;
		sigmaEstimate [requestId] = (prevSigma * (n - 1) + newSigma) / n;
		// Set rprev, eprev
		prevFlowArrivalTime [requestId] = GlobalVariables.currentTime;
	}

	public override void InsertFlow (Flow f)
	{
		//Console.WriteLine("Adding f");
		int requestId = f.task.requestId;
		// If this is a flow coming in late, ignore
		if (isRequestProcessed.ContainsKey (requestId)) {
			return;
		}
		if (numFlowsPerRequest.ContainsKey (requestId)) {
			numFlowsPerRequest [requestId] += 1;
			informationContentPerRequest [requestId] += f.informationContent;
			UpdateMeanAndSigma (requestId);
                
			bool shouldUpdateWaitTime = (numFlowsPerRequest [requestId] % (GlobalVariables.NumWorkerPerMla / 5)) == 0;
			if (shouldUpdateWaitTime) {
				waitTime [requestId] = 0.001 * Algorithms.GetOptimalWaitTimeLogNormal (
                        meanEstimate [requestId], sigmaEstimate [requestId],
                        GlobalVariables.mlaLogNormalTimeMean, GlobalVariables.mlaLogNormalTimeSigma);
				GlobalVariables.UpdateGlobalWaitTime (requestId);
			}
			//Console.Error.WriteLine("{0} New Flow: {1} {2}", requestId, numFlowsPerRequest[requestId], GlobalVariables.currentTimeSec);
		} else {   // first flow
			numFlowsPerRequest.Add (requestId, 1);
			firstFlowArrivalTimePerRequest.Add (requestId, GlobalVariables.currentTime);
			informationContentPerRequest.Add (requestId, f.informationContent);
			meanEstimate.Add (requestId, GlobalVariables.facebookLogNormalMean);
			sigmaEstimate.Add (requestId, GlobalVariables.facebookLogNormalSigma);
			waitTime.Add (requestId, GlobalVariables.mlaWaitTimeSec);
			prevFlowArrivalTime.Add (requestId, GlobalVariables.currentTime);
		}
	}

	public override void AdvanceTime (double timeToSimulate)
	{
		List<int> toRemove = new List<int> ();
		// First check if some new task needs to be added
		foreach (KeyValuePair<int, double> req in firstFlowArrivalTimePerRequest) {
			// If time expired, send partial results, or if all flows received
			double beginTime = GlobalVariables.requests [req.Key].beginSec;
			double w = waitTime[req.Key];
			//if (GlobalVariables.waitTimeGlobal.ContainsKey(req.Key))
			//{
			//    w = GlobalVariables.waitTimeGlobal[req.Key];
			//}
			if ((GlobalVariables.currentTime >= beginTime + w)
				|| (numFlowsPerRequest [req.Key] == GlobalVariables.NumWorkerPerMla)) {   
				GlobalVariables.requests [req.Key].numFlowsCompleted += numFlowsPerRequest [req.Key];
				InsertTask (GlobalVariables.secondStageTasks [req.Key] [id]);
				toRemove.Add (req.Key);
				isRequestProcessed [req.Key] = true;
			}
		}
		foreach (int i in toRemove) {
			numFlowsPerRequest.Remove (i);
			firstFlowArrivalTimePerRequest.Remove (i);
		}

		List<Task> finished = new List<Task> ();
		foreach (Task t in tasksToSchedule) {
			t.progressSec += timeToSimulate;
			if (t.progressSec >= t.processingTimeSec) {
				hla.InsertFlow (Flow.CreateNewFlow (t,
                        informationContentPerRequest [t.requestId] / GlobalVariables.NumWorkerPerMla));
				finished.Add (t);
			}
		}
		foreach (Task t in finished) {
			tasksToSchedule.Remove (t);
		}
	}

}

public class VrikshaMLAEmpirical : MLA
{
	Dictionary<int, double> sum1;
	Dictionary<int, double> sum2;

	public VrikshaMLAEmpirical (int id, int numQueues, Machine tla)
            : base(id, numQueues, tla)
	{
		sum1 = new Dictionary<int, double> ();
		sum2 = new Dictionary<int, double> ();
	}

	public void UpdateMeanAndSigma (int requestId)
	{
		int n = numFlowsPerRequest [requestId];
		double r = Math.Log (GlobalVariables.currentTime * 1000);
		sum1 [requestId] += r;
		sum2 [requestId] += r * r;

		meanEstimate [requestId] = sum1 [requestId] / n;
		sigmaEstimate [requestId] = Math.Sqrt ((sum2 [requestId] / n) - (meanEstimate [requestId] * meanEstimate [requestId]));
		// Set rprev, eprev
		prevFlowArrivalTime [requestId] = GlobalVariables.currentTime;
	}

	public override void InsertFlow (Flow f)
	{
		//Console.WriteLine("Adding f");
		int requestId = f.task.requestId;
		// If this is a flow coming in late, ignore
		if (isRequestProcessed.ContainsKey (requestId)) {
			return;
		}
		if (numFlowsPerRequest.ContainsKey (requestId)) {
			numFlowsPerRequest [requestId] += 1;
			informationContentPerRequest [requestId] += f.informationContent;
			UpdateMeanAndSigma (requestId);

			bool shouldUpdateWaitTime = (numFlowsPerRequest [requestId] % (GlobalVariables.NumWorkerPerMla / 5)) == 0;
			if (shouldUpdateWaitTime) {
				waitTime [requestId] = 0.001 * Algorithms.GetOptimalWaitTimeLogNormal (
                        meanEstimate [requestId], sigmaEstimate [requestId],
                        GlobalVariables.mlaLogNormalTimeMean, GlobalVariables.mlaLogNormalTimeSigma);
				GlobalVariables.UpdateGlobalWaitTime (requestId);
			}
			//Console.Error.WriteLine("{0} New Flow: {1} {2}", requestId, numFlowsPerRequest[requestId], GlobalVariables.currentTimeSec);
		} else {   // first flow
			numFlowsPerRequest.Add (requestId, 1);
			double x = Math.Log (1000.0 * GlobalVariables.currentTime);
			sum1 [requestId] = x;
			sum2 [requestId] = x * x;
			firstFlowArrivalTimePerRequest.Add (requestId, GlobalVariables.currentTime);
			informationContentPerRequest.Add (requestId, f.informationContent);
			meanEstimate.Add (requestId, GlobalVariables.facebookLogNormalMean);
			sigmaEstimate.Add (requestId, GlobalVariables.facebookLogNormalSigma);
			waitTime.Add (requestId, GlobalVariables.mlaWaitTimeSec);
			prevFlowArrivalTime.Add (requestId, GlobalVariables.currentTime);
		}
	}

	public override void AdvanceTime (double timeToSimulate)
	{
		List<int> toRemove = new List<int> ();
		// First check if some new task needs to be added
		foreach (KeyValuePair<int, double> req in firstFlowArrivalTimePerRequest) {
			// If time expired, send partial results, or if all flows received
			double beginTime = GlobalVariables.requests [req.Key].beginSec;
			double w = waitTime [req.Key];
			//if (GlobalVariables.waitTimeGlobal.ContainsKey(req.Key))
			//{
			//    w = GlobalVariables.waitTimeGlobal[req.Key];
			//}
			if ((GlobalVariables.currentTime >= beginTime + w)
				|| (numFlowsPerRequest [req.Key] == GlobalVariables.NumWorkerPerMla)) {

				GlobalVariables.requests [req.Key].numFlowsCompleted += numFlowsPerRequest [req.Key];
				InsertTask (GlobalVariables.secondStageTasks [req.Key] [id]);
				toRemove.Add (req.Key);
				isRequestProcessed [req.Key] = true;
			}
		}
		foreach (int i in toRemove) {
			numFlowsPerRequest.Remove (i);
			firstFlowArrivalTimePerRequest.Remove (i);
		}

		List<Task> finished = new List<Task> ();
		foreach (Task t in tasksToSchedule) {
			t.progressSec += timeToSimulate;
			if (t.progressSec >= t.processingTimeSec) {
				hla.InsertFlow (Flow.CreateNewFlow (t,
                        informationContentPerRequest [t.requestId] / GlobalVariables.NumWorkerPerMla));
				finished.Add (t);
			}
		}
		foreach (Task t in finished) {
			tasksToSchedule.Remove (t);
		}
	}

}

public class OptimalMLA : MLA
{

	public OptimalMLA (int id, int numQueues, Machine tla)
            : base(id, numQueues, tla)
	{
	}

	public override void InsertFlow (Flow f)
	{
		//Console.WriteLine("Adding f");
		int requestId = f.task.requestId;
		// If this is a flow coming in late, ignore
		if (isRequestProcessed.ContainsKey (requestId)) {
			return;
		}
		if (numFlowsPerRequest.ContainsKey (requestId)) {
			numFlowsPerRequest [requestId] += 1;
			informationContentPerRequest [requestId] += f.informationContent;

			//Console.Error.WriteLine("{0} New Flow: {1} {2}", requestId, numFlowsPerRequest[requestId], GlobalVariables.currentTimeSec);
		} else {   // first flow
			numFlowsPerRequest.Add (requestId, 1);
			firstFlowArrivalTimePerRequest.Add (requestId, GlobalVariables.currentTime);
			informationContentPerRequest.Add (requestId, f.informationContent);
			meanEstimate.Add (requestId, GlobalVariables.facebookLogNormalMean);
			sigmaEstimate.Add (requestId, GlobalVariables.facebookLogNormalSigma);
			waitTime.Add (requestId, GlobalVariables.mlaWaitTimeSec);
			prevFlowArrivalTime.Add (requestId, GlobalVariables.currentTime);
		}
	}

	public override void AdvanceTime (double timeToSimulate)
	{
		List<int> toRemove = new List<int> ();
		// First check if some new task needs to be added
		foreach (KeyValuePair<int, double> req in firstFlowArrivalTimePerRequest) {
			// If time expired, send partial results, or if all flows received
			double beginTime = GlobalVariables.requests [req.Key].beginSec;
			double w = GlobalVariables.optimalWaitTimes [req.Key] [id];
			//if (GlobalVariables.waitTimeGlobal.ContainsKey(req.Key))
			//{
			//    w = GlobalVariables.waitTimeGlobal[req.Key];
			//}
			if ((GlobalVariables.currentTime >= beginTime + w)
				|| (numFlowsPerRequest [req.Key] == GlobalVariables.NumWorkerPerMla)) {

				GlobalVariables.requests [req.Key].numFlowsCompleted += numFlowsPerRequest [req.Key];
				InsertTask (GlobalVariables.secondStageTasks [req.Key] [id]);
				toRemove.Add (req.Key);
				isRequestProcessed [req.Key] = true;
			}
		}
		foreach (int i in toRemove) {
			numFlowsPerRequest.Remove (i);
			firstFlowArrivalTimePerRequest.Remove (i);
		}

		List<Task> finished = new List<Task> ();
		foreach (Task t in tasksToSchedule) {
			t.progressSec += timeToSimulate;
			if (t.progressSec >= t.processingTimeSec) {
				hla.InsertFlow (Flow.CreateNewFlow (t,
                        informationContentPerRequest [t.requestId] / GlobalVariables.NumWorkerPerMla));
				finished.Add (t);
			}
		}
		foreach (Task t in finished) {
			tasksToSchedule.Remove (t);
		}
	}

	public static double OptimalWaitTimeLogNormal (List<Double> workerDurations)
	{
		workerDurations.Sort ();
		int n = workerDurations.Count;
		double mean = GlobalVariables.workerLogNormalTimeMean;
		double sigma = GlobalVariables.workerLogNormalTimeSigma;
		for (int i = 1; i < n; i++) {
			double rPrev = Math.Log (workerDurations [i - 1] * 1000);
			//Console.Error.WriteLine(i + "   " + GlobalVariables.orderStats[i]);
			double ePrev = Math.Log (GlobalVariables.orderStats [i]);
			double r = Math.Log (workerDurations [i] * 1000);
			double e = Math.Log (GlobalVariables.orderStats [i + 1]);
			double newSigma = (r - rPrev) / (e - ePrev);
			double newMean = r - e * newSigma;
			if (r == rPrev) {
				newMean = mean;
				newSigma = sigma;
			}
			mean = (mean * (n - 1) + newMean) / n;
			sigma = (sigma * (n - 1) + newSigma) / n;
		}
		return 0.001 * Algorithms.GetOptimalWaitTimeLogNormal (mean, sigma, 
                GlobalVariables.mlaLogNormalTimeMean, GlobalVariables.mlaLogNormalTimeSigma);
	}
}

public class OptimalMLAEntireDistribution : MLA
{

	Dictionary<int, double> opti = new Dictionary<int, double> ();

	public OptimalMLAEntireDistribution (int id, int numQueues, Machine tla)
            : base(id, numQueues, tla)
	{
	}

	public override void InsertFlow (Flow f)
	{
		//Console.WriteLine("Adding f");
		int requestId = f.task.requestId;
		// If this is a flow coming in late, ignore
		if (isRequestProcessed.ContainsKey (requestId)) {
			return;
		}
		if (numFlowsPerRequest.ContainsKey (requestId)) {
			numFlowsPerRequest [requestId] += 1;
			informationContentPerRequest [requestId] += f.informationContent;

			//Console.Error.WriteLine("{0} New Flow: {1} {2}", requestId, numFlowsPerRequest[requestId], GlobalVariables.currentTimeSec);
		} else {   // first flow
			opti [requestId] = 0.001 * Algorithms.GetOptimalWaitTimeLogNormal (
                        GlobalVariables.workerLogNormalTimeMean, GlobalVariables.workerLogNormalTimeSigma,
                        GlobalVariables.mlaLogNormalTimeMean, GlobalVariables.mlaLogNormalTimeSigma);
			numFlowsPerRequest.Add (requestId, 1);
			firstFlowArrivalTimePerRequest.Add (requestId, GlobalVariables.currentTime);
			informationContentPerRequest.Add (requestId, f.informationContent);
			meanEstimate.Add (requestId, GlobalVariables.facebookLogNormalMean);
			sigmaEstimate.Add (requestId, GlobalVariables.facebookLogNormalSigma);
			waitTime.Add (requestId, GlobalVariables.mlaWaitTimeSec);
			prevFlowArrivalTime.Add (requestId, GlobalVariables.currentTime);
		}
	}

	public override void AdvanceTime (double timeToSimulate)
	{
		List<int> toRemove = new List<int> ();
		// First check if some new task needs to be added
		foreach (KeyValuePair<int, double> req in firstFlowArrivalTimePerRequest) {
			// If time expired, send partial results, or if all flows received
			double beginTime = GlobalVariables.requests [req.Key].beginSec;
			double w = opti [req.Key];
			//if (GlobalVariables.waitTimeGlobal.ContainsKey(req.Key))
			//{
			//    w = GlobalVariables.waitTimeGlobal[req.Key];
			//}
			if ((GlobalVariables.currentTime >= beginTime + w)
				|| (numFlowsPerRequest [req.Key] == GlobalVariables.NumWorkerPerMla)) {

				GlobalVariables.requests [req.Key].numFlowsCompleted += numFlowsPerRequest [req.Key];
				InsertTask (GlobalVariables.secondStageTasks [req.Key] [id]);
				toRemove.Add (req.Key);
				isRequestProcessed [req.Key] = true;
			}
		}
		foreach (int i in toRemove) {
			numFlowsPerRequest.Remove (i);
			firstFlowArrivalTimePerRequest.Remove (i);
		}

		List<Task> finished = new List<Task> ();
		foreach (Task t in tasksToSchedule) {
			t.progressSec += timeToSimulate;
			if (t.progressSec >= t.processingTimeSec) {
				hla.InsertFlow (Flow.CreateNewFlow (t,
                        informationContentPerRequest [t.requestId] / GlobalVariables.NumWorkerPerMla));
				finished.Add (t);
			}
		}
		foreach (Task t in finished) {
			tasksToSchedule.Remove (t);
		}
	}

}

public class OracleMLA : MLA
{

	public OracleMLA (int id, int numQueues, Machine tla)
            : base(id, numQueues, tla)
	{
	}

	public override void InsertFlow (Flow f)
	{
		//Console.WriteLine("Adding f");
		int requestId = f.task.requestId;
		// If this is a flow coming in late, ignore
		if (isRequestProcessed.ContainsKey (requestId)) {
			return;
		}
		if (numFlowsPerRequest.ContainsKey (requestId)) {
			numFlowsPerRequest [requestId] += 1;
			informationContentPerRequest [requestId] += f.informationContent;

			//Console.Error.WriteLine("{0} New Flow: {1} {2}", requestId, numFlowsPerRequest[requestId], GlobalVariables.currentTimeSec);
		} else {   // first flow
			numFlowsPerRequest.Add (requestId, 1);
			firstFlowArrivalTimePerRequest.Add (requestId, GlobalVariables.currentTime);
			informationContentPerRequest.Add (requestId, f.informationContent);
			meanEstimate.Add (requestId, GlobalVariables.facebookLogNormalMean);
			sigmaEstimate.Add (requestId, GlobalVariables.facebookLogNormalSigma);
			waitTime.Add (requestId, GlobalVariables.mlaWaitTimeSec);
			prevFlowArrivalTime.Add (requestId, GlobalVariables.currentTime);
		}
	}

	public override void AdvanceTime (double timeToSimulate)
	{
		List<int> toRemove = new List<int> ();
		// First check if some new task needs to be added
		foreach (KeyValuePair<int, double> req in firstFlowArrivalTimePerRequest) {
			// If time expired, send partial results, or if all flows received
			double beginTime = GlobalVariables.requests [req.Key].beginSec;
			double w = GlobalVariables.tlaWaitTimeSec - GlobalVariables.secondStageTasks [req.Key] [id].processingTimeSec - GlobalVariables.timeIncrementSec;
			//if (GlobalVariables.waitTimeGlobal.ContainsKey(req.Key))
			//{
			//    w = GlobalVariables.waitTimeGlobal[req.Key];
			//}
			if ((GlobalVariables.currentTime >= beginTime + w)
				|| (numFlowsPerRequest [req.Key] == GlobalVariables.NumWorkerPerMla)) {
				// if (id == 6) 
				//    Console.Error.WriteLine("Wait Time is: " + w + " Mean " + meanEstimate[req.Key] + " Sigma " + sigmaEstimate[req.Key]
				//   + " " + numFlowsPerRequest[req.Key]);
				GlobalVariables.requests [req.Key].numFlowsCompleted += numFlowsPerRequest [req.Key];
				InsertTask (GlobalVariables.secondStageTasks [req.Key] [id]);
				toRemove.Add (req.Key);
				isRequestProcessed [req.Key] = true;
			}
		}
		foreach (int i in toRemove) {
			numFlowsPerRequest.Remove (i);
			firstFlowArrivalTimePerRequest.Remove (i);
		}

		List<Task> finished = new List<Task> ();
		foreach (Task t in tasksToSchedule) {
			t.progressSec += timeToSimulate;
			if (t.progressSec >= t.processingTimeSec) {
				hla.InsertFlow (Flow.CreateNewFlow (t,
                        informationContentPerRequest [t.requestId] / GlobalVariables.NumWorkerPerMla));
				finished.Add (t);
			}
		}
		foreach (Task t in finished) {
			tasksToSchedule.Remove (t);
		}
	}

}


*/
