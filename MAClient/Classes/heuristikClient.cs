using Common.Classes;
using MAClient.Enumerations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MAClient.Classes
{
    public class HeuristikClient
    {
        private List<Agent> agents = new List<Agent>();
        private Dictionary<char, Node> agentPercepts;
        private Node initialState;
        private Dictionary<string, List<SubGoal>> subGoalDict;
        private Heuristic heuristic;

        Dictionary<char, string> colors;

        private static Node CurrentNode;

        public HeuristikClient()
        {
            ReadMap();
            CurrentNode = initialState;
        }

        private void TakeAction()
        {
            List<Node> possibleMoves = new List<Node>();
            Node bestNode = null;
            int max = int.MaxValue;
            foreach (Agent agent in CurrentNode.agentList.Values)
            {

                possibleMoves = CurrentNode.getExpandedNodes(agent.x, agent.y);
                foreach (Node node in possibleMoves)
                {
                    int val = heuristic.h(node);
                    if (max > val)
                    {
                        max = val;
                        //Debugger.Launch();
                        bestNode = node;
                    }
                }
                CurrentNode = bestNode;
            }

        }


        public Dictionary<string, List<SubGoal>> CreateSubGoals(Node initialState)
        {
            Dictionary<string, List<SubGoal>> ColorToSubGoalDict = new Dictionary<string, List<SubGoal>>();
            foreach(Box box in initialState.boxList.Values)
            {
                SubGoal MoveToBoxSubGoal = new SubGoal(SubGoalType.MoveAgentTo, box, Tuple.Create(box.x, box.y));
                SubGoal MoveBoxToSubGoal = new SubGoal(SubGoalType.MoveBoxTo, box, Tuple.Create(box.assignedGoal.x, box.assignedGoal.y));
                string color = colors[box.id];
                if (!ColorToSubGoalDict.ContainsKey(color))
                {
                    ColorToSubGoalDict.Add(color, new List<SubGoal>());
                }
                ColorToSubGoalDict[color].Add(MoveToBoxSubGoal);
                ColorToSubGoalDict[color].Add(MoveBoxToSubGoal);
            }
            return ColorToSubGoalDict;

        }

        public void ReadMap()
        {
            colors = new Dictionary<char, string>();
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
            initialState = new Node(null, lines.Count, lines[0].Length);

            int y = 0;
            //Debugger.Launch();
            // Read lines specifying level layout
            foreach (string mapLine in lines)
            {

                for (int x = 0; x < mapLine.Length; x++)
                {
                    Tuple<int, int> pos = Tuple.Create(x, y);
                    char chr = mapLine[x];
                    if ('0' <= chr && chr <= '9')
                    {
                        Agent agent = new Agent(x, y, chr, colors[chr]);
                        initialState.agentList.Add(pos, agent);
                    }
                    else if (chr == '+')
                    { // Wall.
                        Node.wallList.Add(pos, true);
                    }
                    else if ('A' <= chr && chr <= 'Z')
                    { // Box.
                        Box box = new Box(x, y, chr, colors[chr]);
                        initialState.boxList.Add(pos, box);

                    }
                    else if ('a' <= chr && chr <= 'z')
                    { // Goal.
                        Goal goal = new Goal(x, y, chr);
                        Node.goalList.Add(pos, goal);
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

            // intital heuristic on the basis of the read map
            heuristic = new Greedy(initialState);
            // create the inital subgoals on the basis of the read map
            subGoalDict = CreateSubGoals(initialState);

            assignGoals();
        }


        private void assignGoals()
        {
            // update agents beliefs to the state of the inital state
            foreach (Agent agent in CurrentNode.agentList.Values)
            {
                agent.CurrentBeliefs = CurrentNode.copyNode();

                }
            foreach (string color in subGoalDict.Keys)
            {
                Stack<SubGoal> assignableSubGoals = new Stack<SubGoal>(subGoalDict[color]);
                List<Agent> agents = CurrentNode.agentList.Values.Where(x => x.color == color).ToList();
                while(assignableSubGoals.Count != 0)
                {
                    Agent agent = agents.First();
                    agents.Remove(agent);
                    SubGoal moveToBox = assignableSubGoals.Pop();
                    SubGoal moveBoxToGoal = assignableSubGoals.Pop();

                    if (moveToBox.type != SubGoalType.MoveAgentTo || moveBoxToGoal.type != SubGoalType.MoveBoxTo)
                    {
                        throw new Exception("wrong goal type");
                    }
                    agent.CurrentBeliefs = CurrentNode.copyNode();
                    agent.subgoals.Push(moveBoxToGoal);
                    agent.subgoals.Push(moveToBox);
                }
                    
                
            }
        }

        public bool Run()
        {
            
            try
            {
                while (!CurrentNode.isGoalState())
                {
                    TakeAction();
                }
                
                List<Node> plan = CurrentNode.extractPlan();
                int count = 0;
                Dictionary<char, string> agentActions = new Dictionary<char, string>();
                foreach (Node node in plan)
                {
                    agentActions.Add(node.agentList[Tuple.Create(node.agentCol, node.agentRow)].id, node.action.ToString());
                    count++;

                    if (count % CurrentNode.agentList.Count == 0 && count != 0)
                    {
                        List<char> list = agentActions.Keys.ToList();
                        list.Sort();
                        var actions = list.Select(x => agentActions[x]);
                        MakeAction(actions.ToList());
                        agentActions = new Dictionary<char, string>();
                    }

                }
            }
            catch (Exception e)
            {
                Debugger.Launch();
                throw e;
                return false;
            }
            return true;
        }


        //public bool SendActions()
        //{
        //    string jointaction = "[";
        //    for (int i = 0; i < agents.count - 1; i++)
        //    {
        //        jointaction += agents[i].act() + ",";
        //    }

        //    jointaction += agents[agents.count - 1].act() + "]";

        //    // place message in buffer
        //    console.out.writeline(jointaction);

        //    // flush buffer
        //    console.out.flush();
        //    // disregard these for now, but read or the server stalls when its output buffer gets filled!
        //    string percepts = console.in.readline();
        //    if (percepts == null)
        //    {
        //        return false;
        //    }

        //    return true;
        //}

        public bool MakeAction(List<string> agentActions)
        {

            string jointaction = "[";
            var agents = CurrentNode.agentList.Values.OrderBy(x => x.id);
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
