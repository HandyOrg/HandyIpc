using System;
using System.Collections.Generic;
using System.Linq;
using HandyIpc;
using HandyIpc.Serializer.Json;
using Xunit;

namespace HandyIpcTests
{
    public class SerializerTest
    {
        [Fact]
        public void TestCSharpBuildInTypes()
        {
            ISerializer serializer = new JsonSerializer();
            var random = new Random();

            TestOneType(serializer, new[] { byte.MinValue, byte.MaxValue });
            TestOneType(serializer, Enumerable.Range(0, 256).Select(item => (byte)item));

            TestOneType(serializer, new[] { short.MinValue, short.MaxValue });
            TestOneType(serializer, Enumerable.Range(0, 1000).Select(_ => (short)random.Next(short.MinValue, short.MaxValue)));

            TestOneType(serializer, new[] { int.MinValue, int.MaxValue });
            TestOneType(serializer, Enumerable.Range(0, 1000).Select(_ => random.Next(int.MinValue, int.MaxValue)));

            TestOneType(serializer, new[] { long.MinValue, long.MaxValue });
            TestOneType(serializer, Enumerable.Range(0, 1000).Select(_ => (long)random.Next(int.MinValue, int.MaxValue)));

            TestOneType(serializer, new[] { ushort.MinValue, ushort.MaxValue });
            TestOneType(serializer, Enumerable.Range(0, 1000).Select(_ => (ushort)random.Next(0, ushort.MaxValue)));

            TestOneType(serializer, new[] { uint.MinValue, uint.MaxValue });
            TestOneType(serializer, Enumerable.Range(0, 1000).Select(_ => (uint)random.Next(0, int.MaxValue)));

            TestOneType(serializer, new[] { ulong.MinValue, ulong.MaxValue });
            TestOneType(serializer, Enumerable.Range(0, 1000).Select(_ => (ulong)random.Next(0, int.MaxValue)));

            TestOneType(serializer, new[] { float.MinValue, float.MaxValue, float.Epsilon, float.NaN, float.NegativeInfinity, float.PositiveInfinity });
            TestOneType(serializer, Enumerable.Range(0, 1000).Select(_ => float.MaxValue * (float)random.NextDouble() + float.MinValue));

            TestOneType(serializer, new[] { double.MinValue, double.MaxValue, double.Epsilon, double.NaN, double.NegativeInfinity, double.PositiveInfinity });
            TestOneType(serializer, Enumerable.Range(0, 1000).Select(_ => double.MaxValue * random.NextDouble() + double.MinValue));

            TestOneType(serializer, new[] { char.MinValue, char.MaxValue });
            TestOneType(serializer, Enumerable.Range(0, 256 * 256).Select(item => (char)item));

            TestOneType(serializer, new string?[] { null });

            TestOneType(serializer, Enumerable.Range(0, 1000).Select(_ =>
            {
                byte[] result = new byte[random.Next(0, 1024 * 1024)];
                random.NextBytes(result);
                return result;
            }));
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
