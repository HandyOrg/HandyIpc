using System;

namespace HandyIpc.Core
{
    public sealed class Pool<TValue> : PoolBase<TValue> where TValue : IDisposable
    {
        private readonly Func<TValue> _factory;
        private readonly Func<TValue, bool> _checkValue;

        public Pool(Func<TValue> factory, Func<TValue, bool> checkValue)
        {
            _factory = factory;
            _checkValue = checkValue;
        }

        public RentedValue<TValue> Rent()
        {
            CheckDisposed("Pool");

            TValue value = TakeOrCreateValue();
            return new RentedValue<TValue>(value, ReturnValue);

            // Local method
            void ReturnValue(TValue rentedValue) => Cache.Add(rentedValue);
        }

        private TValue TakeOrCreateValue()
        {
            TValue result;
            while (!Cache.TryTake(out result) || !_checkValue(result))
            {
                Cache.Add(_factory());
            }

            return result;
        }
    }
}
