using System;

namespace HandyIpc.Core
{
    public readonly struct RentedValue<TValue> : IDisposable
    {
        private readonly Action<TValue> _dispose;

        public TValue Value { get; }

        public RentedValue(TValue value, Action<TValue> dispose)
        {
            _dispose = dispose;
            Value = value;
        }

        void IDisposable.Dispose() => _dispose(Value);
    }
}
