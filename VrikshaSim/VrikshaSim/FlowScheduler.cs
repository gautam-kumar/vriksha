using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;
using System.IO;

namespace VrikshaSim
{

    public abstract class FlowScheduler
    {
        protected double capacityBps;
        public string name;
        public List<Flow> flowsToSchedule;
        public List<Flow> finishedFlows;
        public Machine aggregator;

        public double bytesCount;

        public FlowScheduler (double capacityBps, Machine aggregator)
        {
            this.capacityBps = capacityBps;
            this.flowsToSchedule = new List<Flow>();
            this.finishedFlows = new List<Flow>();
            this.aggregator = aggregator;
            name = "DAS base class";
            bytesCount = 0;
        }
        public abstract void AdvanceTime(double timeToAdvance);

        public void AddFlowToSchedule(Flow f)
        {
            this.flowsToSchedule.Add(f);
        }

        public abstract List<Flow> ScheduleFlows(List<Flow> flows);
    };


    public class InfiniteCapScheduler : FlowScheduler
    {
        public InfiniteCapScheduler(double cap, Machine aggregator)
            : base(cap, aggregator)
        {
            name = "InfiniteCap";
        }

        public override void AdvanceTime(double timeToSimulate)
        {
            int numRunningFlows = flowsToSchedule.Count;
            if (numRunningFlows > 0)
            {
                double fairRate = capacityBps / numRunningFlows;

                foreach (Flow f in flowsToSchedule)
                {
                    double timeToEnd = f.estimateEndTime(fairRate) - GlobalVariables.currentTimeSec;
                    f.AdvanceBy(timeToEnd, fairRate);
                    aggregator.InsertFlow(f);
                    aggregator.AddFlowCompletionTime(f.endSec - f.beginSec);
                }
                finishedFlows.AddRange(flowsToSchedule);
                flowsToSchedule.RemoveAll(flow => flow.IsFinished());
            }
        }

        public override List<Flow> ScheduleFlows(List<Flow> flows)
        {
            List<Flow> unfinishedFlows = flows,
            finishedFlows = new List<Flow>();

            while (unfinishedFlows.Count > 0)
            {
                int numRunningFlows = unfinishedFlows.Count;
                double fairRate = capacityBps / numRunningFlows;

                double firstFinishSec = unfinishedFlows.Min(flow => flow.estimateEndTime(fairRate));

                // advance time to firstFinish
                foreach (Flow f in unfinishedFlows)
                {
                    f.AdvanceBy(firstFinishSec - GlobalVariables.currentTimeSec, fairRate);
                }


                finishedFlows.AddRange(unfinishedFlows.FindAll(flow => flow.IsFinished()));
                unfinishedFlows.RemoveAll(flow => flow.IsFinished());

                GlobalVariables.currentTimeSec = firstFinishSec;

                //Console.WriteLine("{1} # flows rem = {0}",
                //                   unfinishedFlows.Count,
                //                   GlobalVariables.currentTimeSec.ToString("G4"));
            }

            return finishedFlows;
        }

    }

    public class TCPFairScheduler : FlowScheduler
    {
        public TCPFairScheduler(double cap, Machine aggregator)
            : base(cap, aggregator) 
        {
            name = "TCPFair";
        }

        public override void AdvanceTime(double timeToSimulate)
        {
            int numRunningFlows = flowsToSchedule.Count;
            if (numRunningFlows > 0)
            {
                double fairRate = capacityBps / numRunningFlows;


                // advance flows by timetoSimulate
                double bytesTransferred = 0;
                foreach (Flow f in flowsToSchedule)
                {
                    bytesTransferred += f.AdvanceBy(timeToSimulate, fairRate);
                }
                if (GlobalVariables.currentTimeSec >= GlobalVariables.startCountingUtilization && GlobalVariables.currentTimeSec < GlobalVariables.endCountingUtilization)
                {
                    bytesCount += bytesTransferred;
                }

                List<Flow> finishedInThisIteration = flowsToSchedule.FindAll(flow => flow.IsFinished());
                foreach (Flow f in finishedInThisIteration)
                {
                    // Console.WriteLine("Flow {0} finished at {1}", f, GlobalVariables.currentTimeSec);
                    aggregator.InsertFlow(f);
                    aggregator.AddFlowCompletionTime(f.endSec - f.beginSec);
                }
                finishedFlows.AddRange(finishedInThisIteration);
                flowsToSchedule.RemoveAll(flow => flow.IsFinished());
            }
        }

