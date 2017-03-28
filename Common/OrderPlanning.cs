using Common.Enumeration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    class OrderPlanning
    {
        public Dictionary<string, List<string>> dependencies;
        public Dictionary<string, Dictionary<string, List<Tuple<int, int>>>> pathLookupTable;
        public OrderPlanning(Node startingState)
        {
            dependencies = new Dictionary<string, List<string>>();

            foreach (Agent agent in startingState.agents)
            {
                Tuple<int[,], List<Box>> distToBoxes = DistanceMapping(agent.x, agent.y); // muligvis omvendt x/y
                pathLookupTable = PathTable(Tuple.Create(agent.id, distToBoxes.Item1), distToBoxes.Item2);

            }
        }
        public Tuple<int[,], List<Box>> DistanceMapping(int row, int col, Node node)
        {
            // initalize variables
            int[,] distanceMap = new int[node.MAX_COL, node.MAX_ROW];
            Tuple<int, int> startingPos = Tuple.Create(row, col);
            List<Command> cmds = new List<Command>();
            Queue<Tuple<int, int>> frontier = new Queue<Tuple<int, int>>();
            Queue<Tuple<int, int>> nextTier = new Queue<Tuple<int, int>>();

            List<Box> reachedBoxes = new List<Box>();

            // add possible moves
            foreach (Direction d in Enum.GetValues(typeof(Direction)))
            {
                cmds.Add(new Command(d));
            }
            // fill 2d array with -1 values
            for (int i=0; i< node.MAX_COL; i++)
            {
                for(int j=0; j < node.MAX_ROW; j++)
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
                    Tuple<int, int> newPos = Tuple.Create(leafNode.Item1 + Command.DirToRowChange(c), leafNode.Item2 + Command.DirToColChange(c));
                    // if move is possible
                    if(!node.walls[newPos.Item1][newPos.Item2])
                    {
                        Box box = node.boxList.Where(i => i.x == newPos.Item1 && i.y == newPos.Item2).FirstOrDefault();
                        // if box is reached then add box as reachable box and stop this path
                        if (box != null)// muligvis omvendt x/y
                        {
                            distanceMap[newPos.Item1, newPos.Item2] = depth;
                            reachedBoxes.Add(box);
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

            return Tuple.Create(distanceMap, reachedBoxes);
        }

        public Dictionary<string, Dictionary<string, List<Tuple<int,int>>>> PathTable(Tuple<string, List<int[,]>> distanceMaps, Node node, List<Box> reachedBoxes)
        {
            // path lookup table
            Dictionary<string, Dictionary<string, List<Tuple<int, int>>>> pathTable = new Dictionary<string, Dictionary<string, List<Tuple<int, int>>>>();


            foreach (int[,] distMap in distanceMaps.Item2)
            {
                foreach(Box box in reachedBoxes)
                {
                    // foreach reached box in the distance map, perform backtrack to capture path
                    List<Tuple<int,int>> path = backTrack(box, distMap);
                    pathTable[distanceMaps.Item1][box.id] = path;

                }
            }
        }

        private List<Tuple<int, int>> backTrack(Tuple box, int[,] distMap)
        {
            List<Tuple<int, int>> path = new List<Tuple<int, int>>();
            Tuple<int,int> currentPos = Tuple.Create(box.y, box.x);
            // begin at box position
            int currentDist = distMap[currentPos.item1, currentPos.item2];
            bool foundPath = true;
            path.Add(currentPos);

            while (foundPath)
            {
                foundPath = false;
                foreach (Command c in cmds)
                {
                    Tuple<int, int> newPos = Tuple.Create(currentPos.Item1 + Command.DirToRowChange(c), currentPos.Item2 + Command.DirToColChange(c));
                    // if field is marked in the distance map and it's value is lower than current distance, then its part of the optimal path
                    if (distMap[newPos.item1, newPos.item2] != -1 && distMap[newPos.item1, newPos.item2] < currentDist)
                    {
                        // if end position reached, return path
                        if (distMap[newPos.item1, newPos.item2] == 0)
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
                            currentDist = distMap[newPos.item1, newPos.item2];
                        }
                        // if fields contains a box or a goal field then add it as a dependency to this solution
                        Box box = node.boxList.Where(i => i.x == newPos.Item1 && i.y == newPos.Item2).FirstOrDefault()
                        if (box != null)
                        {
                            if (dependencies.ContainsKey(box.id))
                            {
                                dependencies[box.id].Add(box.id)
                            }

                        }
                        else if (node.goals[newPos.item1, newPos.item2])
                        {
                            if (dependencies.ContainsKey(newPos.item1))
                            {
                                dependencies[box.id].Add(box.id)
                            }
                        }
                        // if field that is part of optimal path found, then break.
                        foundPath = true;
                        break;
                    }
                }
            }
            System.Diagnostics.Debug.WriteLine("unable to find path while backtracking");
        }
    }
}
