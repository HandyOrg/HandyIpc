using System.Threading;
using System.Threading.Tasks;

namespace HandyIpc.Server
{
    public interface IRmiServer
    {
        Task RunAsync(string identifier, MiddlewareHandler middleware, CancellationToken token);
    }
}