        public override List<Flow> ScheduleFlows(List<Flow> flows)
        {
            List<Flow> unfinishedFlows = flows,
            finishedFlows = new List<Flow>();

            while (unfinishedFlows.Count > 0)
            {
                int numRunningFlows = unfinishedFlows.Count;
                double fairRate = capacityBps / numRunningFlows;

                double firstFinishSec = unfinishedFlows.Min(flow => flow.estimateEndTime(fairRate));

                // advance time to firstFinish
                foreach (Flow f in unfinishedFlows)
                {
                    f.AdvanceBy(firstFinishSec - GlobalVariables.currentTimeSec, fairRate);
                }
               

                finishedFlows.AddRange(unfinishedFlows.FindAll(flow => flow.IsFinished()));
                unfinishedFlows.RemoveAll(flow => flow.IsFinished());

                GlobalVariables.currentTimeSec = firstFinishSec;

                //Console.WriteLine("{1} # flows rem = {0}",
                //                   unfinishedFlows.Count,
                //                   GlobalVariables.currentTimeSec.ToString("G4"));
            }

            return finishedFlows;
        }

    }

    public class EDFNonPreEmptiveScheduler : FlowScheduler
    {
        public List<Flow> missedFlows;
        public EDFNonPreEmptiveScheduler(double cap, Machine aggregator) : base(cap, aggregator) { name = "EDFPreEmptive"; missedFlows = new List<Flow>(); }
        public override void AdvanceTime(double timeToSimulate)
        {
            double earliestDeadline = Double.MaxValue;
            Flow edFlow = null;
            foreach (Flow f in flowsToSchedule)
            {
                if (f.deadlineSec < earliestDeadline)
                {
                    earliestDeadline = f.deadlineSec;
                    edFlow = f;
                }
            }
            if (edFlow != null)
            {
                double timeToEnd = edFlow.estimateEndTime(capacityBps) - GlobalVariables.currentTimeSec;
                double b = edFlow.AdvanceBy(Math.Min(timeToEnd, timeToSimulate), capacityBps);

                if (GlobalVariables.currentTimeSec >= GlobalVariables.startCountingUtilization && GlobalVariables.currentTimeSec < GlobalVariables.endCountingUtilization)
                {
                    bytesCount += b;
                }
                if (edFlow.IsFinished())
                {
                    //Console.WriteLine("Flow {0} finished.", edFlow);
                    finishedFlows.Add(edFlow);
                    flowsToSchedule.Remove(edFlow);
                    aggregator.InsertFlow(edFlow);
                    aggregator.AddFlowCompletionTime(edFlow.endSec - edFlow.beginSec);

                }

                if (timeToSimulate - timeToEnd > 0)
                {
                    AdvanceTime(timeToSimulate - timeToEnd);
                }
            }
        }
        public override List<Flow> ScheduleFlows(List<Flow> flows)
        {
            List<Flow> unfinishedFlows = flows,
                finishedFlows = new List<Flow>();
            List<Flow> missedFlows = new List<Flow>();

            while (unfinishedFlows.Count > 0)
            {
                double earliestDeadline = unfinishedFlows.Min(flow => flow.deadlineSec);
                Flow edFlow = unfinishedFlows.Find(flow => (flow.deadlineSec == earliestDeadline));

                //Console.WriteLine("\t edf flow = {0}", edFlow);

                double endTime = edFlow.estimateEndTime(capacityBps);

                if (edFlow.deadlineSec < GlobalVariables.currentTimeSec)
                {
                    // already missed
                    missedFlows.Add(edFlow);
                }
                else if (edFlow.deadlineSec >= GlobalVariables.currentTimeSec && edFlow.deadlineSec < endTime)
                {
                    // will miss this flow
                    edFlow.AdvanceBy(edFlow.deadlineSec - GlobalVariables.currentTimeSec, capacityBps);
                    missedFlows.Add(edFlow);
                    GlobalVariables.currentTimeSec = edFlow.deadlineSec;
                }
                else
                {
                    edFlow.AdvanceBy(endTime - GlobalVariables.currentTimeSec, capacityBps);
                    Debug.Assert(edFlow.IsFinished());
                    finishedFlows.Add(edFlow);
                    GlobalVariables.currentTimeSec = endTime;
                }
                unfinishedFlows.Remove(edFlow);

                //Console.WriteLine("{1} # flows rem = {0}",
                //   unfinishedFlows.Count,
                //   GlobalVariables.currentTimeSec.ToString("G4")); 
            }

            foreach (Flow f in missedFlows)
            {
                double e = f.estimateEndTime(capacityBps);
                f.AdvanceBy(e - GlobalVariables.currentTimeSec, capacityBps);
                Debug.Assert(f.IsFinished());
                finishedFlows.Add(f);
                GlobalVariables.currentTimeSec = e;
            }

            return finishedFlows;
        }
    };



