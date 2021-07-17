using HandyIpc;
using HandyIpc.Serializer.Json;

namespace HandyIpcTests
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
