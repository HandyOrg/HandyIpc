using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace HandyIpc.Core
{
    public class AwaiterManager
    {
        private readonly ConcurrentDictionary<string, Awaiter> _pool = new();
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
            Awaiter awaiter = _pool.GetOrAdd(name, _ => new Awaiter(callback, connection));

            byte[] addResult = connection.Invoke(Subscription.Add(_key, name, _serializer));
            if (!addResult.IsUnit())
            {
                // TODO: Use exact exception.
                throw new InvalidOperationException();
            }

            Task.Run(() => LoopWait(awaiter));
        }

        public void Unsubscribe(string name)
        {
            if (_pool.TryRemove(name, out _))
            {
                byte[] removeResult = _sender.Invoke(Subscription.Remove(_key, name, _serializer));
                if (!removeResult.IsUnit())
                {
                    // TODO: Logging.
                }
            }
        }

        private static void LoopWait(Awaiter awaiter)
        {
            using IConnection connection = awaiter.Connection;
            while (true)
            {
                // Will blocked until accepted a notification.
                byte[] input = connection.Read();
                if (input.IsEmpty())
                {
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

            public Awaiter(Action<byte[]> handler, IConnection connection)
            {
                Handler = handler;
                Connection = connection;
            }
        }
    }
}
