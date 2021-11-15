using System;
using System.ComponentModel;
using HandyIpcTests.Interfaces;

namespace HandyIpcTests.Implementations
{
    internal class EventType : IEventType
    {
        public event EventHandler? Changed;

        public event EventHandler<string>? EventWithArgs;

        public event PropertyChangedEventHandler? PropertyChanged;

        public void RaiseChanged(EventArgs e)
        {
            Changed?.Invoke(this, e);
        }
    }
}
