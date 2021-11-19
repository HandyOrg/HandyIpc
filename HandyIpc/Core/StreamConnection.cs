using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace HandyIpc.Core
{
    public class StreamConnection : IConnection
    {
        private readonly Stream _stream;

        private bool _disposedValue;

        public StreamConnection(Stream stream) => _stream = stream;

        public virtual void Write(byte[] bytes)
        {
            _stream.Write(bytes, 0, bytes.Length);
            _stream.Flush();
        }

        public virtual async Task WriteAsync(byte[] bytes, CancellationToken token)
        {
            await _stream.WriteAsync(bytes, 0, bytes.Length, token).ConfigureAwait(false);
            await _stream.FlushAsync(token).ConfigureAwait(false);
        }

        public virtual byte[] Read()
        {
            return _stream.ReadAllBytes();
        }

        public virtual Task<byte[]> ReadAsync(CancellationToken token)
        {
            return _stream.ReadAllBytesAsync(token);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                _stream.Dispose();
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            System.GC.SuppressFinalize(this);
        }
    }
}
