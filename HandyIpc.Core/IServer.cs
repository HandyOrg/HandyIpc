using System;

namespace HandyIpc
{
    /// <summary>
    /// It represents a IPC server instances.
    /// </summary>
    public interface IServer : IDisposable
    {
        bool IsRunning { get; }

        void Start();

        void Stop();
    }
}
