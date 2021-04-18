using System;
using System.Threading.Tasks;

namespace HandyIpc.Server
{
    public delegate Task MiddlewareHandler(Context context, Func<Task> next);
}