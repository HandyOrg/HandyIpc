using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace HandyIpc.Core
{
    public class NotifierManager
    {
        private readonly object _locker = new();
        private readonly ISerializer _serializer;
        private readonly ConcurrentDictionary<string, Notifier> _notifiers = new();

        public NotifierManager(ISerializer serializer)
        {
            _serializer = serializer;
        }

        public void Publish<T>(string name, T args)
        {
            if (_notifiers.TryGetValue(name, out Notifier notifier))
            {
                notifier.Push(_serializer.Serialize(args));
            }
        }

        public void Subscribe(string name, int processId, IConnection connection)
        {
            Notifier notifier = _notifiers.GetOrAdd(name, _ => new Notifier());
            notifier.Subscribe(processId, connection);
        }

        public void Unsubscribe(string name, int processId)
        {
            if (_notifiers.TryGetValue(name, out Notifier notifier))
            {
                notifier.Unsubscribe(processId);
            }
        }

        private class Notifier
        {
            private readonly ConcurrentDictionary<int, IConnection> _connections = new();
            private readonly BlockingCollection<byte[]> _queue = new();

            private CancellationTokenSource? _source;

            public Notifier() => Start();

            public void Push(byte[] bytes)
            {
                if (_connections.IsEmpty)
                {
                    _source?.Cancel();
                    _source = null;
                    return;
                }

                if (_source is null or { IsCancellationRequested: true })
                {
                    Start();
                }

                _queue.Add(bytes);
            }

            public void Subscribe(int processId, IConnection connection)
            {
                _connections[processId] = connection;
            }

            public void Unsubscribe(int processId)
            {
                if (_connections.TryRemove(processId, out IConnection connection))
                {
                    connection.Dispose();
                }
            }

            private void Start()
            {
                while (_queue.TryTake(out _))
                {
                    // Clear history queue.
                }

                _source = new CancellationTokenSource();
                Task.Run(() => Publish(_source.Token));
            }

            private void Publish(CancellationToken token)
            {
                while (!token.IsCancellationRequested)
                {
                    byte[] bytes = _queue.Take(token);
                    var connections = _connections.ToArray();
                    foreach (var item in connections)
                    {
                        int processId = item.Key;
                        IConnection connection = item.Value;

                        try
                        {
                            byte[] result = connection.Invoke(bytes);
                            if (!result.IsUnit())
                            {
                                Unsubscribe(processId);
                            }
                        }
                        catch
                        {
                            Unsubscribe(processId);
                        }
                    }
                }
            }
        }
    }
}
