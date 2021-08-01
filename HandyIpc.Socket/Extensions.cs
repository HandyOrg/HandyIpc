using System;
using System.Net.Sockets;
using HandyIpc.Core;

namespace HandyIpc.Socket
{
    public static class Extensions
    {
        public static IIpcFactory<IRmiClient, IIpcClientHub> UseTcp(this IIpcFactory<IRmiClient, IIpcClientHub> self)
        {
            throw new NotImplementedException();
        }

        public static IIpcFactory<IRmiServer, IIpcServerHub> UseTcp(this IIpcFactory<IRmiServer, IIpcServerHub> self, ILogger? logger = null)
        {
            throw new NotImplementedException();
        }

        public static IIpcFactory<IRmiClient, IIpcClientHub> UseUdp(this IIpcFactory<IRmiClient, IIpcClientHub> self)
        {
            throw new NotImplementedException();
        }

        public static IIpcFactory<IRmiServer, IIpcServerHub> UseUdp(this IIpcFactory<IRmiServer, IIpcServerHub> self, ILogger? logger = null)
        {
            throw new NotImplementedException();
        }
    }
}
