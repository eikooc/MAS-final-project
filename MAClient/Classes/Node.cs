using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;
using System.Linq;
using System.Collections;
using MAClient.Enumerations;
using System.Diagnostics;
using Common.Classes;
using Common.Interfaces;

namespace MAClient.Classes
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


        public EntityList<Box> boxList;
        public static EntityList<Goal> goalList;
        public static Dictionary<Tuple<int, int>, bool> wallList;
        public EntityList<Agent> agentList;

        public int agentRow;
        public int agentCol;


        public Node parent;
        public Command action;

        private int _g;

        private int _hash = 0;

        public Node(Node parent, int row, int col) : this(parent)
        {

            MAX_COL = col;
            MAX_ROW = row;
            this.boxList = new EntityList<Box>(MAX_COL, MAX_ROW);
            goalList = new EntityList<Goal>(MAX_COL, MAX_ROW);
            wallList = new Dictionary<Tuple<int, int>, bool>();
            this.agentList = new EntityList<Agent>(MAX_COL, MAX_ROW);



        }

        public Node(Node parent, Tuple<int, int> pos) : this(parent)
        {
            this.boxList = new EntityList<Box>(MAX_COL, MAX_ROW);
            this.agentList = new EntityList<Agent>(MAX_COL, MAX_ROW);
            agentCol = pos.Item1;
            agentRow = pos.Item2;
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
            this.boxList = new EntityList<Box>(MAX_COL, MAX_ROW);
            this.agentList = new EntityList<Agent>(MAX_COL, MAX_ROW);
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
            foreach (Goal goal in goalList.Entities)
            {
                if (boxList[goal.col, goal.row] != null)
                {
                    if (goal.id == char.ToLower(boxList[goal.col, goal.row].id)) continue;
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

        public bool isSubGoalState(SubGoal subGoal)
        {
            if (subGoal.type == SubGoalType.MoveBoxTo)
            {
                Box box = boxList[subGoal.box.uid];
                if (box == null)
                {
                    throw new Exception("box uid does not exist");
                }

                if (box.col == subGoal.pos.Item1 && box.row == subGoal.pos.Item2) return true;
            }
            else if (subGoal.type == SubGoalType.MoveAgentTo)
            {
                if ((Math.Abs(agentCol - subGoal.pos.Item1) + Math.Abs(agentRow - subGoal.pos.Item2)) == 1) return true;
            }
            return false;
        }

        public List<Node> getExpandedNodes()
        {
            Agent agent = this.agentList[agentCol, agentRow];
            List<Node> expandedNodes = new List<Node>(Command.EVERY.Length);
            foreach (Command c in Command.EVERY)
            {
                // Determine applicability of action
                int newAgentCol = agentCol + Command.dirToColChange(c.dir1);
                int newAgentRow = agentRow + Command.dirToRowChange(c.dir1);

                if (c.actionType == ActionType.Move)
                {
                    // Check if there's a wall or box on the cell to which the agent is moving
                    if (this.cellIsFree(newAgentCol, newAgentRow))
                    { // O(m)
                        Node n = this.ChildNode(); // gl: O(n),  ny: O(m), 
                        n.agentList.UpdatePosition(agentCol, agentRow, newAgentCol, newAgentRow);
                        n.action = c;
                        n.agentCol = newAgentCol;
                        n.agentRow = newAgentRow;
                        expandedNodes.Add(n);
                    }
                }
                else if (c.actionType == ActionType.Push)
                {

                    //Debugger.Launch();
                    // Make sure that there's actually a box to move
                    Box bb = getBox(newAgentCol, newAgentRow);
                    if (this.boxAt(newAgentCol, newAgentRow) && bb.color == agent.color)
                    {
                        int newBoxCol = newAgentCol + Command.dirToColChange(c.dir2.Value);
                        int newBoxRow = newAgentRow + Command.dirToRowChange(c.dir2.Value);
                        // .. and that new cell of box is free
                        if (this.cellIsFree(newBoxCol, newBoxRow))
                        {
                            Node n = this.ChildNode();
                            n.agentList.UpdatePosition(agentCol, agentRow, newAgentCol, newAgentRow);
                            n.action = c;
                            n.agentCol = newAgentCol;
                            n.agentRow = newAgentRow;
                            Box b = n.getBox(newAgentCol, newAgentRow);

                            if (b != null)
                            {
                                n.boxList.UpdatePosition(newAgentCol, newAgentRow, newBoxCol, newBoxRow);

                            }
                            expandedNodes.Add(n);
                        }
                    }
                }
                else if (c.actionType == ActionType.Pull)
                {
                    // Cell is free where agent is going
                    if (this.cellIsFree(newAgentCol, newAgentRow))
                    {
                        int boxRow = this.agentRow + Command.dirToRowChange(c.dir2.Value);
                        int boxCol = this.agentCol + Command.dirToColChange(c.dir2.Value);
                        // .. and there's a box in "dir2" of the agent
                        Box bb = getBox(boxCol, boxRow);
                        if (this.boxAt(boxCol, boxRow) && bb.color == agent.color)
                        {
                            Node n = this.ChildNode();
                            n.agentList.UpdatePosition(agentCol, agentRow, newAgentCol, newAgentRow);

                            n.action = c;
                            n.agentRow = newAgentRow;
                            n.agentCol = newAgentCol;
                            Box b = n.getBox(boxCol, boxRow);
                            if (b != null)
                            {
                                n.boxList.UpdatePosition(boxCol, boxRow, this.agentCol, this.agentRow);
                            }
                            expandedNodes.Add(n);
                        }
                    }
                }
            }


            // Collections.shuffle(expandedNodes, RND);
            return expandedNodes.OrderBy(item => RND.Next()).ToList();
        }

        public IEntity ValidateAction(Command c, int agentCol, int agentRow)
        {
            Agent agent = this.agentList[agentCol, agentRow];
            // Determine applicability of action
            int newAgentRow = agentRow + Command.dirToRowChange(c.dir1);
            int newAgentCol = agentCol + Command.dirToColChange(c.dir1);

            int col = -1;
            int row = -1;

            if (c.actionType == ActionType.Move)
            {
                // Check if there's a wall or box on the cell to which the agent is moving
                col = newAgentCol;
                row = newAgentRow;
            }
            else if (c.actionType == ActionType.Push)
            {
                // Make sure that there's actually a box to move
                Box box = getBox(newAgentCol, newAgentRow);
                if (box != null && box.color == agent.color)
                {
                    col = newAgentCol + Command.dirToColChange(c.dir2.Value);
                    row = newAgentRow + Command.dirToRowChange(c.dir2.Value);
                }
                else
                {
                    // box is no longer at expected position
                    int boxUid = agent.CurrentBeliefs.parent.boxList[newAgentCol, newAgentRow].uid;
                    return boxList[boxUid];
                }
            }
            else if (c.actionType == ActionType.Pull)
            {
                // Cell is free where agent is going and there's a box in "dir2" of the agent
                int boxRow = agentRow + Command.dirToRowChange(c.dir2.Value);
                int boxCol = agentCol + Command.dirToColChange(c.dir2.Value);
                Box box = getBox(boxCol, boxRow);
                if (box != null && box.color == agent.color)
                {
                    col = newAgentCol;
                    row = newAgentRow;
                }
                else
                {
                    // box is no longer at expected position
                    int boxUid = agent.CurrentBeliefs.parent.boxList[boxCol, boxRow].uid;
                    return boxList[boxUid];
                }

            }

            if (col > -1 && row > -1)
            {
                bool? b = parent?.cellIsFree(col, row);
                bool valid = (this.cellIsFree(col, row) && (b.HasValue && b.Value || !b.HasValue));
                return valid ? null : this.GetEntityAt(col, row);
            }

            // not a valid action. return false.
            throw new Exception("could not validate action, but no agent or box found.");

        }

        private IEntity GetEntityAt(int col, int row)
        {
            if (this.agentList[col, row] != null)
            {
                return this.agentList[col, row];
            }
            else if (this.boxList[col, row] != null)
            {
                return this.boxList[col, row];
            }
            else if (this.parent.agentList[col, row] != null)
            {
                return this.parent.agentList[col, row];
            }
            else if (this.parent.boxList[col, row] != null)
            {
                return this.parent.boxList[col, row];
            }

            return null;
        }


        private bool cellIsFree(int col, int row)
        {
            Tuple<int, int> pos = Tuple.Create(col, row);

            return (!wallList.ContainsKey(pos) && boxList[col, row] == null && agentList[col, row] == null);
        }

        public bool boxAt(int x, int y)
        {
            return this.boxList[x, y] != null;
        }

        public Box getBox(int x, int y)
        {
            return boxList[x, y];

        }

        public Node ChildNode()
        {
            Node copy = new Node(this);
            copy.agentCol = this.agentCol;
            copy.agentRow = this.agentRow;

            copy.boxList = this.boxList.Clone();

            copy.agentList = this.agentList.Clone();

            return copy;
        }

        public Node copyNode()
        {
            Node copy = new Node(this.parent);

            copy.boxList = this.boxList.Clone();

            copy.agentList = this.agentList.Clone();

            return copy;
        }

        public void updateNode(Node otherNode, int oldCol, int oldRow)
        {
            this.agentList.UpdatePosition(oldCol, oldRow, otherNode.agentCol, otherNode.agentRow);
            this.action = otherNode.action;
            this.agentCol = otherNode.agentCol;
            this.agentRow = otherNode.agentRow;
            if (otherNode.action.actionType == ActionType.Pull)
            {
                int deltaBoxX = Command.dirToColChange(otherNode.action.dir2.Value);
                int deltaBoxY = Command.dirToRowChange(otherNode.action.dir2.Value);
                this.boxList.UpdatePosition(oldCol + deltaBoxX, oldRow + deltaBoxY, oldCol, oldRow);
            }
            else if (otherNode.action.actionType == ActionType.Push)
            {
                int deltaBoxX = Command.dirToColChange(otherNode.action.dir2.Value);
                int deltaBoxY = Command.dirToRowChange(otherNode.action.dir2.Value);
                this.boxList.UpdatePosition(otherNode.agentCol, otherNode.agentRow, otherNode.agentCol + deltaBoxX, otherNode.agentRow + deltaBoxY);
            }
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
                int boxHash = ((IStructuralEquatable)this.boxList.Entities.ToArray()).GetHashCode(EqualityComparer<Box>.Default);
                result = prime * result + boxHash;

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
            if (!this.boxList.Entities.OrderBy(x => x.uid).SequenceEqual(other.boxList.Entities.OrderBy(x => x.uid)))
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
                    if (boxList[col, row] != null)
                    {
                        s.Append(boxList[col, row].id);
                    }
                    else if (goalList[col, row] != null)
                    {
                        s.Append(goalList[col, row].id);
                    }
                    else if (wallList.ContainsKey(pos))
                    {
                        s.Append("+");
                    }
                    else if (agentList[col, row] != null)
                    {
                        s.Append(agentList[col, row].uid);
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