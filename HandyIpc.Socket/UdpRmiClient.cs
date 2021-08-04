using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HandyIpc.Core;

namespace HandyIpc.Socket
{
    internal class UdpRmiClient : IRmiClient
    {
        public byte[] Invoke(string identifier, byte[] requestBytes) => throw new NotImplementedException();

        public async Task<byte[]> InvokeAsync(string identifier, byte[] requestBytes) => throw new NotImplementedException();
    }
}
