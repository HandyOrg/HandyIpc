using System;
using Demo.Contracts;
using HandyIpc.Server;

namespace Demo.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IpcServerBuilder.Create()
                .Register(typeof(IDemo<>), typeof(Demo<>))
                .Build()
                .Start();

            Console.ReadKey();
        }
    }
}
