using System;
using HandyIpc;
using HandyIpc.NamedPipe;
using HandyIpc.Serializer.Json;
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
            _buildInTypeTestServerToken = server.Start<IBuildInTypeTest, BuildInTypeTest>("{763EA8B3-79AB-413B-9B41-3290755EE7F0}");
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
