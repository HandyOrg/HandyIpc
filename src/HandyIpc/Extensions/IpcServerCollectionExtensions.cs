using System;

// ReSharper disable once CheckNamespace
namespace HandyIpc.Server
{
    public static class IpcServerCollectionExtensions
    {
        public static IpcServerCollection Add<TInterface, TImpl>(this IpcServerCollection server)
            where TInterface : class
            where TImpl : TInterface, new()
        {
            return server.Add<TInterface>(() => new TImpl());
        }

        public static IpcServerCollection Add<TInterface>(this IpcServerCollection server, Func<TInterface> factory)
            where TInterface : class
        {
            return server.Add(typeof(TInterface), factory);
        }

        public static IpcServerCollection Add(this IpcServerCollection server, Type interfaceType, Type classType)
        {
            Guards.ThrowIfNot(classType.ContainsGenericParameters, "", nameof(classType));

            return server.Add(interfaceType, genericTypes =>
            {
                var constructedClassType = classType.MakeGenericType(genericTypes);
                return Activator.CreateInstance(constructedClassType);
            });
        }

        public static IpcServerCollection Remove<T>(this IpcServerCollection server) where T : class
        {
            var interfaceType = typeof(T);
            Guards.ThrowIfNot(!interfaceType.IsGenericType, "", nameof(T));

            return server.Remove(interfaceType);
        }
    }
}
