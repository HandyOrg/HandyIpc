using System;
using HandyIpc;

namespace HandyIpcTests.Interfaces
{
    [IpcContract]
    public interface IEventType
    {
        event EventHandler Changed;

        public void RaiseChanged(EventArgs e);
    }
}
