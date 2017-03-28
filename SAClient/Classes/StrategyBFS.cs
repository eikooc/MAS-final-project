using System;
using System.Collections.Generic;

namespace SAClient.Classes
{
	public class StrategyBFS : Strategy
	{
		private Queue<Node> frontier;
		private HashSet<Node> frontierSet;

		public StrategyBFS() : base()
		{
			frontier = new Queue<Node>();
			frontierSet = new HashSet<Node>();
		}


		public override Node getAndRemoveLeaf()
		{
			Node n = frontier.Dequeue();
			frontierSet.Remove(n);
			return n;
		}


		public override void addToFrontier(Node n)
		{
			frontier.Enqueue(n);
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
			return "Breadth-first Search";
		}
	}
}
