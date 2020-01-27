using System.Threading.Tasks;
using HandyIpc;

namespace Demo.Contracts
{
    [IpcContract(AccessToken = "&hu8^Tt6")]
    public interface IDemo<T>
    {
        double Add(double x, double y);

        Task<T> GetDefaultAsync();
    }
}
