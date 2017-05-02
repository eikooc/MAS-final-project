using Common.Interfaces;
using System;

namespace MAClient.Classes.Entities
{
    public class Position : IEntity
    {
        public int uid { get; set; }
        public int col { get; set; }
        public int row { get; set; }

        public static int idCounter = 0;

        public Position(int col, int row)
        {
            this.uid = idCounter++;
            this.col = col;
            this.row = row;
        }

        public override int GetHashCode()
        {
            return (this.row * Node.MAX_COL) + this.col;
        }

        public override bool Equals(Object obj)
        {

            if (this == obj)
                return true;

            if (obj == null)
                return false;

            if (!(obj is Position))
                return false;

            Position other = (Position)obj;
            if (this.col != other.col || this.row != other.row)
                return false;

            return true;
        }

        public IEntity Clone()
        {
            return new Position(this.col, this.row);
        }
    }
}
