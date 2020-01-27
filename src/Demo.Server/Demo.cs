using System.Threading.Tasks;
using Demo.Contracts;

namespace Demo.Server
{
    public class Demo<T> : IDemo<T>
    {
        public double Add(double x, double y) => x + y;

        public Task<T> GetDefaultAsync() => Task.FromResult<T>(default);
    }
}
