using System.Threading;
using System.Threading.Tasks;

namespace HandyIpc.Server
{
    public interface IUnderlyingServer<out TContext>
    {
        Task RunAsync(string identifier, MiddlewareHandler<TContext> middleware, CancellationToken token);
    }
}
