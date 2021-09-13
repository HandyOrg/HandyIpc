using System;
using System.Collections.Generic;
using System.Text;

namespace HandyIpc.Core
{
    public static class Response
    {
        private const string ResHeader = "handyipc/res";

        private static readonly byte[] Version = { 1 };
        private static readonly byte[] ResHeaderBytes = Encoding.ASCII.GetBytes(ResHeader);

        private static readonly byte[] ResponseValueFlag = { 1 };
        private static readonly byte[] ResponseErrorFlag = { 0 };

        public static byte[] Value(object? value, Type? type, ISerializer serializer)
        {
            List<byte[]> result = new()
            {
                ResHeaderBytes,
                Version,
                ResponseValueFlag,
            };

            if (value is not null && type is not null)
            {
                byte[] valueBytes = serializer.Serialize(value, type);
                result.Add(BitConverter.GetBytes(valueBytes.Length));
                result.Add(valueBytes);
            }

            return result.ConcatBytes();
        }

        public static byte[] Error(Exception exception, ISerializer serializer)
        {
            byte[] typeBytes = serializer.Serialize(exception.GetType(), typeof(Type));
            byte[] exceptionBytes = serializer.Serialize(exception, exception.GetType());
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

        public static bool TryParse(byte[] bytes, Type valueType, ISerializer serializer, out object? value, out Exception? exception)
        {
            if (!ResHeaderBytes.EqualsHeaderBytes(bytes))
            {
                throw new ArgumentException("The bytes is not valid response data.", nameof(bytes));
            }

            // Skip the version number, because the current version is the first one
            // and there is no need to consider compatibility issues.
            int offset = ResHeaderBytes.Length + Version.Length;
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
                    int valueLength = BitConverter.ToInt32(bytes, offset);
                    offset += sizeof(int);
                    value = serializer.Deserialize(bytes.Slice(offset, valueLength), valueType);
                }
            }
            else
            {
                int errorTypeLength = BitConverter.ToInt32(bytes, offset);
                offset += sizeof(int);

                Type errorType = (Type)serializer.Deserialize(bytes.Slice(offset, errorTypeLength), typeof(Type))!;
                offset += errorTypeLength;

                int errorLength = BitConverter.ToInt32(bytes, offset);
                offset += sizeof(int);

                exception = (Exception)serializer.Deserialize(bytes.Slice(offset, errorLength), errorType)!;
                value = null;
            }

            return hasValue;
        }
    }
}
