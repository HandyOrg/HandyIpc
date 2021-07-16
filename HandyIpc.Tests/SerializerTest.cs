using System.Collections.Generic;
using HandyIpc;
using HandyIpc.Serializer.Json;
using Xunit;
using static HandyIpcTests.Mock.MockDataGenerator;

namespace HandyIpcTests
{
    public class SerializerTest
    {
        [Fact]
        public void TestCSharpBuildInTypes()
        {
            ISerializer serializer = new JsonSerializer();

            TestOneType(serializer, Bytes());
            TestOneType(serializer, Shorts());
            TestOneType(serializer, Ints());
            TestOneType(serializer, Longs());
            TestOneType(serializer, Ushorts());
            TestOneType(serializer, Uints());
            TestOneType(serializer, Ulongs());
            TestOneType(serializer, Floats());
            TestOneType(serializer, Doubles());
            TestOneType(serializer, Chars());
            TestOneType(serializer, Null());
            TestOneType(serializer, ByteArrays());
        }

        private static void TestOneType<T>(ISerializer serializer, IEnumerable<T> testCases)
        {
            foreach (T testCase in testCases)
            {
                byte[] bytes = serializer.Serialize(testCase, typeof(T));
                T result = (T)serializer.Deserialize(bytes, typeof(T))!;
                Assert.Equal(testCase, result);
            }
        }
    }
}
