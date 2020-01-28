using System.Collections.Generic;
using System.Threading.Tasks;

namespace HandyIpc.Tests
{
    [IpcContract(AccessToken = "&hu8^Tt6")]
    public interface IDemo<T>
    {
        double Add(double x, double y);

        Task<T> GetDefaultAsync();

        void GenericMethod<T1, T2>(IEnumerable<T1> items1, List<T2> items2);

        Task<string> GenericMethod<T1, T2>(IDictionary<T, T1> items1, List<T2> items2);
        void GenericMethod<T1, T2>(IDictionary<T1, T2> items1, List<T2> items2);
        void GenericMethod<T>(IDictionary<T, T> items1, List<T> items2);
        bool GenericMethod<T1, TT>(string a, T1 b);
        Task GenericMethod<T1, TT>(int a, T1 b);
    }
}
