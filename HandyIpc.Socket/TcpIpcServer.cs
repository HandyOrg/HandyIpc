using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace HandyIpc.Socket
{
    internal class TcpIpcServer : IServer
    {
        private readonly TcpListener _tcpListener;

        public TcpIpcServer(IPAddress ip, int port)
        {
            _tcpListener = new TcpListener(ip, port);
            _tcpListener.Start();
        }

        public async Task<IConnection> WaitForConnectionAsync()
        {
            TcpClient client = await _tcpListener.AcceptTcpClientAsync();
            return new StreamConnection(client.GetStream());
        }

        public void Dispose() => _tcpListener.Stop();
    }
}
