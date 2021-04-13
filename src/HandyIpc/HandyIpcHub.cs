using System;
using HandyIpc.Client;
using HandyIpc.Server;

namespace HandyIpc
{
    /// <summary>
    /// It represents the entry point that HandyIpc provides for user usage.
    /// </summary>
    public static class HandyIpcHub
    {
        private class EmptyLogger : ILogger
        {
            void ILogger.Error(string message, Exception exception = null) { }

            void ILogger.Warning(string message, Exception exception = null) { }

            void ILogger.Info(string message, Exception exception = null) { }
        }

        /// <summary>
        /// Some common preferences for the server and client.
        /// </summary>
        public static IpcPreferences Preferences { get; } = new();

        public static ILogger Logger { get; } = new EmptyLogger();

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
