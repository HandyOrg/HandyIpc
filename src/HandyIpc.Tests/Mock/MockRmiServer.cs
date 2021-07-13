using System;
using System.Threading;
using System.Threading.Tasks;
using HandyIpc.Server;

namespace HandyIpc.Tests.Mock
{
    public class MockRmiServer : IRmiServer
    {
        private readonly ISerializer _serializer;

        public MockRmiServer(ISerializer serializer)
        {
            _serializer = serializer;
        }

        public async Task RunAsync(string identifier, MiddlewareHandler middleware, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
