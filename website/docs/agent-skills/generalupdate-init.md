---
sidebar_position: 2
sidebar_label: 🚀 集成指南
title: 🚀 generalupdate-init — 快速集成指南
---

# 🚀 GeneralUpdate 集成完全指南

帮助开发者在任意 .NET 应用中集成 GeneralUpdate 自动更新。**从零开始，不需要提前了解 GeneralUpdate 的内部细节。**

---

## 先理解：为什么更新需要两个程序？

GeneralUpdate 采用**双进程架构**，这是理解一切配置的前提：

```
你的 App.exe (Client —— 主程序)
    │
    ├── 平时：正常运行你的业务逻辑
    │
    ├── 发现新版本时：
    │   1. 下载更新包
    │   2. 把下载信息写成 IPC 文件（一种临时文件）
    │   3. 启动 "升级程序"（UpgradeApp.exe）
    │   4. 自己退出（因为 Windows 不允许程序覆盖自己）
    │
    ▼
UpgradeApp.exe (Upgrade —— 升级程序)
    │
    ├── 读取 IPC 文件
    ├── 解压/替换文件（此时主程序已退出，文件不占用）
    ├── 启动主程序
    └── 自己退出
```

> **IPC 文件**（Inter-Process Communication）= 临时通讯文件，Client 把下载好的更新信息写进去，Upgrade 读出来执行更新。

> **manifest.json** = 更新清单文件，告诉 GeneralUpdate 你的主程序叫什么、升级程序在哪。

这种设计的原因很简单：**Windows 不允许正在运行的程序覆盖自己的 .exe 文件**，所以需要一个"替身"来完成替换工作。

---

## 你需要准备什么？

```
1. 你的 .NET 项目（WPF / WinForms / Avalonia / MAUI / 控制台 都可以）
2. .NET SDK（8.0+）
3. 一个后端服务（可选）—— 没有后端也可以用 OSS 对象存储
```

---

## 安装 NuGet

```bash
dotnet add package GeneralUpdate.Core --version 10.5.0-beta.4
```

> 如果要用崩溃守护（Bowl），再加一个包：`dotnet add package GeneralUpdate.Bowl --version 10.5.0-beta.4`  
> 差分更新功能已经内置在 Core 里，**不需要**额外装 `GeneralUpdate.Differential`。

---

## 最低配置：3 行代码跑起来

这是最简单的集成方式，适用于控制台应用或想先验证功能：

```csharp
using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;

var config = new UpdateRequest
{
    UpdateUrl = "https://your-server.com/api",   // 服务端地址
    AppSecretKey = "your-secret-key",             // 密钥（长度 ≥ 16 字符）
    MainAppName = "MyApp.exe",                    // 你的主程序文件名
    ClientVersion = "1.0.0.0",                    // 当前版本号
    ProductId = "my-product-001",                 // 你的产品 ID
    InstallPath = "."                              // 安装目录（. 表示当前目录）
};

await new GeneralUpdateBootstrap().SetConfig(config).LaunchAsync();
```

这 6 个字段是 UpdateRequest 的**必填项**，少一个都会报错：

| 字段 | 填什么 | 为什么需要 |
|------|--------|-----------|
| `UpdateUrl` | 服务端 API 地址 | 程序从这里查询有没有新版本 |
| `AppSecretKey` | 你设定的密钥 | 用于 IPC 加密通讯，Client 和 Upgrade 必须一致 |
| `MainAppName` | `"MyApp.exe"` | 告诉升级程序"更新完要启动哪个 exe" |
| `ClientVersion` | `"1.0.0.0"` | 服务端根据这个判断哪些版本需要更新给你 |
| `ProductId` | `"my-app-001"` | 区分不同产品（如果你有多个应用） |
| `InstallPath` | `"."` | 更新包解压到哪个目录 |

---

## 完整配置：带事件监听

如果你想看到更新进度、处理错误、做日志，加上事件监听：

```csharp
using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;
using GeneralUpdate.Core.Download;

var config = new UpdateRequest
{
    UpdateUrl = "https://your-server.com/Upgrade/Verification",
    AppSecretKey = "your-secret-key",
    MainAppName = "MyApp.exe",
    ClientVersion = "1.0.0.0",
    ProductId = "my-product-001",
    InstallPath = AppDomain.CurrentDomain.BaseDirectory,  // 生产环境用这个
};

await new GeneralUpdateBootstrap()
    .SetConfig(config)
    .AddListenerUpdateInfo((_, e) =>
    {
        // 有更新时触发
        Console.WriteLine($"发现 {e.Info?.Body?.Count ?? 0} 个新版本");
    })
    .AddListenerMultiDownloadStatistics((_, e) =>
    {
        // 下载进度（可绑定到进度条）
        Console.WriteLine($"进度: {e.ProgressPercentage}% | 速度: {e.Speed}");
    })
    .AddListenerMultiDownloadCompleted((_, e) =>
    {
        Console.WriteLine($"版本 {e.Version} 下载完成");
    })
    .AddListenerMultiDownloadError((_, e) =>
    {
        Console.WriteLine($"下载失败: {e.Exception?.Message}");
    })
    .AddListenerMultiAllDownloadCompleted((_, e) =>
    {
        Console.WriteLine("全部下载完成，即将开始更新");
    })
    .AddListenerException((_, e) =>
    {
        Console.WriteLine($"异常: {e.Message}");
    })
    .AddListenerProgress((_, e) =>
    {
        // v10.5.0-beta.4 新增的第 7 个事件
    })
    .LaunchAsync();
```

