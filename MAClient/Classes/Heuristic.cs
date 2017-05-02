using MAClient.Classes.Entities;
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
            score += currentSubGoal.heuristicScore(n);
            return score;
        }

        public abstract int f(Node n);

		public int Compare(Node x, Node y)
		{
			return this.f(x) - this.f(y);
		}
	}
}
