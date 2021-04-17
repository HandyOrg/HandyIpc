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
        /// Creates a new factory of the IPC server hub.
        /// </summary>
        /// <returns>The factory of the IPC server hub.</returns>
        public static IIpcFactory<IRmiClient, IIpcClientHub> CreateClientFactory()
        {
            return new IpcFactory<IRmiClient, IIpcClientHub>(rmiClient => new HandyIpcClientHub(rmiClient));
        }

        /// <summary>
        /// Creates a new factory of the IPC client hub.
        /// </summary>
        /// <returns>The factory of the IPC client hub.</returns>
        public static IIpcFactory<IRmiServer, IIpcServerHub> CreateServerFactory()
        {
            return new IpcFactory<IRmiServer, IIpcServerHub>(rmiServer => new HandyIpcServerHub(rmiServer));
        }
    }
}
