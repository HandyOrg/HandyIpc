using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HandyIpc.Core
{
    public class AwaitorManager
    {
        private readonly ConcurrentDictionary<string, Awaitor> _pool = new();
        private readonly string _key;
        private readonly ISerializer _serializer;
        private readonly Sender _sender;

        public AwaitorManager(string key, Sender sender, ISerializer serializer)
        {
            _key = key;
            _sender = sender;
            _serializer = serializer;
        }

        public void Subscribe(string name, int handlerId, Action<byte[]> callback)
        {
            Awaitor awaitor = _pool.GetOrAdd(name, _ => new Awaitor());
            lock (awaitor.Locker)
            {
                if (awaitor.Handlers.Count == 0)
                {
                    RentedValue<IConnection> connection = _sender.ConnectionPool.Rent();
                    Task.Run(() => LoopWait(connection, name, awaitor, awaitor.Source.Token));
                }

                awaitor.Handlers[handlerId] = callback;
            }
        }

        public void Unsubscribe(string name, int handlerId)
        {
            if (!_pool.TryGetValue(name, out Awaitor awaitor))
            {
                return;
            }

            lock (awaitor.Locker)
            {
                awaitor.Handlers.Remove(handlerId);
                if (awaitor.Handlers.Count == 0)
                {
                    _pool.TryRemove(name, out _);
                    awaitor.Source.Cancel();
                }
            }
        }

        private async Task LoopWait(RentedValue<IConnection> rented, string name, Awaitor awaitor, CancellationToken token)
        {
            using (rented)
            {
                IConnection connection = rented.Value;
                await connection.WriteAsync(Subscription.Add(_key, name, _serializer), token);
                byte[] addResult = await connection.ReadAsync(token);
                if (!addResult.IsUnit())
                {
                    // TODO: Use exact exception.
                    throw new InvalidOperationException();
                }

                while (!token.IsCancellationRequested)
                {
                    // Will blocked until accepted a notification.
                    byte[] input = await connection.ReadAsync(token);
                    lock (awaitor.Locker)
                    {
                        foreach (var handler in awaitor.Handlers.Values)
                        {
                            try
                            {
                                handler(input);
                            }
                            catch
                            {
                                // ignored
                            }
                        }
                    }

                    await connection.WriteAsync(Signals.Unit, token);
                }

                await connection.WriteAsync(Subscription.Remove(_key, name, _serializer), token);
                byte[] removeResult = await connection.ReadAsync(token);
                if (!removeResult.IsUnit())
                {
                    // TODO: Logging.
                }
            }
        }

        private class Awaitor
        {
            public readonly object Locker = new();

            public Dictionary<int, Action<byte[]>> Handlers { get; } = new();

            public CancellationTokenSource Source { get; } = new();
        }
    }
}
