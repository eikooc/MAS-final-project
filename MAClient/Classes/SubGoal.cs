using MAClient.Enumerations;
using System;

namespace MAClient.Classes
{
    public class SubGoal
    {
        public SubGoalType type;
        public Box box;
        public Tuple<int,int> pos;
        public bool completed;
        public bool isAssigned;
        public SubGoal(SubGoalType type, Box box, Tuple<int, int> pos)
        {
            this.type = type;
            this.box = box;
            this.pos = pos;
            this.completed = false;
            this.isAssigned = false;
        }
    }
}