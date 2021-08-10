using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace HandyIpc.Implementation
{
    public class AsyncPool<TValue>
    {
        private readonly Func<Task<TValue>> _factory;
        private readonly Func<TValue, Task<bool>> _checkValue;
        private readonly ConcurrentBag<TValue> _cache = new();

        public AsyncPool(Func<Task<TValue>> factory, Func<TValue, Task<bool>>? checkValue = null)
        {
            _factory = factory;
            _checkValue = checkValue ?? (_ => Task.FromResult(true));
        }

        public async Task<IRentedValue<TValue>> RentAsync()
        {
            TValue value = await TakeOrCreateValue();
            return new RentedValue<TValue>(value, ReturnValue);

            // Local method
            void ReturnValue(TValue rentedValue) => _cache.Add(rentedValue);
        }

        private async Task<TValue> TakeOrCreateValue()
        {
            TValue result;
            while (!_cache.TryTake(out result) || !await _checkValue(result))
            {
                _cache.Add(await _factory());
            }

            return result;
        }
    }
}
