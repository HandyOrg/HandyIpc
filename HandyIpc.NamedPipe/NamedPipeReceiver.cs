using System;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using HandyIpc.Core;

namespace HandyIpc.NamedPipe
{
    internal class NamedPipeReceiver : ReceiverBase
    {
        private readonly string _pipeName;

        public NamedPipeReceiver(string pipeName) => _pipeName = pipeName;

        public override async Task StartAsync(RequestHandler handler, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var stream = await CreateServerStreamAsync(_pipeName, token);

                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    // Do not await the request handler, and go to await next stream connection directly.
#pragma warning disable 4014
                    HandleRequestAsync(stream, handler, token);
#pragma warning restore 4014
                }
                catch (OperationCanceledException)
                {
                    // Ignore
                }
                catch (Exception e)
                {
                    Logger.Error($"An unexpected exception occurred in the server (Id: {_pipeName}).", e);
                }
            }
        }

        private static async Task<NamedPipeServerStream> CreateServerStreamAsync(string pipeName, CancellationToken token)
        {
            var stream = new NamedPipeServerStream(pipeName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances);
            await stream.WaitForConnectionAsync(token);
            return stream;
        }

        private static async Task HandleRequestAsync(NamedPipeServerStream stream, RequestHandler handler, CancellationToken token)
        {
            using (stream)
            {
                while (true)
                {
                    if (!stream.IsConnected || token.IsCancellationRequested)
                    {
                        break;
                    }

                    byte[] buffer = await stream.ReadAllBytesAsync(token);
                    if (buffer.Length == 0)
                    {
                        continue;
                    }

                    byte[] output = await handler(buffer);
                    await stream.WriteAsync(output, 0, output.Length, token);
                    await stream.FlushAsync(token);
                }
            }
        }
    }
}
