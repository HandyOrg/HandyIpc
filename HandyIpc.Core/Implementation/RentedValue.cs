using System;

namespace HandyIpc.Implementation
{
    internal sealed class RentedValue<TValue> : IRentedValue<TValue>
    {
        private readonly Action<TValue> _dispose;

        public TValue Value { get; }

        public RentedValue(TValue value, Action<TValue> dispose)
        {
            _dispose = dispose;
            Value = value;
        }

        public void Dispose() => _dispose(Value);
    }
}
