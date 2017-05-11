using Common.Interfaces;
using MAClient.Classes.Entities;
using MAClient.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MAClient.Classes.Goals
{
    public class MoveAway : SubGoal
    {
        public IEntity[] entities;
        public List<IEntity> path;
        public int creator;
        public Position startPos;
        public MoveAway(IEntity[] entities, List<IEntity> path, int creator, int owner) : base(owner)
        {
            this.entities = entities;
            this.path = path;
            this.creator = creator;
        }

        public override bool IsGoalState(Node n)
        {
            bool isInGoalState = true;
            foreach (IEntity entity in entities)
            {
                IEntity obstacle = entity is Agent ? (IEntity)n.agentList[entity.uid] : n.boxList[entity.uid];
                isInGoalState &= !this.path.Contains(new Position(obstacle.col, obstacle.row));
            }
            return isInGoalState;
        }
        public override int heuristicScore(Node n)
        {
            // prioritize movement that takes the agent away from the requesting agents position
            //Agent agent = n.agentList[n.agentCol, n.agentRow];
            //return (-1)*(Math.Abs(agent.col - this.startPos.col) + Math.Abs(agent.row - this.startPos.row));
            return 0;
        }
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (this == obj)
            {
                return true;
            }
            if (!(obj is MoveAway))
            {
                return false;
            }
            MoveAway otherSubgoal = (MoveAway)obj;
            return (otherSubgoal.creator == this.creator && this.entities.Length == otherSubgoal.entities.Length && this.entities.All(e => otherSubgoal.entities.Contains(e)));
        }
    }
}
