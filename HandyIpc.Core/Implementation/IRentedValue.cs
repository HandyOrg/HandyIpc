using System;

namespace HandyIpc.Implementation
{
    public interface IRentedValue<out TValue> : IDisposable
    {
        TValue Value { get; }
    }
}
