using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Classes;
using MAClient.Enumerations;
using Common.Interfaces;

namespace MAClient.Classes
{
    class DistanceMap
    {
        public int depth;
        public int[,] distanceMap { get; }
        Node state;
        public Queue<Tuple<int, int>> frontier { get; set; }
        public Queue<Tuple<int, int>> nextTier { get; set; }

        Tuple <int,int> startingPos;
        List<Command> cmds;

        public DistanceMap(int col, int row, Node state)
        {
            cmds = new List<Command>();
            foreach (Dir d in Enum.GetValues(typeof(Dir)))
            {
                cmds.Add(new Command(d));
            }
            this.state = state;
            distanceMap = new int[Node.MAX_COL,Node.MAX_ROW];
            startingPos = Tuple.Create(col, row);

            frontier = new Queue<Tuple<int, int>>();
            nextTier = new Queue<Tuple<int, int>>();

            depth = 1;
            frontier.Enqueue(startingPos);
            distanceMap[startingPos.Item1, startingPos.Item2] = depth;

        }

        public void Expand()
        {
            depth++;
            while(frontier.Any())
            {
                Tuple<int, int> leaf = frontier.Dequeue();
                foreach (Command c in cmds)
                {
                    Tuple<int, int> newPos = Tuple.Create(leaf.Item1 + Command.dirToColChange(c.dir1), leaf.Item2 + Command.dirToRowChange(c.dir1));
                    // if move is possible
                    if (Node.wallList[newPos.Item1, newPos.Item2] == null)
                    {
                        if (distanceMap[newPos.Item1, newPos.Item2] == 0)
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
            frontier = new Queue<Tuple<int, int>>(nextTier);
            nextTier.Clear();
        }

        public List<IEntity> getEntities()
        {
            List<IEntity> entities = new List<IEntity>();
            foreach(Tuple<int,int> field in frontier)
            {
                if (state.boxList[field.Item1, field.Item2] != null) entities.Add(state.boxList[field.Item1, field.Item2]);
                if (state.agentList[field.Item1, field.Item2] != null) entities.Add(state.agentList[field.Item1, field.Item2]);
                if (Node.goalList[field.Item1, field.Item2] != null) entities.Add(Node.goalList[field.Item1, field.Item2]);
            }
            return entities;
        }
        
    }
}
