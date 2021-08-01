using System.Collections.Generic;

namespace HandyIpc.Core
{
    public static class Signals
    {
        public static readonly byte[] Empty = { 0 };
        public static readonly byte[] Unit = { 1 };

        public static bool IsEmpty(this byte[] bytes) => bytes.Is(Empty);

        public static bool IsUnit(this byte[] bytes) => bytes.Is(Unit);

        private static bool Is(this IReadOnlyList<byte> bytes, IReadOnlyList<byte> pattern)
        {
            return bytes.Count == 1 && bytes[0] == pattern[0];
        }
    }
}
