using System.Threading;
using System.Threading.Tasks;

namespace HandyIpc.Client
{
    public interface IRmiClient
    {
        T Invoke<T>(string pipeName, Request request, object?[]? arguments);

        Task<T> InvokeAsync<T>(string pipeName, Request request, object?[]? arguments);

        // TODO: The CancellationToken is not supported at this time, as this is equivalent to a callback function.
        Task<T> InvokeAsync<T>(string pipeName, Request request, object?[]? arguments, CancellationToken token);
    }
}
