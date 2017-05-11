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
        public bool Completed { get; private set; }
        public SubGoal Current { get; private set; }
        public Plan Plan { get; private set; }

        public SubGoal ContingentOn { get; private set; }
        public List<Agent> samaritans;

        private Strategy strategy;

        public Objective(Strategy strategy)
        {
            this.subgoals = new Stack<SubGoal>();
            this.samaritans = new List<Agent>();
            this.Completed = false;
            this.strategy = strategy;
        }

        public bool IsComplete { get { return this.subgoals.Count == 0; } }

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

        public Node GetNextMove(Agent agent, Node n)
        {
            if (this.Current is WaitFor)
            {
                if (!((WaitFor)Current).completed)
                {
                    return this.PerformNoOp(agent, n);
                }
                else if (((WaitFor)Current).dependency.failed)
                {
                    ResolveConflict(agent, n);
                    return this.PerformNoOp(agent, n);
                }
                else if (this.subgoals.Count > 1)
                {
                    this.Current = this.subgoals.Pop();
                    this.Plan = this.CreatePlan(this.strategy, agent.CurrentBeliefs);
                }
                else
                {
                    this.Completed = true;
                    return null;
                }
            }
            return Plan.GetNextAction();
        }

        public Node ResolveConflict(Agent agent, Node n)
        {
            if (agent.encounteredObjects.Count != 0)
            {
                object obstacle = agent.encounteredObjects.Pop();
                List<IEntity> usedFields = agent.plan.ExtractUsedFields();

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
                                agent.subgoals.Push(new WaitFor(objective.ContingentOn, samaritan.uid));
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
                            agent.subgoals.Push(new WaitFor(objective.ContingentOn, samaritan.uid));
                        }
                        else
                        {
                            return ResolveConflict(agent, n);
                        }
                    }

                }
            }
            return this.PerformNoOp(agent, n);
        }

        public Plan CreatePlan(Strategy strategy, Node CurrentBelief)
        {
            CurrentBelief.parent = null;
            strategy.reset();
            strategy.addToFrontier(CurrentBelief);

            while (true)
            {
                if (strategy.frontierIsEmpty())
                {
                    return null;
                }

                Node leafNode = strategy.getAndRemoveLeaf();
                if (this.Current.IsGoalState(leafNode))
                {
                    System.Diagnostics.Debug.WriteLine(" - SOLUTION!!!!!!");
                    return leafNode.extractPlan();
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
        public Node PerformNoOp(Agent agent, Node currentNode)
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

        private void AddContingentOn(SubGoal subgoal)
        {
            this.ContingentOn = subgoal;
            this.AddSubGoal(subgoal);
        }

        private Objective CreateMoveAway(Box box, Agent agent, Agent samaritan)
        {
            Objective objective = new Objective(this.strategy);

            List<IEntity> usedFields = agent.plan.ExtractUsedFields();
            MoveAway moveAgentAway = new MoveAway(new IEntity[] { box, samaritan }, usedFields, agent.uid, samaritan.uid);
            MoveAgentTo moveAgentTo = new MoveAgentTo(box, samaritan.uid);
            WaitFor waitForCompletion = new WaitFor(agent.subgoals.Peek(), samaritan.uid);
            objective.AddSubGoal(waitForCompletion);
            objective.AddContingentOn(moveAgentAway);
            objective.AddSubGoal(moveAgentTo);
            return objective;
        }

        private Objective CreateMoveAway(Agent agent, Agent samaritan)
        {
            Objective objective = new Objective(this.strategy);
            List<IEntity> usedFields = agent.plan.ExtractUsedFields();
            MoveAway moveAgentAway = new MoveAway(new IEntity[] { samaritan }, usedFields, agent.uid, samaritan.uid);
            WaitFor waitForCompletion = new WaitFor(agent.subgoals.Peek(), samaritan.uid);
            objective.AddSubGoal(waitForCompletion);
            objective.AddSubGoal(moveAgentAway);
            objective.AddContingentOn(moveAgentAway);
            return objective;
        }


    }
}
