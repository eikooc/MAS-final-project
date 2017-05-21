using MAClient.Classes.Entities;
using MAClient.Classes.Goals;
using System.Linq;

namespace MAClient.Classes
{
    public class MapPartitionWithOrder : MapPartition
    {
        public MapPartitionWithOrder(int maxCol, int maxRow) : base(maxCol, maxRow)
        {
        }

        public override Objective GetObjective(Agent agent, Node currentNode) // Node for goalstate check
        {
            if (this.HasAgent(agent.uid))
            {
                int boxAgentDist = int.MaxValue;
                MoveBoxTo candidateSG = null;
                MoveBoxTo sg = this.MoveToBoxSG.Where(x=> ((Box)x.box).color == agent.color && x.owner == -1 && !x.IsGoalState(currentNode)).OrderBy(x => x.dependencyOrder).FirstOrDefault();
                if (sg != null)
                {
                    int order = sg.dependencyOrder;
                    foreach (MoveBoxTo subgoal in this.MoveToBoxSG.Where(x => x.dependencyOrder == order && ((Box)x.box).color == agent.color && x.owner == -1 && !x.IsGoalState(currentNode)))
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
                        candidateSG.dependencyOrder++;
                        return new Objective(candidateSG, new MoveAgentTo(candidateSG.box, agent.uid));
                    }
                }
            }
            return null;
        }

    }
}
