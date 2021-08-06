using HandyIpc.Core;

namespace HandyIpc
{
    /// <summary>
    /// It represents the entry point that HandyIpc provides for user usage.
    /// </summary>
    public class HandyIpcHub
    {
        /// <summary>
        /// Creates a new factory of the IPC client hub to build the <see cref="IIpcClientHub"/> instance
        /// with the specified <see cref="RmiClientBase"/> provider.
        /// </summary>
        /// <returns>The factory of the IPC client hub.</returns>
        public static IIpcFactory<RmiClientBase, IIpcClientHub> CreateClientFactory()
        {
            return new IpcFactory<RmiClientBase, IIpcClientHub>(
                (rmiClient, serializer, logger) =>
                {
                    rmiClient.SetLogger(logger);
                    return new IpcClientHub(rmiClient, serializer);
                });
        }

        /// <summary>
        /// Creates a new factory of the IPC server hub to build the <see cref="IIpcServerHub"/> instance
        /// with the specified <see cref="RmiServerBase"/> provider.
        /// </summary>
        /// <returns>The factory of the IPC server hub.</returns>
        public static IIpcFactory<RmiServerBase, IIpcServerHub> CreateServerFactory()
        {
            return new IpcFactory<RmiServerBase, IIpcServerHub>(
                (rmiServer, serializer, logger) =>
                {
                    rmiServer.SetLogger(logger);
                    return new IpcServerHub(rmiServer, serializer, logger);
                });
        }
    }
}
