using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HandyIpc.Client;
using HandyIpc.NamedPipe;
using HandyIpc.Server;
using HandyIpc.Tests.ContractInterfaces;
using HandyIpc.Tests.Impls;

namespace HandyIpc.Tests
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            IIpcServerHub server = HandyIpcHub
                .CreateServerFactory()
                .UseNamedPipe()
                .Build();
            server.Start<IGenericMethods, GenericMethods>();
            server.Start(typeof(IGenericInterface<>), typeof(GenericImpl<>));

            IIpcClientHub client = HandyIpcHub
                .CreateClientFactory()
                .UseNamedPipe()
                .Build();
            var genericMethods = client.Of<IGenericMethods>();

            var r1 = await genericMethods.PrintAsync<string, int>(null, new List<List<List<string>>>
            {
                new List<List<string>>
                {
                    new List<string>
                    {
                        "HAHAHA!",
                    }
                }
            });
            var r2 = await genericMethods.PrintAsync<string, int>(null, null);

            Console.ReadKey();
        }
    }
}
