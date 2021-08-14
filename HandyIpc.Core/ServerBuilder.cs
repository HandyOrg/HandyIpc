using System;
using System.Collections.Generic;
using HandyIpc.Core;

namespace HandyIpc
{
    internal class ServerBuilder : Configuration, IServerBuilder
    {
        private readonly List<(string key, Type type, Func<object> factory)> _interfaceMap = new();
        private readonly List<(string key, Type type, Func<Type[], object> factory)> _genericInterfaceMap = new();

        public IServerRegistry Register(Type interfaceType, Func<object> factory, string key)
        {
            _interfaceMap.Add((key, interfaceType, factory));
            return this;
        }

        public IServerRegistry Register(Type interfaceType, Func<Type[], object> factory, string key)
        {
            _genericInterfaceMap.Add((key, interfaceType, factory));
            return this;
        }

        public IServer Build()
        {
            Dictionary<string, Middleware> map = new();
            foreach (var (key, type, factory) in _interfaceMap)
            {
                Middleware methodDispatcher = CreateDispatcher(type, factory).Dispatch;
                map.Add(key, methodDispatcher);
            }

            foreach (var (key, type, factory) in _genericInterfaceMap)
            {
                Middleware methodDispatcher = Middlewares.GetMethodDispatcher(
                    genericTypes => CreateDispatcher(
                        type.MakeGenericType(genericTypes),
                        () => factory(genericTypes)));
                map.Add(key, methodDispatcher);
            }

            Middleware middleware = BuildBasicMiddleware().Then(Middlewares.GetInterfaceMiddleware(map));

            ReceiverBase receiver = ReceiverFactory();
            ILogger logger = LoggerFactory();
            receiver.SetLogger(logger);
            return new Server(receiver, middleware, SerializerFactory(), logger);
        }

        private static Middleware BuildBasicMiddleware()
        {
            return Middlewares.Compose(
                Middlewares.Heartbeat,
                Middlewares.ExceptionHandler,
                Middlewares.RequestParser);
        }

        private static IMethodDispatcher CreateDispatcher(Type interfaceType, Func<object> factory)
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
            var dispatcher = (IMethodDispatcher)Activator.CreateInstance(interfaceType.GetDispatcherType(), proxy);

            return dispatcher;
        }
    }
}
