using System;
using System.Threading.Tasks;

namespace HandyIpc.Server
{
    public interface IIpcDispatcher
    {
        Task Dispatch(Context context, Func<Task> next);
    }
}
