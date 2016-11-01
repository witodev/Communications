using System;
using System.Text;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new AsyncServer();
            server.OnRespond += Server_OnRespond1;
            server.StartListening();
            
            Console.WriteLine("Exit...");
            Console.ReadKey();
        }

        private static void Server_OnRespond1(object sender, StateObject e)
        {
            var time = int.Parse(e.sb.ToString());
            e.sb.Append("\tServer: ");
            e.sb.Append(time % 2 == 0 ? "even" : "odd");
        }

        private void Server_OnRespond(object sender, StringBuilder e)
        {
            e.Append("\tServer: ");
            var now = DateTime.Now.Millisecond.ToString();
            e.Append(now);
        }
    }
}
