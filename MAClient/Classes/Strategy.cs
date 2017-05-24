using System;
using System.Collections.Generic;

namespace MAClient.Classes
{
	public abstract class Strategy
	{
		protected HashSet<Node> explored;
		private readonly int startTime;

		public Strategy()
		{
			this.explored = new HashSet<Node>();
			this.startTime = System.Environment.TickCount;
		}

		public void addToExplored(Node n)
		{
			this.explored.Add(n);
		}

		public bool isExplored(Node n)
		{
			return this.explored.Contains(n);
		}

		public int countExplored()
		{
			return this.explored.Count;
		}

		public float timeSpent()
		{
			return (System.Environment.TickCount - this.startTime) / 1000f;
		}

		public abstract Node getAndRemoveLeaf();

		public abstract void addToFrontier(Node n);

		public abstract bool inFrontier(Node n);

		public abstract int countFrontier();

		public abstract bool frontierIsEmpty();

		public override abstract string ToString();

        public virtual void reset()
        {
        }

    }
}
