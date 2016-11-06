using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GUIServer
{
    public class ClientState
    {
        public ulong id = 0;
        public Socket socket = null; // client socket
        public const int BUFFERSIZE = 1024; // size of receive buffer
        public byte[] buffer = new byte[BUFFERSIZE]; // receive buffer
        public StringBuilder sb = new StringBuilder(); // string builder
        public bool close = false;
        public void Close()
        {
            //if (close == false)
            //    return;
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }
    }

    public partial class GUIServer : Form
    {
        private List<Socket> _clients = new List<Socket>();
        private ManualResetEvent allDone = new ManualResetEvent(false);
        private ManualResetEvent sendDone = new ManualResetEvent(false);
        private int Port;
        private ulong _clientCount = 0;
        
        delegate void SetTextCallback(TextBox textBox, string text);
        private void SetText(TextBox textBox, string text)
        {
            if (textBox.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { textBox, text });
            }
            else
            {
                textBox.Text += text + Environment.NewLine;
            }
        }

        private void AddText(string txt)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(AddText), new object[] { txt });
                return;
            }
            txtLog.AppendText(txt + Environment.NewLine);
        }

        public GUIServer()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Port = int.Parse(txtPort.Text);
            IPAddress ipAddress = IPAddress.Parse("0.0.0.0");
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, Port);

            // Create a TCP/IP socket.
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            var thread = new Thread(() =>
            {
                try
                {
                    listener.Bind(localEndPoint);
                    listener.Listen(100);
                    while (true)
                    {
                        allDone.Reset(); // reset all signal
                        AddText("Waiting for client...");
                        //LOG.AppendText("Waiting for client...");
                        listener.BeginAccept(new AsyncCallback(AcceptClient), listener); // listen for client
                        allDone.WaitOne(); // wait for connection
                    }
                }
                catch (Exception exp)
                {
                    SetText(txtLog, exp.Message);
                }
            });

            thread.Start();
        }

        private void AcceptClient(IAsyncResult ar)
        {
            // get listener
            var listener = (Socket)ar.AsyncState;
            var clientSocket = listener.EndAccept(ar); // get client from listener
            _clients.Add(clientSocket);
            SetText(txtLog, "Client accepted #" + (++_clientCount) + " (connected=" + _clients.Count + ")");

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
                var content = state.sb.ToString();
                if (content.IndexOf("<EOF>") > -1)
                {
                    content = content.Substring(0, content.Length - 5);
                    state.sb.Clear();
                    state.sb.Append(content);

                    SetText(txtLog, "Client #" + state.id + " writes...");

                        Send(state, state.sb.ToString());
                    

                    //if (content.IndexOf("close") > -1)
                    //{
                    //    clientSocket.BeginDisconnect(false, new AsyncCallback(DisconnectClient), state);
                    //}
                }
            }
            else
            {
                clientSocket.BeginReceive(state.buffer, 0, ClientState.BUFFERSIZE, SocketFlags.None, new AsyncCallback(ReadClient), state);
            }
        }

        private void Send(ClientState state, string msg)
        {
            var byteToSend = Encoding.ASCII.GetBytes(msg); // convert string to bytes
            var clientSocket = state.socket;
            clientSocket.BeginSend(byteToSend, 0, byteToSend.Length, SocketFlags.None, new AsyncCallback(SendClient), state);
        }

        private void SendClient(IAsyncResult ar)
        {
            try
            {
                var state = (ClientState)ar.AsyncState;
                var clientSocket = state.socket;
                var byteSent = clientSocket.EndSend(ar);
                clientSocket.BeginDisconnect(false, new AsyncCallback(DisconnectClient), state);

                sendDone.Set();
            }
            catch (Exception e)
            {
                SetText(txtLog, e.ToString());
            }
        }

        private void DisconnectClient(IAsyncResult ar)
        {
            sendDone.WaitOne();
            var state = (ClientState)ar.AsyncState;
            var clientSocket = state.socket;
            clientSocket.EndDisconnect(ar);
            CloseClient(clientSocket);
        }

        private void CloseClient(Socket clientSocket)
        {
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
            _clients.Remove(clientSocket);
        }

    }
}
