using System.Collections.Generic;
using System.Linq;

namespace HandyIpc.Server
{
    public static class Middleware
    {
        public static MiddlewareHandler<TContext> Then<TContext>(
            this MiddlewareHandler<TContext> middleware,
            MiddlewareHandler<TContext> nextMiddleware)
        {
            Guards.ThrowIfNull(middleware, nameof(middleware));

            return nextMiddleware == null
                ? middleware
                : (ctx, next) => middleware(ctx, () => nextMiddleware(ctx, next));
        }

        public static MiddlewareHandler<TContext> Compose<TContext>(this IEnumerable<MiddlewareHandler<TContext>> middlewareEnumerable)
        {
            return middlewareEnumerable.Aggregate((accumulation, item) => accumulation.Then(item));
        }

        public static MiddlewareHandler<TContext> Compose<TContext>(params MiddlewareHandler<TContext>[] middlewareArray)
        {
            return middlewareArray.Compose();
        }
    }
}
