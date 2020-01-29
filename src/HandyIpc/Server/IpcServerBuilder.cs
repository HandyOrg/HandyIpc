using System;
using System.Collections.Generic;

namespace HandyIpc.Server
{
    public class IpcServerBuilder
    {
        public static IpcServerBuilder Create() => new IpcServerBuilder();

        private readonly Dictionary<Type, Func<object>> _serverFactories = new Dictionary<Type, Func<object>>();
        private readonly Dictionary<Type, Func<Type[], object>> _genericServerFactories = new Dictionary<Type, Func<Type[], object>>();

        internal IReadOnlyDictionary<Type, Func<object>> ServerFactories => _serverFactories;

        internal IReadOnlyDictionary<Type, Func<Type[], object>> GenericServerFactories => _genericServerFactories;

        internal Action<IpcSettings> IpcConfigure { get; private set; }

        public bool IsImmutable { get; private set; }

        private IpcServerBuilder() { }

        public IpcServerBuilder Configure(Action<IpcSettings> settings)
        {
            Guards.ThrowIfInvalid(!IsImmutable, "The builder has been built and cannot be changed.");
            Guards.ThrowIfNull(settings, nameof(settings));

            var previous = IpcConfigure;
            IpcConfigure = s =>
            {
                previous?.Invoke(s);
                settings(s);
            };

            return this;
        }

        public IpcServerBuilder Register(Type interfaceType, Func<object> factory)
        {
            Guards.ThrowIfInvalid(!IsImmutable, "The builder has been built and cannot be changed.");
            Guards.ThrowIfNot(interfaceType.IsInterface, "The argument must be an interface type.", nameof(interfaceType));
            Guards.ThrowIfNot(!ServerFactories.ContainsKey(interfaceType), "Duplicate interface types can not be added.", nameof(factory));

            _serverFactories[interfaceType] = factory;

            return this;
        }

        public IpcServerBuilder Register(Type interfaceType, Func<Type[], object> factory)
        {
            Guards.ThrowIfInvalid(!IsImmutable, "The builder has been built and cannot be changed.");
            Guards.ThrowIfNot(interfaceType.IsInterface, "The argument must be an interface type.", nameof(interfaceType));
            Guards.ThrowIfNot(interfaceType.ContainsGenericParameters, "Expect a generic interface here.", nameof(interfaceType));
            Guards.ThrowIfNot(!ServerFactories.ContainsKey(interfaceType), "Duplicate interface types can not be added.", nameof(factory));

            _genericServerFactories[interfaceType] = factory;

            return this;
        }

        public IpcServer Build()
        {
            IsImmutable = true;
            return new IpcServer(this);
        }
    }
}
