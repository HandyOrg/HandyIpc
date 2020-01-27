using System;
using System.Threading.Tasks;

namespace HandyIpc.Server
{
    public interface IIpcServerProxy
    {
        Task Dispatch(Context context, Func<Task> next);
    }
}
