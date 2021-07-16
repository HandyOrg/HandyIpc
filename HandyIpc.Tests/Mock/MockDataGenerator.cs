using System;
using System.Collections.Generic;
using System.Linq;

namespace HandyIpcTests.Mock
{
    public static class MockDataGenerator
    {
        private static readonly Random random = new();

        public static IEnumerable<byte> Bytes()
        {
            return Enumerable
                .Range(0, 256)
                .Select(item => (byte)item)
                .Concat(new[] { byte.MinValue, byte.MaxValue });
        }

        public static IEnumerable<short> Shorts()
        {
            return Enumerable
                .Range(0, 1000)
                .Select(_ => (short)random.Next(short.MinValue, short.MaxValue))
                .Concat(new[] { short.MinValue, short.MaxValue });
        }

        public static IEnumerable<int> Ints()
        {
            return Enumerable
                .Range(0, 1000)
                .Select(_ => random.Next(int.MinValue, int.MaxValue))
                .Concat(new[] { int.MinValue, int.MaxValue });
        }

        public static IEnumerable<long> Longs()
        {
            return Enumerable
                .Range(0, 1000)
                .Select(_ => (long)random.Next(int.MinValue, int.MaxValue))
                .Concat(new[] { long.MinValue, long.MaxValue });
        }

        public static IEnumerable<ushort> Ushorts()
        {
            return Enumerable
                .Range(0, 1000)
                .Select(_ => (ushort)random.Next(0, ushort.MaxValue))
                .Concat(new[] { ushort.MinValue, ushort.MaxValue });
        }

        public static IEnumerable<uint> Uints()
        {
            return Enumerable
                .Range(0, 1000)
                .Select(_ => (uint)random.Next(0, int.MaxValue))
                .Concat(new[] { uint.MinValue, uint.MaxValue });
        }

        public static IEnumerable<ulong> Ulongs()
        {
            return Enumerable
                .Range(0, 1000)
                .Select(_ => (ulong)random.Next(0, int.MaxValue))
                .Concat(new[] { ulong.MinValue, ulong.MaxValue });
        }

        public static IEnumerable<float> Floats()
        {
            return Enumerable
                .Range(0, 1000)
                .Select(_ => float.MaxValue * (float)random.NextDouble() + float.MinValue)
                .Concat(new[]
                {
                    float.MinValue,
                    float.MaxValue,
                    float.Epsilon,
                    float.NaN,
                    float.NegativeInfinity,
                    float.PositiveInfinity
                });
        }

        public static IEnumerable<double> Doubles()
        {
            return Enumerable
                .Range(0, 1000)
                .Select(_ => double.MaxValue * random.NextDouble() + double.MinValue)
                .Concat(new[] {
                    double.MinValue,
                    double.MaxValue,
                    double.Epsilon,
                    double.NaN,
                    double.NegativeInfinity,
                    double.PositiveInfinity });
        }

        public static IEnumerable<char> Chars()
        {
            return Enumerable
                .Range(0, 256 * 256)
                .Select(item => (char)item)
                .Concat(new[] { char.MinValue, char.MaxValue });
        }

        public static IEnumerable<object?> Null()
        {
            return new object?[] { null };
        }

        public static IEnumerable<byte[]> ByteArrays()
        {
            return Enumerable
                .Range(0, 1000)
                .Select(_ =>
                {
                    byte[] result = new byte[random.Next(0, 1024)];
                    random.NextBytes(result);
                    return result;
                })
                .Concat(new[] { new byte[0] });
        }
    }
}
