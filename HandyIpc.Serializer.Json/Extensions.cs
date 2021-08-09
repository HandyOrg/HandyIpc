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

        public static IHubBuilder UseJsonSerializer(this IHubBuilder self)
        {
            return self.Use(() => new JsonSerializer());
        }

        internal static byte[] ToJson(this object @object, Type type)
        {
            string jsonText = JsonConvert.SerializeObject(@object, type, Settings);
            return Encoding.UTF8.GetBytes(jsonText);
        }

        internal static object? ToObject(this byte[] bytes, Type type)
        {
            if (bytes.Length == 0)
            {
                return null;
            }

            string jsonText = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject(jsonText, type, Settings);
        }
    }
}
