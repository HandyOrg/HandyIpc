using HandyIpc.Client;
using HandyIpc.Server;
using System;
using System.Collections.Generic;
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

            var b = demo1.GenericMethod<string, int>("aaa", "bbb");
            var rr = await demo1.GenericMethod(new Dictionary<string, int>(), new List<double> { 1.2, 12.21, 2123 });

            Console.ReadKey();
        }
    }
}
