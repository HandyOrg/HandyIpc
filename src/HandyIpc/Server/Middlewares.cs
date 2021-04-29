﻿using System;
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
                ctx.Output = Messages.Empty;
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
                ctx.Output = ctx.Serializer.SerializeResponse(new Response { Exception = e });
            }
        }

        public static async Task RequestParser(Context ctx, Func<Task> next)
        {
            ctx.Set(ctx.Serializer.DeserializeRequest(ctx.Input));

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
                    ctx.Output = ctx.Serializer.SerializeResponse(new Response { Exception = exception });
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

        public static Func<byte[], Task<byte[]>> ToHandler(
            this MiddlewareHandler middleware,
            ISerializer serializationService,
            ILogger logger)
        {
            return async input =>
            {
                var ctx = new Context(input, serializationService, logger);
                await middleware(ctx, () => Task.CompletedTask);
                return ctx.Output;
            };
        }
    }
}