using System;

namespace HandyIpc
{
    /// <summary>
    /// It represents a IPC server instances.
    /// </summary>
    public interface IContainerServer : IDisposable
    {
        bool IsRunning { get; }

        void Start();

        void Stop();
    }
}
