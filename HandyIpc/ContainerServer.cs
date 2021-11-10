using System;
using System.Threading;
using System.Threading.Tasks;
using HandyIpc.Core;

namespace HandyIpc
{
    internal sealed class ContainerServer : IContainerServer
    {
        private readonly IServer _server;
        private readonly Middleware _middleware;
        private readonly ISerializer _serializer;
        private readonly ILogger _logger;

        private CancellationTokenSource? _cancellationTokenSource;

        public bool IsRunning { get; private set; }

        public ContainerServer(IServer server, Middleware middleware, ISerializer serializer, ILogger logger)
        {
            _server = server;
            _middleware = middleware;
            _serializer = serializer;
            _logger = logger;
        }

        public void Start()
        {
            if (_cancellationTokenSource is null or { IsCancellationRequested: true })
            {
                _cancellationTokenSource = new CancellationTokenSource();
            }

#pragma warning disable 4014
            // Async run the server without waiting.
            StartAsync(_cancellationTokenSource.Token);
#pragma warning restore 4014

            IsRunning = true;
        }

        public void Stop()
        {
            _cancellationTokenSource?.Cancel();
            IsRunning = false;
        }

        public void Dispose()
        {
            Stop();
            _server.Dispose();
        }

        private async Task StartAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                IConnection connection = await _server.WaitForConnectionAsync();
                RequestHandler handler = _middleware.ToHandler(
                    ctx =>
                    {
                        ctx.Logger = _logger;
                        ctx.Serializer = _serializer;
                        ctx.Connection = connection;
                    });

                if (token.IsCancellationRequested)
                {
                    break;
                }

                // Do not await the request handler, and go to await next stream connection directly.
#pragma warning disable 4014
                HandleRequestAsync(connection, handler, token);
#pragma warning restore 4014
            }
        }

        private async Task HandleRequestAsync(IConnection connection, RequestHandler handler, CancellationToken token)
        {
            try
            {
                while (true)
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    byte[] buffer = await connection.ReadAsync(token);
                    if (buffer.Length == 0)
                    {
                        continue;
                    }

                    byte[] output = await handler(buffer);
                    await connection.WriteAsync(output, token);
                }
            }
            catch (OperationCanceledException)
            {
                // Ignore
            }
            catch (Exception e)
            {
                _logger.Error("Unexpected exception occurred when starting the server instance.", e);
            }
            finally
            {
                connection.Dispose();
            }
        }
    }
}
