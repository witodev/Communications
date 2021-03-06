﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server
{
    // State object for reading client data asynchronously
    public class StateObject
    {
        // Client  socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 1024;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
    }

    public class AsyncServer
    {
        public List<Socket> _clients = new List<Socket>();

        // Thread signal.
        public ManualResetEvent allDone = new ManualResetEvent(false);
        private ManualResetEvent sendDone = new ManualResetEvent(false);
        public event EventHandler<StringBuilder> OnResponse;

        private int _port = 9876;
        public int Port
        {
            get
            {
                return _port;
            }
            set
            {
                _port = value;
            }
        }
        
        public AsyncServer()
        {
        }

        public void StartListening()
        {
            // Data buffer for incoming data.
            byte[] bytes = new byte[1024];

            // Establish the local endpoint for the socket.

            //IPHostEntry ipHostInfo = Dns.GetHostEntry(_ipAdress);

            //Console.WriteLine("Host info:");
            //int i;
            //for (i = 0; i < ipHostInfo.AddressList.Length; i++)
            //{
            //    var item = ipHostInfo.AddressList[i].ToString();
            //    Console.WriteLine(item);

            //    if (item.Contains("."))
            //    {
            //        break;
            //    }
            //}
            //IPAddress ipAddress = ipHostInfo.AddressList[i];
            IPAddress ipAddress = IPAddress.Parse("0.0.0.0");
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, _port);

            // Create a TCP/IP socket.
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for incoming connections.
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                while (true)
                {
                    // Set the event to nonsignaled state.
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.
                    Console.WriteLine("Waiting for a connection...");
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);

                    // Wait until a connection is made before continuing.
                    allDone.WaitOne();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);
            _clients.Add(handler);
            Console.WriteLine("\tClients count: " + _clients.Count);

            // Signal the main thread to continue.
            allDone.Set();

            // Create the state object.
            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
        }

        public void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket. 
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                // There  might be more data, so store the data received so far.
                state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                // Check for end-of-file tag. If it is not there, read 
                // more data.
                content = state.sb.ToString();
                if (content.IndexOf("<EOF>") > -1)
                {
                    // All the data has been read from the 
                    // client. Display it on the console.
                    // but remove <EOF> tag
                    Console.WriteLine("Read {0} bytes from socket.", content.Length);
                    content = content.Substring(0, content.Length - 5);
                    // Echo the data back to the client.
                    // with modif event
                    var close = false;
                    if (content.IndexOf("close") > -1)
                    {
                        close = true;
                    }
                    state.sb.Clear();
                    state.sb.Append(content);
                    OnResponse.Invoke(this, state.sb);
                    content = state.sb.ToString();
                    Send(handler, content);

                    if (close)
                    {
                        CloseClient(handler);
                    }
                }
                else
                {
                    // Not all data received. Get more.
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                }
            }
        }

        private void Send(Socket handler, String data)
        {
            sendDone.Reset();
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.
            handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);

                sendDone.Set();
                //CloseClient(handler);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void CloseClient(Socket handler)
        {
            sendDone.WaitOne();
            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
            _clients.Remove(handler);
        }
    }
}
