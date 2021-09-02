using System;

namespace HandyIpc
{
    public interface IClientConfiguration : IConfiguration<IClientConfiguration>
    {
        /// <summary>
        /// Use a <see cref="IClient"/> provider for the underlying communication.
        /// </summary>
        /// <param name="factory">
        /// A factory of the <see cref="IClient"/> type for RMI (remote method invocation).
        /// </param>
        /// <returns>The interface instance itself.</returns>
        IClientConfiguration Use(Func<IClient> factory);
    }
}
