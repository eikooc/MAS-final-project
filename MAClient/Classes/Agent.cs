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

        public Agent(int x, int y, char id, string color)
        {
            this.x = x;
            this.y = y;
            this.id = id;
            this.color = color;
        }

        private void run()
        {
            while (subgoals.Count>0)
            {

            }
        }
        private List<Node>solveSubgoal(Strategy strategy)
        {

            while (true)
            {

                if (strategy.frontierIsEmpty())
                {
                    return null;
                }

                Node leafNode = strategy.getAndRemoveLeaf();
                //ShowNode(leafNode, "Leaf");
                if (leafNode.isGoalState())
                {
                    System.Diagnostics.Debug.WriteLine(" - SOLUTION!!!!!!");
                    return leafNode.extractPlan();
                }

                strategy.addToExplored(leafNode);
                foreach (Node n in leafNode.getExpandedNodes(x, y))
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
