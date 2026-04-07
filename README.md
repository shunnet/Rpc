<h1 align="center">Rpc</h1>

<p align="center">
  <img width="120" height="120" src="https://api.shunnet.top/pic/nuget.png" alt="Snet Logo"/>
</p>

<p align="center">
  <b>C# 基于 DotNetty 的高性能 RPC 框架</b>
</p>

<p align="center">

  <img src="https://img.shields.io/badge/.NET-8.0%20%7C%2010.0-purple.svg"/>
  <img src="https://img.shields.io/badge/license-MIT-green"/>
  <img src="https://img.shields.io/github/stars/shunnet/Rpc?style=social"/>

</p>

<p align="center">
  支持进程间通信与分布式远程调用，专为 .NET 平台设计。 
</p>

<p align="center">
  <a href="https://shunnet.top"><b>🌐 官方网站</b></a> ·
  <a href="https://github.com/shunnet/Rpc"><b>📦 GitHub</b></a>
</p>

> ⚡ **基于 DotNetty 的高性能 RPC 框架**  
> 支持进程间通信与分布式远程调用，专为 .NET 平台设计。  

## 📖 简介

**Snet.Rpc** 是一个在 **.NET 平台** 上实现的远程过程调用（RPC）框架。  
基于 **DotNetty** 网络库构建，具备 **高性能 / 低延时 / 易扩展** 的特点，适用于：

- 微服务架构  
- 分布式计算  
- 边缘/网关调用  
- 内网服务通信  

✨ 核心优势：  
- 🔌 **异步 IO**：利用 DotNetty 高并发模型  
- 🛠 **服务注册 / 暴露机制**：支持接口到实现的快速绑定  
- 📡 **客户端代理**：支持同步与异步调用  
- 🔄 **可插拔序列化**：JSON / Protobuf / MsgPack 等  
- ❤️ **内置样例工程**：即下即用，快速上手  


## 🏗 架构概览

```text
+-------------+                       +---------------+
|  Rpc Client | ---(request message)-->|  Rpc Server   |
|  (proxy)    | <-(response message)---|  (service)    |
+-------------+                       +---------------+
       |                                      ^
       |                                      |
       +-- 连接池 / 负载均衡 / 重连策略 ------>|
```

- **传输层**：基于 TCP (DotNetty)，支持心跳与重连  
- **协议层**：自定义消息帧，包含请求 ID / 方法名 / 状态码  
- **编解码**：支持 JSON / Protobuf / 可扩展序列化器  
- **服务端**：方法映射、参数反序列化、异常处理  
- **客户端**：请求发起、Task 异步等待、超时控制  


## 🚀 快速开始

> 项目已内置 **服务端/客户端样例**：  
> `Snet.RPC.Service.Samples` 与 `Snet.RPC.Client.Samples`。

### 📦 安装方式  

通过 NuGet 获取：  

```bash
dotnet add package Snet.Rpc
```

### 服务端示例

```csharp
using Snet.Log;
using Snet.Rpc.service;
using Snet.Utility;
using System.Text;

// 初始化服务端
RpcService rpcService = RpcService.Instance(new Snet.Rpc.data.Service.Basics
{
    Port = 6688,
    TimeOut = 1000,
    UserName = "snet",
    Password = "snet",
    Infos = new List<Snet.Rpc.data.Service.Info>
    {
        new Snet.Rpc.data.Service.Info
        {
            INs = new List<Snet.Rpc.data.Details>{ new Snet.Rpc.data.Details { INames = "IHello" }},
            ISn = "RPC1"
        }
    }
});

LogHelper.Info(rpcService.Open().ToJson(true));

// 注册接口与实现
rpcService.Register<IHello, Hello>();

while (true)
{
    Console.ReadLine();
    IHello hello = rpcService.Create<IHello>();
    hello.Kitty(new Test { aaaa = "服务端 => 客户端", bbbb = DateTime.Now.ToDateTimeString() });
    LogHelper.Info(hello.Get());
}

// 示例接口与实现
public class Test { public string aaaa { get; set; } public string bbbb { get; set; } }
public interface IHello { void Kitty(Test test); string Get(); }
public class Hello : IHello
{
    public string Get() => "我是服务端的 GET 方法: " + DateTime.Now;
    public void Kitty(Test test) => LogHelper.Info(test.ToJson(true));
}
```

### 客户端示例

```csharp
using Snet.Log;
using Snet.Rpc.client;
using Snet.Utility;

// 初始化客户端
RpcClient rpcClient = RpcClient.Instance(new Snet.Rpc.data.Client.Basics
{
    IpAddress = "127.0.0.1",
    Port = 6688,
    TimeOut = 1000,
    Password = "ysai",
    UserName = "ysai",
    ISn = "RPC1",
    INs = new List<Snet.Rpc.data.Details> { new Snet.Rpc.data.Details { INames = "IHello" } },
});

LogHelper.Info(rpcClient.Open().ToJson(true));

// 注册接口与实现
rpcClient.Register<IHello, Hello>();

while (true)
{
    Console.ReadLine();
    IHello hello = rpcClient.Create<IHello>();
    hello.Kitty(new Test { aaaa = "客户端 => 服务端", bbbb = DateTime.Now.ToDateTimeString() });
    LogHelper.Info(hello.Get());
}

// 示例接口与实现
public class Test { public string aaaa { get; set; } public string bbbb { get; set; } }
public interface IHello { void Kitty(Test test); string Get(); }
public class Hello : IHello
{
    public string Get() => "我是客户端的 GET 方法: " + DateTime.Now;
    public void Kitty(Test test) => LogHelper.Info(test.ToJson(true));
}
```

## 🙏 致谢

- 🌐 [Shunnet.top](https://shunnet.top)  
- ⚡ [DotNetty](https://github.com/Azure/DotNetty/)  
- 🔥 [ImpromptuInterface](https://github.com/ekonbenefits/impromptu-interface)  


## 📜 许可证

![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)  

本项目基于 **MIT** 协议开源。  
详情请阅读 [LICENSE](LICENSE)。  

⚠️ 注意：本软件按 “原样” 提供，作者不对使用后果承担责任。  
