using System;
using System.Collections.Concurrent;

namespace HandyIpc.Client
{
    internal class HandyIpcClientHub : IHandyIpcClientHub
    {
        private readonly ConcurrentDictionary<Type, object> _typeInstanceMapping = new ConcurrentDictionary<Type, object>();

        public T Of<T>()
        {
            return (T)_typeInstanceMapping.GetOrAdd(typeof(T), key =>
            {
                var type = key.GetClientType();

                if (key.IsGenericType)
                {
                    key = key.GetGenericTypeDefinition();
                }

                key.ResolveContractInfo(out var identifier, out var accessToken);
                return Activator.CreateInstance(type, identifier, accessToken);
            });
        }
    }
}
