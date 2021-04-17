using System;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using HandyIpc.Extensions;

namespace HandyIpc.Server
{
    public static class Middlewares
    {
        public static async Task Heartbeat(Context context, Func<Task> next)
        {
            if (context.Input.IsEmpty())
            {
                context.Output = DataConstants.Empty;
                return;
            }

            await next();
        }

        public static async Task ExceptionHandler(Context context, Func<Task> next)
        {
            try
            {
                await next();
            }
            catch (Exception e)
            {
                HandyIpcHub.Logger.Error("An unexpected exception occurred on the server", e);
                context.Output = Response.ReturnException(e);
            }
        }

        public static async Task RequestParser(Context context, Func<Task> next)
        {
            context.Set(context.Input.ToObject<Request>());

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
                    HandyIpcHub.Logger.Warning(
                        $"Failed to authenticate this request (token: {request.AccessToken}).", exception);
                    ctx.Output = Response.ReturnException(exception);
                }
            };
        }

        public static MiddlewareHandler GetGenericDispatcher(Func<Type[], IIpcDispatcher> getProxy)
        {
            return async (ctx, next) =>
            {
                var request = ctx.Get<Request>();
                if (request.GenericArguments != null && request.GenericArguments.Any())
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

        public static Func<byte[], Task<byte[]>> ToHandler(this MiddlewareHandler middleware)
        {
            return async input =>
            {
                var ctx = new Context(input);
                await middleware(ctx, () => Task.CompletedTask);
                return ctx.Output;
            };
        }
    }
}
