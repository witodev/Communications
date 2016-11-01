using System;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    class Server
    {
        // variables

        // properties
        public int Port { get; set; }
        
        // methonds
        private TcpListener _serverSocket;
        private TcpClient _clientSocket;

        public Server()
        { Port = 9876; }

        public void Start()
        {
            var ip = IPAddress.Parse("127.0.0.1");
            _serverSocket = new TcpListener(ip, Port);
            _serverSocket.Start();
            Console.WriteLine("- Server Started");

            var count = 0;
            while (true)
            {
                _clientSocket = _serverSocket.AcceptTcpClient();
                count++;
                Console.WriteLine("- Client accepted " + count);
                var newClient = new Client();
                newClient.Start(_clientSocket, count);
            }

            //_clientSocket.Close();
            //_serverSocket.Stop();
            //Console.WriteLine("- Server stops");
        }
    }
}
