using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using HandyIpc.Core;
using HandyIpc.Implementation;

namespace HandyIpc.Socket
{
    internal class TcpRmiClient : RmiClientBase
    {
        private readonly Pool<string, ClientItem> _clientPool;
        private readonly AsyncPool<string, AsyncClientItem> _asyncClientPool;

        public TcpRmiClient()
        {
            _clientPool = new Pool<string, ClientItem>(CreateClient, CheckClient);
            _asyncClientPool = new AsyncPool<string, AsyncClientItem>(CreateAsyncClient, CheckAsyncClient);
        }

        public override byte[] Invoke(string identifier, byte[] requestBytes)
        {
            using var invokeOwner = _clientPool.Rent(identifier);
            byte[] response = invokeOwner.Value.Invoke(requestBytes);
            return response;
        }

        public override async Task<byte[]> InvokeAsync(string identifier, byte[] requestBytes)
        {
            using var invokeOwner = await _asyncClientPool.RentAsync(identifier);
            byte[] response = await invokeOwner.Value.InvokeAsync(requestBytes, CancellationToken.None);
            return response;
        }

        private static ClientItem CreateClient(string connectionString)
        {
            (IPAddress ip, int port) = connectionString.ToIpEndPoint();
            TcpClient client = new();
            client.Connect(ip, port);
            return new ClientItem(client);
        }

        private static bool CheckClient(ClientItem item)
        {
            try
            {
                byte[] response = item.Invoke(Signals.Empty);
                return response.IsEmpty();
            }
            catch (Exception e)
            {
                item.Dispose();
                return false;
            }
        }

        private static async Task<AsyncClientItem> CreateAsyncClient(string connectionString)
        {
            (IPAddress ip, int port) = connectionString.ToIpEndPoint();
            TcpClient client = new();
            await client.ConnectAsync(ip, port);
            return new AsyncClientItem(client);
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
            private readonly TcpClient _client;

            public ClientItem(TcpClient client) => _client = client;

            public byte[] Invoke(byte[] input)
            {
                try
                {
                    NetworkStream stream = _client.GetStream();
                    stream.Write(input, 0, input.Length);
                    stream.Flush();
                    return stream.ReadAllBytes();
                }
                catch (Exception e)
                {
                    _client.Dispose();
                    throw;
                }
            }

            public void Dispose() => _client.Dispose();
        }

        private sealed class AsyncClientItem : IDisposable
        {
            private readonly TcpClient _client;

            public AsyncClientItem(TcpClient client) => _client = client;

            public async Task<byte[]> InvokeAsync(byte[] input, CancellationToken token)
            {
                try
                {
                    NetworkStream stream = _client.GetStream();

                    token.ThrowIfCancellationRequested();

                    await stream.WriteAsync(input, 0, input.Length, token);
                    await stream.FlushAsync(token);
                    return await stream.ReadAllBytesAsync(token);
                }
                catch
                {
                    _client.Dispose();
                    throw;
                }
            }

            public void Dispose() => _client.Dispose();
        }
    }
}
