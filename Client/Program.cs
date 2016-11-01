using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            for (var i = 0; i < 5; i++)
            {
                var now = DateTime.Now.Millisecond;
                var client = new SyncClient();
                client.StartClient(now.ToString());
            }
            
            Console.WriteLine("Exit...");
            Console.ReadKey();
        }
    }
}
