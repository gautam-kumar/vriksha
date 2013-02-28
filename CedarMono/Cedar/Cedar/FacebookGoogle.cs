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
                double[] wT = new double[1] {0.17};
                double[] stdev = new double[3] {0.75, 1.0, 1.25};
                foreach (double s in stdev)
                {
                    //Console.Error.WriteLine("Stdev " + s);
                    GlobalVariables.ReadDcTcpNumbers();
                    GlobalVariables.workerDistType = DistType.Facebook;
                    GlobalVariables.workerLogNormalTimeMean = 4.4;
                    GlobalVariables.workerLogNormalTimeSigma = 1.15;
                    GlobalVariables.mlaDistType = DistType.LogNormal;
                    GlobalVariables.mlaLogNormalTimeMean = 2.94;
                    GlobalVariables.mlaLogNormalTimeSigma = 0.55;
                    GlobalVariables.timeIncrementSec = 0.001;
                    GlobalVariables.NumRequestsToSimulate = 100;

                    double m = GlobalVariables.workerLogNormalTimeMean; double g = GlobalVariables.workerLogNormalTimeSigma;
                    double a = Math.Exp(m + g * g / 2);
                    m = GlobalVariables.mlaLogNormalTimeMean; g = GlobalVariables.mlaLogNormalTimeSigma;
                    double b = Math.Exp(m + g * g / 2);
                    for (int i = 0; i < wT.Length; i++)
                    {
                        GlobalVariables.tlaWaitTimeSec = wT[i];
                        double tmp = 0;
                        tmp = 0.001 * GlobalVariables.GetOptimalWaitTimeLogNormal(GlobalVariables.workerLogNormalTimeMean, GlobalVariables.workerLogNormalTimeSigma,
                               GlobalVariables.mlaLogNormalTimeMean, GlobalVariables.mlaLogNormalTimeSigma);
                        //for (double ml = 0.010; ml < GlobalVariables.tlaWaitTimeSec; ml += .010)
                        //{
                            GlobalVariables.mlaWaitTimeSec = tmp;
                            Tuple<double, double> res3 = WaitTimes.WaitTimesExperiment(args, 2000, null, null);
                        
                    }
                }
            }
        }
    }
}
