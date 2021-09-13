using System;

namespace HandyIpc
{
    public interface IContainerRegistry
    {
        /// <summary>
        /// Registers a mapping between the non-generic contract interface and the non-generic concrete service class,
        /// and start the service.
        /// </summary>
        /// <param name="interfaceType">The non-generic contract interface.</param>
        /// <param name="factory">The factory of the non-generic concrete service class.</param>
        /// <param name="key">A key for mark the instance is registered.</param>
        /// <returns>A token to stop the running service instance.</returns>
        IContainerRegistry Register(Type interfaceType, Func<object> factory, string key);

        /// <summary>
        /// Registers a mapping between the generic contract interface and the generic concrete service class,
        /// and start the service.
        /// </summary>
        /// <param name="interfaceType">The generic contract interface.</param>
        /// <param name="factory">The factory of the generic concrete service class.</param>
        /// <param name="key">A key for mark the instance is registered.</param>
        /// <returns>A token to stop the running service instance.</returns>
        IContainerRegistry Register(Type interfaceType, Func<Type[], object> factory, string key);
    }
}
