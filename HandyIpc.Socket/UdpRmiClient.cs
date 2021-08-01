using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HandyIpc.Core;

namespace HandyIpc.Socket
{
    internal class UdpRmiClient : IRmiClient
    {
        public T Invoke<T>(string identifier, RequestHeader request, IReadOnlyList<Argument> arguments)
        {
            throw new NotImplementedException();
        }

        public async Task<T> InvokeAsync<T>(string identifier, RequestHeader request, IReadOnlyList<Argument> arguments)
        {
            throw new NotImplementedException();
        }
    }
}
