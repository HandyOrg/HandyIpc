using System;
using System.Threading;
using System.Threading.Tasks;
using HandyIpc.Core;
using static HandyIpc.NamedPipe.PrimitiveMethods;

namespace HandyIpc.NamedPipe
{
    internal class RmiServer : IRmiServer
    {
        private readonly ILogger _logger;

        public RmiServer(ILogger logger)
        {
            _logger = logger;
        }

        public async Task RunAsync(string identifier, RequestHandler handler, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var stream = await CreateServerStreamAsync(identifier, token);

                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    // Do not await the request handler, and go to await next stream connection directly.
#pragma warning disable 4014
                    HandleRequestAsync(stream, handler, token);
#pragma warning restore 4014
                }
                catch (OperationCanceledException)
                {
                    // Ignore
                }
                catch (Exception e)
                {
                    _logger.Error($"An unexpected exception occurred in the server (Id: {identifier}).", e);
                }
            }
        }
    }
}
