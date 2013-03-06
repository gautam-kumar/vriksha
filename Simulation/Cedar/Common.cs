using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Request
{
	public int requestId;
	public TimeSpan beginSec;
	public TimeSpan endSec;
	public double numFlowsCompleted; //Measure of how many flows have been completed, information content in the request
	public double informationContent;
	public double workerTaskTimeMeanSec;
	public double mlaTaskTimeMeanSec;
	public double sgm = 0;

	public Request (int requestId, double workerTaskTimeMeanSec, double mlaTaskTimeMeanSec) // double firstComputationSec, double secondComputationSec)
	{
		this.beginSec = GlobalVariables.currentTime;
		this.requestId = requestId;
		this.numFlowsCompleted = 0;
		this.informationContent = 0;
		this.workerTaskTimeMeanSec = workerTaskTimeMeanSec; 
		this.mlaTaskTimeMeanSec = mlaTaskTimeMeanSec;
	}

	public static Request GetNewRequest (int requestId)
	{
		double f = 0;
		double s = 0;
		switch (GlobalVariables.mlaDistType) {
		case DistType.Normal:
			s = GlobalVariables.mlaComputationTimeMeanMs;
			break;
		case DistType.Exponential:
			s = GlobalVariables.mlaComputationTimeMeanMs;
			break;
		case DistType.Google:
			s = 2.94;
			break;
		default:
			s = GlobalVariables.workerLogNormalTimeMean;
			break;
		}

		switch (GlobalVariables.workerDistType) {
		case DistType.Normal:
			f = GlobalVariables.workerComputationTimeMeanMs;
			break;
		case DistType.Exponential:
			f = GlobalVariables.workerComputationTimeMeanMs;
			break;
		case DistType.Google:
			f = 2.94;
			break;
		default:
			f = GlobalVariables.mlaLogNormalTimeMean;
			break;
		}
		return new Request (requestId, f, s);
	}

	public TimeSpan TimeTaken {
		get { return endSec - beginSec; }
	}
}

public class Task
{
	public TimeSpan processingTimeSec;
	public int requestId;
	public TimeSpan progressSec;
	public TaskType taskType;

	private Task (int requestId, TimeSpan processingTime, TaskType t)
	{
		this.requestId = requestId;
		this.processingTimeSec = processingTime;
		this.taskType = t;
	}

	public static Task CreateNewTask (int requestId, TaskType type)
	{
		Randomness rnd = GlobalVariables.rnd;
		switch (type) {
		case TaskType.WorkerTask:
			double fs = GlobalVariables.requests [requestId].workerTaskTimeMeanSec;
			double fsStdev = GlobalVariables.requests [requestId].sgm;
			double f;
			switch (GlobalVariables.workerDistType) {
			case DistType.Exponential:
				f = rnd.GetExponentialSample (fs);
                          //      Math.Max(0.005, 
                           //         Math.Min(rnd.GetExponentialSample(fs), 
                           //             GlobalVariables.workerComputationTimeMaxPerRequest));
				break;
			case DistType.LogNormal:
				fs = GlobalVariables.workerLogNormalTimeMean;
				fsStdev = GlobalVariables.workerLogNormalTimeSigma;
				f = rnd.GetLogNormalSample (fs, fsStdev); 
				break;
			case DistType.DcTcp:
				f = GlobalVariables.GetDcTcpSample ();
				break;
			case DistType.Facebook:
				f = GlobalVariables.GetFacebookSampleMs (requestId);
				break;
			case DistType.Google:
				f = 0.001 * rnd.GetLogNormalSample (2.94, 0.55);
				break;
			case DistType.LogNormalPerJob:
                            //Console.Error.WriteLine("Fs: " + fs);
				f = rnd.GetLogNormalSample (fs, GlobalVariables.requests [requestId].sgm);
				break;
			default: // Normal
				f = Math.Max (0, rnd.GetNormalSample (fs, fsStdev));
				break;
			}
			return new Task (requestId, new TimeSpan(0, 0, 0, 0, (int) f), TaskType.WorkerTask);

                    
		case TaskType.MlaTask:
			double ss = GlobalVariables.requests [requestId].mlaTaskTimeMeanSec;
			double ssStdev = GlobalVariables.mlaComputationTimeSigmaPerRequestMs;
			double s;
			switch (GlobalVariables.mlaDistType) {
			case DistType.Exponential:
				s = rnd.GetExponentialSample (ss);
                            //      Math.Max(0.005, 
                            //         Math.Min(rnd.GetExponentialSample(fs), 
                            //             GlobalVariables.workerComputationTimeMaxPerRequest));
				break;
			case DistType.LogNormal:
				ss = GlobalVariables.mlaLogNormalTimeMean;
				ssStdev = GlobalVariables.mlaLogNormalTimeSigma;
				s = rnd.GetLogNormalSample (ss, ssStdev);
				break;
			case DistType.DcTcp:
				s = GlobalVariables.GetDcTcpSample ();
				break;
			case DistType.Google:
				s = 0.001 * rnd.GetLogNormalSample (2.94, 0.55);
				break;
			case DistType.Facebook:
				s = GlobalVariables.GetFacebookSampleMs (requestId);
				break;
			default: // Normal
				s = Math.Max (0, rnd.GetNormalSample (ss, ssStdev));
				break;
			}
			return new Task (requestId, new TimeSpan(0, 0, 0, 0, (int) s), TaskType.MlaTask);


		default:
			return new Task (requestId, new TimeSpan(0), TaskType.TlaTask);
		}
	}

}