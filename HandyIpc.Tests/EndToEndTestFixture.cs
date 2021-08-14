using System;
using System.Net;
using HandyIpc;
using HandyIpc.Serializer.Json;
using HandyIpc.Socket;
using HandyIpcTests.Implementations;
using HandyIpcTests.Interfaces;

namespace HandyIpcTests
{
    public sealed class EndToEndTestFixture : IDisposable
    {
        private readonly IServer _server;

        public IClient Client { get; }

        public EndToEndTestFixture()
        {
            IClientBuilder clientBuilder = HandyIpcBuilder.CreateClientBuilder();
            clientBuilder
                .UseJsonSerializer()
                .UseTcp(IPAddress.Loopback, 10086);
            Client = clientBuilder.Build();

            IServerBuilder serverBuilder = HandyIpcBuilder.CreateServerBuilder();
            serverBuilder
                .UseJsonSerializer()
                .UseTcp(IPAddress.Loopback, 10086);
            serverBuilder
                .Register<IBuildInTypeTest, BuildInTypeTest>()
                .Register(typeof(IGenericTest<,>), typeof(GenericTest<,>));
            _server = serverBuilder.Build();
            _server.Start();
        }

        public void Dispose()
        {
            Client.Dispose();
            _server.Dispose();
        }
    }
}
