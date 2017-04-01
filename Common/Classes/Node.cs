using System;
using System.Collections.Generic;
using System.Text;
using SAClient.Enumerations;
using System.Collections.Specialized;
using System.Linq;
using System.Collections;

namespace Common.Classes
{
	public class Node
	{
		private static readonly Random RND = new Random(1); // if you don't want the same seed every time: System.Environment.TickCount()

		public static int MAX_ROW;
		public static int MAX_COL;
        
		public int fitness;
		public bool hasFitness = false;
		// Arrays are indexed from the top-left of the level, with first index being row and second being column.
		// Row 0: (0,0) (0,1) (0,2) (0,3) ...
		// Row 1: (1,0) (1,1) (1,2) (1,3) ...
		// Row 2: (2,0) (2,1) (2,2) (2,3) ...
		// ...
		// (Start in the top left corner, first go down, then go right)
		// E.g. this.walls[2] is an array of booleans having size MAX_COL.
		// this.walls[row][col] is true if there's a wall at (row, col)
		//
        

		public Dictionary<Tuple<int, int>, Box> boxList;
		public static Dictionary<Tuple<int, int>, Goal> goalList;
        public static Dictionary<Tuple<int,int>, bool> wallList;
        public Dictionary<Tuple<int, int>, Agent> agentList;

        public int agentRow;
        public int agentCol;


        public Node parent;
		public Command action;

		private int _g;

		private int _hash = 0;

		public Node(Node parent, int row, int col) : this(parent)
		{
			

			this.boxList = new Dictionary<Tuple<int,int>, Box>();
			goalList = new Dictionary<Tuple<int, int>, Goal>();
            wallList = new Dictionary<Tuple<int, int>, bool>();
            this.agentList = new Dictionary<Tuple<int, int>, Agent>();



		}

		public Node(Node parent)
		{
			this.parent = parent;
			if (parent == null)
			{
				this._g = 0;
			}
			else
			{
				this._g = parent.g() + 1;
			}
			this.boxList = new Dictionary<Tuple<int, int>, Box>();
            this.agentList = new Dictionary<Tuple<int, int>, Agent>();
        }

		public int g()
		{
			return this._g;
		}

		public bool isInitialState()
		{
			return this.parent == null;
		}

