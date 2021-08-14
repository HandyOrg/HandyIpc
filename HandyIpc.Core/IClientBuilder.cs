namespace HandyIpc
{
    public interface IClientBuilder : IConfiguration
    {
        IClient Build();
    }
}
