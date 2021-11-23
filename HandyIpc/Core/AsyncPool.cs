using System;
using System.Threading.Tasks;

namespace HandyIpc.Core
{
    public sealed class AsyncPool<TValue> : PoolBase<TValue> where TValue : IDisposable
    {
        private readonly Func<Task<T>> _factory;
        private readonly Func<T, Task<bool>> _checkValue;

        public AsyncPool(Func<Task<TValue>> factory, Func<TValue, Task<bool>> checkValue)
        {
            _factory = factory;
            _checkValue = checkValue;
        }

        public async Task<RentedValue<T>> RentAsync()
        {
            CheckDisposed(nameof(AsyncPool<T>));

            T value = await TakeOrCreateValue();
            return new RentedValue<T>(value, ReturnValue);

            // Local method
            void ReturnValue(T rentedValue) => Cache.Add(rentedValue);
        }

        private async Task<T> TakeOrCreateValue()
        {
            T result;
            while (!Cache.TryTake(out result) || !await _checkValue(result))
            {
                Cache.Add(await _factory());
            }

            return result;
        }
    }
}
