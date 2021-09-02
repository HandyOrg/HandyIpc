using System;
using HandyIpc.Core;

namespace HandyIpc
{
    public class ContainerClientBuilder : IClientConfiguration
    {
        private Func<IClient> _clientFactory = () => throw new InvalidOperationException(
            $"Must invoke the {nameof(IServerConfiguration)}.Use(Func<{nameof(IClient)}> factory) method " +
            "to register a factory before invoking the Build method.");
        private Func<ISerializer> _serializerFactory = () => throw new InvalidOperationException(
            $"Must invoke the {nameof(IServerConfiguration)}.Use(Func<{nameof(ISerializer)}> factory) method " +
            "to register a factory before invoking the Build method.");
        private Func<ILogger> _loggerFactory = () => new DebugLogger();

        public IClientConfiguration Use(Func<ISerializer> factory)
        {
            _serializerFactory = factory;
            return this;
        }

        public IClientConfiguration Use(Func<ILogger> factory)
        {
            _loggerFactory = factory;
            return this;
        }

        public IClientConfiguration Use(Func<IClient> factory)
        {
            _clientFactory = factory;
            return this;
        }

        public IContainerClient Build()
        {
            return new ContainerClient(new Sender(_clientFactory()), _serializerFactory());
        }
    }
}
