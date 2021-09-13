using System.Net;

namespace HandyIpc.Socket
{
    public static class Extensions
    {
        public static IServerConfiguration UseTcp(this IServerConfiguration self, IPAddress ip, int port)
        {
            return self.Use(() => new TcpIpcServer(ip, port));
        }

        public static IClientConfiguration UseTcp(this IClientConfiguration self, IPAddress ip, int port)
        {
            return self.Use(() => new TcpIpcClient(ip, port));
        }
    }
}
