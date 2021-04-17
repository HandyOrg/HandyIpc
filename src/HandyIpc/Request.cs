using System;
using System.Reflection;
using Newtonsoft.Json;
#pragma warning disable 8618

namespace HandyIpc
{
    [Obfuscation(Exclude = true)]
    public class Request
    {
        /// <summary>
        /// Gets the access token, which may be empty.
        /// </summary>
        [JsonProperty("token")]
        public string AccessToken { get; set; }

        /// <summary>
        /// Gets the name of the method.
        /// </summary>
        [JsonProperty("method")]
        public string MethodName { get; set; }

        /// <summary>
        /// Gets the arguments on the method.
        /// </summary>
        [JsonProperty("args")]
        public object?[] Arguments { get; set; }

        /// <summary>
        /// Gets types of arguments on the method, which may be null.
        /// </summary>
        /// <remarks>
        /// The property has been filled only if the method is a generic method.
        /// </remarks>
        [JsonProperty("argTypes")]
        public Type[] ArgumentTypes { get; set; }

        /// <summary>
        /// Gets the generic argument that are defined on the interface, which may be null.
        /// </summary>
        [JsonProperty("iGArgs")]
        public Type[]? GenericArguments { get; set; }

        /// <summary>
        /// Gets the generic arguments that are defined on the method, which may be null.
        /// </summary>
        [JsonProperty("mGArgs")]
        public Type[]? MethodGenericArguments { get; set; }
    }
}
