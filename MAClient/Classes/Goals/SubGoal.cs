using MAClient.Enumerations;
using MAClient.Interfaces;

namespace MAClient.Classes
{


    public abstract class SubGoal : ISubGoal
    {
        public SubGoalType type;
        public bool completed;
        
        public SubGoal(SubGoalType type)
        {
            this.type = type;
            this.completed = false;
        }

        public abstract bool IsSolved(Node n);
        public abstract int heuristicScore(Node n);
    }
}


