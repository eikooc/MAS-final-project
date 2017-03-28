using System;
namespace SAClient.Classes
{
	public class WeightedAStar : Heuristic
	{
		private int W;

		public WeightedAStar(Node initialState, int W) : base(initialState)
		{
			this.W = W;
			this.goalReward /= this.W;
		}

		public override int f(Node n)
		{

			if (!n.hasFitness)
			{
				n.fitness = n.g() + this.W * this.h(n);
				n.hasFitness = true;
				//SearchClient.ShowNode(n, "expanded");
			}
			return n.fitness;
		}

		public override string ToString()
		{
			return string.Format("WA*({0}) evaluation", this.W);
		}
	}
}
