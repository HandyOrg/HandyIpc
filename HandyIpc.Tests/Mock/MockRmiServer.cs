using System;
using System.Threading;
using System.Threading.Tasks;
using HandyIpc.Core;

namespace HandyIpcTests.Mock
{
    public class MockRmiServer : IRmiServer
    {
        public async Task RunAsync(string identifier, RequestHandler handler, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
