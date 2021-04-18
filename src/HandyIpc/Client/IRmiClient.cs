﻿using System.Threading;
using System.Threading.Tasks;

namespace HandyIpc.Client
{
    public interface IRmiClient
    {
        T Invoke<T>(string pipeName, Request request);

        Task<T> InvokeAsync<T>(string pipeName, Request request);

        Task<T> InvokeAsync<T>(string pipeName, Request request, CancellationToken token);
    }
}
