using HandyIpc.Core;

namespace HandyIpc
{
    /// <summary>
    /// It represents the entry point that HandyIpc provides for user usage.
    /// </summary>
    public class HandyIpcHub
    {
        /// <summary>
        /// Creates a new factory of the IPC client hub to build the <see cref="IClientHub"/> instance
        /// with the specified <see cref="SenderBase"/> provider.
        /// </summary>
        /// <returns>The factory of the IPC client hub.</returns>
        public static IHubBuilder<SenderBase, IClientHub> CreateClientBuilder()
        {
            return new HubBuilder<SenderBase, IClientHub>(
                (rmiClient, serializer, logger) =>
                {
                    rmiClient.SetLogger(logger);
                    return new ClientHub(rmiClient, serializer);
                });
        }

        /// <summary>
        /// Creates a new factory of the IPC server hub to build the <see cref="IServerHub"/> instance
        /// with the specified <see cref="ReceiverBase"/> provider.
        /// </summary>
        /// <returns>The factory of the IPC server hub.</returns>
        public static IHubBuilder<ReceiverBase, IServerHub> CreateServerBuilder()
        {
            return new HubBuilder<ReceiverBase, IServerHub>(
                (rmiServer, serializer, logger) =>
                {
                    rmiServer.SetLogger(logger);
                    return new ServerHub(rmiServer, serializer, logger);
                });
        }
    }
}
