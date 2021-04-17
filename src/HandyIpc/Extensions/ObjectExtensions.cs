using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HandyIpc;

// ReSharper disable once CheckNamespace
namespace System
{
    public static class ObjectExtensions
    {
        public static IReadOnlyDictionary<string, MethodInfo> GetGenericMethodMapping(this object instance, Type interfaceType)
        {
            return instance.GetType()
                .GetInterfaceMap(interfaceType)
                .TargetMethods
                .Where(item => item.IsGenericMethod)
                .ToDictionary(item => item.GetCustomAttribute<IpcMethodAttribute>().Identifier);
        }
    }
}
