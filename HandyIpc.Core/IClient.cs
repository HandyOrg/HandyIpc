using System.Threading.Tasks;

namespace HandyIpc
{
    public interface IClient
    {
        IConnection Connect();

        Task<IConnection> ConnectAsync();
    }
}
