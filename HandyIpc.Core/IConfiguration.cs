using System;
using HandyIpc.Core;

namespace HandyIpc
{
    public interface IConfiguration
    {
        /// <summary>
        /// Use a <see cref="SenderBase"/> provider for the underlying communication.
        /// </summary>
        /// <param name="factory">
        /// A factory of the <see cref="SenderBase"/> type for RMI (remote method invocation).
        /// </param>
        /// <returns>The interface instance itself.</returns>
        IConfiguration Use(Func<SenderBase> factory);

        /// <summary>
        /// Use a <see cref="ReceiverBase"/> provider for the underlying communication.
        /// </summary>
        /// <param name="factory">
        /// A factory of the <see cref="ReceiverBase"/> type for RMI (remote method invocation).
        /// </param>
        /// <returns>The interface instance itself.</returns>
        IConfiguration Use(Func<ReceiverBase> factory);

        /// <summary>
        /// Use a <see cref="ISerializer"/> provider for serialization and deserialization.
        /// </summary>
        /// <param name="factory">A factory for creating the <see cref="ISerializer"/> instance.</param>
        /// <returns>The interface instance itself.</returns>
        IConfiguration Use(Func<ISerializer> factory);

        /// <summary>
        /// Use a <see cref="ILogger"/> provider for logging.
        /// </summary>
        /// <param name="factory">A factory for creating the <see cref="ILogger"/> instance.</param>
        /// <returns>The interface instance itself.</returns>
        IConfiguration Use(Func<ILogger> factory);
    }
}
