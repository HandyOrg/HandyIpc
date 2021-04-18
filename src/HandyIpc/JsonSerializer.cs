using System;
using System.Text;
using Newtonsoft.Json;

namespace HandyIpc
{
    public class JsonSerializer : ISerializer
    {
        private static readonly JsonSerializerSettings Settings = new()
        {
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            TypeNameHandling = TypeNameHandling.Auto,
        };

        public byte[] SerializeRequest(Request request) => Serialize(request);

        public byte[] SerializeResponse(Response response) => Serialize(response);

        public Request DeserializeRequest(byte[] bytes)
        {
            Request request = Deserialize<Request>(bytes)!;
            for (int i = 0; i < request.Arguments.Length; i++)
            {
                object? value = request.Arguments[i];
                Type type = request.ArgumentTypes[i];
                // Because the Newtonsoft.Json convert int to long, which will cause an exception,
                // cast the value (long) by the specified type (System.Int32) to avoid the exception here.
                request.Arguments[i] = CastValueByType(value, type);
            }

            return request;
        }

        public Response DeserializeResponse(byte[] bytes) => Deserialize<Response>(bytes)!;

        private static byte[] Serialize(object @object)
        {
            string jsonText = JsonConvert.SerializeObject(@object, Settings);
            return Encoding.UTF8.GetBytes(jsonText);
        }

        private static T? Deserialize<T>(byte[] bytes)
        {
            string jsonText = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject<T>(jsonText, Settings);
        } 

        private static object? CastValueByType(object? value, Type targetType)
        {
            return value != null && targetType.IsValueType
                ? Convert.ChangeType(value, targetType)
                : targetType.IsInstanceOfType(value)
                    ? value
                    : default;
        }
    }
}
