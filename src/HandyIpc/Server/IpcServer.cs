using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HandyIpc.Server
{
    public class IpcServer
    {
        private static readonly object Locker = new object();
        private static readonly IpcServer Instance = new IpcServer();

        public static IpcPreferences Preferences { get; } = new IpcPreferences();

        public static void Update(Action<IpcServerCollection> settings)
        {
            lock (Locker)
            {
                UpdateInternal(settings);
            }
        }

        private static void UpdateInternal(Action<IpcServerCollection> settings)
        {
            var container = Instance.GetIpcServerCollection();
            settings?.Invoke(container);

            Instance.CleanupInterfaces(container.ToBeRemovedInterfaces);

            var defaultMiddleware = Middleware.Compose(
                Middleware.Heartbeat,
                Middleware.ExceptionHandler,
                Middleware.RequestParser);

            Instance.RunGenericInterfaces(container.GenericInterfaces
                .Select(item => (item.Key, item.Value)),
                defaultMiddleware);

            Instance.RunNonGenericInterfaces(container.NonGenericInterfaces
                .Select(item => (item.Key, item.Value)),
                defaultMiddleware);
        }

        private readonly Dictionary<Type, CancellationTokenSource> _runningInterfaces =
            new Dictionary<Type, CancellationTokenSource>();
        private readonly ConcurrentDictionary<Type, IIpcDispatcher> _ipcDispatchers =
            new ConcurrentDictionary<Type, IIpcDispatcher>();

        private IpcServer() { }

        private IpcServerCollection GetIpcServerCollection()
        {
            return new IpcServerCollection(_runningInterfaces.Keys);
        }

        private void CleanupInterfaces(IEnumerable<Type> interfaces)
        {
            foreach (var toBeRemovedInterface in interfaces)
            {
                if (_runningInterfaces.TryGetValue(toBeRemovedInterface, out var source))
                {
                    _runningInterfaces.Remove(toBeRemovedInterface);
                    source.Cancel();

                    if (toBeRemovedInterface.IsGenericType)
                    {
                        _ipcDispatchers
                            .Where(item => EqualityComparer<Type>.Default.Equals(
                                item.Key.GetGenericTypeDefinition(),
                                toBeRemovedInterface))
                            .Select(item => item.Key)
                            .ToList()
                            .ForEach(item => _ipcDispatchers.TryRemove(item, out _));
                    }
                    else
                    {
                        _ipcDispatchers.TryRemove(toBeRemovedInterface, out _);
                    }
                }
            }
        }

        private void RunNonGenericInterfaces(
            IEnumerable<(Type interfaceType, Func<object> factory)> items,
            MiddlewareHandler middleware)
        {
            foreach (var (interfaceType, factory) in items)
            {
                interfaceType.GetContractInfo(out var identifier, out var accessToken);
                if (!string.IsNullOrEmpty(accessToken))
                {
                    middleware = middleware.Then(Middleware.GetAuthenticator(accessToken));
                }

                var dispatcher = GetOrAddIpcDispatcher(interfaceType, factory);
                middleware = middleware.Then(dispatcher.Dispatch);
                var source = new CancellationTokenSource();

#pragma warning disable 4014
                RunServerAsync(identifier, middleware, source.Token);
#pragma warning restore 4014

                _runningInterfaces.Add(interfaceType, source);
            }
        }

        private void RunGenericInterfaces(
            IEnumerable<(Type interfaceType, Func<Type[], object> factory)> items,
            MiddlewareHandler middleware)
        {
            foreach (var (interfaceType, factory) in items)
            {
                interfaceType.GetContractInfo(out var identifier, out var accessToken);
                if (!string.IsNullOrEmpty(accessToken))
                {
                    middleware = middleware.Then(Middleware.GetAuthenticator(accessToken));
                }

                var genericDispatcher = Middleware.GetGenericDispatcher(genericTypes =>
                {
                    var constructedInterfaceType = interfaceType.MakeGenericType(genericTypes);
                    return GetOrAddIpcDispatcher(constructedInterfaceType, () => factory(genericTypes));
                });

                middleware = middleware.Then(genericDispatcher);
                var source = new CancellationTokenSource();

#pragma warning disable 4014
                RunServerAsync(identifier, middleware, source.Token);
#pragma warning restore 4014

                _runningInterfaces.Add(interfaceType, source);
            }
        }

        private IIpcDispatcher GetOrAddIpcDispatcher(Type interfaceType, Func<object> factory)
        {
            return _ipcDispatchers.GetOrAdd(interfaceType, key =>
            {
                var instance = factory();

                Guards.ThrowIfNot(interfaceType.IsInstanceOfType(instance),
                    $"The instance created by the factory corresponding to the {interfaceType} interface " +
                    $"does not implement the {interfaceType} interface.", nameof(factory));

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

                    if (token.IsCancellationRequested) break;

#pragma warning disable 4014
                    PrimitiveMethods.HandleRequestAsync(stream, middleware.ToHandler(), Preferences.BufferSize, token);
#pragma warning restore 4014
                }
                catch (OperationCanceledException)
                {
                    // Ignore
                }
                catch (Exception e)
                {
                    Preferences.Logger.Error($"An unexpected exception occurred in the server (Id: {identifier}).", e);
                }
            }
        }
    }
}
