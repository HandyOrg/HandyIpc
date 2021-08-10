using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using HandyIpc.Core;

namespace HandyIpc.Socket
{
    internal class TcpReceiver : ReceiverBase
    {
        private readonly IPAddress _ip;
        private readonly int _port;

        public TcpReceiver(IPAddress ip, int port)
        {
            _ip = ip;
            _port = port;
        }

        public override async Task StartAsync(RequestHandler handler, CancellationToken token)
        {
            TcpListener listener = CreateTcpListener(_ip, _port);
            while (!token.IsCancellationRequested)
            {
                try
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();

                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    // Do not await the request handler, and go to await next stream connection directly.
#pragma warning disable 4014
                    HandleRequestAsync(client, handler, token);
#pragma warning restore 4014
                }
                catch (OperationCanceledException)
                {
                    // Ignore
                }
                catch (Exception e)
                {
                    Logger.Error($"An unexpected exception occurred in the server (IP: {_ip}, Port: {_port}).", e);
                }
            }

            listener.Stop();
        }

        private static TcpListener CreateTcpListener(IPAddress ip, int port)
        {
            var listener = new TcpListener(ip, port);
            listener.Start();
            return listener;
        }

        private static async Task HandleRequestAsync(TcpClient client, RequestHandler handler, CancellationToken token)
        {
            using (client)
            {
                while (true)
                {
                    if (!client.Connected || token.IsCancellationRequested)
                    {
                        break;
                    }

                    NetworkStream stream = client.GetStream();

                    byte[] buffer = await stream.ReadAllBytesAsync(token);

                    if (buffer.Length == 0)
                    {
                        continue;
                    }

                    byte[] output = await handler(buffer);

                    await stream.WriteAsync(output, 0, output.Length, token);
                    await stream.FlushAsync(token);
                }
            }
        }
    }
}
