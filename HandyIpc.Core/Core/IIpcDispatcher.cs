using System;
using System.Threading.Tasks;

namespace HandyIpc.Core
{
    public interface IIpcDispatcher
    {
        Task Dispatch(Context context, Func<Task> next);
    }
}
