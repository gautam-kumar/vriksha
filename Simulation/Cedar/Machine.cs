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
		t.progressSec = 0;
		tasksToSchedule.Add (t);
	}

	public abstract void AdvanceTime (double timeToSimulate);

	public abstract void InsertFlow (Flow f);

	public abstract void AddFlowCompletionTime (double fTime); 
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

	public override void AdvanceTime (double timeToSimulate)
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

	public override void AddFlowCompletionTime (double fTime)
	{
	}
}

public class MLA : Machine
{
	public int id;
	public Machine hla;
	public Dictionary<int, int> numFlowsPerRequest;
	public Dictionary<int, double> firstFlowArrivalTimePerRequest;
	public Dictionary<int, bool> isRequestProcessed;
	public Dictionary<int, double> informationContentPerRequest;
	public Dictionary<int, double> meanEstimate;
	public Dictionary<int, double> sigmaEstimate;
	public Dictionary<int, double> waitTime;
	public Dictionary<int, double> prevFlowArrivalTime;

	public MLA (int id, int numQueues, Machine tla) : base()
	{
		this.id = id;
		this.hla = tla;
		numFlowsPerRequest = new Dictionary<int, int> ();
		firstFlowArrivalTimePerRequest = new Dictionary<int, double> ();
		isRequestProcessed = new Dictionary<int, bool> ();
		informationContentPerRequest = new Dictionary<int, double> ();
		meanEstimate = new Dictionary<int, double> ();
		sigmaEstimate = new Dictionary<int, double> ();
		waitTime = new Dictionary<int, double> ();
		prevFlowArrivalTime = new Dictionary<int,double> ();
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
			firstFlowArrivalTimePerRequest.Add (requestId, GlobalVariables.currentTimeSec);
			informationContentPerRequest.Add (requestId, f.informationContent);
			meanEstimate.Add (requestId, GlobalVariables.facebookLogNormalMean);
			sigmaEstimate.Add (requestId, GlobalVariables.facebookLogNormalSigma);
			waitTime.Add (requestId, GlobalVariables.mlaWaitTimeSec);
			prevFlowArrivalTime.Add (requestId, GlobalVariables.currentTimeSec);
		}
	}

	public override void AdvanceTime (double timeToSimulate)
	{
		List<int> toRemove = new List<int> ();
		// First check if some new task needs to be added
		foreach (KeyValuePair<int, double> req in firstFlowArrivalTimePerRequest) { 
			// If time expired, send partial results, or if all flows received
			double beginTime = GlobalVariables.requests [req.Key].beginSec;
			double w = GlobalVariables.mlaWaitTimeSec;
			if ((GlobalVariables.currentTimeSec >= beginTime + w)
				|| (numFlowsPerRequest [req.Key] == GlobalVariables.NumWorkerPerMla)) {
				//GlobalVariables.
				//Console.Error.WriteLine("Timed out: " + w);
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
                        informationContentPerRequest [t.requestId]));
				finished.Add (t);
			}
		}
		foreach (Task t in finished) {
			tasksToSchedule.Remove (t);
		}
	}

	public override void AddFlowCompletionTime (double fTime)
	{
		GlobalVariables.flowCompletionTimesMla.Add (fTime);
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
			//Console.Error.WriteLine("{0} TLA inserted flow {1}", f.requestId, isRequestProcessed.ContainsKey(requestId));
			numFlowsPerRequest.Add (requestId, 1);
			firstFlowArrivalTimePerRequest.Add (requestId, GlobalVariables.currentTimeSec);
			informationContentPerRequest.Add (requestId, f.informationContent);
			//Console.Error.WriteLine("Added flow: " + requestId + " at " 
			//s    + (GlobalVariables.currentTimeSec - GlobalVariables.requests[requestId].beginSec));
			//Console.Out.WriteLine("Added " + f.informationContent);
		}
	}
	
	public override void AdvanceTime (double timeToSimulate)
	{
		// First check if some new task needs to be added
		List<int> toRemove = new List<int> ();
		foreach (KeyValuePair<int, double> req in firstFlowArrivalTimePerRequest) {
			// If time expired, send partial results, or if all flows received
			double beginTime = GlobalVariables.requests [req.Key].beginSec;
			if ((GlobalVariables.currentTimeSec >= beginTime + GlobalVariables.tlaWaitTimeSec)
			    || (numFlowsPerRequest [req.Key] == GlobalVariables.NumMlaPerTla)) {
				GlobalVariables.requests [req.Key].numFlowsCompleted += numFlowsPerRequest [req.Key];
				if (GlobalVariables.currentTimeSec <= beginTime + GlobalVariables.tlaWaitTimeSec + 2 * GlobalVariables.timeIncrementSec) {
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
			r.endSec = GlobalVariables.currentTimeSec;
			t.progressSec += timeToSimulate;
			completedRequests.Add (GlobalVariables.requests [requestId]);
			finished.Add (t);
			
		}
		foreach (Task t in finished) {
			tasksToSchedule.Remove (t);
		}
	}
	
	public override void AddFlowCompletionTime (double fTime)
	{
		GlobalVariables.flowCompletionTimesTla.Add (fTime);
	}
}

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
		double r = Math.Log (GlobalVariables.currentTimeSec * 1000);
		double e = Math.Log (GlobalVariables.orderStats [n]);
		double newSigma = (r - rPrev) / (e - ePrev);
		double newMean = r - e * newSigma;
		if ((r == rPrev)) {
			newMean = meanEstimate [requestId];
			newSigma = sigmaEstimate [requestId];
		}/*
            if (id == 6) {
                Console.Error.WriteLine("::::" + newMean + " " + newSigma);
                Console.Error.WriteLine("Current " + r + " " + rPrev + " " + e + " " + ePrev);
            }*/
		double prevMean = meanEstimate [requestId];
		double prevSigma = sigmaEstimate [requestId];
		meanEstimate [requestId] = (prevMean * (n - 1) + newMean) / n;
		sigmaEstimate [requestId] = (prevSigma * (n - 1) + newSigma) / n;
		// Set rprev, eprev
		prevFlowArrivalTime [requestId] = GlobalVariables.currentTimeSec;
	}

	public void UpdateMeanAndSigma (int requestId)
	{
		int n = numFlowsPerRequest [requestId];
		double rPrev = Math.Log (prevFlowArrivalTime [requestId] * 1000);
		double ePrev = Math.Log (GlobalVariables.orderStats [n - 1]);
		double r = Math.Log (GlobalVariables.currentTimeSec * 1000);
		double e = Math.Log (GlobalVariables.orderStats [n]);
		double newSigma = (r - rPrev) / (e - ePrev);
		double newMean = r - e * newSigma;
		if ((r == rPrev)) {
			newMean = meanEstimate [requestId];
			newSigma = sigmaEstimate [requestId];
		}/*
            if (id == 6) {
                Console.Error.WriteLine("::::" + newMean + " " + newSigma);
                Console.Error.WriteLine("Current " + r + " " + rPrev + " " + e + " " + ePrev);
            }*/
		double prevMean = meanEstimate [requestId];
		double prevSigma = sigmaEstimate [requestId];
		meanEstimate [requestId] = (prevMean * (n - 1) + newMean) / n;
		sigmaEstimate [requestId] = (prevSigma * (n - 1) + newSigma) / n;
		// Set rprev, eprev
		prevFlowArrivalTime [requestId] = GlobalVariables.currentTimeSec;
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
				waitTime [requestId] = 0.001 * GlobalVariables.GetOptimalWaitTimeLogNormal (
                        meanEstimate [requestId], sigmaEstimate [requestId],
                        GlobalVariables.mlaLogNormalTimeMean, GlobalVariables.mlaLogNormalTimeSigma);
				GlobalVariables.UpdateGlobalWaitTime (requestId);
			}
			//Console.Error.WriteLine("{0} New Flow: {1} {2}", requestId, numFlowsPerRequest[requestId], GlobalVariables.currentTimeSec);
		} else {   // first flow
			numFlowsPerRequest.Add (requestId, 1);
			firstFlowArrivalTimePerRequest.Add (requestId, GlobalVariables.currentTimeSec);
			informationContentPerRequest.Add (requestId, f.informationContent);
			meanEstimate.Add (requestId, GlobalVariables.facebookLogNormalMean);
			sigmaEstimate.Add (requestId, GlobalVariables.facebookLogNormalSigma);
			waitTime.Add (requestId, GlobalVariables.mlaWaitTimeSec);
			prevFlowArrivalTime.Add (requestId, GlobalVariables.currentTimeSec);
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
			if ((GlobalVariables.currentTimeSec >= beginTime + w)
				|| (numFlowsPerRequest [req.Key] == GlobalVariables.NumWorkerPerMla)) {   
				/* if (id == 6) */
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
		double r = Math.Log (GlobalVariables.currentTimeSec * 1000);
		sum1 [requestId] += r;
		sum2 [requestId] += r * r;

		meanEstimate [requestId] = sum1 [requestId] / n;
		sigmaEstimate [requestId] = Math.Sqrt ((sum2 [requestId] / n) - (meanEstimate [requestId] * meanEstimate [requestId]));
		// Set rprev, eprev
		prevFlowArrivalTime [requestId] = GlobalVariables.currentTimeSec;
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
				waitTime [requestId] = 0.001 * GlobalVariables.GetOptimalWaitTimeLogNormal (
                        meanEstimate [requestId], sigmaEstimate [requestId],
                        GlobalVariables.mlaLogNormalTimeMean, GlobalVariables.mlaLogNormalTimeSigma);
				GlobalVariables.UpdateGlobalWaitTime (requestId);
			}
			//Console.Error.WriteLine("{0} New Flow: {1} {2}", requestId, numFlowsPerRequest[requestId], GlobalVariables.currentTimeSec);
		} else {   // first flow
			numFlowsPerRequest.Add (requestId, 1);
			double x = Math.Log (1000.0 * GlobalVariables.currentTimeSec);
			sum1 [requestId] = x;
			sum2 [requestId] = x * x;
			firstFlowArrivalTimePerRequest.Add (requestId, GlobalVariables.currentTimeSec);
			informationContentPerRequest.Add (requestId, f.informationContent);
			meanEstimate.Add (requestId, GlobalVariables.facebookLogNormalMean);
			sigmaEstimate.Add (requestId, GlobalVariables.facebookLogNormalSigma);
			waitTime.Add (requestId, GlobalVariables.mlaWaitTimeSec);
			prevFlowArrivalTime.Add (requestId, GlobalVariables.currentTimeSec);
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
			if ((GlobalVariables.currentTimeSec >= beginTime + w)
				|| (numFlowsPerRequest [req.Key] == GlobalVariables.NumWorkerPerMla)) {
                    
				/* if (id == 6) 
                        Console.Error.WriteLine("Wait Time is: " + w + " Mean " + meanEstimate[req.Key] + " Sigma " + sigmaEstimate[req.Key]
                       + " " + numFlowsPerRequest[req.Key]);
                    */
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
			firstFlowArrivalTimePerRequest.Add (requestId, GlobalVariables.currentTimeSec);
			informationContentPerRequest.Add (requestId, f.informationContent);
			meanEstimate.Add (requestId, GlobalVariables.facebookLogNormalMean);
			sigmaEstimate.Add (requestId, GlobalVariables.facebookLogNormalSigma);
			waitTime.Add (requestId, GlobalVariables.mlaWaitTimeSec);
			prevFlowArrivalTime.Add (requestId, GlobalVariables.currentTimeSec);
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
			if ((GlobalVariables.currentTimeSec >= beginTime + w)
				|| (numFlowsPerRequest [req.Key] == GlobalVariables.NumWorkerPerMla)) {
				/* if (id == 6) */
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
		return 0.001 * GlobalVariables.GetOptimalWaitTimeLogNormal (mean, sigma, 
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
			opti [requestId] = 0.001 * GlobalVariables.GetOptimalWaitTimeLogNormal (
                        GlobalVariables.workerLogNormalTimeMean, GlobalVariables.workerLogNormalTimeSigma,
                        GlobalVariables.mlaLogNormalTimeMean, GlobalVariables.mlaLogNormalTimeSigma);
			numFlowsPerRequest.Add (requestId, 1);
			firstFlowArrivalTimePerRequest.Add (requestId, GlobalVariables.currentTimeSec);
			informationContentPerRequest.Add (requestId, f.informationContent);
			meanEstimate.Add (requestId, GlobalVariables.facebookLogNormalMean);
			sigmaEstimate.Add (requestId, GlobalVariables.facebookLogNormalSigma);
			waitTime.Add (requestId, GlobalVariables.mlaWaitTimeSec);
			prevFlowArrivalTime.Add (requestId, GlobalVariables.currentTimeSec);
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
			if ((GlobalVariables.currentTimeSec >= beginTime + w)
				|| (numFlowsPerRequest [req.Key] == GlobalVariables.NumWorkerPerMla)) {
				/* if (id == 6) */
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
			firstFlowArrivalTimePerRequest.Add (requestId, GlobalVariables.currentTimeSec);
			informationContentPerRequest.Add (requestId, f.informationContent);
			meanEstimate.Add (requestId, GlobalVariables.facebookLogNormalMean);
			sigmaEstimate.Add (requestId, GlobalVariables.facebookLogNormalSigma);
			waitTime.Add (requestId, GlobalVariables.mlaWaitTimeSec);
			prevFlowArrivalTime.Add (requestId, GlobalVariables.currentTimeSec);
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
			if ((GlobalVariables.currentTimeSec >= beginTime + w)
				|| (numFlowsPerRequest [req.Key] == GlobalVariables.NumWorkerPerMla)) {
				/* if (id == 6) */
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



