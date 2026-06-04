---
sidebar_position: 3
title: 入门实战手册
---

# GeneralUpdate 入门实战手册

这篇手册面向第一次接触 GeneralUpdate 的开发者。目标不是一次讲完所有 API，而是让你从零开始，写完 Client → 写完 Upgrade → 用 Tools 生成配置 → 启动 Server → 一条命令跑通完整的"发现更新 → 下载 → 应用 → 回到新版本"闭环。

:::info 你将学到什么
完成这篇手册后，你会拥有一个**可运行的端到端自动更新系统**：一个能检查更新的 Client、一个能应用更新的 Upgrade、一份自动生成的 manifest、以及一个本地测试 Server。
:::

<iframe
  src="//player.bilibili.com/player.html?bvid=BV12P9dBiEEh&page=1"
  width="100%"
  height="480"
  style={{ borderRadius: '8px', border: 'none' }}
  allowFullScreen
  scrolling="no"
/>

## 更新流程总览

```
┌──────────────────────────────────────────────────────────────────────┐
│                     GeneralUpdate 完整更新流程                        │
└──────────────────────────────────────────────────────────────────────┘

  ① 版本检查               ② 下载包               ③ 应用更新
  ┌──────────┐           ┌──────────┐           ┌──────────┐
  │  Client  │──POST──→  │  Server  │           │ Upgrade  │
  │ (主程序)  │←─JSON───  │ (更新服务)│           │ (升级进程) │
  └────┬─────┘           └────┬─────┘           └────┬─────┘
       │                      │                      │
       │  POST /Upgrade/Verification               │                      │
       │  {version, platform} │                      │
       │ ────────────────────→│                      │
       │                      │                      │
       │  [{version, url,     │                      │
       │    hash, size}]      │                      │
       │ ←────────────────────│                      │
       │                      │                      │
       │  GET /File/Download  │                      │
       │ ────────────────────→│                      │
       │                      │                      │
       │  .zip packages       │                      │
       │ ←────────────────────│                      │
       │                      │                      │
       │  [写入 IPC 加密契约]   │                      │
       │  [启动 Upgrade.exe]   │                      │
       │ ────────────────────────────────────────────→│
       │                      │                      │
       │                      │    [读取 IPC 数据]    │
       │                      │    [校验 Hash]        │
       │                      │    [解压 → 覆盖]      │
       │                      │    [启动新版本 Client] │
       │                      │                      │
       │ ←────────────────────────────────────────────│
       │  [新版本运行中 ✓]     │                      │
       │                      │                      │
```

| 角色 | 在项目中的位置 | 负责什么 |
| --- | --- | --- |
| Client | `MyApp.exe`（你自己的主程序） | 检查更新 → 下载包 → 启动 Upgrade |
| Upgrade | `MyApp.Upgrade.exe`（独立的升级进程） | 校验 Hash → 解压覆盖 → 启动新版 Client |
| Server | `GeneralUpdate-Samples/src/Server` | 返回版本信息、接收更新报告、提供补丁下载 |
| Tools | GeneralUpdate.Tools | 生成 `generalupdate.manifest.json`，串联 Client 与 Upgrade |

---

## Phase 1：环境准备

:::caution 开始之前
所有后续步骤都依赖这两个工具。请先用验证命令确认它们安装正确，否则 Phase 4 的 `dotnet publish` 和 Phase 5 的 Server 启动会失败。
:::

### 安装清单

| 项 | 要求 | 验证命令 |
| --- | --- | --- |
| .NET SDK | 8.0+（推荐 10.0） | `dotnet --version` |
| Git | 任意版本 | `git --version` |

### 新建项目

打开终端，创建 Client 和 Upgrade 两个控制台项目：

```bash
dotnet new console -n MyApp
dotnet new console -n MyApp.Upgrade

cd MyApp
dotnet add package GeneralUpdate.Core

cd ../MyApp.Upgrade
dotnet add package GeneralUpdate.Core
```

**预期结果**：两个项目目录下都有 `GeneralUpdate.Core` 的 NuGet 引用。

---

## Phase 2：集成 Client 代码

> **在 `MyApp/Program.cs` 中粘贴以下全部内容。Client 负责：检查更新 → 下载包 → 启动 Upgrade。**

