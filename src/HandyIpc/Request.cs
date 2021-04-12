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

        [JsonProperty("iGArgs")]
        public Type[] GenericArguments { get; set; }

        [JsonProperty("method")]
        public string MethodName { get; set; }

        [JsonProperty("args")]
        public object[] Arguments { get; set; }

        [JsonProperty("argTypes")]
        // The property has been filled only if the method is a generic method.
        public Type[] ArgumentTypes { get; set; }

        /// <summary>
        /// Generic arguments that are defined on the method.
        /// </summary>
        [JsonProperty("mGArgs")]
        public Type[] MethodGenericArguments { get; set; }
    }
}
