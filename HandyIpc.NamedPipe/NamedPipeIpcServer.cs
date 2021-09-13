using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using HandyIpc.Core;

namespace HandyIpc.NamedPipe
{
    internal class NamedPipeIpcServer : IServer
    {
        private readonly string _pipeName;
        private readonly CancellationTokenSource _source = new();

        public NamedPipeIpcServer(string pipeName)
        {
            _pipeName = pipeName;
        }

        public async Task<IConnection> WaitForConnectionAsync()
        {
            var stream = new NamedPipeServerStream(_pipeName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances);
            await stream.WaitForConnectionAsync(_source.Token);
            return new StreamConnection(stream);
        }

        public void Dispose()
        {
            _source.Cancel();
            _source.Dispose();
        }
    }
}
