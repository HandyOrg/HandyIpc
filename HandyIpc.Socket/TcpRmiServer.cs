using System;
using System.Threading;
using System.Threading.Tasks;
using HandyIpc.Core;

namespace HandyIpc.Socket
{
    internal class TcpRmiServer : IRmiServer
    {
        public async Task RunAsync(string identifier, MiddlewareHandler middleware, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
