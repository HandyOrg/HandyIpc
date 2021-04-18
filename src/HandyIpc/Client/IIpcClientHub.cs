namespace HandyIpc.Client
{
    /// <summary>
    /// It represents a hub of IPC client instances.
    /// </summary>
    public interface IIpcClientHub
    {
        /// <summary>
        /// Gets or adds a IPC client instance with the specified interface type,
        /// which is a proxy of remote server.
        /// </summary>
        /// <typeparam name="T">The specified interface type.</typeparam>
        /// <returns>The IPC client instance that implements the specified interface type.</returns>
        T Of<T>();
    }
}
