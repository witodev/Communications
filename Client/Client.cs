using System;
using System.Net;
using System.Net.Sockets;

namespace Client
{
    class Client
    {
        // variables
        private TcpClient _clientSocket;
        private NetworkStream _networkStream;

        // properties
        public string IP { get; set; }
        public int Port { get; set; }
        public string Respond { get; private set; }

        // methods
        public Client(string ip, int port)
        {
            IP = ip;
            Port = port;

            Start();
        }

        private void Start()
        {
            _clientSocket = new TcpClient();
            _clientSocket.Connect(IPAddress.Parse(IP), Port);
        }

        public void Send(string v)
        {
            _networkStream = _clientSocket.GetStream();
            var toSend = System.Text.Encoding.ASCII.GetBytes(v);
            _networkStream.Write(toSend, 0, toSend.Length);
            _networkStream.Flush();
        }

        private void Read()
        {
            var toRead = new byte[1024];
            _networkStream = _clientSocket.GetStream();
            _networkStream.Read(toRead, 0, _clientSocket.ReceiveBufferSize);
            Respond = System.Text.Encoding.ASCII.GetString(toRead);
        }
    }
}
