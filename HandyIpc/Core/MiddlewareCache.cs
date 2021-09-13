using System;
using System.Collections.Generic;
using System.Linq;

namespace HandyIpc.Core
{
    public class MiddlewareCache
    {
        private readonly Dictionary<Type, Dictionary<string, Middleware>> _cache = new();

        internal MiddlewareCache() { }

        public Middleware GetOrAdd(Type interfaceType, string key, Func<Type, string, Middleware> factory)
        {
            bool hasMap = _cache.TryGetValue(interfaceType, out var middlewareMap);
            if (!hasMap)
            {
                middlewareMap = new Dictionary<string, Middleware>();
                _cache.Add(interfaceType, middlewareMap);
            }

            if (!hasMap || !middlewareMap.TryGetValue(key, out var middleware))
            {
                middleware = factory(interfaceType, key);
                middlewareMap.Add(key, middleware);
            }

            return middleware;
        }

        public void Remove(Type interfaceType)
        {
            if (interfaceType.IsGenericType)
            {
                var allRelatedInterfaces = _cache
                    .Where(item => interfaceType.IsAssignableFrom(item.Key))
                    .Select(item => item.Key);

                foreach (Type type in allRelatedInterfaces)
                {
                    _cache.Remove(type);
                }
            }
            else
            {
                _cache.Remove(interfaceType);
            }
        }
    }
}
