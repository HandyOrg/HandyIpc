using System;
using System.Collections.Generic;
using System.Linq;

namespace HandyIpc.Server
{
    public class IpcServerCollection
    {
        private readonly IReadOnlyCollection<Type> _runningInterfaces;

        internal IDictionary<Type, Func<object>> NonGenericInterfaces { get; } =
            new Dictionary<Type, Func<object>>();

        internal IDictionary<Type, Func<Type[], object>> GenericInterfaces { get; } =
            new Dictionary<Type, Func<Type[], object>>();

        internal ICollection<Type> ToBeRemovedInterfaces { get; } =
            new HashSet<Type>();

        internal IpcServerCollection(IReadOnlyCollection<Type> runningInterfaces)
        {
            _runningInterfaces = runningInterfaces;
        }

        public IpcServerCollection Add(Type interfaceType, Func<object> factory)
        {
            Guards.ThrowIfNot(interfaceType.IsInterface, "The argument must be an interface type.", nameof(interfaceType));
            Guards.ThrowIfNot(!interfaceType.IsGenericType, "Expect a non-generic interface here.", nameof(interfaceType));
            Guards.ThrowIfNot(
                !_runningInterfaces.Contains(interfaceType) || ToBeRemovedInterfaces.Contains(interfaceType),
                $"This interface ({interfaceType}) is running, if you want to add it again, please remove it first.",
                nameof(interfaceType));

            NonGenericInterfaces[interfaceType] = factory;

            return this;
        }

        public IpcServerCollection Add(Type interfaceType, Func<Type[], object> factory)
        {
            Guards.ThrowIfNot(interfaceType.IsInterface, "The argument must be an interface type.", nameof(interfaceType));
            Guards.ThrowIfNot(interfaceType.IsGenericType, "Expect a generic interface here.", nameof(interfaceType));
            Guards.ThrowIfNot(
                !_runningInterfaces.Contains(interfaceType) || ToBeRemovedInterfaces.Contains(interfaceType),
                $"This interface ({interfaceType}) is running, if you want to add it again, please remove it first.",
                nameof(interfaceType));

            GenericInterfaces[interfaceType] = factory;

            return this;
        }

        public IpcServerCollection Remove(Type interfaceType)
        {
            Guards.ThrowIfNot(interfaceType.IsInterface, "The argument must be an interface type.", nameof(interfaceType));

            if (!NonGenericInterfaces.Remove(interfaceType) &&
                !GenericInterfaces.Remove(interfaceType))
            {
                ToBeRemovedInterfaces.Add(interfaceType);
            }

            return this;
        }
    }
}
