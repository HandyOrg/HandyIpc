using HandyIpc.Client;
using HandyIpc.Server;

namespace HandyIpc.NamedPipe
{
    public static class Extensions
    {
        private const long BufferSize = 4 * 1024;

        public static IIpcFactory<IRmiClient, IIpcClientHub> UseNamedPipe(
            this IIpcFactory<IRmiClient, IIpcClientHub> self,
            long bufferSize = BufferSize)
        {
            return self.Use(() => new RmiClient(bufferSize, new JsonSerializer()));
        }

        public static IIpcFactory<IRmiServer, IIpcServerHub> UseNamedPipe(
            this IIpcFactory<IRmiServer, IIpcServerHub> self,
            long bufferSize = BufferSize,
            ILogger? logger = null)
        {
            return self.Use(() => new RmiServer(bufferSize, new JsonSerializer(), logger ?? new DebugLogger()));
        }
    }
}
