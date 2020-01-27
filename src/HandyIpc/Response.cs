using System;
using System.Reflection;
using HandyIpc.Extensions;
using Newtonsoft.Json;

namespace HandyIpc
{
    [Obfuscation(Exclude = true)]
    public class Response
    {
        public static byte[] ReturnValue(object value) => new Response { Value = value }.ToBytes();

        public static byte[] ReturnException(Exception exception) => new Response { Exception = exception }.ToBytes();

        [JsonProperty("val")]
        public object Value { get; set; }

        [JsonProperty("e")]
        public Exception Exception { get; set; }
    }
}
