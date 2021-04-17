using System;
using System.Threading.Tasks;
using HandyIpc.NamedPipe;
using HandyIpc.Tests.ContractInterfaces;
using HandyIpc.Tests.Impls;

namespace HandyIpc.Tests
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var server = HandyIpcHub
                .CreateServerFactory()
                .UseNamedPipe()
                .Build();
            server.Start<IGenericMethods, GenericMethods>();
            server.Start(typeof(IGenericInterface<>), typeof(GenericImpl<>));

            var client = HandyIpcHub
                .CreateClientFactory()
                .UseNamedPipe()
                .Build();
            var genericMethods = client.Of<IGenericMethods>();

            var r1 = await genericMethods.PrintAsync<string, int>(null, null);
            var r2 = await genericMethods.PrintAsync<string, int>(null, null);

            Console.ReadKey();
        }
    }
}
