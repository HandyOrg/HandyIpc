using System;
using System.Threading;
using System.Threading.Tasks;

namespace HandyIpc.Core
{
    public static class ConnectionExtensions
    {
        public static byte[] Invoke(this IConnection connection, byte[] input)
        {
            try
            {
                connection.Write(input);
                return connection.Read();
            }
            catch
            {
                connection.Dispose();
                throw;
            }
        }

        public static async Task<byte[]> InvokeAsync(this IConnection connection, byte[] input, CancellationToken token)
        {
            try
            {
                await connection.WriteAsync(input, token);
                return await connection.ReadAsync(token);
            }
            catch
            {
                connection.Dispose();
                throw;
            }
        }

        public static void Subscribe(this IConnection connection, Guid id, Func<byte[], byte[]> callback, CancellationToken token)
        {
            // TODO: Subscribe by id.
            connection.Write(id.ToByteArray());
            byte[] subscribeResult = connection.Read();
            // Check result and retry.

            Task.Run(() => connection.Loop(id, callback, token), token);
        }

        private static async Task Loop(this IConnection connection, Guid id, Func<byte[], byte[]> callback, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                // Will blocked until accepted a notification.
                byte[] input = await connection.ReadAsync(token);
                byte[] output = callback(input);
                await connection.WriteAsync(output, token);
            }

            // TODO: Unsubscribe by id.
            await connection.WriteAsync(id.ToByteArray(), token);
            byte[] unsubscribeResult = await connection.ReadAsync(token);
            // Check result and retry.
        }
    }
}
