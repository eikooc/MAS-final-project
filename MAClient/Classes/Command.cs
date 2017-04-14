using MAClient.Enumerations;
using System;
using System.Collections.Generic;


namespace MAClient.Classes
{
    public class Command
    {
        public readonly ActionType actionType;
        public readonly Dir dir1;
        public readonly Dir? dir2;


        public static readonly Command[] EVERY;

        static Command()
        {
            List<Command> cmds = new List<Command>();
            foreach (Dir d1 in Enum.GetValues(typeof(Dir)))
            {
                foreach (Dir d2 in Enum.GetValues(typeof(Dir)))
                {
                    if (!Command.isOpposite(d1, d2))
                    {
                        cmds.Add(new Command(ActionType.Push, d1, d2));
                    }
                }
            }
            foreach (Dir d1 in Enum.GetValues(typeof(Dir)))
            {
                foreach (Dir d2 in Enum.GetValues(typeof(Dir)))
                {
                    if (d1 != d2)
                    {
                        cmds.Add(new Command(ActionType.Pull, d1, d2));
                    }
                }
            }
            foreach (Dir d in Enum.GetValues(typeof(Dir)))
            {
                cmds.Add(new Command(d));
            }

            EVERY = cmds.ToArray();
        }


        public static bool isOpposite(Dir d1, Dir d2)
        {
            return ((int)d1 + (int)d2) == 3;
        }

        public static int dirToRowChange(Dir d)
        {
            // South is down one row (1), north is up one row (-1).
            switch (d)
            {
                case Dir.S:
                    return 1;
                case Dir.N:
                    return -1;
                default:
                    return 0;
            }
        }

        public static int dirToColChange(Dir d)
        {
            // East is right one column (1), west is left one column (-1).
            switch (d)
            {
                case Dir.E:
                    return 1;
                case Dir.W:
                    return -1;
                default:
                    return 0;
            }
        }


        public Command(ActionType action)
        {
            this.actionType = action;
            this.dir1 = Dir.E;// skal måske rettes. ingen direction til NoOP
            this.dir2 = null;
        }

        public Command(Dir d)
        {
            this.actionType = ActionType.Move;
            this.dir1 = d;
            this.dir2 = null;
        }

        public Command(ActionType t, Dir d1, Dir d2)
        {
            this.actionType = t;
            this.dir1 = d1;
            this.dir2 = d2;
        }

        public override string ToString()
        {
            if (this.actionType == ActionType.NoOp)
                return string.Format("{0}", this.actionType.ToString());
            else if (this.actionType == ActionType.Move)
                return string.Format("{0}({1})", this.actionType.ToString(), this.dir1.ToString());
            else
                return string.Format("{0}({1},{2})", this.actionType.ToString(), this.dir1.ToString(), this.dir2.ToString());
        }
    }
}
