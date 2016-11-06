using System;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new MyServer();
            server.Port = 9876;
            server.OnResponse += Server_OnResponse;
            server.Start();

            Console.WriteLine("Exit...");
            Console.ReadKey();
        }

        private static void Server_OnResponse(object sender, System.Text.StringBuilder e)
        {
            var msg = e.ToString();
            e.Clear();
            e.Append("Echo: " + msg);
        }
    }
}
