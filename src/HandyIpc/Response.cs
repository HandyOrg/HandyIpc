using System;
using System.Reflection;
using Newtonsoft.Json;

namespace HandyIpc
{
    [Obfuscation(Exclude = true)]
    public class Response
    {
        [JsonProperty("val")]
        public object? Value { get; set; }

        [JsonProperty("e")]
        public Exception? Exception { get; set; }
    }
}
