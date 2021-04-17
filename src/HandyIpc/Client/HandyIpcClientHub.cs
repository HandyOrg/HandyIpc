﻿using System;
using System.Collections.Concurrent;

namespace HandyIpc.Client
{
    internal class HandyIpcClientHub : IIpcClientHub
    {
        private readonly IRmiClient _rmiClient;
        private readonly ConcurrentDictionary<Type, object> _typeInstanceMapping = new();

        public HandyIpcClientHub(IRmiClient rmiClient)
        {
            _rmiClient = rmiClient;
        }

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
                return Activator.CreateInstance(type, _rmiClient, identifier, accessToken);
            });
        }
    }
}
