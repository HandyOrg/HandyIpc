using System;
using System.Collections.Concurrent;
using HandyIpc.Core;

namespace HandyIpc
{
    internal class ClientHub : IClientHub
    {
        private readonly SenderBase _sender;
        private readonly ISerializer _serializer;
        private readonly ConcurrentDictionary<Type, object> _typeInstanceMapping = new();

        public ClientHub(SenderBase sender, ISerializer serializer)
        {
            _sender = sender;
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
                return Activator.CreateInstance(type, _sender, _serializer, identifier, accessToken);
            });
        }
    }
}
