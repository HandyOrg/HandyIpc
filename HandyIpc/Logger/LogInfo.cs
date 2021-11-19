using System;
using System.Threading;

namespace HandyIpc.Logger
{
    public readonly struct LogInfo
    {
        public DateTime Timestamp { get; }

        public string Message { get; }

        public string ScopeName { get; }

        public int ThreadId { get; }

        public Exception? Exception { get; }

        public LogInfo(string message, Exception? exception = null, string? scopeName = null, int? threadId = null)
        {
            Timestamp = DateTime.Now;
            Message = message;
            Exception = exception;
            ScopeName = scopeName ?? "UnknownScope";
            ThreadId = threadId ?? Thread.CurrentThread.ManagedThreadId;
        }
    }
}
