using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HandyIpc.Core
{
    public static class SerializerExtensions
    {
        private const string ResHeader = "handyipc/res";

        private static readonly byte[] Version = { 1 };
        private static readonly byte[] ResHeaderBytes = Encoding.ASCII.GetBytes(ResHeader);

        private static readonly byte[] ResponseValueFlag = { 1 };
        private static readonly byte[] ResponseErrorFlag = { 0 };

        public static byte[] SerializeResponseValue(this ISerializer self, object? value, Type? type)
        {
            List<byte[]> result = new()
            {
                ResHeaderBytes,
                Version,
                ResponseValueFlag,
            };

            if (value is not null && type is not null)
            {
                byte[] valueBytes = self.Serialize(value, type);
                result.Add(BitConverter.GetBytes(valueBytes.Length));
                result.Add(valueBytes);
            }

            return result.ConcatBytes();
        }

        public static byte[] SerializeResponseError(this ISerializer self, Exception exception)
        {
            byte[] typeBytes = self.Serialize(exception.GetType(), typeof(Type));
            byte[] exceptionBytes = self.Serialize(exception, exception.GetType());
            List<byte[]> result = new()
            {
                ResHeaderBytes,
                Version,
                ResponseErrorFlag,
                BitConverter.GetBytes(typeBytes.Length),
                typeBytes,
                BitConverter.GetBytes(exceptionBytes.Length),
                exceptionBytes,
            };

            return result.ConcatBytes();
        }

        public static bool DeserializeResponse(this ISerializer self, byte[] bytes, Type valueType, out object? value, out Exception? exception)
        {
            int offset = 0;
            if (!bytes.Slice(offset, ResHeaderBytes.Length).SequenceEqual(ResHeaderBytes))
            {
                throw new ArgumentException("The bytes is not valid response data.", nameof(bytes));
            }

            // Skip the version number, because the current version is the first one
            // and there is no need to consider compatibility issues.
            offset += ResHeaderBytes.Length + Version.Length;
            bool hasValue = bytes.Slice(offset, 1)[0] == ResponseValueFlag[0];
            offset++;
            if (hasValue)
            {
                exception = null;
                if (offset == bytes.Length)
                {
                    value = null;
                }
                else
                {
                    int valueLength = BitConverter.ToInt32(bytes.Slice(offset, sizeof(int)), 0);
                    offset += sizeof(int);
                    value = self.Deserialize(bytes.Slice(offset, valueLength), valueType);
                }
            }
            else
            {
                int errorTypeLength = BitConverter.ToInt32(bytes.Slice(offset, sizeof(int)), 0);
                offset += sizeof(int);

                Type errorType = (Type)self.Deserialize(bytes.Slice(offset, errorTypeLength), typeof(Type))!;
                offset += errorTypeLength;

                int errorLength = BitConverter.ToInt32(bytes.Slice(offset, sizeof(int)), 0);
                offset += sizeof(int);

                exception = (Exception?)self.Deserialize(bytes.Slice(offset, errorLength), errorType);
                value = null;
            }

            return hasValue;
        }

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
