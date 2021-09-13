using System;
using HandyIpc.Core;

namespace HandyIpc
{
    public interface IConfiguration<out T> where T : IConfiguration<T>
    {
        /// <summary>
        /// Use a <see cref="ISerializer"/> provider for serialization and deserialization.
        /// </summary>
        /// <param name="factory">A factory for creating the <see cref="ISerializer"/> instance.</param>
        /// <returns>The interface instance itself.</returns>
        T Use(Func<ISerializer> factory);

        /// <summary>
        /// Use a <see cref="ILogger"/> provider for logging.
        /// </summary>
        /// <param name="factory">A factory for creating the <see cref="ILogger"/> instance.</param>
        /// <returns>The interface instance itself.</returns>
        T Use(Func<ILogger> factory);
    }
}
