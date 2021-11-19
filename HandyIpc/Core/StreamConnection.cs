using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace HandyIpc.Core
{
    public sealed class StreamConnection : IConnection
    {
        private readonly Stream _stream;

        public StreamConnection(Stream stream) => _stream = stream;

        public void Write(byte[] bytes)
        {
            _stream.Write(bytes, 0, bytes.Length);
            _stream.Flush();
        }

        public async Task WriteAsync(byte[] bytes, CancellationToken token)
        {
            await _stream.WriteAsync(bytes, 0, bytes.Length, token).ConfigureAwait(false);
            await _stream.FlushAsync(token).ConfigureAwait(false);
        }

        public byte[] Read()
        {
            return _stream.ReadAllBytes();
        }

        public Task<byte[]> ReadAsync(CancellationToken token)
        {
            return _stream.ReadAllBytesAsync(token);
        }

        public void Dispose() => _stream.Dispose();
    }
}
