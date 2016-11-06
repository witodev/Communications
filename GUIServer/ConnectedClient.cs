using System.Net.Sockets;
using System.Text;

namespace GUIServer
{
    public class ConnectedClient
    {
        public ulong ID { get; private set; }
        private Socket socket;
        private byte[] buffer;
        private const int MaxBuffer = 1024;
        private StringBuilder sb = new StringBuilder();
    }
}
