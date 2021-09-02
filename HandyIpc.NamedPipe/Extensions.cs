namespace HandyIpc.NamedPipe
{
    public static class Extensions
    {
        public static IServerConfiguration UseNamedPipe(this IServerConfiguration self, string pipeName)
        {
            return self.Use(() => new NamedPipeIpcServer(pipeName));
        }

        public static IClientConfiguration UseNamedPipe(this IClientConfiguration self, string pipeName)
        {
            return self.Use(() => new NamedPipeIpcClient(pipeName));
        }
    }
}
