using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace HandyIpc.Server
{
    public class IpcServer
    {
        private readonly IpcServerBuilder _builder;
        private readonly ConcurrentDictionary<Type, IIpcDispatcher> _ipcDispatchers = new ConcurrentDictionary<Type, IIpcDispatcher>();

        private CancellationTokenSource _cancellationTokenSource;

        internal IpcServer(IpcServerBuilder builder) => _builder = builder;

        public void Start()
        {
            // Fix configure
            _builder.IpcConfigure?.Invoke(IpcSettings.Instance);

            // Fix middleware
            var defaultMiddleware = Middleware.Compose(
                Middleware.Heartbeat,
                Middleware.ExceptionHandler,
                Middleware.RequestParser);

            if (_cancellationTokenSource == null || _cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource = new CancellationTokenSource();
            }

            RunNonGenericInterfaces(defaultMiddleware, _cancellationTokenSource.Token);

            RunGenericInterfaces(defaultMiddleware, _cancellationTokenSource.Token);
        }

        public void Stop() => _cancellationTokenSource.Cancel();

        private void RunNonGenericInterfaces(MiddlewareHandler middleware, CancellationToken token)
        {
            foreach (var item in _builder.ServerFactories)
            {
                var interfaceType = item.Key;
                var factory = item.Value;

                interfaceType.GetContractInfo(out var identifier, out var accessToken);
                if (!string.IsNullOrEmpty(accessToken))
                {
                    middleware = middleware.Compose(Middleware.GetAuthenticator(accessToken));
                }

                var dispatcher = GetOrAddIpcDispatcher(interfaceType, factory);
                middleware = middleware.Compose(dispatcher.Dispatch);

#pragma warning disable 4014
                RunServerAsync(identifier, middleware, token);
#pragma warning restore 4014
            }
        }

        private void RunGenericInterfaces(MiddlewareHandler middleware, CancellationToken token)
        {
            foreach (var item in _builder.GenericServerFactories)
            {
                var interfaceType = item.Key;
                var factory = item.Value;

                interfaceType.GetContractInfo(out var identifier, out var accessToken);
                if (!string.IsNullOrEmpty(accessToken))
                {
                    middleware = middleware.Compose(Middleware.GetAuthenticator(accessToken));
                }

                var genericDispatcher = Middleware.GetGenericDispatcher(genericTypes =>
                {
                    var constructedInterfaceType = interfaceType.MakeGenericType(genericTypes);
                    Guards.ThrowIfInvalid(constructedInterfaceType.IsConstructedGenericType, "");
                    return GetOrAddIpcDispatcher(constructedInterfaceType, () => factory(genericTypes));
                });

                middleware = middleware.Compose(genericDispatcher);

#pragma warning disable 4014
                RunServerAsync(identifier, middleware, token);
#pragma warning restore 4014
            }
        }

        private IIpcDispatcher GetOrAddIpcDispatcher(Type interfaceType, Func<object> factory)
        {
            return _ipcDispatchers.GetOrAdd(interfaceType, key =>
            {
                var instance = factory();

                Guards.ThrowIfInvalid(interfaceType.IsInstanceOfType(instance), "");

                var proxy = Activator.CreateInstance(interfaceType.GetServerProxyType(), instance);
                return (IIpcDispatcher)Activator.CreateInstance(interfaceType.GetDispatcherType(), proxy);
            });
        }

        private static async Task RunServerAsync(string identifier, MiddlewareHandler middleware, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var stream = await PrimitiveMethods.CreateServerStreamAsync(identifier, token);
#pragma warning disable 4014
                    PrimitiveMethods.HandleRequestAsync(stream, middleware.ToHandler(), token);
#pragma warning restore 4014
                }
                catch (OperationCanceledException)
                {
                    // Ignore
                }
                catch (Exception e)
                {
                    IpcSettings.Instance.Logger.Error($"An unexpected exception occurred in the server (Id: {identifier}).", e);
                }
            }
        }
    }
}
