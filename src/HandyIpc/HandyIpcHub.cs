using HandyIpc.Client;
using HandyIpc.Server;

namespace HandyIpc
{
    /// <summary>
    /// It represents the entry point that HandyIpc provides for user usage.
    /// </summary>
    public class HandyIpcHub
    {
        /// <summary>
        /// Creates a new factory of the IPC client hub to build the <see cref="IIpcClientHub"/> instance
        /// with the specified <see cref="IRmiClient"/> provider.
        /// </summary>
        /// <returns>The factory of the IPC client hub.</returns>
        public static IIpcFactory<IRmiClient, IIpcClientHub> CreateClientFactory()
        {
            return new IpcFactory<IRmiClient, IIpcClientHub>(rmiClient => new HandyIpcClientHub(rmiClient));
        }

        /// <summary>
        /// Creates a new factory of the IPC server hub to build the <see cref="IIpcServerHub"/> instance
        /// with the specified <see cref="IRmiServer"/> provider.
        /// </summary>
        /// <returns>The factory of the IPC server hub.</returns>
        public static IIpcFactory<IRmiServer, IIpcServerHub> CreateServerFactory()
        {
            return new IpcFactory<IRmiServer, IIpcServerHub>(rmiServer => new HandyIpcServerHub(rmiServer));
        }
    }
}
