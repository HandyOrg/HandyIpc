using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using HandyIpc.Core;

namespace HandyIpc
{
    internal class IpcServerHub : IIpcServerHub
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
        private readonly Dictionary<Type, IIpcDispatcher> _ipcDispatchers = new();

        public IpcServerHub(RmiServerBase rmiServer, ISerializer serializer, ILogger logger)
        {
            _rmiServer = rmiServer;
            _serializer = serializer;
            _logger = logger;
        }

        public IDisposable Start(Type interfaceType, Func<object> factory, string? accessToken = null)
        {
            lock (_locker)
            {
                IIpcDispatcher dispatcher = GetOrAddIpcDispatcher(interfaceType, factory);
                StartInterface(interfaceType, dispatcher.Dispatch, accessToken);

                return new Disposable(() => StopAndRemoveInterface(interfaceType));
            }
        }

        public IDisposable Start(Type interfaceType, Func<Type[], object> factory, string? accessToken = null)
        {
            lock (_locker)
            {
                Middleware genericDispatcher = Middlewares.GetGenericDispatcher(genericTypes =>
                {
                    Type constructedInterfaceType = interfaceType.MakeGenericType(genericTypes);
                    return GetOrAddIpcDispatcher(constructedInterfaceType, () => factory(genericTypes));
                });
                StartInterface(interfaceType, genericDispatcher, accessToken);

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

                    if (interfaceType.IsGenericType)
                    {
                        _ipcDispatchers
                            .Where(item => EqualityComparer<Type>.Default.Equals(
                                item.Key.GetGenericTypeDefinition(),
                                interfaceType))
                            .Select(item => item.Key)
                            .ToList()
                            .ForEach(item => _ipcDispatchers.Remove(item));
                    }
                    else
                    {
                        _ipcDispatchers.Remove(interfaceType);
                    }
                }
            }
        }

        private void StartInterface(Type interfaceType, Middleware dispatcher, string? accessToken)
        {
            Middleware middleware = _rmiServer.BuildMiddleware(dispatcher, accessToken);
            var source = new CancellationTokenSource();
            string identifier = interfaceType.ResolveIdentifier();

            // Async run the server without waiting.
            _rmiServer.RunAsync(identifier, middleware.ToHandler(_serializer, _logger), source.Token);

            _runningInterfaces.Add(interfaceType, source);
        }

        private IIpcDispatcher GetOrAddIpcDispatcher(Type interfaceType, Func<object> factory)
        {
            if (!_ipcDispatchers.TryGetValue(interfaceType, out IIpcDispatcher dispatcher))
            {
                object instance = factory();

                Guards.ThrowIfNot(interfaceType.IsInstanceOfType(instance),
                    $"The instance created by the factory corresponding to the {interfaceType} interface " +
                    $"does not implement the {interfaceType} interface.", nameof(factory));

                // NOTE:
                // 1. Why would we need a Proxy class?
                // To call generic methods remotely.
                // As we know, the server cannot know the possible generic parameters at compile time,
                // and a "MethodName to generic MethodInfo" mapping table must be maintained,
                // then determining the specific generic type at runtime by MethodInfo.MakeGenericMethod().
                //
                // 2. Why can't the XxxDispatcher and XxxProxy be combined into one class?
                // Because the Dispatcher class has some members declared by this framework,
                // such as Dispatch methods, and Proxy only implements the IContract interface declared by users,
                // this does not lead to naming conflicts even if the user also declares a Dispatch method
                // with the same signature in the IContract interface.
                object proxy = Activator.CreateInstance(interfaceType.GetServerProxyType(), instance);
                dispatcher = (IIpcDispatcher)Activator.CreateInstance(interfaceType.GetDispatcherType(), proxy);
                _ipcDispatchers.Add(interfaceType, dispatcher);
            }

            return dispatcher;
        }
    }
}