    public class EDFPreEmptiveScheduler : FlowScheduler
    {
        public List<Flow> missedFlows;
        public EDFPreEmptiveScheduler(double cap, Machine aggregator) : base(cap, aggregator) { name = "EDFPreEmptive"; missedFlows = new List<Flow>(); }
        public override void AdvanceTime(double timeToSimulate)
        {
            double earliestDeadline = Double.MaxValue;
            Flow edFlow = null;
            foreach (Flow f in flowsToSchedule)
            {
                if (f.deadlineSec < earliestDeadline)
                {
                    earliestDeadline = f.deadlineSec;
                    edFlow = f;
                }
            }
            if (edFlow != null)
            {
                double timeToEnd = edFlow.estimateEndTime(capacityBps) - GlobalVariables.currentTimeSec;
                double b = edFlow.AdvanceBy(Math.Min(timeToEnd, timeToSimulate), capacityBps);
                if (GlobalVariables.currentTimeSec >= GlobalVariables.startCountingUtilization && GlobalVariables.currentTimeSec < GlobalVariables.endCountingUtilization)
                {
                    bytesCount += b;
                }
                
                if (edFlow.IsFinished())
                {
                    //Console.WriteLine("Flow {0} finished.", edFlow);
                    finishedFlows.Add(edFlow);
                    flowsToSchedule.Remove(edFlow);
                    aggregator.InsertFlow(edFlow);
                    aggregator.AddFlowCompletionTime(edFlow.endSec - edFlow.beginSec);

                }
                else if (edFlow.deadlineSec < GlobalVariables.currentTimeSec + timeToSimulate)
                {
                    missedFlows.Add(edFlow);
                    flowsToSchedule.Remove(edFlow);
                    GlobalVariables.numFlowsMissed += 1;
                    //Console.WriteLine("Missed a flow {0}\n", edFlow);
                }

                if (timeToSimulate - timeToEnd > 0)
                {
                    AdvanceTime(timeToSimulate - timeToEnd);
                }
            }
        }
        public override List<Flow> ScheduleFlows(List<Flow> flows)
        {
            List<Flow> unfinishedFlows = flows,
                finishedFlows = new List<Flow>();
            List<Flow> missedFlows = new List<Flow>();

            while (unfinishedFlows.Count > 0)
            {
                double earliestDeadline = unfinishedFlows.Min(flow => flow.deadlineSec);
                Flow edFlow = unfinishedFlows.Find(flow => (flow.deadlineSec == earliestDeadline));

                //Console.WriteLine("\t edf flow = {0}", edFlow);

                double endTime = edFlow.estimateEndTime(capacityBps);

                if (edFlow.deadlineSec < GlobalVariables.currentTimeSec)
                {
                    // already missed
                    missedFlows.Add(edFlow);
                }
                else if (edFlow.deadlineSec >= GlobalVariables.currentTimeSec && edFlow.deadlineSec < endTime)
                {
                    // will miss this flow
                    edFlow.AdvanceBy(edFlow.deadlineSec - GlobalVariables.currentTimeSec, capacityBps);
                    missedFlows.Add(edFlow);
                    GlobalVariables.currentTimeSec = edFlow.deadlineSec;
                }
                else
                {
                    edFlow.AdvanceBy(endTime - GlobalVariables.currentTimeSec, capacityBps);
                    Debug.Assert(edFlow.IsFinished());
                    finishedFlows.Add(edFlow);
                    GlobalVariables.currentTimeSec = endTime;
                }
                unfinishedFlows.Remove(edFlow);

                //Console.WriteLine("{1} # flows rem = {0}",
                //   unfinishedFlows.Count,
                //   GlobalVariables.currentTimeSec.ToString("G4"));
            }

            foreach (Flow f in missedFlows)
            {
                double e = f.estimateEndTime(capacityBps);
                f.AdvanceBy(e - GlobalVariables.currentTimeSec, capacityBps);
                Debug.Assert(f.IsFinished());
                finishedFlows.Add(f);
                GlobalVariables.currentTimeSec = e;
            }

            return finishedFlows;
        }
    };


