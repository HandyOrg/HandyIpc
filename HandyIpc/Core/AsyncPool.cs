using System;
using System.Threading.Tasks;

namespace HandyIpc.Core
{
    public sealed class AsyncPool<TValue> : PoolBase<TValue> where TValue : IDisposable
    {
        private readonly Func<Task<TValue>> _factory;
        private readonly Func<TValue, Task<bool>> _checkValue;

        public AsyncPool(Func<Task<TValue>> factory, Func<TValue, Task<bool>> checkValue)
        {
            _factory = factory;
            _checkValue = checkValue;
        }

        public async Task<RentedValue<TValue>> RentAsync()
        {
            CheckDisposed("AsyncPool");

            TValue value = await TakeOrCreateValue();
            return new RentedValue<TValue>(value, ReturnValue);

            // Local method
            void ReturnValue(TValue rentedValue) => Cache.Add(rentedValue);
        }

        private async Task<TValue> TakeOrCreateValue()
        {
            TValue result;
            while (!Cache.TryTake(out result) || !await _checkValue(result))
            {
                Cache.Add(await _factory());
            }

            return result;
        }
    }
}
