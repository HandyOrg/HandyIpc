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

        /// <summary>
        /// Gets or sets a bool value to indicate to connection ownership has been transferred
        /// and the service loop should end without dealing with its lifecycle.
        /// (do NOT dispose the connection, as the connection ownership has been taken over by another object).
        /// </summary>
        public bool ForgetConnection { get; set; }
    }
}
