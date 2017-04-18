﻿using MAClient.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MAClient.Classes
{
	public abstract class Heuristic : System.Collections.Generic.IComparer<Node>
	{
		public int maxDist;
		public int goalReward;

		public Heuristic(Node initialState)
		{
			// maxDist = maximum manhatten distance between any two points in the map
			this.maxDist = Node.MAX_ROW + Node.MAX_COL;

			// initialize boxes

			this.goalReward = int.MaxValue / (Node.goalList.Count + 1);

			Dictionary<Box, Box> boxList = new Dictionary<Box, Box>();
			foreach (Box box in initialState.boxList.Entities)
			{
				boxList.Add(box, box);
			}
			/// Find the closest box for the respective goal
			foreach (Goal goal in Node.goalList.Entities)
			{
				int minBoxDist = this.maxDist;
				Box _box = null;
				foreach (Box box in boxList.Values.Where(x=> char.ToLower(x.id) == goal.id))
				{
					int boxDist = Math.Abs(box.col - goal.col) + Math.Abs(box.row - goal.row);
					if (boxDist < minBoxDist)
					{
						minBoxDist = boxDist;
						_box = box;
					}
				}
                if(_box != null)
                {
                    _box.assignGoal(goal);
                    boxList.Remove(_box);
                }
			}
		}
        /*
		public int h(Node n)
		{
			int score = 0;
            foreach (Agent agent in n.agentList.Values)
            {
                int closestProblemDist = this.maxDist;
                int closestProblemGoalDist = 0;
                foreach (Box box in n.boxList.Values.Where(x=> x.color == agent.color))
                {
                    if (!box.hasGoal())
                        continue;

                    //if the box already in goal then reward it
                    if (box.inGoal())
                    {
                        score -= this.goalReward;
                    }
                    else
                    {

                        int moveDist = Math.Abs(agent.x - box.x) + Math.Abs(agent.y - box.y);

                        if (moveDist < closestProblemDist)
                        {
                            closestProblemDist = moveDist;
                            closestProblemGoalDist = box.goalDistance();
                        }
                    }
                }

                // yield score for the goal/box closest to the agent
                score += closestProblemGoalDist + closestProblemDist;

            }
            return score;
		}
        */

        public int h(Node n)
        {

            int score = 0;
            int maxSubgoalValue = int.MinValue / (Node.goalList.Count+1);
            Agent agent = n.agentList[n.agentCol, n.agentRow];
            SubGoal currentSubGoal = agent.subgoals.Peek();

            foreach (Box box in n.boxList.Entities)
            {
                if (box.goalDistance() == 0)
                {
                    score -= 1;
                }
            }
            if (currentSubGoal.type == SubGoalType.MoveAgentTo)
            {
                int moveDist = Math.Abs(agent.col - currentSubGoal.pos.Item1) + Math.Abs(agent.row - currentSubGoal.pos.Item2);
                score += moveDist;
            }
            else if (currentSubGoal.type == SubGoalType.MoveBoxTo)
            {
                Box box = n.boxList[currentSubGoal.box.uid];
                int moveToDist = Math.Abs(box.col - currentSubGoal.pos.Item1) + Math.Abs(box.row -currentSubGoal.pos.Item2);
                int moveDist = Math.Abs(agent.col - box.col) + Math.Abs(agent.row - box.row);
                score += moveToDist + moveDist;
            }
            else
            {
                throw new  InvalidOperationException("subGoal must have a type");
            }

        return score;
        }

        public abstract int f(Node n);

		public int Compare(Node x, Node y)
		{
			return this.f(x) - this.f(y);
		}
	}
}
