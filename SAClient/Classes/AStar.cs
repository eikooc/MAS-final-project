using System;
namespace SAClient.Classes
{
	public class AStar : Heuristic
	{
		public AStar(Node initialState) : base(initialState)
		{

		}

		public override int f(Node n)
		{
			if (!n.hasFitness)
			{
				n.fitness = n.g() + this.h(n);
				n.hasFitness = true;
				//SearchClient.ShowNode(n, "expanded");
			}
			return n.fitness;
		}

		public override string ToString()
		{
			return "A* evaluation";
		}
	}
}
