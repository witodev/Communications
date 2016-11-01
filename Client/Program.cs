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
            var client = new Client("127.0.0.1", 9876);

            client.Send("Siema");
            var respond = client.Respond;
            Console.WriteLine(respond);
            

            client.Send("exit");
            respond = client.Respond;
            Console.WriteLine(respond);

            Console.WriteLine("Client exit...");
            Console.ReadKey();
        }
    }
}
