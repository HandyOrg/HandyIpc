namespace HandyIpc
{
    public interface IServerBuilder : IConfiguration, IServerRegistry
    {
        IServer Build();
    }
}
