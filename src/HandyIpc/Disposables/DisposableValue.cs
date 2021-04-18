using System;

namespace HandyIpc
{
    internal sealed class DisposableValue<T> : IDisposable
    {
        private readonly Action<T> _dispose;

        public T Value { get; }

        public DisposableValue(T value, Action<T> dispose)
        {
            Guards.ThrowIfNull(dispose, nameof(dispose));

            Value = value;
            _dispose = dispose;
        }

        public void Dispose() => _dispose(Value);
    }
}
