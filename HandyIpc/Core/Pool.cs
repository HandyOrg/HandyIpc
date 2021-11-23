using System;

namespace HandyIpc.Core
{
    public sealed class Pool<TValue> : PoolBase<TValue> where TValue : IDisposable
    {
        private readonly Func<T> _factory;
        private readonly Func<T, bool> _checkValue;

        public Pool(Func<TValue> factory, Func<TValue, bool> checkValue)
        {
            _factory = factory;
            _checkValue = checkValue;
        }

        public RentedValue<T> Rent()
        {
            CheckDisposed(nameof(Pool<T>));

            T value = TakeOrCreateValue();
            return new RentedValue<T>(value, ReturnValue);

            // Local method
            void ReturnValue(T rentedValue) => Cache.Add(rentedValue);
        }

        private T TakeOrCreateValue()
        {
            T result;
            while (!Cache.TryTake(out result) || !_checkValue(result))
            {
                Cache.Add(_factory());
            }

            return result;
        }
    }
}
