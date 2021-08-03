using System.Threading;
using System.Threading.Tasks;
using HandyIpc.Core;

namespace HandyIpc.NamedPipe
{
    internal class RmiClient : IRmiClient
    {
        private readonly ClientConnectionPool _clientPool;

        public RmiClient()
        {
            _clientPool = new ClientConnectionPool();
        }

        public byte[] Invoke(string pipeName, byte[] requestBytes)
        {
            using var invokeOwner = _clientPool.Rent(pipeName);
            byte[] response = invokeOwner.Value(requestBytes);
            return response;
        }

        public async Task<byte[]> InvokeAsync(string pipeName, byte[] requestBytes)
        {
            using var invokeOwner = await _clientPool.RentAsync(pipeName);
            byte[] response = await invokeOwner.Value(requestBytes, CancellationToken.None);
            return response;
        }
    }
}
