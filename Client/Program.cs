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
                client.OnResponse += Client_OnResponse;
                var now = DateTime.Now.Millisecond.ToString();
                Console.WriteLine("To server: " + now);
                client.StartClient(now);
            }
            Console.WriteLine("Exit...");
            Console.ReadKey();
        }

        private static void Client_OnResponse(object sender, System.Text.StringBuilder e)
        {
            Console.WriteLine("Resonse event: " + e.ToString());
        }
    }
}
