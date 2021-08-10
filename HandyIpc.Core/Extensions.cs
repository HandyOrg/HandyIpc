using System;

namespace HandyIpc
{
    public static class Extensions
    {
        public static IDisposable Register<TInterface, TImpl>(this IServerHub server)
            where TInterface : class
            where TImpl : TInterface, new()
        {
            return server.Register<TInterface>(() => new TImpl());
        }

        public static IDisposable Register<TInterface>(this IServerHub server, Func<TInterface> factory)
            where TInterface : class
        {
            return server.Register(typeof(TInterface), factory);
        }

        public static IDisposable Register(this IServerHub server, Type interfaceType, Type classType)
        {
            // TODO: Add defensive code.

            return classType.ContainsGenericParameters
                ? server.Register(interfaceType, genericTypes =>
                {
                    var constructedClassType = classType.MakeGenericType(genericTypes);
                    return Activator.CreateInstance(constructedClassType);
                })
                : server.Register(interfaceType, () => Activator.CreateInstance(classType));
        }

        internal static Type GetClientType(this Type interfaceType)
        {
            return GetAutoGeneratedType(interfaceType, "ClientProxy");
        }

        internal static Type GetDispatcherType(this Type interfaceType)
        {
            return GetAutoGeneratedType(interfaceType, "Dispatcher");
        }

        internal static Type GetServerProxyType(this Type interfaceType)
        {
            return GetAutoGeneratedType(interfaceType, "ServerProxy");
        }

        private static Type GetAutoGeneratedType(Type interfaceType, string category)
        {
            string typeName;
            string prefix = category;

            if (interfaceType.IsNested)
            {
                string className = $"{prefix}{interfaceType.DeclaringType!.Name}{interfaceType.Name}";
                typeName = interfaceType.AssemblyQualifiedName!.Replace(
                    $"{interfaceType.DeclaringType.FullName}+{interfaceType.Name}",
                    $"{interfaceType.Namespace}.{className}");
            }
            else
            {
                string className = $"{prefix}{interfaceType.Name}";

                if (interfaceType.Namespace == null)
                {
                    className = $"{className}.{className}";
                }

                typeName = interfaceType.AssemblyQualifiedName!.Replace(interfaceType.Name, className);
            }

            return Type.GetType(typeName) ??
                   throw new InvalidOperationException($"{interfaceType.Name} doesn't look like a Ipc interface. ");
        }
    }
}
