﻿using System;

namespace MAClient.Classes
{
    public class Box
    {
        public int x;
        public int y;

        public Goal assignedGoal;

        public char id;
        public static int idCounter = 0;
        public int uid;

        public string color;

        public Box(int x, int y, char id, string color, Box parent = null)
        {
            this.x = x;
            this.y = y;
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

            int prime = 31;
            int result = 1;
            result = prime * result + this.uid;
            result = prime * result + ((this.y * Node.MAX_ROW) + this.x);
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
            return (this.x == other.x && this.y == other.y && this.uid == other.uid);
        }
    }
}
