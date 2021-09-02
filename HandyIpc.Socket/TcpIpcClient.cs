using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace HandyIpc.Socket
{
    internal class TcpIpcClient : IClient
    {
        private readonly IPAddress _ip;
        private readonly int _port;

        public TcpIpcClient(IPAddress ip, int port)
        {
            _ip = ip;
            _port = port;
        }

        public IConnection Connect()
        {
            TcpClient client = new();
            client.Connect(_ip, _port);
            return new StreamConnection(client.GetStream());
        }

        public async Task<IConnection> ConnectAsync()
        {
            TcpClient client = new();
            await client.ConnectAsync(_ip, _port);
            return new StreamConnection(client.GetStream());
        }
    }
}
