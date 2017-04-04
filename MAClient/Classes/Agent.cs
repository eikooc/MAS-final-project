using System;

namespace MAClient.Classes
{
    public class Agent
    {
        public int x;
        public int y;

        public char id;

        public string color;

        public Agent(int x, int y, char id, string color)
        {
            this.x = x;
            this.y = y;
            this.id = id;
            this.color = color;
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
