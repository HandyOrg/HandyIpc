using System.Collections.Generic;

namespace HandyIpc.Server
{
    public class Context
    {
        public byte[] Input { get; }

        public byte[] Output { get; set; }

        public IDictionary<object, object> Items { get; } = new Dictionary<object, object>();

        public Context(byte[] input) => Input = input;
    }
}
