using System;
using RandomWalkClient.Classes;


namespace RandomWalkClient
{
    public class Program
    {
        static void Main(string[] args)
        {
            // Use stderr to print to console
            System.Diagnostics.Debug.WriteLine("Hello from RandomWalkClient. I am sending this using the error outputstream");
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
