using System;

namespace HandyIpc.Serializer.Json
{
    public class JsonSerializer : ISerializer
    {
        private static readonly byte[] EmptyBytes = Array.Empty<byte>();

        public byte[] Serialize(object? value, Type type)
        {
            return value switch
            {
                byte v => new[] { v },
                byte[] v => v,
                short v => BitConverter.GetBytes(v),
                int v => BitConverter.GetBytes(v),
                long v => BitConverter.GetBytes(v),
                ushort v => BitConverter.GetBytes(v),
                uint v => BitConverter.GetBytes(v),
                ulong v => BitConverter.GetBytes(v),
                float v => BitConverter.GetBytes(v),
                double v => BitConverter.GetBytes(v),
                char v => BitConverter.GetBytes(v),
                null => EmptyBytes,
                _ => value.ToJson(type),
            };
        }

        public object? Deserialize(byte[] bytes, Type type)
        {
            if (type == typeof(byte))
            {
                return bytes[0];
            }

            if (type == typeof(byte[]))
            {
                return bytes;
            }

            if (type == typeof(short))
            {
                return BitConverter.ToInt16(bytes, 0);
            }

            if (type == typeof(int))
            {
                return BitConverter.ToInt32(bytes, 0);
            }

            if (type == typeof(long))
            {
                return BitConverter.ToInt64(bytes, 0);
            }

            if (type == typeof(ushort))
            {
                return BitConverter.ToUInt16(bytes, 0);
            }

            if (type == typeof(uint))
            {
                return BitConverter.ToUInt32(bytes, 0);
            }

            if (type == typeof(ulong))
            {
                return BitConverter.ToUInt64(bytes, 0);
            }

            if (type == typeof(float))
            {
                return BitConverter.ToSingle(bytes, 0);
            }

            if (type == typeof(double))
            {
                return BitConverter.ToDouble(bytes, 0);
            }

            if (type == typeof(char))
            {
                return BitConverter.ToChar(bytes, 0);
            }

            return bytes.ToObject(type);
        }
    }
}
