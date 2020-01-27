using System;
using Demo.Contracts;
using HandyIpc.Server;

namespace Demo.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IpcServer.Center
                .Register(typeof(IDemo<>), typeof(Demo<>))
                .Start();

            Console.ReadKey();
        }
    }
}
