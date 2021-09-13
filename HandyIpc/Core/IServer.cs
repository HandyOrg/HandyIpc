using System;
using System.Threading.Tasks;

namespace HandyIpc.Core
{
    public interface IServer : IDisposable
    {
        Task<IConnection> WaitForConnectionAsync();
    }
}
