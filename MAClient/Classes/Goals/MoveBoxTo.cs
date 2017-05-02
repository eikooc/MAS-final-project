using Common.Interfaces;
using MAClient.Classes.Entities;
using MAClient.Enumerations;
using System;

namespace MAClient.Classes.Goals
{

    public class MoveBoxTo : SubGoal
    {
        public IEntity box;
        public IEntity position;

        public MoveBoxTo(IEntity box, IEntity position) : base(SubGoalType.MoveBoxTo)
        {
            this.box = box;
            this.position = position;
        }

        public override bool IsSolved(Node n)
        {
            Box box = n.boxList[this.box.uid];
            return (box.col == position.col && box.row == position.row);
        }

        public override int heuristicScore(Node n)
        {
            Agent agent = n.agentList[n.agentCol, n.agentRow];
            Box box = n.boxList[this.box.uid];
            int moveToDist = Math.Abs(box.col - this.position.col) + Math.Abs(box.row - this.position.row);
            int moveDist = Math.Abs(agent.col - box.col) + Math.Abs(agent.row - box.row);
            return moveToDist + moveDist;
        }

    }
}
