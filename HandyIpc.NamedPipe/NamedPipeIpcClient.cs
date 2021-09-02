using System.IO.Pipes;
using System.Threading.Tasks;

namespace HandyIpc.NamedPipe
{
    internal class NamedPipeIpcClient : IClient
    {
        private readonly string _pipeName;

        public NamedPipeIpcClient(string pipeName)
        {
            _pipeName = pipeName;
        }

        public IConnection Connect()
        {
            var stream = new NamedPipeClientStream(".", _pipeName, PipeDirection.InOut);
            stream.Connect();
            return new StreamConnection(stream);
        }

        public async Task<IConnection> ConnectAsync()
        {
            var stream = new NamedPipeClientStream(".", _pipeName, PipeDirection.InOut);
            await stream.ConnectAsync();
            return new StreamConnection(stream);
        }
    }
}
