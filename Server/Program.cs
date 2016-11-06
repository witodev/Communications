using System;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new AsyncServer();
            server.Port = 9876;
            server.OnResponse += Server_OnResponse;
            server.StartListening();
            
            Console.WriteLine("Exit...");
            Console.ReadKey();
        }

        private static void Server_OnResponse(object sender, System.Text.StringBuilder e)
        {
            var s = (AsyncServer)sender;
            var count = s._clients.Count;
            var msg = e.ToString();
            e.Clear();
            e.Append("You are client: " + count);
        }
    }
}
