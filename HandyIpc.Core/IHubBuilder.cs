using System;
using HandyIpc.Core;

namespace HandyIpc
{
    /// <summary>
    /// It represents the factory of the IPC server or client hub.
    /// </summary>
    public interface IHubBuilder
    {
        /// <summary>
        /// Use a <see cref="SenderBase"/> provider for the underlying communication.
        /// </summary>
        /// <param name="factory">
        /// A factory of the <see cref="SenderBase"/> type for RMI (remote method invocation).
        /// </param>
        /// <returns>The interface instance itself.</returns>
        IHubBuilder Use(Func<SenderBase> factory);

        /// <summary>
        /// Use a <see cref="ReceiverBase"/> provider for the underlying communication.
        /// </summary>
        /// <param name="factory">
        /// A factory of the <see cref="ReceiverBase"/> type for RMI (remote method invocation).
        /// </param>
        /// <returns>The interface instance itself.</returns>
        IHubBuilder Use(Func<ReceiverBase> factory);

        /// <summary>
        /// Use a <see cref="ISerializer"/> provider for serialization and deserialization.
        /// </summary>
        /// <param name="factory">A factory for creating the <see cref="ISerializer"/> instance.</param>
        /// <returns>The interface instance itself.</returns>
        IHubBuilder Use(Func<ISerializer> factory);

        /// <summary>
        /// Use a <see cref="ILogger"/> provider for logging.
        /// </summary>
        /// <param name="factory">A factory for creating the <see cref="ILogger"/> instance.</param>
        /// <returns>The interface instance itself.</returns>
        IHubBuilder Use(Func<ILogger> factory);

        /// <summary>
        /// Builds an instance of the <see cref="IClientHub"/> type with specified factories.
        /// </summary>
        /// <returns>An instance of the <see cref="IClientHub"/> type.</returns>
        IClientHub BuildClientHub();

        /// <summary>
        /// Builds an instance of the <see cref="IServerHub"/> type with specified factories.
        /// </summary>
        /// <returns>An instance of the <see cref="IServerHub"/> type.</returns>
        IServerHub BuildServerHub();
    }
}
