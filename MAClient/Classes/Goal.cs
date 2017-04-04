using System;

namespace MAClient.Classes
{
    public class Goal
    {
        public int x;
        public int y;

        public char id;

        public Goal(int x, int y, char id)
        {
            this.x = x;
            this.y = y;
            this.id = id;
        }



        public override int GetHashCode()
        {
            return (this.y * Node.MAX_ROW) + this.x;
        }


        //public override bool Equals(Object obj)
        //{

        //    if (this == obj)
        //        return true;

        //    if (obj == null)
        //        return false;

        //    if (!(obj is Tuple))
        //        return false;

        //    Tuple other = (Tuple)obj;
        //    if (this.x != other.x || this.y != other.y)
        //        return false;

        //    return true;
        //}
    }
}
