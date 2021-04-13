using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HandyIpc.Server
{
    internal class HandyIpcServerHub : IHandyIpcServerHub
    {
        private readonly object _locker = new();
        private readonly Dictionary<Type, CancellationTokenSource> _runningInterfaces = new();
        private readonly ConcurrentDictionary<Type, IIpcDispatcher> _ipcDispatchers = new();

        public IDisposable Start(Type interfaceType, Func<object> factory)
        {
            StartInterface(interfaceType, defaultMiddleware =>
            {
                IIpcDispatcher dispatcher = GetOrAddIpcDispatcher(interfaceType, factory);
                return defaultMiddleware.Then(dispatcher.Dispatch);
            });
            return new Disposable(() => StopAndRemoveInterface(interfaceType));
        }

        public IDisposable Start(Type interfaceType, Func<Type[], object> factory)
        {
            StartInterface(interfaceType, defaultMiddleware =>
            {
                var genericDispatcher = Middlewares.GetGenericDispatcher(genericTypes =>
                {
                    var constructedInterfaceType = interfaceType.MakeGenericType(genericTypes);
                    return GetOrAddIpcDispatcher(constructedInterfaceType, () => factory(genericTypes));
                });
                return defaultMiddleware.Then(genericDispatcher);
            });

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

        private void StartInterface(
            Type interfaceType,
            Func<MiddlewareHandler<Context>, MiddlewareHandler<Context>> append)
        {
            lock (_locker)
            {
                var middleware = Middleware.Compose<Context>(
                    Middlewares.Heartbeat,
                    Middlewares.ExceptionHandler,
                    Middlewares.RequestParser);

                interfaceType.ResolveContractInfo(out var identifier, out var accessToken);
                if (!string.IsNullOrEmpty(accessToken))
                {
                    middleware = middleware.Then(Middlewares.GetAuthenticator(accessToken));
                }

                middleware = append(middleware);
                var source = new CancellationTokenSource();

#pragma warning disable 4014
                RunServerAsync(identifier, middleware, source.Token);
#pragma warning restore 4014

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

        private static async Task RunServerAsync(string identifier, MiddlewareHandler<Context> middleware, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var stream = await PrimitiveMethods.CreateServerStreamAsync(identifier, token);

                    if (token.IsCancellationRequested) break;

#pragma warning disable 4014
                    PrimitiveMethods.HandleRequestAsync(stream, middleware.ToHandler(), HandyIpcHub.Preferences.BufferSize, token);
#pragma warning restore 4014
                }
                catch (OperationCanceledException)
                {
                    // Ignore
                }
                catch (Exception e)
                {
                    HandyIpcHub.Logger.Error($"An unexpected exception occurred in the server (Id: {identifier}).", e);
                }
            }
        }
    }
}
