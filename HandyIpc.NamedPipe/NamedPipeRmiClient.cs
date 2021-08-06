using System.Threading;
using System.Threading.Tasks;
using HandyIpc.Core;

namespace HandyIpc.NamedPipe
{
    internal class NamedPipeRmiClient : RmiClientBase
    {
        private readonly ClientConnectionPool _clientPool;

        public NamedPipeRmiClient()
        {
            _clientPool = new ClientConnectionPool();
        }

        public override byte[] Invoke(string pipeName, byte[] requestBytes)
        {
            using var invokeOwner = _clientPool.Rent(pipeName);
            byte[] response = invokeOwner.Value(requestBytes);
            return response;
        }

        public override async Task<byte[]> InvokeAsync(string pipeName, byte[] requestBytes)
        {
            using var invokeOwner = await _clientPool.RentAsync(pipeName);
            byte[] response = await invokeOwner.Value(requestBytes, CancellationToken.None);
            return response;
        }
    }
}
