---
sidebar_position: 3
title: 入门实战手册
---

# GeneralUpdate 入门实战手册

这篇手册面向第一次接触 GeneralUpdate 的开发者。目标不是一次讲完所有 API，而是让你先跑通一次"发现更新 → 下载补丁 → 启动升级 → 应用补丁 → 回到新版本"的闭环，然后知道每个组件在流程里负责什么。

## 你会用到哪些角色

| 角色 | 在样例中的位置 | 负责什么 | 深入阅读 |
| --- | --- | --- | --- |
| Hub | `src\Hub` | 交互式示例浏览器，通过菜单选择并运行各类更新场景 | [GeneralUpdate.Core](../doc/GeneralUpdate.Core.md) |
| Server | `src\Server` | 返回版本信息、接收更新报告、提供补丁下载 | [GeneralUpdate.Core](../doc/GeneralUpdate.Core.md) |
| Packet | `src\Server\wwwroot\packages` | 可下载的 `.zip` 更新包和 `versions.json` 元数据 | [GeneralUpdate.Tools](../doc/GeneralUpdate.PacketTool.md) |
| Tools | GeneralUpdate.Tools 仓库 | 生成补丁包、Hash、OSS 清单、manifest 和仿真报告 | [GeneralUpdate.Tools](../doc/GeneralUpdate.PacketTool.md) |
| Bowl | Hub Samples 中集成 | 监控进程异常并导出失败信息 | [GeneralUpdate.Bowl](../doc/GeneralUpdate.Bowl.md) |
| Differential | Hub Samples + Core 默认集成 | 对 old/new 文件生成差分，并在更新阶段应用 | [GeneralUpdate.Differential](../doc/GeneralUpdate.Differential.md) |

## Step 1：准备仓库和运行环境

克隆 Samples 仓库，并确认本机可以执行 `dotnet`：

```powershell
git clone https://github.com/GeneralLibrary/GeneralUpdate-Samples.git
cd GeneralUpdate-Samples
dotnet --info
```

当前 Samples Hub 面向 `net10.0`，需要安装 [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)。

**预期结果**：`dotnet --info` 能输出 .NET 10 SDK 信息，`src` 目录下能看到 `Hub`、`Server`、`ImDiskDriver`、`content_client`、`content_upgrade`、`gen_packages.ps1`、`Run.cmd`、`Run.ps1` 等。

## Step 2：先跑通内置更新样例

### 2.1 首次运行 — 编译本地组件 DLL

从源码克隆下来后，`src\Hub\libs\` 目录下包含预编译的 DLL 文件。如果你修改了 GeneralUpdate 组件的源码，可以重新编译并复制 DLL：

```powershell
cd src
.\Run.ps1 -BuildLibs
```

`-BuildLibs` 会自动查找同级目录 `..\GeneralUpdate\src\c#` 下的源码，依次编译 `GeneralUpdate.Differential`、`GeneralUpdate.Core`、`GeneralUpdate.Bowl`、`GeneralUpdate.Extension`、`GeneralUpdate.Drivelution`，并将 DLL 复制到 `src\Hub\libs\`。

如果只想快速体验（无需编译源码，使用预置 DLL），直接运行即可：

```powershell
cd src
.\Run.cmd
```

或使用 PowerShell：

```powershell
cd src
.\Run.ps1
```

### 2.2 示例菜单

启动后会看到交互式菜单：

```text
  ╔══════════════════════════════════════╗
  ║    GeneralUpdate 示例浏览器          ║
  ╚══════════════════════════════════════╝

  配置: http://localhost:5000

  ═══════════════════════════════════
    1. 完整更新 — 版本发现→下载→应用
    2. OSS 更新
    3. 静默更新
    4. 推送更新
    5. 差分算法
    6. 压缩演示
    7. 扩展管理
    8. Bowl 进程守护
    9. ImDisk 快速安装
  ───────────────────────────────────
    0. 退出 (Exit)
  ═══════════════════════════════════
```

### 2.3 运行第一个样例

输入 `1` 并按回车，选择"完整更新"。Hub 会自动：

