﻿using System;
using System.Threading.Tasks;

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

        public void Dispose() => _dispose?.Invoke(Value);
    }

    internal sealed class AsyncDisposableValue<T> /*: IAsyncDisposable*/
    {
        private readonly Func<T, /*Value*/Task> _dispose;

        public T Value { get; }

        public AsyncDisposableValue(T value, Func<T, /*Value*/Task> dispose)
        {
            Guards.ThrowIfNull(dispose, nameof(dispose));

            Value = value;
            _dispose = dispose;
        }

        public /*Value*/Task DisposeAsync() => _dispose(Value);
    }

    internal sealed class Disposable : IDisposable
    {
        private readonly Action _dispose;

        public Disposable(Action dispose) => _dispose = dispose;

        public void Dispose() => _dispose?.Invoke();
    }
}
