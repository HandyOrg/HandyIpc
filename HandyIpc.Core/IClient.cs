using System;

namespace HandyIpc
{
    /// <summary>
    /// It represents a IPC client instances.
    /// </summary>
    public interface IClient : IDisposable
    {
        /// <summary>
        /// Gets or adds a IPC client instance with the specified interface type,
        /// which is a proxy of remote server.
        /// </summary>
        /// <typeparam name="T">The specified interface type.</typeparam>
        /// <param name="key">A key for mark the instance is registered.</param>
        /// <returns>An IPC client proxy singleton of the <typeparamref name="T"/> type.</returns>
        T Resolve<T>(string key);
    }
}
