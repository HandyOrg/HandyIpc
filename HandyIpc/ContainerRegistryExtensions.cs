using System;

namespace HandyIpc
{
    public static class ContainerRegistryExtensions
    {
        public static IContainerRegistry Register<TInterface, TImpl>(this IContainerRegistry registry, string? key = null)
            where TInterface : class
            where TImpl : TInterface, new()
        {
            return registry.Register<TInterface>(() => new TImpl(), key);
        }

        public static IContainerRegistry Register<TInterface>(this IContainerRegistry registry, Func<TInterface> factory, string? key = null)
            where TInterface : class
        {
            key ??= typeof(TInterface).GetDefaultKey();
            return registry.Register(typeof(TInterface), factory, key);
        }

        public static IContainerRegistry Register(this IContainerRegistry registry, Type interfaceType, Type classType, string? key = null)
        {
            key ??= interfaceType.GetDefaultKey();
            return classType.ContainsGenericParameters
                ? registry.Register(interfaceType, GenericFactory, key)
                : registry.Register(interfaceType, () => Activator.CreateInstance(classType), key);

            // Local Method
            object GenericFactory(Type[] genericTypes)
            {
                var constructedClassType = classType.MakeGenericType(genericTypes);
                return Activator.CreateInstance(constructedClassType);
            }
        }
    }
}
