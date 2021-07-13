using System;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace HandyIpc.Server
{
    public static class Middlewares
    {
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
                ctx.Output = Signals.GetResponseError(e, ctx.Serializer.Serialize);
            }
        }

        public static async Task RequestParser(Context ctx, Func<Task> next)
        {
            ctx.Set(Signals.GetRequest(ctx.Input, ctx.Serializer.Serialize));

            await next();
        }

        public static MiddlewareHandler GetAuthenticator(string accessToken)
        {
            return async (ctx, next) =>
            {
                var request = ctx.Get<Request>();

                if (string.Equals(request.AccessToken, accessToken, StringComparison.InvariantCulture))
                {
                    await next();
                }
                else
                {
                    var exception = new AuthenticationException($"Invalid accessToken: '{request.AccessToken}'.");
                    ctx.Logger.Warning($"Failed to authenticate this request (token: {request.AccessToken}).", exception);
                    ctx.Output = Signals.GetResponseError(exception, ctx.Serializer.Serialize);
                }
            };
        }

        public static MiddlewareHandler GetGenericDispatcher(Func<Type[], IIpcDispatcher> getProxy)
        {
            return async (ctx, next) =>
            {
                var request = ctx.Get<Request>();
                if (request.GenericArguments is not null && request.GenericArguments.Any())
                {
                    var proxy = getProxy(request.GenericArguments);
                    await proxy.Dispatch(ctx, next);
                }
                else
                {
                    await next();
                }
            };
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
