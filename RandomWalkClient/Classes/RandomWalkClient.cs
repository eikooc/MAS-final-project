using Common.Classes;
using RandomWalkClient.Classes;
using System;
using System.Collections.Generic;

namespace RandomWalkClient.Classes
{
	public class RngWalkClient
	{
		private List<Agent> agents = new List<Agent>();

		public RngWalkClient()
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

			// Read lines specifying level layout
			while (!line.Equals(string.Empty))
			{
				for (int i = 0; i < line.Length; i++)
				{
					char id = line[i];
					if ('0' <= id && id <= '9')
					{
						agents.Add(new Agent(id, colors[id]));
					}
				}

				line = Console.In.ReadLine();
			}
		}

		public bool Update()
		{
			string jointAction = "[";
			for (int i = 0; i < agents.Count - 1; i++)
			{
				jointAction += agents[i].Act() + ",";
			}

			jointAction += agents[agents.Count - 1].Act() + "]";

			// Place message in buffer
			Console.Out.WriteLine(jointAction);

			// Flush buffer
			Console.Out.Flush();
			// Disregard these for now, but read or the server stalls when its output buffer gets filled!
			string percepts = Console.In.ReadLine();
			if (percepts == null)
			{
				return false;
			}

			return true;
		}

	}
}
