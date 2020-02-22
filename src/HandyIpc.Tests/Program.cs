using HandyIpc.Client;
using HandyIpc.Server;
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
            IpcServer.Update(collection => collection
                .Add<IGenericMethods, GenericMethods>()
                .Add(typeof(IGenericInterface<>), typeof(GenericImpl<>)));

            IpcClient.Preferences.BufferSize = 1024 * 8;
            var client = IpcClient.Of<IGenericMethods>();

            var r1 = await client.PrintAsync<string, int>(null, null);
            var r2 = await client.PrintAsync<string, int>(null, null);

            Console.ReadKey();
        }
    }
}
