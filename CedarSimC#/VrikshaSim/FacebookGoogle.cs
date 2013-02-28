using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace VrikshaSim
{
    class FacebookGoogle
    {
        public static void FacebookGoogleExperiment(string[] args)
        {

            int[] T = new int[1] {50};
            foreach (int t in T)
            {
                GlobalVariables.NumWorkerPerMla = t;
                GlobalVariables.NumMlaPerTla = t;
                GlobalVariables.NumWorkers = t * t;

                Console.Error.WriteLine("Topology: " + t);
                double[] wT = new double[1] {0.10};
                double[] stdev = new double[3] {0.75, 1.0, 1.25};
                foreach (double s in stdev)
                {
                    //Console.Error.WriteLine("Stdev " + s);
                    GlobalVariables.ReadDcTcpNumbers();
                    GlobalVariables.workerDistType = DistType.LogNormal ;
                    GlobalVariables.workerLogNormalTimeMean = 3.0;
                    GlobalVariables.workerLogNormalTimeSigma = s;
                    GlobalVariables.mlaDistType = DistType.LogNormal;
                    GlobalVariables.mlaLogNormalTimeMean = 3.0;
                    GlobalVariables.mlaLogNormalTimeSigma = s;
                    GlobalVariables.timeIncrementSec = 0.001;
                    GlobalVariables.NumRequestsToSimulate = 100;

                    double m = GlobalVariables.workerLogNormalTimeMean; double g = GlobalVariables.workerLogNormalTimeSigma;
                    double a = Math.Exp(m + g * g / 2);
                    m = GlobalVariables.mlaLogNormalTimeMean; g = GlobalVariables.mlaLogNormalTimeSigma;
                    double b = Math.Exp(m + g * g / 2);
                    //Console.Error.WriteLine(a + ", " + b);
                    //GlobalVariables.requestGenerationIntervalSec = 0.001;
                    GlobalVariables.ReadDcTcpNumbers();
                    double[] opt = new double[9] { 0.092, 0.097, 0.101, 0.105, 0.11, 0.114, 0.118, 0.123, 0.127 };
                    for (int i = 0; i < wT.Length; i++)
                    {
                        GlobalVariables.tlaWaitTimeSec = wT[i];
                        double tmp = 0;
                        //double waitTime = 0.11;
                        
                         //     b);
                        
                        //double tmp = GlobalVariables.tlaWaitTimeSec / 2;
                        //double tmp = 0.187;
                        
                        //double tmp = (157.8 * GlobalVariables.tlaWaitTimeSec / 179.8);
                        //tmp = GlobalVariables.tlaWaitTimeSec / 2.0;
                        //GlobalVariables.mlaWaitTimeSec = tmp;
                        //Tuple<double, double> res1 = WaitTimes.WaitTimesExperiment(args, 1000, null, null);
                        tmp = (a * GlobalVariables.tlaWaitTimeSec / (a + b));
                        //GlobalVariables.mlaWaitTimeSec = tmp;
                        //Tuple<double, double> res2 = WaitTimes.WaitTimesExperiment(args, 2000, null, null);
                        //Console.Error.WriteLine("Quality " + t + " " + res2.Item2);
                        //tmp = opt[i];
                        
                        tmp = 0.001 * GlobalVariables.GetOptimalWaitTimeLogNormal(GlobalVariables.workerLogNormalTimeMean, GlobalVariables.workerLogNormalTimeSigma,
                               GlobalVariables.mlaLogNormalTimeMean, GlobalVariables.mlaLogNormalTimeSigma);
                        for (double ml = 0.010; ml < GlobalVariables.tlaWaitTimeSec; ml += .010)
                        {
                            GlobalVariables.mlaWaitTimeSec = ml;
                            //Console.Error.WriteLine(s + " " + tmp);
                            Tuple<double, double> res3 = WaitTimes.WaitTimesExperiment(args, 2000, null, null);
                        }
                        /*
                        //double i1 = 100.0 * (res3.Item2 - res1.Item2) / res1.Item2;
                        //double i2 = 100.0 * (res3.Item2 - res2 .Item2) / res2.Item2;
                        //Console.Error.WriteLine("Numbers " + wT[i] + " " + res1.Item2 + " " + res2.Item2 + " " + res3.Item2);
                        Console.Error.WriteLine("Quality " + t + " " + res3.Item2);
                        //Console.Error.WriteLine("Improvement1 " + s + " " + 100.0 * (res3.Item2 - res1.Item2) / res1.Item2);*/
                        //Console.Error.WriteLine("\n\nImprovement2 " + s + " " + 100.0 * (res3.Item2 - res2.Item2) / res2.Item2 + "\n\n");
                        
                    }
                }
            }
        }
    }
}