```csharp
using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;
using GeneralUpdate.Core.Event;

// ================================================================
// Client — 主程序入口，负责检测和下载更新
// ================================================================

// 只需设置 3 个 secret。MainAppName、ClientVersion、UpdateAppName
// 等身份字段由框架自动从 generalupdate.manifest.json 发现，无需手写。
var request = new UpdateRequest
{
    UpdateUrl    = "http://localhost:5000/Upgrade/Verification",
    ReportUrl    = "http://localhost:5000/Upgrade/Report",
    AppSecretKey = "dfeb5833-975e-4afb-88f1-6278ee9aeff6",
};

await new GeneralUpdateBootstrap()
    .SetConfig(request)
    .SetOption(Option.AppType, AppType.Client)
    .AddListenerUpdateInfo((_, e) =>
    {
        Console.WriteLine($"发现 {e.Info?.Body?.Count ?? 0} 个可用更新");
        if (e.Info?.Body != null)
        {
            foreach (var v in e.Info.Body)
                Console.WriteLine($"  v{v.Version} — {v.Name} ({v.Size} bytes)");
        }
    })
    .AddListenerMultiDownloadStatistics((_, e) =>
    {
        Console.WriteLine($"\r下载: {e.ProgressPercentage:F0}% {e.Speed}");
    })
    .AddListenerMultiDownloadCompleted((_, e) =>
    {
        Console.WriteLine();
        Console.WriteLine($"下载 {(e.IsCompleted ? "✓ 完成" : "✗ 失败")}");
    })
    .AddListenerException((_, e) =>
    {
        Console.WriteLine($"异常: {e.Exception.Message}");
    })
    .LaunchAsync();
```

### 为什么只设 3 个参数？

`ClientStrategy` 在执行时会自动调用 `AppMetadataDiscoverer.Discover()`，从 `generalupdate.manifest.json`（Phase 4 用 Tools 生成）中读取身份字段并补全 `UpdateRequest`：

| 字段 | 谁负责 | 说明 |
| --- | --- | --- |
| `UpdateUrl` | **你写代码** | Server 验证接口地址 |
| `ReportUrl` | **你写代码** | Server 报告接口地址 |
| `AppSecretKey` | **你写代码** | 应用密钥 |
| `MainAppName` | **框架自动发现** | 从 manifest 读取 |
| `ClientVersion` | **框架自动发现** | 从 manifest 读取 |
| `UpdateAppName` | **框架自动发现** | 从 manifest 读取 |
| `ProductId` | **框架自动发现** | 从 manifest 读取 |
| `UpdatePath` | **框架自动发现** | 从 manifest 读取 |
| `InstallPath` | **默认值** | `AppDomain.CurrentDomain.BaseDirectory` |

如果你不想写 `UpdateRequest`，还可以用更简短的 `SetSource()`：

```csharp
await new GeneralUpdateBootstrap()
    .SetSource("http://localhost:5000/Upgrade/Verification",
               "dfeb5833-975e-4afb-88f1-6278ee9aeff6",
               "http://localhost:5000/Upgrade/Report")
    .SetOption(Option.AppType, AppType.Client)
    // ... listeners
    .LaunchAsync();
```

Client 不直接应用更新——下载完成后写 IPC 加密契约 → 启动 Upgrade → 自身退出。

:::tip 关键设计
Client **只负责发现和下载**，文件替换工作完全交给独立的 Upgrade 进程。这是因为运行中的进程无法覆盖自身的可执行文件——通过 IPC 加密契约把控制权交给 Upgrade，是实现"自己更新自己"的核心架构。
:::

---

## Phase 3：集成 Upgrade 代码

> **在 `MyApp.Upgrade/Program.cs` 中粘贴以下全部内容。Upgrade 比 Client 更简单——不需要 `SetConfig()`，所有参数通过加密 IPC 文件从 Client 传入。**

```csharp
using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;
using GeneralUpdate.Core.Event;

// ================================================================
// Upgrade — 独立的升级进程 (MyApp.Upgrade.exe)
// 无需 SetConfig()，参数由 Client 通过加密 IPC 文件传入
// ================================================================

await new GeneralUpdateBootstrap()
    .SetOption(Option.AppType, AppType.Upgrade)
    .AddListenerMultiDownloadStatistics((_, e) =>
    {
        Console.WriteLine($"\r应用补丁: {e.ProgressPercentage:F0}%");
    })
    .AddListenerMultiDownloadCompleted((_, e) =>
    {
        Console.WriteLine();
        Console.WriteLine($"补丁 {(e.IsCompleted ? "✓ 完成" : "✗ 失败")}");
    })
    .AddListenerException((_, e) =>
    {
        Console.WriteLine($"异常: {e.Exception.Message}");
    })
    .LaunchAsync();
```

### Client vs Upgrade 差异

