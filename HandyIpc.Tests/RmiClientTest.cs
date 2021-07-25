using HandyIpc;
using HandyIpc.Client;
using HandyIpcTests.Interfaces;
using HandyIpcTests.Mock;
using Xunit;

namespace HandyIpcTests
{
    public class RmiClientTest
    {
        [Fact]
        public void TestBuildInTypeInterface()
        {
            IRmiClient rmiClient = new MockRmiClient();
            ITestableRmiClient mockClient = (ITestableRmiClient)rmiClient;

            var client = HandyIpcHub
                .CreateClientFactory()
                .Use(() => new MockSerializer())
                .Use(_ => rmiClient)
                .Build();

            var buildInTypeTest = client.Of<IBuildInTypeTest>();

            mockClient.ExpectedIdentifier = nameof(IBuildInTypeTest);

            mockClient.ExpectedRequest = GetRequestForBuildInType("TestFloat(float)");
            mockClient.ExpectedArguments = new[] { new Argument(typeof(float), 3.14f) };
            buildInTypeTest.TestFloat(3.14f);

            mockClient.ExpectedRequest = GetRequestForBuildInType("TestDouble(double)");
            mockClient.ExpectedArguments = new[] { new Argument(typeof(double), 3.14) };
            buildInTypeTest.TestDouble(3.14);

            mockClient.ExpectedRequest = GetRequestForBuildInType("TestShort(short)");
            mockClient.ExpectedArguments = new[] { new Argument(typeof(short), (short)42) };
            buildInTypeTest.TestShort(42);

            mockClient.ExpectedRequest = GetRequestForBuildInType("TestInt(int)");
            mockClient.ExpectedArguments = new[] { new Argument(typeof(int), 42) };
            buildInTypeTest.TestInt(42);

            mockClient.ExpectedRequest = GetRequestForBuildInType("TestLong(long)");
            mockClient.ExpectedArguments = new[] { new Argument(typeof(long), (long)42) };
            buildInTypeTest.TestLong(42);

            mockClient.ExpectedRequest = GetRequestForBuildInType("TestUshort(ushort)");
            mockClient.ExpectedArguments = new[] { new Argument(typeof(ushort), (ushort)42) };
            buildInTypeTest.TestUshort(42);

            mockClient.ExpectedRequest = GetRequestForBuildInType("TestUint(uint)");
            mockClient.ExpectedArguments = new[] { new Argument(typeof(uint), (uint)42) };
            buildInTypeTest.TestUint(42);

            mockClient.ExpectedRequest = GetRequestForBuildInType("TestUlong(ulong)");
            mockClient.ExpectedArguments = new[] { new Argument(typeof(ulong), (ulong)42) };
            buildInTypeTest.TestUlong(42);

            mockClient.ExpectedRequest = GetRequestForBuildInType("TestChar(char)");
            mockClient.ExpectedArguments = new[] { new Argument(typeof(char), '\0') };
            buildInTypeTest.TestChar('\0');

            mockClient.ExpectedRequest = GetRequestForBuildInType("TestByte(byte)");
            mockClient.ExpectedArguments = new[] { new Argument(typeof(byte), (byte)255) };
            buildInTypeTest.TestByte(255);

            mockClient.ExpectedRequest = GetRequestForBuildInType("TestVoidWithBasicTypeParams(float, double, long, int, short, ulong, uint, ushort, char, byte)");
            mockClient.ExpectedArguments = new[]
            {
                new Argument(typeof(float), 3.14f),
                new Argument(typeof(double), 3.14),
                new Argument(typeof(long), (long)42),
                new Argument(typeof(int), 42),
                new Argument(typeof(short), (short)42),
                new Argument(typeof(ulong), (ulong)42),
                new Argument(typeof(uint), (uint)42),
                new Argument(typeof(ushort), (ushort)42),
                new Argument(typeof(char), '\0'),
                new Argument(typeof(byte), (byte)255),
            };
            buildInTypeTest.TestVoidWithBasicTypeParams(3.14f, 3.14, 42, 42, 42, 42, 42, 42, '\0', 255);
        }

        private static RequestHeader GetRequestForBuildInType(string methodName)
        {
            return new(methodName)
            {
                ArgumentTypes = null,
                AccessToken = null,
                GenericArguments = null,
                MethodGenericArguments = null,
            };
        }
    }
}
