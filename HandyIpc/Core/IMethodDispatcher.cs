using System;
using System.Threading.Tasks;

namespace HandyIpc.Core
{
    public interface IMethodDispatcher
    {
        Task Dispatch(Context context, Func<Task> next);
    }
}
