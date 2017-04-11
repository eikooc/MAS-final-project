using System;
using System.Collections.Generic;
namespace MAClient.Classes
{
	public class StrategyBestFirst : Strategy
	{
		private PriorityQueue<Node> frontier;
		private HashSet<Node> frontierSet;
		private Heuristic heuristic;

		public StrategyBestFirst(Heuristic h) : base()
		{
			this.heuristic = h;
            list.Sort((x, y) => y.Item1.CompareTo(x.Item1));
            frontier = new List<Tuple<int,Node>>(Comparator.comparingInt((Node n)->h.f(n)));
			frontierSet = new HashSet<Node>();
		}


		public override Node getAndRemoveLeaf()
		{
			Node n = frontier.poll();
			frontierSet.Remove(n);
			return n;
		}

		int idx = 0;

		public override void addToFrontier(Node n)
		{
			frontier.add(n);
			frontierSet.Add(n);
		}


		public override int countFrontier()
		{
			return frontier.size();
		}

		public override bool frontierIsEmpty()
		{
			return frontier.isEmpty();
		}


		public override bool inFrontier(Node n)
		{
			return frontierSet.Contains(n);
		}


		public override String ToString()
		{
			return "Best-first Search using " + this.heuristic.ToString();
		}
	}

}