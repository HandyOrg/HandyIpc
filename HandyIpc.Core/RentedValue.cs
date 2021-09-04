using System;

namespace HandyIpc
{
    internal readonly struct RentedValue<TValue> : IDisposable
    {
        private readonly Action<TValue> _dispose;

        public TValue Value { get; }

        internal RentedValue(TValue value, Action<TValue> dispose)
        {
            _dispose = dispose;
            Value = value;
        }

        void IDisposable.Dispose() => _dispose(Value);
    }
}
