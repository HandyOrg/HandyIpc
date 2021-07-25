using System.Collections.Generic;

namespace HandyIpc.Server
{
    public class Context
    {
        private static readonly byte[] EmptyBytes = new byte[0];

        public byte[] Input { get; }

        public byte[] Output { get; set; } = EmptyBytes;

        public IDictionary<object, object> Items { get; } = new Dictionary<object, object>();

        public RequestHeader? RequestHeader { get; set; }

        public ISerializer Serializer { get; }

        public ILogger Logger { get; }

        public Context(byte[] input, ISerializer serializer, ILogger logger)
        {
            Input = input;
            Serializer = serializer;
            Logger = logger;
        }
    }
}
