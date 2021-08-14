using System;
using System.Collections.Concurrent;

namespace HandyIpc.Implementation
{
    public class Pool<TValue>
    {
        private readonly Func<TValue> _factory;
        private readonly Func<TValue, bool> _checkValue;
        private readonly ConcurrentBag<TValue> _cache = new();

        public Pool(Func<TValue> factory, Func<TValue, bool>? checkValue = null)
        {
            _factory = factory;
            _checkValue = checkValue ?? (_ => true);
        }

        public RentedValue<TValue> Rent()
        {
            TValue value = TakeOrCreateValue();
            return new RentedValue<TValue>(value, ReturnValue);

            // Local method
            void ReturnValue(TValue rentedValue) => _cache.Add(rentedValue);
        }

        private TValue TakeOrCreateValue()
        {
            TValue result;
            while (!_cache.TryTake(out result) || !_checkValue(result))
            {
                _cache.Add(_factory());
            }

            return result;
        }
    }
}
