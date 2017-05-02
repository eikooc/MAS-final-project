using Common.Interfaces;
using MAClient.Classes.Entities;
using MAClient.Enumerations;
using System.Collections.Generic;

namespace MAClient.Classes.Goals
{
    public class MoveAway : SubGoal
    {
        public IEntity entity;
        public List<IEntity> path;

        public MoveAway(IEntity entity, List<IEntity> path) : base(SubGoalType.MoveAgentAway)
        {
            this.entity = entity;
            this.path = path;
        }

        public override bool IsSolved(Node n)
        {
            return !this.path.Contains(new Position(n.agentCol, n.agentRow));
        }
        public override int heuristicScore(Node n)
        {
            return 0;
        }
    }
}
