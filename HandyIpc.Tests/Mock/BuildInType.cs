using System.Collections.Generic;
using static HandyIpcTests.Mock.MockDataGenerator;

namespace HandyIpcTests.Mock
{
    public class BuildInType
    {
        public float Float { get; init; }

        public double Double { get; init; }

        public long Long { get; init; }

        public int Int { get; init; }

        public short Short { get; init; }

        public ulong Ulong { get; init; }

        public uint Uint { get; init; }

        public ushort Ushort { get; init; }

        public char Char { get; init; }

        public byte Byte { get; init; }

        public bool Boolean { get; init; }

        public static IEnumerable<BuildInType> Generate()
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

                yield return new BuildInType
                {
                    Float = @float,
                    Double = @double,
                    Long = @long,
                    Int = @int,
                    Short = @short,
                    Ulong = @ulong,
                    Uint = @uint,
                    Ushort = @ushort,
                    Char = @char,
                    Byte = @byte,
                    Boolean = i >> 1 == 0,
                };
            }
        }
    }
}
