using System.Collections.Generic;

namespace HandyIpc.Core
{
    public class NotifierManager
    {
        private readonly object _locker = new();
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
                    notifier.Publish(_serializer.Serialize(e, typeof(T)));
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
                if (_notifiers.TryGetValue(name, out Notifier notifier))
                {
                    notifier.Unsubscribe(processId);
                }
            }
        }

        private class Notifier
        {
            private readonly object _locker = new();
            private readonly Dictionary<int, IConnection> _connections = new();

            public void Publish(byte[] e)
            {
                lock (_locker)
                {
                    foreach (var item in _connections)
                    {
                        int processId = item.Key;
                        IConnection connection = item.Value;

                        try
                        {
                            byte[] result = connection.Invoke(e);
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
            }

            public void Subscribe(int processId, IConnection connection)
            {
                lock (_locker)
                {
                    _connections[processId] = connection;
                }
            }

            public void Unsubscribe(int processId)
            {
                lock (_locker)
                {
                    if (_connections.TryGetValue(processId, out IConnection connection))
                    {
                        _connections.Remove(processId);
                        // Send a signal to notify end this connection.
                        connection.Write(Signals.Empty);
                    }
                }
            }
        }
    }
}
