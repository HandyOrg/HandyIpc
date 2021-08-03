using System.Threading;
using System.Threading.Tasks;

namespace HandyIpc.Core
{
    public interface IRmiServer
    {
        Task RunAsync(string identifier, RequestHandler handler, CancellationToken token);
    }
}
