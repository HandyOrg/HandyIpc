using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using HandyIpc.Exceptions;

namespace HandyIpc.Core
{
    public class AwaiterManager
    {
        private readonly ConcurrentDictionary<string, Awaiter> _awaiters = new();
        private readonly string _key;
        private readonly ISerializer _serializer;
        private readonly Sender _sender;

        public AwaiterManager(string key, Sender sender, ISerializer serializer)
        {
            _key = key;
            _sender = sender;
            _serializer = serializer;
        }

        public void Subscribe(string name, Action<byte[]> callback)
        {
            IConnection connection = _sender.ConnectionPool.Rent().Value;
            Awaiter awaiter = _awaiters.GetOrAdd(name, _ => new Awaiter(callback, connection));

            byte[] addResult = connection.Invoke(Subscription.Add(_key, name, _serializer));
            if (!addResult.IsUnit())
            {
                throw new IpcException();
            }

            Task.Run(() => LoopWait(name, awaiter));
        }

        public void Unsubscribe(string name)
        {
            if (_awaiters.TryRemove(name, out Awaiter awaiter))
            {
                awaiter.Cancellation.Cancel();
                // Send a signal to dispose the remote connection.
                _sender.Invoke(Subscription.Remove(_key, name, _serializer));
            }
        }

        private void LoopWait(string name, Awaiter awaiter)
        {
            using IConnection connection = awaiter.Connection;
            using CancellationTokenSource cancellation = awaiter.Cancellation;
            CancellationToken token = cancellation.Token;

            while (!token.IsCancellationRequested)
            {
                // Will blocked until accepted a notification.
                byte[] input = connection.Read();
                if (token.IsCancellationRequested)
                {
                    break;
                }

                if (input.Length == 0)
                {
                    // The server unexpectedly closes the connection and the client should retry automatically.
                    _awaiters.TryRemove(name, out _);
                    Subscribe(name, awaiter.Handler);
                    break;
                }

                try
                {
                    // The Read() and Write() are used to ensure that calls are synchronized (blocked)
                    // and that the Write() must be called after the execution of the Handler() is completed.
                    awaiter.Handler(input);
                }
                finally
                {
                    connection.Write(Signals.Unit);
                }
            }
        }

        private class Awaiter
        {
            public Action<byte[]> Handler { get; }

            public IConnection Connection { get; }

            public CancellationTokenSource Cancellation { get; } = new();

            public Awaiter(Action<byte[]> handler, IConnection connection)
            {
                Handler = handler;
                Connection = connection;
            }
        }
    }
}
