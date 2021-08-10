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

        private IReadOnlyList<Type>? _typeArguments;
        private string? _methodName;
        private IReadOnlyList<Type>? _methodTypeArguments;
        private IReadOnlyList<Type>? _argumentTypes;
        private IReadOnlyList<object?>? _arguments;

        private (int start, int length) _typeArgumentsRange;
        private (int start, int length) _methodNameRange;
        private (int start, int length) _methodTypeArgumentsRange;
        private (int start, int length) _argumentTypesRange;
        private (int start, int length) _argumentsRange;

        /// <summary>
        /// Gets the generic argument that are defined on the interface.
        /// </summary>
        public IReadOnlyList<Type> TypeArguments
        {
            get => _typeArguments ??= DeserializeArray<Type>(_typeArgumentsRange);
            set => _typeArguments = value;
        }

        /// <summary>
        /// Gets the name of the method.
        /// </summary>
        public string MethodName
        {
            get => _methodName ??= Deserialize<string>(_methodNameRange);
            set => _methodName = value;
        }

        /// <summary>
        /// Gets the generic arguments that are defined on the method.
        /// </summary>
        public IReadOnlyList<Type> MethodTypeArguments
        {
            get => _methodTypeArguments ??= DeserializeArray<Type>(_methodTypeArgumentsRange);
            set => _methodTypeArguments = value;
        }

        /// <summary>
        /// Gets types of arguments on the method.
        /// </summary>
        /// <remarks>
        /// The property has been filled only if the method is a generic method.
        /// </remarks>
        public IReadOnlyList<Type> ArgumentTypes
        {
            get => _argumentTypes ??= DeserializeArray<Type>(_argumentTypesRange);
            set => _argumentTypes = value;
        }

        /// <summary>
        /// Gets arguments of the method.
        /// </summary>
        public IReadOnlyList<object?> Arguments
        {
            get => _arguments ??= DeserializeArray<object?>(_argumentsRange, index => ArgumentTypes[index]);
            set => _arguments = value;
        }

        public Request(ISerializer serializer, string methodName)
        {
            _serializer = serializer;
            _methodName = methodName;
            _typeArguments = EmptyTypeList;
            _methodTypeArguments = EmptyTypeList;
            _argumentTypes = EmptyTypeList;
            _arguments = EmptyObjectList;

            _bytes = null!;
        }

        private Request(ISerializer serializer, byte[] bytes)
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
             * | TypeArgumentsLength       |
             * | MethodNameLength          |
             * | MethodTypeArgumentsLength |
             * | ArgumentTypesLength       |
             * | ArgumentsLength           |
             * < Body                      >
             * | TypeArguments             |
             * | MethodName                |
             * | MethodTypeArguments       |
             * | ArgumentTypes             |
             * | Arguments                 |
             */

            byte[] typeArgumentsBytes = SerializeArray(TypeArguments);
            byte[] methodNameBytes = _serializer.Serialize(MethodName, typeof(string));
            byte[] methodTypeArgumentsBytes = SerializeArray(MethodTypeArguments);
            // Send parameter types only if the method is a generic method,
            // because the parameter types may contain generic types that cannot be determined at compile time
            // and they need to be monomorphism at runtime.
            byte[] argumentTypesBytes = MethodTypeArguments.Any() ? SerializeArray(ArgumentTypes) : EmptyArray;
            byte[] argumentsBytes = SerializeArray(Arguments, index => ArgumentTypes[index]);

            byte[][] bytesList =
            {
                ReqHeaderBytes,
                Version,
                BitConverter.GetBytes(typeArgumentsBytes.Length),
                BitConverter.GetBytes(methodNameBytes.Length),
                BitConverter.GetBytes(methodTypeArgumentsBytes.Length),
                BitConverter.GetBytes(argumentTypesBytes.Length),
                BitConverter.GetBytes(argumentsBytes.Length),
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
            // Skip layout table, 5 is six field in bytes table.
            int start = offset + sizeof(int) * 5;

            request = new Request(serializer, bytes);

            int dataLength = BitConverter.ToInt32(bytes, offset);
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

        private byte[] SerializeArray<T>(IReadOnlyList<T> array, Func<int, Type>? typeProvider = null)
        {
            if (array.Count == 0)
            {
                return EmptyArray;
            }

            typeProvider ??= (_ => typeof(T));

            List<byte[]> result = new();
            for (int i = 0; i < array.Count; i++)
            {
                byte[] bytes = _serializer.Serialize(array[i], typeProvider(i));
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

        private IReadOnlyList<T> DeserializeArray<T>((int start, int length) range, Func<int, Type>? typeProvider = null)
        {
            typeProvider ??= (_ => typeof(T));

            (int start, int length) = range;
            int end = start + length;
            List<T> result = new();

            for (int offset = start, index = 0; offset < end; index++)
            {
                int dataLength = BitConverter.ToInt32(_bytes, offset);
                offset += sizeof(int);

                T data = (T)_serializer.Deserialize(_bytes.Slice(offset, dataLength), typeProvider(index))!;
                offset += dataLength;

                result.Add(data);
            }

            return result;
        }
    }
}
