using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace HandyIpc.Client
{
    public class ClientPool
    {
        public static ClientPool Shared { get; } = new ClientPool();

        private ClientPool() { }

        private readonly ConcurrentDictionary<string, ConcurrentBag<(Action dispose, RemoteInvokeAsync invoke)>> _poolAsync =
            new ConcurrentDictionary<string, ConcurrentBag<(Action dispose, RemoteInvokeAsync invoke)>>();
        private readonly ConcurrentDictionary<string, ConcurrentBag<(Action dispose, RemoteInvoke invoke)>> _pool =
            new ConcurrentDictionary<string, ConcurrentBag<(Action dispose, RemoteInvoke invoke)>>();

        public DisposableValue<RemoteInvoke> Rent(string pipeName)
        {
            var item = TakeOrCreateItem(pipeName);
            return new DisposableValue<RemoteInvoke>(
                item.invoke,
                value => AddItem(pipeName, item));
        }

        public async Task<AsyncDisposableValue<RemoteInvokeAsync>> RentAsync(string pipeName)
        {
            var item = await TakeOrCreateItemAsync(pipeName);
            return new AsyncDisposableValue<RemoteInvokeAsync>(
                item.invoke,
                value => AddItemAsync(pipeName, item));
        }

        private (Action dispose, RemoteInvoke invoke) TakeOrCreateItem(string pipeName)
        {
            var bag = GetBagFromSyncPool(pipeName);

            (Action dispose, RemoteInvoke invoke) result;
            while (bag.IsEmpty || !bag.TryTake(out result) || !CheckItem(result))
            {
                bag.Add(PrimitiveMethods.CreateClient(pipeName));
            }

            Guards.ThrowIfNull(result.dispose, nameof(result.dispose));
            Guards.ThrowIfNull(result.invoke, nameof(result.invoke));

            return result;
        }

        private async Task<(Action dispose, RemoteInvokeAsync invoke)> TakeOrCreateItemAsync(string pipeName)
        {
            var bag = GetBagFromAsyncPool(pipeName);

            (Action dispose, RemoteInvokeAsync invoke) result;
            while (bag.IsEmpty || !bag.TryTake(out result) || !await CheckItemAsync(result))
            {
                bag.Add(await PrimitiveMethods.CreateClientAsync(pipeName));
            }

            Guards.ThrowIfNull(result.dispose, nameof(result.dispose));
            Guards.ThrowIfNull(result.invoke, nameof(result.invoke));

            return result;
        }

        private void AddItem(string pipeName, (Action dispose, RemoteInvoke invoke) item)
        {
            if (CheckItem(item))
            {
                GetBagFromSyncPool(pipeName).Add(item);
            }
        }

        private async Task AddItemAsync(string pipeName, (Action dispose, RemoteInvokeAsync invoke) item)
        {
            if (await CheckItemAsync(item))
            {
                GetBagFromAsyncPool(pipeName).Add(item);
            }
        }

        private ConcurrentBag<(Action dispose, RemoteInvoke invoke)> GetBagFromSyncPool(string pipeName)
        {
            return _pool.GetOrAdd(pipeName,
                key => new ConcurrentBag<(Action dispose, RemoteInvoke invoke)>());
        }

        private ConcurrentBag<(Action dispose, RemoteInvokeAsync invoke)> GetBagFromAsyncPool(string pipeName)
        {
            return _poolAsync.GetOrAdd(pipeName,
                key => new ConcurrentBag<(Action dispose, RemoteInvokeAsync invoke)>());
        }

        private static bool CheckItem((Action dispose, RemoteInvoke invoke) item)
        {
            try
            {
                var response = item.invoke(DataConstants.Empty);
                return response.IsEmpty();
            }
            catch
            {
                item.dispose();
                return false;
            }
        }

        private static async Task<bool> CheckItemAsync((Action dispose, RemoteInvokeAsync invoke) item)
        {
            try
            {
                var response = await item.invoke(DataConstants.Empty, CancellationToken.None);
                return response.IsEmpty();
            }
            catch
            {
                item.dispose();
                return false;
            }
        }
    }
}
