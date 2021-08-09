using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using HandyIpc.Core;
using HandyIpc.Implementation;

namespace HandyIpc.Socket
{
    internal class TcpSender : SenderBase
    {
        private readonly Pool<string, ClientItem> _clientPool;
        private readonly AsyncPool<string, AsyncClientItem> _asyncClientPool;

        public TcpSender()
        {
            _clientPool = new Pool<string, ClientItem>(CreateClient, CheckClient);
            _asyncClientPool = new AsyncPool<string, AsyncClientItem>(CreateAsyncClient, CheckAsyncClient);
        }

        public override byte[] Send(string identifier, byte[] requestBytes)
        {
            using var invokeOwner = _clientPool.Rent(identifier);
            byte[] response = invokeOwner.Value.Invoke(requestBytes);
            return response;
        }

        public override async Task<byte[]> SendAsync(string identifier, byte[] requestBytes)
        {
            using var invokeOwner = await _asyncClientPool.RentAsync(identifier);
            byte[] response = await invokeOwner.Value.InvokeAsync(requestBytes, CancellationToken.None);
            return response;
        }

        private ClientItem CreateClient(string connectionString)
        {
            try
            {
                (IPAddress ip, int port) = connectionString.ToIpEndPoint();
                TcpClient client = new();
                client.Connect(ip, port);
                return new ClientItem(client);
            }
            catch (Exception e)
            {
                Logger.Error("Unexpected exception occurred when sending message by creating tcp client.", e);
                throw;
            }
        }

        private bool CheckClient(ClientItem item)
        {
            try
            {
                byte[] response = item.Invoke(Signals.Empty);
                return response.IsEmpty();
            }
            catch (Exception e)
            {
                Logger.Error("Unexpected exception occurred when sending message by tcp client.", e);
                item.Dispose();
                return false;
            }
        }

        private async Task<AsyncClientItem> CreateAsyncClient(string connectionString)
        {
            try
            {
                (IPAddress ip, int port) = connectionString.ToIpEndPoint();
                TcpClient client = new();
                await client.ConnectAsync(ip, port);
                return new AsyncClientItem(client);
            }
            catch (Exception e)
            {
                Logger.Error("Unexpected exception occurred when sending ping by creating tcp client.", e);
                throw;
            }
        }

        private async Task<bool> CheckAsyncClient(AsyncClientItem item)
        {
            try
            {
                byte[] response = await item.InvokeAsync(Signals.Empty, CancellationToken.None);
                return response.IsEmpty();
            }
            catch (Exception e)
            {
                Logger.Error("Unexpected exception occurred when sending ping by tcp client.", e);
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
                catch
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
