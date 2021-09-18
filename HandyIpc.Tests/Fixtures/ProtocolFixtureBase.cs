using System;
using HandyIpc;
using HandyIpcTests.Implementations;
using HandyIpcTests.Interfaces;

namespace HandyIpcTests.Fixtures
{
    public abstract class ProtocolFixtureBase : IDisposable
    {
        private readonly IContainerServer _server;

        public IContainerClient Client { get; }

        protected ProtocolFixtureBase(ContainerClientBuilder clientBuilder, ContainerServerBuilder serverBuilder)
        {
            Client = clientBuilder.Build();

            serverBuilder
                .Register<IBuildInType, BuildInTypeImpl>()
                .Register(typeof(IGenericType<,>), typeof(GenericTypeImpl<,>))
                .Register<ITaskReturnType, TaskReturnTypeImpl>();

            _server = serverBuilder.Build();
            _server.Start();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Client.Dispose();

                _server.Stop();
                _server.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
