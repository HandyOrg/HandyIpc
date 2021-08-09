using System;
using System.Threading;
using System.Threading.Tasks;

namespace HandyIpc.Core
{
    public abstract class ReceiverBase
    {
        protected ILogger Logger { get; private set; } = null!;

        internal void SetLogger(ILogger logger) => Logger = logger;

        public virtual Middleware BuildMiddleware(Type interfaceType, Func<object> factory, string? accessToken, MiddlewareCache cache)
        {
            Middleware dispatcher = cache.GetOrAdd(
                interfaceType,
                nameof(IRequestDispatcher),
                (type, _) => CreateDispatcher(type, factory).Dispatch);

            return BuildBasicMiddleware(accessToken).Then(dispatcher);
        }

        public virtual Middleware BuildMiddleware(Type interfaceType, Func<Type[], object> factory, string? accessToken, MiddlewareCache cache)
        {
            Middleware genericDispatcher = cache.GetOrAdd(
                interfaceType,
                nameof(IRequestDispatcher),
                (type, _) => Middlewares.GetGenericDispatcher(
                    genericTypes => CreateDispatcher(
                        type.MakeGenericType(genericTypes),
                        () => factory(genericTypes))));

            return BuildBasicMiddleware(accessToken).Then(genericDispatcher);
        }

        public abstract Task StartAsync(string identifier, RequestHandler handler, CancellationToken token);

        private static Middleware BuildBasicMiddleware(string? accessToken)
        {
            Middleware middleware = Middlewares.Compose(
                Middlewares.Heartbeat,
                Middlewares.ExceptionHandler,
                Middlewares.RequestParser);

            if (!string.IsNullOrEmpty(accessToken))
            {
                middleware = middleware.Then(Middlewares.GetAuthenticator(accessToken!));
            }

            return middleware;
        }

        private static IRequestDispatcher CreateDispatcher(Type interfaceType, Func<object> factory)
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
            var dispatcher = (IRequestDispatcher)Activator.CreateInstance(interfaceType.GetDispatcherType(), proxy);

            return dispatcher;
        }
    }
}