| | Client | Upgrade |
| --- | --- | --- |
| `AppType` | `AppType.Client` | `AppType.Upgrade` |
| `SetConfig()` | ✅ 必须，设置服务器地址、版本等 | ❌ 不需要，通过 IPC 从 Client 获取 |
| 职责 | 检查版本 → 下载包 → 启动 Upgrade | Hash 校验 → 解压覆盖 → 启动新版 Client |
| 谁启动谁 | 由用户直接启动 | 由 Client 进程自动启动 |

---

## Phase 4：使用 Tools 生成项目结构

> **Client 和 Upgrade 代码写好后，用 Tools 的配置生成器（Config 标签页）一键 `dotnet publish` 并生成 `generalupdate.manifest.json`。**

### 操作步骤

打开 **GeneralUpdate.Tools** → 切换到 **Config** 标签页（配置生成器）：

1. **Client .csproj Path**：点击 Browse，选择 `MyApp.csproj`
2. **Upgrade .csproj Path**：点击 Browse，选择 `MyApp.Upgrade.csproj`
3. 点击 **Analyze**：Tools 解析两个 .csproj，自动填入：
   - `MainAppName`：来自 Client 项目的 AssemblyName（如 `MyApp.exe`）
   - `UpdateAppName`：来自 Upgrade 项目的 AssemblyName（如 `MyApp.Upgrade.exe`）
   - `ClientVersion` / `UpgradeClientVersion`：默认 `1.0.0`，按需修改
4. 检查自动填入的字段，确认 `UpdatePath` 为 `update/`（默认值）
5. 点击 **Generate Sample**：Tools 依次执行：
   - `dotnet publish` Client → 输出到根目录
   - `dotnet publish` Upgrade → 输出到 `update/` 子目录
   - 写入 `generalupdate.manifest.json` 到根目录

### 发布后的目录结构

Tools 可以帮助你一键 `dotnet publish` 并组装输出目录（通过 Config 标签页的 Sample Publisher）。最终结构如下：

```
publish/
├── MyApp.exe                   # Client publish 产物（Phase 2）
├── MyApp.dll
├── GeneralUpdate.Core.dll
├── generalupdate.manifest.json # Tools 生成，放根目录，框架自动发现
└── update/                     # Upgrade publish 产物（Phase 3），子目录名来自 manifest 的 updatePath
    ├── MyApp.Upgrade.exe
    └── MyApp.Upgrade.dll
```

> **注意**：`MyApp.Upgrade.exe` 不在根目录，而是在 `update/` 子目录下。框架根据 manifest 的 `updatePath` 字段（默认 `"update/"`）找到并启动它。

:::warning 目录结构错误会导致升级失败
Upgrade 进程(.exe) **必须**放在 manifest 指定的 `updatePath` 子目录下。如果 Upgrade 不在该位置，Client 将无法找到并启动升级进程，整个更新链路中断。常见错误：把 Upgrade 放在根目录或不同名称的子目录中。
:::

### `generalupdate.manifest.json` 示例

```json
{
  "mainAppName": "MyApp.exe",
  "clientVersion": "1.0.0.0",
  "updateAppName": "MyApp.Upgrade.exe",
  "upgradeClientVersion": "1.0.0.0",
  "appType": "Client",
  "productId": "2d974e2a-31e6-4887-9bb1-b4689e98c77a",
  "updatePath": "update/"
}
```

> **注意：manifest 只存身份/结构信息，不存 secret。** `UpdateUrl`、`ReportUrl`、`AppSecretKey` 等敏感字段始终写在代码里，由 Phase 2 的 `request.XXX = "..."` 提供。

---

## Phase 5：启动 Server

> **Client / Upgrade / manifest 全部就绪，启动 Server 准备端到端验证。**

```bash
git clone https://github.com/GeneralLibrary/GeneralUpdate-Samples.git
cd GeneralUpdate-Samples/src/Server
dotnet run
```

预期输出：

```
╔══════════════════════════════════════════════════╗
║     GeneralUpdate Sample Upgrade Server          ║
╠══════════════════════════════════════════════════╣
║  Verification: http://localhost:5000/Upgrade/Verification
║  Report:       http://localhost:5000/Upgrade/Report
║  Download:     http://localhost:5000/File/Download/{hash}
║  Packages:     N version(s) loaded
╚══════════════════════════════════════════════════╝
```

### Server 三端点速查

| 端点 | 方法 | 用途 |
| --- | --- | --- |
| `/Upgrade/Verification` | POST | Client 上报当前版本 → Server 返回可用更新列表 |
| `/Upgrade/Report` | POST | Client 上报更新结果（成功 / 失败） |
| `/File/Download/{hash}` | GET | 下载更新包（支持 HTTP Range 断点续传） |

