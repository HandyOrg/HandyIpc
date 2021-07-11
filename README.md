# HandyIpc

This library provides a high-level RMI (remote method invocation) API. Its underlying communication can be implemented by whatever you like, such as Named Pipe, MMF (memory mapping file) or Socket, and this framework does not care about the specific implementation.

The design of this library API was inspired by [orleans](https://github.com/dotnet/orleans), and and its implementation refers to [refit](https://github.com/reactiveui/refit).

## NuGet

| Package                    | NuGet                                                                                                                    |
| -------------------------- | ------------------------------------------------------------------------------------------------------------------------ |
| `HandyIpc.Core`            | [![version](https://img.shields.io/badge/version-0.3.1-orange)](https://www.nuget.org/packages/HandyIpc.Core)            |
| `HandyIpc.NamedPipe`       | [![version](https://img.shields.io/badge/version-0.1.1-orange)](https://www.nuget.org/packages/HandyIpc.NamedPipe)       |
| `HandyIpc.Serializer.Json` | [![version](https://img.shields.io/badge/version-0.1.1-orange)](https://www.nuget.org/packages/HandyIpc.Serializer.Json) |

## How to use

#### 1. Define IPC contract

```csharp
    // Declare an interface contains a set of methods that needs to be called remotely,
    // and mark it with IpcContractAttribute.
    [IpcContract]
    public interface IDemo<T>
    {
        double Add(double x, double y);
        Task<T> GetDefaultAsync();
        string GenericMethod<T1, T2>(IEnumerable<T1> items, T2 arg0, T arg1);
    }
```

#### 2. Implement, register IPC contract in server side

```csharp
    // Implement the IPC contract (interface).
    public class Demo<T> : IDemo<T>
    {
        public double Add(double x, double y) => x + y;

        public Task<T> GetDefaultAsync() => Task.FromResult<T>(default);

        string IDemo<T>.GenericMethod<T1, T2>(IEnumerable<T1> items, T2 arg0, T arg1)
        {
            return $"T1={typeof(T1)}, T2={typeof(T2)}";
        }
    }
```

```csharp
    // Create a server hub and enable requested interface services.
    IIpcServerHub server = HandyIpcHub
        .CreateServerFactory()
        .UseJsonSerializer()
        .UseNamedPipe()
        .Build();

    server.Start(typeof(IDemo<>), typeof(Demo<>));
    server.Start<IOther, OtherImpl>();
```

#### 3. Invoke remote methods in client side

```csharp
    // Create a client hub and resolve requested interface proxy.
    IIpcClientHub client = HandyIpcHub
        .CreateClientFactory()
        .UseJsonSerializer()
        .UseNamedPipe()
        .Build();

    var demo1 = client.Of<IDemo<string>>();
    var demo2 = client.Of<IDemo<int>>();

    var result0 = demo1.Add(16, 26); // 42
    var result1 = await demo1.GetDefaultAsync(); // null
    var result2 = await demo2.GetDefaultAsync(); // 0
    var result3 = demo1.GenericMethod<string, int>(null, 0, null); // T1=System.String, T2=System.Int32
```

## TODO List

1. [x] Support for generic interface.
2. [x] Support for `Task/Task<T>` return value in interface method.
3. [x] Support for generic methods (parameter type allow contains nested generic types).
4. [ ] NOT support for interface inheritance.
