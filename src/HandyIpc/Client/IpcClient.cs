using System;
using System.Collections.Concurrent;

namespace HandyIpc.Client
{
    public abstract class IpcClient
    {
        private static readonly ConcurrentDictionary<Type, object> TypeInstanceMapping = new ConcurrentDictionary<Type, object>();

        public static IpcPreferences Preferences { get; } = new IpcPreferences();

        public static T Of<T>()
        {
            return (T)TypeInstanceMapping.GetOrAdd(typeof(T), key =>
            {
                var type = key.GetClientType();

                if (key.IsGenericType)
                {
                    key = key.GetGenericTypeDefinition();
                }

                key.GetContractInfo(out var identifier, out var accessToken);
                return Activator.CreateInstance(type, identifier, accessToken);
            });
        }

        protected string Identifier { get; }

        protected string AccessToken { get; }

        protected IpcClient(string identifier, string accessToken)
        {
            Identifier = identifier;
            AccessToken = accessToken;
        }
    }
}
