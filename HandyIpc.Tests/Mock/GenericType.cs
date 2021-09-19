namespace HandyIpcTests.Mock
{
    public record GenericType<TKey, TValue>
    {
        public TKey Key { get; init; }

        public TValue Value { get; init; }
    }

    public record GenericType<T>
    {
        public T Value { get; init; }

        public string Name { get; init; }
    }
}
