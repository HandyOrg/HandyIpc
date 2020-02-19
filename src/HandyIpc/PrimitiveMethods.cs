using System;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HandyIpc
{
    public delegate Task<byte[]> RemoteInvokeAsync(byte[] input, CancellationToken token);
    public delegate byte[] RemoteInvoke(byte[] input);

    public static class PrimitiveMethods
    {
        public static async Task<NamedPipeServerStream> CreateServerStreamAsync(string pipeName, CancellationToken token)
        {
            var stream = new NamedPipeServerStream(pipeName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances);
            await stream.WaitForConnectionAsync(token);
            return stream;
        }

        public static async Task HandleRequestAsync(
            NamedPipeServerStream stream,
            Func<byte[], Task<byte[]>> handler,
            long bufferSize,
            CancellationToken token)
        {
            using (stream)
            {
                var buffer = new byte[bufferSize];
                while (true)
                {
                    if (!stream.IsConnected || token.IsCancellationRequested) break;
                    var count = await stream.ReadAsync(buffer, 0, buffer.Length, token);
                    Guards.ThrowIfInvalid(count < bufferSize, $"The buffer length ({bufferSize}) may be too small and needs to be increased.");

                    if (!stream.IsConnected || token.IsCancellationRequested) break;
                    if (count <= 0) continue;
                    var output = await handler(buffer.Take(count).ToArray());

                    if (!stream.IsConnected || token.IsCancellationRequested) break;
                    await stream.WriteAsync(output, 0, output.Length, token);
                    await stream.FlushAsync(token);
                }
            }
        }

        public static async Task<(Action dispose, RemoteInvokeAsync invoke)> CreateClientAsync(string pipeName, long bufferSize)
        {
            var stream = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut);
            await stream.ConnectAsync();

            return (
                dispose: () => stream.Dispose(),
                invoke: async (input, token) =>
                {
                    var buffer = new byte[bufferSize];

                    try
                    {
                        token.ThrowIfCancellationRequested();
                        await stream.WriteAsync(input, 0, input.Length, token);

                        token.ThrowIfCancellationRequested();
                        await stream.FlushAsync(token);

                        token.ThrowIfCancellationRequested();
                        var count = await stream.ReadAsync(buffer, 0, buffer.Length, token);
                        Guards.ThrowIfInvalid(count < bufferSize, $"The buffer length ({bufferSize}) may be too small and needs to be increased.");

                        return buffer.Take(count).ToArray();
                    }
                    catch
                    {
                        stream.Dispose();
                        throw;
                    }
                }
            );
        }

        public static (Action dispose, RemoteInvoke invoke) CreateClient(string pipeName, long bufferSize)
        {
            var stream = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut);
            stream.Connect();

            return (
                dispose: stream.Dispose,
                invoke: input =>
                {
                    var buffer = new byte[bufferSize];

                    try
                    {
                        stream.Write(input, 0, input.Length);
                        stream.Flush();

                        var count = stream.Read(buffer, 0, buffer.Length);
                        Guards.ThrowIfInvalid(count < bufferSize, $"The buffer length ({bufferSize}) may be too small and needs to be increased.");

                        return buffer.Take(count).ToArray();
                    }
                    catch
                    {
                        stream.Dispose();
                        throw;
                    }
                }
            );
        }
    }
}
