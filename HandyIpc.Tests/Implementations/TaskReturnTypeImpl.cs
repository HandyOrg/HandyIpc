using System.Threading.Tasks;
using HandyIpcTests.Interfaces;
using HandyIpcTests.Mock;

namespace HandyIpcTests.Implementations
{
    public class TaskReturnTypeImpl : ITaskReturnType
    {
        public Task TestDoNothing() => Task.CompletedTask;

        public Task TestException() => Task.FromException(new TestException());

        public Task<string> TestReturnString() => Task.FromResult("cff112a4-4088-45fd-baf9-c47cec2db799");

        public Task<double> TestReturnDouble() => Task.FromResult(double.Epsilon);

        public Task<ComplexType> TestReturnCustomType() => Task.FromResult(new ComplexType());

        public Task<string> TestWithParams(string str) => Task.FromResult(str);

        public Task<double> Add(double x, double y) => Task.FromResult(x + y);

        public void SyncMethod() { }

        public void SyncMethodWithException() => throw new TestException();

        public int Add(int x, int y) => x + y;

        public Task<T> TestGenericType<T>(T value) => Task.FromResult(value);

        public Task<GenericType<TK, TV>> TestGenericType<TK, TV>(TK key, TV value)
        {
            return Task.FromResult(new GenericType<TK, TV>
            {
                Key = key,
                Value = value,
            });
        }

        public Task TestGenericException<T>(T value) => Task.FromException(new TestException());
    }
}
