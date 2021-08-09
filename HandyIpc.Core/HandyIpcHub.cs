namespace HandyIpc
{
    /// <summary>
    /// It represents the entry point that HandyIpc provides for user usage.
    /// </summary>
    public class HandyIpcHub
    {
        /// <summary>
        /// Creates a new builder of the IPC hub to build the hub instance.
        /// </summary>
        /// <returns>The factory of the IPC hub.</returns>
        public static IHubBuilder CreateBuilder() => new HubBuilder();
    }
}
