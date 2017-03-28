using System;
using System.Collections.Generic;

namespace SAClient.Classes
{
	public class StrategyDFS : Strategy
	{
		private Stack<Node> frontier;
		private HashSet<Node> frontierSet;

		public StrategyDFS() : base()
		{
			frontier = new Stack<Node>();
			frontierSet = new HashSet<Node>();
		}

		public override Node getAndRemoveLeaf()
		{
			Node n = frontier.Pop();
			frontierSet.Remove(n);
			return n;
		}

		public override void addToFrontier(Node n)
		{
			frontier.Push(n);
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
			return "Depth-first Search";
		}
	}
}
