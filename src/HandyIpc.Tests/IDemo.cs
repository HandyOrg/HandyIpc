using System.Threading.Tasks;

namespace HandyIpc.Tests
{
    [IpcContract(AccessToken = "&hu8^Tt6")]
    public interface IDemo<T>
    {
        double Add(double x, double y);

        Task<T> GetDefaultAsync();
    }
}
