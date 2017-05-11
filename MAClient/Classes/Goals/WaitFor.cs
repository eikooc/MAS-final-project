using Common.Interfaces;
using MAClient.Classes.Entities;
using MAClient.Enumerations;
using System.Collections.Generic;

namespace MAClient.Classes.Goals
{
    public class WaitFor : SubGoal
    {
        public SubGoal dependency;

        public WaitFor(SubGoal dependency, int owner) : base(owner)
        {
            this.dependency = dependency;
        }

        public override bool IsGoalState(Node n)
        {
            return dependency.completed;
        }
        public override int heuristicScore(Node n)
        {
            return 0;
        }

    }
}
