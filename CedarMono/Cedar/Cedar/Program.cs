using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;



namespace VrikshaSim
{
    class Program
    {
        public static void Main(String[] args)
        {
            FacebookGoogle.FacebookGoogleExperiment(args); 
        }

        static void MainRequestDeadlines(String[] args)
        {
            StreamWriter logOut = null;
            StreamWriter flowInformation = null;

            int[] waitTimes = { 10, 20, 30, 2000 };

            foreach (int w in waitTimes)
            {
                //try
                //{
                //    logOut = new StreamWriter("results-" + w + "ms.txt");
                //    flowInformation = new StreamWriter("flows-" + w + "ms.txt");
                //}
                //catch (IOException exc)
                //{
                //    Console.WriteLine(exc.Message + "Cannot open file.");
                //}
                double wTime = (1.0 * w) / 1000.0;
                GlobalVariables.mlaWaitTimeSec = wTime;
                GlobalVariables.tlaWaitTimeSec = wTime;
                for (int i = 0; i < 15; i++)
                {
                    int fMean = 5000 + (i * 5000);
                    int seed = (int)(10000);
                    GlobalVariables.flowSizeMinBytes = fMean / 2;
                    GlobalVariables.flowSizeMaxBytes = (fMean * 3) / 2;
                    Console.Error.Write("<TCP> " + fMean + ": "); //logOut.Write("<TCP> " + i + ": "); flowInformation.Write("<TCP> " + i + ": ");
                    MeetingRequestDeadlinesExperiment(args, SchedulerType.TCP, seed, logOut, flowInformation);
                    Console.Error.Write("<EDF-NP> " + fMean + ": "); //logOut.Write("<EDF-NP> " + i + ": "); flowInformation.Write("<EDF-NP> " + i + ": ");
                    MeetingRequestDeadlinesExperiment(args, SchedulerType.EDFNonPreEmptive, seed, logOut, flowInformation);
                    Console.Error.Write("<EDF-P> " + fMean + ": "); //logOut.Write("<EDF-P> " + i + ": "); flowInformation.Write("<EDF-P> " + i + ": ");
                    MeetingRequestDeadlinesExperiment(args, SchedulerType.EDFPreEmptive, seed, logOut, flowInformation);
                    Console.Error.Write("<Vriksha> " + fMean + ": "); //logOut.Write("<Vriksha> " + i + ": "); flowInformation.Write("<Vriksha> " + i + ": ");
                    MeetingRequestDeadlinesExperiment(args, SchedulerType.VrikshaNonKilling, seed, logOut, flowInformation);
                    Console.Error.Write("<SJF-P> " + fMean + ": "); //logOut.Write("<SJF-P> " + i + ": "); flowInformation.Write("<SJF-P> " + i + ": ");
                    MeetingRequestDeadlinesExperiment(args, SchedulerType.SJFPreEmptive, seed, logOut, flowInformation);
                }

                if (logOut != null)
                {
                    logOut.Close();
                    flowInformation.Close();
                }
            }
        }

