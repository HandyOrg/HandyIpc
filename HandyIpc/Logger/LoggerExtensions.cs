using System;
using System.Runtime.CompilerServices;

namespace HandyIpc.Logger
{
    public static class LoggerExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEnabled(this ILogger logger, LogLevel level)
        {
            return level <= logger.EnabledLevel;
        }

        public static void Fatal(this ILogger logger, string message, Exception? exception = null, [CallerMemberName] string? scopeName = null, int? threadId = null)
        {
            logger.Log(LogLevel.Fatal, new LogInfo(message, exception, scopeName, threadId));
        }

        public static void Error(this ILogger logger, string message, Exception? exception = null, [CallerMemberName] string? scopeName = null, int? threadId = null)
        {
            logger.Log(LogLevel.Error, new LogInfo(message, exception, scopeName, threadId));
        }

        public static void Warning(this ILogger logger, string message, Exception? exception = null, [CallerMemberName] string? scopeName = null, int? threadId = null)
        {
            logger.Log(LogLevel.Warning, new LogInfo(message, exception, scopeName, threadId));
        }

        public static void Info(this ILogger logger, string message, Exception? exception = null, [CallerMemberName] string? scopeName = null, int? threadId = null)
        {
            logger.Log(LogLevel.Info, new LogInfo(message, exception, scopeName, threadId));
        }

        public static void Debug(this ILogger logger, string message, Exception? exception = null, [CallerMemberName] string? scopeName = null, int? threadId = null)
        {
            logger.Log(LogLevel.Debug, new LogInfo(message, exception, scopeName, threadId));
        }
    }
}
