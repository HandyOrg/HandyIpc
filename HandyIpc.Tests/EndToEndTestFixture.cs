using System;
using HandyIpc;
using HandyIpc.Serializer.Json;
using HandyIpc.Socket;
using HandyIpcTests.Implementations;
using HandyIpcTests.Interfaces;

namespace HandyIpcTests
{
    public sealed class EndToEndTestFixture : IDisposable
    {
        private readonly IDisposable _buildInTypeTestServerToken;
        private readonly IDisposable _genericTestServerToken;

        public IClientHub ClientHub { get; }

        public EndToEndTestFixture()
        {
            var server = HandyIpcHub
                .CreateBuilder()
                .UseJsonSerializer()
                .UseTcp()
                .BuildServerHub();
            _buildInTypeTestServerToken = server.Register<IBuildInTypeTest, BuildInTypeTest>("{763EA8B3-79AB-413B-9B41-3290755EE7F0}");
            _genericTestServerToken = server.Register(typeof(IGenericTest<,>), typeof(GenericTest<,>));

            ClientHub = HandyIpcHub
                .CreateBuilder()
                .UseJsonSerializer()
                .UseTcp()
                .BuildClientHub();
        }

        public void Dispose()
        {
            _buildInTypeTestServerToken.Dispose();
            _genericTestServerToken.Dispose();
        }
    }
}
