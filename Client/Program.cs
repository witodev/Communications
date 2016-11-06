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
                client.Server = "MYPC";
                client.Port = 9876;
                client.Start();

                var now = DateTime.Now.Millisecond.ToString();
                Console.WriteLine("To server: " + now);
                
                //client.StartClient(now);
                client.Send(now);
                client.OnResponse += Client_OnResponse;
                client.Receive();
                client.Close();
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
