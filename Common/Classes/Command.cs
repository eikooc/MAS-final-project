using Common.Enumeration;
using System;
using System.Collections.Generic;

namespace Common.Classes
{
	public class Command
	{
		public readonly static Command[] Every;
		public readonly Enumeration.Type actType;
		public readonly Direction? dir1;
		public readonly Direction? dir2;

		static Command()
		{
			List<Command> cmds = new List<Command>();
			foreach (Direction d in Enum.GetValues(typeof(Direction)))
			{
				cmds.Add(new Command(d));
			}

			foreach (Direction d1 in Enum.GetValues(typeof(Direction)))
			{
				foreach (Direction d2 in Enum.GetValues(typeof(Direction)))
				{
					if (!Command.isOpposite(d1, d2))
					{
						cmds.Add(new Command(Enumeration.Type.Push, d1, d2));
					}
				}
			}

			foreach (Direction d1 in Enum.GetValues(typeof(Direction)))
			{
				foreach (Direction d2 in Enum.GetValues(typeof(Direction)))
				{
					if (d1 != d2)
					{
						cmds.Add(new Command(Enumeration.Type.Pull, d1, d2));
					}
				}
			}

			cmds.Add(new Command());

			Every = cmds.ToArray();
		}

		private static bool isOpposite(Direction d1, Direction d2)
		{
			return ((int)d1 + (int)d2) == 3;
		}

		public Command()
		{
			actType = Enumeration.Type.NoOp;
			dir1 = null;
			dir2 = null;

		}

		public Command(Direction d)
		{
			actType = Enumeration.Type.Move;
			dir1 = d;
			dir2 = null;
		}


		public Command(Enumeration.Type t, Direction d1, Direction d2)
		{
			actType = t;
			dir1 = d1;
			dir2 = d2;
		}

		public string toString()
		{
			if (actType == Enumeration.Type.NoOp)
			{
				return actType.ToString();
			}
			if (actType == Enumeration.Type.Move)
			{
				return actType.ToString() + "(" + dir1 + ")";
			}

			return actType.ToString() + "(" + dir1 + "," + dir2 + ")";
		}

		public string toActionString()
		{
			return "[" + this.toString() + "]";
		}

	}

}
