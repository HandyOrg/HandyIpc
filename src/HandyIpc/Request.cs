using System;
using System.Reflection;
using Newtonsoft.Json;

namespace HandyIpc
{
    [Obfuscation(Exclude = true)]
    public class Request
    {
        /// <summary>
        /// Gets the access token, which may be empty.
        /// </summary>
        [JsonProperty("token")]
        public string AccessToken { get; }

        /// <summary>
        /// Gets the generic argument that are defined on the interface, which may be null.
        /// </summary>
        [JsonProperty("iGArgs")]
        public Type[]? GenericArguments { get; }

        /// <summary>
        /// Gets the name of the method.
        /// </summary>
        [JsonProperty("method")]
        public string MethodName { get; }

        /// <summary>
        /// Gets the arguments on the method.
        /// </summary>
        [JsonProperty("args")]
        public object[] Arguments { get; }

        /// <summary>
        /// Gets types of arguments on the method, which may be null.
        /// </summary>
        /// <remarks>
        /// The property has been filled only if the method is a generic method.
        /// </remarks>
        [JsonProperty("argTypes")]
        public Type[] ArgumentTypes { get; }

        /// <summary>
        /// Gets the generic arguments that are defined on the method, which may be null.
        /// </summary>
        [JsonProperty("mGArgs")]
        public Type[]? MethodGenericArguments { get; }

        public Request(
            string accessToken,
            string methodName,
            object[] arguments,
            Type[] argumentTypes,
            Type[]? genericArguments,
            Type[]? methodGenericArguments)
        {
            AccessToken = accessToken;
            MethodName = methodName;
            Arguments = arguments;
            ArgumentTypes = argumentTypes;
            GenericArguments = genericArguments;
            MethodGenericArguments = methodGenericArguments;
        }
    }
}
