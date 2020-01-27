using System.Threading.Tasks;

namespace HandyIpc.Tests
{
    public class Demo<T> : IDemo<T>
    {
        public double Add(double x, double y) => x + y;

        public Task<T> GetDefaultAsync() => Task.FromResult<T>(default);
    }
}
