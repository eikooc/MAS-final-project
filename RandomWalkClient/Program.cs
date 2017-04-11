﻿using System;
using RandomWalkClient.Classes;
using System.Linq;

namespace RandomWalkClient
{
    public class Program
    {
        static void Main(string[] args)
        {
            // Use stderr to print to console
            DebugOut.Debug.WriteLine("Hello from RandomWalkClient. I am sending this using the error outputstream");
            try
            {
                RngWalkClient client = new RngWalkClient();
                while (client.Update()) ;
            }
            catch (Exception e)
            {
                // Got nowhere to write to probably
            }
        }
    }
}
