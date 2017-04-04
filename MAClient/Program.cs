using MAClient.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAClient
{
    class Program
    {
        static void Main(string[] args)
        {
            // Use stderr to print to console
            System.Diagnostics.Debug.WriteLine("Hello from Heuristik with a k. I am sending this using the error outputstream");
            try
            {
                HeuristikClient client = new HeuristikClient();
                client.Run();
            }
            catch (Exception e)
            {
                // Got nowhere to write to probably
            }
        }
    }
}
