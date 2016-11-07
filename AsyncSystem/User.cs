using System.Net.Sockets;
using System.Text;

namespace AsyncSystem
{
    public class User
    {
        public int id;
        public Socket socket;
        public byte[] data;
        public StringBuilder sb = new StringBuilder();
        public EStatus status;
    }

    public enum EStatus
    {
        Connected = 0,
        Disconected
    }
}
