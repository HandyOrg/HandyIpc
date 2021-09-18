using System;

namespace HandyIpc
{
    internal static class Guards
    {
        public static void ThrowArgument(bool value, string message, string paramName)
        {
            if (!value)
            {
                throw new ArgumentException(message, paramName);
            }
        }
    }
}
