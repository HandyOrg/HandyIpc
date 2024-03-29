using System;

namespace HandyIpc.Core
{
    public interface ILogger
    {
        void Error(string message, Exception? exception = null);

        void Warning(string message, Exception? exception = null);

        void Info(string message, Exception? exception = null);
    }
}
