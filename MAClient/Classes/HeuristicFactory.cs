using System;

namespace MAClient.Classes
{

    public class HeuristicFactory
    {
        private static string heuristic;
        public static void Initialize(string arg)
        {
            heuristic = arg;
        }

        public static Heuristic Create(Node n)
        {
            switch (heuristic.ToLower().Trim())
            {
                case "-astar": return new AStar(n);
                case "-greedy": return new Greedy(n);
            }
            throw new Exception("Unknown heuristic");
        }
    }
}
