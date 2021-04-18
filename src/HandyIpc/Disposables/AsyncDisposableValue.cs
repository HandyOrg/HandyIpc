﻿using System;
using System.Threading.Tasks;

namespace HandyIpc
{
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
}
