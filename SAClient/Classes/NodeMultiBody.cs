using System;
using System.Collections.Generic;
using System.Text;
using SAClient.Enumerations;
using System.Collections.Specialized;
using System.Linq;
using System.Collections;

namespace SAClient.Classes
{
    public class NodeMultiBody
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

        public bool[][] walls;
        public char[][] boxes;
        public char[][] goals;

        public Dictionary<Tuple<int, int>, Box> boxList;
        public static Dictionary<Tuple<int, int>, Goal> goalList;
        public Dictionary<Tuple<int, int>, Agent> agentList;


        public Node parent;
        public Command action;

        private int _g;

        private int _hash = 0;

        public NodeMultiBody(NodeMultiBody parent, int row, int col) : this(parent)
        {
            MAX_ROW = row;
            MAX_COL = col;
            this.walls = new bool[MAX_ROW][];
            for (int x = 0; x < walls.Length; x++)
            {
                walls[x] = new bool[MAX_COL];
            }
            this.boxes = new char[MAX_ROW][];
            for (int x = 0; x < boxes.Length; x++)
            {
                boxes[x] = new char[MAX_COL];
            }
            this.goals = new char[MAX_ROW][];
            for (int x = 0; x < goals.Length; x++)
            {
                goals[x] = new char[MAX_COL];
            }

            this.boxList = new Dictionary<Tuple<int, int>, Box>();
            goalList = new Dictionary<Tuple<int, int>, Goal>();
            this.agentList = new Dictionary<Tuple<int, int>, Agent>();

        }

