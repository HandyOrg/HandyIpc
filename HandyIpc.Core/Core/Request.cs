using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace HandyIpc.Core
{
    /// <summary>
    /// Represents a request for a call to a remote method.
    /// </summary>
    [Obfuscation(Exclude = true)]
    public class Request
    {
        private const string ReqHeader = "handyipc/req";

        private static readonly byte[] Version = { 1 };
        private static readonly byte[] ReqHeaderBytes = Encoding.ASCII.GetBytes(ReqHeader);
        private static readonly byte[] EmptyArray = BitConverter.GetBytes(0);
        private static readonly IReadOnlyList<Type> EmptyTypeList = Enumerable.Empty<Type>().ToList().AsReadOnly();
        private static readonly IReadOnlyList<object?> EmptyObjectList = Enumerable.Empty<object?>().ToList().AsReadOnly();

        private readonly ISerializer _serializer;
        private readonly byte[] _bytes;

        private string? _accessToken;
        private IReadOnlyList<Type>? _typeArguments;
        private string? _methodName;
        private IReadOnlyList<Type>? _methodTypeArguments;
        private IReadOnlyList<Type>? _argumentTypes;
        private IReadOnlyList<object?>? _arguments;

        private (int start, int length) _accessTokenRange;
        private (int start, int length) _typeArgumentsRange;
        private (int start, int length) _methodNameRange;
        private (int start, int length) _methodTypeArgumentsRange;
        private (int start, int length) _argumentTypesRange;
        private (int start, int length) _argumentsRange;

        /// <summary>
        /// Gets the access token, which may be empty string.
        /// </summary>
        public string AccessToken => _accessToken ??= Deserialize<string>(_accessTokenRange);

        /// <summary>
        /// Gets the generic argument that are defined on the interface.
        /// </summary>
        public IReadOnlyList<Type> TypeArguments => _typeArguments ??= DeserializeArray<Type>(_typeArgumentsRange);

        /// <summary>
        /// Gets the name of the method.
        /// </summary>
        public string MethodName => _methodName ??= Deserialize<string>(_methodNameRange);

        /// <summary>
        /// Gets the generic arguments that are defined on the method.
        /// </summary>
        public IReadOnlyList<Type> MethodTypeArguments => _methodTypeArguments ??= DeserializeArray<Type>(_methodTypeArgumentsRange);

        /// <summary>
        /// Gets types of arguments on the method.
        /// </summary>
        /// <remarks>
        /// The property has been filled only if the method is a generic method.
        /// </remarks>
        public IReadOnlyList<Type> ArgumentTypes => _argumentTypes ??= DeserializeArray<Type>(_argumentTypesRange);

        /// <summary>
        /// Gets arguments of the method.
        /// </summary>
        public IReadOnlyList<object?> Arguments => _arguments ??= DeserializeArray<object?>(_argumentsRange);

        public Request(
            ISerializer serializer,
            string methodName,
            string? accessToken = null,
            IReadOnlyList<Type>? typeArguments = null,
            IReadOnlyList<Type>? methodTypeArguments = null,
            IReadOnlyList<Type>? argumentTypes = null,
            IReadOnlyList<object?>? arguments = null)
        {
            // FIXME: Replace these initialization with the 'Property { get; init; }' syntax.
            _serializer = serializer;
            _methodName = methodName;
            _accessToken = accessToken ?? string.Empty;
            _typeArguments = typeArguments ?? EmptyTypeList;
            _methodTypeArguments = methodTypeArguments ?? EmptyTypeList;
            _argumentTypes = argumentTypes ?? EmptyTypeList;
            _arguments = arguments ?? EmptyObjectList;

            _bytes = null!;
        }

        public Request(ISerializer serializer, byte[] bytes)
        {
            _serializer = serializer;
            _bytes = bytes;
        }

        public byte[] ToBytes()
        {
            /*
             * < Header                    >
             * | Req Token                 |
             * | Version                   |
             * < Layout Table              >
             * | AccessTokenLength         |
             * | TypeArgumentsLength       |
             * | MethodNameLength          |
             * | MethodTypeArgumentsLength |
             * | ArgumentTypesLength       |
             * | ArgumentsLength           |
             * < Body                      >
             * | AccessToken               |
             * | TypeArguments             |
             * | MethodName                |
             * | MethodTypeArguments       |
             * | ArgumentTypes             |
             * | Arguments                 |
             */

            byte[] accessTokenBytes = _serializer.Serialize(AccessToken, typeof(string));
            byte[] typeArgumentsBytes = SerializeArray(TypeArguments);
            byte[] methodNameBytes = _serializer.Serialize(MethodName, typeof(string));
            byte[] methodTypeArgumentsBytes = SerializeArray(MethodTypeArguments);
            byte[] argumentTypesBytes = SerializeArray(ArgumentTypes);
            byte[] argumentsBytes = SerializeArray(Arguments);

            byte[][] bytesList =
            {
                ReqHeaderBytes,
                Version,
                BitConverter.GetBytes(accessTokenBytes.Length),
                BitConverter.GetBytes(typeArgumentsBytes.Length),
                BitConverter.GetBytes(methodNameBytes.Length),
                BitConverter.GetBytes(methodTypeArgumentsBytes.Length),
                BitConverter.GetBytes(argumentTypesBytes.Length),
                BitConverter.GetBytes(argumentsBytes.Length),
                accessTokenBytes,
                typeArgumentsBytes,
                methodNameBytes,
                methodTypeArgumentsBytes,
                argumentTypesBytes,
                argumentsBytes,
            };

            return bytesList.ConcatBytes();
        }

        public static bool TryParse(byte[] bytes, ISerializer serializer, out Request request)
        {
            if (!ReqHeaderBytes.EqualsHeaderBytes(bytes))
            {
                request = null!;
                return false;
            }

            // Skip header and version bytes.
            int offset = ReqHeaderBytes.Length + 1;
            // Skip layout table, 6 is six field in bytes table.
            int start = offset + sizeof(int) * 6;

            request = new Request(serializer, bytes);

            int dataLength = BitConverter.ToInt32(bytes, offset);
            offset += sizeof(int);
            request._accessTokenRange = (start, dataLength);
            start += dataLength;

            dataLength = BitConverter.ToInt32(bytes, offset);
            offset += sizeof(int);
            request._typeArgumentsRange = (start, dataLength);
            start += dataLength;

            dataLength = BitConverter.ToInt32(bytes, offset);
            offset += sizeof(int);
            request._methodNameRange = (start, dataLength);
            start += dataLength;

            dataLength = BitConverter.ToInt32(bytes, offset);
            offset += sizeof(int);
            request._methodTypeArgumentsRange = (start, dataLength);
            start += dataLength;

            dataLength = BitConverter.ToInt32(bytes, offset);
            offset += sizeof(int);
            request._argumentTypesRange = (start, dataLength);
            start += dataLength;

            dataLength = BitConverter.ToInt32(bytes, offset);
            request._argumentsRange = (start, dataLength);

            return true;
        }

        private byte[] SerializeArray<T>(IReadOnlyList<T> array)
        {
            if (array.Count == 0)
            {
                return EmptyArray;
            }

            List<byte[]> result = new();
            for (int i = 0; i < array.Count; i++)
            {
                byte[] bytes = _serializer.Serialize(array[i], typeof(T));
                result.Add(BitConverter.GetBytes(bytes.Length));
                result.Add(bytes);
            }

            return result.ConcatBytes();
        }

        private T Deserialize<T>((int start, int length) range)
        {
            (int start, int length) = range;
            return (T)_serializer.Deserialize(_bytes.Slice(start, length), typeof(T))!;
        }

        private IReadOnlyList<T> DeserializeArray<T>((int start, int length) range)
        {
            (int start, int length) = range;
            int end = start + length;
            List<T> result = new();
            int offset = 0;
            while (offset < end)
            {
                int dataLength = BitConverter.ToInt32(_bytes, start + offset);
                offset += sizeof(int);

                T data = (T)_serializer.Deserialize(_bytes.Slice(offset, dataLength), typeof(T))!;
                offset += dataLength;

                result.Add(data);
            }

            return result;
        }
    }
}
