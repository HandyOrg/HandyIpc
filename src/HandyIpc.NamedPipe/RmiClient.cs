using System;
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

        public T Invoke<T>(string pipeName, Request request, object?[]? arguments)
        {
            using var invokeOwner = _clientPool.Rent(pipeName);
            var response = invokeOwner.Value(_serializer.SerializeRequest(request, arguments));
            return Unpack<T>(response);
        }

        public Task<T> InvokeAsync<T>(string pipeName, Request request, object?[]? arguments)
        {
            return InvokeAsync<T>(pipeName, request, arguments, CancellationToken.None);
        }

        public async Task<T> InvokeAsync<T>(string pipeName, Request request, object?[]? arguments, CancellationToken token)
        {
            AsyncDisposableValue<RemoteInvokeAsync>? invokeOwner = null;
            try
            {
                invokeOwner = await _clientPool.RentAsync(pipeName);
                var response = await invokeOwner.Value(_serializer.SerializeRequest(request, arguments), token);
                return Unpack<T>(response);
            }
            finally
            {
                if (invokeOwner is not null)
                {
                    await invokeOwner.DisposeAsync();
                }
            }
        }

        private T Unpack<T>(byte[] bytes)
        {
            var response = _serializer.DeserializeResponse(bytes);

            if (response == null)
            {
                throw new NullReferenceException("The response can not be null.");
            }

            if (response.Exception is not null)
            {
                throw response.Exception;
            }

            // If the Exception is null, the Value must be valid.
            return (T)response.Value!;
        }
    }
}
