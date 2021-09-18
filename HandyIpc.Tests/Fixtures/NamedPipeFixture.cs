using HandyIpc;
using HandyIpc.NamedPipe;
using HandyIpc.Serializer.Json;

namespace HandyIpcTests.Fixtures
{
    public class NamedPipeFixture : ProtocolFixtureBase
    {
        public NamedPipeFixture() : base(GetClientBuilder(), GetServerBuilder())
        {
        }

        private static ContainerClientBuilder GetClientBuilder()
        {
            ContainerClientBuilder clientBuilder = new();
            clientBuilder
                .UseNamedPipe("ec57043f-465c-4766-ae49-b9b1ee9ac571")
                .UseJsonSerializer();

            return clientBuilder;
        }

        private static ContainerServerBuilder GetServerBuilder()
        {
            ContainerServerBuilder serverBuilder = new();
            serverBuilder
                .UseNamedPipe("ec57043f-465c-4766-ae49-b9b1ee9ac571")
                .UseJsonSerializer();

            return serverBuilder;
        }
    }
}
