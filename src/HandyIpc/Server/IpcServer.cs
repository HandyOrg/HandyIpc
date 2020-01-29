using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HandyIpc.Server
{
    public class IpcServer
    {
        public static IpcServer Center { get; } = new IpcServer();

        private IpcServer() { }

        private readonly Dictionary<Type, Func<object>> _serverFactories = new Dictionary<Type, Func<object>>();
        private readonly Dictionary<Type, Func<Type[], object>> _genericServerFactories = new Dictionary<Type, Func<Type[], object>>();
        private readonly ConcurrentDictionary<Type, IIpcDispatcher> _ipcDispatchers = new ConcurrentDictionary<Type, IIpcDispatcher>();

        private Action<IpcSettings> _configure;

        public IpcServer Configure(Action<IpcSettings> settings)
        {
            Guards.ThrowIfNull(settings, nameof(settings));

            var previous = _configure;
            _configure = s =>
            {
                previous?.Invoke(s);
                settings(s);
            };

            return this;
        }

        public IpcServer Register(Type interfaceType, Func<object> factory)
        {
            Guards.ThrowIfNot(interfaceType.IsInterface, "", nameof(interfaceType));
            Guards.ThrowIfNot(!_serverFactories.ContainsKey(interfaceType), "", nameof(factory));

            _serverFactories[interfaceType] = factory;

            return this;
        }

        public IpcServer Register(Type interfaceType, Func<Type[], object> factory)
        {
            Guards.ThrowIfNot(interfaceType.IsInterface, "", nameof(interfaceType));
            Guards.ThrowIfNot(interfaceType.ContainsGenericParameters, "", nameof(interfaceType));
            Guards.ThrowIfNot(!_serverFactories.ContainsKey(interfaceType), "", nameof(factory));

            _genericServerFactories[interfaceType] = factory;

            return this;
        }

        public void Start()
        {
            // Fix configure
            _configure?.Invoke(IpcSettings.Instance);

            // Fix middleware
            var defaultMiddleware = Middleware.Compose(
                Middleware.Heartbeat,
                Middleware.ExceptionHandler,
                Middleware.RequestParser);

            RunNonGenericInterfaces(defaultMiddleware);

            RunGenericInterfaces(defaultMiddleware);
        }

        private void RunNonGenericInterfaces(MiddlewareHandler middleware)
        {
            foreach (var item in _serverFactories)
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
                RunServerAsync(identifier, middleware);
#pragma warning restore 4014
            }
        }

        private void RunGenericInterfaces(MiddlewareHandler middleware)
        {
            foreach (var item in _genericServerFactories)
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
                RunServerAsync(identifier, middleware);
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

        private static async Task RunServerAsync(string identifier, MiddlewareHandler middleware, CancellationToken token = default)
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
                catch
                {
                    IpcSettings.Instance.Logger.Error("");
                }
            }
        }
    }
}
