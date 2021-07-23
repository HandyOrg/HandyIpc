namespace HandyIpcTests.Mock
{
    public class ClassWithNewCtor
    {
        public static string InitialName { get; set; } = string.Empty;

        public string Name { get; init; } = InitialName;

        public string Id { get; set; } = string.Empty;
    }
}