    public class SJFPreEmptiveScheduler : FlowScheduler
    {
        public SJFPreEmptiveScheduler(double cap, Machine aggregator) : base(cap, aggregator) { name = "SJFPreEmptive"; }
        public override void AdvanceTime(double timeToSimulate)
        {
            double minFlowSize = Double.MaxValue;
            Flow sjFlow = null;
            foreach (Flow f in flowsToSchedule)
            {
                if (f.remainingBytes < minFlowSize)
                {
                    minFlowSize = f.remainingBytes;
                    sjFlow = f;
                }
            }
            if (sjFlow != null)
            {
                double timeToEnd = sjFlow.estimateEndTime(capacityBps) - GlobalVariables.currentTimeSec;
                double b = sjFlow.AdvanceBy(Math.Min(timeToEnd, timeToSimulate), capacityBps);

                if (GlobalVariables.currentTimeSec >= GlobalVariables.startCountingUtilization && GlobalVariables.currentTimeSec < GlobalVariables.endCountingUtilization)
                {
                    bytesCount += b;
                }
                if (sjFlow.IsFinished())
                {
                    //Console.WriteLine("Flow {0} finished.", edFlow);
                    finishedFlows.Add(sjFlow);
                    flowsToSchedule.Remove(sjFlow);
                    aggregator.InsertFlow(sjFlow);
                    aggregator.AddFlowCompletionTime(sjFlow.endSec - sjFlow.beginSec);
                }
                
                if (timeToSimulate - timeToEnd > 0)
                {
                    AdvanceTime(timeToSimulate - timeToEnd);
                }
            }
        }

        // Should not be used
        public override List<Flow> ScheduleFlows(List<Flow> flows)
        {
            List<Flow> unfinishedFlows = flows,
                finishedFlows = new List<Flow>();
            List<Flow> missedFlows = new List<Flow>();

            while (unfinishedFlows.Count > 0)
            {
                double earliestDeadline = unfinishedFlows.Min(flow => flow.deadlineSec);
                Flow edFlow = unfinishedFlows.Find(flow => (flow.deadlineSec == earliestDeadline));

                //Console.WriteLine("\t edf flow = {0}", edFlow);

                double endTime = edFlow.estimateEndTime(capacityBps);

                if (edFlow.deadlineSec < GlobalVariables.currentTimeSec)
                {
                    // already missed
                    missedFlows.Add(edFlow);
                }
                else if (edFlow.deadlineSec >= GlobalVariables.currentTimeSec && edFlow.deadlineSec < endTime)
                {
                    // will miss this flow
                    edFlow.AdvanceBy(edFlow.deadlineSec - GlobalVariables.currentTimeSec, capacityBps);
                    missedFlows.Add(edFlow);
                    GlobalVariables.currentTimeSec = edFlow.deadlineSec;
                }
                else
                {
                    edFlow.AdvanceBy(endTime - GlobalVariables.currentTimeSec, capacityBps);
                    Debug.Assert(edFlow.IsFinished());
                    finishedFlows.Add(edFlow);
                    GlobalVariables.currentTimeSec = endTime;
                }
                unfinishedFlows.Remove(edFlow);

                //Console.WriteLine("{1} # flows rem = {0}",
                //   unfinishedFlows.Count,
                //   GlobalVariables.currentTimeSec.ToString("G4"));
            }

            foreach (Flow f in missedFlows)
            {
                double e = f.estimateEndTime(capacityBps);
                f.AdvanceBy(e - GlobalVariables.currentTimeSec, capacityBps);
                Debug.Assert(f.IsFinished());
                finishedFlows.Add(f);
                GlobalVariables.currentTimeSec = e;
            }

            return finishedFlows;
        }
    };



