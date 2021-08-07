using System;
using HandyIpc.Core;

namespace HandyIpc
{
    /// <summary>
    /// It represents the factory of the IPC server or client hub.
    /// </summary>
    /// <typeparam name="TRmi">
    /// The service of remote method invocation, which can only be <see cref="RmiServerBase"/>
    /// or <see cref="RmiClientBase"/> type.
    /// </typeparam>
    /// <typeparam name="THub">
    /// The hub of the IPC server or client, which can only be <see cref="IServerHub"/>
    /// or <see cref="IClientHub"/> type.
    /// </typeparam>
    public interface IHubBuilder<in TRmi, out THub>
    {
        /// <summary>
        /// Use a <typeparamref name="TRmi"/> provider for the underlying communication.
        /// </summary>
        /// <param name="factory">
        /// A factory of the <typeparamref name="TRmi"/> (Remote method invocation) type,
        /// which can only return an instance derived from <see cref="RmiServerBase"/> or <see cref="RmiClientBase"/>.
        /// </param>
        /// <returns>The interface instance itself.</returns>
        IHubBuilder<TRmi, THub> Use(Func<TRmi> factory);

        /// <summary>
        /// Use a <see cref="ISerializer"/> provider for serialization and deserialization.
        /// </summary>
        /// <param name="factory">A factory for creating the <see cref="ISerializer"/> instance.</param>
        /// <returns>The interface instance itself.</returns>
        IHubBuilder<TRmi, THub> Use(Func<ISerializer> factory);

        /// <summary>
        /// Use a <see cref="ILogger"/> provider for logging.
        /// </summary>
        /// <param name="factory">A factory for creating the <see cref="ILogger"/> instance.</param>
        /// <returns>The interface instance itself.</returns>
        IHubBuilder<TRmi, THub> Use(Func<ILogger> factory);

        /// <summary>
        /// Builds an instance of the <typeparamref name="THub"/> type.
        /// </summary>
        /// <returns>An instance of the <typeparamref name="THub"/> type.</returns>
        THub Build();
    }
}
