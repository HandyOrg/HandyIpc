using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HandyIpc.Extensions;

namespace HandyIpc
{
    public delegate byte[] Serialize(object? value, Type type);

    public delegate object? Deserialize(byte[] bytes, Type type);

    public static class Signals
    {
        private const string ReqHeader = "handyipc/req";
        private const string ResHeader = "hangyipc/res";

        public static readonly byte[] Empty = { 0 };
        public static readonly byte[] Unit = { 1 };
        public static readonly byte[] Retry = { 2 };

        private static readonly byte[] ReqHeaderBytes = Encoding.ASCII.GetBytes(ReqHeader);
        private static readonly byte[] ResHeaderBytes = Encoding.ASCII.GetBytes(ResHeader);
        private static readonly byte[] Version = { 1 };
        private static readonly byte[] ResponseValueFlag = { 1 };
        private static readonly byte[] ResponseErrorFlag = { 0 };

        public static bool IsEmpty(this byte[] bytes) => bytes.Is(Empty);

        public static bool IsUnit(this byte[] bytes) => bytes.Is(Unit);

        public static bool IsRetry(this byte[] bytes) => bytes.Is(Retry);

        private static bool Is(this IReadOnlyList<byte> bytes, IReadOnlyList<byte> pattern)
        {
            return bytes.Count == 1 && bytes[0] == pattern[0];
        }

        public static byte[] GetRequestBytes(Request request, object?[]? arguments, Type[]? argumentTypes, Serialize serialize)
        {
            byte[] requestBytes = serialize(request, typeof(Request));
            List<byte[]> bytesList = new()
            {
                ReqHeaderBytes,
                Version,
                BitConverter.GetBytes(requestBytes.Length),
                requestBytes,
            };
            if (arguments is not null && argumentTypes is not null && arguments.Length == argumentTypes.Length)
            {
                for (int i = 0; i < arguments.Length; i++)
                {
                    byte[] argumentBytes = serialize(arguments[i], argumentTypes[i]);
                    bytesList.Add(BitConverter.GetBytes(argumentBytes.Length));
                    bytesList.Add(argumentBytes);
                }
            }

            return bytesList.ConcatBytes();
        }

        public static Request GetRequest(byte[] bytes, Deserialize deserialize)
        {
            PreprocessRequestBytes(bytes, out int requestOffset, out int requestLength);

            return (Request)deserialize(bytes.Slice(requestOffset, requestLength), typeof(Request))!;
        }

        public static object?[]? GetArguments(byte[] bytes, Type[] argumentTypes, Deserialize deserialize)
        {
            PreprocessRequestBytes(bytes, out int requestOffset, out int requestLength);

            int argumentOffset = requestOffset + requestLength;
            if (bytes.Length <= argumentOffset)
            {
                return null;
            }

            List<object?> result = new();
            int typeIndex = 0;
            while (argumentOffset < bytes.Length)
            {
                int argumentLength = BitConverter.ToInt32(bytes.Slice(argumentOffset, sizeof(int)), 0);
                argumentOffset += sizeof(int);

                result.Add(deserialize(bytes.Slice(argumentOffset, argumentLength), argumentTypes[typeIndex++]));
                argumentOffset += argumentLength;
            }

            return result.ToArray();
        }

        public static byte[] GetResponseValue(object? value, Type? type, Serialize serialize)
        {
            List<byte[]> result = new()
            {
                ResHeaderBytes,
                Version,
                ResponseValueFlag,
            };

            if (value is not null && type is not null)
            {
                byte[] valueBytes = serialize(value, type);
                result.Add(BitConverter.GetBytes(valueBytes.Length));
                result.Add(valueBytes);
            }

            return result.ConcatBytes();
        }

        public static byte[] GetResponseError(Exception exception, Serialize serialize)
        {
            byte[] typeBytes = serialize(exception.GetType(), typeof(Type));
            byte[] exceptionBytes = serialize(exception, exception.GetType());
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

        public static bool GetResponse(byte[] bytes, Type type, Deserialize deserialize, out object? value, out Exception? exception)
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
                int valueLength = BitConverter.ToInt32(bytes.Slice(offset, sizeof(int)), 0);
                offset += sizeof(int);
                value = deserialize(bytes.Slice(offset, valueLength), type);
                exception = null;
            }
            else
            {
                int errorTypeLength = BitConverter.ToInt32(bytes.Slice(offset, sizeof(int)), 0);
                offset += sizeof(int);

                Type errorType = (Type)deserialize(bytes.Slice(offset, errorTypeLength), typeof(Type))!;
                offset += errorTypeLength;

                int errorLength = BitConverter.ToInt32(bytes.Slice(offset, sizeof(int)), 0);
                offset += sizeof(int);

                exception = (Exception?)deserialize(bytes.Slice(offset, errorLength), errorType);
                value = null;
            }

            return hasValue;
        }

        private static void PreprocessRequestBytes(byte[] bytes, out int requestOffset, out int requestLength)
        {
            int offset = 0;
            if (!bytes.Slice(offset, ReqHeaderBytes.Length).SequenceEqual(ReqHeaderBytes))
            {
                throw new ArgumentException("The bytes is not valid request data.", nameof(bytes));
            }

            // Skip the version number, because the current version is the first one
            // and there is no need to consider compatibility issues.
            offset += ReqHeaderBytes.Length + Version.Length;
            requestLength = BitConverter.ToInt32(bytes.Slice(offset, sizeof(int)), 0);
            offset += sizeof(int);
            requestOffset = offset;
        }
    }
}
