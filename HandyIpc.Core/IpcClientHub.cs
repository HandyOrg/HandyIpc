using System;
using System.Collections.Concurrent;
using HandyIpc.Core;

namespace HandyIpc
{
    internal class IpcClientHub : IIpcClientHub
    {
        private readonly IRmiClient _rmiClient;
        private readonly ISerializer _serializer;
        private readonly ConcurrentDictionary<Type, object> _typeInstanceMapping = new();

        public IpcClientHub(IRmiClient rmiClient, ISerializer serializer)
        {
            _rmiClient = rmiClient;
            _serializer = serializer;
        }

        public T Of<T>(string? accessToken = null)
        {
            return (T)_typeInstanceMapping.GetOrAdd(typeof(T), key =>
            {
                Type type = key.GetClientType();

                if (key.IsGenericType)
                {
                    key = key.GetGenericTypeDefinition();
                }

                string identifier = key.ResolveIdentifier();
                return Activator.CreateInstance(type, _rmiClient, _serializer, identifier, accessToken);
            });
        }
    }
}
