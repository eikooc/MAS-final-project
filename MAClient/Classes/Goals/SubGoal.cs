﻿using Common.Interfaces;
using MAClient.Enumerations;
using MAClient.Interfaces;
using System.Collections.Generic;

namespace MAClient.Classes
{


    public abstract class SubGoal : ISubGoal
    {
        public bool completed;
        public bool failed;
        public int owner;
        public Stack<IEntity> EncounteredObjects { get; private set; }

        public SubGoal(int owner)
        {
            this.failed = false;
            this.completed = false;
            this.owner = owner;
            this.EncounteredObjects = new Stack<IEntity>();
        }

        public abstract bool IsGoalState(Node n);
        public abstract int heuristicScore(Node n);

        public void UpdateState(Node n)
        {
            this.completed = this.IsGoalState(n);
        }
    }
}


