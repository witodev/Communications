using System;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new AsyncClient();
            client.Server = "MYPC";
            client.Port = 9876;
            client.Start();

            var now = "close";

            Console.WriteLine("To server: " + now);                
            //client.StartClient(now);
            client.OnResponse += Client_OnResponse;
            
            client.Send(now);
            client.Receive();

            client.Close();            

            Console.WriteLine("Exit...");
            Console.ReadKey();
        }

        private static void Client_OnResponse(object sender, System.Text.StringBuilder e)
        {
            Console.WriteLine("Resonse event: " + e.ToString());
        }
    }
}
