using System;
using Demo.Contracts;
using HandyIpc;

namespace Demo.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            HandyIpcHub.Server.Start(typeof(IDemo<>), typeof(Demo<>));

            Console.ReadKey();
        }
    }
}
