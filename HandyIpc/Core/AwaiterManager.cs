using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
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

        public void Subscribe(string name, int handlerId, Action<byte[]> callback)
        {
            Awaiter awaiter = _pool.GetOrAdd(name, _ => new Awaiter());
            lock (awaiter.Locker)
            {
                if (awaiter.Handlers.Count == 0)
                {
                    RentedValue<IConnection> rented = _sender.ConnectionPool.Rent();
                    IConnection connection = rented.Value;
                    connection.Write(Subscription.Add(_key, name, _serializer));
                    byte[] addResult = connection.Read();
                    if (!addResult.IsUnit())
                    {
                        // TODO: Use exact exception.
                        throw new InvalidOperationException();
                    }

                    Task.Run(() => LoopWait(rented, name, awaiter, awaiter.Source.Token));
                }

                awaiter.Handlers[handlerId] = callback;
            }
        }

        public void Unsubscribe(string name, int handlerId)
        {
            if (!_pool.TryGetValue(name, out Awaiter awaiter))
            {
                return;
            }

            lock (awaiter.Locker)
            {
                awaiter.Handlers.Remove(handlerId);
                if (awaiter.Handlers.Count == 0)
                {
                    _pool.TryRemove(name, out _);
                    awaiter.Source.Cancel();
                }
            }
        }

        private async Task LoopWait(RentedValue<IConnection> rented, string name, Awaiter awaiter, CancellationToken token)
        {
            using (rented)
            {
                IConnection connection = rented.Value;
                while (!token.IsCancellationRequested)
                {
                    // Will blocked until accepted a notification.
                    byte[] input = await connection.ReadAsync(token);
                    lock (awaiter.Locker)
                    {
                        foreach (var handler in awaiter.Handlers.Values)
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

        private class Awaiter
        {
            public readonly object Locker = new();

            public Dictionary<int, Action<byte[]>> Handlers { get; } = new();

            public CancellationTokenSource Source { get; } = new();
        }
    }
}
