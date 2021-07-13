using HandyIpc.Client;
using HandyIpc.Server;

namespace HandyIpc.Tests.Mock
{
    public static class Extensions
    {
        public static IIpcFactory<IRmiClient, IIpcClientHub> UseMockClient(this IIpcFactory<IRmiClient, IIpcClientHub> self)
        {
            return self.Use(serializer => new MockRmiClient(serializer));
        }

        public static IIpcFactory<IRmiServer, IIpcServerHub> UseMockServer(this IIpcFactory<IRmiServer, IIpcServerHub> self)
        {
            return self.Use(serializer => new MockRmiServer(serializer));
        }

        public static IIpcFactory<TRmi, THub> UseMockSerializer<TRmi, THub>(this IIpcFactory<TRmi, THub> self)
        {
            return self.Use(() => new MockSerializer());
        }
    }
}
