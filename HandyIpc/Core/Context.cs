using System;
using System.Collections.Generic;

namespace HandyIpc.Core
{
    public class Context
    {
        private static readonly byte[] EmptyBytes = Array.Empty<byte>();

        public byte[] Input { get; }

        public byte[] Output { get; set; } = EmptyBytes;

        public IDictionary<object, object> Items { get; } = new Dictionary<object, object>();

        public Request? Request { get; set; }

        public ISerializer Serializer { get; set; } = null!;

        public ILogger Logger { get; set; } = null!;

        public IConnection Connection { get; set; } = null!;

        public Context(byte[] input)
        {
            Input = input;
        }
    }
}