> **通常你只需要关注 3 个事件**：`MultiDownloadStatistics`（进度条）、`MultiDownloadError`（下载失败）、`Exception`（异常）。其他事件按需添加。

---

## UpdateRequest 完整配置说明

除了 6 个必填字段，还有这些可选配置：

```csharp
var config = new UpdateRequest
{
    // === 必填（上面已经列出了） ===
    UpdateUrl = "...", AppSecretKey = "...", MainAppName = "...",
    ClientVersion = "1.0.0.0", ProductId = "...", InstallPath = ".",

    // === 可选：认证方式 ===
    AuthScheme = AuthScheme.Hmac,         // Hmac / Bearer / ApiKey / Basic
    Token = "your-token",
    BasicUsername = "user",
    BasicPassword = "pass",

    // === 可选：备份时排除的文件 ===
    Files = new List<string> { "*.log", "*.tmp" },        // 排除日志/临时文件
    Formats = new List<string> { ".pdb" },                 // 排除调试符号文件
    Directories = new List<string> { "logs", "cache" },   // 排除整个目录
};
```

也可以使用**建造者模式**（效果一样，写法更流畅）：

```csharp
var config = UpdateRequestBuilder.Create()
    .SetUpdateUrl("https://your-server.com/api")
    .SetAppSecretKey("your-secret-key")
    .SetMainAppName("MyApp.exe")
    .SetClientVersion("1.0.0.0")
    .SetProductId("my-product-001")
    .SetInstallPath(".")
    .Build();
```

或者**零配置模式**（从 manifest.json 自动发现，更简洁）：

```csharp
await new GeneralUpdateBootstrap()
    .SetSource(
        updateUrl: "https://your-server.com/api",
        appSecretKey: "your-secret-key")
    .AddListenerUpdateInfo(...)
    .LaunchAsync();
```

---

## 4 大更新场景（服务端返回什么）

GeneralUpdate 根据服务端返回的更新包类型自动选择动作：

| 场景 | 发生了什么 |
|------|-----------|
| **None** | 无需更新 → 直接启动主程序 |
| **UpgradeOnly** | 只更新升级程序自身 → 主程序原地解压覆盖升级程序 |
| **MainOnly** | 只更新主程序 → Client 下载 → IPC → 启动 Upgrade 替换主程序 |
| **Both** | 两者都更新 |

---

## Upgrade 进程配置

Upgrade（升级程序）不需要填配置，它从 IPC 文件自动读取：

```csharp
using GeneralUpdate.Core;

await new GeneralUpdateBootstrap()
    .AddListenerException((_, e) =>
        Console.WriteLine($"错误: {e.Message}"))
    .LaunchAsync();
```

> 你的项目里需要**两个独立的项目**：一个 Client（主程序），一个 Upgrade（升级程序）。

---

## 部署结构检查清单

发布后你的目录应该长这样：

```
publish/
├── MyApp.exe                         ← 你的主程序
├── generalupdate.manifest.json       ← 更新清单（告诉 GeneralUpdate 升级程序在哪）
└── update/
    └── UpgradeApp.exe                ← 升级程序（首个版本就必须有！）
```

首次部署时最常见的错误就是**忘了放 UpgradeApp.exe**，导致检测到更新后无法执行升级。

---

## 应用角色说明

`AppType` 区分两个进程的角色：

| 值 | 枚举 | 谁在用 |
|----|------|--------|
| 1 | `AppType.Client` | 主程序（你的业务代码） |
| 2 | `AppType.Upgrade` | 升级程序（UpgradeApp.exe） |
| 3 | `AppType.OssClient` | OSS 模式的主程序 |
| 4 | `AppType.OssUpgrade` | OSS 模式的升级程序 |

---

## 新手常见错误

| # | 错误 | 后果 | 正确做法 |
|---|------|------|---------|
| 1 | Client 和 Upgrade 用了**不同版本**的 NuGet 包 | 运行时报 `Method not found` | 两个项目用**完全一样**的版本号 |
| 2 | 忘了放 UpgradeApp.exe | 检测到更新但无法执行 | 从第一个版本就放入 update/ 目录 |
| 3 | IPC 文件编码没设 UTF-8 | Linux/macOS 中文乱码 | 设置 `Encoding.UTF8` |
| 4 | 版本号不是 4 段式（如 `1.0`） | 版本比较出错 | 始终用 `1.0.0.0` 格式 |
| 5 | manifest.json 写的 exe 名不对 | 更新后找不到主程序 | 和实际文件名完全一致 |

---

## 下一步

- 需要更新 UI → [generalupdate-ui](generalupdate-ui)
- 选哪种更新策略 → [generalupdate-strategy](generalupdate-strategy)
- 需要崩溃守护 / 高级定制 → [generalupdate-advanced](generalupdate-advanced)
- 遇到问题 → [generalupdate-troubleshoot](generalupdate-troubleshoot)
