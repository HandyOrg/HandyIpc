using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using HandyIpc.Extensions;

namespace HandyIpc.Server
{
    public delegate Task MiddlewareHandler(Context context, Func<Task> next);

    public static class Middleware
    {
        #region Basic middleware

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
                IpcSettings.Instance.Logger.Error("", e);
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
                    IpcSettings.Instance.Logger.Warning("");
                    var exception = new AuthenticationException($"Invalid accessToken: '{request.AccessToken}'.");
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

        #endregion

        #region Build methods

        public static Func<byte[], Task<byte[]>> ToHandler(this MiddlewareHandler middleware)
        {
            return async input =>
            {
                var ctx = new Context(input);
                await middleware(ctx, () => Task.CompletedTask);
                return ctx.Output;
            };
        }

        public static MiddlewareHandler Compose(this MiddlewareHandler middleware, MiddlewareHandler nextMiddleware)
        {
            Guards.ThrowIfNull(middleware, nameof(middleware));

            return nextMiddleware == null
                ? middleware
                : (ctx, next) => middleware(ctx, () => nextMiddleware(ctx, next));
        }

        public static MiddlewareHandler Compose(this IEnumerable<MiddlewareHandler> middlewareEnumerable)
        {
            return middlewareEnumerable.Aggregate((accumulation, item) => accumulation.Compose(item));
        }

        public static MiddlewareHandler Compose(params MiddlewareHandler[] middlewareArray)
        {
            return middlewareArray.Compose();
        }

        #endregion
    }
}
