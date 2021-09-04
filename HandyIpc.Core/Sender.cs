using System;
using System.Threading;
using System.Threading.Tasks;
using HandyIpc.Core;

namespace HandyIpc
{
    public sealed class Sender : IDisposable
    {
        private readonly IClient _client;
        private readonly Pool<ClientItem> _clientPool;
        private readonly AsyncPool<AsyncClientItem> _asyncClientPool;

        internal Sender(IClient client)
        {
            _client = client;
            _clientPool = new Pool<ClientItem>(CreateClient, CheckClient);
            _asyncClientPool = new AsyncPool<AsyncClientItem>(CreateAsyncClient, CheckAsyncClient);
        }

        public byte[] Invoke(byte[] bytes)
        {
            using RentedValue<ClientItem> invokeOwner = _clientPool.Rent();
            byte[] response = invokeOwner.Value.Invoke(bytes);
            return response;
        }

        public async Task<byte[]> InvokeAsync(byte[] bytes)
        {
            using RentedValue<AsyncClientItem> invokeOwner = await _asyncClientPool.RentAsync();
            byte[] response = await invokeOwner.Value.InvokeAsync(bytes, CancellationToken.None);
            return response;
        }

        private ClientItem CreateClient()
        {
            IConnection connection = _client.Connect();
            return new ClientItem(connection);
        }

        private async Task<AsyncClientItem> CreateAsyncClient()
        {
            IConnection connection = await _client.ConnectAsync();
            return new AsyncClientItem(connection);
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

        public void Dispose()
        {
            _clientPool.Dispose();
            _asyncClientPool.Dispose();
        }

        private sealed class ClientItem : IDisposable
        {
            private readonly IConnection _connection;

            public ClientItem(IConnection connection) => _connection = connection;

            public byte[] Invoke(byte[] input)
            {
                try
                {
                    _connection.Write(input);
                    return _connection.Read();
                }
                catch
                {
                    _connection.Dispose();
                    throw;
                }
            }

            public void Dispose() => _connection.Dispose();
        }

        private sealed class AsyncClientItem : IDisposable
        {
            private readonly IConnection _connection;

            public AsyncClientItem(IConnection connection) => _connection = connection;

            public async Task<byte[]> InvokeAsync(byte[] input, CancellationToken token)
            {
                try
                {
                    await _connection.WriteAsync(input, token);
                    return await _connection.ReadAsync(token);
                }
                catch
                {
                    _connection.Dispose();
                    throw;
                }
            }

            public void Dispose() => _connection.Dispose();
        }
    }
}
