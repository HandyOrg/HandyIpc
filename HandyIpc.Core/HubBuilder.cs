using System;
using HandyIpc.Core;

namespace HandyIpc
{
    internal class HubBuilder : IHubBuilder
    {
        private Func<ReceiverBase> _receiverFactory = () => throw new InvalidOperationException(
            $"Must invoke the IHubBuilder<TRmi, THub>.Use(Func<{nameof(ReceiverBase)}> factory) method " +
            "to register a factory before invoking the Build method.");
        private Func<SenderBase> _senderFactory = () => throw new InvalidOperationException(
            $"Must invoke the IHubBuilder<TRmi, THub>.Use(Func<{nameof(SenderBase)}> factory) method " +
            "to register a factory before invoking the Build method.");
        private Func<ISerializer> _serializerFactory = () => throw new InvalidOperationException(
            $"Must invoke the IHubBuilder<TRmi, THub>.Use(Func<{nameof(ISerializer)}> factory) method " +
            "to register a factory before invoking the Build method.");
        private Func<ILogger> _loggerFactory = () => new DebugLogger();

        public IHubBuilder Use(Func<SenderBase> factory)
        {
            _senderFactory = factory;
            return this;
        }

        public IHubBuilder Use(Func<ReceiverBase> factory)
        {
            _receiverFactory = factory;
            return this;
        }

        public IHubBuilder Use(Func<ILogger> factory)
        {
            _loggerFactory = factory;
            return this;
        }

        public IHubBuilder Use(Func<ISerializer> factory)
        {
            _serializerFactory = factory;
            return this;
        }

        public IClientHub BuildClientHub()
        {
            SenderBase sender = _senderFactory();
            sender.SetLogger(_loggerFactory());
            return new ClientHub(sender, _serializerFactory());
        }

        public IServerHub BuildServerHub()
        {
            ReceiverBase receiver = _receiverFactory();
            ILogger logger = _loggerFactory();
            receiver.SetLogger(logger);
            return new ServerHub(receiver, _serializerFactory(), logger);
        }
    }
}
