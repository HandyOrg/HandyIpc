using System;
using System.Threading;
using System.Threading.Tasks;

namespace HandyIpc.Core
{
    public sealed class Sender : IDisposable
    {
        private readonly Pool<IConnection> _connectionPool;
        private readonly AsyncPool<IConnection> _asyncConnectionPool;

        internal Sender(IClient client)
        {
            _connectionPool = new Pool<IConnection>(client.Connect, CheckConnection);
            _asyncConnectionPool = new AsyncPool<IConnection>(client.ConnectAsync, CheckAsyncConnection);
        }

        public byte[] Invoke(byte[] bytes)
        {
            using RentedValue<IConnection> invokeOwner = _connectionPool.Rent();
            byte[] response = invokeOwner.Value.Invoke(bytes);
            return response;
        }

        public async Task<byte[]> InvokeAsync(byte[] bytes)
        {
            using RentedValue<IConnection> invokeOwner = await _asyncConnectionPool.RentAsync();
            byte[] response = await invokeOwner.Value.InvokeAsync(bytes, CancellationToken.None);
            return response;
        }

        private static bool CheckConnection(IConnection connection)
        {
            try
            {
                byte[] response = connection.Invoke(Signals.Empty);
                return response.IsEmpty();
            }
            catch
            {
                connection.Dispose();
                return false;
            }
        }

        private static async Task<bool> CheckAsyncConnection(IConnection connection)
        {
            try
            {
                byte[] response = await connection.InvokeAsync(Signals.Empty, CancellationToken.None);
                return response.IsEmpty();
            }
            catch
            {
                connection.Dispose();
                return false;
            }
        }

        public void Dispose()
        {
            _connectionPool.Dispose();
            _asyncConnectionPool.Dispose();
        }
    }
}
