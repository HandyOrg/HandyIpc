using System;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using HandyIpc.Core;
using HandyIpc.Implementation;

namespace HandyIpc.NamedPipe
{
    internal class NamedPipeSender : SenderBase
    {
        private readonly Pool<string, ClientItem> _clientPool;
        private readonly AsyncPool<string, AsyncClientItem> _asyncClientPool;

        public NamedPipeSender()
        {
            _clientPool = new Pool<string, ClientItem>(CreateClient, CheckClient);
            _asyncClientPool = new AsyncPool<string, AsyncClientItem>(CreateAsyncClient, CheckAsyncClient);
        }

        public override byte[] Send(string pipeName, byte[] requestBytes)
        {
            using var invokeOwner = _clientPool.Rent(pipeName);
            byte[] response = invokeOwner.Value.Invoke(requestBytes);
            return response;
        }

        public override async Task<byte[]> SendAsync(string pipeName, byte[] requestBytes)
        {
            using var invokeOwner = await _asyncClientPool.RentAsync(pipeName);
            byte[] response = await invokeOwner.Value.InvokeAsync(requestBytes, CancellationToken.None);
            return response;
        }

        private static ClientItem CreateClient(string pipeName)
        {
            var stream = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut);
            stream.Connect();
            return new ClientItem(stream);
        }

        private static bool CheckClient(ClientItem item)
        {
            try
            {
                byte[] response = item.Invoke(Signals.Empty);
                return response.IsEmpty();
            }
            catch
            {
                item.Dispose();
                return false;
            }
        }

        private static async Task<AsyncClientItem> CreateAsyncClient(string pipeName)
        {
            var stream = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut);
            await stream.ConnectAsync();
            return new AsyncClientItem(stream);
        }

        private static async Task<bool> CheckAsyncClient(AsyncClientItem item)
        {
            try
            {
                byte[] response = await item.InvokeAsync(Signals.Empty, CancellationToken.None);
                return response.IsEmpty();
            }
            catch
            {
                item.Dispose();
                return false;
            }
        }

        private sealed class ClientItem : IDisposable
        {
            private readonly NamedPipeClientStream _stream;

            public ClientItem(NamedPipeClientStream stream) => _stream = stream;

            public byte[] Invoke(byte[] input)
            {
                try
                {
                    _stream.Write(input, 0, input.Length);
                    _stream.Flush();
                    return _stream.ReadAllBytes();
                }
                catch
                {
                    _stream.Dispose();
                    throw;
                }
            }

            public void Dispose() => _stream.Dispose();
        }

        private sealed class AsyncClientItem : IDisposable
        {
            private readonly NamedPipeClientStream _stream;

            public AsyncClientItem(NamedPipeClientStream stream) => _stream = stream;

            public async Task<byte[]> InvokeAsync(byte[] input, CancellationToken token)
            {
                try
                {
                    token.ThrowIfCancellationRequested();
                    await _stream.WriteAsync(input, 0, input.Length, token);
                    await _stream.FlushAsync(token);
                    return await _stream.ReadAllBytesAsync(token);
                }
                catch
                {
                    _stream.Dispose();
                    throw;
                }
            }

            public void Dispose() => _stream.Dispose();
        }
    }
}
