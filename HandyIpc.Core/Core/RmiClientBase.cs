using System.Threading.Tasks;

namespace HandyIpc.Core
{
    public abstract class RmiClientBase
    {
        protected ILogger Logger { get; private set; } = null!;

        internal void SetLogger(ILogger logger) => Logger = logger;

        public abstract byte[] Invoke(string identifier, byte[] requestBytes);

        public abstract Task<byte[]> InvokeAsync(string identifier, byte[] requestBytes);
    }
}
