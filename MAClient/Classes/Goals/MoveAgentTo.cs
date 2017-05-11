using Common.Interfaces;
using MAClient.Classes.Entities;
using MAClient.Enumerations;
using System;

namespace MAClient.Classes.Goals
{
    public class MoveAgentTo : SubGoal
    {
        public IEntity position;

        public MoveAgentTo(IEntity position, int owner) : base(owner)
        {
            this.position = position;
        }

        public override bool IsGoalState(Node n)
        {
            Agent agent = n.agentList[this.owner];
            return ((Math.Abs(agent.col - this.position.col) + Math.Abs(agent.row - this.position.row)) == 1);
        }

        public override int heuristicScore(Node n)
        {
            Agent agent = n.agentList[n.agentCol, n.agentRow];
            return Math.Abs(agent.col - this.position.col) + Math.Abs(agent.row - this.position.row);
        }
    }
}
