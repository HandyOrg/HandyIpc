using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HandyIpc.Core
{
    public class NotifierManager
    {
        private readonly object _locker = new();
        private readonly string _key = Guid.NewGuid().ToString();
        private readonly ISerializer _serializer;
        private readonly Dictionary<string, Notifier> _notifiers = new();

        public NotifierManager(ISerializer serializer)
        {
            _serializer = serializer;
        }

        public void Publish<T>(string name, T e)
        {
            lock (_locker)
            {
                if (_notifiers.TryGetValue(name, out Notifier notifier))
                {
                    notifier.Push(_serializer.Serialize(e, typeof(T)));
                }
            }
        }

        public void Subscribe(string name, int processId, IConnection connection)
        {
            lock (_locker)
            {
                if (!_notifiers.ContainsKey(name))
                {
                    _notifiers.Add(name, new Notifier());
                }

                var notifier = _notifiers[name];
                notifier.Subscribe(processId, connection);
            }
        }

        public void Unsubscribe(string name, int processId)
        {
            lock (_locker)
            {
                if (_notifiers.TryGetValue(_key, out Notifier notifier))
                {
                    notifier.Unsubscribe(processId);
                }
            }
        }

        private class Notifier
        {
            private readonly BlockingCollection<byte[]> _queue = new();
            private readonly ConcurrentDictionary<int, IConnection> _connections = new();

            private CancellationTokenSource? _source;

            public void Push(byte[] e)
            {
                _queue.Add(e);
            }

            public void Subscribe(int porcessId, IConnection connection)
            {
                if (_connections.Count == 0)
                {
                    _source?.Cancel();
                    _source = new CancellationTokenSource();
                    Task.Run(() => Consume(_source.Token));
                }

                _connections.TryAdd(porcessId, connection);
            }

            public void Unsubscribe(int processId)
            {
                _connections.TryRemove(processId, out _);

                if (_connections.Count == 0)
                {
                    _source?.Cancel();
                    _source = null;
                }
            }

            private async Task Consume(CancellationToken token)
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        byte[] e = _queue.Take(token);
                        foreach (var item in _connections)
                        {
                            int processId = item.Key;
                            IConnection connection = item.Value;

                            try
                            {
                                await connection.WriteAsync(e, token);
                                byte[] result = await connection.ReadAsync(token);
                                if (!result.IsUnit())
                                {
                                    // TODO: Handle exception.
                                }
                            }
                            catch
                            {
                                Unsubscribe(processId);
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        // ignored
                    }
                }
            }
        }
    }
}
