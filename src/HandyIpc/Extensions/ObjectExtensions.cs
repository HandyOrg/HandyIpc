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
    }
}
