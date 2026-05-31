---
sidebar_position: 5
---

# GeneralUpdate.Core

## 组件定位

`GeneralUpdate.Core` 是 GeneralUpdate 的更新执行核心。它负责把“检查版本、下载更新包、校验、解压/合并、替换文件、启动目标程序”等步骤串成一个完整流程，并通过 `GeneralUpdateBootstrap` 提供统一入口。

Core 既可以运行在主程序内执行 `Client` / `OssClient` 流程，也可以作为独立升级程序执行 `Upgrade` / `OssUpgrade` 流程。实际项目中最常见的部署方式是：

1. 主程序负责启动更新检查。
2. Core 在客户端流程中获取版本清单并下载更新包。
3. Core 启动独立升级程序，升级程序关闭占用进程后完成文件替换。
4. 升级完成后重新启动主程序。

> 固件升级不属于本页范围；驱动安装能力请参考后续 `GeneralUpdate.Drivelution` 文档。

**命名空间:** `GeneralUpdate.Core`
**主要入口:** `GeneralUpdateBootstrap`
**NuGet 包:** `GeneralUpdate.Core`

```bash
dotnet add package GeneralUpdate.Core
```

## 适用场景

| 场景 | 是否适合使用 Core |
| --- | --- |
| 桌面应用自更新 | 适合。主程序检查更新，升级程序替换文件。 |
| 需要多版本连续升级 | 适合。Core 可以按服务端返回的版本序列处理多个包。 |
| 需要差分补丁更新 | 适合。配合差分包和 `Option.PatchEnabled` / `Option.DiffMode` 使用。 |
| 需要云存储分发更新包 | 适合。使用 `OssClient` / `OssUpgrade` 角色。 |
| 固件刷写 | 不适合。本页不覆盖固件升级组件。 |

## 运行角色

Core 通过 `Option.AppType` 决定当前进程承担的更新角色：

| AppType | 角色 | 说明 |
| --- | --- | --- |
| `Client` | 主程序侧更新流程 | 检查服务端版本、下载包、准备升级上下文，并启动升级程序。默认值。 |
| `Upgrade` | 独立升级程序流程 | 读取主程序传入的 IPC 数据，执行文件替换，再启动主程序。 |
| `OssClient` | OSS 主程序侧流程 | 从 OSS 配置检查更新并启动 OSS 升级程序。 |
| `OssUpgrade` | OSS 升级程序流程 | 从 OSS 下载并部署更新包。 |

升级程序通常不需要手动调用 `SetConfig`。当它由主程序启动时，Core 会通过加密文件 IPC 自动读取 `ProcessContract`，恢复安装路径、版本、临时目录、下载配置、更新包列表等上下文。

## 最小升级程序

`GeneralUpdate-Samples/src/Upgrade/Program.cs` 展示了独立升级程序的最小形态。该程序只需要注册必要事件，然后调用 `LaunchAsync()`：

```csharp
using GeneralUpdate.Common.Download;
using GeneralUpdate.Common.Internal;
using GeneralUpdate.Common.Shared.Object;
using GeneralUpdate.Core;

try
{
    Console.WriteLine($"Updater started at {DateTime.Now}");

    _ = await new GeneralUpdateBootstrap()
        .AddListenerMultiDownloadStatistics(OnMultiDownloadStatistics)
        .AddListenerMultiDownloadCompleted(OnMultiDownloadCompleted)
        .AddListenerMultiAllDownloadCompleted(OnMultiAllDownloadCompleted)
        .AddListenerMultiDownloadError(OnMultiDownloadError)
        .AddListenerException(OnException)
        .LaunchAsync();
}
catch (Exception ex)
{
    Console.WriteLine(ex);
}

void OnMultiDownloadStatistics(object sender, MultiDownloadStatisticsEventArgs args)
{
    var version = args.Version as VersionInfo;
    Console.WriteLine(
        $"Version: {version?.Version}, Speed: {args.Speed}, Progress: {args.ProgressPercentage}%");
}

void OnMultiDownloadCompleted(object sender, MultiDownloadCompletedEventArgs args)
{
    var version = args.Version as VersionInfo;
    Console.WriteLine(args.IsComplated
        ? $"Version {version?.Version} download completed."
        : $"Version {version?.Version} download failed.");
}

void OnMultiAllDownloadCompleted(object sender, MultiAllDownloadCompletedEventArgs args)
{
    Console.WriteLine(args.IsAllDownloadCompleted
        ? "All download tasks completed."
        : $"Download failed. Failed versions: {args.FailedVersions.Count}");
}

void OnMultiDownloadError(object sender, MultiDownloadErrorEventArgs args)
{
    var version = args.Version as VersionInfo;
    Console.WriteLine($"Version {version?.Version} download error: {args.Exception}");
}

void OnException(object sender, ExceptionEventArgs args)
{
    Console.WriteLine(args.Exception);
}
```

