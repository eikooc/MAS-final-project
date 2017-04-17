using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;
using System.Linq;
using System.Collections;
using MAClient.Enumerations;
using System.Diagnostics;
using Common.Classes;

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


        public Dictionary<Tuple<int, int>, Box> boxList;
        public static Dictionary<Tuple<int, int>, Goal> goalList;
        public static Dictionary<Tuple<int, int>, bool> wallList;
        public Dictionary<Tuple<int, int>, Agent> agentList;

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
            this.boxList = new Dictionary<Tuple<int, int>, Box>();
            goalList = new Dictionary<Tuple<int, int>, Goal>();
            wallList = new Dictionary<Tuple<int, int>, bool>();
            this.agentList = new Dictionary<Tuple<int, int>, Agent>();



        }

        public Node(Node parent, Tuple<int,int> pos) : this(parent)
        {
            this.boxList = new Dictionary<Tuple<int, int>, Box>();
            this.agentList = new Dictionary<Tuple<int, int>, Agent>();
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
            foreach (Goal goal in goalList.Values)
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

        public bool isSubGoalState(SubGoal subGoal)
        {
            if (subGoal.type == SubGoalType.MoveBoxTo)
            {
                Box box = boxList.Values.Where(x => x.uid == subGoal.box.uid).FirstOrDefault();
                if(box == null)
                {
                    throw new Exception("box uid does not exist");
                }

                if (box.x == subGoal.pos.Item1 && box.y == subGoal.pos.Item2) return true;
            }
            else if (subGoal.type == SubGoalType.MoveAgentTo)
            {
                if ((Math.Abs(agentCol - subGoal.pos.Item1) + Math.Abs(agentRow - subGoal.pos.Item2)) == 1) return true;
            }
            return false;
        }

        public List<Node> getExpandedNodes()
        {
            Tuple<int, int> oldPos = Tuple.Create(agentCol, agentRow);
            Agent agent = this.agentList[oldPos];
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
                        UpdateAgentList(agentCol, agentRow, newAgentCol, newAgentRow, n);
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
                            UpdateAgentList(agentCol, agentRow, newAgentCol, newAgentRow, n);
                            n.action = c;
                            n.agentCol = newAgentCol;
                            n.agentRow = newAgentRow;
                            Box b = n.getBox(newAgentCol, newAgentRow);

                            if (b != null)
                            {
                                b.x = newBoxCol;
                                b.y = newBoxRow;

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
                    if (this.cellIsFree(newAgentCol, newAgentRow))
                    {
                        int boxRow = this.agentRow + Command.dirToRowChange(c.dir2.Value);
                        int boxCol = this.agentCol + Command.dirToColChange(c.dir2.Value);
                        // .. and there's a box in "dir2" of the agent
                        Box bb = getBox(boxCol, boxRow);
                        if (this.boxAt(boxCol, boxRow) && bb.color == agent.color)
                        {
                            Node n = this.ChildNode();
                            UpdateAgentList(agentCol, agentRow, newAgentCol, newAgentRow, n);

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

        public bool ValidateAction(Command c, int agentCol, int agentRow)
        {
            Tuple<int, int> oldPos = Tuple.Create(agentCol, agentRow);
            Agent agent = this.agentList[oldPos];
            // Determine applicability of action
            int newAgentRow = agentRow + Command.dirToRowChange(c.dir1);
            int newAgentCol = agentCol + Command.dirToColChange(c.dir1);

            if (c.actionType == ActionType.Move)
            {
                // Check if there's a wall or box on the cell to which the agent is moving
                if (this.cellIsFree(newAgentCol, newAgentRow))
                {
                    return true;
                }
            }
            else if (c.actionType == ActionType.Push)
            {

                //Debugger.Launch();
                // Make sure that there's actually a box to move
                Box bb = getBox(newAgentCol, newAgentRow);
                if (this.boxAt(newAgentCol, newAgentRow) && bb.color == agent.color)
                {
                    int newBoxRow = newAgentRow + Command.dirToRowChange(c.dir2.Value);
                    int newBoxCol = newAgentCol + Command.dirToColChange(c.dir2.Value);
                    // .. and that new cell of box is free
                    if (this.cellIsFree(newBoxCol, newBoxRow))
                    {
                        return true;
                    }
                }
            }
            else if (c.actionType == ActionType.Pull)
            {
                // Cell is free where agent is going
                if (this.cellIsFree(newAgentCol, newAgentRow))
                {
                    int boxRow = agentRow + Command.dirToRowChange(c.dir2.Value);
                    int boxCol = agentCol + Command.dirToColChange(c.dir2.Value);
                    // .. and there's a box in "dir2" of the agent
                    Box bb = getBox(boxCol, boxRow);
                    if (this.boxAt(boxCol, boxRow) && bb.color == agent.color)
                    {
                        return true;
                    }
                }
            }
            // not a valid action. return false.
            return false;
        }

        private static void UpdateAgentList(int agentCol, int agentRow, int newAgentCol, int newAgentRow, Node n)
        {
            Tuple<int, int> oldPos = Tuple.Create(agentCol, agentRow);
            Agent agent = n.agentList[oldPos];
            n.agentList.Remove(oldPos);
            agent.x = newAgentCol;
            agent.y = newAgentRow;
            n.agentList.Add(Tuple.Create(newAgentCol, newAgentRow), agent);
        }

        private static void UpdateBoxList(int boxCol, int boxRow, int newBoxCol, int newBoxRow, Node n)
        {
            Tuple<int, int> oldPos = Tuple.Create(boxCol, boxRow);
            Box box = n.boxList[oldPos];
            n.boxList.Remove(oldPos);
            box.x = newBoxCol;
            box.y = newBoxRow;
            n.boxList.Add(Tuple.Create(newBoxCol, newBoxRow), box);
        }

        private bool cellIsFree(int col, int row)
        {
            Tuple<int, int> pos = Tuple.Create(col, row);

            return (!wallList.ContainsKey(pos) && !boxList.ContainsKey(pos) && !agentList.ContainsKey(pos));
        }

        public bool boxAt(int x, int y)
        {
            Tuple<int, int> pos = Tuple.Create(x, y);
            return this.boxList.ContainsKey(pos);
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

        public Node ChildNode()
        {
            Node copy = new Node(this);
            copy.agentCol = this.agentCol;
            copy.agentRow = this.agentRow;
            foreach (Box box in boxList.Values)
            {
                Tuple<int, int> t = Tuple.Create(box.x, box.y);
                Box newBox = new Box(box.x, box.y, box.id, box.color, box);
                newBox.assignedGoal = box.assignedGoal;
                copy.boxList.Add(t, newBox);
            }
            

            foreach (Agent agent in agentList.Values)
            {
                Tuple<int, int> t = Tuple.Create(agent.x, agent.y);
                Agent newAgent = new Agent(agent.x, agent.y, agent.id, agent.color);
                copy.agentList.Add(t, newAgent);
                newAgent.subgoals = agent.subgoals;
                newAgent.plan = agent.plan;
                newAgent.strategy = agent.strategy;
                newAgent.CurrentBeliefs = agent.CurrentBeliefs;
            }

            return copy;
        }

        public Node copyNode()
        {
            Node copy = new Node(this.parent);

            foreach (Box box in boxList.Values)
            {
                Tuple<int, int> t = Tuple.Create(box.x, box.y);
                Box newBox = new Box(box.x, box.y, box.id, box.color, box);
                newBox.assignedGoal = box.assignedGoal;
                copy.boxList.Add(t, newBox);
            }
            

            foreach (Agent agent in agentList.Values)
            {
                Tuple<int, int> t = Tuple.Create(agent.x, agent.y);
                Agent newAgent = new Agent(agent.x, agent.y, agent.id, agent.color);
                copy.agentList.Add(t, newAgent);
                newAgent.subgoals = agent.subgoals;
                newAgent.plan = agent.plan;
                newAgent.strategy = agent.strategy;
                newAgent.CurrentBeliefs = agent.CurrentBeliefs;
            }

            return copy;
        }

        public void updateNode(Node otherNode,Tuple<int,int> oldAgentPos)
        {
            UpdateAgentList(oldAgentPos.Item1, oldAgentPos.Item2, otherNode.agentCol, otherNode.agentRow, this);
            this.action = otherNode.action;
            this.agentCol = otherNode.agentCol;
            this.agentRow = otherNode.agentRow;
            if (otherNode.action.actionType == ActionType.Pull)
            {
                int deltaBoxX = Command.dirToColChange(otherNode.action.dir2.Value);
                int deltaBoxY = Command.dirToRowChange(otherNode.action.dir2.Value);
                UpdateBoxList(oldAgentPos.Item1 + deltaBoxX, oldAgentPos.Item2 + deltaBoxY, oldAgentPos.Item1, oldAgentPos.Item2, this);
            }
            else if (otherNode.action.actionType == ActionType.Push)
            {
                int deltaBoxX = Command.dirToColChange(otherNode.action.dir2.Value);
                int deltaBoxY = Command.dirToRowChange(otherNode.action.dir2.Value);
                UpdateBoxList(otherNode.agentCol, otherNode.agentRow, otherNode.agentCol + deltaBoxX, otherNode.agentRow + deltaBoxY, this);
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
                int boxHash = ((IStructuralEquatable)this.boxList.Values.ToArray()).GetHashCode(EqualityComparer<Box>.Default);
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