1. **启动 Server**（如果尚未运行）— 输出 `[Server] 启动中... ✓`
2. 在 `mock_app` 目录创建模拟的 v1.0.0.0 应用文件
3. 通过 `GeneralUpdateBootstrap` 向 `http://localhost:5000/Upgrade/Verification` 请求更新
4. 下载新版本包并应用更新
5. 打印更新后的文件列表

**预期结果**：

- Server 控制台显示 `GeneralUpdate Sample Upgrade Server`，监听 `http://localhost:5000/`
- Hub 控制台显示版本发现、下载进度、更新完成信息
- `mock_app` 目录中的文件从 v1.0.0.0 更新到 v2.0.0.0

## Step 3：看懂 Server 返回了什么

Hub 中每个 Sample 通过 `appsettings.json` 统一配置，再由各 Sample 的代码设置更新参数。查看 [src\Hub\appsettings.json](https://github.com/GeneralLibrary/GeneralUpdate-Samples/blob/main/src/Hub/appsettings.json)：

```json
{
  "ServerUrl": "http://localhost:5000",
  "AppSecretKey": "dfeb5833-975e-4afb-88f1-6278ee9aeff6",
  "ProductId": "2d974e2a-31e6-4887-9bb1-b4689e98c77a",
  "ClientVersion": "1.0.0.0",
  "UpgradeClientVersion": "1.0.0.0",
  "MainAppName": "Hub.exe",
  "UpgradeAppName": "Hub.exe"
}
```

"完整更新"样例中 [CompleteUpdateSample.cs](https://github.com/GeneralLibrary/GeneralUpdate-Samples/blob/main/src/Hub/Samples/CompleteUpdateSample.cs) 使用 `UpdateRequest` 设置更新参数：

```csharp
var request = new UpdateRequest
{
    UpdateUrl = $"{config.ServerUrl}/Upgrade/Verification",
    ReportUrl = $"{config.ServerUrl}/Upgrade/Report",
    AppSecretKey = config.AppSecretKey,
    InstallPath = mockAppDir,
    ClientVersion = config.ClientVersion,
    MainAppName = config.MainAppName,
    UpdateAppName = config.UpgradeAppName,
    ProductId = config.ProductId
};
```

Server 的 `POST /Upgrade/Verification` 会读取客户端版本、AppType、Platform、ProductId 和 UpgradeMode，然后从 `versions.json` 里筛选更高版本。返回结果包含 `RecordId`、`Name`、`Hash`、`Url`、`Version`、`AppType`、`Platform`、`Format`、`Size` 等字段。

**预期结果**：当 Client 当前版本是 `1.0.0.0`，而 `versions.json` 里存在 `2.0.0.0` 包时，Client 会收到可更新版本。

## Step 4：理解 GeneralUpdateBootstrap 的流程

每个 Sample 的核心都是一个 `GeneralUpdateBootstrap` 实例。以"完整更新"为例：

```csharp
var bootstrap = new GeneralUpdateBootstrap()
    .SetConfig(request)
    .SetOption(Option.AppType, AppType.Client)
    .AddListenerUpdateInfo((_, e) => { /* 处理版本信息 */ })
    .AddListenerMultiDownloadStatistics((_, e) => { /* 处理下载进度 */ })
    .AddListenerException((_, e) => { /* 处理异常 */ });

await bootstrap.LaunchAsync();
```

`GeneralUpdate.Core` 统一了原来的 `GeneralUpdate.ClientCore` 和 `GeneralUpdate.Common`，现在只需要引用一个 NuGet 包即可获得全部能力：

- **客户端更新管理**：版本检查、更新包下载、完整性验证、拉起升级程序
- **升级执行引擎**：独立进程升级、文件替换、差分包应用、驱动安装
- **公共基础设施**：生命周期追踪、下载引擎、序列化等底层能力

**预期结果**：`LaunchAsync()` 执行完成后，目标目录的文件已被更新到新版本。

## Step 5：用 Tools 生成自己的补丁包

当你已经有两个发布目录时，可以用 GeneralUpdate.Tools 的 Patch 页面生成补丁：

| Tools 字段 | 应该选择什么 |
| --- | --- |
| Old Directory | 用户当前安装的旧版本目录，例如 `publish\v1.0.0` |
| New Directory | 你准备发布的新版本目录，例如 `publish\v2.0.0` |
| Package Name | 建议包含版本，例如 `client_1.0.0_to_2.0.0` |
| Version | 目标版本，例如 `2.0.0` |
| Output Directory | 保存补丁 ZIP 的目录 |

Samples 仓库也提供了 `gen_packages.ps1` 脚本用于快速生成测试包：

```powershell
cd src
.\gen_packages.ps1
```

该脚本使用 `src\content_client\v1.0.0.0\` 和 `src\content_client\v2.0.0.0\` 的内容生成测试补丁包，输出到 `src\Server\wwwroot\packages\`。

Tools 会调用 Core 差分管线：旧文件和新文件不同则生成 `.patch`，新文件直接复制，删除的文件写入 `generalupdate.delete.json`，最后压缩为 ZIP。

**预期结果**：输出目录出现更新包 ZIP 文件，Server 的 `versions.json` 被更新。

## Step 6：发布补丁包给 Server

Samples Server 默认从这里读取包：

```text
src\Server\wwwroot\packages\
```

你需要把补丁 ZIP 放到该目录，并确保同目录下的 `versions.json` 包含对应记录。一条版本记录至少要让 Server 能匹配和下载：

```json
{
  "PacketName": "client_1.0.0_to_2.0.0",
  "Hash": "补丁ZIP的sha256",
  "Version": "2.0.0.0",
  "Url": "http://localhost:5000/File/Download/补丁ZIP的sha256",
  "AppType": 1,
  "Platform": 1,
  "ProductId": "2d974e2a-31e6-4887-9bb1-b4689e98c77a",
  "Format": ".zip",
  "IsCrossVersion": true,
  "FromVersion": "1.0.0.0",
  "ToVersion": "2.0.0.0"
}
```

Hash 可以用 Tools 的 OSS 页面计算，也可以在 CI 中计算。Server 下载接口是 `GET /File/Download/{hash}`，支持 HTTP Range，便于断点续传。

**预期结果**：重启 Hub 后运行样例，Server 启动横幅会列出加载到的版本；请求时能拿到你刚发布的版本记录。

## Step 7：探索其他内置样例

Hub 提供了 9 个内置样例，覆盖了 GeneralUpdate 的主要使用场景：

| 编号 | 样例 | 说明 |
| --- | --- | --- |
| 1 | 完整更新 | 标准更新闭环：版本发现→下载→应用 |
| 2 | OSS 更新 | 基于对象存储的更新模式 |
| 3 | 静默更新 | 后台下载，用户无感知 |
| 4 | 推送更新 | 基于 SignalR 的推送实时更新 |
| 5 | 差分算法 | 二进制差分 Clean/Dirty 演示 |
| 6 | 压缩演示 | 压缩中间件演示 |
| 7 | 扩展管理 | 插件/扩展的查询、下载、安装 |
| 8 | Bowl 进程守护 | 进程崩溃监控与 Dump 收集 |
| 9 | ImDisk 快速安装 | 驱动级快速安装演示 |

建议依次运行每个样例，观察它们在 `mock_app` 目录中的操作结果。

**预期结果**：每个样例运行后，控制台会明确显示该场景的关键步骤和结果。

## 下一步

跑通这条链路后，建议按顺序阅读：

1. [GeneralUpdate.Core](../doc/GeneralUpdate.Core.md)：更新策略、事件通知、静默更新、manifest 极简配置。
2. [GeneralUpdate.Tools](../doc/GeneralUpdate.PacketTool.md)：补丁包、Hash、OSS Config、Simulation。
3. [GeneralUpdate.Differential](../doc/GeneralUpdate.Differential.md)：差分算法、并行处理和 Clean/Dirty。
4. [GeneralUpdate.Bowl](../doc/GeneralUpdate.Bowl.md)：崩溃监控、备份和失败恢复。
5. [高级实战手册](./Advanced cookbook.md)：生产环境部署、CI/CD 集成、企业级方案设计。