        static void MeetingRequestDeadlinesExperiment(string[] args, SchedulerType type, int seed, TextWriter logOut, TextWriter flowInformation)
        {
            GlobalVariables.init(seed);

            // Initialize System Components
            HLA tla = new HLA();
            FlowScheduler topScheduler;
            switch (type)
            {
                case SchedulerType.TCP:
                    topScheduler = new TCPFairScheduler(GlobalVariables.capacityBps, tla);
                    break;
                case SchedulerType.EDFPreEmptive:
                    topScheduler = new EDFPreEmptiveScheduler(GlobalVariables.capacityBps, tla);
                    break;
                case SchedulerType.EDFNonPreEmptive:
                    topScheduler = new EDFNonPreEmptiveScheduler(GlobalVariables.capacityBps, tla);
                    break;
                case SchedulerType.VrikshaNonKilling:
                    topScheduler = new VrikshaNonKillingScheduler(GlobalVariables.capacityBps, tla, 1);
                    break;
                case SchedulerType.D3:
                    topScheduler = new D3Scheduler(GlobalVariables.capacityBps, tla, true);
                    break;
                case SchedulerType.SJFPreEmptive:
                    topScheduler = new SJFPreEmptiveScheduler(GlobalVariables.capacityBps, tla);
                    break;
                default:
                    topScheduler = new TCPFairScheduler(GlobalVariables.capacityBps, tla);
                    break;
            }

            List<MLA> mlas = new List<MLA>();
            for (int i = 0; i < GlobalVariables.NumMlaPerTla; i++)
            {
                mlas.Add(new MLA(i, GlobalVariables.NumQueuesInMla, tla));
            }

            List<FlowScheduler> midSchedulers = new List<FlowScheduler>();
            for (int i = 0; i < GlobalVariables.NumMlaPerTla; i++)
            {
                switch (type)
                {
                    case SchedulerType.TCP:
                        midSchedulers.Add(new TCPFairScheduler(GlobalVariables.capacityBps, mlas[i]));
                        break;
                    case SchedulerType.EDFPreEmptive:
                        midSchedulers.Add(new EDFPreEmptiveScheduler(GlobalVariables.capacityBps, mlas[i]));
                        break;
                    case SchedulerType.EDFNonPreEmptive:
                        midSchedulers.Add(new EDFNonPreEmptiveScheduler(GlobalVariables.capacityBps, mlas[i]));
                        break;
                    case SchedulerType.VrikshaNonKilling:
                        midSchedulers.Add(new VrikshaNonKillingScheduler(GlobalVariables.capacityBps, mlas[i], 0));
                        break;
                    case SchedulerType.D3:
                        midSchedulers.Add(new D3Scheduler(GlobalVariables.capacityBps, mlas[i], true));
                        break;
                    case SchedulerType.SJFPreEmptive:
                        midSchedulers.Add(new SJFPreEmptiveScheduler(GlobalVariables.capacityBps, mlas[i]));
                        break;
                    default:
                        midSchedulers.Add(new TCPFairScheduler(GlobalVariables.capacityBps, mlas[i]));
                        break;
                }
            }
            List<Worker> workers = new List<Worker>();
            for (int i = 0; i < GlobalVariables.NumWorkers; i++)
            {
                workers.Add(new Worker(GlobalVariables.NumQueuesInWorker, mlas[i / GlobalVariables.NumWorkerPerMla]));
            }

            double timeSinceLastRequestInjectionSec = 1;
            int requestNumber = 0;
            while (requestNumber < GlobalVariables.NumRequestsToSimulate)
            { // TODO:
                if (timeSinceLastRequestInjectionSec >= GlobalVariables.requestGenerationIntervalSec)
                {

                    // Console.WriteLine("RNum: {2}, F: {0}, S: {1}", f, s, requestNumber);
                    Randomness rnd = GlobalVariables.rnd;
                    double fs = GlobalVariables.workerComputationTimeMeanSec, fsStdev = GlobalVariables.workerComputationTimeStdevSec;
                    double f = Math.Max(fs - 1.0 * fsStdev, Math.Min(rnd.GetNormalSample(fs, fsStdev), fs + 1.0 * fsStdev));
                    double ss = GlobalVariables.mlaComputationTimeMeanSec, ssStdev = GlobalVariables.mlaComputationTimeStdevSec;
                    double s = Math.Max(ss - 1.0 * ssStdev, Math.Min(rnd.GetNormalSample(ss, ssStdev), ss + 1.0 * ssStdev));
                    GlobalVariables.requests.Add(new Request(requestNumber, f, s));
                    // Console.Error.WriteLine("Injecting {0}th request at time {1}", requestNumber, GlobalVariables.currentTimeSec);
                    timeSinceLastRequestInjectionSec = 0;
                    foreach (Worker w in workers)
                    {
                        Task t = Task.CreateNewTask(requestNumber, TaskType.WorkerTask);
                        w.InsertTask(t);
                    }
                    requestNumber += 1;
                }
                if (requestNumber < GlobalVariables.NumRequestsToSimulate)
                {
                    timeSinceLastRequestInjectionSec += GlobalVariables.timeIncrementSec;
                }

                foreach (Worker w in workers)
                {
                    w.AdvanceTime(GlobalVariables.timeIncrementSec);
                }
                foreach (FlowScheduler fs in midSchedulers)
                {
                    fs.AdvanceTime(GlobalVariables.timeIncrementSec);
                }
                foreach (MLA m in mlas)
                {
                    m.AdvanceTime(GlobalVariables.timeIncrementSec);
                }
                topScheduler.AdvanceTime(GlobalVariables.timeIncrementSec);
                tla.AdvanceTime(GlobalVariables.timeIncrementSec);

                GlobalVariables.currentTimeSec += GlobalVariables.timeIncrementSec;
            }

            // Remove the first edge requests
            List<Request> edgeRequests = new List<Request>();
            List<double> responseTimes = new List<double>();
            foreach (Request r in tla.completedRequests)
            {
                //Console.WriteLine(r.requestId);
                if (r.requestId < GlobalVariables.NumEdgeRequestsToDelete || r.requestId >= (GlobalVariables.NumRequestsToSimulate - GlobalVariables.NumEdgeRequestsToDelete))
                {
                    edgeRequests.Add(r);
                }
                else
                {
                    responseTimes.Add(r.endSec - r.beginSec);
                }
            }
            //Console.Error.WriteLine("Removed Requests: {0}", edgeRequests.Count);
            foreach (Request r in edgeRequests)
            {
                tla.completedRequests.Remove(r);
            }

            Console.Error.WriteLine("\n" + type + "\n");
            Console.Error.Write("Completed Requests: {0}, ", tla.completedRequests.Count);
            responseTimes.Sort();
            Console.Error.WriteLine("Min: {0}, Max: {1} Mean: {2}, 99%: {3}, 99.9%: {4} MeanIC: {5}, FlowsMissed: {6}", tla.completedRequests.Min(r => r.TimeTaken),
                    tla.completedRequests.Max(r => r.TimeTaken), tla.completedRequests.Sum(r => r.TimeTaken) / tla.completedRequests.Count,
                    responseTimes[99 * responseTimes.Count / 100],
                    responseTimes[999 * responseTimes.Count / 1000],
                    tla.completedRequests.Sum(r => r.informationContent) / tla.completedRequests.Count,
                    GlobalVariables.numFlowsMissed);
            if (logOut != null)
            {
                logOut.Write("Completed Requests: {0}, ", tla.completedRequests.Count);
                logOut.WriteLine("Min: {0}, Max: {1} Mean: {2}, 99%: {3}, 99.9%: {4} MeanIC: {5}, FlowsMissed: {6}", tla.completedRequests.Min(r => r.TimeTaken),
                    tla.completedRequests.Max(r => r.TimeTaken), tla.completedRequests.Sum(r => r.TimeTaken) / tla.completedRequests.Count,
                    responseTimes[99 * responseTimes.Count / 100],
                    responseTimes[999 * responseTimes.Count / 1000],
                    tla.completedRequests.Sum(r => r.informationContent) / tla.completedRequests.Count,
                    GlobalVariables.numFlowsMissed);
            }
            GlobalVariables.flowCompletionTimesTla.Sort();
            GlobalVariables.flowCompletionTimesMla.Sort();
            int count = GlobalVariables.flowCompletionTimesMla.Count;
            Console.Error.WriteLine("<MLA> NumFlowCompleted: {0} Mean: {1} 50%: {2} 70%: {3}, 80%: {4}, 90%: {5}, 95%: {6}, 99%: {7} 99.5%: {8} ",
                count,
                GlobalVariables.flowCompletionTimesMla.Sum(r => r) / count,
                GlobalVariables.flowCompletionTimesMla[(int)(0.5 * count)],
                GlobalVariables.flowCompletionTimesMla[(int)(0.7 * count)],
                GlobalVariables.flowCompletionTimesMla[(int)(0.8 * count)],
                GlobalVariables.flowCompletionTimesMla[(int)(0.9 * count)],
                GlobalVariables.flowCompletionTimesMla[(int)(0.95 * count)],
                GlobalVariables.flowCompletionTimesMla[(int)(0.99 * count)],
                GlobalVariables.flowCompletionTimesMla[(int)(0.995 * count)]);
            count = GlobalVariables.flowCompletionTimesTla.Count;
            Console.Error.WriteLine("<TLA> NumFlowCompleted: {0} Mean: {1} Median: {2} 99Perc: {3} 99.5Perc: {4} 99.9%: {5}",
                count,
                GlobalVariables.flowCompletionTimesTla.Sum(r => r) / count,
                GlobalVariables.flowCompletionTimesTla[(int)(0.5 * count)],
                GlobalVariables.flowCompletionTimesTla[(int)(0.99 * count)],
                GlobalVariables.flowCompletionTimesTla[(int)(0.995 * count)],
                GlobalVariables.flowCompletionTimesTla[(int)(0.999 * count)]);
            if (flowInformation != null)
            {
                flowInformation.WriteLine("NumFlowCompleted: {0} Mean: {1} Median: {2} 99Perc: {3} 99.5Perc: {4} ",
                count,
                GlobalVariables.flowCompletionTimesMla.Sum(r => r) / count,
                GlobalVariables.flowCompletionTimesMla[(int)(0.5 * count)],
                GlobalVariables.flowCompletionTimesMla[(int)(0.99 * count)],
                GlobalVariables.flowCompletionTimesMla[(int)(0.995 * count)]);
            }
            List<Double> mlaUtilization = new List<Double>();
            foreach (FlowScheduler s in midSchedulers)
            {
                mlaUtilization.Add(s.bytesCount * 8 / (GlobalVariables.capacityBps * (GlobalVariables.endCountingUtilization - GlobalVariables.startCountingUtilization)));
            }
            Console.Error.WriteLine("<MLA> Utilization Mean: {0} Max = {1} Min = {2}", mlaUtilization.Average(), mlaUtilization.Max(), mlaUtilization.Min()); ;

        }

	}
}