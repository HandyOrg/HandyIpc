using System;
using System.Diagnostics;
using HandyIpc.Client;
using HandyIpc.Server;

namespace HandyIpc
{
    /// <summary>
    /// It represents the entry point that HandyIpc provides for user usage.
    /// </summary>
    public static class HandyIpcHub
    {
        private class DebugLogger : ILogger
        {
            public void Error(string message, Exception? exception = null)
            {
                Debug.WriteLine($"[HandyIpc] [ERROR] [{DateTime.Now:HH:mm:ss.fff}] " +
                                $"{message}{Environment.NewLine}" +
                                $"{exception?.Message}{Environment.NewLine}{exception?.StackTrace}");
            }

            public void Warning(string message, Exception? exception = null)
            {
                Debug.WriteLine($"[HandyIpc] [WARNING] [{DateTime.Now:HH:mm:ss.fff}] " +
                                $"{message}{Environment.NewLine}" +
                                $"{exception?.Message}{Environment.NewLine}{exception?.StackTrace}");
            }

            public void Info(string message, Exception? exception = null)
            {
                Debug.WriteLine($"[HandyIpc] [INFO] [{DateTime.Now:HH:mm:ss.fff}] " +
                                $"{message}{Environment.NewLine}" +
                                $"{exception?.Message}{Environment.NewLine}{exception?.StackTrace}");
            }
        }

        /// <summary>
        /// Some common preferences for the server and client.
        /// </summary>
        public static IpcPreferences Preferences { get; } = new();

        /// <summary>
        /// Gets or Sets an instance of the <see cref="ILogger"/>.
        /// </summary>
        public static ILogger Logger { get; set; } = new DebugLogger();

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
