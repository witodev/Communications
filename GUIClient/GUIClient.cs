﻿using Client;
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

namespace GUIClient
{
    public partial class GUIClient : Form
    {
        int Port;
        string IPadress;


        private ManualResetEvent connectDone = new ManualResetEvent(false);
        private ManualResetEvent sendDone = new ManualResetEvent(false);
        private ManualResetEvent receiveDone = new ManualResetEvent(false);
        private Socket client;
        private string response;


        private void AddText(string txt)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(AddText), new object[] { txt });
                return;
            }
            txtLog.AppendText(txt + Environment.NewLine);
        }

        public GUIClient()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            IPadress = txtIP.Text;
            Port = int.Parse(txtPort.Text);

            try
            {
                // Establish the remote endpoint for the socket.

                IPHostEntry ipHostInfo = Dns.GetHostEntry(IPadress);

                Console.WriteLine("Host info:");
                int i = 0;
                for (i = 0; i < ipHostInfo.AddressList.Length; i++)
                {
                    var item = ipHostInfo.AddressList[i].ToString();
                    Console.WriteLine(item);

                    if (item.Contains("."))
                    {
                        break;
                    }
                }

                IPAddress ipAddress = ipHostInfo.AddressList[i];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, Port);

                // Create a TCP/IP socket.
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // Connect to the remote endpoint.
                client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), client);
                //connectDone.WaitOne();
            }
            catch (Exception exp)
            {
                AddText("Error: " + exp.Message);
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.
                client.EndConnect(ar);

                AddText("Socket connected to " + client.RemoteEndPoint.ToString());

                // Signal that the connection has been made.
                connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Send test data to the remote device.
            Send(client, txtToSend.Text + "<EOF>");
            sendDone.WaitOne();

            //// Receive the response from the remote device.
            //Receive(client);
            //receiveDone.WaitOne();

            //// Write the response to the console.
            //txtLog.AppendText("Response received : " + response);

            //// Release the socket.
            //client.Shutdown(SocketShutdown.Both);
            //client.Close();
        }

        private void Receive(Socket client)
        {
            try
            {
                // Create the state object.
                StateObject state = new StateObject();
                state.workSocket = client;

                // Begin receiving the data from the remote device.
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket 
                // from the asynchronous state object.
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // Read data from the remote device.
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                    // Get the rest of the data.
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    // All the data has arrived; put it in response.
                    if (state.sb.Length > 1)
                    {
                        response = state.sb.ToString();
                    }
                    // Signal that all bytes have been received.
                    receiveDone.Set();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void Send(Socket client, String data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.
            client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), client);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = client.EndSend(ar);
                //Console.WriteLine("Sent {0} bytes to server.", bytesSent);

                // Signal that all bytes have been sent.
                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
