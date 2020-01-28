using System.Collections.Generic;
using System.Threading.Tasks;

namespace HandyIpc.Tests
{
    [IpcContract(AccessToken = "&hu8^Tt6")]
    public interface IDemo<T>
    {
        double Add(double x, double y);

        Task<T> GetDefaultAsync();

        //void GenericMethod<T1, T2>(IEnumerable<T1> items1, List<T2> items2);
    }
}
