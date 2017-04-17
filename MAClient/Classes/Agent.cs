using Common.Classes;
using System;
using System.Collections.Generic;

namespace MAClient.Classes
{
    public class Agent
    {
        public int x;
        public int y;

        public char id;

        public string color;
        public Stack<SubGoal> subgoals;
        public Node CurrentBeliefs;
        public Stack<Node> plan;
        public Strategy strategy;

        

        public Agent(int x, int y, char id, string color)
        {
            this.x = x;
            this.y = y;
            this.id = id;
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
                        planList.Reverse();
                        plan = new Stack<Node>(planList);
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
                plan = null;
            }
            CurrentBeliefs = nextMove;
            return nextMove;
        }
        public void backTrack()
        {
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
                //ShowNode(leafNode, "Leaf");
                if (leafNode.isSubGoalState(subGoal))
                {
                    subgoals.Pop();
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
            return (this.y * Node.MAX_ROW) + this.x;
        }


        //public override bool Equals(Object obj)
        //{

        //    if (this == obj)
        //        return true;

        //    if (obj == null)
        //        return false;

        //    if (!(obj is Tuple))
        //        return false;

        //    Tuple other = (Tuple)obj;
        //    if (this.x != other.x || this.y != other.y)
        //        return false;

        //    return true;
        //}
    }
}
