using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using HandyIpc.Core;

namespace HandyIpc
{
    public class ContainerServerBuilder : IServerConfiguration, IContainerRegistry
    {
        private readonly List<(string key, Type type, Func<object> factory)> _interfaceMap = new();
        private readonly List<(string key, Type type, Func<Type[], object> factory)> _genericInterfaceMap = new();

        private Func<IServer> _serverFactory = () => throw new InvalidOperationException(
           $"Must invoke the {nameof(IServerConfiguration)}.Use(Func<{nameof(IServer)}> factory) method " +
           "to register a factory before invoking the Build method.");
        private Func<ISerializer> _serializerFactory = () => throw new InvalidOperationException(
           $"Must invoke the {nameof(IServerConfiguration)}.Use(Func<{nameof(ISerializer)}> factory) method " +
           "to register a factory before invoking the Build method.");
        private Func<ILogger> _loggerFactory = () => new DebugLogger();

        public IServerConfiguration Use(Func<ISerializer> factory)
        {
            _serializerFactory = factory;
            return this;
        }

        public IServerConfiguration Use(Func<ILogger> factory)
        {
            _loggerFactory = factory;
            return this;
        }

        public IServerConfiguration Use(Func<IServer> factory)
        {
            _serverFactory = factory;
            return this;
        }

        public IContainerRegistry Register(Type interfaceType, Func<object> factory, string key)
        {
            _interfaceMap.Add((key, interfaceType, factory));
            return this;
        }

        public IContainerRegistry Register(Type interfaceType, Func<Type[], object> factory, string key)
        {
            _genericInterfaceMap.Add((key, interfaceType, factory));
            return this;
        }

        public IContainerServer Build()
        {
            Dictionary<string, Middleware> map = new();
            ConcurrentDictionary<string, NotifierManager> notifiers = new();
            foreach (var (key, type, factory) in _interfaceMap)
            {
                IMethodDispatcher dispatcher = CreateDispatcher(type, factory);
                dispatcher.NotifierManager = notifiers.GetOrAdd(key, _ => new NotifierManager(_serializerFactory()));
                Middleware methodDispatcher = dispatcher.Dispatch;
                map.Add(key, methodDispatcher);
            }

            foreach (var (key, type, factory) in _genericInterfaceMap)
            {
                Middleware methodDispatcher = Middlewares.GetMethodDispatcher(
                    genericTypes =>
                    {
                        IMethodDispatcher dispatcher = CreateDispatcher(
                            type.MakeGenericType(genericTypes),
                            () => factory(genericTypes));
                        dispatcher.NotifierManager = notifiers.GetOrAdd(key, _ => new NotifierManager(_serializerFactory()));
                        return dispatcher;
                    });
                map.Add(key, methodDispatcher);
            }

            Middleware middleware = Middlewares.Compose(
                Middlewares.Heartbeat,
                Middlewares.ExceptionHandler,
                Middlewares.GetHandleRequest(map),
                Middlewares.GetHandleEvent(notifiers),
                Middlewares.NotFound);

            return new ContainerServer(_serverFactory(), middleware, _serializerFactory(), _loggerFactory());
        }

        private static IMethodDispatcher CreateDispatcher(Type interfaceType, Func<object> factory)
        {
            object instance = factory();

            Guards.ThrowArgument(interfaceType.IsInstanceOfType(instance),
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
            var dispatcher = (IMethodDispatcher)Activator.CreateInstance(interfaceType.GetDispatcherType(), proxy);

            return dispatcher;
        }
    }
}
