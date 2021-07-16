using System;
using HandyIpc;
using HandyIpc.Client;
using HandyIpc.NamedPipe;
using HandyIpc.Serializer.Json;
using HandyIpc.Server;
using HandyIpcTests.Implementations;
using HandyIpcTests.Interfaces;
using Xunit;
using static HandyIpcTests.Mock.MockDataGenerator;

namespace HandyIpcTests
{
    public sealed class EndToEndTestFixture : IDisposable
    {
        private readonly IDisposable _buildInTypeTestServerToken;
        private readonly IDisposable _genericTestServerToken;

        public IBuildInTypeTest BuildInTypeTest { get; }

        public IIpcClientHub ClientHub { get; }

        public EndToEndTestFixture()
        {
            var server = HandyIpcHub
                .CreateServerFactory()
                .UseJsonSerializer()
                .UseNamedPipe()
                .Build();
            _buildInTypeTestServerToken = server.Start<IBuildInTypeTest, BuildInTypeTest>();
            _genericTestServerToken = server.Start(typeof(IGenericTest<,,>), typeof(GenericTest<,,>));

            ClientHub = HandyIpcHub
                .CreateClientFactory()
                .UseJsonSerializer()
                .UseNamedPipe()
                .Build();
            BuildInTypeTest = ClientHub.Of<IBuildInTypeTest>();
        }

        public void Dispose()
        {
            _buildInTypeTestServerToken.Dispose();
            _genericTestServerToken.Dispose();
        }
    }

    public class EndToEndTest : IClassFixture<EndToEndTestFixture>
    {
        private readonly EndToEndTestFixture _fixture;

        public EndToEndTest(EndToEndTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void TestIBuildInTypeTestInterface()
        {
            var @interface = _fixture.BuildInTypeTest;

            foreach (byte value in Bytes())
            {
                Assert.Equal(value, @interface.TestByte(value));
            }

            foreach (short value in Shorts())
            {
                Assert.Equal(value, @interface.TestShort(value));
            }

            foreach (int value in Ints())
            {
                Assert.Equal(value, @interface.TestInt(value));
            }

            foreach (long value in Longs())
            {
                Assert.Equal(value, @interface.TestLong(value));
            }

            foreach (ushort value in Ushorts())
            {
                Assert.Equal(value, @interface.TestUshort(value));
            }

            foreach (uint value in Uints())
            {
                Assert.Equal(value, @interface.TestUint(value));
            }

            foreach (ulong value in Ulongs())
            {
                Assert.Equal(value, @interface.TestUlong(value));
            }

            foreach (float value in Floats())
            {
                Assert.Equal(value, @interface.TestFloat(value));
            }

            foreach (double value in Doubles())
            {
                Assert.Equal(value, @interface.TestDouble(value));
            }

            foreach (char value in Chars())
            {
                Assert.Equal(value, @interface.TestChar(value));
            }

            foreach (object? value in Null())
            {
                Assert.Equal(value, @interface.TestNull(value));
            }

            foreach (byte[] value in ByteArrays())
            {
                Assert.Equal(value, @interface.TestByteArray(value));
            }
        }
    }
}