## 主程序侧配置示例

如果你在主程序内直接使用 Core 的 `Client` 流程，可以显式传入 `UpdateRequest`：

```csharp
using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;

await new GeneralUpdateBootstrap()
    .SetConfig(new UpdateRequest
    {
        UpdateUrl = "https://update.example.com/api/upgrade/verification",
        ReportUrl = "https://update.example.com/api/upgrade/report",
        UpdateAppName = "UpgradeSample.exe",
        MainAppName = "ClientSample.exe",
        InstallPath = AppDomain.CurrentDomain.BaseDirectory,
        ClientVersion = "1.0.0",
        AppSecretKey = "your-app-secret",
        ProductId = "your-product-id"
    })
    .SetOption(Option.AppType, AppType.Client)
    .SetOption(Option.DownloadTimeout, 60)
    .SetOption(Option.MaxConcurrency, 3)
    .AddListenerUpdateInfo((sender, args) =>
    {
        Console.WriteLine($"Server returned {args.Info.Body?.Count ?? 0} update versions.");
    })
    .AddListenerException((sender, args) =>
    {
        Console.WriteLine(args.Exception);
    })
    .LaunchAsync();
```

也可以使用 `SetSource` 走简化配置路径：

```csharp
await new GeneralUpdateBootstrap()
    .SetSource(
        updateUrl: "https://update.example.com/api/upgrade/verification",
        appSecretKey: "your-app-secret",
        reportUrl: "https://update.example.com/api/upgrade/report")
    .SetOption(Option.AppType, AppType.Client)
    .LaunchAsync();
```

## 核心执行流程

1. `SetConfig` / `SetSource` 载入更新地址、应用名称、版本号、密钥、安装目录等配置。
2. `LaunchAsync` 根据 `Option.AppType` 选择 `ClientStrategy`、`UpdateStrategy` 或 OSS 策略。
3. Client 流程向服务端请求版本信息，并触发 `AddListenerUpdateInfo` / `AddListenerUpdatePrecheck`。
4. Core 下载需要更新的版本包，并通过下载事件持续回调速度、进度和错误。
5. 下载完成后执行校验、解压、差分合并和文件替换等管道步骤。
6. Upgrade 流程完成后根据 `Option.LaunchClientAfterUpdate` 决定是否启动主程序。
7. 如配置了 `ReportUrl` 或自定义 `UpdateReporter`，Core 会上报更新状态。

## 常用配置项

使用 `SetOption(Option.Xxx, value)` 设置运行时选项：

| 选项 | 默认值 | 说明 |
| --- | --- | --- |
| `Option.AppType` | `AppType.Client` | 当前进程角色。 |
| `Option.Encoding` | `Encoding.UTF8` | 压缩包文件名/内容处理编码。 |
| `Option.Format` | `Format.Zip` | 更新包压缩格式。 |
| `Option.DownloadTimeout` | `30` | 下载超时时间，单位为秒。 |
| `Option.PatchEnabled` | `true` | 是否启用差分补丁处理。 |
| `Option.BackupEnabled` | `true` | 是否在更新前备份被替换文件。 |
| `Option.MaxConcurrency` | `3` | 多文件下载最大并发数。 |
| `Option.EnableResume` | `true` | 是否启用断点续传。 |
| `Option.RetryCount` | `3` | 下载失败重试次数。 |
| `Option.RetryInterval` | `1s` | 下载重试间隔。 |
| `Option.VerifyChecksum` | `true` | 是否校验文件 Hash。 |
| `Option.DiffMode` | `DiffMode.Serial` | 差分合并执行模式。 |
| `Option.Silent` | `false` | 是否启用静默轮询更新。 |
| `Option.SilentPollIntervalMinutes` | `60` | 静默模式检查更新间隔。 |
| `Option.LaunchClientAfterUpdate` | `true` | 升级后是否启动主程序。 |

示例：

```csharp
await new GeneralUpdateBootstrap()
    .SetConfig(request)
    .SetOption(Option.AppType, AppType.Client)
    .SetOption(Option.PatchEnabled, true)
    .SetOption(Option.BackupEnabled, true)
    .SetOption(Option.VerifyChecksum, true)
    .SetOption(Option.MaxConcurrency, 4)
    .LaunchAsync();
```

## 事件监听

Core 通过事件监听器暴露更新过程状态：

