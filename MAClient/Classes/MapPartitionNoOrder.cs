using MAClient.Classes.Entities;
using MAClient.Classes.Goals;
using System.Linq;

namespace MAClient.Classes
{
    public class MapPartitionNoOrder : MapPartition
    {
        public MapPartitionNoOrder(int maxCol, int maxRow) : base(maxCol, maxRow)
        {
        }

        public override Objective GetObjective(Agent agent, Node currentNode) // Node for goalstate check
        {
            if (this.HasAgent(agent.uid))
            {
                int boxAgentDist = int.MaxValue;
                MoveBoxTo candidateSG = null;
                foreach (MoveBoxTo subgoal in this.MoveToBoxSG.Where(x => ((Box)x.box).color == agent.color && x.owner == -1 && !x.IsGoalState(currentNode)))
                {
                    int dist = Dist(subgoal.box, agent);
                    if (dist < boxAgentDist)
                    {
                        boxAgentDist = dist;
                        candidateSG = subgoal;
                    }
                }
                if (candidateSG != null)
                {
                    Box box = SearchClient.CurrentNode.boxList[candidateSG.box.uid];
                    candidateSG.box = box;
                    return new Objective(candidateSG, new MoveAgentTo(candidateSG.box, agent.uid));
                }
            }
            return null;
        }

    }
}
