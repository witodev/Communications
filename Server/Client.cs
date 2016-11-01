using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server
{
    class Client
    {
        private TcpClient _clientSocket;
        private int _id;

        public void Start(TcpClient clientSocket, int count)
        {
            _id = count;
            _clientSocket = clientSocket;

            StartNewThread();
        }

        public void Start(TcpClient clientSocket)
        {
            _id = -1;
            _clientSocket = clientSocket;

            StartNewThread();
        }

        private void StartNewThread()
        {
            var clientThread = new Thread(work);
            clientThread.Start();
        }

        private void work()
        {
            byte[] bytesFrom = new byte[1024];
            string dataFromClient = null;
            Byte[] sendBytes = null;
            string serverResponse = null;
          
            while ((true))
            {
                try
                {
                    NetworkStream networkStream = _clientSocket.GetStream();

                    networkStream.Read(bytesFrom, 0, _clientSocket.ReceiveBufferSize);
                    dataFromClient = Encoding.ASCII.GetString(bytesFrom);
                    //dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));
                    Console.WriteLine("Client (" + _id + ") sends: " + dataFromClient);
                    
                    serverResponse = "Server to clinet(" + _id + ") sends: " + _id;
                    sendBytes = Encoding.ASCII.GetBytes(serverResponse);
                    networkStream.Write(sendBytes, 0, sendBytes.Length);
                    networkStream.Flush();
                    Console.WriteLine(serverResponse);

                    if (dataFromClient == "exit")
                        break;
                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine(" >> " + ex.ToString());
                }
            }

            Console.WriteLine("- Client " + _id + " closed");
        }

    }
}
