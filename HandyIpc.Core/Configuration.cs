using System;
using HandyIpc.Core;

namespace HandyIpc
{
    internal abstract class Configuration : IConfiguration
    {
        protected Func<ReceiverBase> ReceiverFactory { get; private set; } = () => throw new InvalidOperationException(
            $"Must invoke the IHubBuilder<TRmi, THub>.Use(Func<{nameof(ReceiverBase)}> factory) method " +
            "to register a factory before invoking the Build method.");

        protected Func<SenderBase> SenderFactory { get; private set; } = () => throw new InvalidOperationException(
            $"Must invoke the IHubBuilder<TRmi, THub>.Use(Func<{nameof(SenderBase)}> factory) method " +
            "to register a factory before invoking the Build method.");

        protected Func<ISerializer> SerializerFactory { get; private set; } = () => throw new InvalidOperationException(
            $"Must invoke the IHubBuilder<TRmi, THub>.Use(Func<{nameof(ISerializer)}> factory) method " +
            "to register a factory before invoking the Build method.");

        protected Func<ILogger> LoggerFactory { get; private set; } = () => new DebugLogger();

        public IConfiguration Use(Func<SenderBase> factory)
        {
            SenderFactory = factory;
            return this;
        }

        public IConfiguration Use(Func<ReceiverBase> factory)
        {
            ReceiverFactory = factory;
            return this;
        }

        public IConfiguration Use(Func<ISerializer> factory)
        {
            SerializerFactory = factory;
            return this;
        }

        public IConfiguration Use(Func<ILogger> factory)
        {
            LoggerFactory = factory;
            return this;
        }
    }
}
