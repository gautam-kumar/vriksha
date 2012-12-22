using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;
using System.IO;

namespace VrikshaSim
{
    public class Flow
    {
        static int nextFlowId = 0;

        public Task task;
        public double sizeBytes;  
        public double deadlineSec;

        public int flowId;
        public double informationContent; // between 0 and 1 relative to maximum
        // can change as flow is executed
        public double beginSec,
               endSec;
        public double remainingBytes;


        private Flow(Task task, double sizeBytes, double deadlineSec, double informationContent)
        {
            this.beginSec = GlobalVariables.currentTimeSec;
            this.task = task;
            this.sizeBytes = sizeBytes;
            this.remainingBytes = sizeBytes;
            this.deadlineSec = deadlineSec;
            this.informationContent = informationContent;
            flowId = nextFlowId++;
        }

        // Estimate the end time of the flow given a rate.
        public double estimateEndTime(double rateBps)
        {
            Debug.Assert(rateBps > 0);
            double estimatedEndTime = GlobalVariables.currentTimeSec + (remainingBytes * 8.0 / rateBps);
            return estimatedEndTime;
        }


        // return value indicates successful advance
        public double AdvanceBy(double intervalSec, double rateBps)
        {
            double temp = remainingBytes;
            double decrement = intervalSec * rateBps / 8.0;
            remainingBytes -= decrement;

            if (remainingBytes < 1)
            { // this is to protect against FPU round off problems
                remainingBytes = 0;
                endSec = GlobalVariables.currentTimeSec + intervalSec;
                return temp;
            }

            Debug.Assert(remainingBytes >= 0);
            return decrement;
        }

        public bool IsFinished()
        {
            if (remainingBytes < .001)
            {
                return true;
            }
            return false;
        }

        public override string ToString()
        {
            return "F " + task.requestId + ": "
                         + "b=" + (sizeBytes / 1000.0).ToString("G4") + "kB, D="
                         + (deadlineSec * 1000).ToString("G5") + "ms; b/e="
                         + (beginSec * 1000).ToString("G4") + "/"
                         + (endSec * 1000).ToString("G4")
                         + ", remB=" + (remainingBytes / 1000.0).ToString("G4") + "kB"
                         + ", prog=" + ((1 - remainingBytes / sizeBytes) * 100).ToString("G3") + "%"
                         ;
        }

        public bool DeadlineMet()
        {
            return endSec <= deadlineSec;
        }

        public double EstimateRequiredRateBps()
        {
            return remainingBytes * 8.0 / (deadlineSec - GlobalVariables.currentTimeSec);
        }

        public static Flow CreateNewFlow(Task task, double ic)
        {
            double deadline = GlobalVariables.workerMlaFlowDeadlineSec;
            switch (task.taskType)
            {
                case TaskType.WorkerTask:
                    deadline = GlobalVariables.workerMlaFlowDeadlineSec;
                    break;
                case TaskType.MlaTask:
                    deadline = GlobalVariables.mlaTlaFlowDeadlineSec;
                    break;
                default:
                    break;
            }

            Flow f = new Flow(task,
                            GlobalVariables.rnd.pickRandomDouble(GlobalVariables.flowSizeMinBytes, GlobalVariables.flowSizeMaxBytes), // Flow Size
                // TODO: Deadline
                // GlobalVariables.currentTimeSec + (0.002 + GlobalVariables.rnd.GetExponentialSample(GlobalVariables.firstDeadline)),
                            GlobalVariables.currentTimeSec + deadline,
                            ic);
            return f;
        }
                            
    }
}
