using System;
using System.IO;
using System.Collections.Generic;

namespace SAClient.Classes
{
	public class SearchClient
	{
		public Node initialState;

		public SearchClient(TextReader serverMessages)
		{
            Dictionary<char, string> colors = new Dictionary<char, string>();
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
                        this.initialState.agentList.Add(new Tuple(row, col), new Agent(row, col, chr, colors[chr]));
                        
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
						System.Diagnostics.Debug.WriteLine("Error, read invalid level character: " + (int)chr);
						Environment.Exit(1);
					}
				}

			}
		}

		public List<Node> Search(Strategy strategy)
		{
			System.Diagnostics.Debug.WriteLine("Search starting with strategy %s.\n", strategy.ToString());
			strategy.addToFrontier(this.initialState);

			int iterations = 0;
			while (true)
			{
				if (iterations == 1000)
				{
					System.Diagnostics.Debug.WriteLine(strategy.searchStatus());
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
					System.Diagnostics.Debug.WriteLine(" - SOLUTION!!!!!!");
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
			System.Diagnostics.Debug.Write("[" + name + "]fn:" + n.fitness + " gn:" + n.g());
			System.Diagnostics.Debug.Write(" AGENT:[" + n.agentCol + "," + n.agentRow + "]");
			System.Diagnostics.Debug.Write(" BOX(x,y): ");
			foreach (Tuple box in n.boxList.Keys)
			{
				System.Diagnostics.Debug.Write("[" + box.x + "," + box.y + "]");
			}
			System.Diagnostics.Debug.Write(" GOAL(x,y): ");
			foreach (Tuple goal in Node.goalList.Keys)
			{
				System.Diagnostics.Debug.Write("[" + goal.x + "," + goal.y + "] #");
			}
			System.Diagnostics.Debug.WriteLine(n.GetHashCode());
		}


		public static void Main(string[] args)
		{
			TextReader serverMessages = Console.In;

			// Use stderr to print to console
			System.Diagnostics.Debug.WriteLine("SearchClient initializing. I am sending this using the error output stream.");

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
					/*case "-astar":
						strategy = new StrategyBestFirst(new AStar(client.initialState));
						break;
					case "-wastar":
						// You're welcome to test WA* out with different values, but for the report you must at least indicate benchmarks for W = 5.
						strategy = new StrategyBestFirst(new WeightedAStar(client.initialState, 5));
						break;
					case "-greedy":
						strategy = new StrategyBestFirst(new Greedy(client.initialState));
						break;*/
					default:
						strategy = new StrategyBFS();
						System.Diagnostics.Debug.WriteLine("Defaulting to BFS search. Use arguments -bfs, -dfs, -astar, -wastar, or -greedy to set the search strategy.");
						break;
				}
			}
			else
			{
				strategy = new StrategyBFS();
				System.Diagnostics.Debug.WriteLine("Defaulting to BFS search. Use arguments -bfs, -dfs, -astar, -wastar, or -greedy to set the search strategy.");
			}

			List<Node> solution;
			try
			{
				solution = client.Search(strategy);
			}
			catch (OutOfMemoryException ex)
			{
				System.Diagnostics.Debug.WriteLine("Maximum memory usage exceeded.");
				solution = null;
			}

			if (solution == null)
			{
				System.Diagnostics.Debug.WriteLine(strategy.searchStatus());
				System.Diagnostics.Debug.WriteLine("Unable to solve level.");
				Environment.Exit(0);
			}
			else
			{
				System.Diagnostics.Debug.WriteLine("\nSummary for " + strategy.ToString());
				System.Diagnostics.Debug.WriteLine("Found solution of length " + solution.Count);
				System.Diagnostics.Debug.WriteLine(strategy.searchStatus());

				foreach (Node n in solution)
				{
					String act = n.action.ToString();
					Console.Out.WriteLine(act);
					String response = serverMessages.ReadLine();
					if (response.Contains("false"))
					{
						System.Diagnostics.Debug.WriteLine("Server responsed with %s to the inapplicable action: %s\n", response, act);
						System.Diagnostics.Debug.WriteLine("%s was attempted in \n%s\n", act, n.ToString());
						break;
					}
				}
			}
		}
	}
}
