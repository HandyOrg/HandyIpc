using System;
using System.Collections.Concurrent;
using HandyIpc.Core;

namespace HandyIpc
{
    internal sealed class Client : IClient
    {
        private readonly SenderBase _sender;
        private readonly ISerializer _serializer;
        private readonly ConcurrentDictionary<Type, object> _typeInstanceMapping = new();

        private bool _isDisposed;

        public Client(SenderBase sender, ISerializer serializer)
        {
            _sender = sender;
            _serializer = serializer;
        }

        public T Resolve<T>(string key)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(IClient));
            }

            return (T)_typeInstanceMapping.GetOrAdd(typeof(T), interfaceType =>
            {
                Type type = interfaceType.GetClientType();
                return Activator.CreateInstance(type, _sender, _serializer, key);
            });
        }

        public void Dispose()
        {
            _isDisposed = true;

            _typeInstanceMapping.Clear();
        }
    }
}
