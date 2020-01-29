# HandyIpc

An easy to use inter-process communication library. The design of this library API was inspired by [orleans](https://github.com/dotnet/orleans), and and its implementation refers to [refit](https://github.com/reactiveui/refit).

## How to use

#### 1. Define IPC contract

```csharp
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
// Implement the IPC contract (interface)
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
// Build and start server
IpcServerBuilder.Create()
    .Register(typeof(IDemo<>), typeof(Demo<>))
    .Register<IOtherInterface, OtherImpl>()
    .Build()
    .Start();
```

#### 3. Invoke remote methods in client side

```csharp
var demo1 = IpcClient.Of<IDemo<string>>();
var demo2 = IpcClient.Of<IDemo<int>>();

var result0 = demo1.Add(16, 26); // 42
var result1 = await demo1.GetDefaultAsync(); // null
var result2 = await demo2.GetDefaultAsync(); // 0
var result3 = demo1.GenericMethod<string, int>(null, 0, null) // T1=System.String, T2=System.Int32
```

## Feature

1. [x] Support for generic interface.
2. [x] Support for Task/Task<T> return value in interface method.
3. [x] Support for generic methods (parameter type allow contains nested generic types).
4. [ ] Support for interface inheritance.

