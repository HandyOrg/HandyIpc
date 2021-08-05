using System;
using HandyIpc.Core;

namespace HandyIpc
{
    internal class IpcFactory<TRmi, THub> : IIpcFactory<TRmi, THub>
    {
        private readonly Func<TRmi, ISerializer, ILogger, THub> _hubFactory;

        private Func<TRmi> _rmiFactory = () => throw new InvalidOperationException(
            $"Must invoke the IIpcFactory<TRmi, THub>.Use(Func<{nameof(TRmi)}> factory) method " +
            "to register a factory before invoking the Build method.");
        private Func<ISerializer> _serializerFactory = () => throw new InvalidOperationException(
            $"Must invoke the IIpcFactory<TRmi, THub>.Use(Func<{nameof(ISerializer)}> factory) method " +
            "to register a factory before invoking the Build method.");
        private Func<ILogger> _loggerFactory = () => new DebugLogger();

        public IpcFactory(Func<TRmi, ISerializer, ILogger, THub> hubFactory) => _hubFactory = hubFactory;

        public IIpcFactory<TRmi, THub> Use(Func<ILogger> factory)
        {
            _loggerFactory = factory;
            return this;
        }

        public IIpcFactory<TRmi, THub> Use(Func<TRmi> factory)
        {
            _rmiFactory = factory;
            return this;
        }

        public IIpcFactory<TRmi, THub> Use(Func<ISerializer> factory)
        {
            _serializerFactory = factory;
            return this;
        }

        public THub Build() => _hubFactory(_rmiFactory(), _serializerFactory(), _loggerFactory());
    }
}
