using System;
using System.Threading.Tasks;
using Demo.Contracts;
using HandyIpc;
using HandyIpc.NamedPipe;
using HandyIpc.Serializer.Json;

namespace Demo.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var client = HandyIpcHub
                .CreateClientFactory()
                .UseJsonSerializer()
                .UseNamedPipe()
                .Build();

            var demo = client.Of<IDemo<string>>();

            Console.WriteLine("Start.");
            //Parallel.For(0, 1000000, (i) =>
            //{
            //    var result = demo.Add(12, i);
            //    Debug.Assert(Math.Abs(12 + i - result) < 10e-6);
            //});
            for (int i = 0; i < 1000; i++)
            {
                demo.PrintMessage($"Message: {i}");
                await Task.Delay(1000);
            }

            Console.WriteLine("Completed.");
            Console.ReadKey();
        }
    }
}
