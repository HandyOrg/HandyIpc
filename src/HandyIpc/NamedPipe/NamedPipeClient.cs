using System;
using System.Threading;
using System.Threading.Tasks;
using HandyIpc.Client;
using HandyIpc.Extensions;

namespace HandyIpc.NamedPipe
{
    public class NamedPipeClient : IUnderlyingClient
    {
        public T Invoke<T>(string pipeName, Request request)
        {
            using var invokeOwner = ClientConnectionPool.Shared.Rent(pipeName);
            var response = invokeOwner.Value(request.ToBytes());
            return Unpack<T>(response);
        }

        public Task<T> InvokeAsync<T>(string pipeName, Request request)
        {
            return InvokeAsync<T>(pipeName, request, CancellationToken.None);
        }

        public async Task<T> InvokeAsync<T>(string pipeName, Request request, CancellationToken token)
        {
            AsyncDisposableValue<RemoteInvokeAsync> invokeOwner = null;
            try
            {
                invokeOwner = await ClientConnectionPool.Shared.RentAsync(pipeName);
                var response = await invokeOwner.Value(request.ToBytes(), token);
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

        private static T Unpack<T>(byte[] response)
        {
            var ipcResponse = response.ToObject<Response>();
            if (ipcResponse.Exception != null)
            {
                throw ipcResponse.Exception;
            }

            return ipcResponse.Value.CastTo<T>();
        }
    }
}
