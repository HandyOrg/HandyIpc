using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace HandyIpc.NamedPipe
{
    internal class ClientConnectionPool
    {
        private readonly ConcurrentDictionary<string, ConcurrentBag<(Action dispose, RemoteInvokeAsync invoke)>> _poolAsync = new();
        private readonly ConcurrentDictionary<string, ConcurrentBag<(Action dispose, RemoteInvoke invoke)>> _pool = new();

        public DisposableValue<RemoteInvoke> Rent(string pipeName)
        {
            var item = TakeOrCreateItem(pipeName);
            return new DisposableValue<RemoteInvoke>(
                item.invoke,
                _ => AddItem(pipeName, item));
        }

        public async Task<AsyncDisposableValue<RemoteInvokeAsync>> RentAsync(string pipeName)
        {
            var item = await TakeOrCreateItemAsync(pipeName);
            return new AsyncDisposableValue<RemoteInvokeAsync>(
                item.invoke,
                _ => AddItemAsync(pipeName, item));
        }

        private (Action dispose, RemoteInvoke invoke) TakeOrCreateItem(string pipeName)
        {
            var bag = GetBagFromSyncPool(pipeName);

            (Action dispose, RemoteInvoke invoke) result;
            while (bag.IsEmpty || !bag.TryTake(out result) || !CheckItem(result))
            {
                bag.Add(PrimitiveMethods.CreateClient(pipeName));
            }

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
                _ => new ConcurrentBag<(Action dispose, RemoteInvoke invoke)>());
        }

        private ConcurrentBag<(Action dispose, RemoteInvokeAsync invoke)> GetBagFromAsyncPool(string pipeName)
        {
            return _poolAsync.GetOrAdd(pipeName,
                _ => new ConcurrentBag<(Action dispose, RemoteInvokeAsync invoke)>());
        }

        private static bool CheckItem((Action dispose, RemoteInvoke invoke) item)
        {
            try
            {
                var response = item.invoke(Messages.Empty);
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
                var response = await item.invoke(Messages.Empty, CancellationToken.None);
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
