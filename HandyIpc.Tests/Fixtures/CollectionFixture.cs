using Xunit;

namespace HandyIpcTests.Fixtures
{
    [CollectionDefinition(nameof(CollectionFixture))]
    public class CollectionFixture : ICollectionFixture<NamedPipeFixture>, ICollectionFixture<SocketFixture>
    {
    }
}
