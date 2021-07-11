using System;
using Demo.Contracts;
using HandyIpc;
using HandyIpc.NamedPipe;
using HandyIpc.Serializer.Json;
using HandyIpc.Server;

namespace Demo.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var server = HandyIpcHub
                .CreateServerFactory()
                .UseJsonSerializer()
                .UseNamedPipe()
                .Build();

            server.Start(typeof(IDemo<>), typeof(Demo<>));

            Console.ReadKey();
        }
    }
}
