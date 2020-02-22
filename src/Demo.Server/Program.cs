using System;
using Demo.Contracts;
using HandyIpc.Server;

namespace Demo.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IpcServer.Update(collection => collection.Add(typeof(IDemo<>), typeof(Demo<>)));

            Console.ReadKey();
        }
    }
}
