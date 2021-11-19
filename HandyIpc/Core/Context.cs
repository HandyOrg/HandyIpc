using System;
using HandyIpc.Logger;

namespace HandyIpc.Core
{
    public class Context
    {
        private static readonly byte[] EmptyBytes = Array.Empty<byte>();

        // FIXME: Replace 'set;' with 'init;'
        public byte[] Input { get; set; } = null!;

        public byte[] Output { get; set; } = EmptyBytes;

        public Request? Request { get; set; }

        public ISerializer Serializer { get; set; } = null!;

        public ILogger Logger { get; set; } = null!;

        public IConnection Connection { get; set; } = null!;

        public bool KeepAlive { get; set; } = true;
    }
}
