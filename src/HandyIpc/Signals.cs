using System.Collections.Generic;

namespace HandyIpc
{
    public static class Signals
    {
        public static readonly byte[] Empty = { 0 };
        public static readonly byte[] Unit = { 1 };
        public static readonly byte[] Retry = { 2 };

        public static bool IsEmpty(this byte[] bytes) => bytes.Is(Empty);

        public static bool IsUnit(this byte[] bytes) => bytes.Is(Unit);

        public static bool IsRetry(this byte[] bytes) => bytes.Is(Retry);

        private static bool Is(this IReadOnlyList<byte> bytes, IReadOnlyList<byte> pattern)
        {
            return bytes.Count == 1 && bytes[0] == pattern[0];
        }
    }
}
