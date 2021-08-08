namespace HandyIpc
{
    /// <summary>
    /// It represents a hub of IPC client instances.
    /// </summary>
    public interface IClientHub
    {
        /// <summary>
        /// Gets or adds a IPC client instance with the specified interface type,
        /// which is a proxy of remote server.
        /// </summary>
        /// <typeparam name="T">The specified interface type.</typeparam>
        /// <param name="accessToken">Allow to specify an access token for authentication.</param>
        /// <returns>An IPC client proxy singleton of the <typeparamref name="T"/> type.</returns>
        T Of<T>(string? accessToken = null);
    }
}