    public class VrikshaNonKillingScheduler : FlowScheduler
    {
        public int level;
        public VrikshaNonKillingScheduler(double cap, Machine aggregator, int level) : base(cap, aggregator) 
        { 
            name = "VrikshaNonPreEmptive";
            this.level = level;
        }
        public override void AdvanceTime(double timeToSimulate)
        {
            double minSlack = Double.MaxValue;
            Flow msFlow = null;
            foreach (Flow f in flowsToSchedule)
            {
                
                Request r = GlobalVariables.requests[f.task.requestId];
                double slack = r.beginSec - GlobalVariables.currentTimeSec;
                //double slack = f.sizeBytes;
                if (level == 0) // i.e Worker -> MLA
                {
                    slack -= GlobalVariables.mlaComputationTimeMeanSec;
                }
                if (slack < minSlack)
                {
                    minSlack = slack;
                    msFlow = f;
                }
            }
            if (msFlow != null)
            {
                double timeToEnd = msFlow.estimateEndTime(capacityBps) - GlobalVariables.currentTimeSec;
                double b = msFlow.AdvanceBy(Math.Min(timeToEnd, timeToSimulate), capacityBps);
                if (GlobalVariables.currentTimeSec >= GlobalVariables.startCountingUtilization && GlobalVariables.currentTimeSec < GlobalVariables.endCountingUtilization)
                {
                    bytesCount += b;
                }
                if (msFlow.IsFinished())
                {
                    //Console.WriteLine("Flow {0} finished.", edFlow);
                    finishedFlows.Add(msFlow);
                    flowsToSchedule.Remove(msFlow);
                    aggregator.InsertFlow(msFlow);
                    aggregator.AddFlowCompletionTime(msFlow.endSec - msFlow.beginSec);

                }
                if (timeToSimulate - timeToEnd > 0)
                {
                    AdvanceTime(timeToSimulate - timeToEnd);
                }
            }
        }
        
        // TODO: Should not be used, doesn't make any sense
        public override List<Flow> ScheduleFlows(List<Flow> flows)
        {
            List<Flow> unfinishedFlows = flows,
                finishedFlows = new List<Flow>();
            List<Flow> missedFlows = new List<Flow>();

            while (unfinishedFlows.Count > 0)
            {
                double earliestDeadline = unfinishedFlows.Min(flow => flow.deadlineSec);
                Flow edFlow = unfinishedFlows.Find(flow => (flow.deadlineSec == earliestDeadline));

                //Console.WriteLine("\t edf flow = {0}", edFlow);

                double endTime = edFlow.estimateEndTime(capacityBps);

                if (edFlow.deadlineSec < GlobalVariables.currentTimeSec)
                {
                    // already missed
                    missedFlows.Add(edFlow);
                }
                else if (edFlow.deadlineSec >= GlobalVariables.currentTimeSec && edFlow.deadlineSec < endTime)
                {
                    // will miss this flow
                    edFlow.AdvanceBy(edFlow.deadlineSec - GlobalVariables.currentTimeSec, capacityBps);
                    missedFlows.Add(edFlow);
                    GlobalVariables.currentTimeSec = edFlow.deadlineSec;
                }
                else
                {
                    edFlow.AdvanceBy(endTime - GlobalVariables.currentTimeSec, capacityBps);
                    Debug.Assert(edFlow.IsFinished());
                    finishedFlows.Add(edFlow);
                    GlobalVariables.currentTimeSec = endTime;
                }
                unfinishedFlows.Remove(edFlow);

                //Console.WriteLine("{1} # flows rem = {0}",
                //   unfinishedFlows.Count,
                //   GlobalVariables.currentTimeSec.ToString("G4"));
            }

            foreach (Flow f in missedFlows)
            {
                double e = f.estimateEndTime(capacityBps);
                f.AdvanceBy(e - GlobalVariables.currentTimeSec, capacityBps);
                Debug.Assert(f.IsFinished());
                finishedFlows.Add(f);
                GlobalVariables.currentTimeSec = e;
            }

            return finishedFlows;
        }
    };

    
    public class D3Scheduler : FlowScheduler
    {
        bool skipMissedDeadlines;

