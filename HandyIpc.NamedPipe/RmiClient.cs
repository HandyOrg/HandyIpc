using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HandyIpc.Client;

namespace HandyIpc.NamedPipe
{
    internal class RmiClient : IRmiClient
    {
        private readonly ClientConnectionPool _clientPool;
        private readonly ISerializer _serializer;

        public RmiClient(ISerializer serializer)
        {
            _clientPool = new ClientConnectionPool();
            _serializer = serializer;
        }

        public T Invoke<T>(string pipeName, RequestHeader request, IReadOnlyList<Argument> arguments)
        {
            using var invokeOwner = _clientPool.Rent(pipeName);
            var response = invokeOwner.Value(Signals.GetRequestBytes(request, arguments, _serializer.Serialize));
            return Unpack<T>(response);
        }

        public async Task<T> InvokeAsync<T>(string pipeName, RequestHeader request, IReadOnlyList<Argument> arguments)
        {
            using var invokeOwner = await _clientPool.RentAsync(pipeName);
            var response = await invokeOwner.Value(
                Signals.GetRequestBytes(request, arguments, _serializer.Serialize),
                CancellationToken.None);
            return Unpack<T>(response);
        }

        private T Unpack<T>(byte[] bytes)
        {
            bool hasValue = Signals.GetResponse(bytes, typeof(T), _serializer.Deserialize, out object? value, out Exception? exception);

            return hasValue ? (T) value! : throw exception!;
        }
    }
}
