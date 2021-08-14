using System.Threading;
using System.Threading.Tasks;

namespace HandyIpc.Core
{
    public abstract class ReceiverBase
    {
        protected ILogger Logger { get; private set; } = null!;

        internal void SetLogger(ILogger logger) => Logger = logger;

        public abstract Task StartAsync(RequestHandler handler, CancellationToken token);
    }
}
