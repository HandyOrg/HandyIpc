using System;
using System.Threading.Tasks;
using HandyIpc;
using HandyIpcTests.Fixtures;
using HandyIpcTests.Implementations;
using HandyIpcTests.Interfaces;
using HandyIpcTests.Mock;
using Xunit;

namespace HandyIpcTests
{
    [Collection(nameof(CollectionFixture))]
    public class TaskReturnTypeTest
    {
        private readonly NamedPipeFixture _namedPipeFixture;
        private readonly SocketFixture _socketFixture;

        public TaskReturnTypeTest(NamedPipeFixture namedPipeFixture, SocketFixture socketFixture)
        {
            _namedPipeFixture = namedPipeFixture;
            _socketFixture = socketFixture;
        }

        [Fact]
        public Task TestBuildInTypesWithNamedPipe()
        {
            var instance = _socketFixture.Client.Resolve<ITaskReturnType>();
            return TestCases(instance);
        }

        [Fact]
        public Task TestBuildInTypesWithSocket()
        {
            var instance = _namedPipeFixture.Client.Resolve<ITaskReturnType>();
            return TestCases(instance);
        }

        private static async Task TestCases(ITaskReturnType instance)
        {
            Random random = new();
            ITaskReturnType local = new TaskReturnTypeImpl();

            for (int i = 0; i < 1000; i++)
            {
                int a = random.Next();
                int b = random.Next();
                int r = instance.Add(a, b);
                Assert.Equal(a + b, r);
            }

            for (int i = 0; i < 1000; i++)
            {
                double a = random.NextDouble();
                double b = random.NextDouble();
                double r = await instance.Add(a, b);
                Assert.Equal(a + b, r);
            }

            string str = string.Empty;
            for (int i = 0; i < 1000; i++)
            {
                str += Guid.NewGuid().ToString();
                string r = await instance.TestWithParams(str);
                Assert.Equal(r, str);
            }

            Assert.Equal("test str", await instance.TestGenericType("test str"));
            Assert.Equal(int.MaxValue, await instance.TestGenericType(int.MaxValue));
            Assert.Equal(new ComplexType(), await instance.TestGenericType(new ComplexType()));

            var input = new GenericType<string, double>();
            var output = await instance.TestGenericType(input);
            Assert.NotSame(input, output);
            Assert.Equal(input, output);

            instance.SyncMethod();

            Helper.AssertInnerException<TestException>(instance.SyncMethodWithException);

            await instance.TestDoNothing();

            await Helper.AssertInnerException<TestException>(instance.TestException);

            await Helper.AssertInnerException<TestException>(() => instance.TestGenericException(string.Empty));

            Assert.Equal(await local.TestReturnDouble(), await instance.TestReturnDouble());

            Assert.Equal(await local.TestReturnCustomType(), await instance.TestReturnCustomType());
            Assert.NotSame(await local.TestReturnCustomType(), await instance.TestReturnCustomType());

            Assert.Equal(await local.TestReturnString(), await instance.TestReturnString());
        }
    }
}
