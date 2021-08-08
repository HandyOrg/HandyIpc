using System;
using System.Collections.Concurrent;

namespace HandyIpc.Implementation
{
    public class Pool<TKey, TValue>
    {
        private readonly Func<TKey, TValue> _factory;
        private readonly Func<TValue, bool> _checkValue;
        private readonly ConcurrentDictionary<TKey, ConcurrentBag<TValue>> _cache = new();

        public Pool(Func<TKey, TValue> factory, Func<TValue, bool>? checkValue = null)
        {
            _factory = factory;
            _checkValue = checkValue ?? (_ => true);
        }

        public IRentedValue<TValue> Rent(TKey key)
        {
            TValue value = TakeOrCreateValue(key);
            return new RentedValue<TKey, TValue>(key, value, ReturnValue);

            // Local method
            void ReturnValue(TKey rentedKey, TValue rentedValue) => GetBag(rentedKey).Add(rentedValue);
        }

        private TValue TakeOrCreateValue(TKey key)
        {
            ConcurrentBag<TValue> bag = GetBag(key);

            TValue result;
            while (!bag.TryTake(out result) || !_checkValue(result))
            {
                bag.Add(_factory(key));
            }

            return result;
        }

        private ConcurrentBag<TValue> GetBag(TKey key) => _cache.GetOrAdd(key, _ => new ConcurrentBag<TValue>());
    }
}
