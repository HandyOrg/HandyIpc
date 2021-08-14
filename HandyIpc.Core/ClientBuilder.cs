using HandyIpc.Core;

namespace HandyIpc
{
    internal class ClientBuilder : Configuration, IClientBuilder
    {
        public IClient Build()
        {
            SenderBase sender = SenderFactory();
            sender.SetLogger(LoggerFactory());
            return new Client(sender, SerializerFactory());
        }
    }
}
