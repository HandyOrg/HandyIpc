﻿using System;

namespace HandyIpc
{
    public class IpcPreferences
    {
        internal IpcPreferences() { }

        public long BufferSize { get; set; } = 4 * 1024;

        public ILogger Logger { get; set; } = new EmptyLogger();


        private class EmptyLogger : ILogger
        {
            public void Error(string message, Exception exception = null) { }

            public void Warning(string message, Exception exception = null) { }

            public void Info(string message, Exception exception = null) { }
        }
    }
}
