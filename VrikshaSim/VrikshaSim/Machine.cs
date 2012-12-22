using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VrikshaSim
{
    
    /* Abstract class Machine */
    public abstract class Machine
    {
        public List<Task> tasksToSchedule;
        public Task[] currentTasks = null;
        public int numQueues;

        public Machine(int numQueues)
        {
            this.numQueues = numQueues;
            tasksToSchedule = new List<Task>();
            currentTasks = new Task[numQueues];
        }

        public void InsertTask(Task t)
        {
            tasksToSchedule.Add(t);
        }

        public void ScheduleNewTask(int queue)
        {
            currentTasks[queue] = tasksToSchedule[0];
            currentTasks[queue].progressSec = 0;
            tasksToSchedule.RemoveAt(0); // TODO: Verify if this is ok
        }

        public abstract void AdvanceTime(double timeToSimulate);
        public abstract void InsertFlow(Flow f);
        public abstract void AddFlowCompletionTime(double fTime); 
    }

    public class Worker : Machine
    {
        public FlowScheduler scheduler;

        public Worker(int numQueues, FlowScheduler scheduler) : base(numQueues)
        {
            // Denotes where to insert a flow to
            this.scheduler = scheduler;
        }

        public override void InsertFlow(Flow f)
        {
        }

        public override void AdvanceTime(double timeToSimulate) 
        {
            for (int i = 0; i < numQueues; i++)
            {
                if (currentTasks[i] == null && tasksToSchedule.Count > 0)
                {
                    ScheduleNewTask(i);
                }
                if (currentTasks[i] != null)
                {
                    currentTasks[i].progressSec += timeToSimulate;
                    if (currentTasks[i].progressSec >= currentTasks[i].processingTimeSec)
                    {
                        // A Task completed; Add a new flow to the flow scheduler
                        scheduler.AddFlowToSchedule(Flow.CreateNewFlow(currentTasks[i], 1.0)); // TODO: Fix
                        currentTasks[i] = null;
                    }
                }
            }
        }
        public override void AddFlowCompletionTime(double fTime)
        {
        }
    }


    public class MLA : Machine
    {
        public FlowScheduler scheduler;
        public Dictionary<int, int> numFlowsPerRequest;
        public Dictionary<int, double> firstFlowArrivalTimePerRequest;
        public Dictionary<int, bool> isRequestProcessed;
        public Dictionary<int, double> informationContentPerRequest;

        public MLA(int numQueues, FlowScheduler scheduler) : base(numQueues)
        {
            this.scheduler = scheduler;
            numFlowsPerRequest = new Dictionary<int, int>();
            firstFlowArrivalTimePerRequest = new Dictionary<int, double>();
            isRequestProcessed = new Dictionary<int, bool>();
            informationContentPerRequest = new Dictionary<int, double>();
        }

        public override void InsertFlow(Flow f)
        {
            //Console.WriteLine("Adding f");
            int requestId = f.task.requestId;
            // If this is a flow coming in late, ignore
            if (isRequestProcessed.ContainsKey(requestId))
            {
                return;
            }
            if (numFlowsPerRequest.ContainsKey(requestId))
            {
                numFlowsPerRequest[requestId] += 1;
                informationContentPerRequest[requestId] += f.informationContent;
                // Console.Error.WriteLine("{0} New Flow: {1}", requestId, numFlowsPerRequest[requestId]);
            }
            else
            {
                numFlowsPerRequest.Add(requestId, 1);
                firstFlowArrivalTimePerRequest.Add(requestId, GlobalVariables.currentTimeSec);
                informationContentPerRequest.Add(requestId, f.informationContent);
            }
        }

        public override void AdvanceTime(double timeToSimulate)
        {
            List<int> toRemove = new List<int>();
            // First check if some new task needs to be added
            foreach (KeyValuePair<int, double> req in firstFlowArrivalTimePerRequest)
            {
                // If time expired, send partial results, or if all flows received
                if ((GlobalVariables.currentTimeSec >= req.Value + GlobalVariables.mlaWaitTimeSec)
                    || (numFlowsPerRequest[req.Key] == GlobalVariables.NumWorkerPerMla))
                {
                    // Console.Error.WriteLine("{0} MLA Inserting a task", req.Key); 
                    GlobalVariables.requests[req.Key].numFlowsCompleted += numFlowsPerRequest[req.Key];
                    InsertTask(Task.CreateNewTask(req.Key, TaskType.MlaTask));
                    toRemove.Add(req.Key); ;
                    isRequestProcessed[req.Key] = true;
                }
            }
            foreach (int i in toRemove)
            {
                numFlowsPerRequest.Remove(i);
                firstFlowArrivalTimePerRequest.Remove(i);
            }

            for (int i = 0; i < numQueues; i++)
            {
                if (currentTasks[i] == null && tasksToSchedule.Count > 0)
                {
                    ScheduleNewTask(i);
                }
                if (currentTasks[i] != null)
                {
                    currentTasks[i].progressSec += timeToSimulate;
                    if (currentTasks[i].progressSec >= currentTasks[i].processingTimeSec)
                    {
                        //Console.Error.WriteLine("\nAdding a flow to the TopScheduler.\n");
                        // A Task completed; Add a new flow to the flow scheduler
                        scheduler.AddFlowToSchedule(Flow.CreateNewFlow(currentTasks[i], 
                            informationContentPerRequest[currentTasks[i].requestId] / GlobalVariables.NumWorkerPerMla));
                        currentTasks[i] = null;
                    }
                }
            }
        
        }

        public override void AddFlowCompletionTime(double fTime)
        {
            GlobalVariables.flowCompletionTimesMla.Add(fTime);
        }
    }


    class TLA : Machine
    {
        public Dictionary<int, int> numFlowsPerRequest;
        public List<Request> completedRequests;
        public Dictionary<int, double> firstFlowArrivalTimePerRequest;
        public Dictionary<int, bool> isRequestProcessed;
        public Dictionary<int, double> informationContentPerRequest;
        
        public TLA() : base (0)
        {
            tasksToSchedule = new List<Task>();
            numFlowsPerRequest = new Dictionary<int, int>();
            completedRequests = new List<Request>();
            firstFlowArrivalTimePerRequest = new Dictionary<int, double>();
            isRequestProcessed = new Dictionary<int, bool>();
            informationContentPerRequest = new Dictionary<int, double>();
        }

        public override void InsertFlow(Flow f)
        {
            
            int requestId = f.task.requestId;
            // Flow coming in late
            
            if (isRequestProcessed.ContainsKey(requestId))
            {
                return;
            }

            if (numFlowsPerRequest.ContainsKey(requestId))
            {
                numFlowsPerRequest[requestId] += 1;
                informationContentPerRequest[requestId] += f.informationContent; ;
            }
            else
            {
                // Console.Error.WriteLine("{0} TLA inserted flow {1}", f.requestId, isRequestProcessed.ContainsKey(requestId));
                numFlowsPerRequest.Add(requestId, 1);
                firstFlowArrivalTimePerRequest.Add(requestId, GlobalVariables.currentTimeSec);
                informationContentPerRequest.Add(requestId, f.informationContent);
            }
        }

        public override void AdvanceTime(double timeToSimulate)
        {
            // First check if some new task needs to be added
            List<int> toRemove = new List<int>();
            foreach (KeyValuePair<int, double> req in firstFlowArrivalTimePerRequest)
            {
                // If time expired, send partial results, or if all flows received
                if ((GlobalVariables.currentTimeSec >= req.Value + GlobalVariables.tlaWaitTimeSec)
                    || (numFlowsPerRequest[req.Key] == GlobalVariables.NumMlaPerTla))
                {
                    GlobalVariables.requests[req.Key].numFlowsCompleted += numFlowsPerRequest[req.Key];
                    InsertTask(Task.CreateNewTask(req.Key, TaskType.TlaTask));
                    toRemove.Add(req.Key);
                    isRequestProcessed[req.Key] = true;
                }
            }
            foreach (int i in toRemove)
            {
                numFlowsPerRequest.Remove(i);
                firstFlowArrivalTimePerRequest.Remove(i);
            }


            if (tasksToSchedule.Count > 0)
            {
                int requestId = tasksToSchedule[0].requestId;
                Request r = GlobalVariables.requests[requestId];
                r.informationContent = informationContentPerRequest[r.requestId] / GlobalVariables.NumMlaPerTla;
                //Console.Error.WriteLine("Finished {0} at {1}, TimeTaken: {2}, FirstTime: {3}, SecondTime: {4}, NumFlowsCompleted: {5}, InformationContent: {6}", requestId, 
                //    GlobalVariables.currentTimeSec, GlobalVariables.currentTimeSec - r.beginSec, 
                //    r.firstComputationSec, r.secondComputationSec, 
                //    r.numFlowsCompleted, r.informationContent);
                r.endSec = GlobalVariables.currentTimeSec;
                completedRequests.Add(GlobalVariables.requests[requestId]);
                tasksToSchedule.RemoveAt(0);
            }
        }

        public override void AddFlowCompletionTime(double fTime)
        {
            GlobalVariables.flowCompletionTimesTla.Add(fTime);
        }
    }

}
