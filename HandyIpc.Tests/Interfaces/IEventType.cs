using System;
using System.ComponentModel;
using HandyIpc;

namespace HandyIpcTests.Interfaces
{
    [IpcContract]
    public interface IEventType
    {
        event EventHandler Changed;

        event EventHandler<string> EventWithArgs;

        event PropertyChangedEventHandler PropertyChanged;

        public void RaiseChanged(EventArgs e);
    }
}
