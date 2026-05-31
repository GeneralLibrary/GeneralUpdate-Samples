---
sidebar_position: 3
title: Beginner cookbook
---

# GeneralUpdate 入门实战手册

这篇手册面向第一次接触 GeneralUpdate 的开发者。目标不是一次讲完所有 API，而是让你先跑通一次“发现更新 → 下载补丁 → 启动 Upgrade → 应用补丁 → 回到新版本”的闭环，然后知道每个组件在流程里负责什么。

## 你会用到哪些角色

| 角色 | 在样例中的位置 | 负责什么 | 深入阅读 |
| --- | --- | --- | --- |
| Client | `src\Client` | 主程序，向服务端检查版本，下载更新包，并拉起 Upgrade | [GeneralUpdate.Core](../doc/GeneralUpdate.Core.md) |
| Upgrade | `src\Upgrade` | 独立升级程序，接收 Client 传递的上下文并应用补丁 | [GeneralUpdate.Core](../doc/GeneralUpdate.Core.md) |
| Server | `src\Server` | 返回版本信息、接收更新报告、提供补丁下载 | [GeneralUpdate.Core](../doc/GeneralUpdate.Core.md) |
| Packet | `src\Server\wwwroot\packages` | 可下载的 `.zip` 更新包和 `versions.json` 元数据 | [GeneralUpdate.Tools](../doc/GeneralUpdate.PacketTool.md) |
| Tools | GeneralUpdate.Tools 仓库 | 生成补丁包、Hash、OSS 清单、manifest 和仿真报告 | [GeneralUpdate.Tools](../doc/GeneralUpdate.PacketTool.md) |
| Bowl | `src\Bowl` | 监控进程异常并导出失败信息 | [GeneralUpdate.Bowl](../doc/GeneralUpdate.Bowl.md) |
| Differential | `src\Diff` / Core 默认集成 | 对 old/new 文件生成差分，并在 Upgrade 阶段应用 | [GeneralUpdate.Differential](../doc/GeneralUpdate.Differential.md) |

## Step 1：准备仓库和运行环境

克隆 Samples 仓库，并确认本机可以执行 `dotnet`：

```powershell
git clone https://github.com/GeneralLibrary/GeneralUpdate-Samples.git
cd GeneralUpdate-Samples\src
dotnet --info
```

当前 Samples 主流程项目面向 `net8.0`，所以至少需要能构建 .NET 8 项目。GeneralUpdate.Tools 当前源码使用 .NET 10，如果后续要运行 Tools 源码，请额外安装 .NET 10 SDK。

**预期结果**：`dotnet --info` 能输出 SDK 信息，`src` 目录下能看到 `Client`、`Upgrade`、`Server`、`Bowl`、`Diff` 等子目录。

## Step 2：先跑通内置更新样例

在 `src` 目录执行：

```powershell
.\start.cmd
```

最新脚本会依次执行 `Server\build.bat`、`Upgrade\build.bat`、`Client\build.bat`，然后删除旧的 `src\run`，把三个项目的 Release 输出复制到 `src\run`，最后启动：

```text
src\run\ServerSample.exe
src\run\ClientSample.exe
```

**预期结果**：

1. Server 控制台显示 `GeneralUpdate Sample Upgrade Server`，监听 `http://localhost:5000/`。
2. Server 读取 `src\Server\wwwroot\packages\versions.json` 中的包信息。
3. Client 控制台显示主程序初始化、请求更新、下载进度、更新信息回调。
4. Client 完成下载后会拉起 Upgrade，Upgrade 控制台显示升级程序初始化和补丁处理进度。

## Step 3：看懂 Server 返回了什么

Client 的配置在 `src\Client\Program.cs`：

```csharp
ReportUrl = "http://127.0.0.1:5000/Upgrade/Report",
UpdateUrl = "http://127.0.0.1:5000/Upgrade/Verification",
AppName = "UpgradeSample.exe",
MainAppName = "ClientSample.exe",
ClientVersion = "1.0.0.0",
UpgradeClientVersion = "1.0.0.0",
ProductId = "2d974e2a-31e6-4887-9bb1-b4689e98c77a",
AppSecretKey = "dfeb5833-975e-4afb-88f1-6278ee9aeff6",
```

Server 的 `POST /Upgrade/Verification` 会读取客户端版本、AppType、Platform、ProductId 和 UpgradeMode，然后从 `versions.json` 里筛选更高版本。返回结果包含 `RecordId`、`Name`、`Hash`、`Url`、`Version`、`AppType`、`Platform`、`Format`、`Size` 等字段。

**预期结果**：当 Client 当前版本是 `1.0.0.0`，而 `versions.json` 里存在 `2.0.0.0` 包时，Client 会收到可更新版本，并在 `AddListenerUpdateInfo` 中打印版本数量。

## Step 4：观察 Upgrade 做了什么

Upgrade 项目入口在 `src\Upgrade\Program.cs`。它不再手写服务端地址，而是通过 Client 传递的更新上下文启动：

```csharp
await new GeneralUpdateBootstrap()
    .AddListenerMultiDownloadStatistics(OnMultiDownloadStatistics)
    .AddListenerMultiDownloadCompleted(OnMultiDownloadCompleted)
    .AddListenerMultiAllDownloadCompleted(OnMultiAllDownloadCompleted)
    .AddListenerMultiDownloadError(OnMultiDownloadError)
    .AddListenerException(OnException)
    .LaunchAsync();
```

