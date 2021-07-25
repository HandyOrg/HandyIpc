using System;
using System.Threading;
using System.Threading.Tasks;
using HandyIpc;
using HandyIpc.Core;

namespace HandyIpcTests.Mock
{
    public class MockRmiServer : IRmiServer
    {
        public async Task RunAsync(string identifier, MiddlewareHandler middleware, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
