using System;
using System.Collections.Generic;
namespace SAClient.Classes
{
	public abstract class Heuristic : System.Collections.Generic.IComparer<Node>
	{
		public int maxDist;
		public int goalReward;

		public Heuristic(Node initialState)
		{
			// maxDist = maximum manhatten distance between any two points in the map
			this.maxDist = initialState.goals.Length + initialState.goals[0].Length;

			// initialize boxes
			int x = 0;
			int y = 0;
			foreach (char[] boxX in initialState.boxes)
			{
				x = 0;
				foreach (char boxY in boxX)
				{
					if (boxY != 0)
					{
						Tuple t = new Tuple(x, y);
						initialState.boxList.Add(t, new Box(x,y,boxY, ));
					}
					x++;
				}
				y++;
			}

			y = 0;
			// initialize goals
			foreach (char[] goalX in initialState.goals)
			{
				x = 0;
				foreach (char goalY in goalX)
				{
					if (goalY != 0)
					{
						Tuple t = new Tuple(x, y);
						Node.goalList.Add(t, new Goal(x,y, goalY));
					}
					x++;
				}
				y++;
			}
			this.goalReward = int.MaxValue / (Node.goalList.Count + 1);

			Dictionary<Tuple, Tuple> boxList = new Dictionary<Tuple, Tuple>();
			foreach (Tuple box in initialState.boxList.Keys)
			{
				boxList.Add(box, box);
			}
			/// Find the closest box for the respective goal
			foreach (Tuple goal in Node.goalList.Keys)
			{
				int minBoxDist = this.maxDist;
				Tuple _box = null;
				foreach (Tuple box in boxList.Keys)
				{
					int boxDist = Math.Abs(box.x - goal.x) + Math.Abs(box.y - goal.y);
					if (boxDist < minBoxDist)
					{
						minBoxDist = boxDist;
						_box = box;
					}
				}

				_box.assignGoal(goal);
				boxList.Remove(_box);
			}
		}

		public int h(Node n)
		{
			int score = 0;
			int closestProblemDist = this.maxDist;
			int closestProblemGoalDist = 0;
			foreach (Tuple box in n.boxList.Keys)
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
					// Calculate the distance required to move towards it
					int moveDist = Math.Abs(n.agentCol - box.x) + Math.Abs(n.agentRow - box.y);
					if (moveDist < closestProblemDist)
					{
						closestProblemDist = moveDist;
						closestProblemGoalDist = box.goalDistance();
					}
				}
			}

			// yield score for the goal/box closest to the agent
			score += closestProblemGoalDist + closestProblemDist;
			return score;
		}

		public abstract int f(Node n);

		public int Compare(Node x, Node y)
		{
			return this.f(x) - this.f(y);
		}
	}
}
