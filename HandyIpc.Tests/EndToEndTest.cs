using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HandyIpcTests.Implementations;
using HandyIpcTests.Interfaces;
using HandyIpcTests.Mock;
using Xunit;
using static HandyIpcTests.Mock.MockDataGenerator;

namespace HandyIpcTests
{
    public class EndToEndTest : IClassFixture<EndToEndTestFixture>
    {
        private readonly EndToEndTestFixture _fixture;

        public EndToEndTest(EndToEndTestFixture fixture) => _fixture = fixture;

        [Fact]
        public void TestIBuildInTypeTestInterface()
        {
            var @interface = _fixture.ClientHub.Of<IBuildInTypeTest>();

            Assert.Throws<TestException>(() => @interface.TestVoidWithParams());

            foreach (var item in BuildInType.Generate())
            {
                string result = @interface.TestVoidWithBasicTypeParams(
                    item.Float, item.Double, item.Long, item.Int, item.Short, item.Ulong, item.Uint, item.Ushort, item.Char, item.Byte);
                string expected = $"{item.Float}{item.Double}{item.Long}{item.Int}{item.Short}{item.Ulong}{item.Uint}{item.Ushort}{item.Char}{item.Byte}";
                Assert.Equal(expected, result);
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

        [Fact]
        public async Task TestIGenericTestInterface()
        {
            var remote = _fixture.ClientHub.Of<IGenericTest<ClassWithNewCtor, string>>();
            var local = new GenericTest<ClassWithNewCtor, string>();

            {
                const string expected = "first";
                string actual = remote.First(new[] { expected, "other" });
                Assert.Equal(expected, actual);
            }

            await Assert.ThrowsAsync<TestException>(async () => await remote.TestAsync());

            {
                ClassWithNewCtor.InitialName = Guid.NewGuid().ToString();
                var actual = await remote.TestAsync(string.Empty);

                Assert.Equal(ClassWithNewCtor.InitialName, actual.Name);
            }

            {
                string expected = local.TestTypeOf<string, BuildInType>();
                string actual = remote.TestTypeOf<string, BuildInType>();

                Assert.Equal(expected, actual);
            }

            {
                var expected = new ClassWithNewCtor
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = Guid.NewGuid().ToString()
                };
                var actual = remote.TestGenericConstraint(expected);

                Assert.Equal(expected.Id, actual.Id);
                Assert.Equal(expected.Name, actual.Name);
            }

            {
                var expected = new ClassWithNewCtor
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = Guid.NewGuid().ToString()
                };
                var actual = remote.ReturnOriginalValue(expected);

                Assert.Equal(expected.Id, actual.Id);
                Assert.Equal(expected.Name, actual.Name);
            }

            {
                var actual = remote.Flatten(new List<List<List<string>>>
                {
                    new()
                    {
                        new() { "1", "2", "3", },
                        new() { "4", "5", "6", },
                    },
                    new()
                    {
                        new() { "7", "8", "9", },
                        new() { "10", "11", "12", },
                    },
                });

                Assert.Equal(Enumerable.Range(1, 12).Select(item => $"{item}"), actual);
            }

            {
                var expected = new List<string> { "1", "2", "3", };
                var actual = remote.SendList(expected);

                Assert.Equal(expected.AsEnumerable(), actual);
            }

            {
                var arg1 = new ClassWithNewCtor
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = Guid.NewGuid().ToString()
                };
                var arg2 = Guid.NewGuid().ToString();

                string expected = local.PrintGenericArguments(arg1, arg2);
                string actual = remote.PrintGenericArguments(arg1, arg2);

                Assert.Equal(expected, actual);
            }
        }
    }
}
