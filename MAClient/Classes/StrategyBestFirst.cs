using Priority_Queue;
using System;
using System.Collections.Generic;

namespace MAClient.Classes
{
    public class StrategyBestFirst : Strategy
    {
        private SimplePriorityQueue<Node> frontier;
        private HashSet<Node> frontierSet;
        private Heuristic heuristic;

        public StrategyBestFirst(Heuristic h) : base()
        {
            this.heuristic = h;
            frontier = new SimplePriorityQueue<Node>();
            frontierSet = new HashSet<Node>();
        }

        public override Node getAndRemoveLeaf()
        {
            Node n = frontier.Dequeue();
            frontierSet.Remove(n);
            return n;
        }

        int idx = 0;

        public override void addToFrontier(Node n)
        {
            frontier.Enqueue(n, heuristic.f(n));
            frontierSet.Add(n);
        }


        public override int countFrontier()
        {
            return frontier.Count;
        }

        public override bool frontierIsEmpty()
        {
            return frontier.Count == 0;
        }


        public override bool inFrontier(Node n)
        {
            return frontierSet.Contains(n);
        }


        public override String ToString()
        {
            return "Best-first Search using " + this.heuristic.ToString();
        }

        public override void reset()
        {
            frontierSet.Clear();
            frontier.Clear();
        }
    }

}
