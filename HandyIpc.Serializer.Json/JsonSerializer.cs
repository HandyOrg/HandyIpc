using System;
using System.Collections.Generic;
using System.Text;

namespace HandyIpc.Serializer.Json
{
    public class JsonSerializer : ISerializer
    {
        private static readonly byte[] EmptyBytes = Array.Empty<byte>();
        private static readonly IReadOnlyDictionary<Type, Func<object, byte[]>> BuildInTypeSerializerMap = new Dictionary<Type, Func<object, byte[]>>
        {
            [typeof(byte)] = value => new[] { (byte)value },
            [typeof(byte[])] = value => (byte[])value,
            [typeof(short)] = value => BitConverter.GetBytes((short)value),
            [typeof(int)] = value => BitConverter.GetBytes((int)value),
            [typeof(long)] = value => BitConverter.GetBytes((long)value),
            [typeof(ushort)] = value => BitConverter.GetBytes((ushort)value),
            [typeof(uint)] = value => BitConverter.GetBytes((uint)value),
            [typeof(ulong)] = value => BitConverter.GetBytes((ulong)value),
            [typeof(float)] = value => BitConverter.GetBytes((float)value),
            [typeof(double)] = value => BitConverter.GetBytes((double)value),
            [typeof(char)] = value => BitConverter.GetBytes((char)value),
            [typeof(string)] = value => Encoding.UTF8.GetBytes((string)value),
            //[typeof(bool)] = value => BitConverter.GetBytes((bool)value),
        };
        private static readonly IReadOnlyDictionary<Type, Func<byte[], object?>> BuildInTypeDeserializerMap = new Dictionary<Type, Func<byte[], object?>>
        {
            [typeof(byte)] = bytes => bytes[0],
            [typeof(byte[])] = bytes => bytes,
            [typeof(short)] = bytes => BitConverter.ToInt16(bytes, 0),
            [typeof(int)] = bytes => BitConverter.ToInt32(bytes, 0),
            [typeof(long)] = bytes => BitConverter.ToInt64(bytes, 0),
            [typeof(ushort)] = bytes => BitConverter.ToUInt16(bytes, 0),
            [typeof(uint)] = bytes => BitConverter.ToUInt32(bytes, 0),
            [typeof(ulong)] = bytes => BitConverter.ToUInt64(bytes, 0),
            [typeof(float)] = bytes => BitConverter.ToSingle(bytes, 0),
            [typeof(double)] = bytes => BitConverter.ToDouble(bytes, 0),
            [typeof(char)] = bytes => BitConverter.ToChar(bytes, 0),
            [typeof(string)] = bytes => Encoding.UTF8.GetString(bytes),
            //[typeof(bool)] = bytes => BitConverter.ToBoolean(bytes, 0),
        };

        public byte[] Serialize(object? value, Type type)
        {
            return value is null
                ? EmptyBytes
                : BuildInTypeSerializerMap.TryGetValue(type, out var serialize)
                    ? serialize(value)
                    : value.ToJson(type);
        }

        public object? Deserialize(byte[] bytes, Type type)
        {
            return BuildInTypeDeserializerMap.TryGetValue(type, out var deserialize)
                ? deserialize(bytes)
                : bytes.ToObject(type);
        }
    }
}
