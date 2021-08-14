namespace HandyIpc
{
    public static class HandyIpcBuilder
    {
        public static IServerBuilder CreateServerBuilder() => new ServerBuilder();

        public static IClientBuilder CreateClientBuilder() => new ClientBuilder();
    }
}
