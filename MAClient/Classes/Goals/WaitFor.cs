using Common.Interfaces;
using MAClient.Classes.Entities;
using MAClient.Enumerations;
using System.Collections.Generic;

namespace MAClient.Classes.Goals
{
    public class WaitFor : SubGoal
    {
        public Objective dependency;

        public WaitFor(Objective dependency, int owner) : base(owner)
        {
            this.dependency = dependency;
        }

        public override bool IsGoalState(Node n)
        {
            return dependency.IsComplete;
        }
        public override int heuristicScore(Node n)
        {
            return 0;
        }

    }
}
