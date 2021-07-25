using System.Threading;
using System.Threading.Tasks;

namespace HandyIpc.Core
{
    public interface IRmiServer
    {
        Task RunAsync(string identifier, MiddlewareHandler middleware, CancellationToken token);
    }
}
