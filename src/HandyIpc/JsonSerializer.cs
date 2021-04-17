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

        public byte[] SerializeRequest(Request request)
        {
            string jsonText = JsonConvert.SerializeObject(request, Settings);
            return Encoding.UTF8.GetBytes(jsonText);
        }

        public byte[] SerializeResponse(Response response)
        {
            string jsonText = JsonConvert.SerializeObject(response, Settings);
            return Encoding.UTF8.GetBytes(jsonText);
        }

        public Request? DeserializeRequest(byte[] bytes)
        {
            string jsonText = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject<Request>(jsonText, Settings);
        }

        public Response? DeserializeResponse(byte[] bytes)
        {
            string jsonText = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject<Response>(jsonText, Settings);
        }
    }
}
