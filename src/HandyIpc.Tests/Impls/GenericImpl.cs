using System.Collections.Generic;
using System.Threading.Tasks;
using HandyIpc.Tests.ContractInterfaces;

namespace HandyIpc.Tests.Impls
{
    internal class GenericImpl<T1, T2, T3> : IGenericInterface<T1, T2, T3>
        where T1 : class where T2 : new()
    {
        public T3 GetDefaultValue<TT1, TT2>(IEnumerable<TT1> items, TT2 value) => default;

        public Task<T1> GetDefaultValueAsync<TT1>(TT1 value) => Task.FromResult<T1>(default);

        public Task<string> PrintTypeAndValue(T1 value1, T2 value2) => Task.FromResult($"[{typeof(T1)}: {value1}], [{typeof(T2)}: {value2}]");
    }

    public class GenericImpl<T1, T2> : IGenericInterface<T1, T2>
    {
        public T1 GetDefaultValue1() => default;

        public T2 GetDefaultValue2() => default;

        public string PrintTypeAndValue(T1 value1, T2 value2) => $"[{typeof(T1)}: {value1}], [{typeof(T2)}: {value2}]";
    }

    internal class GenericImpl<T> : IGenericInterface<T>
    {
        public T GetDefaultValue() => default;

        public string PrintTypeAndValue(T value) => $"[{typeof(T)}: {value}]";
    }
}
