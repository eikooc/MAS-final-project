using Common.Interfaces;
using System;

namespace MAClient.Classes.Entities
{
    public class Goal : IEntity
    {
        public int col { get; set; }
        public int row { get; set; }
        public char id;
        public int uid { get; set; }

        public static int idCounter = 0;

        public Goal(int x, int y, char id)
        {
            this.col = x;
            this.row = y;
            this.id = id;
            this.uid = idCounter++;
        }

        public IEntity Clone()
        {
            return new Goal(this.col, this.row, this.id);
        }

        public override int GetHashCode()
        {
            return (this.row * Node.MAX_COL) + this.col;
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
