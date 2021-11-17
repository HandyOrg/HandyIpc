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
        private static readonly IReadOnlyList<Type> EmptyTypeList = Enumerable.Empty<Type>().ToList().AsReadOnly();
        private static readonly IReadOnlyList<object?> EmptyObjectList = Enumerable.Empty<object?>().ToList().AsReadOnly();

        private readonly ISerializer _serializer;
        private readonly byte[] _bytes;

        private string? _name;
        private IReadOnlyList<Type>? _typeArguments;
        private string? _methodName;
        private IReadOnlyList<Type>? _methodTypeArguments;
        private IReadOnlyList<object?>? _arguments;
        private IReadOnlyList<Type> _argumentTypes = null!;

        private (int start, int length) _nameRange;
        private (int start, int length) _typeArgumentsRange;
        private (int start, int length) _methodNameRange;
        private (int start, int length) _methodTypeArgumentsRange;
        private (int start, int length) _argumentsRange;

        /// <summary>
        /// Gets and sets the name of the interface.
        /// </summary>
        public string Name
        {
            get => _name ??= _serializer.Deserialize<string>(_bytes.Slice(_nameRange));
            set => _name = value;
        }

        /// <summary>
        /// Gets the generic argument that are defined on the interface.
        /// </summary>
        public IReadOnlyList<Type> TypeArguments
        {
            get => _typeArguments ??= _serializer.DeserializeArray<Type>(_bytes.Slice(_typeArgumentsRange));
            set => _typeArguments = value;
        }

        /// <summary>
        /// Gets the name of the method.
        /// </summary>
        public string MethodName
        {
            get => _methodName ??= _serializer.Deserialize<string>(_bytes.Slice(_methodNameRange));
            set => _methodName = value;
        }

        /// <summary>
        /// Gets the generic arguments that are defined on the method.
        /// </summary>
        public IReadOnlyList<Type> MethodTypeArguments
        {
            get => _methodTypeArguments ??= _serializer.DeserializeArray<Type>(_bytes.Slice(_methodTypeArgumentsRange));
            set => _methodTypeArguments = value;
        }

        /// <summary>
        /// Gets arguments of the method.
        /// </summary>
        public IReadOnlyList<object?> Arguments
        {
            get => _arguments ??= _serializer.DeserializeArray<object?>(_bytes.Slice(_argumentsRange), _argumentTypes);
            set => _arguments = value;
        }

        public Request(ISerializer serializer, string methodName)
        {
            _serializer = serializer;
            _methodName = methodName;
            _typeArguments = EmptyTypeList;
            _methodTypeArguments = EmptyTypeList;
            _arguments = EmptyObjectList;

            _bytes = null!;
        }

        private Request(ISerializer serializer, byte[] bytes)
        {
            _serializer = serializer;
            _bytes = bytes;
        }

        public void SetArgumentTypes(IReadOnlyList<Type> argumentTypes) => _argumentTypes = argumentTypes;

        public byte[] ToBytes()
        {
            /*
             * < Header                    >
             * | Req Token                 |
             * | Version                   |
             * < Layout Table              >
             * | NameLength                |
             * | TypeArgumentsLength       |
             * | MethodNameLength          |
             * | MethodTypeArgumentsLength |
             * | ArgumentsLength           |
             * < Body                      >
             * | Name                      |
             * | TypeArguments             |
             * | MethodName                |
             * | MethodTypeArguments       |
             * | Arguments                 |
             */

            byte[] nameBytes = _serializer.Serialize(Name, typeof(string));
            byte[] typeArgumentsBytes = _serializer.SerializeArray(TypeArguments);
            byte[] methodNameBytes = _serializer.Serialize(MethodName, typeof(string));
            byte[] methodTypeArgumentsBytes = _serializer.SerializeArray(MethodTypeArguments);
            byte[] argumentsBytes = _serializer.SerializeArray(Arguments, _argumentTypes);

            byte[][] bytesList =
            {
                ReqHeaderBytes,
                Version,
                BitConverter.GetBytes(nameBytes.Length),
                BitConverter.GetBytes(typeArgumentsBytes.Length),
                BitConverter.GetBytes(methodNameBytes.Length),
                BitConverter.GetBytes(methodTypeArgumentsBytes.Length),
                BitConverter.GetBytes(argumentsBytes.Length),
                nameBytes,
                typeArgumentsBytes,
                methodNameBytes,
                methodTypeArgumentsBytes,
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

            request = new Request(serializer, bytes)
            {
                _nameRange = GetRangeAndMoveNext(bytes, ref offset, ref start),
                _typeArgumentsRange = GetRangeAndMoveNext(bytes, ref offset, ref start),
                _methodNameRange = GetRangeAndMoveNext(bytes, ref offset, ref start),
                _methodTypeArgumentsRange = GetRangeAndMoveNext(bytes, ref offset, ref start),
                _argumentsRange = GetRangeAndMoveNext(bytes, ref offset, ref start)
            };

            return true;
        }

        private static (int start, int end) GetRangeAndMoveNext(byte[] bytes, ref int offset, ref int start)
        {
            int dataLength = BitConverter.ToInt32(bytes, offset);
            offset += sizeof(int);
            (int start, int end) range = (start, dataLength);
            start += dataLength;

            return range;
        }
    }
}
