using System.Net;
using HandyIpc;
using HandyIpc.Serializer.Json;
using HandyIpc.Socket;

namespace HandyIpcTests.Fixtures
{
    public class SocketFixture : ProtocolFixtureBase
    {
        public SocketFixture() : base(GetClientBuilder(), GetServerBuilder())
        {
        }

        private static ContainerClientBuilder GetClientBuilder()
        {
            ContainerClientBuilder clientBuilder = new();
            clientBuilder
                .UseTcp(IPAddress.Loopback, 10086)
                .UseJsonSerializer();

            return clientBuilder;
        }

        private static ContainerServerBuilder GetServerBuilder()
        {
            ContainerServerBuilder serverBuilder = new();
            serverBuilder
                .UseTcp(IPAddress.Loopback, 10086)
                .UseJsonSerializer();

            return serverBuilder;
        }
    }
}
