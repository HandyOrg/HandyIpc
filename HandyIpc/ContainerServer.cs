using System;
using System.Threading;
using System.Threading.Tasks;
using HandyIpc.Core;
using HandyIpc.Logger;

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
            _logger.Info("IPC service has been started...");
        }

        public void Stop()
        {
            _cancellationTokenSource?.Cancel();
            IsRunning = false;
            _logger.Info("IPC service has been stopped.");
        }

        public void Dispose()
        {
            Stop();
            _server.Dispose();
            _logger.Info("IPC service has been disposed.");
        }

        private async Task StartAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                IConnection connection = await _server.WaitForConnectionAsync().ConfigureAwait(false);
                if (token.IsCancellationRequested)
                {
                    break;
                }

                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.Debug($"A new connection is established. (hashCode: {connection.GetHashCode()})");
                }

                // Do not await the request handler, and go to await next stream connection directly.
#pragma warning disable 4014
                HandleRequestAsync(connection, token);
#pragma warning restore 4014
            }
        }

        private async Task HandleRequestAsync(IConnection connection, CancellationToken token)
        {
            Task Handler(Context context) => _middleware(context, () => Task.CompletedTask);

            bool disposeConnection = true;
            try
            {
                while (!token.IsCancellationRequested)
                {
                    byte[] input = await connection.ReadAsync(token).ConfigureAwait(false);
                    if (input.Length == 0)
                    {
                        // When the remote side closes the link, the read does NOT throw any exception,
                        // only returns 0 bytes data, and does NOT BLOCK, causing a high frequency dead loop.
                        // Differences in behavior:
                        // - NamedPipe: After multiple non-blocking loops, an ArgumentException is thrown and the loop is terminated,
                        // so for the most part it behaves normally.
                        // - Socket(Tcp): Always in the unblock loop.
                        break;
                    }

                    Context ctx = new()
                    {
                        Input = input,
                        Connection = connection,
                        Logger = _logger,
                        Serializer = _serializer,
                    };
                    await Handler(ctx).ConfigureAwait(false);
                    await connection.WriteAsync(ctx.Output, token).ConfigureAwait(false);

                    if (!ctx.KeepAlive)
                    {
                        disposeConnection = false;
                        break;
                    }
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
                if (disposeConnection)
                {
                    connection.Dispose();
                }

                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.Debug($"A connection is released. (hashCode: {connection.GetHashCode()}, disposed: {disposeConnection})");
                }
            }
        }
    }
}
