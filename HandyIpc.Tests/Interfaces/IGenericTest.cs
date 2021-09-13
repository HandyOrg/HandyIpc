using System.Collections.Generic;
using System.Threading.Tasks;
using HandyIpc;

namespace HandyIpcTests.Interfaces
{
    [IpcContract]
    public interface IGenericTest<T1, in T2>
        where T1 : class, new()
    {
        T1 ReturnOriginalValue(T1 value);

        string PrintGenericArguments(T1 value1, T2 value2);

        TM TestGenericConstraint<TM>(TM value) where TM : class, new();

        List<TM> SendList<TM>(List<TM> value);

        TM First<TM>(TM[] value);

        List<TM> Flatten<TM>(List<List<List<TM>>> toFlatten);

        string TestTypeOf<T, U>();

        Task TestAsync();

        Task<T1> TestAsync(string id);
    }
}
