using System;
using System.Net;
using HandyIpc;
using HandyIpc.NamedPipe;
using HandyIpc.Serializer.Json;
using HandyIpc.Socket;
using HandyIpcTests.Implementations;
using HandyIpcTests.Interfaces;

namespace HandyIpcTests
{
    public sealed class EndToEndTestFixture : IDisposable
    {
        private readonly IContainerServer _server;

        public IContainerClient Client { get; }

        public EndToEndTestFixture()
        {
            ContainerClientBuilder clientBuilder = new();
            clientBuilder
                //.UseTcp(IPAddress.Loopback, 10086)
                .UseNamedPipe("ec57043f-465c-4766-ae49-b9b1ee9ac571")
                .UseJsonSerializer();
            Client = clientBuilder.Build();

            ContainerServerBuilder serverBuilder = new();
            serverBuilder
                //.UseTcp(IPAddress.Loopback, 10086)
                .UseNamedPipe("ec57043f-465c-4766-ae49-b9b1ee9ac571")
                .UseJsonSerializer();

            serverBuilder
                .Register<IBuildInTypeTest, BuildInTypeTest>()
                .Register(typeof(IGenericTest<,>), typeof(GenericTest<,>));

            _server = serverBuilder.Build();
            _server.Start();
        }

        public void Dispose()
        {
            Client.Dispose();

            _server.Stop();
            _server.Dispose();
        }
    }
}
