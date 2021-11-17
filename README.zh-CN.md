# HandyIpc

[English](./README.md) | 中文

HandyIpc 是一个开箱即用的进程间通讯（IPC）库，对远程方法的调用类似 [WCF](https://docs.microsoft.com/en-us/dotnet/framework/wcf/whats-wcf)，但相比之下更为轻量，免去了繁琐的配置，从入门到精通只需读完此 README。

本仓库提供了一组 High-Level 的 API 用于远程方法调用。它的底层通讯协议可以任意选择，如：Named Pipe、MMF（内存映射文件）或 Socket 等，框架本身并不关心具体的实现。

一句话概括本仓库的 API 的设计理念：一个远程的 Ioc 容器。熟悉 Ioc 容器的朋友应该了解：Ioc 容器大致分为注册对象（`Register<T, U>()`）和取用对象（`Resolve<T>()`）两个操作，而一个 IPC 库无非就是将这两个操作拆分到了两个进程中，即：在服务端注册对象，在客户端取用对象。（当然，Ioc 容器还有一个很重要的功能：自动根据依赖关系对接口类型进行赋值，本库当然没有这个功能，也完全不需要实现这一功能。）

# 功能

1. [x] 支持基础的方法和事件。
2. [x] 支持泛型接口。
3. [x] 支持泛型方法。（允许任意嵌套的泛型参数）。
4. [x] 支持以 `Task/Task<T>` 作为返回值的异步方法。
5. [x] 支持使用 NamedPipe 或 Socket 进行通讯。
6. [x] 对于不支持的情况，提供了丰富的编译时检查，详情见[这里](https://github.com/HandyOrg/HandyIpc/wiki/Diagnostic-Messages)。

## NuGet

| 包名                       | 描述                                                   | NuGet                                                                                                                    |
| -------------------------- | ------------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------ |
| `HandyIpc`                 | 核心库，提供 IPC 所需的 High-Level API.                | [![version](https://img.shields.io/badge/version-0.5.2-orange)](https://www.nuget.org/packages/HandyIpc)                 |
| `HandyIpc.NamedPipe`       | 提供基于 NamedPipe 技术的 IPC 功能。                   | [![version](https://img.shields.io/badge/version-0.5.0-orange)](https://www.nuget.org/packages/HandyIpc.NamedPipe)       |
| `HandyIpc.Socket`          | 提供基于 Socket 技术的 IPC 功能，当前仅支持 Tcp 协议。 | [![version](https://img.shields.io/badge/version-0.5.0-orange)](https://www.nuget.org/packages/HandyIpc.Socket)          |
| `HandyIpc.Serializer.Json` | 为 IPC 通讯提供 Json 序列化的支持。                    | [![version](https://img.shields.io/badge/version-0.5.0-orange)](https://www.nuget.org/packages/HandyIpc.Serializer.Json) |

## 安装

添加以下 3 个包到你的项目中。

注意: `HandyIpc.NamedPipe` 和 `HandyIpc.Socket` 仅需要安装一个即可, 如无必要，推荐使用 `HandyIpc.NamedPipe`，它会稍微比 `HandyIpc.Socket` 要快一些。

```
<PackageReference Include="HandyIpc" Version="0.5.2" />
<PackageReference Include="HandyIpc.NamedPipe" Version="0.5.0" />
<PackageReference Include="HandyIpc.Serializer.Json" Version="0.5.0" />
```

## 如何使用

#### 1. 定义 IPC 合同接口

```csharp
// 将需要远程调用的一组方法声明为一个接口，并使用 IpcContractAttribute 将其标记，该接口被称为“合同”接口。
[IpcContract]
// 特性: 支持泛型接口。
public interface IDemo<T>
{
    Task<T> GetDefaultAsync();
    double Add(double x, double y);
    // 特性：支持 Task/Task<T> 作为返回值的异步方法。
    Task<double> AddAsync(double x, double y);
    // 特性：支持泛型方法。
    string GetTypeName<T>();
}
```

#### 2. 在服务端实现并注册 IPC 合同接口

```csharp
// 实现上述的 IPC 合同接口。
public class Demo<T> : IDemo<T>
{
    public Task<T> GetDefaultAsync() => Task.FromResult<T>(default);
    public double Add(double x, double y) => x + y;
    public Task<double> AddAsync(double x, double y) => Task.FromResult(x + y);
    public string GetTypeName<T>() => typeof(T).Name;
}
```

```csharp
// 创建一个容器服务器 Builder。
ContainerServerBuilder serverBuilder = new();
serverBuilder
    //.UseTcp(IPAddress.Loopback, 10086)
    .UseNamedPipe("ec57043f-465c-4766-ae49-b9b1ee9ac571")
    .UseJsonSerializer();

serverBuilder
    // 未单态化的泛型接口，必须使用以下这种较为丑陋的语法进行注册。
    .Register(typeof(IDemo<>), typeof(Demo<>))
    // 非泛型接口或已单态化的泛型接口，则可以采用以下这种更为优雅的扩展方法进行注册。
    .Register<IDemo<string>, Demo<string>>()
    .Register<ICalculator, Calculator>();

using var server = serverBuilder.Build();
// 别忘了启动服务器哦！
server.Start();

// server.Stop();
```

#### 3. 在客户端调用方法的远程实现

```csharp
// 创建一个容器客户端 Builder。
ContainerClientBuilder clientBuilder = new();
clientBuilder
    //.UseTcp(IPAddress.Loopback, 10086)
    .UseNamedPipe("ec57043f-465c-4766-ae49-b9b1ee9ac571")
    .UseJsonSerializer();
using var client = clientBuilder.Build();

// 从以上构建好的 client 中取用合同实例。
var demo1 = client.Resolve<IDemo<string>>();
var demo2 = client.Resolve<IDemo<int>>();

// 使用合同实例，它们将执行存在于另一个进程中的实现！
var result0 = demo1.Add(16, 26); // 42
var result1 = await demo1.AddAsync(40, 2); // 42
var result2 = demo1.GetTypeName<string>(); // "String"

var result3 = await demo1.GetDefaultAsync(); // null
var result3 = await demo2.GetDefaultAsync(); // 0
```
