using System;

namespace HandyIpc.Implementation
{
    internal sealed class RentedValue<TKey, TValue> : IRentedValue<TValue>
    {
        private readonly TKey _key;
        private readonly Action<TKey, TValue> _dispose;

        public TValue Value { get; }

        public RentedValue(TKey key, TValue value, Action<TKey, TValue> dispose)
        {
            _key = key;
            _dispose = dispose;
            Value = value;
        }

        public void Dispose() => _dispose(_key, Value);
    }
}
