using System;
using System.Threading;
using System.Threading.Tasks;
using HandyIpc.Server;
using static HandyIpc.NamedPipe.PrimitiveMethods;

namespace HandyIpc.NamedPipe
{
    internal class RmiServer : IRmiServer
    {
        private readonly ISerializer _serializer;
        private readonly ILogger _logger;

        public RmiServer(ISerializer serializer, ILogger logger)
        {
            _serializer = serializer;
            _logger = logger;
        }

        public async Task RunAsync(string identifier, MiddlewareHandler middleware, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var stream = await CreateServerStreamAsync(identifier, token);

                    if (token.IsCancellationRequested) break;

#pragma warning disable 4014
                    HandleRequestAsync(stream, middleware.ToHandler(_serializer, _logger), token);
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
