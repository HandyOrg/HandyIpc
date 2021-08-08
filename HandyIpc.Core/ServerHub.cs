using System;
using System.Collections.Generic;
using System.Threading;
using HandyIpc.Core;

namespace HandyIpc
{
    internal class ServerHub : IServerHub
    {
        private sealed class Disposable : IDisposable
        {
            private readonly Action _dispose;

            public Disposable(Action dispose) => _dispose = dispose;

            public void Dispose() => _dispose();
        }

        private readonly RmiServerBase _rmiServer;
        private readonly ISerializer _serializer;
        private readonly ILogger _logger;
        private readonly object _locker = new();
        private readonly Dictionary<Type, CancellationTokenSource> _runningInterfaces = new();
        private readonly MiddlewareCache _middlewareCache = new();

        public ServerHub(RmiServerBase rmiServer, ISerializer serializer, ILogger logger)
        {
            _rmiServer = rmiServer;
            _serializer = serializer;
            _logger = logger;
        }

        public IDisposable Start(Type interfaceType, Func<object> factory, string? accessToken = null)
        {
            lock (_locker)
            {
                Middleware middleware = _rmiServer.BuildMiddleware(interfaceType, factory, accessToken, _middlewareCache);
                StartInterface(interfaceType, middleware);

                return new Disposable(() => StopAndRemoveInterface(interfaceType));
            }
        }

        public IDisposable Start(Type interfaceType, Func<Type[], object> factory, string? accessToken = null)
        {
            lock (_locker)
            {
                Middleware middleware = _rmiServer.BuildMiddleware(interfaceType, factory, accessToken, _middlewareCache);
                StartInterface(interfaceType, middleware);

                return new Disposable(() => StopAndRemoveInterface(interfaceType));
            }
        }

        private void StopAndRemoveInterface(Type interfaceType)
        {
            lock (_locker)
            {
                if (_runningInterfaces.TryGetValue(interfaceType, out CancellationTokenSource source))
                {
                    _runningInterfaces.Remove(interfaceType);
                    source.Cancel();

                    _middlewareCache.Remove(interfaceType);
                }
            }
        }

        private void StartInterface(Type interfaceType, Middleware middleware)
        {
            string identifier = interfaceType.ResolveIdentifier();
            var source = new CancellationTokenSource();

            // Async run the server without waiting.
            _rmiServer.RunAsync(identifier, middleware.ToHandler(_serializer, _logger), source.Token);

            _runningInterfaces.Add(interfaceType, source);
        }
    }
}
