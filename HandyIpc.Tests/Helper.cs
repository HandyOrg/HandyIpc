using System;
using System.Threading.Tasks;
using HandyIpc.Exceptions;
using Xunit;

namespace HandyIpcTests
{
    public static class Helper
    {
        public static void AssertInnerException<T>(Action function)
        {
            try
            {
                function();
            }
            catch (IpcException e)
            {
                Assert.Equal(typeof(T), e.InnerException!.GetType());
            }
        }

        public static async Task AssertInnerException<T>(Func<Task> function)
        {
            try
            {
                await function();
            }
            catch (IpcException e)
            {
                Assert.Equal(typeof(T), e.InnerException!.GetType());
            }
        }
    }
}
