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

        public RmiClient(long bufferSize, ISerializer serializer)
        {
            _clientPool = new ClientConnectionPool(bufferSize);
            _serializer = serializer;
        }

        public T Invoke<T>(string pipeName, Request request)
        {
            using var invokeOwner = _clientPool.Rent(pipeName);
            var response = invokeOwner.Value(_serializer.SerializeRequest(request));
            return Unpack<T>(response);
        }

        public Task<T> InvokeAsync<T>(string pipeName, Request request)
        {
            return InvokeAsync<T>(pipeName, request, CancellationToken.None);
        }

        public async Task<T> InvokeAsync<T>(string pipeName, Request request, CancellationToken token)
        {
            AsyncDisposableValue<RemoteInvokeAsync>? invokeOwner = null;
            try
            {
                invokeOwner = await _clientPool.RentAsync(pipeName);
                var response = await invokeOwner.Value(_serializer.SerializeRequest(request), token);
                return Unpack<T>(response);
            }
            finally
            {
                if (invokeOwner != null)
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

            if (response.Exception != null)
            {
                throw response.Exception;
            }

            // If the Exception is not null, the Value can not be null.
            return (T)response.Value!;
        }
    }
}
