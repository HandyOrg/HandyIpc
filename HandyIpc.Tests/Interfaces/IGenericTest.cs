using System.Collections.Generic;
using HandyIpc;

namespace HandyIpcTests.Interfaces
{
    [IpcContract(Identifier = "IGenericTest<T1, in T2, T3>")]
    public interface IGenericTest<T1, in T2, T3>
        where T1 : class
        where T2 : T3
    {
        void TestVoid();

        T1 TestGenericT1(T1 value);

        T1 TestGenericT1(T1 value1, T2 value2);

        TM TestGenericMethod<TM>(TM value) where TM : new();

        List<TM> TestGenericMethod<TM>(List<TM> value);

        TM TestGenericMethod<TM>(TM[] value);

        List<TM> TestNestedGeneric<TM>(List<List<List<TM>>> toFlatten);
    }
}
