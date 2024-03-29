using System;
using System.Diagnostics;
using HandyIpc.Core;

namespace HandyIpc
{
    public sealed class DebugLogger : ILogger
    {
        public void Error(string message, Exception? exception = null)
        {
            Print(nameof(Error), message, exception);
        }

        public void Warning(string message, Exception? exception = null)
        {
            Print(nameof(Warning), message, exception);
        }

        public void Info(string message, Exception? exception = null)
        {
            Print(nameof(Info), message, exception);
        }

        private static void Print(string level, string message, Exception? exception = null)
        {
            Debug.WriteLine($"[HandyIpc] [{level}] [{DateTime.Now:HH:mm:ss.fff}] " +
                            $"{message}{Environment.NewLine}" +
                            $"{exception?.GetType().Name}: {exception?.Message}{Environment.NewLine}{exception?.StackTrace}");
        }
    }
}
