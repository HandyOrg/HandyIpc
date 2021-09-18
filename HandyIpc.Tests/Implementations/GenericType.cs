using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HandyIpcTests.Interfaces;

namespace HandyIpcTests.Implementations
{
    public class GenericType<T1, T2> : IGenericType<T1, T2>
        where T1 : class, new()
    {
        public T1 ReturnOriginalValue(T1 value) => value;

        public string PrintGenericArguments(T1 value1, T2 value2) => $"({typeof(T1)}: {value1}, {typeof(T2)}: {value2})";

        public TM TestGenericConstraint<TM>(TM value) where TM : class, new() => value;

        TM IGenericType<T1, T2>.TestGenericConstraint<TM>(TM value) where TM : class => value;

        public List<TM> SendList<TM>(List<TM> value) => value;

        public TM First<TM>(TM[] value) => value.First();

        public List<TM> Flatten<TM>(List<List<List<TM>>> toFlatten)
        {
            return toFlatten.SelectMany(item => item.SelectMany(subItem => subItem)).ToList();
        }

        public string TestTypeOf<T, U>() => $"{typeof(T)}{typeof(T)}";

        public Task TestAsync() => Task.FromException(new TestException());

        public Task<T1> TestAsync(string id) => Task.FromResult(new T1());
    }
}
