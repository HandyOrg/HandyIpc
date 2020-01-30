using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HandyIpc;

// ReSharper disable once CheckNamespace
namespace System
{
    public static class ObjectExtensions
    {
        public static T CastTo<T>(this object value)
        {
            return value != null && typeof(T).IsValueType
                ? (T)Convert.ChangeType(value, typeof(T))
                : value is T typeValue ? typeValue : default;
        }

        public static object CastTo(this object value, Type targetType)
        {
            return value != null && targetType.IsValueType
                ? Convert.ChangeType(value, targetType)
                : targetType.IsInstanceOfType(value) ? value : default;
        }

        public static object[] CastTo(this object[] values, Type[] targetTypes)
        {
            Guards.ThrowIfNot(values.Length == targetTypes.Length, "The number of target types does not match the number of values.", nameof(targetTypes));

            return values.Select((item, i) => item.CastTo(targetTypes[i])).ToArray();
        }

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
