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
        /// <returns>An IPC client proxy singleton of the <typeparamref name="T"/> type.</returns>
        T Of<T>();
    }
}
