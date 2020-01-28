using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HandyIpc.Tests
{
    public class Demo<T> : IDemo<T>
    {
        public double Add(double x, double y) => x + y;

        public void GenericMethod<T1, T2>(IEnumerable<T1> items1, List<T2> items2)
        {
            Console.WriteLine($"T1 = {typeof(T1)}, T2 = {typeof(T2)}");
        }

        public Task<string> GenericMethod<T1, T2>(IDictionary<T, T1> items1, List<T2> items2)
        {
            return Task.FromResult($"1-{string.Join(", ", items1)}, 2-{string.Join(",,", items2)}");
        }

        public void GenericMethod<T1, T2>(IDictionary<T1, T2> items1, List<T2> items2)
        {
            throw new NotImplementedException();
        }

        public void GenericMethod<T1>(IDictionary<T1, T1> items1, List<T1> items2)
        {
            throw new NotImplementedException();
        }

        public bool GenericMethod<T1, TT>(string a, T1 b)
        {
            Console.WriteLine($"a = {a}, b = {b}");
            return true;
        }

        public Task<T> GetDefaultAsync() => Task.FromResult<T>(default);
    }
}
