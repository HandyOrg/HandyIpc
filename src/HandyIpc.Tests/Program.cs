using HandyIpc.Client;
using HandyIpc.Server;
using System;
using System.Text;
using System.Threading.Tasks;
using HandyIpc.Tests.ContractInterfaces;
using HandyIpc.Tests.Impls;
using Newtonsoft.Json;

namespace HandyIpc.Tests
{
    public class Program
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            TypeNameHandling = TypeNameHandling.All
        };

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
