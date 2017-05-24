using Common.Classes;
using Common.Interfaces;
using MAClient.Classes.Goals;
using MAClient.Enumerations;
using System.Collections.Generic;
using System.Linq;

namespace MAClient.Classes.Entities
{
    public class Agent : IEntity
    {
        public int col { get; set; }
        public int row { get; set; }
        public int uid { get; set; }
        public string color;
        private Stack<SubGoal> subgoals;
        public Node CurrentBeliefs;
        public Strategy strategy;
        public Plan plan;
        private Stack<IEntity> encounteredObjects;
        public SubGoal CurrentSubgoal { get { return this.subgoals.Count > 0 ? this.subgoals.Peek() : null; } }
        public bool HasPlan { get { return this.plan != null; } }
        public bool HasSubGoals { get { return this.subgoals != null && this.subgoals.Count != 0; } }

        public int actionIndex = 0;

        public Agent(int x, int y, int id, string color)
        {
            this.col = x;
            this.row = y;
            this.uid = id;
            this.color = color;
            subgoals = new Stack<SubGoal>();
            encounteredObjects = new Stack<IEntity>();
        }

        public void ProcessAgentAction(ref Node CurrentNode)
        {
            if (this.IsWaiting())
            {
                // agent is done with subgoals, perform noOp
                this.PerformNoOp(ref CurrentNode);
            }
            else
            {
                // get agents next move
                Node nextMove = this.GetNextMove(ref CurrentNode);
                

                // convert the node to a command
                Command nextAction = nextMove.action;
                // validate that the command is legal
                IEntity obstacle = CurrentNode.ValidateAction(nextAction, this.col, this.row);
                if (obstacle == null)
                {
                    // succesfull move
                    this.AcceptNextMove();
                    CurrentNode = CurrentNode.ChildNode();
                    CurrentNode.updateNode(nextMove, this.col, this.row);
                }
                else
                {
                    // if not, then update agents beliefs, and replan a plan for the current sub goal
                    this.Backtrack();
                    if (obstacle is Box)
                    {
                        // opdaterer kun en box position men ikke en players hvis den blive "handlet". Kan ikke skelne imellem en box i bevægelse og en stationær
                        this.UpdateCurrentBelief(obstacle, CurrentNode.boxList, this.CurrentBeliefs.boxList);
                        this.UpdateCurrentBelief(null, CurrentNode.agentList, this.CurrentBeliefs.agentList);
                        this.encounteredObjects.Push(obstacle);
                        this.TryConflictResolve(ref CurrentNode);
                    }
                    else if (obstacle is Agent)
                    {
                        Agent otherAgent = (Agent)obstacle;
                        Agent perceivedAgent = otherAgent.CurrentBeliefs.agentList[this.uid];
                        if (perceivedAgent != null && perceivedAgent.col == this.col && perceivedAgent.row == this.row)
                        {
                            this.PerformNoOp(ref CurrentNode);
                        }
                        else
                        {
                            this.UpdateCurrentBelief(obstacle, CurrentNode.agentList, this.CurrentBeliefs.agentList);
                            //this.UpdateCurrentBelief(null, CurrentNode.boxList, this.CurrentBeliefs.boxList);
                            this.encounteredObjects.Push(obstacle);
                            this.TryConflictResolve(ref CurrentNode);
                        }
                    }
                }
            }
        }
        public void AddSubGoal(SubGoal subgoal)
        {
            this.subgoals?.Push(subgoal);
            subgoal.owner = this.uid;
        }

