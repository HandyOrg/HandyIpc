using System;
using HandyIpcTests.Interfaces;

namespace HandyIpcTests.Implementations
{
    internal class EventType : IEventType
    {
        public event EventHandler? Changed;

        public void RaiseChanged(EventArgs e)
        {
            Changed?.Invoke(this, e);
        }
    }
}
