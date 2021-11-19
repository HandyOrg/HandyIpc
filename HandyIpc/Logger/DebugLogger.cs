using System;
using System.Diagnostics;

namespace HandyIpc.Logger
{
    public sealed class DebugLogger : ILogger
    {
        public LogLevel EnabledLevel { get; set; }

        public void Log(LogLevel level, LogInfo info)
        {
            if (!this.IsEnabled(level))
            {
                return;
            }

            string message =
                $"[HandyIpc] {info.Timestamp:HH:mm.ss.fff} {info.ScopeName} [{info.ThreadId}]{Environment.NewLine}" +
                $"{info.Message}{Environment.NewLine}";

            Exception? exception = info.Exception;
            if (exception is not null)
            {
                message += $"{exception.Message}{Environment.NewLine}" +
                           $"{exception.StackTrace}{Environment.NewLine}";
            }

            Trace.WriteLine(message, level.ToString());
        }
    }
}
