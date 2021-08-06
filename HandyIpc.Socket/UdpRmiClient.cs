using System;
using System.Threading.Tasks;
using HandyIpc.Core;

namespace HandyIpc.Socket
{
    internal class UdpRmiClient : RmiClientBase
    {
        public override byte[] Invoke(string identifier, byte[] requestBytes) => throw new NotImplementedException();

        public override async Task<byte[]> InvokeAsync(string identifier, byte[] requestBytes) => throw new NotImplementedException();
    }
}
