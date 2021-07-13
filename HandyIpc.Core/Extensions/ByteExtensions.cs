using System.Collections.Generic;

namespace HandyIpc.Extensions
{
    internal static class ByteExtensions
    {
        public static byte[] Slice(this byte[] bytes, int start, int length)
        {
            byte[] result = new byte[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = bytes[start + i];
            }

            return result;
        }

        public static byte[] ConcatBytes(this IReadOnlyList<byte[]> bytesList)
        {
            int totalLength = 0;
            // The performance of for loops is greater than that of iterations and linq.
            for (int i = 0; i < bytesList.Count; i++)
            {
                totalLength += bytesList[i].Length;
            }

            byte[] result = new byte[totalLength];
            int p = 0;
            for (int i = 0; i < bytesList.Count; i++)
            {
                for (int j = 0; j < bytesList[i].Length; j++)
                {
                    result[p++] = bytesList[i][j];
                }
            }

            return result;
        }
    }
}
