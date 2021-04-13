using System;

namespace HandyIpc
{
    public class IpcPreferences
    {
        internal IpcPreferences() { }

        public long BufferSize { get; set; } = 4 * 1024;
    }
}
