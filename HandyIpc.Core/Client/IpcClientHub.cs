using System;
using System.Collections.Concurrent;

namespace HandyIpc.Client
{
    internal class IpcClientHub : IIpcClientHub
    {
        private readonly IRmiClient _rmiClient;
        private readonly ConcurrentDictionary<Type, object> _typeInstanceMapping = new();

        public IpcClientHub(IRmiClient rmiClient)
        {
            _rmiClient = rmiClient;
        }

        public T Of<T>(string? accessToken = null)
        {
            return (T)_typeInstanceMapping.GetOrAdd(typeof(T), key =>
            {
                var type = key.GetClientType();

                if (key.IsGenericType)
                {
                    key = key.GetGenericTypeDefinition();
                }

                string identifier = key.ResolveIdentifier();
                return Activator.CreateInstance(type, _rmiClient, identifier, accessToken);
            });
        }
    }
}
