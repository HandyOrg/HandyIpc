using System;
using System.Threading.Tasks;

namespace HandyIpc.Core
{
    public interface IMethodDispatcher
    {
        NotifierManager NotifierManager { get; set; }

        Task Dispatch(Context context, Func<Task> next);
    }
}
