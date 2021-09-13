using System;
using HandyIpc.Core;

namespace HandyIpc
{
    public interface IServerConfiguration : IConfiguration<IServerConfiguration>
    {
        /// <summary>
        /// Use a <see cref="IServer"/> provider for the underlying communication.
        /// </summary>
        /// <param name="factory">
        /// A factory of the <see cref="IServer"/> type for RMI (remote method invocation).
        /// </param>
        /// <returns>The interface instance itself.</returns>
        IServerConfiguration Use(Func<IServer> factory);
    }
}
