using HandyIpc.Client;
using HandyIpc.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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
            var typeName = Enumerable.Repeat("213", 10).GetType().AssemblyQualifiedName;
            var type = Type.GetType(typeName);

            Console.ReadKey();
        }
    }
}
