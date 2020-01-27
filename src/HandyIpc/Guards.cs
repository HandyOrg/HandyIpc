using System;

namespace HandyIpc
{
    public static class Guards
    {
        public static void ThrowIfNull<T>(T value, string paramName) where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(paramName, "This value can not be null.");
            }
        }

        public static void ThrowIfNot(bool value, string message, string paramName)
        {
            if (!value)
            {
                throw new ArgumentException(message, paramName);
            }
        }

        public static void ThrowIfInvalid(bool value, string message)
        {
            if (!value)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