Server 读取 `wwwroot/packages/versions.json` 中的包元数据并对外提供下载。

**快速验证**：

```bash
curl -X POST http://localhost:5000/Upgrade/Verification \
  -H "Content-Type: application/json" \
  -d '{"Version":"1.0.0.0"}'
```

应返回包含可用更新版本的 JSON 响应。

---

## Phase 6：端到端运行验证

```
# 终端 1 — 确保 Server 在运行
cd GeneralUpdate-Samples/src/Server && dotnet run

# 终端 2 — 运行 Client
cd MyApp && dotnet run
```

### 完整预期输出

```
Client:
  [UpdateInfo] 发现 1 个可用更新
    v2.0.0.0 — client_1.0.0_to_2.0.0 (xxxxx bytes)
  下载: 45% 1.2MB/s
  下载: 100%
  下载 ✓ 完成
  → 启动 MyApp.Upgrade.exe...

Upgrade:
  应用补丁: 50%
  应用补丁: 100%
  补丁 ✓ 完成
  → 启动 MyApp.exe (新版本)...

Client (新版本):
  MyApp v2.0.0.0 ✓
```

:::tip 排查清单
Use this table to diagnose common issues quickly. Most failures trace back to one of these five checks.

| 现象 | 检查 |
| --- | --- |
| Client 连不上 Server | `curl -X POST http://localhost:5000/Upgrade/Verification` 确认 Server 在跑 |
| 返回"Already up to date" | `versions.json` 中是否有比 `ClientVersion` 更高的版本 |
| 下载完成但未启动 Upgrade | `MyApp.Upgrade.exe` 是否在 `update/` 子目录下，manifest 的 `updatePath` 是否正确 |
| Upgrade 报错退出 | 检查目录写入权限、杀毒软件是否拦截 |
| 端口被占用 | 修改 Server 启动参数 `--Urls http://0.0.0.0:5001`，同步修改 Client 的 `UpdateUrl` |
:::

---

## 下一步

跑通这条闭环后，建议按顺序深入阅读：

1. [GeneralUpdate.Core](../doc/GeneralUpdate.Core)：更新策略、事件通知、静默更新、manifest 极简配置
2. [GeneralUpdate.Differential](../doc/GeneralUpdate.Differential)：差分算法、并行处理和 Clean/Dirty
3. [GeneralUpdate.Bowl](../doc/GeneralUpdate.Bowl)：崩溃监控、备份和失败恢复
4. [GeneralUpdate.Tools](./GeneralUpdate.PacketTool)：补丁包、Hash、OSS Config、Simulation 的完整操作指南

## 安装目录注意事项

更新过程中，应用程序需要对安装目录进行读写操作。为避免触发 Windows UAC（用户账户控制）弹窗，请将应用安装在以下非系统保护的目录：

| 推荐目录 | 路径示例 |
| --- | --- |
| 用户 AppData（推荐） | `C:\Users\<用户名>\AppData\Local\<你的应用名>\` |
| 用户主目录 | `C:\Users\<用户名>\<你的应用名>\` |
| 非系统盘自定义目录 | `D:\<你的应用名>\` |

**不推荐的目录：**

- `C:\Program Files\` — 需要管理员权限才能写入
- `C:\Program Files (x86)\` — 需要管理员权限才能写入
- `C:\Windows\` — 系统目录，禁止写入
- 系统盘根目录（如 `C:\`） — 可能触发权限限制

> **提示**：如果在更新时遇到"权限不足"或 UAC 弹窗，请检查应用是否安装在不推荐的目录中。将应用迁移到用户目录通常可以解决此问题。

### 关于加密文件与加壳程序

> **注意**：使用加壳工具（如 Themida、VMProtect）、代码混淆器或加密的二进制文件**无法进行二进制差分更新**。加壳/加密会导致整个文件在新旧版本间完全不同，差分算法失效，补丁包体积等同于全量文件。如需差分更新，请确保原始发布文件未被加壳或加密。

## 示例仓库
| 仓库 | 地址 |
| --- | --- |
| Samples（Server + Hub） | [GeneralUpdate-Samples](https://github.com/GeneralLibrary/GeneralUpdate-Samples) |
| Tools（GUI 配置工具） | [GeneralUpdate.Tools](https://github.com/Juster/GeneralUpdate.Tools) |
| Core（NuGet 包源码） | [GeneralUpdate](https://github.com/GeneralLibrary/GeneralUpdate) |
