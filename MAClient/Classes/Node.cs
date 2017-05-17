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
using MAClient.Classes.Entities;

namespace MAClient.Classes
{
    public class Node
    {
        private static readonly Random RND = new Random(1); // if you don't want the same seed every time: System.Environment.TickCount()
        public static int MAX_ROW;
        public static int MAX_COL;
        public int fitness;
        public bool hasFitness = false;
        public EntityList<Box> boxList;
        public static EntityList<Goal> goalList;
        public static EntityList<Position> wallList;
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
            wallList = new EntityList<Position>(MAX_COL, MAX_ROW);
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
                Box box = boxList[goal.col, goal.row];
                if (box == null || goal.id != char.ToLower(box.id))
                {
                    return false;
                }
            }
            return true;
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
                    {
                        Node n = this.ChildNode();
                        n.agentList.UpdatePosition(agentCol, agentRow, newAgentCol, newAgentRow);
                        n.action = c;
                        n.agentCol = newAgentCol;
                        n.agentRow = newAgentRow;
                        expandedNodes.Add(n);
                    }
                }
                else if (c.actionType == ActionType.Push)
                {
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
                            n.boxList.UpdatePosition(newAgentCol, newAgentRow, newBoxCol, newBoxRow);
                            expandedNodes.Add(n);
                        }
                    }
                }
                else if (c.actionType == ActionType.Pull)
                {
                    int boxRow = this.agentRow + Command.dirToRowChange(c.dir2.Value);
                    int boxCol = this.agentCol + Command.dirToColChange(c.dir2.Value);
                    // .. and there's a box in "dir2" of the agent
                    Box bb = getBox(boxCol, boxRow);
                    if (this.boxAt(boxCol, boxRow) && bb.color == agent.color)
                    {
                        // Cell is free where agent is going
                        if (this.cellIsFree(newAgentCol, newAgentRow))
                        {
                            Node n = this.ChildNode();
                            n.agentList.UpdatePosition(agentCol, agentRow, newAgentCol, newAgentRow);
                            n.action = c;
                            n.agentRow = newAgentRow;
                            n.agentCol = newAgentCol;
                            n.boxList.UpdatePosition(boxCol, boxRow, this.agentCol, this.agentRow);
                            expandedNodes.Add(n);
                        }
                    }
                }
            }

            return expandedNodes.OrderBy(item => RND.Next()).ToList();
        }

        public IEntity ValidateAction(Command c, int agentCol, int agentRow)
        {
            bool canValidate = false;
            int col = -1;
            int row = -1;
            int newAgentRow = agentRow + Command.dirToRowChange(c.dir1);
            int newAgentCol = agentCol + Command.dirToColChange(c.dir1);
            Agent agent = this.agentList[agentCol, agentRow];

            switch (c.actionType)
            {
                case ActionType.Move:
                    col = newAgentCol;
                    row = newAgentRow;
                    canValidate = true;
                    break;

                case ActionType.Push:
                    canValidate = this.HasExpectedBox(newAgentCol, newAgentRow, agent.color);
                    col = canValidate ? newAgentCol + Command.dirToColChange(c.dir2.Value) : newAgentCol;
                    row = canValidate ? newAgentRow + Command.dirToRowChange(c.dir2.Value) : newAgentRow;
                    break;

                case ActionType.Pull:
                    int boxRow = agentRow + Command.dirToRowChange(c.dir2.Value);
                    int boxCol = agentCol + Command.dirToColChange(c.dir2.Value);

                    canValidate = this.HasExpectedBox(boxCol, boxRow, agent.color);
                    col = canValidate ? newAgentCol : boxCol;
                    row = canValidate ? newAgentRow : boxRow;
                    break;
                case ActionType.NoOp:
                    //All is fine and dandy
                    return null;
            }

            if (canValidate)
            {
                Node ancestor = this.FindAncestor(this.agentList.Count - 1);
                bool ancestorFree = ancestor.cellIsFree(col, row);
                bool currentNodeFree = this.cellIsFree(col, row);
                if (!currentNodeFree)
                {
                    return this.GetEntityAt(col, row);
                }
                else if (!ancestorFree)
                {
                    return ancestor.GetEntityAt(col, row);
                }
                return null;
            }
            // box is no longer at expected position
            else
            {
                int boxUid = agent.CurrentBeliefs.parent.boxList[col, row].uid;
                return boxList[boxUid];
            }

            // not a valid action. return false.
            throw new Exception("could not validate action, but no agent or box found.");
        }

        private Node FindAncestor(int level)
        {
            Node n = this;
            for (int cnt = 0; cnt < level; cnt++)
            {
                if (n.parent == null) break;
                n = n.parent;
            }
            return n;
        }

        bool HasExpectedBox(int col, int row, string color)
        {
            Box box = getBox(col, row);
            return box != null && box.color == color;
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

            return null;
        }

        private bool cellIsFree(int col, int row)
        {
            return (wallList[col, row] == null && boxList[col, row] == null && (agentList[col, row] == null)); //|| (agentCol == col && agentRow == row && agentList[agentCol,agentRow].uid == agentList[col, row].uid)
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

        public Plan extractPlan()
        {
            return new Plan(this);
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
                //if (wallList[0, row] == null)
                //{
                //    break;
                //}
                for (int col = 0; col < MAX_COL; col++)
                {
                    if (boxList[col, row] != null)
                    {
                        s.Append(boxList[col, row].id);
                    }
                    else if (goalList[col, row] != null)
                    {
                        s.Append(goalList[col, row].id);
                    }
                    else if (wallList[col, row] != null)
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