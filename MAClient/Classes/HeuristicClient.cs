using Common;
using Common.Classes;
using System;
using System.Collections.Generic;

namespace MAClient.Classes
{
	public class HeuristicClient
	{
		private List<Agent> agents = new List<Agent>();
        private Node initialState;
		public HeuristicClient()
		{
			ReadMap();
		}

		public void ReadMap()
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
                line = Console.In.ReadLine();
            }
            this.initialState = new Node(null, lines.Count, lines[0].Length);

            int y = 0;
            // Read lines specifying level layout
            foreach(string mapLine in lines)
            {

                for (int x = 0; x < line.Length; x++)
                {
                    Tuple<int, int> pos = Tuple.Create(x, y);
                    char chr = mapLine[x];
                    if ('0' <= chr && chr <= '9')
                    {
                        Agent agent = new Agent(x,y, chr, colors[chr]);
                        initialState.agentList.Add(pos, agent);
                    }
                    else if (chr == '+')
                    { // Wall.
                        Node.wallList.Add(pos, true) ;
                    }
                    else if ('A' <= chr && chr <= 'Z')
                    { // Box.
                        Box box = new Box(x,y, chr, colors[chr]);
                        initialState.boxList.Add(pos, box);

                    }
                    else if ('a' <= chr && chr <= 'z')
                    { // Goal.
                        Goal goal = new Goal(x,y, chr);
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
                
		}

        //public bool update()
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

    }
}
