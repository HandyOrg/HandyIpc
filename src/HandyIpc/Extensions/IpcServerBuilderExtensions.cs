using System;

// ReSharper disable once CheckNamespace
namespace HandyIpc.Server
{
    public static class IpcServerBuilderExtensions
    {
        public static IpcServerBuilder Register<TInterface, TImpl>(this IpcServerBuilder server)
            where TInterface : class
            where TImpl : TInterface, new()
        {
            return server.Register<TInterface>(() => new TImpl());
        }

        public static IpcServerBuilder Register<TInterface>(this IpcServerBuilder server, Func<TInterface> factory)
            where TInterface : class
        {
            return server.Register(typeof(TInterface), factory);
        }

        public static IpcServerBuilder Register(this IpcServerBuilder server, Type interfaceType, Type classType)
        {
            Guards.ThrowIfNot(classType.ContainsGenericParameters, "", nameof(classType));

            return server.Register(interfaceType, genericTypes =>
            {
                var constructedClassType = classType.MakeGenericType(genericTypes);
                return Activator.CreateInstance(constructedClassType);
            });
        }
    }
}
