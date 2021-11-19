using System;

namespace HandyIpc.Exceptions
{
    public class IpcException : Exception
    {
        public IpcException()
        {
        }

        public IpcException(string message) : base(message)
        {
        }

        public IpcException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public class IpcProtocolException : IpcException
    {
        public IpcProtocolException()
        {
        }

        public IpcProtocolException(string message) : base(message)
        {
        }

        public IpcProtocolException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
