using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HandyIpc.Client;

namespace HandyIpc.Tests.Mock
{
    public class MockRmiClient : IRmiClient
    {
        private readonly ISerializer _serializer;

        public MockRmiClient(ISerializer serializer)
        {
            _serializer = serializer;
        }

        public T Invoke<T>(string pipeName, Request request, IReadOnlyList<Argument> arguments)
        {
            throw new NotImplementedException();
        }

        public async Task<T> InvokeAsync<T>(string pipeName, Request request, IReadOnlyList<Argument> arguments)
        {
            throw new NotImplementedException();
        }
    }
}
