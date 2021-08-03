using System;
using System.Threading;
using System.Threading.Tasks;
using HandyIpc.Core;

namespace HandyIpc.Socket
{
    internal class UdpRmiServer : IRmiServer
    {
        public async Task RunAsync(string identifier, RequestHandler handler, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
