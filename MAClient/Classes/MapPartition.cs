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
            this.CompletionStateSpace = new HashSet<int[]>();
        }

        public EntityList<Agent> Agents { get; private set; }
        public EntityList<Box> Boxes { get; private set; }
        public EntityList<Goal> Goals { get; private set; }
        private HashSet<int[]> CompletionStateSpace;

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
                // MoveToBoxSG.Count indexes indicate wether an objective have been sovled. last two indexes indicate agent.uid and assigned objective
                //int[] objectiveState = new int[MoveToBoxSG.Count + 2];
                //for (int i = 0; i< MoveToBoxSG.Count; i++)
                //{
                //    if (MoveToBoxSG.ElementAt(i).IsGoalState(currentNode))
                //    {
                //        objectiveState[i] = 1;
                //    }
                //}
                //objectiveState[MoveToBoxSG.Count] = agent.uid;
                int boxAgentDist = int.MaxValue;
                MoveBoxTo candidateSG = null;
                List<MoveBoxTo>secondTier = new List<MoveBoxTo>();

                foreach (MoveBoxTo subgoal in this.MoveToBoxSG)
                {

                    //if (CompletionStateSpace.Contains(objectiveState))
                    //{
                    //    secondTier.Add(subgoal);
                    //    continue;
                    //}
                    if (((Box)subgoal.box).color != agent.color || subgoal.owner != -1 || subgoal.IsGoalState(currentNode)) continue;

                    //objectiveState[MoveToBoxSG.Count + 1] = MoveToBoxSG.IndexOf(subgoal);
                    int dist = Dist(subgoal.box, agent);
                    if (dist < boxAgentDist)
                    {
                        boxAgentDist = dist;
                        candidateSG = subgoal;
                    }
                }
                if (candidateSG != null)
                {
                    //CompletionStateSpace.Add(objectiveState);
                    return new Objective(candidateSG, new MoveAgentTo(candidateSG.box, agent.uid));
                }
                //else
                //{
                //    foreach (MoveBoxTo subgoal in secondTier)
                //    {

                //    }
                //}
            }
            return null;
        }

        private int Dist(IEntity e1, IEntity e2)
        {
            return Math.Abs(e1.col - e2.col) + Math.Abs(e1.row - e2.row);
        }
    }
}
