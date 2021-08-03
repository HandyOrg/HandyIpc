using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace HandyIpc.Core
{
    public delegate Task MiddlewareHandler(Context context, Func<Task> next);

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
                ctx.Output = ctx.Serializer.SerializeResponseError(e);
            }
        }

        public static async Task RequestHeaderParser(Context ctx, Func<Task> next)
        {
            if (Request.TryParse(ctx.Input, ctx.Serializer, out Request request))
            {
                ctx.Request = request;
                await next();
            }
            else
            {
                throw new ArgumentException("Invalid request bytes.");
            }
        }

        public static MiddlewareHandler GetAuthenticator(string accessToken)
        {
            return async (ctx, next) =>
            {
                var request = ctx.Request;

                if (request is null)
                {
                    throw new InvalidOperationException($"The {nameof(Context.Request)} must be parsed from {nameof(Context.Input)} before it can be used.");
                }

                if (string.Equals(request.AccessToken, accessToken, StringComparison.Ordinal))
                {
                    await next();
                }
                else
                {
                    var exception = new AuthenticationException($"Invalid accessToken: '{request.AccessToken}'.");
                    ctx.Logger.Warning($"Failed to authenticate this request (token: {request.AccessToken}).", exception);
                    ctx.Output = ctx.Serializer.SerializeResponseError(exception);
                }
            };
        }

        public static MiddlewareHandler GetGenericDispatcher(Func<Type[], IIpcDispatcher> getProxy)
        {
            return async (ctx, next) =>
            {
                var request = ctx.Request;

                if (request is null)
                {
                    throw new InvalidOperationException($"The {nameof(Context.Request)} must be parsed from {nameof(Context.Input)} before it can be used.");
                }

                if (request.TypeArguments.Any())
                {
                    IIpcDispatcher proxy = getProxy(request.TypeArguments.ToArray());
                    await proxy.Dispatch(ctx, next);
                }
                else
                {
                    await next();
                }
            };
        }

        #endregion

        public static MiddlewareHandler Then(this MiddlewareHandler middleware, MiddlewareHandler nextMiddleware)
        {
            return (ctx, next) => middleware(ctx, () => nextMiddleware(ctx, next));
        }

        public static MiddlewareHandler Compose(this IEnumerable<MiddlewareHandler> middlewareEnumerable)
        {
            return middlewareEnumerable.Aggregate((accumulation, item) => accumulation.Then(item));
        }

        public static MiddlewareHandler Compose(params MiddlewareHandler[] middlewareArray)
        {
            return middlewareArray.Compose();
        }

        public static Func<byte[], Task<byte[]>> ToHandler(this MiddlewareHandler middleware, ISerializer serializer, ILogger logger)
        {
            return async input =>
            {
                var ctx = new Context(input, serializer, logger);
                await middleware(ctx, () => Task.CompletedTask);
                return ctx.Output;
            };
        }
    }
}
