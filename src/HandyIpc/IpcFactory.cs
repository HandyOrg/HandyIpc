using System;

namespace HandyIpc
{
    internal class IpcFactory<TRmi, THub> : IIpcFactory<TRmi, THub>
    {
        private readonly Func<TRmi, THub> _hubFactory;

        private Func<ISerializer> _serializerFactory = null!;
        private Func<ISerializer, TRmi> _rmiFactory = _ => throw new InvalidOperationException(
            "Must invoke the IIpcFactory<TRmi, THub>.Use(Func<TRmi> factory) method " +
            "to register a factory before invoking the Build method.");

        public IpcFactory(Func<TRmi, THub> hubFactory) => _hubFactory = hubFactory;

        public IIpcFactory<TRmi, THub> Use(Func<ISerializer> factory)
        {
            _serializerFactory = factory;
            return this;
        }

        public IIpcFactory<TRmi, THub> Use(Func<ISerializer, TRmi> factory)
        {
            _rmiFactory = factory;
            return this;
        }

        public THub Build() => _hubFactory(_rmiFactory(_serializerFactory()));
    }
}
