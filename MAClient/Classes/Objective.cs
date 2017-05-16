namespace MAClient.Classes
{

    public class Objective
    {
        public SubGoal MoveBoxTo { get; private set; }
        public SubGoal MoveAgentTo { get; private set; }

        public Objective(SubGoal moveBoxTo, SubGoal moveAgentTo)
        {
            this.MoveBoxTo = moveBoxTo;
            this.MoveAgentTo = moveAgentTo;
        }
    }
}
