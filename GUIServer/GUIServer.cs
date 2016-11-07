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
    public partial class GUIServer : Form
    {
        private List<ConnectedClient> _clients = new List<ConnectedClient>();
        private ManualResetEvent allDone = new ManualResetEvent(false);
        //private ManualResetEvent receiveDone = new ManualResetEvent(false);
        //private ManualResetEvent sendDone = new ManualResetEvent(false);
        //private ulong _clientCount = 0;

        private int Port;
        private Thread ListenThread;
        private bool working;
        private IPAddress ipAddress;
        private IPEndPoint localEndPoint;

        private void AddText(string txt)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(AddText), new object[] { txt });
                return;
            }
            txtLog.AppendText(txt + Environment.NewLine);
        }

        private void UpdateClientList()
        {
            if(InvokeRequired)
            {
                Invoke(new Action(UpdateClientList));
            }
            _clients = _clients.Where(x => x.state == EState.Connected).Select(x => x).ToList();            
        }

        public GUIServer()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (ListenThread != null && ListenThread.IsAlive == true)
                return;

            Port = int.Parse(txtPort.Text);
            ipAddress = IPAddress.Parse("0.0.0.0");
            localEndPoint = new IPEndPoint(ipAddress, Port);
            working = true;
            ListenThread = new Thread(ListenForClients);

            ListenThread.Start();
        }

        private void ListenForClients()
        {
            // Create a TCP/IP socket.
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);
                while (working)
                {
                    allDone.Reset(); // reset all signal
                    AddText("Waiting for client...");
                    listener.BeginAccept(new AsyncCallback(AcceptClient), listener); // listen for client
                    allDone.WaitOne(); // wait for connection
                }
                AddText("END");
                // todo: remove clients

                foreach (var item in _clients)
                {
                    item.state = EState.Disconected;
                }
            }
            catch (Exception exp)
            {
                AddText(exp.Message);
            }
            //listener.Shutdown(SocketShutdown.Both);
            listener.Close();
            //listener = null;
        }

        private void AcceptClient(IAsyncResult ar)
        {
            try
            {
                if (working == false)
                    return;

                var listener = (Socket)ar.AsyncState;
                var clientSocket = listener.EndAccept(ar);
                
                allDone.Set();

                var client = new ConnectedClient();
                client.ID = (ulong)_clients.Count;
                client.socket = clientSocket;
                client.state = EState.Connected;

                _clients.Add(client);
                UpdateClientList();

                ReceiveFromClient(client);
            }
            catch (Exception exp)
            {
                AddText("Accept error: " + exp.Message);
                working = false;
                allDone.Set();
            }
        }

        private void ReceiveFromClient(ConnectedClient client)
        {
            var clientSocket = client.socket;
            clientSocket.BeginReceive(client.buffer, 0, ConnectedClient.MaxBuffer, SocketFlags.None, new AsyncCallback(ReceiveClient), client);
        }

        private void ReceiveClient(IAsyncResult ar)
        {
            var client = (ConnectedClient)ar.AsyncState;
            if (client.state == EState.Disconected || working == false)
            {
                CloseClient(client);
                return;
            }
            var socket = client.socket;
            
            //receiveDone.Set();
            int bytesRead;
            try
            {
                // Read data from the client socket. 
                bytesRead = socket.EndReceive(ar);
            }
            catch (Exception exp)
            {
                AddText("Receive error: " + exp.Message);
                client.state = EState.Disconected;
                // close connection
                CloseClient(client);
                return;
            }

            if (bytesRead > 0)
            {
                // There  might be more data, so store the data received so far.
                client.sb.Append(Encoding.ASCII.GetString(client.buffer, 0, bytesRead));

                // Check for end-of-file tag. If it is not there, read 
                // more data.
                var content = client.sb.ToString();
                if (content.IndexOf("<EOF>") > -1)
                {
                    // All the data has been read from the 
                    // client. Display it on the console.
                    AddText(string.Format("Read {0} bytes from socket: {1}", content.Length, content));

                    SendToClient(client);

                    client.sb.Clear();
                    ReceiveFromClient(client);
                }
                else
                {
                    // Not all data received. Get more.
                    socket.BeginReceive(client.buffer, 0, ConnectedClient.MaxBuffer, 0, new AsyncCallback(ReceiveClient), client);
                }
            }
        }

        private void SendToClient(ConnectedClient client)
        {
            //sendDone.Reset();
            var socket = client.socket;
            var input = client.sb.ToString();
            input = input.Substring(0, input.Length - 5);

            var data = Encoding.ASCII.GetBytes(GetResponse(input) + "<EOF>");

            socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendClient), socket);
        }

        private string GetResponse(string input)
        {
            var lines = txtResp.Lines;
            var resp = lines.ToDictionary(o => o.Split('|')[0], o => o.Split('|')[1]);

            if (resp.ContainsKey(input))
                return resp[input];
            else
                return "notihing";
        }

        private void SendClient(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket socket = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int data = socket.EndSend(ar);
                AddText(string.Format("Sent {0} bytes to client.", data));

                //sendDone.Set();
                //CloseClient(handler);
            }
            catch (Exception e)
            {
                AddText("Send error: " + e.Message);
            }
        }

        private void CloseClient(ConnectedClient client)
        {
            var socket = client.socket;
            if (socket.Connected == true)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            _clients.Remove(client);
            UpdateClientList();
        }

        private void GUIServer_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void END(object sender, PaintEventArgs e)
        { 

        }

        private void button2_Click(object sender, EventArgs e)
        {
            working = false;
            allDone.Set();
        }

        private void GUIServer_FormClosing(object sender, FormClosingEventArgs e)
        {
            working = false;
            allDone.Set();
        }
    }

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
}
