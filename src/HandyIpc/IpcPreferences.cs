using System;

namespace HandyIpc
{
    public class IpcPreferences
    {
        internal IpcPreferences() { }

        public long BufferSize { get; set; } = 4 * 1024;

        public ILogger Logger { get; set; } = new EmptyLogger();


        private class EmptyLogger : ILogger
        {
            void ILogger.Error(string message, Exception exception = null) { }

            void ILogger.Warning(string message, Exception exception = null) { }

            void ILogger.Info(string message, Exception exception = null) { }
        }
    }
}
