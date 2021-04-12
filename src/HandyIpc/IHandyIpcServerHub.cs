﻿using System;

namespace HandyIpc
{
    /// <summary>
    /// It represents a hub of IPC server instances.
    /// </summary>
    public interface IHandyIpcServerHub
    {
        /// <summary>
        /// Registers a mapping between the non-generic contract interface and the non-generic concrete service class,
        /// and start the service.
        /// </summary>
        /// <param name="interfaceType">The non-generic contract interface.</param>
        /// <param name="factory">The factory of the non-generic concrete service class.</param>
        /// <returns>A token to stop the running service instance.</returns>
        IDisposable Start(Type interfaceType, Func<object> factory);

        /// <summary>
        /// Registers a mapping between the generic contract interface and the generic concrete service class,
        /// and start the service.
        /// </summary>
        /// <param name="interfaceType">The generic contract interface.</param>
        /// <param name="factory">The factory of the generic concrete service class.</param>
        /// <returns>A token to stop the running service instance.</returns>
        IDisposable Start(Type interfaceType, Func<Type[], object> factory);
    }
}