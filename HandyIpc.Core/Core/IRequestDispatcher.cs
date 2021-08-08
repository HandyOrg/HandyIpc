using System;
using System.Threading.Tasks;

namespace HandyIpc.Core
{
    public interface IRequestDispatcher
    {
        Task Dispatch(Context context, Func<Task> next);
    }
}
