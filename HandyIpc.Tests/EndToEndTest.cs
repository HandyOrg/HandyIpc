using HandyIpcTests.Interfaces;
using Xunit;
using static HandyIpcTests.Mock.MockDataGenerator;

namespace HandyIpcTests
{
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
            var @interface = _fixture.ClientHub.Of<IBuildInTypeTest>();

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
