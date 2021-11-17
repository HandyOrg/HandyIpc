using System;
using System.Collections.Generic;

namespace HandyIpc.Core
{
    public static class SerializerExtensions
    {
        private static readonly byte[] EmptyArray = BitConverter.GetBytes(0);

        public static byte[] Serialize<T>(this ISerializer serializer, T value)
        {
            return serializer.Serialize(value, typeof(T));
        }

        public static byte[] SerializeArray<T>(this ISerializer serializer, IReadOnlyList<T> array, IReadOnlyList<Type>? types = null)
        {
            if (array.Count == 0)
            {
                return EmptyArray;
            }

            List<byte[]> result = new();
            for (int i = 0; i < array.Count; i++)
            {
                byte[] bytes = serializer.Serialize(array[i], types == null ? typeof(T) : types[i]);
                result.Add(BitConverter.GetBytes(bytes.Length));
                result.Add(bytes);
            }

            return result.ConcatBytes();
        }

        public static T Deserialize<T>(this ISerializer serializer, byte[] bytes)
        {
            return (T)serializer.Deserialize(bytes, typeof(T))!;
        }

        public static IReadOnlyList<T> DeserializeArray<T>(this ISerializer serializer, byte[] bytes, IReadOnlyList<Type>? types = null)
        {
            List<T> result = new();

            for (int offset = 0, i = 0; offset < bytes.Length; i++)
            {
                int dataLength = BitConverter.ToInt32(bytes, offset);
                offset += sizeof(int);

                T data = (T)serializer.Deserialize(bytes.Slice(offset, dataLength), types == null ? typeof(T) : types[i])!;
                offset += dataLength;

                result.Add(data);
            }

            return result;
        }
    }
}