| 方法 | 触发时机 | 典型用途 |
| --- | --- | --- |
| `AddListenerUpdateInfo` | 服务端返回版本信息后 | 展示更新日志、版本列表、更新大小。 |
| `AddListenerUpdatePrecheck` | 下载开始前 | 检查磁盘空间、网络环境、用户确认。 |
| `AddListenerMultiDownloadStatistics` | 下载过程中持续触发 | 展示速度、剩余时间、百分比。 |
| `AddListenerMultiDownloadCompleted` | 单个版本包下载完成 | 记录每个版本下载结果。 |
| `AddListenerMultiAllDownloadCompleted` | 全部下载任务完成 | 切换 UI 状态或写日志。 |
| `AddListenerMultiDownloadError` | 下载任务失败 | 输出失败版本和异常。 |
| `AddListenerProgress` | 通用更新进度变化 | 对接统一进度条。 |
| `AddListenerException` | Core 捕获到异常 | 写入日志、提示用户或上报。 |

如果不想逐个注册事件，可以实现 `IUpdateEventListener` 后使用 `AddEventListener<TListener>()` 批量注册。

## 静默更新

静默更新适合后台定期检查更新。启用后，`Client` 流程会启动后台轮询并立即返回，更新准备完成后在进程退出时继续升级。

```csharp
await new GeneralUpdateBootstrap()
    .SetConfig(request)
    .SetOption(Option.AppType, AppType.Client)
    .SetOption(Option.Silent, true)
    .SetOption(Option.SilentPollIntervalMinutes, 30)
    .SetOption(Option.LaunchClientAfterUpdate, true)
    .LaunchAsync();
```

静默更新仍然需要正确配置更新地址、应用密钥、主程序名称、升级程序名称和安装目录。

## 扩展点

`GeneralUpdateBootstrap` 继承自 `AbstractBootstrap`，可以替换多个内部组件：

| 方法 | 用途 |
| --- | --- |
| `Strategy<T>()` | 指定自定义平台策略。 |
| `Hooks<T>()` | 注入更新前、下载后、更新后、启动前、异常时的生命周期钩子。 |
| `UpdateReporter<T>()` | 自定义更新状态上报。 |
| `SslPolicy<T>()` | 自定义 HTTPS 证书校验策略。 |
| `UpdateAuth<T>()` | 为 HTTP 请求添加认证信息。 |
| `DownloadSource<T>()` | 自定义版本清单和文件来源。 |
| `DownloadPolicy<T>()` | 自定义下载重试/超时策略。 |
| `DownloadExecutor<T>()` | 自定义单文件下载实现。 |
| `DownloadPipeline<T>()` | 自定义下载后处理，例如解密、杀毒、校验。 |
| `DownloadOrchestrator<T>()` | 完全接管批量下载流程。 |

高级项目可以只替换某一个环节，而不需要 fork Core。

## 与 GeneralUpdate.Tools 的关系

Core 消费的是更新服务端或 OSS 返回的版本清单和更新包。`GeneralUpdate.Tools` 用来帮助生成和验证这些输入：

- 使用 Patch Package 生成差分更新包。
- 使用 Extension Package 生成扩展包。
- 使用 OSS Config 准备云存储分发配置。
- 使用模拟、报告和 Hash 相关能力提前验证包结构与完整性。

推荐流程是：先用 Tools 生成并校验更新包，再把包和版本清单发布到服务端或 OSS，最后由 Core 在客户端执行更新。

## 常见问题

### 升级程序启动后没有执行更新

确认它是否由主程序启动。如果直接双击独立升级程序，通常没有 IPC 上下文，`Upgrade` 流程无法知道安装目录和待更新版本。开发调试时可以先从主程序触发完整流程。

### 文件替换失败

通常是主程序或 Bowl 进程仍占用文件。确认主程序已退出，并正确配置 `Bowl` 进程名或相关关闭逻辑。

### 下载成功但校验失败

确认服务端或 OSS 上的包没有被重新压缩、截断或替换。若启用了 `Option.VerifyChecksum`，客户端收到的 Hash 必须和清单中的 Hash 一致。

### 差分包没有生效

确认服务端返回的是差分包清单，并且 `Option.PatchEnabled` 为 `true`。差分包建议通过 `GeneralUpdate.Tools` 生成，避免手工组织目录导致清单和包内容不一致。

## 相关示例

- [Upgrade sample](https://github.com/GeneralLibrary/GeneralUpdate-Samples/blob/main/src/Upgrade/Program.cs)
- [OSS upgrade sample](https://github.com/GeneralLibrary/GeneralUpdate-Samples/tree/main/src/OSS/OSSUpgradeSample)
- [GeneralUpdate repository](https://github.com/GeneralLibrary/GeneralUpdate)
- [GeneralUpdate.Tools repository](https://github.com/GeneralLibrary/GeneralUpdate.Tools)
