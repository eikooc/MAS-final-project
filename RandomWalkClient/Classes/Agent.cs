using RandomWalkClient.Classes;
using System;

namespace RandomWalkClient.Classes
{
    public class Agent
    {
        private static Random rand = new Random();

        public Agent(char id, string color)
        {
            System.Diagnostics.Debug.WriteLine("Found " + color + " agent " + id);
        }

        public string Act()
        {
            return Command.Every[rand.Next(Command.Every.Length)].toString();
        }
    }
}
