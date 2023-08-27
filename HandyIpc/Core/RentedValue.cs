using System;

namespace HandyIpc.Core
{
    public readonly struct RentedValue<T> : IDisposable
    {
        private readonly Action<T> _dispose;

        public T Value { get; }

        public RentedValue(T value, Action<T> dispose)
        {
            _dispose = dispose;
            Value = value;
        }

        void IDisposable.Dispose() => _dispose(Value);
    }
}
