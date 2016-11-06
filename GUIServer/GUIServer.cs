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
        private ManualResetEvent receiveDone = new ManualResetEvent(false);
        //private ManualResetEvent sendDone = new ManualResetEvent(false);       
        //private ulong _clientCount = 0;

        private int Port;
        private Thread ListenForClients;
        private bool working;
        private IPAddress ipAddress;
        private IPEndPoint localEndPoint;

        //delegate void SetTextCallback(TextBox textBox, string text);
        //private void SetText(TextBox textBox, string text)
        //{
        //    if (textBox.InvokeRequired)
        //    {
        //        SetTextCallback d = new SetTextCallback(SetText);
        //        this.Invoke(d, new object[] { textBox, text });
        //    }
        //    else
        //    {
        //        textBox.Text += text + Environment.NewLine;
        //    }
        //}

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
            var list = _clients.Where(x => x.state == EState.Connected).Select(x => "Client #" + x.ID.ToString()).ToList();
            lClients.DataSource = list;
        }

        public GUIServer()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Port = int.Parse(txtPort.Text);
            ipAddress = IPAddress.Parse("0.0.0.0");
            localEndPoint = new IPEndPoint(ipAddress, Port);
            working = true;
            ListenForClients = new Thread(Listen);

            ListenForClients.Start();
        }

        private void Listen()
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
            }
            catch (Exception exp)
            {
                AddText(exp.Message);
            }
            
        }

        private void AcceptClient(IAsyncResult ar)
        {
            var listener = (Socket)ar.AsyncState;
            var clientSocket = listener.EndAccept(ar);
            
            allDone.Set();

            var client = new ConnectedClient();
            client.ID = (ulong)_clients.Count;
            client.socket = clientSocket;
            client.state = EState.Connected;

            _clients.Add(client);
            UpdateClientList();
            
            clientSocket.BeginReceive(client.buffer, 0, ConnectedClient.MaxBuffer, SocketFlags.None, new AsyncCallback(ReceiveClient), client);
        }

        private void ReceiveClient(IAsyncResult ar)
        {
            var client = (ConnectedClient)ar.AsyncState;
            var socket = client.socket;
            
            receiveDone.Set();
            int bytesRead;
            try
            {
                // Read data from the client socket. 
                bytesRead = socket.EndReceive(ar);
            }
            catch (Exception exp)
            {
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
                    client.sb.Clear();
                    socket.BeginReceive(client.buffer, 0, ConnectedClient.MaxBuffer, SocketFlags.None, new AsyncCallback(ReceiveClient), client);
                }
                else
                {
                    // Not all data received. Get more.
                    socket.BeginReceive(client.buffer, 0, ConnectedClient.MaxBuffer, 0, new AsyncCallback(ReceiveClient), client);
                }
            }
        }

        private void CloseClient(ConnectedClient client)
        {
            var socket = client.socket;
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            _clients.Remove(client);
            UpdateClientList();
        }

        private void GUIServer_FormClosed(object sender, FormClosedEventArgs e)
        {

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
