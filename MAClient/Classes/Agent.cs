using Common.Classes;
using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MAClient.Classes
{
    public class Agent : IEntity
    {
        public int col { get; set; }
        public int row { get; set; }
        public int uid { get; set; }

        public string color;
        public Stack<SubGoal> subgoals;
        public Node CurrentBeliefs;
        public Stack<Node> plan;
        public Strategy strategy;


        public Agent(int x, int y, int  id, string color)
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
            while(subgoals.Count != 0)
            {
                if(plan == null)
                {
                    plan = new Stack<Node>(solveSubgoal(strategy));
                }
            }
        }

        public Node getNextMove()
        {
            if (plan == null)
            {
                if(subgoals.Count != 0)
                {
                    List<Node> planList = new List<Node>();
                    while (planList.Count == 0)
                    {
                        CurrentBeliefs.parent = null;
                        strategy.reset();
                        strategy.addToFrontier(CurrentBeliefs);
                        planList = solveSubgoal(strategy);
                        // subgoals må kun slettes hvis de er løst. kan ikke håndtere situationer der ikkan kan solves på egen hånd
                        planList.Reverse();
                        plan = new Stack<Node>(planList);
                        if (plan.Count == 0)
                        {
                            subgoals.Pop();
                        }
                    }
                }
                else
                {
                    return null;
                }
            }
            
            Node nextMove = plan.Pop();

            // pass on the remaining plan to agent in the next state
            if (plan.Count == 0)
            {
                subgoals.Pop();

                CurrentBeliefs.boxList.Entities.Where(x => x.color != this.color).ToList().ForEach(b => CurrentBeliefs.boxList.Remove(b.uid));
                CurrentBeliefs.agentList.Entities.Where(x => x.uid != this.uid).ToList().ForEach(a => CurrentBeliefs.agentList.Remove(a.uid));
                plan = null;
            }
            CurrentBeliefs = nextMove;
            return nextMove;
        }
        public void backTrack()
        {
            plan.Push(CurrentBeliefs);
            this.CurrentBeliefs = CurrentBeliefs.parent;
            
        }

        private List<Node>solveSubgoal(Strategy strategy)
        {
            SubGoal subGoal = subgoals.Peek();
            while (true)
            {

                if (strategy.frontierIsEmpty())
                {
                    return null;
                }

                Node leafNode = strategy.getAndRemoveLeaf();

                if (leafNode.isSubGoalState(subGoal))
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