        private Node GetNextMove(ref Node currentNode)
        {
            if (!this.HasPlan || this.plan.Completed)
            {
                if (!this.HasSubGoals)
                {
                    SearchClient.AssignGoal(this, currentNode);
                }
                if (this.HasSubGoals)
                {
                    this.plan = this.CreatePlan();
                    while (!this.HasPlan || this.plan.Completed)
                    {
                        if (!this.HasPlan)
                        {
                            this.CurrentBeliefs = this.CreateNoOp(ref currentNode);
                            this.ResetBeliefs();
                            return this.CurrentBeliefs;
                        }
                        if (this.plan.Completed)
                        {
                            this.SolveSubgoal();
                            if (this.HasSubGoals)
                            {
                                this.plan = this.CreatePlan();
                            }
                            else
                            {
                                this.CurrentBeliefs = this.CreateNoOp(ref currentNode);
                                this.ResetBeliefs();
                                return this.CurrentBeliefs;
                            }
                        }
                    }
                }
                else
                {
                    this.CurrentBeliefs = this.CreateNoOp(ref currentNode);
                    this.ResetBeliefs();
                    return this.CurrentBeliefs;
                }
            }

            // pass on the remaining plan to agent in the next state
            this.CurrentBeliefs = this.plan.GetNextAction();
            return this.CurrentBeliefs;
        }
        private void AcceptNextMove()
        {
            if (this.HasPlan && this.plan.Completed)
            {
                this.SolveSubgoal();
                this.ResetBeliefs();
                this.plan = null;
            }
        }
        private bool IsWaiting()
        {
            if (this.subgoals.Count > 0)
            {
                SubGoal currentSubgoal = this.subgoals.Peek();
                if (currentSubgoal is WaitFor)
                {
                    if (currentSubgoal.IsGoalState(null))
                    {
                        this.SolveSubgoal();
                        return false;
                    }
                    return true;
                }
                return false;
            }
            return false;
        }
        private void UpdateCurrentBelief<T>(IEntity entity, EntityList<T> currentNode, EntityList<T> currentBelief) where T : IEntity
        {
            foreach (IEntity oldEntity in currentBelief.Entities)
            {
                IEntity currentEntity = currentNode[oldEntity.uid];
                currentBelief.UpdatePosition(oldEntity.col, oldEntity.row, currentEntity.col, currentEntity.row);
            }
            if (entity != null && currentBelief[entity.uid] == null)
            {
                currentBelief.Add((T)entity);
            }
        }
        private void TryConflictResolve(ref Node CurrentNode)
        {
            Plan plan = this.CreatePlan();
            if (plan == null)
            {
                this.ResolveConflict(ref CurrentNode);
                this.PerformNoOp(ref CurrentNode);
            }
            else
            {
                this.plan = plan;
                this.PerformNoOp(ref CurrentNode);
            }
        }

