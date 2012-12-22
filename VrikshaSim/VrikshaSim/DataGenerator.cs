using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VrikshaSim
{
    public class DataGenerator
    {
        public enum D3DeadlineTag { VeryTight, Tight, Medium, Lax };

        public static double D3TagToDeadline(D3DeadlineTag t)
        {
            switch (t)
            {
                case D3DeadlineTag.VeryTight:
                    return .008;
                case D3DeadlineTag.Tight:
                    return .01;
                case D3DeadlineTag.Medium:
                    return .015;
                case D3DeadlineTag.Lax:
                    return .02;
                default:
                    throw new Exception("unknown d3deadlinetag " + t);
            }
        }

        public static List<Flow> D3GenerateData(int numWorkers, D3DeadlineTag t)
        {
            Randomness rnd = new Randomness(0);

            List<Flow> retval = new List<Flow>();
            double deadlineMean = D3TagToDeadline(t);

            while (numWorkers-- > 0)
            {
                // mimic the distribution in D^3
                Task task = Task.CreateNewTask(0, TaskType.WorkerTask);
                Flow f = Flow.CreateNewFlow(task, 1.0);
                retval.Add(f);
            }
            
            return retval;
        }

    }
}
