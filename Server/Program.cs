using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new AsyncServer();
            server.StartListening();

            Console.WriteLine("Exit...");
            Console.ReadKey();
            var server = new SyncServer();
            server.OnRespond += Server_OnRespond;
            server.StartListening();

            Console.WriteLine("Exit...");
            Console.ReadKey();
        }

        private static void Server_OnRespond(object sender, StringBuilder e)
        {
            e.Append("\tServer: ");
            var now = DateTime.Now.Millisecond.ToString();
            Thread.Sleep(1000);
            var sleep = DateTime.Now.Millisecond.ToString();
            e.Append(now);
            e.Append("\t");
            e.Append(sleep);
        }
    }
}
