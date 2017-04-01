using System;
using SAClient.Classes;

namespace Common
{
	public class Box
	{
		public int x;
		public int y;

		public Goal assignedGoal;

        public char id;

        public string color;

		public Box(int x, int y, char id, string color)
		{
			this.x = x;
			this.y = y;
            this.id = id;
            this.color = color;
		}

		public bool hasGoal()
		{
			return assignedGoal != null;
		}

		public bool inGoal()
		{
			return this.hasGoal() && this.x == this.assignedGoal.x && this.y == this.assignedGoal.y;
		}

		public void assignGoal(Goal goal)
		{
			this.assignedGoal = goal;
		}

		public int goalDistance()
		{
			return Math.Abs(this.x - this.assignedGoal.x) + Math.Abs(this.y - this.assignedGoal.y);
		}



		public override int GetHashCode()
		{
			return (this.y * Node.MAX_ROW) + this.x;
		}


		public override bool Equals(Object obj)
		{

			if (this == obj)
				return true;

			if (obj == null)
				return false;

			if (!(obj is Tuple))
				return false;

			Tuple other = (Tuple)obj;
			if (this.x != other.x || this.y != other.y)
				return false;

			return true;
		}
	}
}
