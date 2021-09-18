using System.Threading.Tasks;
using HandyIpc;
using HandyIpcTests.Mock;

namespace HandyIpcTests.Interfaces
{
    [IpcContract]
    public interface ITaskReturnType
    {
        Task TestDoNothing();

        Task TestException();

        Task<string> TestReturnString();

        Task<double> TestReturnDouble();

        Task<ComplexType> TestReturnCustomType();

        Task<string> TestWithParams(string str);

        Task<double> Add(double x, double y);

        void SyncMethod();

        void SyncMethodWithException();

        int Add(int x, int y);

        Task<T> TestGenericType<T>(T value);
    }
}
