using Common.Interfaces;
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
        public Stack<SubGoal> subgoals;
        public Node CurrentBeliefs;
        public Plan plan;
        public Strategy strategy;


        public Agent(int x, int y, int id, string color)
        {
            this.col = x;
            this.row = y;
            this.uid = id;
            this.color = color;
            subgoals = new Stack<SubGoal>();
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
                if (currentSubgoal.type == SubGoalType.WaitFor)
                {
                    if(currentSubgoal.IsSolved(null))
                    {
                        SolveSubgoal();
                        return false;
                    }
                    return true;
                }
                return false;
            }
            return true;
        }

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
                            SolveSubgoal();
                            if(subgoals.Count != 0)
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

            Node nextMove = plan.GetNextAction();

            // pass on the remaining plan to agent in the next state
            CurrentBeliefs = nextMove;
            return nextMove;
        }

        public void acceptNextMove()
        {
            if (plan.Completed)
            {
                this.SolveSubgoal();
                CurrentBeliefs.boxList.Entities.Where(x => x.color != this.color).ToList().ForEach(b => CurrentBeliefs.boxList.Remove(b.uid));
                CurrentBeliefs.agentList.Entities.Where(x => x.uid != this.uid).ToList().ForEach(a => CurrentBeliefs.agentList.Remove(a.uid));
                plan = null;
            }
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
            this.plan = this.CreatePlan(this.strategy);
        }

        public Plan CreatePlan(Strategy strategy)
        {
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

                if (subGoal.IsSolved(leafNode))
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
