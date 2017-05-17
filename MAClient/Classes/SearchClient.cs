using Common.Classes;
using Common.Interfaces;
using MAClient.Classes.Entities;
using MAClient.Classes.Goals;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MAClient.Classes
{
    public class SearchClient
    {
        private Node initialState;
        private Dictionary<char, string> colors;
        private List<int> agentIds;
        public static Node CurrentNode;
        private static Dictionary<int, MapPartition> partitionMap;
        private static List<MapPartition> partitions;
        public SearchClient()
        {
            //Debugger.Launch();
            this.ReadMap();
            // update current node to the inital state
            CurrentNode = this.initialState;
            // create the inital subgoals on the basis of the read map
            this.CreatePartitions(initialState);
            this.AssignGoalsToAgents();
        }

        public static IEnumerable<Agent> FindSamaritans(Agent agent)
        {
            if (partitionMap.ContainsKey(agent.uid))
            {
                return CurrentNode.agentList.Entities.Where(x => partitionMap[agent.uid].Agents.Ids.Contains(x.uid));
            }
            return new List<Agent>();
        }
        private void CreatePartitions(Node intitialState)
        {
            partitions = new List<MapPartition>();
            partitionMap = new Dictionary<int, MapPartition>();
            HashSet<int> visitedGoals = new HashSet<int>();
            foreach (Goal goal in Node.goalList.Entities)
            {
                if (visitedGoals.Contains(goal.uid)) continue;

                MapPartition partition = new MapPartition(Node.MAX_COL, Node.MAX_ROW);
                DistanceMap dm = new DistanceMap(goal.col, goal.row, intitialState);
                while (dm.frontier.Count != 0)
                {
                    foreach (IEntity entity in dm.getEntities())
                    {
                        if (entity is Agent) partition.AddAgent((Agent)entity);
                        else if (entity is Box) partition.AddBox((Box)entity);
                        else if (entity is Goal)
                        {
                            partition.AddGoal((Goal)entity);
                            visitedGoals.Add(entity.uid);
                        }
                    }
                    dm.Expand();
                }

                visitedGoals.Add(goal.uid);
                foreach (Agent agent in partition.Agents.Entities)
                {
                    partitionMap.Add(agent.uid, partition);
                }
                partitions.Add(partition);
                partition.ProcessPartition();
            }
        }
        private void AssignGoalsToAgents()
        {
            foreach (Agent agent in CurrentNode.agentList.Entities)
            {
                agent.CurrentBeliefs = CurrentNode.copyNode();
                agent.CurrentBeliefs.boxList.Entities.Where(x => x.color != agent.color).ToList().ForEach(b => agent.CurrentBeliefs.boxList.Remove(b.uid));
                agent.CurrentBeliefs.agentList.Entities.Where(x => x.uid != agent.uid).ToList().ForEach(a => agent.CurrentBeliefs.agentList.Remove(a.uid));
                agent.CurrentBeliefs.agentCol = agent.col;
                agent.CurrentBeliefs.agentRow = agent.row;
                agent.strategy = new StrategyBestFirst(new AStar(agent.CurrentBeliefs));
                AssignGoal(agent, CurrentNode);
            }
        }
        public static bool AssignGoal(Agent agent, Node currentNode)
        {
            if (partitionMap.ContainsKey(agent.uid))
            {
                Objective objective = partitionMap[agent.uid].GetObjective(agent, currentNode);
                if (objective != null)
                {
                    agent.AddSubGoal(objective.MoveBoxTo);
                    agent.AddSubGoal(objective.MoveAgentTo);
                    return true;
                }
            }
            return false; // agent cannot do anything
        }
        public void ReadMap()
        {
            colors = new Dictionary<char, string>();
            agentIds = new List<int>();
            string line, color;

            // Read lines specifying colors
            while ((line = Console.In.ReadLine()).Matches(@"^[a-z]+:\s*[0-9A-Z](,\s*[0-9A-Z])*\s*"))
            {
                line = line.Replace(" ", string.Empty);
                color = line.Split(':')[0];

                foreach (string id in line.Split(':')[1].Split(','))
                {
                    colors.Add(id[0], color);
                }
            }

            // Pre-cache the level to determine its size.
            List<string> lines = new List<string>();
            while (!line.Equals(""))
            {
                lines.Add(line);
                line = Console.In.ReadLine();
            }
            int maxWidth = lines.Max(x => x.Length);
            initialState = new Node(null, lines.Count, maxWidth);

            int y = 0;
            // Read lines specifying level layout
            foreach (string mapLine in lines)
            {
                for (int x = 0; x < mapLine.Length; x++)
                {
                    char chr = mapLine[x];
                    if ('0' <= chr && chr <= '9')
                    {
                        int id = (int)chr - '0';
                        if (!colors.ContainsKey(chr)) colors.Add(chr, "blue");
                        Agent agent = new Agent(x, y, id, colors[chr]);
                        initialState.agentList.Add(agent);
                        this.agentIds.Add(agent.uid);
                    }
                    else if (chr == '+')
                    { // Wall.
                        Node.wallList.Add(new Position(x, y));
                    }
                    else if ('A' <= chr && chr <= 'Z')
                    { // Box.
                        if (!colors.ContainsKey(chr)) colors.Add(chr, "blue");
                        initialState.boxList.Add(new Box(x, y, chr, colors[chr]));
                    }
                    else if ('a' <= chr && chr <= 'z')
                    { // Goal.
                        Node.goalList.Add(new Goal(x, y, chr));
                    }
                    else if (chr == ' ')
                    {
                        // Free space.
                    }
                    else
                    {
                        //System.Diagnostics.Debug.WriteLine("Error, read invalid level character: " + (int)chr);
                        Environment.Exit(1);
                    }
                }
                y++;
            }
        }
        public bool Run()
        {
            try
            {
                while (!CurrentNode.isGoalState())
                {
                    this.TakeAction();
                }

                Plan plan = CurrentNode.extractPlan();
                int count = 0;
                Dictionary<int, string> agentActions = new Dictionary<int, string>();
                foreach (Node node in plan.path)
                {
                    agentActions.Add(node.agentList[node.agentCol, node.agentRow].uid, node.action.ToString());
                    count++;

                    if (count % CurrentNode.agentList.Count == 0 && count != 0)
                    {
                        List<int> list = agentActions.Keys.ToList();
                        list.Sort();
                        var actions = list.Select(x => agentActions[x]);
                        this.MakeAction(actions.ToList());
                        agentActions = new Dictionary<int, string>();
                    }

                }
            }
            catch (Exception e)
            {
                //Debugger.Launch();
                throw e;
                return false;
            }
            return true;
        }

        private void TakeAction()
        {
            foreach (int agentId in agentIds)
            {
                Agent agent = CurrentNode.agentList[agentId];
                agent.ProcessAgentAction(ref CurrentNode);
            }
        }
        private bool MakeAction(List<string> agentActions)
        {

            string jointaction = "[";
            var agents = CurrentNode.agentList.Entities.OrderBy(x => x.uid);
            foreach (string command in agentActions.Take(agentActions.Count - 1))
            {
                jointaction += command + ",";
            }
            jointaction += agentActions.Last() + "]";

            // place message in buffer
            Console.Out.WriteLine(jointaction);

            // flush buffer
            Console.Out.Flush();
            // disregard these for now, but read or the server stalls when its output buffer gets filled!
            string percepts = Console.In.ReadLine();
            if (percepts == null)
            {
                return false;
            }
            return true;
        }
    }
}
