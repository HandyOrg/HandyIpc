using System;
using System.Collections.Concurrent;

namespace HandyIpc.Core
{
    public abstract class PoolBase<T> : IDisposable where T : IDisposable
    {
        protected readonly ConcurrentBag<T> Cache = new();

        private bool _isDisposed;

        protected void CheckDisposed(string objectName)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(objectName);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            _isDisposed = true;

            if (disposing)
            {
                while (Cache.TryTake(out T item))
                {
                    item.Dispose();
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
