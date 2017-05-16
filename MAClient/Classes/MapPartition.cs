using Common.Classes;
using Common.Interfaces;
using MAClient.Classes;
using MAClient.Classes.Entities;
using MAClient.Classes.Goals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAClient.Classes
{
    class MapPartition
    {
        public MapPartition(int maxCol, int maxRow)
        {
            this.Agents = new EntityList<Agent>(maxCol, maxRow);
            this.Boxes = new EntityList<Box>(maxCol, maxRow);
            this.Goals = new EntityList<Goal>(maxCol, maxRow);
            this.MoveToBoxSG = new List<MoveBoxTo>();
        }

        public EntityList<Agent> Agents { get; private set; }
        public EntityList<Box> Boxes { get; private set; }
        public EntityList<Goal> Goals { get; private set; }

        public List<MoveBoxTo> MoveToBoxSG { get; private set; }

        public void AddAgent(Agent agent)
        {
            this.Agents.Add(agent);
        }

        public void AddBox(Box box)
        {
            this.Boxes.Add(box);
        }

        public void AddGoal(Goal goal)
        {
            this.Goals.Add(goal);
        }

        public void ProcessPartition()
        {
            foreach (Goal goal in this.Goals.Entities)
            {
                int goalBoxDist = int.MaxValue;
                Box candidateBox = null;
                foreach (Box box in this.Boxes.Entities)
                {
                    if (box.hasGoal() || char.ToLower(box.id) != goal.id) continue;
                    int dist = Dist(goal, box);
                    if (dist < goalBoxDist)
                    {
                        goalBoxDist = dist;
                        candidateBox = box;
                    }
                }
                candidateBox.assignGoal(goal);
                this.MoveToBoxSG.Add(new MoveBoxTo(candidateBox, new Position(candidateBox.assignedGoal.col, candidateBox.assignedGoal.row), -1));
            }
        }

        public bool HasAgent(int id)
        {
            return this.Agents[id] != null;
        }

        public bool HasBox(int id)
        {
            return this.Boxes[id] != null;
        }

        public bool HasGoal(int id)
        {
            return this.Goals[id] != null;
        }

        public Objective GetObjective(Agent agent, Node currentNode) // Node for goalstate check
        {
            if (this.HasAgent(agent.uid))
            {
                int boxAgentDist = int.MaxValue;
                MoveBoxTo candidateSG = null;
                foreach (MoveBoxTo subgoal in this.MoveToBoxSG)
                {
                    if (((Box)subgoal.box).color != agent.color || subgoal.owner != -1 || subgoal.IsGoalState(currentNode)) continue;
                    int dist = Dist(subgoal.box, agent);
                    if (dist < boxAgentDist)
                    {
                        boxAgentDist = dist;
                        candidateSG = subgoal;
                    }
                }
                if (candidateSG != null)
                {
                    return new Objective(candidateSG, new MoveAgentTo(candidateSG.box, agent.uid));
                }
            }
            return null;
        }

        private int Dist(IEntity e1, IEntity e2)
        {
            return Math.Abs(e1.col - e2.col) + Math.Abs(e1.row - e2.row);
        }
    }
}
