using System.Threading;
using System.Threading.Tasks;

namespace HandyIpc.Core
{
    public abstract class RmiServerBase
    {
        protected ILogger Logger { get; private set; } = null!;

        internal void SetLogger(ILogger logger) => Logger = logger;

        public virtual Middleware BuildMiddleware(Middleware dispatcher, string? accessToken)
        {
            Middleware middleware = Middlewares.Compose(
                Middlewares.Heartbeat,
                Middlewares.ExceptionHandler,
                Middlewares.RequestParser);

            if (!string.IsNullOrEmpty(accessToken))
            {
                middleware = middleware.Then(Middlewares.GetAuthenticator(accessToken!));
            }

            return middleware.Then(dispatcher);
        }

        public abstract Task RunAsync(string identifier, RequestHandler handler, CancellationToken token);
    }
}
