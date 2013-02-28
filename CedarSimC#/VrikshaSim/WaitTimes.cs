﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace VrikshaSim
{
    class WaitTimes
    {
        public static void MultipleWaitTimes(string[] args)
        {
            int[] T = new int[10] { 5, 10, 15, 20, 25, 30, 35, 40, 45, 50 };
            foreach (int t in T)
            {
                GlobalVariables.NumWorkerPerMla = t;
                GlobalVariables.NumMlaPerTla = t;
                GlobalVariables.NumWorkers = t * t;

                Console.Error.WriteLine("Topology: " + t);
                double[] wT = new double[1] { 2.5 };
                //double[] wT = new double[1] { 0003 };
                GlobalVariables.workerDistType = DistType.Exponential;
                GlobalVariables.mlaDistType = DistType.Exponential;
                GlobalVariables.workerComputationTimeMeanSec = 0.1;
                GlobalVariables.workerComputationTimeSigmaSecPerRequest = 0.11;
                     GlobalVariables.mlaComputationTimeMeanSec = 0.01;
                GlobalVariables.mlaComputationTimeSigmaSecPerRequest = 0.11;
                GlobalVariables.timeIncrementSec = 0.001;
                //GlobalVariables.requestGenerationIntervalSec = 0.001;
                foreach (double w  in wT)
                {
                    double[] stdev = new double[5] {0.11, 0.12, 0.13, 0.14, 0.15};
                    foreach (double st in stdev)
                    {

                        GlobalVariables.workerComputationTimeSigmaSecPerRequest
                            = GlobalVariables.mlaComputationTimeSigmaSecPerRequest = st;

                        GlobalVariables.tlaWaitTimeSec = w;
                        for (double waitTime = w / 10.0; waitTime < GlobalVariables.tlaWaitTimeSec - w / 20.0;
                           waitTime += w / 10.0)
                        {
                            //double waitTime = 0.11;
                            GlobalVariables.mlaWaitTimeSec = waitTime;
                            Tuple<double, double> res = WaitTimesExperiment(args, 1000, null, null);
                        }
                    }
                }
            }

        }


        public static Tuple<double, double> WaitTimesExperiment(string[] args, 
            int seed, TextWriter logOut, TextWriter flowInformation)
        {
            GlobalVariables.init(seed);

            // Initialize System Components
            GlobalVariables.hla = new HLA();
            GlobalVariables.mlas = new List<MLA>();
            for (int i = 0; i < GlobalVariables.NumMlaPerTla; i++)
            {
                GlobalVariables.mlas.Add(new MLA(i, GlobalVariables.NumQueuesInMla,
                    GlobalVariables.hla));
                //GlobalVariables.mlas[i].Setid(i);
            }
            GlobalVariables.workers = new List<Worker>();
            for (int i = 0; i < GlobalVariables.NumWorkers; i++) 
            {
                GlobalVariables.workers.Add(new Worker(GlobalVariables.NumQueuesInWorker,
                    GlobalVariables.mlas[i / GlobalVariables.NumWorkerPerMla]));
            }

            int requestNumber = 0;
            while (requestNumber < GlobalVariables.NumRequestsToSimulate)
            {
                GlobalVariables.requests.Add(Request.GetNewRequest(requestNumber));
                List<Double> durations = new List<Double>();
                foreach (Worker w in GlobalVariables.workers)
                {
                    Task t = Task.CreateNewTask(requestNumber, TaskType.WorkerTask);
                    w.InsertTask(t);
                    durations.Add(t.processingTimeSec);
                }

                GlobalVariables.secondStageTasks[requestNumber] = new List<Task>();
                GlobalVariables.optimalWaitTimes[requestNumber] = new List<double>();
                int index = 0;
                foreach (MLA m in GlobalVariables.mlas)
                {
                    GlobalVariables.secondStageTasks[requestNumber].Add(Task.CreateNewTask(requestNumber, TaskType.MlaTask));
                    GlobalVariables.optimalWaitTimes[requestNumber].Add(OptimalMLA.OptimalWaitTimeLogNormal(durations.GetRange(index, GlobalVariables.NumWorkerPerMla)));
                    index += GlobalVariables.NumWorkerPerMla;
                }
                requestNumber += 1;

            }
            while (GlobalVariables.currentTimeSec <= 
                GlobalVariables.tlaWaitTimeSec + 2 * GlobalVariables.timeIncrementSec) 
            //while (GlobalVariables.currentTimeSec <= 1.3 * GlobalVariables.tlaWaitTimeSec)
            {
                //Console.Error.WriteLine(GlobalVariables.currentTimeSec + ": " + hla.completedRequests.Count);
                foreach (Worker w in GlobalVariables.workers)
                {
                    w.AdvanceTime(GlobalVariables.timeIncrementSec);
                }
                foreach (MLA m in GlobalVariables.mlas)
                {
                    m.AdvanceTime(GlobalVariables.timeIncrementSec);
                }
                GlobalVariables.hla.AdvanceTime(GlobalVariables.timeIncrementSec);

                GlobalVariables.currentTimeSec += GlobalVariables.timeIncrementSec;
                //Console.Error.WriteLine(GlobalVariables.currentTimeSec);
            }

            //Console.Error.WriteLine("Time is " + GlobalVariables.currentTimeSec);


             
            // Remove the first edge requests
            List<Request> edgeRequests = new List<Request>();
            List<double> informationContents = new List<double>();
            List<double> responseTimes = new List<double>();
            Dictionary<int, double> icMap = new Dictionary<int, double>();
            foreach (Request r in GlobalVariables.hla.completedRequests)
            {
                //Console.WriteLine(r.requestId);
                if (r.requestId < GlobalVariables.NumEdgeRequestsToDelete || r.requestId >= (GlobalVariables.NumRequestsToSimulate - GlobalVariables.NumEdgeRequestsToDelete))
                {
                    edgeRequests.Add(r);
                }
                else
                {
                    icMap[r.requestId] = r.informationContent;
                    informationContents.Add(r.informationContent);
                    responseTimes.Add(r.endSec - r.beginSec);
                }
            }
            //Console.Error.WriteLine("Removed Requests: {0}", edgeRequests.Count);
            /*List<double> icValuesById = new List<double>();
            StreamWriter sw = new StreamWriter(@"text\Prop" + GlobalVariables.tlaWaitTimeSec + ".txt");
            for (int i = 0; i < GlobalVariables.NumRequestsToSimulate; i++)
            {
                if (icMap.ContainsKey(i))
                {
                    sw.WriteLine(icMap[i]);
                }
                else
                {
                    sw.WriteLine(0);
                }
            }
            sw.Close();*/


            foreach (Request r in edgeRequests)
            {
                GlobalVariables.hla.completedRequests.Remove(r);
            }
            int numToFill  = GlobalVariables.NumRequestsToSimulate - GlobalVariables.hla.completedRequests.Count;
            for (int i = 0; i < numToFill; i++)
            {
                informationContents.Add(0.0);
            }
            
            //Console.Error.Write(GlobalVariables.mlaWaitTimeSec + " Completed Requests: {0}, ", tla.completedRequests.Count);
            informationContents.Sort();
            double averageIc = informationContents.Sum() / GlobalVariables.NumRequestsToSimulate;
            int index50 = ((int)(50 * informationContents.Count / 100.0)) - 1;
            double percentile50 = 0;
            if (index50 >= 0 && index50 < informationContents.Count)
            {
                percentile50 = informationContents[index50];
            }

            int index60 = ((int)(40 * informationContents.Count / 100.0)) - 1;
            double percentile60 = 0;
            if (index60 >= 0 && index60 < informationContents.Count)
            {
                percentile60 = informationContents[index60];
            }

            int index75 = ((int)(25 * informationContents.Count / 100.0)) - 1;
            double percentile75 = 0;
            if (index75 >= 0 && index75 < informationContents.Count)
            {
                percentile75 = informationContents[index75];
            }

            int index80 = ((int)(20 * informationContents.Count / 100.0)) - 1;
            double percentile80 = 0;
            if (index80 >= 0 && index80 < informationContents.Count)
            {
                percentile80 = informationContents[index80];
            }

            int index90 = ((int)(10 * informationContents.Count / 100.0)) - 1;
            double percentile90 = 0;
            if (index90 >= 0 && index90 < informationContents.Count)
            {
                percentile90 = informationContents[index90];
            }

            int index95 = ((int)(5 * informationContents.Count / 100.0)) - 1;
            double percentile95 = 0;
            if (index95 >= 0 && index95 < informationContents.Count)
            {
                percentile95 = informationContents[index95];
            }

            int index98 = ((int)(2 * informationContents.Count / 100.0)) - 1;
            double percentile98 = 0;
            if (index98 >= 0 && index98 < informationContents.Count)
            {
                percentile98 = informationContents[index98];
            }



            int index1 = ((int)(1.0 * informationContents.Count / 100.0)) - 1;
            double percentile99 = 0;
            if (index1 >= 0 && index1 < informationContents.Count)
            {
                percentile99 = informationContents[index1];
            }

            int index2 = ((int)(informationContents.Count / 1000.0)) - 1;
            double percentile99_9 = 0;
            if (index2 >= 0 && index2 < informationContents.Count)
            {
                percentile99_9 = informationContents[index2];
            }

            // #Requests with IC > 0.9, 0.95 and 0.99
            int num80 = 0, num90 = 0, num95 = 0, num99 = 0;
            for (int i = 0; i < informationContents.Count; i++)
            {
                if (informationContents[i] >= 0.8)
                {
                    if (num80 == 0)
                    {
                        num80 = informationContents.Count - i;
                    }
                }

                if (informationContents[i] >= 0.9)
                {
                    if (num90 == 0)
                    {
                        num90 = informationContents.Count - i;
                    }
                }
                if (informationContents[i] >= 0.95)
                {
                    if (num95 == 0)
                    {
                        num95 = informationContents.Count - i;
                    }
                }
                if (informationContents[i] >= 0.99)
                {
                    if (num99 == 0)
                    {
                        num99 = informationContents.Count - i;
                    }
                }
            }
            Console.Error.WriteLine(GlobalVariables.workerLogNormalTimeSigma + " " +
                GlobalVariables.mlaWaitTimeSec + " " + averageIc);
            /*
            Console.Error.WriteLine(GlobalVariables.workerLogNormalTimeSigma + " " +
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
                " NumCompleted " + informationContents.Count);*/
            return new Tuple<double, double>(averageIc, percentile50);
        }
    }
}
