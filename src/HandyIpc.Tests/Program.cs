using System;
using System.Threading.Tasks;
using HandyIpc.Tests.ContractInterfaces;
using HandyIpc.Tests.Impls;

namespace HandyIpc.Tests
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            HandyIpcHub.Preferences.BufferSize = 1024 * 8;
            HandyIpcHub.Server.Start<IGenericMethods, GenericMethods>();
            HandyIpcHub.Server.Start(typeof(IGenericInterface<>), typeof(GenericImpl<>));

            var client = HandyIpcHub.Client.Of<IGenericMethods>();

            var r1 = await client.PrintAsync<string, int>(null, null);
            var r2 = await client.PrintAsync<string, int>(null, null);

            Console.ReadKey();
        }
    }
}
