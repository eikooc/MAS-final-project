using Common.Interfaces;
using MAClient.Classes.Goals;
using MAClient.Enumerations;
using System.Collections.Generic;
using System.Linq;

namespace MAClient.Classes.Entities
{
    // refactor agent, such that all subgoals are replaced with objectives.
    // objectives should now contain multiple subgoals.
    public class Agent : IEntity
    {
        public int col { get; set; }
        public int row { get; set; }
        public int uid { get; set; }

        public string color;
        public Stack<SubGoal> subgoals; // redundant
        public Node CurrentBeliefs;
        public Plan plan; // redundant
        public Strategy strategy;
        public Stack<IEntity> encounteredObjects; // refactor into the specific subgoals / objective
        private Objective CurrentObjective;

        public Agent(int x, int y, int id, string color)
        {
            this.col = x;
            this.row = y;
            this.uid = id;
            this.color = color;
            subgoals = new Stack<SubGoal>();
            encounteredObjects = new Stack<IEntity>();
        }

        public void run()
        {
            // NOT DONE
            strategy = new StrategyBestFirst(new Greedy(CurrentBeliefs));
            while (subgoals.Count != 0)
            {
                if (plan == null)
                {
                    plan = CreatePlan(strategy);
                }
            }
        }

        public bool IsWaiting()
        {
            if (this.subgoals.Count > 0)
            {
                SubGoal currentSubgoal = this.subgoals.Peek();
                if (currentSubgoal is WaitFor)
                {
                    return !currentSubgoal.IsGoalState(null);
                }
                return false;
            }
            return true;
        }

        // use objective's getnextMove
        public Node getNextMove()
        {
            if (plan == null)
            {
                if (subgoals.Count != 0)
                {
                    plan = CreatePlan(strategy);
                    while (plan == null || plan.Completed)
                    {
                        if (plan == null)
                        {
                            return null;
                        }
                        if (plan.Completed)
                        {
                            subgoals.Pop();
                            if (subgoals.Count != 0)
                            {
                                plan = CreatePlan(strategy);
                            }
                            else
                            {
                                return null;
                            }
                        }
                    }
                }
                else
                {
                    return null;
                }
            }

            Node nextMove = plan.GetNextAction(); // objective.getnextMove

            // pass on the remaining plan to agent in the next state
            CurrentBeliefs = nextMove;
            return nextMove;
        }

        public void acceptNextMove()
        {
            if (plan.Completed)
            {
                this.SolveSubgoal();
                ResetBeliefs();
                plan = null;
            }
        }

        private void ResetBeliefs()
        {
            CurrentBeliefs.boxList.Entities.Where(x => x.color != this.color).ToList().ForEach(b => CurrentBeliefs.boxList.Remove(b.uid));
            CurrentBeliefs.agentList.Entities.Where(x => x.uid != this.uid).ToList().ForEach(a => CurrentBeliefs.agentList.Remove(a.uid));
        }

        public void SolveSubgoal()
        {
            SubGoal subgoal = subgoals.Pop();
            subgoal.completed = true;
        }

        public void backTrack()
        {
            plan.UndoAction(this.CurrentBeliefs);
            this.CurrentBeliefs = this.CurrentBeliefs.parent;
        }

        public void ReplanWithSubGoal(SubGoal subGoal)
        {
            this.subgoals.Push(subGoal);
            ResetBeliefs();
            this.plan = this.CreatePlan(this.strategy);
        }

        public bool AcceptObjective(Objective objective)
        {
            //if objective already exists then
            //    return false
            //else
            //    add objective to stack, and ....
            //return true;
            return true;
        }

        public Node ResolveConflict(Node n)
        {
            Plan plan = this.CreatePlan(this.strategy);
            if (plan == null)
            {
                return this.CurrentObjective.ResolveConflict(this, n);
            }
            else
            {
                this.plan = plan;
                return this.CurrentObjective.PerformNoOp(this, n);
            }
        }

        public void UpdateSubgoalStates(Node n)
        {
            if (subgoals.Count > 0)
            {
                this.subgoals.Peek().UpdateState(n);
                if (this.subgoals.Peek().completed)
                {
                    subgoals.Pop();
                }
            }
        }

        public Plan CreatePlan(Strategy strategy)
        {
            // return currentObjective.CreatePlan(strategy, CurrentBeliefs); 
            // redundant
            CurrentBeliefs.parent = null;
            strategy.reset();
            strategy.addToFrontier(CurrentBeliefs);
            SubGoal subGoal = subgoals.Peek();
            
            while (true)
            {
                if (strategy.frontierIsEmpty())
                {
                    return null;
                }

                Node leafNode = strategy.getAndRemoveLeaf();

                if (subGoal.IsGoalState(leafNode))
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

            return clone;
        }
    }
}
