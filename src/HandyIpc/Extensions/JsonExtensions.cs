using System.Text;
using Newtonsoft.Json;

namespace HandyIpc.Extensions
{
    internal static class JsonExtensions
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            TypeNameHandling = TypeNameHandling.Auto
        };

        public static byte[] ToBytes(this object @object)
        {
            var jsonText = JsonConvert.SerializeObject(@object, Settings);
            return Encoding.UTF8.GetBytes(jsonText);
        }

        public static T? ToObject<T>(this byte[] bytes)
        {
            var jsonText = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject<T>(jsonText, Settings);
        }
    }
}
