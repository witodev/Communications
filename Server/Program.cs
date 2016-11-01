using System;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new AsyncServer();
            server.OnResponse += Server_OnResponse;
            server.StartListening();
            
            Console.WriteLine("Exit...");
            Console.ReadKey();
        }

        private static void Server_OnResponse(object sender, System.Text.StringBuilder e)
        {
            var time = int.Parse(e.ToString());
            e.Clear();
            e.Append(time % 2 == 0 ? "even" : "odd");
        }
    }
}
