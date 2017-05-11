using Common.Classes;
using Common.Interfaces;
using MAClient.Classes.Entities;
using MAClient.Classes.Goals;
using MAClient.Enumerations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace MAClient.Classes
{
    public class SearchClient
    {
        private Node initialState;
        private Dictionary<string, List<SubGoal>> subGoalDict;
        private Heuristic heuristic;

        Dictionary<char, string> colors;
        List<int> agentIds;

        private static Node CurrentNode;

        public SearchClient()
        {
            Debugger.Launch();
            ReadMap();

            // update current node to the inital state
            CurrentNode = initialState;

            // intital heuristic on the basis of the read map
            heuristic = new Greedy(initialState);
            // create the inital subgoals on the basis of the read map
            subGoalDict = CreateSubGoals(initialState);
            // assigne goals to agents
            assignGoals();
        }


        private void TakeAction()
        {
            foreach (int agentId in agentIds)
            {
                Agent agent = CurrentNode.agentList[agentId];
                ProcessAgentAction(agent);
            }

        }

        // redundant
        private void ResolveConflict(Agent agent)
        {
            if (agent.encounteredObjects.Count != 0)
            {
                object obstacle = agent.encounteredObjects.Pop();
                List<IEntity> usedFields = agent.plan.ExtractUsedFields();
                if (obstacle != null)
                {

                    // agent plan is hindered by obstacle
                    if (obstacle is Box)
                    {
                        Box box = ((Box)obstacle);
                        foreach (Agent samaritan in CurrentNode.agentList.Entities.Where(x => x.color == box.color))
                        {
                            MoveAway moveAgentAway = new MoveAway(new IEntity[] { box, samaritan }, usedFields, agent.uid, samaritan.uid);
                            if (!samaritan.subgoals.Any(x => x.Equals(moveAgentAway)))
                            {
                                MoveAgentTo moveAgentTo = new MoveAgentTo(box, samaritan.uid);
                                WaitFor waitForCompletion = new WaitFor(agent.subgoals.Peek(), samaritan.uid);
                                samaritan.subgoals.Push(waitForCompletion);
                                samaritan.subgoals.Push(moveAgentAway);
                                samaritan.ReplanWithSubGoal(moveAgentTo);
                                samaritan.plan = null;
                                agent.subgoals.Push(new WaitFor(moveAgentAway, samaritan.uid));
                                performNoOp(agent);
                            }
                            else
                            {
                                ResolveConflict(agent);
                            }
                        }
                    }
                    else if (obstacle is Agent)
                    {
                        Agent samaritan = (Agent)obstacle;
                        MoveAway moveAgentAway = new MoveAway(new IEntity[] { samaritan }, usedFields, agent.uid, samaritan.uid);
                        if (!samaritan.subgoals.Any(x => x.Equals(moveAgentAway)))
                        {
                            WaitFor waitForCompletion = new WaitFor(agent.subgoals.Peek(), samaritan.uid);
                            this.UpdateCurrentBelief(agent, CurrentNode.agentList, samaritan.CurrentBeliefs.agentList);
                            samaritan.subgoals.Push(waitForCompletion);
                            samaritan.ReplanWithSubGoal(moveAgentAway);
                            agent.subgoals.Push(new WaitFor(moveAgentAway, samaritan.uid));
                            performNoOp(agent);
                        }
                        else
                        {
                            ResolveConflict(agent);
                        }
                    }
                }
            }
            else
            {
                performNoOp(agent);
            }
        }

        private void ProcessAgentAction(Agent agent)
        {
            agent.UpdateSubgoalStates(CurrentNode); // purpose?

            if (agent.IsWaiting()) // cleanup here after reffactoring
            {
                performNoOp(agent);
            }
            else
            {
                // get agents next move
                Node nextMove = agent.getNextMove();
                if (nextMove == null) // cleanup here after reffactoring
                {
                    if (agent.IsWaiting())
                    {
                        // agent is done with subgoals, perform noOp
                        performNoOp(agent);
                    }
                }
                else // if (nextMove != null)
                {
                    // convert the node to a command
                    Command nextAction = nextMove.action;
                    // validate that the command is legal
                    IEntity obstacle = CurrentNode.ValidateAction(nextAction, agent.col, agent.row);
                    if (obstacle == null)
                    {
                        // succesfull move
                        agent.acceptNextMove();
                        CurrentNode = CurrentNode.ChildNode();
                        CurrentNode.updateNode(nextMove, agent.col, agent.row);
                    }
                    else
                    {
                        // if not, then update agents beliefs, and replan a plan for the current sub goal
                        agent.backTrack();
                        if (obstacle is Box)
                        {
                            // opdaterer kun en box position men ikke en players hvis den blive "handlet". Kan ikke skelne imellem en box i bevægelse og en stationær
                            this.UpdateCurrentBelief(obstacle, CurrentNode.boxList, agent.CurrentBeliefs.boxList);
                            this.UpdateCurrentBelief(null, CurrentNode.agentList, agent.CurrentBeliefs.agentList);
                            agent.encounteredObjects.Push(obstacle);
                            this.TryConflictResolve(agent);
                        }
                        else if (obstacle is Agent)
                        {
                            Agent otherAgent = (Agent)obstacle;
                            Agent perceivedAgent = otherAgent.CurrentBeliefs.agentList[agent.uid];
                            if (perceivedAgent != null && perceivedAgent.col == agent.col && perceivedAgent.row == agent.row)
                            {
                                performNoOp(agent);
                            }
                            else
                            {
                                this.UpdateCurrentBelief(obstacle, CurrentNode.agentList, agent.CurrentBeliefs.agentList);
                                this.UpdateCurrentBelief(null, CurrentNode.boxList, agent.CurrentBeliefs.boxList);
                                agent.encounteredObjects.Push(obstacle);
                                this.TryConflictResolve(agent);
                            }
                        }
                    }
                }
            }
        }

        private static void findSpotAndSamaritan(IEntity encounteredObstacle, HashSet<Tuple<int, int>> usedFields, string color, ref Tuple<int, int> freeSpot, ref Agent samaritan)
        {
            DistanceMap dm = new DistanceMap(encounteredObstacle.col, encounteredObstacle.row, CurrentNode);
            while (samaritan == null || freeSpot == null)
            {
                dm.Expand();
                if (samaritan == null)
                {
                    foreach (IEntity entity in dm.getEntities())
                    {
                        if (entity is Agent && ((Agent)entity).color == color)
                        {
                            samaritan = (Agent)entity;
                            break;
                        }
                    }
                }
                if (freeSpot == null)
                {
                    foreach (Tuple<int, int> spot in dm.frontier)
                    {
                        if (!usedFields.Contains(spot))
                        {
                            freeSpot = spot;
                            break;
                        }
                    }
                }
            }
        }

        private void UpdateCurrentBelief<T>(IEntity entity, EntityList<T> currentNode, EntityList<T> currentBelief) where T : IEntity
        {
            foreach (IEntity oldEntity in currentBelief.Entities)
            {
                IEntity currentEntity = currentNode[oldEntity.uid];
                currentBelief.UpdatePosition(oldEntity.col, oldEntity.row, currentEntity.col, currentEntity.row);
            }
            if (entity != null && currentBelief[entity.uid] == null)
            {
                currentBelief.Add((T)entity);
            }
        }

        // implement
        //private void TryConflictResolve(Agent agent)
        //{
        //    Node nextMove = agent.ResolveConflict(CurrentNode);
        //    CurrentNode = CurrentNode.ChildNode();
        //    CurrentNode.updateNode(nextMove, agent.col, agent.row);
        //}

        // redundant
        private void TryConflictResolve(Agent agent)
        {
            Plan plan = agent.CreatePlan(agent.strategy);
            if (plan == null)
            {
                ResolveConflict(agent);
                performNoOp(agent);
             }
            else
            {
                agent.plan = plan;
                performNoOp(agent);
            }
        }

        // redundant
        private static void performNoOp(Agent agent)
        {
            Node n = CurrentNode.copyNode();
            n.action = new Command(ActionType.NoOp);
            n.agentCol = agent.col;
            n.agentRow = agent.row;
            CurrentNode = CurrentNode.ChildNode();
            CurrentNode.updateNode(n, agent.col, agent.row);
        }

        // intitate subgoals based on boxes and goals in game
        public Dictionary<string, List<SubGoal>> CreateSubGoals(Node initialState)
        {
            Dictionary<string, List<SubGoal>> ColorToSubGoalDict = new Dictionary<string, List<SubGoal>>();
            // loop throug goals to create sub goals, if goal does not have a corresponding box then it will not work.
            foreach (Box box in initialState.boxList.Entities)
            {
                if (box.assignedGoal != null)
                {
                    MoveAgentTo MoveToBoxSubGoal = new MoveAgentTo(box, -1);
                    MoveBoxTo MoveBoxToSubGoal = new MoveBoxTo(box, new Position(box.assignedGoal.col, box.assignedGoal.row), -1);
                    string color = colors[box.id];
                    if (!ColorToSubGoalDict.ContainsKey(color))
                    {
                        ColorToSubGoalDict.Add(color, new List<SubGoal>());
                    }
                    ColorToSubGoalDict[color].Add(MoveToBoxSubGoal);
                    ColorToSubGoalDict[color].Add(MoveBoxToSubGoal);
                }
            }
            return ColorToSubGoalDict;

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
            initialState = new Node(null, lines.Count, lines[0].Length);

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

        // utilize objectives instead of subgoals
        private void assignGoals()
        {

            // update agents beliefs to the state of the inital state
            foreach (Agent agent in CurrentNode.agentList.Entities)
            {
                agent.CurrentBeliefs = CurrentNode.copyNode();
                agent.CurrentBeliefs.boxList.Entities.Where(x => x.color != agent.color).ToList().ForEach(b => agent.CurrentBeliefs.boxList.Remove(b.uid));

                agent.CurrentBeliefs.agentList.Entities.Where(x => x.uid != agent.uid).ToList().ForEach(a => agent.CurrentBeliefs.agentList.Remove(a.uid));
                agent.CurrentBeliefs.agentCol = agent.col;
                agent.CurrentBeliefs.agentRow = agent.row;
                agent.strategy = new StrategyBestFirst(new Greedy(agent.CurrentBeliefs));
            }
            foreach (string color in subGoalDict.Keys)
            {
                List<SubGoal> assignableSubGoals = subGoalDict[color];
                List<Agent> agents = CurrentNode.agentList.Entities.Where(x => x.color == color).ToList();
                for (int i = 0; i < assignableSubGoals.Count; i += 2)
                {
                    Agent agent = agents.ElementAt((i / 2) % agents.Count);
                    SubGoal moveToBox = assignableSubGoals.ElementAt(i);
                    SubGoal moveBoxToGoal = assignableSubGoals.ElementAt(i + 1);

                    if (!(moveToBox is MoveAgentTo) || !(moveBoxToGoal is MoveBoxTo))
                    {
                        throw new Exception("wrong goal type");
                    }
                    moveToBox.owner = agent.uid;
                    moveBoxToGoal.owner = agent.uid;
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
                        MakeAction(actions.ToList());
                        agentActions = new Dictionary<int, string>();
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


        public bool MakeAction(List<string> agentActions)
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
