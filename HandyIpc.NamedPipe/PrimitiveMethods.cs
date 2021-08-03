using System;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace HandyIpc.NamedPipe
{
    internal delegate Task<byte[]> RemoteInvokeAsync(byte[] input, CancellationToken token);
    internal delegate byte[] RemoteInvoke(byte[] input);

    internal static class PrimitiveMethods
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
            CancellationToken token)
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

                    if (!stream.IsConnected || token.IsCancellationRequested)
                    {
                        break;
                    }

                    if (buffer.Length == 0)
                    {
                        continue;
                    }

                    byte[] output = await handler(buffer);

                    if (!stream.IsConnected || token.IsCancellationRequested)
                    {
                        break;
                    }

                    await stream.WriteAsync(output, 0, output.Length, token);
                    await stream.FlushAsync(token);
                }
            }
        }

        public static async Task<(Action dispose, RemoteInvokeAsync invoke)> CreateClientAsync(string pipeName)
        {
            var stream = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut);
            await stream.ConnectAsync();

            return (
                dispose: () => stream.Dispose(),
                invoke: async (input, token) =>
                {
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        await stream.WriteAsync(input, 0, input.Length, token);

                        token.ThrowIfCancellationRequested();
                        await stream.FlushAsync(token);

                        token.ThrowIfCancellationRequested();
                        return await stream.ReadAllBytesAsync(token);
                    }
                    catch
                    {
                        stream.Dispose();
                        throw;
                    }
                }
            );
        }

        public static (Action dispose, RemoteInvoke invoke) CreateClient(string pipeName)
        {
            var stream = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut);
            stream.Connect();

            return (
                dispose: stream.Dispose,
                invoke: input =>
                {
                    try
                    {
                        stream.Write(input, 0, input.Length);
                        stream.Flush();
                        return stream.ReadAllBytes();
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
