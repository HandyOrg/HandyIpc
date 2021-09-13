using System;
using System.Threading;
using System.Threading.Tasks;

namespace HandyIpc.Core
{
    public interface IConnection : IDisposable
    {
        void Write(byte[] bytes);

        Task WriteAsync(byte[] bytes, CancellationToken token);

        byte[] Read();

        Task<byte[]> ReadAsync(CancellationToken token);
    }
}
