using Common.Classes;
using SAClient.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    class OrderPlanning
    {
        public Dictionary<char, List<char>> dependencies;
        public Dictionary<char, Dictionary<char, List<Tuple<int, int>>>> pathLookupTable;
        List<Command> cmds;
        public OrderPlanning(Node startingState)
        {
            dependencies = new Dictionary<char, List<char>>();
            pathLookupTable = new Dictionary<char, Dictionary<char, List<Tuple<int, int>>>>();
            cmds = new List<Command>();

            // add possible moves
            foreach (Dir d in Enum.GetValues(typeof(Dir)))
            {
                cmds.Add(new Command(d));
            }

            foreach (Agent agent in startingState.agentList.Values)
            {
                Tuple<int[,], List<Box>> distToBoxes = DistanceMapping(agent.x, agent.y, startingState); // muligvis omvendt x/y
                PathTable(Tuple.Create(agent.id,  distToBoxes.Item1), startingState, distToBoxes.Item2);

            }
        }
        public Tuple<int[,], List<Box>> DistanceMapping(int row, int col, Node node)
        {
            // initalize variables
            int[,] distanceMap = new int[Node.MAX_ROW, Node.MAX_COL];
            Tuple<int, int> startingPos = Tuple.Create(row, col);
            Queue<Tuple<int, int>> frontier = new Queue<Tuple<int, int>>();
            Queue<Tuple<int, int>> nextTier = new Queue<Tuple<int, int>>();

            List<Box> reachedBoxes = new List<Box>();
            // fill 2d array with -1 values
            for (int i=0; i< Node.MAX_COL; i++)
            {
                for(int j=0; j < Node.MAX_ROW; j++)
                {
                    distanceMap[i, j] = -1;
                }
            }


            // add starting point to distance map
            frontier.Enqueue(startingPos);
            int depth = 0;
            distanceMap[startingPos.Item1, startingPos.Item2] = 0;

            // continue until all boxes are reached or no more moves are available
            while (!frontier.Any() || reachedBoxes.Count == node.boxList.Count)
            {

                Tuple<int, int> leafNode = frontier.Dequeue();
           
                // current depth depleeted, increment depth and add next step fields to frontier
                if (frontier.Count == 0)
                {
                    depth++;
                    frontier = new Queue<Tuple<int, int>>(nextTier);
                    nextTier.Clear();
                }

                // try each possible move
                foreach (Command c in cmds)
                {
                    Tuple<int, int> newPos = Tuple.Create(leafNode.Item1 + Command.dirToRowChange(c.dir1), leafNode.Item2 + Command.dirToColChange(c.dir1));
                    // if move is possible
                    if(!Node.wallList.ContainsKey(Tuple.Create(newPos.Item1, newPos.Item2)))
                    {
                        // if box is reached then add box as reachable box and stop this path
                        if (node.boxList.ContainsKey(Tuple.Create(newPos.Item1, newPos.Item2)))// muligvis omvendt x/y
                        {
                            distanceMap[newPos.Item1, newPos.Item2] = depth;
                            reachedBoxes.Add(node.boxList[Tuple.Create(newPos.Item1, newPos.Item2)]);
                        }
                        else if(distanceMap[newPos.Item1, newPos.Item2] == -1)
                        {
                            // new field reached, mark depth and add field to next depth search.
                            distanceMap[newPos.Item1, newPos.Item2] = depth;
                            nextTier.Enqueue(newPos);
                        }
                        else
                        {
                            // field has already been visited, stop this path
                        }
                    }
                }
            }
            // return a map for the agent/goal object with a list of the reached boxes
            return Tuple.Create(distanceMap, reachedBoxes);
        }

        public void PathTable(Tuple<char, int[,]> distanceMaps, Node node, List<Box> reachedBoxes)
        {
            // path lookup table
            Dictionary<string, Dictionary<string, List<Tuple<int, int>>>> pathTable = new Dictionary<string, Dictionary<string, List<Tuple<int, int>>>>();


            
            foreach(Box box in reachedBoxes)
            {
                // foreach reached box in the distance map, perform backtrack to capture path
                List<Tuple<int,int>> path = backTrack(box, distanceMaps.Item2, node, distanceMaps.Item1);
                pathLookupTable[distanceMaps.Item1][box.id] = path;

            }
        }

        private List<Tuple<int, int>> backTrack(Box box, int[,] distMap, Node node, char currentGoalName)
        {
            List<Tuple<int, int>> path = new List<Tuple<int, int>>();
            Tuple<int,int> currentPos = Tuple.Create(box.y, box.x);
            // begin at box position
            int currentDist = distMap[currentPos.Item1, currentPos.Item2];
            bool foundPath = true;
            path.Add(currentPos);

            while (foundPath)
            {
                foundPath = false;
                foreach (Command c in cmds)
                {
                    Tuple<int, int> newPos = Tuple.Create(currentPos.Item1 + Command.dirToRowChange(c.dir1), currentPos.Item2 + Command.dirToColChange(c.dir1));
                    // if field is marked in the distance map and it's value is lower than current distance, then its part of the optimal path
                    if (distMap[newPos.Item1, newPos.Item2] != -1 && distMap[newPos.Item1, newPos.Item2] < currentDist)
                    {
                        // if end position reached, return path
                        if (distMap[newPos.Item1, newPos.Item2] == 0)
                        {
                            path.Add(newPos);
                            return path;
                        }
                        // add to path and continue
                        else
                        {
                            path.Add(newPos);
                            currentPos = newPos;
                            // update depth
                            currentDist = distMap[newPos.Item1, newPos.Item2];
                        }
                        // if fields contains a box or a goal field then add it as a dependency to this solution
                        if (node.boxAt(newPos.Item1, newPos.Item2))
                        {
                            if (dependencies.ContainsKey(box.id))
                            {
                                dependencies[currentGoalName].Add(box.id);
                            }

                        }
                        else if (Node.goalList.ContainsKey(Tuple.Create(newPos.Item1, newPos.Item2)))
                        {
                            if (dependencies.ContainsKey(currentGoalName))
                            {
                                dependencies[currentGoalName].Add(Node.goalList[Tuple.Create(newPos.Item1, newPos.Item2)].id);
                            }
                            else
                            {
                                dependencies.Add(currentGoalName, new List<char>(Node.goalList[Tuple.Create(newPos.Item1, newPos.Item2)].id));
                            }
                        }
                        // if field that is part of optimal path found, then break.
                        foundPath = true;
                        break;
                    }
                }
            }
            return null;
            System.Diagnostics.Debug.WriteLine("unable to find path while backtracking");
        }
    }
}
