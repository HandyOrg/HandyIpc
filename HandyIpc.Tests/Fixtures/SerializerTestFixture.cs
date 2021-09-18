using HandyIpc.Core;
using HandyIpc.Serializer.Json;

namespace HandyIpcTests.Fixtures
{
    public class SerializerTestFixture
    {
        public ISerializer Serializer { get; }

        public SerializerTestFixture()
        {
            Serializer = new JsonSerializer();
        }
    }
}
