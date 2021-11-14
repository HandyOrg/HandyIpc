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
    }
}
