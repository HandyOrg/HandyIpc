using System;

// ReSharper disable once CheckNamespace
namespace HandyIpc
{
    public static class HandyIpcServerHubExtensions
    {
        public static IDisposable Start<TInterface, TImpl>(this IIpcServerHub server)
            where TInterface : class
            where TImpl : TInterface, new()
        {
            return server.Start<TInterface>(() => new TImpl());
        }

        public static IDisposable Start<TInterface>(this IIpcServerHub server, Func<TInterface> factory)
            where TInterface : class
        {
            return server.Start(typeof(TInterface), factory);
        }

        public static IDisposable Start(this IIpcServerHub server, Type interfaceType, Type classType)
        {
            // TODO: Add defensive code.

            return classType.ContainsGenericParameters
                ? server.Start(interfaceType, genericTypes =>
                {
                    var constructedClassType = classType.MakeGenericType(genericTypes);
                    return Activator.CreateInstance(constructedClassType);
                })
                : server.Start(interfaceType, () => Activator.CreateInstance(classType));
        }
    }
}
