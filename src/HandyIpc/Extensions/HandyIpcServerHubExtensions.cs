using System;

// ReSharper disable once CheckNamespace
namespace HandyIpc.Server
{
    public static class HandyIpcServerHubExtensions
    {
        public static IDisposable Start<TInterface, TImpl>(this IIpcServerHub server, string? accessToken = null)
            where TInterface : class
            where TImpl : TInterface, new()
        {
            return server.Start<TInterface>(() => new TImpl(), accessToken);
        }

        public static IDisposable Start<TInterface>(this IIpcServerHub server, Func<TInterface> factory, string? accessToken = null)
            where TInterface : class
        {
            return server.Start(typeof(TInterface), factory, accessToken);
        }

        public static IDisposable Start(this IIpcServerHub server, Type interfaceType, Type classType, string? accessToken = null)
        {
            // TODO: Add defensive code.

            return classType.ContainsGenericParameters
                ? server.Start(interfaceType, genericTypes =>
                {
                    var constructedClassType = classType.MakeGenericType(genericTypes);
                    return Activator.CreateInstance(constructedClassType);
                }, accessToken)
                : server.Start(interfaceType, () => Activator.CreateInstance(classType), accessToken);
        }
    }
}
