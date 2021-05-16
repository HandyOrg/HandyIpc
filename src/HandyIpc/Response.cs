using System;
using System.Reflection;

namespace HandyIpc
{
    [Obfuscation(Exclude = true)]
    public class Response
    {
        public object? Value { get; set; }

        public Exception? Exception { get; set; }
    }
}
