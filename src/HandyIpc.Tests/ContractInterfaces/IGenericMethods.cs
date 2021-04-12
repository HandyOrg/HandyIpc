using System.Collections.Generic;
using System.Threading.Tasks;

namespace HandyIpc.Tests.ContractInterfaces
{
    [HandyIpcHub.IpcContract]
    public interface IGenericMethods
    {
        Task<T1> PrintAsync<T1, T2>(List<T2> items1, List<List<List<T1>>> items2);

        Task<string> PrintAsync<T1, T2, T3>();

        Task<IEnumerable<T>> RepeatAsync<T>(T item, int count);
    }
}
