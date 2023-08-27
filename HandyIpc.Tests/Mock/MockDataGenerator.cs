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
            return new[] { byte.MinValue, byte.MaxValue }
                .Concat(Enumerable
                .Range(0, 256)
                .Select(item => (byte)item));
        }

        public static IEnumerable<short> Shorts()
        {
            return new[] { short.MinValue, short.MaxValue }
                .Concat(Enumerable
                .Range(0, 1000)
                .Select(_ => (short)random.Next(short.MinValue, short.MaxValue)));
        }

        public static IEnumerable<int> Ints()
        {
            return new[] { int.MinValue, int.MaxValue }
                .Concat(Enumerable
                .Range(0, 1000)
                .Select(_ => random.Next(int.MinValue, int.MaxValue)));
        }

        public static IEnumerable<long> Longs()
        {
            return new[] { long.MinValue, long.MaxValue }
                .Concat(Enumerable
                .Range(0, 1000)
                .Select(_ => (long)random.Next(int.MinValue, int.MaxValue)));
        }

        public static IEnumerable<ushort> Ushorts()
        {
            return new[] { ushort.MinValue, ushort.MaxValue }
                .Concat(Enumerable
                .Range(0, 1000)
                .Select(_ => (ushort)random.Next(0, ushort.MaxValue)));
        }

        public static IEnumerable<uint> Uints()
        {
            return new[] { uint.MinValue, uint.MaxValue }
                .Concat(Enumerable
                .Range(0, 1000)
                .Select(_ => (uint)random.Next(0, int.MaxValue)));
        }

        public static IEnumerable<ulong> Ulongs()
        {
            return new[] { ulong.MinValue, ulong.MaxValue }
                .Concat(Enumerable
                .Range(0, 1000)
                .Select(_ => (ulong)random.Next(0, int.MaxValue)));
        }

        public static IEnumerable<float> Floats()
        {
            return new[]
                {
                    float.MinValue,
                    float.MaxValue,
                    float.Epsilon,
                    float.NaN,
                    float.NegativeInfinity,
                    float.PositiveInfinity
                }
                .Concat(Enumerable
                .Range(0, 1000)
                .Select(_ => float.MaxValue * (float)random.NextDouble() + float.MinValue));
        }

        public static IEnumerable<double> Doubles()
        {
            return new[]
                {
                    double.MinValue,
                    double.MaxValue,
                    double.Epsilon,
                    double.NaN,
                    double.NegativeInfinity,
                    double.PositiveInfinity
                }
                .Concat(Enumerable
                .Range(0, 1000)
                .Select(_ => double.MaxValue * random.NextDouble() + double.MinValue));
        }

        public static IEnumerable<char> Chars()
        {
            return new[] { char.MinValue, char.MaxValue }
                .Concat(Enumerable
                .Range(0, 256 * 256)
                .Select(item => (char)item));
        }

        public static IEnumerable<object?> Null()
        {
            return new object?[] { null };
        }

        public static IEnumerable<byte[]> ByteArrays()
        {
            return new[] { Array.Empty<byte>() }
                .Concat(Enumerable
                .Range(0, 1000)
                .Select(_ =>
                {
                    byte[] result = new byte[random.Next(0, 1024)];
                    random.NextBytes(result);
                    return result;
                }));
        }
    }
}
