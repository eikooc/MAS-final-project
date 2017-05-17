using MAClient.Enumerations;
using MAClient.Interfaces;

namespace MAClient.Classes
{

    public abstract class SubGoal : ISubGoal
    {
        public bool completed;
        public bool failed;
        public int owner;
        public int dependencyOrder;

        public SubGoal(int owner)
        {
            this.failed = false;
            this.completed = false;
            this.owner = owner;
            this.dependencyOrder = 0;
        }

        public abstract bool IsGoalState(Node n);
        public abstract int heuristicScore(Node n);

        public void UpdateState(Node n)
        {
            this.completed = this.IsGoalState(n);
        }
    }
}


