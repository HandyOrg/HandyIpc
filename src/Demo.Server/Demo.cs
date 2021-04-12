using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Demo.Contracts;

namespace Demo.Server
{
    public class Demo<T> : IDemo<T>
    {
        public double Add(double x, double y) => x + y;

        public Task<T> GetDefaultAsync() => Task.FromResult<T>(default);

        public string GenericMethod<T1, T2>(T1 a, IEnumerable<T2> items)
        {
            return $"T1={typeof(T1)}, T2={typeof(T2)}";
        }

        public void PrintMessage(string text)
        {
            Console.WriteLine(text);
        }
    }
}
