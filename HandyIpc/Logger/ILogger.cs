namespace HandyIpc.Logger
{
    public interface ILogger
    {
        LogLevel EnabledLevel { get; set; }

        void Log(LogLevel level, LogInfo info);
    }
}
