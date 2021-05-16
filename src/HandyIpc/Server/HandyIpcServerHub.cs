using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace HandyIpc.Server
{
    internal class HandyIpcServerHub : IIpcServerHub
    {
        private sealed class Disposable : IDisposable
        {
            private readonly Action _dispose;

            public Disposable(Action dispose) => _dispose = dispose;

            public void Dispose() => _dispose();
        }

        private readonly IRmiServer _rmiServer;
        private readonly object _locker = new();
        private readonly Dictionary<Type, CancellationTokenSource> _runningInterfaces = new();
        private readonly ConcurrentDictionary<Type, IIpcDispatcher> _ipcDispatchers = new();

        public HandyIpcServerHub(IRmiServer rmiServer)
        {
            _rmiServer = rmiServer;
        }

        public IDisposable Start(Type interfaceType, Func<object> factory, string? accessToken = null)
        {
            StartInterface(interfaceType, defaultMiddleware =>
            {
                IIpcDispatcher dispatcher = GetOrAddIpcDispatcher(interfaceType, factory);
                return defaultMiddleware.Then(dispatcher.Dispatch);
            }, accessToken);

            return new Disposable(() => StopAndRemoveInterface(interfaceType));
        }

        public IDisposable Start(Type interfaceType, Func<Type[], object> factory, string? accessToken = null)
        {
            StartInterface(interfaceType, defaultMiddleware =>
            {
                var genericDispatcher = Middlewares.GetGenericDispatcher(genericTypes =>
                {
                    var constructedInterfaceType = interfaceType.MakeGenericType(genericTypes);
                    return GetOrAddIpcDispatcher(constructedInterfaceType, () => factory(genericTypes));
                });
                return defaultMiddleware.Then(genericDispatcher);
            }, accessToken);

            return new Disposable(() => StopAndRemoveInterface(interfaceType));
        }

        private void StopAndRemoveInterface(Type interfaceType)
        {
            lock (_locker)
            {
                if (_runningInterfaces.TryGetValue(interfaceType, out var source))
                {
                    _runningInterfaces.Remove(interfaceType);
                    source.Cancel();

                    if (interfaceType.IsGenericType)
                    {
                        _ipcDispatchers
                            .Where(item => EqualityComparer<Type>.Default.Equals(
                                item.Key.GetGenericTypeDefinition(),
                                interfaceType))
                            .Select(item => item.Key)
                            .ToList()
                            .ForEach(item => _ipcDispatchers.TryRemove(item, out _));
                    }
                    else
                    {
                        _ipcDispatchers.TryRemove(interfaceType, out _);
                    }
                }
            }
        }

        private void StartInterface(Type interfaceType, Func<MiddlewareHandler, MiddlewareHandler> append, string? accessToken)
        {
            lock (_locker)
            {
                var middleware = Middleware.Compose(
                    Middlewares.Heartbeat,
                    Middlewares.ExceptionHandler,
                    Middlewares.RequestParser);

                if (!string.IsNullOrEmpty(accessToken))
                {
                    middleware = middleware.Then(Middlewares.GetAuthenticator(accessToken!));
                }

                middleware = append(middleware);
                var source = new CancellationTokenSource();

                string identifier = interfaceType.ResolveIdentifier();
                // Async run the server without waiting.
                _rmiServer.RunAsync(identifier, middleware, source.Token);

                _runningInterfaces.Add(interfaceType, source);
            }
        }

        private IIpcDispatcher GetOrAddIpcDispatcher(Type interfaceType, Func<object> factory)
        {
            return _ipcDispatchers.GetOrAdd(interfaceType, _ =>
            {
                var instance = factory();

                Guards.ThrowIfNot(interfaceType.IsInstanceOfType(instance),
                    $"The instance created by the factory corresponding to the {interfaceType} interface " +
                    $"does not implement the {interfaceType} interface.", nameof(factory));

                var proxy = Activator.CreateInstance(interfaceType.GetServerProxyType(), instance);
                return (IIpcDispatcher)Activator.CreateInstance(interfaceType.GetDispatcherType(), proxy);
            });
        }
    }
}
