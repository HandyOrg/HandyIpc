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
        private readonly IDisposable _buildInTypeTestServerToken;
        private readonly IDisposable _genericTestServerToken;

        public IClientHub ClientHub { get; }

        public EndToEndTestFixture()
        {
            IHubBuilder builder = HandyIpcHub
                .CreateBuilder()
                .UseJsonSerializer()
                .UseTcp(IPAddress.Loopback, 10086);

            IServerHub serverHub = builder.BuildServerHub();
            _buildInTypeTestServerToken = serverHub.Register<IBuildInTypeTest, BuildInTypeTest>();
            _genericTestServerToken = serverHub.Register(typeof(IGenericTest<,>), typeof(GenericTest<,>));

            ClientHub = builder.BuildClientHub();
        }

        public void Dispose()
        {
            _buildInTypeTestServerToken.Dispose();
            _genericTestServerToken.Dispose();
        }
    }
}
