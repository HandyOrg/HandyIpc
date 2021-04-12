using HandyIpc.Client;
using HandyIpc.Server;

namespace HandyIpc
{
    /// <summary>
    /// It represents the entry point that HandyIpc provides for user usage.
    /// </summary>
    public static class HandyIpcHub
    {
        /// <summary>
        /// Some common preferences for the server and client.
        /// </summary>
        public static IpcPreferences Preferences { get; } = new IpcPreferences();

        /// <summary>
        /// A singleton of the <see cref="IHandyIpcServerHub"/>.
        /// </summary>
        public static IHandyIpcServerHub Server { get; } = new HandyIpcServerHub();

        /// <summary>
        /// A singleton of the <see cref="IHandyIpcClientHub"/>.
        /// </summary>
        public static IHandyIpcClientHub Client { get; } = new HandyIpcClientHub();
    }
}
