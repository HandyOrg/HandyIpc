using System;
using System.Threading;
using System.Threading.Tasks;
using HandyIpc.Server;

namespace HandyIpc.NamedPipe
{
    public class NamedPipeServer : IUnderlyingServer<Context>
    {
        public async Task RunAsync(string identifier, MiddlewareHandler<Context> middleware, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var stream = await PrimitiveMethods.CreateServerStreamAsync(identifier, token);

                    if (token.IsCancellationRequested) break;

#pragma warning disable 4014
                    PrimitiveMethods.HandleRequestAsync(stream, middleware.ToHandler(), HandyIpcHub.Preferences.BufferSize, token);
#pragma warning restore 4014
                }
                catch (OperationCanceledException)
                {
                    // Ignore
                }
                catch (Exception e)
                {
                    HandyIpcHub.Logger.Error($"An unexpected exception occurred in the server (Id: {identifier}).", e);
                }
            }
        }
    }
}
