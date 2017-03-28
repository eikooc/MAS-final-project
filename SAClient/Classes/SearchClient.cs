using System;
using System.IO;
using System.Collections.Generic;
using DebugOut;
using System.Threading;

namespace SAClient.Classes
{
	public class SearchClient
	{
		public Node initialState;

		public SearchClient(TextReader serverMessages)
		{
			// Read lines specifying colors
			string line = serverMessages.ReadLine();
			if (line.Matches("^[a-z]+:\\s*[0-9A-Z](\\s*,\\s*[0-9A-Z])*\\s*$"))
			{
				DebugOut.Debug.WriteLine("Error, client does not support colors.");
				DebugOut.Debug.Exit(1);
			}

			bool agentFound = false;

			// Pre-cache the level to determine its size.
			List<string> lines = new List<string>();
			while (!line.Equals(""))
			{
				lines.Add(line);
				line = serverMessages.ReadLine();
			}
			this.initialState = new Node(null, lines.Count, lines[0].Length);


			for (int row = 0; row < lines.Count; row++)
			{
				line = lines[row];

				for (int col = 0; col < line.Length; col++)
				{
					char chr = line[col];

					if (chr == '+')
					{ // Wall.
						this.initialState.walls[row][col] = true;
					}
					else if ('0' <= chr && chr <= '9')
					{ // Agent.
						if (agentFound)
						{
							DebugOut.Debug.WriteLine("Error, not a single agent level");
                            DebugOut.Debug.Exit(1);
						}
						agentFound = true;
						this.initialState.agentRow = row;
						this.initialState.agentCol = col;
					}
					else if ('A' <= chr && chr <= 'Z')
					{ // Box.
						this.initialState.boxes[row][col] = chr;
					}
					else if ('a' <= chr && chr <= 'z')
					{ // Goal.
						this.initialState.goals[row][col] = chr;
					}
					else if (chr == ' ')
					{
						// Free space.
					}
					else
					{
						DebugOut.Debug.WriteLine("Error, read invalid level character: " + (int)chr);
                        DebugOut.Debug.Exit(1);
					}
				}

			}
		}

		public List<Node> Search(Strategy strategy)
		{
			DebugOut.Debug.WriteLine("Search starting with strategy {0}.\n", strategy.ToString());
			strategy.addToFrontier(this.initialState);

			int iterations = 0;
			while (true)
			{
				if (iterations == 1000)
				{
					DebugOut.Debug.WriteLine(strategy.searchStatus());
					iterations = 0;
				}

				if (strategy.frontierIsEmpty())
				{
					return null;
				}

				Node leafNode = strategy.getAndRemoveLeaf();
				//ShowNode(leafNode, "Leaf");
				if (leafNode.isGoalState())
				{
					DebugOut.Debug.WriteLine(" - SOLUTION!!!!!!");
					return leafNode.extractPlan();
				}

				strategy.addToExplored(leafNode);
				foreach (Node n in leafNode.getExpandedNodes())
				{ // The list of expanded nodes is shuffled randomly; see Node.java.
					if (!strategy.isExplored(n) && !strategy.inFrontier(n))
					{
						strategy.addToFrontier(n);
					}
				}
				iterations++;
			}
		}

		public static void ShowNode(Node n, String name)
		{
			DebugOut.Debug.Write("[" + name + "]fn:" + n.fitness + " gn:" + n.g());
			DebugOut.Debug.Write(" AGENT:[" + n.agentCol + "," + n.agentRow + "]");
			DebugOut.Debug.Write(" BOX(x,y): ");
			foreach (Tuple box in n.boxList.Keys)
			{
				DebugOut.Debug.Write("[" + box.x + "," + box.y + "]");
			}
			DebugOut.Debug.Write(" GOAL(x,y): ");
			foreach (Tuple goal in Node.goalList.Keys)
			{
				DebugOut.Debug.Write("[" + goal.x + "," + goal.y + "] #");
			}
			DebugOut.Debug.WriteLine(n.GetHashCode());
		}


		public static void Main(string[] args)
		{
			TextReader serverMessages = Console.In;

			// Use stderr to print to console
			DebugOut.Debug.WriteLine("SearchClient initializing. I am sending this using the error output stream.");

			// Read level and create the initial state of the problem
			SearchClient client = new SearchClient(serverMessages);

			Strategy strategy;
			if (args.Length > 0)
			{
				switch (args[0].ToLower())
				{
					case "-bfs":
						strategy = new StrategyBFS();
						break;
					case "-dfs":
						strategy = new StrategyDFS();
						break;
					case "-astar":
						strategy = new StrategyBestFirst(new AStar(client.initialState));
						break;
					case "-wastar":
						// You're welcome to test WA* out with different values, but for the report you must at least indicate benchmarks for W = 5.
						strategy = new StrategyBestFirst(new WeightedAStar(client.initialState, 5));
						break;
					case "-greedy":
						strategy = new StrategyBestFirst(new Greedy(client.initialState));
						break;
					default:
						strategy = new StrategyBFS();
						DebugOut.Debug.WriteLine("Defaulting to BFS search. Use arguments -bfs, -dfs, -astar, -wastar, or -greedy to set the search strategy.");
						break;
				}
			}
			else
			{
				strategy = new StrategyBFS();
                DebugOut.Debug.WriteLine("Defaulting to BFS search. Use arguments -bfs, -dfs, -astar, -wastar, or -greedy to set the search strategy.");
			}

			List<Node> solution;
			try
			{
				solution = client.Search(strategy);
			}
			catch (OutOfMemoryException ex)
			{
				DebugOut.Debug.WriteLine("Maximum memory usage exceeded.");
				solution = null;
			}

			if (solution == null)
			{
				DebugOut.Debug.WriteLine(strategy.searchStatus());
				DebugOut.Debug.WriteLine("Unable to solve level.");
                DebugOut.Debug.Exit(0);
			}
			else
			{
				DebugOut.Debug.WriteLine("\nSummary for " + strategy.ToString());
				DebugOut.Debug.WriteLine("Found solution of length " + solution.Count);
				DebugOut.Debug.WriteLine(strategy.searchStatus());

				foreach (Node n in solution)
				{
					String act = n.action.ToString();
					Console.Out.WriteLine(act);
					String response = serverMessages.ReadLine();
					if (response.Contains("false"))
					{
						DebugOut.Debug.WriteLine("Server responsed with {0} to the inapplicable action: {1}\n", response, act);
						DebugOut.Debug.WriteLine("{0} was attempted in \n{1}\n", act, n.ToString());
						break;
					}
				}
                DebugOut.Debug.Exit(0);

            }
            
		}
	}
}
