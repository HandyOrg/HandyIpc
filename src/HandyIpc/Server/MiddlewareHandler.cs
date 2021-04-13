using System;
using System.Threading.Tasks;

namespace HandyIpc.Server
{
    public delegate Task MiddlewareHandler<in TContext>(TContext context, Func<Task> next);
}