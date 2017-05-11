using MAClient.Classes;

namespace MAClient.Interfaces
{
    public interface ISubGoal
    {
        bool IsGoalState(Node n);
        int heuristicScore(Node n);
    }
}
