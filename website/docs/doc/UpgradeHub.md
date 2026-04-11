---
sidebar_position: 7
---

# UpgradeHub

## 组件概览

**UpgradeHubService** 是基于 SignalR 的实时版本推送服务，集成在 `GeneralUpdate.ClientCore` 中。服务端通过 `UpgradeHub` 向已连接的客户端主动推送版本更新通知，客户端通过 `UpgradeHubService` 订阅并接收消息，支持点对点（一对一）和广播（一对多）两种推送模式。

**命名空间：** `GeneralUpdate.ClientCore.Hubs`  
**程序集：** `GeneralUpdate.ClientCore.dll`

```csharp
public class UpgradeHubService : IUpgradeHubService
```

---

## 核心特性

### 1. 实时推送
- 基于 SignalR 长连接，服务端主动推送，无需客户端轮询
- 支持点对点推送（按 appkey 定向推送）
- 支持广播推送（同时通知所有在线客户端）

### 2. 自动重连
- 内置随机退避重连策略（`RandomRetryPolicy`）
- 断线后自动尝试重连，保证连接稳定性

### 3. 完整生命周期管理
- 连接建立（`StartAsync`）
- 优雅停止（`StopAsync`）
- 资源释放（`DisposeAsync`）

### 4. 多事件订阅
- 接收版本推送消息（`AddListenerReceive`）
- 在线 / 离线状态通知（`AddListenerOnline`）
- 重连成功通知（`AddListenerReconnected`）
- 连接关闭通知（`AddListenerClosed`）

---

## 快速开始

### 安装

`UpgradeHubService` 包含在 `GeneralUpdate.ClientCore` 中，无需单独安装：

```bash
dotnet add package GeneralUpdate.ClientCore
```

### 初始化与使用

以下示例展示了如何在客户端应用中接收服务端推送的版本更新消息：

```csharp
using GeneralUpdate.ClientCore.Hubs;

// 创建 UpgradeHubService 实例
// url    — SignalR Hub 订阅地址
// token  — ID4 认证令牌（可选）
// appkey — 客户端唯一标识（推荐使用 GUID），服务端用此字段定向推送
var hub = new UpgradeHubService(
    "http://localhost:5000/UpgradeHub",
    token: null,
    appkey: "dfeb5833-975e-4afb-88f1-6278ee9aeff6");

// 订阅服务端推送的版本更新消息
// message 为服务端推送的 JSON 字符串（通常为 Packet 对象序列化结果）
hub.AddListenerReceive((message) =>
{
    Console.WriteLine($"收到更新推送: {message}");
});

// 订阅在线 / 离线状态通知
hub.AddListenerOnline((info) =>
{
    Console.WriteLine($"在线状态: {info}");
});

// 订阅重连成功通知
hub.AddListenerReconnected((connectionId) =>
{
    Console.WriteLine($"已重连，connectionId={connectionId}");
    return Task.CompletedTask;
});

// 订阅连接关闭通知
hub.AddListenerClosed((exception) =>
{
    if (exception is not null)
        Console.WriteLine($"连接异常关闭: {exception.Message}");
    else
        Console.WriteLine("连接已正常关闭");
    return Task.CompletedTask;
});

// 建立 SignalR 连接
await hub.StartAsync();

Console.WriteLine("已连接，等待服务端推送...");
Console.ReadLine();

// 停止连接（可在应用进入后台时调用，保留重连能力）
await hub.StopAsync();

// 释放资源（不可再复用）
await hub.DisposeAsync();
```

### 在依赖注入项目中使用（以 Prism 为例）

```csharp
// 注册服务
protected override void RegisterTypes(IContainerRegistry containerRegistry)
{
    containerRegistry.Register<IUpgradeHubService, UpgradeHubService>();
}

// 在 ViewModel 中使用
public MainWindowViewModel(IUpgradeHubService hubService)
{
    hubService.AddListenerReceive((message) =>
    {
        Console.WriteLine($"收到推送: {message}");
    });
    _ = hubService.StartAsync();
}
```

---

## 核心 API 参考

### 构造函数

```csharp
public UpgradeHubService(string url, string? token = null, string? appkey = null)
```

**参数：**

| 参数     | 类型      | 说明                                                         |
| -------- | --------- | ------------------------------------------------------------ |
| `url`    | `string`  | SignalR Hub 订阅地址，例如 `http://127.0.0.1/UpgradeHub`    |
| `token`  | `string?` | ID4 认证令牌字符串（可选）                                    |
| `appkey` | `string?` | 客户端唯一标识，推荐使用 GUID，服务端用于定向推送（可选）     |

---

### 方法

#### AddListenerReceive 方法

订阅服务端推送的版本更新消息。

```csharp
public void AddListenerReceive(Action<string> receiveMessageCallback)
```

**参数：**
- `receiveMessageCallback` — 接收到消息时的回调，`string` 为消息内容（建议为 JSON 格式的 `Packet` 对象）

---

#### AddListenerOnline 方法

订阅在线 / 离线状态变化通知。

```csharp
public void AddListenerOnline(Action<string> onlineMessageCallback)
```

**参数：**
- `onlineMessageCallback` — 状态变化时的回调，`string` 为状态描述

---

#### AddListenerReconnected 方法

订阅断线重连成功通知。

```csharp
public void AddListenerReconnected(Func<string?, Task>? reconnectedCallback)
```

**参数：**
- `reconnectedCallback` — 重连成功后的异步回调，`string?` 为新的 connectionId（可能为 null）

---

#### AddListenerClosed 方法

订阅连接关闭通知。

```csharp
public void AddListenerClosed(Func<Exception?, Task> closeCallback)
```

**参数：**
- `closeCallback` — 连接关闭时的异步回调，`Exception?` 为关闭原因（正常关闭时为 null）

