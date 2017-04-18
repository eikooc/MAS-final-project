using Common.Interfaces;
using System;

namespace MAClient.Classes
{
    public class Box : IEntity
    {
        public int col { get; set; }
        public int row { get; set; }

        public Goal assignedGoal;

        public char id;
        public static int idCounter = 0;
        public int uid { get; set; }

        public string color;

        public Box(int x, int y, char id, string color, Box parent = null)
        {
            this.col = x;
            this.row = y;
            this.id = id;
            this.color = color;
            this.uid = parent == null ? idCounter++ : parent.uid;
        }

        public bool hasGoal()
        {
            return assignedGoal != null;
        }

        public bool inGoal()
        {
            return this.hasGoal() && this.col == this.assignedGoal.x && this.row == this.assignedGoal.y;
        }

        public void assignGoal(Goal goal)
        {
            this.assignedGoal = goal;
        }

        public int goalDistance()
        {
            return Math.Abs(this.col - this.assignedGoal.x) + Math.Abs(this.row - this.assignedGoal.y);
        }



        public override int GetHashCode()
        {

            int prime = 31;
            int result = 1;
            result = prime * result + this.uid;
            result = prime * result + ((this.row * Node.MAX_COL) + this.col);
            return result;
        }

        public override bool Equals(Object obj)
        {

            if (this == obj)
                return true;

            if (obj == null)
                return false;

            if (!(obj is Box))
                return false;

            Box other = (Box)obj;
            return (this.col == other.col && this.row == other.row && this.uid == other.uid);
        }

        public IEntity Clone()
        {
            Box clone = new Box(this.col, this.row, this.id, this.color, this);
            clone.assignedGoal = this.assignedGoal;
            return clone;
        }

    }
}