        // TODO: Need to implement skipMissedDeadlines
        public D3Scheduler(double capacityBps, Machine aggregator, bool skipMissedDeadlines)
            : base(capacityBps, aggregator)
        {
            this.skipMissedDeadlines = skipMissedDeadlines;
            name = "D3Scheduler_" + skipMissedDeadlines;
        }

        int routerNumFlows = 0;
        Dictionary<int, double> currentRateAllocations = new Dictionary<int, double>();
        Dictionary<int, double> oldRateDemand = new Dictionary<int, double>();
        Dictionary<int, double> actualRateAllocations = new Dictionary<int, double>();
        double totalAllocatedBps = 0;
        double totalDemandBps = 0;


        public override void AdvanceTime(double timeToAdvance)
        {

            double remainingCapacityBps;

            foreach (Flow f in flowsToSchedule)
            {
                double demandBps = f.EstimateRequiredRateBps();

                double oldAllocationBps =
                    currentRateAllocations.ContainsKey(f.flowId) ? currentRateAllocations[f.flowId] : 0;
                double oldDemandBps =
                    oldRateDemand.ContainsKey(f.flowId) ? oldRateDemand[f.flowId] : 0;

                routerNumFlows = flowsToSchedule.Count;

                totalAllocatedBps -= oldAllocationBps;
                totalDemandBps += demandBps - oldDemandBps;
                remainingCapacityBps = capacityBps - totalAllocatedBps;

                double fairShareBps = (capacityBps - totalDemandBps) / routerNumFlows;

                double allocationBps;
                if (remainingCapacityBps > demandBps)
                {
                    allocationBps = demandBps + fairShareBps;
                }
                else
                {
                    allocationBps = remainingCapacityBps;
                }

                totalAllocatedBps += allocationBps;

                if (!oldRateDemand.ContainsKey(f.flowId))
                    oldRateDemand.Add(f.flowId, 0);
                if (!currentRateAllocations.ContainsKey(f.flowId))
                    currentRateAllocations.Add(f.flowId, 0);

                currentRateAllocations[f.flowId] = allocationBps;
                oldRateDemand[f.flowId] = demandBps;
            }

            double overAllocFactor = totalAllocatedBps / capacityBps;
            foreach (Flow f in flowsToSchedule)
            {
                double actualRateBps = currentRateAllocations[f.flowId];

                if (overAllocFactor > 1.0) actualRateBps /= overAllocFactor;
                if (!actualRateAllocations.ContainsKey(f.flowId))
                    actualRateAllocations.Add(f.flowId, 0);

                actualRateAllocations[f.flowId] = actualRateBps;
                f.AdvanceBy(timeToAdvance, actualRateBps);
            }

            List<Flow> finishedInThisIteration = flowsToSchedule.FindAll(flow => flow.IsFinished());
            foreach (Flow f in finishedInThisIteration)
            {
                Console.WriteLine("Flow {0} finished at {1}", f, GlobalVariables.currentTimeSec);
                aggregator.InsertFlow(f);
                totalAllocatedBps -= currentRateAllocations[f.flowId];
                totalDemandBps -= oldRateDemand[f.flowId];
                currentRateAllocations.Remove(f.flowId);
                oldRateDemand.Remove(f.flowId);
                actualRateAllocations.Remove(f.flowId);
            }
            finishedFlows.AddRange(finishedInThisIteration);
            flowsToSchedule.RemoveAll(flow => flow.IsFinished());
            //flowsToSchedule.RemoveAll(flow => (flow.deadlineSec < GlobalVariables.currentTimeSec + timeToAdvance));
        }

        public override List<Flow> ScheduleFlows(List<Flow> flows)
        {
            flowsToSchedule = flows;
            int iteration = 0;
            while (flowsToSchedule.Count > 0)
            {
                AdvanceTime(GlobalVariables.timeIncrementSec);
                iteration += 1;
                Console.WriteLine("Iteration {0} #Flows: {1} #Finished: {2}", iteration, flowsToSchedule.Count, finishedFlows.Count);
                GlobalVariables.currentTimeSec += GlobalVariables.timeIncrementSec;
            }
            return finishedFlows;
        }
    }
}

