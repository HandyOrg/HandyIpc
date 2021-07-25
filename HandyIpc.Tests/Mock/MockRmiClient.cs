using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HandyIpc;
using HandyIpc.Client;
using Xunit;

namespace HandyIpcTests.Mock
{
    public interface ITestableRmiClient
    {
        string ExpectedIdentifier { get; set; }

        RequestHeader ExpectedRequest { get; set; }

        IReadOnlyList<Argument> ExpectedArguments { get; set; }

        bool IsReturnVoid { get; set; }
    }

    public class MockRmiClient : IRmiClient, ITestableRmiClient
    {
        public string ExpectedIdentifier { get; set; } = null!;

        public RequestHeader ExpectedRequest { get; set; } = null!;

        public IReadOnlyList<Argument> ExpectedArguments { get; set; } = null!;

        public bool IsReturnVoid { get; set; }

        public T Invoke<T>(string identifier, RequestHeader request, IReadOnlyList<Argument> arguments)
        {
            AssertAll(identifier, request, arguments);

            // It will not be used.
            return IsReturnVoid ? (T)(object)Signals.Unit : default!;
        }

        public Task<T> InvokeAsync<T>(string identifier, RequestHeader request, IReadOnlyList<Argument> arguments)
        {
            AssertAll(identifier, request, arguments);

            // It will not be used.
            return Task.FromResult<T>(default!);
        }

        private void AssertAll(string identifier, RequestHeader request, IReadOnlyList<Argument> arguments)
        {
            Assert.Equal(ExpectedIdentifier, identifier);

            Assert.Equal(ExpectedRequest.AccessToken, request.AccessToken);
            AssertNullableArray(ExpectedRequest.ArgumentTypes, request.ArgumentTypes);
            AssertNullableArray(ExpectedRequest.GenericArguments, request.GenericArguments);
            AssertNullableArray(ExpectedRequest.MethodGenericArguments, request.MethodGenericArguments);
            Assert.Equal(ExpectedRequest.MethodName, request.MethodName);

            Assert.Equal(ExpectedArguments.AsEnumerable(), arguments.AsEnumerable());
        }

        private static void AssertNullableArray<T>(T[]? expected, T[]? actual)
        {
            if (expected is null)
            {
                Assert.Null(actual);
            }
            else
            {
                Assert.NotNull(actual);
                Assert.Equal(expected.AsEnumerable(), actual!.AsEnumerable());
            }
        }
    }
}
