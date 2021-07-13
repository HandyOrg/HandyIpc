using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HandyIpc.Server
{
    public delegate Task MiddlewareHandler(Context context, Func<Task> next);

    public static class Middleware
    {
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
    }
}
