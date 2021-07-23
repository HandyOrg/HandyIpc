using System;
using HandyIpc;
using HandyIpc.Client;
using HandyIpc.NamedPipe;
using HandyIpc.Serializer.Json;
using HandyIpc.Server;
using HandyIpcTests.Implementations;
using HandyIpcTests.Interfaces;

namespace HandyIpcTests
{
    public sealed class EndToEndTestFixture : IDisposable
    {
        private readonly IDisposable _buildInTypeTestServerToken;
        private readonly IDisposable _genericTestServerToken;

        public IIpcClientHub ClientHub { get; }

        public EndToEndTestFixture()
        {
            var server = HandyIpcHub
                .CreateServerFactory()
                .UseJsonSerializer()
                .UseNamedPipe()
                .Build();
            _buildInTypeTestServerToken = server.Start<IBuildInTypeTest, BuildInTypeTest>();
            _genericTestServerToken = server.Start(typeof(IGenericTest<,>), typeof(GenericTest<,>));

            ClientHub = HandyIpcHub
                .CreateClientFactory()
                .UseJsonSerializer()
                .UseNamedPipe()
                .Build();
        }

        public void Dispose()
        {
            _buildInTypeTestServerToken.Dispose();
            _genericTestServerToken.Dispose();
        }
    }
}
