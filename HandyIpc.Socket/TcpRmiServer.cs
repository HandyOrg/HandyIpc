using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using HandyIpc.Core;

namespace HandyIpc.Socket
{
    internal class TcpRmiServer : IRmiServer
    {
        private static readonly char[] IdentifierSplitter = { ':', ' ' };

        public async Task RunAsync(string identifier, RequestHandler handler, CancellationToken token)
        {
            string[] address = identifier.Split(IdentifierSplitter, StringSplitOptions.RemoveEmptyEntries);
            if (address.Length != 2)
            {
                throw new FormatException("Invalid identifier, and the correct format is 'ip.address:port'.");
            }

            TcpListener listener = new TcpListener(IPAddress.Parse(address[0]), int.Parse(address[1]));
            var client = await listener.AcceptTcpClientAsync();
            client.GetStream().ReadAsync(,,)
        }
    }
}
