
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace AsyncSystem
{
    public class Server
    {
        // variables
        private const int MaxBuffer = 1024;
        private Socket _listener;
        private Thread _listenerThread;

        public ManualResetEvent allDone = new ManualResetEvent(false);
        private IPEndPoint _localEndPoint;
        private bool _working;

        // properties
        public int Port { get; set; }

        // events
        public event EventHandler<User> Response;

        // methods
        private void log(string msg)
        {
            Console.WriteLine(msg);
        }

        public void Start()
        {
            IPAddress ipAddress = IPAddress.Parse("0.0.0.0");
            _localEndPoint = new IPEndPoint(ipAddress, Port);

            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _working = true;
            _listenerThread = new Thread(ListenerFunction);
            _listenerThread.Start();
        }

        private void ListenerFunction()
        {
            try
            {
                _listener.Bind(_localEndPoint);
                _listener.Listen(10);

                while (_working)
                {
                    // Set the event to nonsignaled state.
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.
                    Console.WriteLine("Waiting for a connection...");
                    _listener.BeginAccept(new AsyncCallback(AcceptClient), _listener);

                    // Wait until a connection is made before continuing.
                    allDone.WaitOne();
                }
            }
            catch (Exception exp)
            {
                log("Listener error: " + exp.Message);
            }
            log("Server is OFF");
        }

        private void AcceptClient(IAsyncResult ar)
        {
            var socket = (Socket)ar.AsyncState; // get listening socket
            if (socket.Connected == false) // check if socket is connected
                return;

            var client = socket.EndAccept(ar); // get connetcted client
            allDone.Set(); // contitue accepting clients
            if (client.Connected == false) // check if client is connected
                return;

            var user = new User(); // create new client to hold state
            user.id = 0;
            user.socket = client;
            user.data = new byte[MaxBuffer];
            user.status = EStatus.Connected;

            ReceiveFromUser(user); // reads data from client         
        }

        private void ReceiveFromUser(User user)
        {
            var socket = user.socket;
            if (socket.Connected == false)
            {
                user.status = EStatus.Disconected;
                return;
            }

            socket.BeginReceive(user.data, 0, MaxBuffer, SocketFlags.None, new AsyncCallback(ReceiveCallback), user);
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            var user = (User)ar.AsyncState; // get user from async state
            if (user.status == EStatus.Disconected) // check if user is connected
                return;
            var socket = user.socket;
            
            var count = socket.EndReceive(ar); // number of received bytes

            if (count > 0) // if we received something
            {
                var msg = Encoding.ASCII.GetString(user.data, 0, count); // decode bytes to string
                user.sb.Append(msg); // add to string
                if (msg.IndexOf("<EOF>") > -1) // if this is end of message
                {
                    msg = user.sb.ToString(); // build whole message
                    //msg = msg.Substring(0, msg.Length - 5);
                    log("Client -> Server: " + msg); // log it
                    // and response
                    Response.Invoke(this, user);
                    SendToUser(user);
                    user.sb.Clear();
                }
            }
            ReceiveFromUser(user); // listen for more data
        }

        private void SendToUser(User user)
        {
            if (user.status == EStatus.Disconected) // check if user is connected
                return;

            var socket = user.socket;
            var msg = user.sb.ToString();
            var data = Encoding.ASCII.GetBytes(msg); // code string to bytes
            // send bytes to user
            socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), user);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                var user = (User)ar.AsyncState;
                if (user.status == EStatus.Disconected)
                    return;
                var socket = user.socket;

                var count = socket.EndSend(ar); // finalize send
                log("SendCallback: " + count + " bytes sent");
            }
            catch (Exception exp)
            {
                log("SendCallback error: " + exp.Message);
            }
        }

        public void Stop()
        {
            _working = false;
            allDone.Set();
            if (_listener.Connected == true)
                _listener.Shutdown(SocketShutdown.Both);
            _listener.Close();
        }
    }
}
