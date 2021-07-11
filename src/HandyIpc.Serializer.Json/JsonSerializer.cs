using System;
using System.Collections.Generic;
using System.Text;

namespace HandyIpc.Serializer.Json
{
    public class JsonSerializer : ISerializer
    {
        private const string HeaderFlag = "handyipc/json/v1";

        public byte[] SerializeRequest(Request request, object?[]? arguments)
        {
            byte[] headerBytes = Encoding.ASCII.GetBytes(HeaderFlag);
            byte headerLengthByte = (byte)headerBytes.Length;
            byte[] requestBytes = request.ToJson();
            byte[] requestLengthBytes = BitConverter.GetBytes(requestBytes.Length);
            byte[] argsBytes = arguments is null ? new byte[0] : arguments.ToJson();
            byte[] argsLengthBytes = BitConverter.GetBytes(argsBytes.Length);

            byte[] result = new byte[
                headerBytes.Length +
                1 + // headerLengthByte.Length
                requestBytes.Length +
                requestLengthBytes.Length +
                argsBytes.Length +
                argsLengthBytes.Length];

            /*
             * | Header Length | Header | Request Length | Arguments Length | Request | Arguments |
             * | 1 byte        | -      | 4 bytes        | 4 bytes          | -       | -         |
             */
            result[0] = headerLengthByte;
            int offset = 1;

            headerBytes.CopyTo(result, offset);
            offset += headerBytes.Length;

            requestLengthBytes.CopyTo(result, offset);
            offset += requestLengthBytes.Length;

            argsLengthBytes.CopyTo(result, offset);
            offset += argsLengthBytes.Length;

            requestBytes.CopyTo(result, offset);
            offset += requestBytes.Length;

            argsBytes.CopyTo(result, offset);

            return result;
        }

        public byte[] SerializeResponse(Response response) => response.ToJson();

        public Request DeserializeRequest(byte[] bytes)
        {
            CheckBytes(bytes, out int requestOffset, out int requestLength, out _);

            return bytes.Slice(requestOffset, requestLength).ToObject<Request>();
        }

        public object?[]? DeserializeArguments(byte[] bytes, IReadOnlyList<Type> types)
        {
            CheckBytes(bytes, out int requestOffset, out int requestLength, out int argumentsLength);

            if (argumentsLength == 0)
            {
                return null;
            }

            int argumentsOffset = requestOffset + requestLength;
            object?[] arguments = bytes.Slice(argumentsOffset, argumentsLength).ToObject<object?[]>();
            for (int i = 0; i < arguments.Length; i++)
            {
                arguments[0] = arguments[0].CastTo(types[0]);
            }

            return arguments;
        }

        public Response DeserializeResponse(byte[] bytes) => bytes.ToObject<Response>()!;

        private static void CheckBytes(byte[] bytes, out int requestOffset, out int requestLength, out int argumentsLength)
        {
            int headerLength = bytes[0];
            int offset = 1;

            byte[] headerBytes = bytes.Slice(offset, headerLength);
            offset += headerBytes.Length;
            string header = Encoding.ASCII.GetString(headerBytes);

            if (!string.Equals(header, HeaderFlag))
            {
                throw new ArgumentException("The bytes is not a valid 'handyipc/json/v*' data.", nameof(bytes));
            }

            requestLength = bytes.Slice(offset, sizeof(int)).ToInt32();
            offset += sizeof(int);

            argumentsLength = bytes.Slice(offset, sizeof(int)).ToInt32();
            offset += sizeof(int);

            requestOffset = offset;
        }
    }
}
