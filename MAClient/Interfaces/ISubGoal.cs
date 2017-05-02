using MAClient.Classes;

namespace MAClient.Interfaces
{
    public interface ISubGoal
    {
        bool IsSolved(Node n);
        int heuristicScore(Node n);
    }
}
