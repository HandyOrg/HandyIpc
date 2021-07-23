namespace HandyIpcTests.Mock
{
    public class GenericType<TKey, TValue>
    {
        public TKey Key { get; init; }

        public TValue Value { get; init; }
    }

    public class GenericType<T>
    {
        public T Value { get; init; }

        public string Name { get; init; }
    }
}
