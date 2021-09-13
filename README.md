# HandyIpc

English | [中文](./README.zh-CN.md)

This library provides a high-level RMI (remote method invocation) API. Its underlying communication can be implemented by whatever you like, such as Named Pipe, MMF (memory mapping file) or Socket, and this framework does not care about the specific implementation.

## NuGet

| Package                    | Description                                                                        | NuGet                                                                                                                    |
| -------------------------- | ---------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------ |
| `HandyIpc`                 | The core library that provides the high-level APIs required for IPC communication. | [![version](https://img.shields.io/badge/version-0.5.0-orange)](https://www.nuget.org/packages/HandyIpc)                 |
| `HandyIpc.NamedPipe`       | Provides NamedPipe-based IPC.                                                      | [![version](https://img.shields.io/badge/version-0.5.0-orange)](https://www.nuget.org/packages/HandyIpc.NamedPipe)       |
| `HandyIpc.Socket`          | Provides Socket-based IPC, and only tcp protocol is currently supported.           | [![version](https://img.shields.io/badge/version-0.5.0-orange)](https://www.nuget.org/packages/HandyIpc.Socket)          |
| `HandyIpc.Serializer.Json` | Provides json serialization as the protocol for IPC.                               | [![version](https://img.shields.io/badge/version-0.5.0-orange)](https://www.nuget.org/packages/HandyIpc.Serializer.Json) |

## Install Packages

Add the following 3 packages to your project.

Note: `HandyIpc.NamedPipe` and `HandyIpc.Socket` only need to install one, it is recommended to choose `HandyIpc.NamedPipe`, it is faster than `HandyIpc.Socket`.

```
<PackageReference Include="HandyIpc" Version="0.5.0" />
<PackageReference Include="HandyIpc.NamedPipe" Version="0.5.0" />
<PackageReference Include="HandyIpc.Serializer.Json" Version="0.5.0" />
```

## How to use

#### 1. Define IPC contract

```csharp
// Declare an interface contains a set of methods that needs to be called remotely,
// and mark it with IpcContractAttribute.
[IpcContract]
// Feature: Supports generic interfaces.
public interface IDemo<T>
{
    Task<T> GetDefaultAsync();
    double Add(double x, double y);
    // Feature: Supports Task/Task<T> async methods.
    Task<double> AddAsync(double x, double y);
    // Feature: Supports generic methdos.
    string GetTypeName<T>();
}
```

#### 2. Implement, register IPC contract in server side

```csharp
// Implement the IPC contract (interface).
public class Demo<T> : IDemo<T>
{
    public Task<T> GetDefaultAsync() => Task.FromResult<T>(default);
    public double Add(double x, double y) => x + y;
    public Task<double> AddAsync(double x, double y) => Task.FromResult(x + y);
    public string GetTypeName<T>() => typeof(T).Name;
}
```

```csharp
// Create a ContainerServerBuilder instance to build the IContainerServer instance.
ContainerServerBuilder serverBuilder = new();
serverBuilder
    //.UseTcp(IPAddress.Loopback, 10086)
    .UseNamedPipe("ec57043f-465c-4766-ae49-b9b1ee9ac571")
    .UseJsonSerializer();

serverBuilder
    // Generic interfaces that are not monomorphic need to be registered in this form.
    .Register(typeof(IDemo<>), typeof(Demo<>))
    // Non-generic interfaces or generic interfaces that are already monomorphic can use more elegant extension methods.
    .Register<IDemo<string>, Demo<string>>()
    .Register<ICalculator, Calculator>();

using var server = serverBuilder.Build();
// Don't forget to start the server.
server.Start();

// server.Stop();
```

#### 3. Invoke remote methods in client side

```csharp
// Create a ContainerClientBuilder instance to build the IContainerClient instance.
ContainerClientBuilder clientBuilder = new();
clientBuilder
    //.UseTcp(IPAddress.Loopback, 10086)
    .UseNamedPipe("ec57043f-465c-4766-ae49-b9b1ee9ac571")
    .UseJsonSerializer();
using var client = clientBuilder.Build();

// Resolve contract instances from the client instance.
var demo1 = client.Resolve<IDemo<string>>();
var demo2 = client.Resolve<IDemo<int>>();

// Using contract instances, they will call an implementation from another process.
var result0 = demo1.Add(16, 26); // 42
var result1 = await demo1.AddAsync(40, 2); // 42
var result2 = demo1.GetTypeName<string>(); // "String"

var result3 = await demo1.GetDefaultAsync(); // null
var result3 = await demo2.GetDefaultAsync(); // 0
```

## TODO List

1. [x] Support for generic interface.
2. [x] Support for `Task/Task<T>` return value in interface method.
3. [x] Support for generic methods (parameter type allow contains nested generic types).
4. [x] NOT support for interface inheritance.
