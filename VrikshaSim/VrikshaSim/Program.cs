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
            //MainRequestDeadlines(args);
            //MeetingRequestDeadlinesExperiment(args, SchedulerType.TCP, 2000, null, null); ;

            WaitTimes.MultipleWaitTimes(args); 
            //Test.WaitTime(); ;
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
                mlas.Add(new MLA(GlobalVariables.NumQueuesInMla, topScheduler));
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
                workers.Add(new Worker(GlobalVariables.NumQueuesInWorker, midSchedulers[i / GlobalVariables.NumWorkerPerMla]));
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



        static void MeetingFlowDeadlinesExperiment(string[] args)
        {
            int numIterations = 1;

            int totalDeadlinesMet = 0;
            for (int i = 0; i < numIterations; i++)
            {
                GlobalVariables.currentTimeSec = 0;
                List<Flow> flowsToSchedule = DataGenerator.D3GenerateData(30, DataGenerator.D3DeadlineTag.VeryTight);
                EDFPreEmptiveScheduler scheduler = new EDFPreEmptiveScheduler(1000000000.0, new HLA());
                List<Flow> finishedFlows = scheduler.ScheduleFlows(flowsToSchedule);
                //Console.WriteLine("#Flows completed: {0}", finishedFlows.Count(flow => flow.IsFinished()));
                //Console.WriteLine("#Flows Deadline Met: {0}", finishedFlows.Count(flow => flow.DeadlineMet()));
                int numDeadlinesMet = finishedFlows.Count(flow => flow.DeadlineMet());
                totalDeadlinesMet += numDeadlinesMet;
            }
            Console.WriteLine("EDF Pre : #Avg. {0}", totalDeadlinesMet / 1000.0);

            totalDeadlinesMet = 0;
            for (int i = 0; i < 1; i++)
            {
                GlobalVariables.currentTimeSec = 0;
                List<Flow> flowsToSchedule = DataGenerator.D3GenerateData(30, DataGenerator.D3DeadlineTag.VeryTight);
                D3Scheduler scheduler = new D3Scheduler(1000000000.0, new HLA(), true);
                List<Flow> finishedFlows = scheduler.ScheduleFlows(flowsToSchedule);
                //Console.WriteLine("#Flows completed: {0}", finishedFlows.Count(flow => flow.IsFinished()));
                //Console.WriteLine("#Flows Deadline Met: {0}", finishedFlows.Count(flow => flow.DeadlineMet()));
                int numDeadlinesMet = finishedFlows.Count(flow => flow.DeadlineMet());
                totalDeadlinesMet += numDeadlinesMet;
            }
            Console.WriteLine("D3 : #Avg. {0}", totalDeadlinesMet / 1000.0);

            // TCP Fair Scheduler
            /*
            totalDeadlinesMet = 0;
            for (int i = 0; i < numIterations; i++)
            {
                GlobalVariables.currentTimeSec = 0;
                List<Flow> flowsToSchedule = DataGenerator.D3GenerateData(30, DataGenerator.D3DeadlineTag.VeryTight);
                TCPFairScheduler scheduler = new TCPFairScheduler(1000000000.0, new TLA());
                List<Flow> finishedFlows = scheduler.ScheduleFlows(flowsToSchedule);
                // Console.WriteLine("#Flows completed: {0}", finishedFlows.Count(flow => flow.IsFinished()));
                // Console.WriteLine("#Flows Deadline Met: {0}", finishedFlows.Count(flow => flow.DeadlineMet()));
                int numDeadlinesMet = finishedFlows.Count(flow => flow.DeadlineMet());
                totalDeadlinesMet += numDeadlinesMet;
            }
            Console.WriteLine("TCP Fair: #Avg. {0}", 1.0 * totalDeadlinesMet / numIterations ); 
            */
        }
    }




    


    public class Test
    {
        public static void main()
        {
            List<Task> l = new List<Task>();
            GlobalVariables.init(9000);
            for (int i = 0; i < 10000; i++)
            {
                l.Add(Task.CreateNewTask(0, TaskType.WorkerTask));
                l[i].progressSec = GlobalVariables.rnd.GetExponentialSample(2.5);
            }

            // SJF
            int numCompleted = 0;
            double currentTime = 0;
            List<Double> responseTimes = new List<Double>();
            List<Task> finished = new List<Task>();
            var A = l.OrderBy(x => x.processingTimeSec).ToList();
            foreach (Task t in A)
            {
                currentTime += t.processingTimeSec;
                responseTimes.Add(currentTime);
                numCompleted += 1;
                // Console.WriteLine(numCompleted);
            }

            responseTimes.Sort();
            double avg = responseTimes.Average();
            double perc99 = responseTimes[9899];
            double perc99_5 = responseTimes[9949];
            Console.WriteLine("Average:{0}, 99Percentile: {1}, 99.5Percentile: {2}", avg, perc99, perc99_5);

            var B = l.OrderBy(x => x.progressSec);
            responseTimes = new List<Double>();
            currentTime = 0;
            foreach (Task t in B)
            {
                currentTime += t.processingTimeSec;
                //finished.Add(t);
                responseTimes.Add(currentTime);
                numCompleted += 1;
                // Console.WriteLine(numCompleted);
            }
            responseTimes.Sort();
            avg = responseTimes.Average();
            perc99 = responseTimes[9899];
            perc99_5 = responseTimes[9949];
            Console.WriteLine("Average:{0}, 99%: {1}, 99.5%: {2}", avg, perc99, perc99_5);

            List<Task> C = l.OrderBy(x => x.processingTimeSec).ToList();
            responseTimes = new List<Double>();
            currentTime = 0;
            foreach (Task t in C)
            {
                t.progressSec = 0;
            }
            int numRemaining = C.Count;
            while (numRemaining > 0)
            {
                //currentTime += 0.01;
                double increment = 0.1 / numRemaining;
                List<Task> toRemove = new List<Task>();
                foreach (Task t in C)
                {
                    currentTime += increment;
                    t.progressSec += increment;
                    if (t.progressSec >= t.processingTimeSec)
                    {
                        toRemove.Add(t);
                        responseTimes.Add(currentTime);
                        numRemaining--;
                        ///if (numRemaining % 10000 == 0) { 
                        //    Console.WriteLine(numRemaining);
                        //}
                    }
                }
                foreach (Task t in toRemove)
                {
                    C.Remove(t);
                }
            }
            responseTimes.Sort();
            avg = responseTimes.Average();
            perc99 = responseTimes[9899];
            perc99_5 = responseTimes[9949];
            Console.WriteLine("Average:{0}, 99Percentile: {1}, 99.5Percentile: {2}", avg, perc99, perc99_5);
        }


        public static void WaitTime()
        {
            TextWriter logOut = null; ;
            try
            {
                logOut = new StreamWriter("waitTime.txt");
                Console.SetOut(logOut);
            }
            catch (IOException exc)
            {
                Console.WriteLine(exc.Message + "Cannot open file.");
            }
            List<Randomness> generators = new List<Randomness>();
            int K = 40;
            for (int i = 0; i < K + 1; i++)
            {
                generators.Add(new Randomness(i));
            }

            int NumSamples = 1000;
            List<Double> Z = new List<Double>();
            List<Double> Y = new List<Double>();
            List<Double> X1 = new List<Double>();

            List<Double> X2 = new List<Double>();
            for (int i = 0; i < NumSamples; i++)
            {
                double x2 = generators[0].GetExponentialSample(10.0);
                double y = 0;
                X2.Add(x2);
                double x1 = 0;
                for (int j = 1; j <= K; j++)
                {
                    double x = generators[j].GetExponentialSample(10.0);
                    if (j == 1)
                    {
                        x1 = x;
                        X1.Add(x);
                    }
                    if (x > y)
                    {
                        y = x;
                    }
                }
                Y.Add(y);
                Z.Add(y + x2);
                Console.WriteLine("{0} {1} {2} {3}", x2, x1 + x2, y, y + x2);
            }
            logOut.Close();
        }

    }

}