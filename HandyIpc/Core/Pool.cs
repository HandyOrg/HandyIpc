using System;

namespace HandyIpc.Core
{
    internal sealed class Pool<T> : PoolBase<T> where T : IDisposable
    {
        private readonly Func<T> _factory;
        private readonly Func<T, bool> _checkValue;

        public Pool(Func<T> factory, Func<T, bool>? checkValue = null)
        {
            _factory = factory;
            _checkValue = checkValue ?? (_ => true);
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