---

#### StartAsync 方法

建立 SignalR 连接，开始订阅推送通知。

```csharp
public Task StartAsync()
```

---

#### StopAsync 方法

优雅停止连接。当前正在处理的消息会完成，不再接受新消息。适合在应用进入后台或暂停时调用，之后可重新调用 `StartAsync` 恢复连接。

```csharp
public Task StopAsync()
```

---

#### DisposeAsync 方法

彻底释放 Hub 实例及所有资源，释放后不可再复用。

```csharp
public Task DisposeAsync()
```

---

## 实际使用示例

### 示例 1：基本推送订阅

```csharp
using GeneralUpdate.ClientCore.Hubs;

var hub = new UpgradeHubService("http://localhost:5000/UpgradeHub",
    appkey: Guid.NewGuid().ToString());

hub.AddListenerReceive((message) =>
{
    Console.WriteLine($"[更新推送] {message}");
});

await hub.StartAsync();
```

### 示例 2：完整生命周期管理

```csharp
using GeneralUpdate.ClientCore.Hubs;

var hub = new UpgradeHubService(
    "http://localhost:5000/UpgradeHub",
    token: null,
    appkey: "dfeb5833-975e-4afb-88f1-6278ee9aeff6");

hub.AddListenerReceive(msg => Console.WriteLine($"推送: {msg}"));
hub.AddListenerOnline(info => Console.WriteLine($"在线状态: {info}"));
hub.AddListenerReconnected(id => { Console.WriteLine($"重连: {id}"); return Task.CompletedTask; });
hub.AddListenerClosed(ex => { Console.WriteLine(ex != null ? $"异常: {ex.Message}" : "关闭"); return Task.CompletedTask; });

try
{
    await hub.StartAsync();
    Console.ReadLine(); // 等待用户输入
}
finally
{
    await hub.StopAsync();
    await hub.DisposeAsync();
}
```

### 示例 3：搭配 GeneralClientBootstrap 使用

实际项目中，`UpgradeHubService` 通常与 `GeneralClientBootstrap` 一起使用。客户端先通过 `GeneralClientBootstrap` 完成常规版本检查和下载，同时用 `UpgradeHubService` 接收服务端的实时推送通知，无需等待下一次轮询即可立即触发更新。

```csharp
using System.Text;
using GeneralUpdate.ClientCore;
using GeneralUpdate.ClientCore.Hubs;
using GeneralUpdate.Common.Download;
using GeneralUpdate.Common.Internal;
using GeneralUpdate.Common.Internal.Bootstrap;
using GeneralUpdate.Common.Shared.Object;

// 1. 启动 SignalR 推送监听
var hub = new UpgradeHubService("http://localhost:5000/UpgradeHub",
    appkey: "dfeb5833-975e-4afb-88f1-6278ee9aeff6");
hub.AddListenerReceive(msg => Console.WriteLine($"实时推送: {msg}"));
await hub.StartAsync();

// 2. 常规更新流程
var config = new Configinfo
{
    UpdateUrl = "http://127.0.0.1:5000/Upgrade/Verification",
    ReportUrl = "http://127.0.0.1:5000/Upgrade/Report",
    AppName = "UpgradeSample.exe",
    MainAppName = "ClientSample.exe",
    ClientVersion = "1.0.0.0",
    UpgradeClientVersion = "1.0.0.0",
    InstallPath = AppDomain.CurrentDomain.BaseDirectory,
    ProductId = "2d974e2a-31e6-4887-9bb1-b4689e98c77a",
    AppSecretKey = "dfeb5833-975e-4afb-88f1-6278ee9aeff6"
};

await new GeneralClientBootstrap()
    .AddListenerException((_, e) => Console.WriteLine(e.Exception))
    .SetConfig(config)
    .LaunchAsync();
```

---

## 注意事项

### ⚠️ 重要提示

1. **服务端配置**
   - 服务端需要安装 `Microsoft.AspNetCore.SignalR` 并注册 `UpgradeHub`
   - 在 `Program.cs` 中：`app.MapHub<UpgradeHub>("/UpgradeHub");`

2. **appkey 唯一性**
   - `appkey` 用于服务端定向推送，每个客户端实例应使用唯一值
   - 推荐使用固定 GUID（与 `Configinfo.AppSecretKey` 保持一致）

3. **StopAsync vs DisposeAsync**
   - `StopAsync`：暂停连接，之后可重新调用 `StartAsync` 恢复
   - `DisposeAsync`：彻底释放，不可再复用

4. **消息格式**
   - `AddListenerReceive` 中接收的消息格式由服务端决定，建议使用 JSON 字符串（`Packet` 对象序列化结果）

### 💡 最佳实践

- 在应用启动时调用 `StartAsync`，在应用关闭时调用 `StopAsync` + `DisposeAsync`
- 使用依赖注入管理 `UpgradeHubService` 实例的生命周期
- 在 `AddListenerClosed` 回调中记录日志，便于排查连接问题

---

## 适用平台

| 产品           | 版本                  |
| -------------- | --------------------- |
| .NET           | 5, 6, 7, 8, 9, 10     |
| .NET Framework | 4.6.1                 |
| .NET Standard  | 2.0                   |
| .NET Core      | 2.0                   |
| ASP.NET        | Any                   |

---

## 相关资源

- **示例代码：** [查看 GitHub 示例](https://github.com/GeneralLibrary/GeneralUpdate-Samples/blob/main/src/Push/Program.cs)
- **主仓库：** [GeneralUpdate 项目](https://github.com/GeneralLibrary/GeneralUpdate)
- **相关组件：** [GeneralUpdate.ClientCore](./GeneralUpdate.ClientCore.md) | [GeneralUpdate.Core](./GeneralUpdate.Core.md)
