using System;
using System.Reflection;
using Newtonsoft.Json;

namespace HandyIpc
{
    [Obfuscation(Exclude = true)]
    public class Request
    {
        [JsonProperty("token")]
        public string AccessToken { get; set; }

        [JsonProperty("genericArgs")]
        public Type[] GenericArguments { get; set; }

        [JsonProperty("method")]
        public string MethodName { get; set; }

        [JsonProperty("args")]
        public object[] Arguments { get; set; }
    }
}
