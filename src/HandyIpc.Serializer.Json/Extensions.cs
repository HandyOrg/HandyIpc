using System;
using System.Text;
using Newtonsoft.Json;

namespace HandyIpc.Serializer.Json
{
    public static class Extensions
    {
        private static readonly JsonSerializerSettings Settings = new()
        {
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            TypeNameHandling = TypeNameHandling.Auto,
        };

        public static IIpcFactory<TRmi, THub> UseJsonSerializer<TRmi, THub>(this IIpcFactory<TRmi, THub> self)
        {
            return self.Use(() => new JsonSerializer());
        }

        internal static byte[] Slice(this byte[] bytes, int start, int length)
        {
            byte[] result = new byte[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = bytes[start + i];
            }

            return result;
        }

        internal static byte[] ToJson(this object @object)
        {
            string jsonText = JsonConvert.SerializeObject(@object, Settings);
            return Encoding.UTF8.GetBytes(jsonText);
        }

        internal static T ToObject<T>(this byte[] bytes)
        {
            string jsonText = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject<T>(jsonText, Settings);
        }

        internal static object? CastTo(this object? value, Type targetType)
        {
            return value is not null && targetType.IsValueType
                ? Convert.ChangeType(value, targetType)
                : targetType.IsInstanceOfType(value)
                    ? value
                    : default;
        }

        internal static int ToInt32(this byte[] bytes)
        {
            return BitConverter.ToInt32(bytes, 0);
        }
    }
}
