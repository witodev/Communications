using System;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            for (var i = 0; i < 5; i++)
            {
                var client = new AsyncClient();
                var now = DateTime.Now.Millisecond.ToString();
                client.StartClient(now);
                Console.WriteLine("From server: " + client.Response);
            }
            Console.WriteLine("Exit...");
            Console.ReadKey();
        }
    }
}