        private void ResolveConflict(ref Node CurrentNode)
        {
            if (this.encounteredObjects.Count != 0)
            {
                object obstacle = this.encounteredObjects.Pop();
                List<IEntity> usedFields = this.plan.ExtractUsedFields();
                if (obstacle != null)
                {
                    // agent plan is hindered by obstacle
                    if (obstacle is Box)
                    {
                        Box box = ((Box)obstacle);

                        foreach (Agent samaritan in SearchClient.FindSamaritans(this).Where(x => x.color == box.color))
                        {
                            if (samaritan.uid == this.uid) continue;
                            MoveAway moveAgentAway = new MoveAway(new IEntity[] { box, samaritan }, usedFields, this.uid, samaritan.uid);
                            if (!samaritan.subgoals.Any(x => x.Equals(moveAgentAway)))
                            {
                                MoveAgentTo moveAgentTo = new MoveAgentTo(box, samaritan.uid);
                                WaitFor waitForCompletion = new WaitFor(this.subgoals.Peek(), samaritan.uid);
                                samaritan.AddSubGoal(waitForCompletion);
                                samaritan.AddSubGoal(moveAgentAway);
                                samaritan.ReplanWithSubGoal(moveAgentTo);
                                this.AddSubGoal(new WaitFor(moveAgentAway, samaritan.uid));
                            }
                            else
                            {
                                this.ResolveConflict(ref CurrentNode);
                            }
                        }
                    }
                    else if (obstacle is Agent)
                    {
                        Agent samaritan = (Agent)obstacle;
                        MoveAway moveAgentAway = new MoveAway(new IEntity[] { samaritan }, usedFields, this.uid, samaritan.uid);
                        if (!samaritan.subgoals.Any(x => x.Equals(moveAgentAway)))
                        {
                            WaitFor waitForCompletion = new WaitFor(this.subgoals.Peek(), samaritan.uid);
                            this.UpdateCurrentBelief(this, CurrentNode.agentList, samaritan.CurrentBeliefs.agentList);
                            samaritan.AddSubGoal(waitForCompletion);
                            samaritan.ReplanWithSubGoal(moveAgentAway);
                            this.AddSubGoal(new WaitFor(moveAgentAway, samaritan.uid));
                        }
                        else
                        {
                            this.ResolveConflict(ref CurrentNode);
                        }
                    }
                }
            }
            else
            {
                this.PerformNoOp(ref CurrentNode);
            }
        }
        private void PerformNoOp(ref Node currentNode)
        {
            Node n = this.CreateNoOp(ref currentNode);
            currentNode = currentNode.ChildNode();
            currentNode.updateNode(n, this.col, this.row);
        }
        private Node CreateNoOp(ref Node currentNode)
        {
            Node n = currentNode.copyNode();
            n.action = new Command(ActionType.NoOp);
            n.agentCol = this.col;
            n.agentRow = this.row;
            return n;
        }
        private void ResetBeliefs()
        {
            this.CurrentBeliefs.boxList.Entities.Where(x => x.color != this.color).ToList().ForEach(b => CurrentBeliefs.boxList.Remove(b.uid));
            this.CurrentBeliefs.agentList.Entities.Where(x => x.uid != this.uid).ToList().ForEach(a => CurrentBeliefs.agentList.Remove(a.uid));
        }
        private void SolveSubgoal()
        {
            if (this.HasSubGoals)
            {
                SubGoal subgoal = this.subgoals.Pop();
                subgoal.completed = true;
                subgoal.owner = -1;
            }
            //this.ResetBeliefs(); // test later
            //this.plan = null;
        }
        private void Backtrack()
        {
            this.plan.UndoAction(this.CurrentBeliefs);
            this.CurrentBeliefs = this.CurrentBeliefs.parent;
        }
        private void ReplanWithSubGoal(SubGoal subGoal)
        {
            this.subgoals.Push(subGoal);
            //this.ResetBeliefs();
            this.plan = this.CreatePlan();
            if (this.HasPlan && this.plan.Completed )
            {
                this.SolveSubgoal();
                this.plan = null;
            }
        }
        private Plan CreatePlan()
        {
            this.CurrentBeliefs.parent = null;
            //this.strategy.reset();
            this.strategy.addToFrontier(CurrentBeliefs);
            SubGoal subGoal = subgoals.Peek();

            while (true)
            {
                if (this.strategy.frontierIsEmpty())
                {
                    this.strategy.reset();
                    return null;
                }

                Node leafNode = strategy.getAndRemoveLeaf();
                if (subGoal.IsGoalState(leafNode))
                {
                    this.strategy.reset();
                    return leafNode.extractPlan();
                }

                this.strategy.addToExplored(leafNode);
                foreach (Node n in leafNode.getExpandedNodes())
                {
                    if (!this.strategy.isExplored(n) && !this.strategy.inFrontier(n))
                    {
                        this.strategy.addToFrontier(n);
                    }
                }
            }
        }

        public override int GetHashCode()
        {
            return (this.row * Node.MAX_ROW) + this.col;
        }
        public override bool Equals(object obj)
        {
            if (this == obj)
                return true;

            if (obj == null)
                return false;

            if (!(obj is Agent))
                return false;

            Agent other = (Agent)obj;
            return (this.col == other.col && this.row == other.row && this.uid == other.uid);
        }
        public IEntity Clone()
        {
            Agent clone = new Agent(this.col, this.row, this.uid, this.color);
            clone.plan = this.plan;
            clone.strategy = this.strategy;
            clone.CurrentBeliefs = this.CurrentBeliefs;
            clone.subgoals = this.subgoals;
            clone.actionIndex = this.actionIndex + 1;
            return clone;
        }
    }
}
