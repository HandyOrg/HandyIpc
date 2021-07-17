using HandyIpcTests.Implementations;
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

            Assert.Throws<TestException>(() => @interface.TestVoidWithParams());

            {
                using var floatEnumerator = Floats().GetEnumerator();
                using var doubleEnumerator = Doubles().GetEnumerator();
                using var longEnumerator = Longs().GetEnumerator();
                using var intEnumerator = Ints().GetEnumerator();
                using var shortEnumerator = Shorts().GetEnumerator();
                using var ulongEnumerator = Ulongs().GetEnumerator();
                using var uintEnumerator = Uints().GetEnumerator();
                using var ushortEnumerator = Ushorts().GetEnumerator();
                using var charEnumerator = Chars().GetEnumerator();
                using var byteEnumerator = Bytes().GetEnumerator();
                for (int i = 0; i < 256; i++)
                {
                    floatEnumerator.MoveNext();
                    doubleEnumerator.MoveNext();
                    intEnumerator.MoveNext();
                    shortEnumerator.MoveNext();
                    ulongEnumerator.MoveNext();
                    uintEnumerator.MoveNext();
                    ushortEnumerator.MoveNext();
                    charEnumerator.MoveNext();
                    byteEnumerator.MoveNext();

                    float @float = floatEnumerator.Current;
                    double @double = doubleEnumerator.Current;
                    long @long = longEnumerator.Current;
                    int @int = intEnumerator.Current;
                    short @short = shortEnumerator.Current;
                    ulong @ulong = ulongEnumerator.Current;
                    uint @uint = uintEnumerator.Current;
                    ushort @ushort = ushortEnumerator.Current;
                    char @char = charEnumerator.Current;
                    byte @byte = byteEnumerator.Current;

                    string result = @interface.TestVoidWithBasicTypeParams(@float, @double, @long, @int, @short, @ulong, @uint, @ushort, @char, @byte);
                    string expected = $"{@float}{@double}{@long}{@int}{@short}{@ulong}{@uint}{@ushort}{@char}{@byte}";
                    Assert.Equal(expected, result);
                }
            }

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
