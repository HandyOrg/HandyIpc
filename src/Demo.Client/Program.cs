using System;
using Demo.Contracts;
using HandyIpc.Client;

namespace Demo.Client
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var demo = IpcClient.Of<IDemo<string>>();

            Console.ReadKey();
        }
    }
}
