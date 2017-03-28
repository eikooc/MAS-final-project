using System;
namespace SAClient.Classes
{
	public class Greedy : Heuristic
	{
		public Greedy(Node initialState) : base(initialState)
		{
		}

		public override int f(Node n)
		{
			if (!n.hasFitness)
			{
				n.fitness = this.h(n);
				n.hasFitness = true;
				//SearchClient.ShowNode(n, "expanded");
			}
			return n.fitness;
		}

		public override string ToString()
		{
			return "Greedy evaluation";
		}
	}
}
