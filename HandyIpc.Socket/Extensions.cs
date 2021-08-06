using HandyIpc.Core;

namespace HandyIpc.Socket
{
    public static class Extensions
    {
        public static IIpcFactory<IRmiClient, IIpcClientHub> UseTcp(this IIpcFactory<IRmiClient, IIpcClientHub> self)
        {
            return self.Use(() => new TcpRmiClient());
        }

        public static IIpcFactory<IRmiServer, IIpcServerHub> UseTcp(this IIpcFactory<IRmiServer, IIpcServerHub> self, ILogger? logger = null)
        {
            return self.Use(() => new TcpRmiServer());
        }

        public static IIpcFactory<IRmiClient, IIpcClientHub> UseUdp(this IIpcFactory<IRmiClient, IIpcClientHub> self)
        {
            return self.Use(() => new UdpRmiClient());
        }

        public static IIpcFactory<IRmiServer, IIpcServerHub> UseUdp(this IIpcFactory<IRmiServer, IIpcServerHub> self, ILogger? logger = null)
        {
            return self.Use(() => new UdpRmiServer());
        }
    }
}
