using System;
using System.Threading.Tasks;

namespace HandyIpc
{
    public interface IServer : IDisposable
    {
        Task<IConnection> WaitForConnectionAsync();
    }
}
