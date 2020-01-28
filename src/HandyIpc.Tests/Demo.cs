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

        public Task<T> GetDefaultAsync() => Task.FromResult<T>(default);
    }
}
