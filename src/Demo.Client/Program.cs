using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Demo.Contracts;
using HandyIpc.Client;

namespace Demo.Client
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var demo = IpcClient.Of<IDemo<string>>();

            Console.WriteLine("Start.");
            Parallel.For(0, 1000000, (i) =>
            {
                var result = demo.Add(12, i);
                Debug.Assert(Math.Abs(12 + i - result) < 10e-6);
            });
            Console.WriteLine("Completed.");
            Console.ReadKey();
        }
    }
}
