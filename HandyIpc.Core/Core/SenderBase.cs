using System.Threading.Tasks;

namespace HandyIpc.Core
{
    public abstract class SenderBase
    {
        protected ILogger Logger { get; private set; } = null!;

        internal void SetLogger(ILogger logger) => Logger = logger;

        public abstract byte[] Invoke(byte[] requestBytes);

        public abstract Task<byte[]> InvokeAsync(byte[] requestBytes);
    }
}
