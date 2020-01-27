using System;

// ReSharper disable once CheckNamespace
namespace HandyIpc.Server
{
    public static class IpcServerExtensions
    {
        public static IpcServer Register<TInterface, TImpl>(this IpcServer server)
            where TInterface : class
            where TImpl : TInterface, new()
        {
            return server.Register<TInterface>(() => new TImpl());
        }

        public static IpcServer Register<TInterface>(this IpcServer server, Func<TInterface> factory)
            where TInterface : class
        {
            return server.Register(typeof(TInterface), factory);
        }
    }
}
