// ReSharper disable once CheckNamespace
namespace HandyIpc.Server
{
    public static class ContextExtensions
    {
        public static T Get<T>(this Context context, string key = default)
        {
            return (T)context.Items[string.IsNullOrEmpty(key) ? nameof(T) : key];
        }

        public static void Set<T>(this Context context, T value, string key = default)
        {
            context.Items[string.IsNullOrEmpty(key) ? nameof(T) : key] = value;
        }
    }
}
