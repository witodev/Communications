using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    public class ClientState
    {
        public ulong id = 0;
        public Socket socket = null; // client socket
        public const int BUFFERSIZE = 1024; // size of receive buffer
        public byte[] buffer = new byte[BUFFERSIZE]; // receive buffer
        public StringBuilder sb = new StringBuilder(); // string builder
    }

    class MyServer
    {        
        public int Port { get; set; }
        public List<Socket> _clients = new List<Socket>();

        private ulong _clientCount = 0;
        private ManualResetEvent allDone = new ManualResetEvent(false);
        public EventHandler<StringBuilder> OnResponse;

        public MyServer()
        { }

        public void Start()
        {
            IPAddress ipAddress = IPAddress.Parse("0.0.0.0");
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, Port);

            // Create a TCP/IP socket.
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);
                while (true)
                {
                    allDone.Reset(); // reset all signal
                    Console.WriteLine("Waiting for client...");
                    listener.BeginAccept(new AsyncCallback(AcceptClient), listener); // listen for client
                    allDone.WaitOne(); // wait for connection
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void AcceptClient(IAsyncResult ar)
        {
            // get listener
            var listener = (Socket)ar.AsyncState;
            var clientSocket = listener.EndAccept(ar); // get client from listener
            _clients.Add(clientSocket);
            Console.WriteLine("Client accepted ID=" + (++_clientCount) + " (connected=" + _clients.Count + ")");
            
            allDone.Set(); // continue main thread (listening on socket)

            // wait for client send data
            var state = new ClientState();
            state.socket = clientSocket;
            state.id = _clientCount;

            clientSocket.BeginReceive(state.buffer, 0, ClientState.BUFFERSIZE, SocketFlags.None, new AsyncCallback(ReadClient), state);
        }

        private void ReadClient(IAsyncResult ar)
        {
            var state = (ClientState)ar.AsyncState;
            var clientSocket = state.socket;

            var bytesRead = clientSocket.EndReceive(ar);

            if (bytesRead > 0)
            {
                state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead)); // decode bytes to string

            }
            else
            {
                clientSocket.BeginReceive(state.buffer, 0, ClientState.BUFFERSIZE, SocketFlags.None, new AsyncCallback(ReadClient), state);
            }
        }
    }
}
