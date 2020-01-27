using HandyIpc.Client;
using HandyIpc.Server;
using System;
using System.Threading.Tasks;

namespace HandyIpc.Tests
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            IpcServer.Center
                .Register(typeof(IDemo<>), typeof(Demo<>))
                .Start();

            var demo1 = IpcClient.Of<IDemo<string>>();
            var demo2 = IpcClient.Of<IDemo<int>>();

            var r1 = await demo1.GetDefaultAsync();
            var r2 = await demo2.GetDefaultAsync();

            Console.ReadKey();
        }
    }
}
