using System;
using System.Net.Sockets;
using System.Text;

namespace GUIServer
{
    public class ConnectedClient
    {
        public ulong ID { get; set; }
        public Socket socket;
        public byte[] buffer = new byte[MaxBuffer];
        public const int MaxBuffer = 1024;
        public StringBuilder sb = new StringBuilder();
        public EState state;

        internal void CheckStatus()
        {
            if (socket.Connected == true)
                state = EState.Connected;
            else
                state = EState.Disconected;
        }
    }

    public enum EState : int
    {
        Connected = 0,
        Disconected
    }
}