		public bool isGoalState()
		{
            foreach(Goal goal in goalList.Values)
            {
                if (boxList.ContainsKey(Tuple.Create(goal.x, goal.y)))
                {
                    if (goal.id == char.ToLower(boxList[Tuple.Create(goal.x, goal.y)].id)) continue;
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            return true;
		}

		public List<Node> getExpandedNodes()
		{
			List<Node> expandedNodes = new List<Node>(Command.EVERY.Length);
			foreach (Command c in Command.EVERY)
			{
				// Determine applicability of action
				int newAgentRow = this.agentRow + Command.dirToRowChange(c.dir1);
				int newAgentCol = this.agentCol + Command.dirToColChange(c.dir1);

				if (c.actionType == ActionType.Move)
				{
					// Check if there's a wall or box on the cell to which the agent is moving
					if (this.cellIsFree(newAgentRow, newAgentCol))
					{ // O(m)
						Node n = this.ChildNode(); // gl: O(n),  ny: O(m), 
						n.action = c;
						n.agentRow = newAgentRow;
						n.agentCol = newAgentCol;
						expandedNodes.Add(n);
					}
				}
				else if (c.actionType == ActionType.Push)
				{
					// Make sure that there's actually a box to move
					if (this.boxAt(newAgentRow, newAgentCol))
					{
						int newBoxRow = newAgentRow + Command.dirToRowChange(c.dir2.Value);
						int newBoxCol = newAgentCol + Command.dirToColChange(c.dir2.Value);
						// .. and that new cell of box is free
						if (this.cellIsFree(newBoxRow, newBoxCol))
						{
							Node n = this.ChildNode();
							n.action = c;
							n.agentRow = newAgentRow;
							n.agentCol = newAgentCol;
							Box b = n.getBox(newAgentCol, newAgentRow);

                            if (b != null)
							{
								b.y = newBoxRow;
								b.x = newBoxCol;

                                n.boxList.Remove(Tuple.Create(newAgentCol, newAgentRow));
                                n.boxList.Add(Tuple.Create(b.x, b.y), b);
                            }
							expandedNodes.Add(n);
						}
					}
				}
				else if (c.actionType == ActionType.Pull)
				{
					// Cell is free where agent is going
					if (this.cellIsFree(newAgentRow, newAgentCol))
					{
						int boxRow = this.agentRow + Command.dirToRowChange(c.dir2.Value);
						int boxCol = this.agentCol + Command.dirToColChange(c.dir2.Value);
						// .. and there's a box in "dir2" of the agent
						if (this.boxAt(boxRow, boxCol))
						{
							Node n = this.ChildNode();
							n.action = c;
							n.agentRow = newAgentRow;
							n.agentCol = newAgentCol;
							Box b = n.getBox(boxCol, boxRow);
							if (b != null)
                            {
                                n.boxList.Remove(Tuple.Create(b.x, b.y));

                                b.y = this.agentRow;
								b.x = this.agentCol;

                                n.boxList.Add(Tuple.Create(b.x, b.y), b);
                            }
							expandedNodes.Add(n);
						}
					}
				}
			}


			// Collections.shuffle(expandedNodes, RND);
			return expandedNodes.OrderBy(item => RND.Next()).ToList();
		}


		private bool cellIsFree(int row, int col)
		{
            Tuple<int, int> pos = Tuple.Create(col, row);

            return (!wallList.ContainsKey(pos) && !boxList.ContainsKey(pos) && !agentList.ContainsKey(pos));
		}

		public bool boxAt(int row, int col)
        {
            Tuple<int, int> pos = Tuple.Create(col, row);
            return this.boxList.ContainsKey(pos);
		}

		public Box getBox(int x, int y)
		{
			Tuple<int,int> key = Tuple.Create(x, y);
			if(boxList.ContainsKey(key)) 
			{
				return boxList[key];
			}
			return null;

		}

		private Node ChildNode()
		{
			Node copy = new Node(this);

			foreach (Box box in boxList.Values)
			{
				Tuple<int,int> t = Tuple.Create(box.x, box.y);
                Box newBox = new Box(box.x, box.y, box.id, box.color);
                newBox.assignedGoal = box.assignedGoal;
                copy.boxList.Add(t, newBox);
			}


            foreach (Agent agent in agentList.Values)
            {
                Tuple<int, int> t = Tuple.Create(agent.x, agent.y);
                Agent newAgent = new Agent(agent.x, agent.y, agent.id, agent.color);
                copy.agentList.Add(t, newAgent);
            }
            
			return copy;
		}

		public List<Node> extractPlan()
		{
			List<Node> plan = new List<Node>();
			Node n = this;
			while (!n.isInitialState())
			{
				plan.Insert(0, n);
				n = n.parent;
			}
			return plan;
		}


		public override int GetHashCode()
		{
			if (this._hash == 0)
			{
				int prime = 31;
				int result = 1;
				result = prime * result + this.agentCol;
				result = prime * result + this.agentRow;
				result = prime * result + this.boxList.GetHashCode();
										  
				//result = prime * result + ((IStructuralEquatable)this.goals).GetHashCode(comparer);
				//result = prime * result + ((IStructuralEquatable)this.walls).GetHashCode(comparer);
				this._hash = result;
			}
			return this._hash;
		}


		public override bool Equals(Object obj)
		{

			if (this == obj)
				return true;
			if (obj == null)
				return false;
			if (this.GetType() != obj.GetType())
				return false;
			Node other = (Node)obj;
			if (this.agentRow != other.agentRow || this.agentCol != other.agentCol)
				return false;
			if (!this.boxList.OrderBy(x => x.Key).SequenceEqual(other.boxList.OrderBy(x => x.Key)))
				return false;

			return true;
		}


		public override string ToString()
		{
			StringBuilder s = new StringBuilder();
			for (int row = 0; row < MAX_ROW; row++)
			{
				if (!wallList.ContainsKey(Tuple.Create(0, row)))
				{
					break;
				}
				for (int col = 0; col < MAX_COL; col++)
				{
                    Tuple<int, int> pos = Tuple.Create(col, row);
					if (boxList.ContainsKey(pos))
					{
						s.Append(boxList[pos].id);
					}
					else if (goalList.ContainsKey(pos))
					{
						s.Append(goalList[pos].id);
					}
					else if (wallList.ContainsKey(pos))
					{
						s.Append("+");
					}
					else if (agentList.ContainsKey(pos))
					{
						s.Append(agentList[pos].id);
					}
					else
					{
						s.Append(" ");
					}
				}
				s.Append("\n");
			}
			return s.ToString();
		}

	}
}