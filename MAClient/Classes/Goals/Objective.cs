using Common.Interfaces;
using MAClient.Classes.Entities;
using MAClient.Enumerations;
using System.Collections.Generic;
using System.Linq;

namespace MAClient.Classes.Goals
{
    public class Objective
    {
        private Stack<SubGoal> subgoals;
        private Plan Plan { get; set; }
        private Strategy strategy;

        public SubGoal Current { get; private set; }
        public SubGoal ContingentOn { get; private set; }
        public List<Agent> samaritans;
        public bool HasPlan { get { return Plan != null; } }
        public bool PlanCompleted { get { return this.HasPlan && this.Plan.Completed; } }
        public bool IsComplete { get { return this.subgoals.Count == 0 && Current.completed; } }
        public bool Failed { get { return Current == null || Current.failed; } }

        public Objective(Strategy strategy)
        {
            this.subgoals = new Stack<SubGoal>();
            this.samaritans = new List<Agent>();
            this.strategy = strategy;
        }


        public void AddSubGoal(SubGoal subGoal)
        {
            if (!this.subgoals.Contains(subGoal))
            {
                this.subgoals.Push(subGoal);
            }
        }

        /// this method is meant to check if the current subgoal have completed. If so, advance the objective
        public void UpdateSubgoalStates(Node n)
        {
            if (subgoals.Count > 0)
            {
                this.subgoals.Peek().UpdateState(n);
                if (this.subgoals.Peek().completed)
                {
                    this.Current = subgoals.Pop();
                }
            }
        }

        public void UndoAction(Node n)
        {
            this.Plan.UndoAction(n);
        }
        public bool acceptNextMove()
        {
            if (Plan.Completed)
            {
                this.SolveSubgoal();
                this.Current = subgoals.Pop();
                Plan = null;
                return true;
            }

            return false;
        }

        private void SolveSubgoal()
        {
            SubGoal subgoal = subgoals.Pop();
            subgoal.completed = true;
        }

        public void AddEncounteredObject(IEntity obstacle)
        {
            this.Current.EncounteredObjects.Push(obstacle);
        }

        public Node GetNextMove(Agent agent, Node n)
        {
            if (this.Current is WaitFor)
            {
                if (!((WaitFor)Current).completed)
                {
                    return PerformNoOp(agent, n);
                }
                else if (((WaitFor)Current).dependency.Failed)
                {
                    return ResolveConflict(agent, n);
                }
            }

            if (Current == null || Current.completed)
            {
                if (this.subgoals.Count > 1)
                {
                    this.Current = this.subgoals.Pop();
                    this.CreatePlan(this.strategy, agent.CurrentBeliefs);
                    if (!this.HasPlan)
                    {
                        return ResolveConflict(agent, n);
                    }
                }
                else
                {
                    return PerformNoOp(agent, n);
                }
            }
            return Plan.GetNextAction();
        }

        public Node ResolveConflict(Agent agent, Node n)
        {
            if (this.Current.EncounteredObjects.Count != 0)
            {
                object obstacle = this.Current.EncounteredObjects.Pop();
                List<IEntity> usedFields = this.Plan.ExtractUsedFields();

                if (obstacle != null)
                {
                    if (obstacle is Box)
                    {
                        bool solved = false;
                        Box box = ((Box)obstacle);
                        var agents = n.agentList.Entities.Where(x => x.color == box.color);

                        foreach (Agent samaritan in agents.Where(a => !samaritans.Contains(a)))
                        {
                            Objective objective = this.CreateMoveAway(box, agent, samaritan);
                            if (samaritan.AcceptObjective(objective))
                            {
                                this.AddSubGoal(new WaitFor(objective, samaritan.uid));
                                this.samaritans.Add(samaritan);
                                solved = true;
                                break;
                            }
                        }
                        if (!solved)
                        {
                            return ResolveConflict(agent, n);
                        }
                    }
                    else if (obstacle is Agent)
                    {
                        Agent samaritan = (Agent)obstacle;
                        Objective objective = this.CreateMoveAway(agent, samaritan);
                        if (samaritan.AcceptObjective(objective))
                        {
                            this.AddSubGoal(new WaitFor(objective, samaritan.uid));
                        }
                        else
                        {
                            return ResolveConflict(agent, n);
                        }
                    }
                }
            }
            else
            {
                Current.failed = true;
            }
            return PerformNoOp(agent, n);
        }

        public void CreatePlan(Strategy strategy, Node CurrentBelief)
        {
            CurrentBelief.parent = null;
            strategy.reset();
            strategy.addToFrontier(CurrentBelief);

            while (true)
            {
                if (strategy.frontierIsEmpty())
                {
                    this.Plan = null;
                }

                Node leafNode = strategy.getAndRemoveLeaf();
                if (this.Current.IsGoalState(leafNode))
                {
                    System.Diagnostics.Debug.WriteLine(" - SOLUTION!!!!!!");
                    this.Plan = leafNode.extractPlan();
                }

                strategy.addToExplored(leafNode);
                foreach (Node n in leafNode.getExpandedNodes())
                { // The list of expanded nodes is shuffled randomly; see Node.java.
                    if (!strategy.isExplored(n) && !strategy.inFrontier(n))
                    {
                        strategy.addToFrontier(n);
                    }
                }
            }
        }

        public static Node PerformNoOp(Agent agent, Node currentNode)
        {
            Node n = currentNode.copyNode();
            n.action = new Command(ActionType.NoOp);
            n.agentCol = agent.col;
            n.agentRow = agent.row;
            return n;
        }

        private Objective CreateBoxToGoal(Box box)
        {
            return null;
        }

        public Objective CreateMoveAway(Box box, Agent agent, Agent samaritan)
        {
            Objective objective = new Objective(this.strategy);

            List<IEntity> usedFields = this.Plan.ExtractUsedFields();
            MoveAway moveAgentAway = new MoveAway(new IEntity[] { box, samaritan }, usedFields, agent.uid, samaritan.uid);
            MoveAgentTo moveAgentTo = new MoveAgentTo(box, samaritan.uid);
            WaitFor waitForCompletion = new WaitFor(this, samaritan.uid);
            objective.AddSubGoal(waitForCompletion);
            objective.AddSubGoal(moveAgentAway);
            objective.AddSubGoal(moveAgentTo);
            return objective;
        }

        public Objective CreateMoveAway(Agent agent, Agent samaritan)
        {
            Objective objective = new Objective(this.strategy);
            List<IEntity> usedFields = this.Plan.ExtractUsedFields();
            MoveAway moveAgentAway = new MoveAway(new IEntity[] { samaritan }, usedFields, agent.uid, samaritan.uid);
            WaitFor waitForCompletion = new WaitFor(this, samaritan.uid);
            objective.AddSubGoal(waitForCompletion);
            objective.AddSubGoal(moveAgentAway);
            return objective;
        }


    }
}
