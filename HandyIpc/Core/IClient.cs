using System.Threading.Tasks;

namespace HandyIpc.Core
{
    public interface IClient
    {
        IConnection Connect();

        Task<IConnection> ConnectAsync();
    }
}
