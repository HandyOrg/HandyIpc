using System;
using System.Threading;
using System.Threading.Tasks;
using HandyIpc.Extensions;

namespace HandyIpc.Client
{
    public static class RemoteMethod
    {
        public static void Invoke(string pipeName, Request request)
        {
            var result = Invoke<byte[]>(pipeName, request);
            Guards.ThrowIfInvalid(result.IsUnit(), "The server did not return the correct result.");
        }

        public static async Task InvokeAsync(string pipeName, Request request)
        {
            var result = await InvokeAsync<byte[]>(pipeName, request);
            Guards.ThrowIfInvalid(result.IsUnit(), "The server did not return the correct result.");
        }

        public static T Invoke<T>(string pipeName, Request request)
        {
            using var invokeOwner = ClientPool.Shared.Rent(pipeName);
            var response = invokeOwner.Value(request.ToBytes());
            return Unpack<T>(response);
        }

        public static Task<T> InvokeAsync<T>(string pipeName, Request request)
        {
            return InvokeAsync<T>(pipeName, request, CancellationToken.None);
        }

        private static async Task<T> InvokeAsync<T>(string pipeName, Request request, CancellationToken token)
        {
            AsyncDisposableValue<RemoteInvokeAsync> invokeOwner = null;
            try
            {
                invokeOwner = await ClientPool.Shared.RentAsync(pipeName);
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

            if (ipcResponse == null)
            {
                throw new NullReferenceException();
            }

            if (ipcResponse.Exception != null)
            {
                throw ipcResponse.Exception;
            }

            return ipcResponse.Value.CastTo<T>()!;
        }
    }
}
