namespace HandyIpcTests.Mock
{
    public enum Gender
    {
        Unknown,
        Female,
        Male,
    }

    public record ComplexType
    {
        public string Name => nameof(Name);

        public string Id => nameof(Id);

        public int Age => 10000;

        public Gender Gender = Gender.Unknown;
    }
}
