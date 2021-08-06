using HandyIpc.Core;

namespace HandyIpc.Socket
{
    public static class Extensions
    {
        public static IIpcFactory<RmiClientBase, IIpcClientHub> UseTcp(this IIpcFactory<RmiClientBase, IIpcClientHub> self)
        {
            return self.Use(() => new TcpRmiClient());
        }

        public static IIpcFactory<RmiServerBase, IIpcServerHub> UseTcp(this IIpcFactory<RmiServerBase, IIpcServerHub> self, ILogger? logger = null)
        {
            return self.Use(() => new TcpRmiServer());
        }

        public static IIpcFactory<RmiClientBase, IIpcClientHub> UseUdp(this IIpcFactory<RmiClientBase, IIpcClientHub> self)
        {
            return self.Use(() => new UdpRmiClient());
        }

        public static IIpcFactory<RmiServerBase, IIpcServerHub> UseUdp(this IIpcFactory<RmiServerBase, IIpcServerHub> self, ILogger? logger = null)
        {
            return self.Use(() => new UdpRmiServer());
        }
    }
}