        public NodeMultiBody(NodeMultiBody parent)
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
            this.boxes = new char[MAX_ROW][];
            for (int x = 0; x < boxes.Length; x++)
            {
                boxes[x] = new char[MAX_COL];
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
            for (int row = 1; row < MAX_ROW - 1; row++)
            {
                for (int col = 1; col < MAX_COL - 1; col++)
                {
                    char g = goals[row][col];
                    char b = char.ToLower(boxes[row][col]);
                    if (g > 0 && b != g)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public List<NodeMultiBody> getExpandedNodes()
        {
            List<NodeMultiBody> expandedNodes = new List<NodeMultiBody>(Command.EVERY.Length);

            List<Agent> visitedAgents = new List<Agent>();
            foreach (Agent agent in agentList.Values)
            {

                foreach (Command c in Command.EVERY)
                {

                    // Determine applicability of action
                    int newAgentRow = agent.y + Command.dirToRowChange(c.dir1);
                    int newAgentCol = agent.x + Command.dirToColChange(c.dir1);
                    Tuple<int, int> pos = Tuple.Create(agent.x, agent.y);

                    foreach (Agent otherAgent in agentList.Values)
                    {
                        if (agent == otherAgent) continue; // only do it for other agents
                        if (visitedAgents.Contains(otherAgent)) continue; // if we have already calculated agent*otherAgent combination. Move along

                        if (c.actionType == ActionType.Move)
                        {
                            // Check if there's a wall or box on the cell to which the agent is moving
                            if (this.cellIsFree(newAgentRow, newAgentCol))
                            { // O(m)
                                NodeMultiBody n = this.ChildNode(); // gl: O(n),  ny: O(m), 
                                n.action = c;
                                n.agentList[pos].x = newAgentCol;
                                n.agentList[pos].y = newAgentRow;

                                expandedNodes.Add(n);
                            }
                        }
                        //else if (c.actionType == ActionType.Push)
                        //{
                        //    // Make sure that there's actually a box to move
                        //    if (this.boxAt(newAgentRow, newAgentCol))
                        //    {
                        //        int newBoxRow = newAgentRow + Command.dirToRowChange(c.dir2.Value);
                        //        int newBoxCol = newAgentCol + Command.dirToColChange(c.dir2.Value);
                        //        // .. and that new cell of box is free
                        //        if (this.cellIsFree(newBoxRow, newBoxCol))
                        //        {
                        //            NodeMultiBody n = this.ChildNode();
                        //            n.action = c;
                        //            n.agentRow = newAgentRow;
                        //            n.agentCol = newAgentCol;
                        //            Box b = n.getBox(newAgentCol, newAgentRow);
                        //            if (b != null)
                        //            {
                        //                b.y = newBoxRow;
                        //                b.x = newBoxCol;
                        //            }
                        //            n.boxes[newBoxRow][newBoxCol] = this.boxes[newAgentRow][newAgentCol];
                        //            n.boxes[newAgentRow][newAgentCol] = (char)0;
                        //            expandedNodes.Add(n);
                        //        }
                        //    }
                        //}
                        //else if (c.actionType == ActionType.Pull)
                        //{
                        //    // Cell is free where agent is going
                        //    if (this.cellIsFree(newAgentRow, newAgentCol))
                        //    {
                        //        int boxRow = this.agentRow + Command.dirToRowChange(c.dir2.Value);
                        //        int boxCol = this.agentCol + Command.dirToColChange(c.dir2.Value);
                        //        // .. and there's a box in "dir2" of the agent
                        //        if (this.boxAt(boxRow, boxCol))
                        //        {
                        //            NodeMultiBody n = this.ChildNode();
                        //            n.action = c;
                        //            n.agentRow = newAgentRow;
                        //            n.agentCol = newAgentCol;
                        //            Box b = n.getBox(boxCol, boxRow);
                        //            if (b != null)
                        //            {
                        //                b.y = this.agentRow;
                        //                b.x = this.agentCol;
                        //            }
                        //            n.boxes[this.agentRow][this.agentCol] = this.boxes[boxRow][boxCol];
                        //            n.boxes[boxRow][boxCol] = (char)0;
                        //            expandedNodes.Add(n);
                        //        }
                        //    }
                        //}
                    }
                }
                visitedAgents.Add(agent);


            }


            // Collections.shuffle(expandedNodes, RND);
            return expandedNodes.OrderBy(item => RND.Next()).ToList();
        }


        private bool cellIsFree(int row, int col)
        {
            return !this.walls[row][col] && this.boxes[row][col] == 0;
        }

        public bool boxAt(int row, int col)
        {
            return this.boxes[row][col] > 0;
        }

        public Box getBox(int x, int y)
        {
            Tuple<int, int> key = Tuple.Create(x, y);
            if (boxList.ContainsKey(key))
            {
                return boxList[key];
            }
            return null;

        }

        private NodeMultiBody ChildNode()
        {
            NodeMultiBody copy = new Node(this);
            copy.walls = this.walls;
            copy.goals = this.goals;

            foreach (Box box in this.boxList.Values)
            {
                Tuple<int, int> t = Tuple.Create(box.x, box.y);
                Box newBox = new Box(box.x, box.y, box.id, box.color);
                newBox.assignedGoal = box.assignedGoal;
                copy.boxList.Add(t, newBox);
            }

            foreach (Agent agent in this.agentList.Values)
            {
                Tuple<int, int> t = Tuple.Create(agent.x, agent.y);
                Agent newAgent = new Agent(agent.x, agent.y, agent.id, agent.color);
                copy.agentList.Add(t, newAgent);
            }

            for (int row = 0; row < MAX_ROW; row++)
            {
                Array.Copy(this.boxes[row], copy.boxes[row], MAX_COL);
            }
            return copy;
        }

        public List<NodeMultiBody> extractPlan()
        {
            List<NodeMultiBody> plan = new List<Node>();
            NodeMultiBody n = this;
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
                result = prime * result + ((IStructuralEquatable)this.boxes.SelectMany(x => x).ToArray()).GetHashCode(EqualityComparer<char>.Default);

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
            NodeMultiBody other = (Node)obj;
            if (this.agentRow != other.agentRow || this.agentCol != other.agentCol)
                return false;
            if (!this.boxes.SelectMany(x => x).SequenceEqual(other.boxes.SelectMany(x => x)))
                return false;

            //if (!this.goals.Cast<char>().SequenceEqual(other.goals.Cast<char>()))
            //	return false;

            //if (!this.walls.Cast<char>().SequenceEqual(other.walls.Cast<char>()))
            //	return false;
            return true;
        }


        public override string ToString()
        {
            StringBuilder s = new StringBuilder();
            for (int row = 0; row < MAX_ROW; row++)
            {
                if (!this.walls[row][0])
                {
                    break;
                }
                for (int col = 0; col < MAX_COL; col++)
                {
                    if (this.boxes[row][col] > 0)
                    {
                        s.Append(this.boxes[row][col]);
                    }
                    else if (this.goals[row][col] > 0)
                    {
                        s.Append(this.goals[row][col]);
                    }
                    else if (this.walls[row][col])
                    {
                        s.Append("+");
                    }
                    else if (row == this.agentRow && col == this.agentCol)
                    {
                        s.Append("0");
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