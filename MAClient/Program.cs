using MAClient.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MAClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread thrd = Thread.CurrentThread;
            thrd.Priority = ThreadPriority.Highest;
            try
            {
                SearchClient client = new SearchClient();
                client.Run();
            }
            catch (Exception e)
            {
                // Got nowhere to write to probably
            }
        }
    }
}
