using HandyIpc.Core;

namespace HandyIpc.Socket
{
    public static class Extensions
    {
        public static IHubBuilder<RmiClientBase, IClientHub> UseTcp(this IHubBuilder<RmiClientBase, IClientHub> self)
        {
            return self.Use(() => new TcpRmiClient());
        }

        public static IHubBuilder<RmiServerBase, IServerHub> UseTcp(this IHubBuilder<RmiServerBase, IServerHub> self, ILogger? logger = null)
        {
            return self.Use(() => new TcpRmiServer());
        }

        public static IHubBuilder<RmiClientBase, IClientHub> UseUdp(this IHubBuilder<RmiClientBase, IClientHub> self)
        {
            return self.Use(() => new UdpRmiClient());
        }

        public static IHubBuilder<RmiServerBase, IServerHub> UseUdp(this IHubBuilder<RmiServerBase, IServerHub> self, ILogger? logger = null)
        {
            return self.Use(() => new UdpRmiServer());
        }
    }
}
