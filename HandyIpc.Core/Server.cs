using System;
using System.Threading;
using System.Threading.Tasks;
using HandyIpc.Core;

namespace HandyIpc
{
    internal sealed class Server : IServer
    {
        private readonly ReceiverBase _receiver;
        private readonly Middleware _middleware;
        private readonly ISerializer _serializer;
        private readonly ILogger _logger;

        private CancellationTokenSource? _cancellationTokenSource;

        public bool IsRunning { get; private set; }

        public Server(ReceiverBase receiver, Middleware middleware, ISerializer serializer, ILogger logger)
        {
            _receiver = receiver;
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
            CatchException(_receiver.StartAsync(_middleware.ToHandler(_serializer, _logger), _cancellationTokenSource.Token));
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
        }

        private async Task CatchException(Task task)
        {
            try
            {
                await task;
            }
            catch (Exception e)
            {
                _logger.Error("Unexpected exception occurred when starting the server instance.", e);
            }
        }
    }
}
