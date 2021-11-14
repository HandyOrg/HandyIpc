using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HandyIpc.Core
{
    public delegate Task Middleware(Context context, Func<Task> next);

    public delegate Task<byte[]> RequestHandler(byte[] input);

    public static class Middlewares
    {
        #region Build-in middlewares

        public static async Task Heartbeat(Context ctx, Func<Task> next)
        {
            if (ctx.Input.IsEmpty())
            {
                ctx.Output = Signals.Empty;
                return;
            }

            await next();
        }

        public static async Task ExceptionHandler(Context ctx, Func<Task> next)
        {
            try
            {
                await next();
            }
            catch (Exception e)
            {
                ctx.Logger.Error("An unexpected exception occurred on the server", e);
                ctx.Output = Response.Error(e, ctx.Serializer);
            }
        }

        public static Middleware GetHandleRequest(IReadOnlyDictionary<string, Middleware> map)
        {
            return async (ctx, next) =>
            {
                if (Request.TryParse(ctx.Input, ctx.Serializer, out Request request))
                {
                    ctx.Request = request;
                    if (map.TryGetValue(request.Name, out Middleware middleware))
                    {
                        await middleware(ctx, next);
                        return;
                    }
                }

                await next();
            };
        }

        public static Middleware GetHandleEvent(ConcurrentDictionary<string, NotifierManager> notifiers)
        {
            return async (ctx, next) =>
            {
                if (Subscription.TryParse(ctx.Input, ctx.Serializer, out Subscription subscription))
                {
                    switch (subscription.Type)
                    {
                        case SubscriptionType.Add:
                            {
                                var manager = notifiers.GetOrAdd(subscription.Name, _ => new NotifierManager(ctx.Serializer));
                                manager.Subscribe(subscription.CallbackName, subscription.ProcessId, ctx.Connection);
                                ctx.Output = Signals.Unit;
                            }
                            break;
                        case SubscriptionType.Remove:
                            {
                                if (notifiers.TryGetValue(subscription.Name, out NotifierManager manager))
                                {
                                    manager.Unsubscribe(subscription.CallbackName, subscription.ProcessId);
                                }

                                ctx.Output = Signals.Unit;
                            }
                            break;
                    }

                    return;
                }

                await next();
            };
        }

        public static Middleware GetMethodDispatcher(Func<Type[], IMethodDispatcher> getProxy)
        {
            return async (ctx, next) =>
            {
                Request request = EnsureRequest(ctx);

                if (request.TypeArguments.Any())
                {
                    IMethodDispatcher proxy = getProxy(request.TypeArguments.ToArray());
                    await proxy.Dispatch(ctx, next);
                }
                else
                {
                    await next();
                }
            };
        }

        public static Task NotFound(Context ctx, Func<Task> next)
        {
            string message = ctx.Request is { } request
                ? $"Unknown interface method ({request.Name}.{request.MethodName}) invoked."
                : "Unknown interface method invoked.";

            ctx.Logger.Warning(message);
            ctx.Output = Response.Error(new NotSupportedException(message), ctx.Serializer);

            return Task.CompletedTask;
        }

        private static Request EnsureRequest(Context ctx)
        {
            Request? request = ctx.Request;

            if (request is null)
            {
                throw new InvalidOperationException($"The {nameof(Context.Request)} must be parsed from {nameof(Context.Input)} before it be used.");
            }

            return request;
        }

        #endregion

        public static Middleware Then(this Middleware middleware, Middleware nextMiddleware)
        {
            return (ctx, next) => middleware(ctx, () => nextMiddleware(ctx, next));
        }

        public static Middleware Compose(this IEnumerable<Middleware> middlewareEnumerable)
        {
            return middlewareEnumerable.Aggregate((accumulation, item) => accumulation.Then(item));
        }

        public static Middleware Compose(params Middleware[] middlewareArray)
        {
            return middlewareArray.Compose();
        }

        public static RequestHandler ToHandler(this Middleware middleware, Action<Context> configure)
        {
            return async input =>
            {
                var ctx = new Context(input);
                configure(ctx);
                await middleware(ctx, () => Task.CompletedTask);
                return ctx.Output;
            };
        }
    }
}
