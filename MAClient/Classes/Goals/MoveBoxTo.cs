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
        public DistanceMap dm;

        public MoveBoxTo(IEntity box, IEntity position, int owner) : base(owner)
        {
            this.box = box;
            this.position = position;
        }


        public void CreateDistanceMap()
        {
            this.dm = new DistanceMap(this.position.col, this.position.row, SearchClient.CurrentNode);
            while (dm.frontier.Count != 0)
            {
                dm.Expand();
            }
        }

        public override bool IsGoalState(Node n)
        {
            Box box = n.boxList[this.box.uid];
            return (box.col == position.col && box.row == position.row);
        }

        public override int heuristicScore(Node n)
        {
            Agent agent = n.agentList[n.agentCol, n.agentRow];
            Box box = n.boxList[this.box.uid];
            //int moveToDist = Math.Abs(box.col - this.position.col) + Math.Abs(box.row - this.position.row);
            int moveToDist = this.dm.distanceMap[box.col, box.row] - 2;
            int moveDist = Math.Abs(agent.col - box.col) + Math.Abs(agent.row - box.row);
            return moveToDist + moveDist;
        }

    }
}
