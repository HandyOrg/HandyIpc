using System;
using System.Collections.Generic;

namespace HandyIpc.Core
{
    internal static class ByteExtensions
    {
        internal static byte[] Slice(this byte[] bytes, int start, int length)
        {
            byte[] result = new byte[length];
            Array.Copy(bytes, start, result, 0, length);

            return result;
        }

        internal static byte[] ConcatBytes(this IReadOnlyList<byte[]> bytesList)
        {
            int totalLength = 0;
            // The "for" performance is better than "foreach" or "linq".
            for (int i = 0; i < bytesList.Count; i++)
            {
                totalLength += bytesList[i].Length;
            }

            byte[] result = new byte[totalLength];
            // The "for" performance is better than "foreach" or "linq".
            for (int i = 0, offset = 0; i < bytesList.Count; i++)
            {
                byte[] bytes = bytesList[i];
                bytes.CopyTo(result, offset);
                offset += bytes.Length;
            }

            return result;
        }

        internal static bool EqualsHeaderBytes(this byte[] expected, byte[] actual)
        {
            if (expected.Length > actual.Length)
            {
                return false;
            }

            for (int i = 0; i < expected.Length; i++)
            {
                if (expected[i] != actual[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
