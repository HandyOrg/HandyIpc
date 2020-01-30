using System.Collections.Generic;
using System.Threading.Tasks;

namespace HandyIpc.Tests.ContractInterfaces
{
    [IpcContract]
    public interface IGenericInterface<T>
    {
        string PrintTypeAndValue(T value);

        T GetDefaultValue();
    }

    [IpcContract]
    public interface IGenericInterface<T1, T2>
    {
        string PrintTypeAndValue(T1 value1, T2 value2);

        T1 GetDefaultValue1();

        T2 GetDefaultValue2();
    }

    [IpcContract]
    public interface IGenericInterface<T1, in T2, out T3> where T1 : class where T2 : new()
    {
        Task<string> PrintTypeAndValue(T1 value1, T2 value2);

        Task<T1> GetDefaultValueAsync<TT1>(TT1 value);

        T3 GetDefaultValue<TT1, TT2>(IEnumerable<TT1> items, TT2 value);
    }
}
