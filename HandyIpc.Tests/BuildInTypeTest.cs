using HandyIpc;
using HandyIpcTests.Fixtures;
using HandyIpcTests.Implementations;
using HandyIpcTests.Interfaces;
using HandyIpcTests.Mock;
using Xunit;
using static HandyIpcTests.Mock.MockDataGenerator;

namespace HandyIpcTests
{
    [Collection(nameof(CollectionFixture))]
    public class BuildInTypeTest
    {
        private readonly NamedPipeFixture _namedPipeFixture;
        private readonly SocketFixture _socketFixture;

        public BuildInTypeTest(NamedPipeFixture namedPipeFixture, SocketFixture socketFixture)
        {
            _namedPipeFixture = namedPipeFixture;
            _socketFixture = socketFixture;
        }

        [Fact]
        public void TestBuildInTypesWithNamedPipe()
        {
            var instance = _socketFixture.Client.Resolve<IBuildInType>();
            TestCases(instance);
        }

        [Fact]
        public void TestBuildInTypesWithSocket()
        {
            var instance = _namedPipeFixture.Client.Resolve<IBuildInType>();
            TestCases(instance);
        }

        private static void TestCases(IBuildInType instance)
        {
            Helper.AssertInnerException<TestException>(instance.TestVoidWithoutParams);

            instance.TestDoNothing();

            foreach (var item in BuildInType.Generate())
            {
                string result = instance.TestVoidWithBasicTypeParams(
                    item.Float, item.Double, item.Long, item.Int, item.Short, item.Ulong, item.Uint, item.Ushort, item.Char, item.Byte);
                string expected = $"{item.Float}{item.Double}{item.Long}{item.Int}{item.Short}{item.Ulong}{item.Uint}{item.Ushort}{item.Char}{item.Byte}";
                Assert.Equal(expected, result);
            }

            foreach (byte value in Bytes())
            {
                Assert.Equal(value, instance.TestByte(value));
            }

            foreach (short value in Shorts())
            {
                Assert.Equal(value, instance.TestShort(value));
            }

            foreach (int value in Ints())
            {
                Assert.Equal(value, instance.TestInt(value));
            }

            foreach (long value in Longs())
            {
                Assert.Equal(value, instance.TestLong(value));
            }

            foreach (ushort value in Ushorts())
            {
                Assert.Equal(value, instance.TestUshort(value));
            }

            foreach (uint value in Uints())
            {
                Assert.Equal(value, instance.TestUint(value));
            }

            foreach (ulong value in Ulongs())
            {
                Assert.Equal(value, instance.TestUlong(value));
            }

            foreach (float value in Floats())
            {
                Assert.Equal(value, instance.TestFloat(value));
            }

            foreach (double value in Doubles())
            {
                Assert.Equal(value, instance.TestDouble(value));
            }

            foreach (char value in Chars())
            {
                Assert.Equal(value, instance.TestChar(value));
            }

            foreach (object? value in Null())
            {
                Assert.Equal(value, instance.TestNull(value));
            }

            foreach (byte[] value in ByteArrays())
            {
                Assert.Equal(value, instance.TestByteArray(value));
            }
        }
    }
}
