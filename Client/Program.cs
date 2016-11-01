using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var now = DateTime.Now;
            var client = new SyncClient();
            client.StartClient(now.ToString());
            
            Console.WriteLine("Exit...");
            Console.ReadKey();
        }
    }
}
