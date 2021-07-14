using System.Collections.Generic;
using System.Threading.Tasks;

namespace HandyIpc.Client
{
    public interface IRmiClient
    {
        T Invoke<T>(string identifier, Request request, IReadOnlyList<Argument> arguments);

        Task<T> InvokeAsync<T>(string identifier, Request request, IReadOnlyList<Argument> arguments);
    }
}