Upgrade 会处理下载后的补丁包，应用差分文件，复制新增文件，并根据删除清单移除旧文件。

**预期结果**：Upgrade 控制台能看到下载/应用进度；Server 控制台能看到 `/Upgrade/Report` 或 `/Update/Report` 的状态上报；`src\run` 中的程序文件被更新到新版本。

## Step 5：用 Tools 生成自己的补丁包

当你已经有两个发布目录时，可以用 GeneralUpdate.Tools 的 Patch 页面生成补丁：

| Tools 字段 | 应该选择什么 |
| --- | --- |
| Old Directory | 用户当前安装的旧版本目录，例如 `publish\v1.0.0` |
| New Directory | 你准备发布的新版本目录，例如 `publish\v2.0.0` |
| Package Name | 建议包含版本，例如 `client_1.0.0_to_2.0.0` |
| Version | 目标版本，例如 `2.0.0` |
| Output Directory | 保存补丁 ZIP 的目录 |

Tools 会调用 Core 差分管线：旧文件和新文件不同则生成 `.patch`，新文件直接复制，删除的文件写入 `generalupdate.delete.json`，最后压缩为 ZIP。

**预期结果**：输出目录出现 `{PackageName}.zip`。如果修改很小，差分包通常明显小于完整发布目录。

## Step 6：发布补丁包给 Server

Samples Server 默认从这里读取包：

```text
src\Server\wwwroot\packages\
```

你需要把补丁 ZIP 放到该目录，并更新同目录下的 `versions.json`。一条版本记录至少要让 Server 能匹配和下载：

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

**预期结果**：重启 Server 后，启动横幅会列出加载到的版本；Client 再次请求时能拿到你刚发布的版本记录。

## Step 7：触发一次自己的更新

确认 `src\Client\Program.cs` 中的 `ClientVersion` 低于 `versions.json` 中的 `Version`，然后重新运行 `.\start.cmd` 或手动启动 `ServerSample.exe` 和 `ClientSample.exe`。

如果你想让用户先看更新内容再决定是否更新，可以在 Client 里使用 `AddListenerUpdatePrecheck`。这个回调不只能决定是否跳过更新，还能拿到本次更新的版本信息，适合整理成弹窗、展示更新日志、文件大小和强制更新标记。

**预期结果**：Client 收到更新信息，预检回调打印版本数量；返回 `false` 时继续更新，返回 `true` 时跳过本次非强制更新。

## Step 8：给样例加上 Bowl 可靠性保护

Bowl 不是更新包生成工具，它用于守护进程和收集失败现场。样例在 `src\Bowl\BowlSample\Program.cs`：

```csharp
var processInfo = new MonitorParameter
{
    ProcessNameOrId = "ThrowApp.exe",
    DumpFileName = $"{lastVersion}_fail.dmp",
    FailFileName = $"{lastVersion}_fail.json",
    TargetPath = installPath,
    FailDirectory = Path.Combine(installPath, "fail", lastVersion),
    BackupDirectory = Path.Combine(installPath, lastVersion),
    WorkModel = "Normal"
};
Bowl.Launch(processInfo);
```

把它放进真实产品时，通常让 Bowl 监控 Client 或关键业务进程。进程异常时，Bowl 会把 dump 和失败信息写入 `fail\{version}`，方便定位“更新后启动失败”这类问题。

**预期结果**：被监控进程异常退出后，`fail` 目录出现 `.dmp` 和 `.json` 失败信息。

## Step 9：理解 Differential 在哪里生效

你不需要在普通 Core 更新流程里额外集成 Differential：Core 默认使用差分管线处理补丁。你需要理解的是两个阶段：

| 阶段 | 方法概念 | 谁执行 | 结果 |
| --- | --- | --- | --- |
| Clean | old → new 生成 patch | Tools、CI 或发布端 | `.patch`、新增文件、`generalupdate.delete.json` |
| Dirty | app + patch 应用更新 | Upgrade 端 | 本地文件更新到目标版本 |

`src\Diff\Program.cs` 展示了底层调用方式：

```csharp
await DifferentialCore.Clean(source, target, patch, new DefaultCleanStrategy(new DefaultCleanMatcher()));
await DifferentialCore.Dirty(source, patch, new DefaultDirtyStrategy(new DefaultDirtyMatcher()));
```

**预期结果**：当你只改动少量文件时，Clean 输出的 patch 目录只包含变更内容；Dirty 后旧目录变成新版本目录。

## 下一步

跑通这条链路后，建议按顺序阅读：

1. [GeneralUpdate.Core](../doc/GeneralUpdate.Core.md)：更新策略、事件通知、静默更新、manifest 极简配置。
2. [GeneralUpdate.Tools](../doc/GeneralUpdate.PacketTool.md)：补丁包、Hash、OSS Config、Simulation。
3. [GeneralUpdate.Differential](../doc/GeneralUpdate.Differential.md)：差分算法、并行处理和 Clean/Dirty。
4. [GeneralUpdate.Bowl](../doc/GeneralUpdate.Bowl.md)：崩溃监控、备份和失败恢复。
