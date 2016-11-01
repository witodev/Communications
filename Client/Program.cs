using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
            }
            Console.WriteLine("Exit...");
            Console.ReadKey();
        }
    }
}
