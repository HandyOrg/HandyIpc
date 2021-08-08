using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace HandyIpc.Implementation
{
    public class AsyncPool<TKey, TValue>
    {
        private readonly Func<TKey, Task<TValue>> _factory;
        private readonly Func<TValue, Task<bool>> _checkValue;
        private readonly ConcurrentDictionary<TKey, ConcurrentBag<TValue>> _cache = new();

        public AsyncPool(Func<TKey, Task<TValue>> factory, Func<TValue, Task<bool>>? checkValue = null)
        {
            _factory = factory;
            _checkValue = checkValue ?? (_ => Task.FromResult(true));
        }

        public async Task<IRentedValue<TValue>> RentAsync(TKey key)
        {
            TValue value = await TakeOrCreateValue(key);
            return new RentedValue<TKey, TValue>(key, value, ReturnValue);

            // Local method
            void ReturnValue(TKey rentedKey, TValue rentedValue) => GetBag(rentedKey).Add(rentedValue);
        }

        private async Task<TValue> TakeOrCreateValue(TKey key)
        {
            ConcurrentBag<TValue> bag = GetBag(key);

            TValue result;
            while (!bag.TryTake(out result) || !await _checkValue(result))
            {
                bag.Add(await _factory(key));
            }

            return result;
        }

        private ConcurrentBag<TValue> GetBag(TKey key) => _cache.GetOrAdd(key, _ => new ConcurrentBag<TValue>());
    }
}
