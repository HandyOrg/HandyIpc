using System;
using System.Diagnostics;

namespace HandyIpc
{
    internal sealed class DebugLogger : ILogger
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
}