---
sidebar_position: 5
---

# GeneralUpdate.Core

`GeneralUpdate.Core` 是 GeneralUpdate 的更新执行核心，重点提供可编程的启动器、配置模型、事件模型、下载子系统扩展点、生命周期钩子、状态上报、差分管道和平台策略扩展。本页聚焦组件 API、属性和扩展方式；完整端到端上手流程会放到 cookbook 中。

**命名空间:** `GeneralUpdate.Core`
**主要入口:** `GeneralUpdateBootstrap`
**NuGet 包:** `GeneralUpdate.Core`

```bash
dotnet add package GeneralUpdate.Core
```

## 组件能力边界

Core 负责“执行更新”，不负责生成更新包，也不直接管理服务端后台。

| 能力 | Core 是否负责 | 说明 |
| --- | --- | --- |
| 读取更新配置 | 是 | 通过 `UpdateRequest`、配置文件、`SetSource` 或 IPC 恢复运行参数。 |
| 检查服务端版本 | 是 | `Client` / `OssClient` 角色会读取版本清单并生成下载计划。 |
| 下载更新包 | 是 | 可替换下载来源、执行器、重试策略、后处理管道或完整编排器。 |
| 校验与应用补丁 | 是 | 支持 Hash 校验、压缩包处理、差分补丁管道。 |
| 文件替换与重启应用 | 是 | `Upgrade` / `OssUpgrade` 角色用于独立升级程序。 |
| 生成差分包 | 否 | 推荐使用 `GeneralUpdate.Tools`。 |

## 入口类：GeneralUpdateBootstrap

`GeneralUpdateBootstrap` 是 Core 的主要门面类。它继承 `AbstractBootstrap<GeneralUpdateBootstrap, IStrategy>`，因此同时拥有自身方法和基类提供的扩展注册方法。

```csharp
using GeneralUpdate.Core;

var bootstrap = new GeneralUpdateBootstrap();
```

### 方法总览

| 方法 | 用途 | 常用场景 |
| --- | --- | --- |
| `LaunchAsync()` | 按当前 `Option.AppType` 启动更新流程。 | 所有 Core 使用场景最终都会调用。 |
| `Cancel()` | 请求取消当前更新操作。 | UI 中提供“取消更新”按钮。 |
| `SetConfig(UpdateRequest)` | 使用强类型对象配置更新。 | 主程序内显式配置更新参数。 |
| `SetConfig(string)` | 从 JSON 文件读取 `UpdateRequest`。 | 将更新参数放到 `update_config.json` 或自定义配置文件。 |
| `SetSource(...)` | 只提供更新地址、密钥、报告地址等基础参数。 | 零配置/轻配置入口。 |
| `SetOption(Option<T>, T)` | 设置运行时选项。 | 设置角色、超时、并发、差分、静默更新等。 |
| `UseDiffPipeline(Action<DiffPipelineBuilder>)` | 自定义差分补丁管道。 | 替换 differ、调整并行度、接入补丁进度。 |
| `AddListenerUpdatePrecheck(Func<UpdateInfoEventArgs, bool>)` | 下载前预检查。 | 检查磁盘空间、网络状态或弹窗确认。 |
| `AddListener...` | 注册单个事件回调。 | 更新 UI、写日志、上报监控。 |
| `AddEventListener<TListener>()` | 批量注册事件监听器。 | 将事件处理封装成类。 |

### LaunchAsync

```csharp
public Task<GeneralUpdateBootstrap> LaunchAsync()
```

`LaunchAsync` 会读取 `Option.AppType` 并选择对应策略：

| `Option.AppType` | 策略 | 说明 |
| --- | --- | --- |
| `AppType.Client` | `ClientStrategy` | 主程序侧：检查版本、下载包、准备升级上下文、启动升级程序。 |
| `AppType.Upgrade` | `UpdateStrategy` | 升级程序侧：读取 IPC 上下文并执行文件替换。 |
| `AppType.OssClient` | `OssStrategy` | OSS 主程序侧更新流程。 |
| `AppType.OssUpgrade` | `OssStrategy` | OSS 升级程序侧更新流程。 |

示例：独立升级程序入口。

```csharp
await new GeneralUpdateBootstrap()
    .SetOption(Option.AppType, AppType.Upgrade)
    .AddListenerException((_, e) => Console.WriteLine(e.Exception))
    .LaunchAsync();
```

> 当升级程序由主程序启动时，Core 会通过加密文件 IPC 自动恢复更新上下文，通常不需要在升级程序里再次调用 `SetConfig`。

### Cancel

```csharp
public void Cancel()
```

`Cancel` 会触发内部 `CancellationTokenSource`，更新策略会在安全检查点观察取消请求。适合 UI 应用把 bootstrap 保存为字段后绑定取消按钮。

```csharp
private GeneralUpdateBootstrap? _bootstrap;

async Task StartUpdateAsync(UpdateRequest request)
{
    _bootstrap = new GeneralUpdateBootstrap()
        .SetConfig(request)
        .AddListenerException((_, e) => Console.WriteLine(e.Exception));

    await _bootstrap.LaunchAsync();
}

void CancelUpdate()
{
    _bootstrap?.Cancel();
}
```

### SetConfig(UpdateRequest)

```csharp
public GeneralUpdateBootstrap SetConfig(UpdateRequest configInfo)
```

`SetConfig(UpdateRequest)` 会调用 `UpdateRequest.Validate()`，并把外部配置映射为内部 `UpdateContext`。当角色不是 `AppType.Upgrade` 时，它还会初始化临时目录和黑名单匹配器。

```csharp
using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;

var request = new UpdateRequest
{
    UpdateUrl = "https://update.example.com/api/upgrade/verification",
    ReportUrl = "https://update.example.com/api/upgrade/report",
    UpdateAppName = "UpgradeSample.exe",
    MainAppName = "ClientSample.exe",
    InstallPath = AppDomain.CurrentDomain.BaseDirectory,
    ClientVersion = "1.0.0",
    AppSecretKey = "your-app-secret",
    ProductId = "your-product-id",
    Files = new List<string> { "appsettings.json" },
    Formats = new List<string> { ".log", ".tmp" },
    Directories = new List<string> { "logs", "cache" }
};

await new GeneralUpdateBootstrap()
    .SetConfig(request)
    .SetOption(Option.AppType, AppType.Client)
    .LaunchAsync();
```

### SetConfig(string)

```csharp
public GeneralUpdateBootstrap SetConfig(string filePath)
```

`SetConfig(string)` 从 UTF-8 JSON 文件读取 `UpdateRequest`。如果只传文件名，会从当前应用基目录解析；如果传相对或绝对路径，会按路径解析。

```json
{
  "updateUrl": "https://update.example.com/api/upgrade/verification",
  "reportUrl": "https://update.example.com/api/upgrade/report",
  "updateAppName": "UpgradeSample.exe",
  "mainAppName": "ClientSample.exe",
  "installPath": "C:\\Program Files\\MyApp",
  "clientVersion": "1.0.0",
  "appSecretKey": "your-app-secret",
  "productId": "your-product-id"
}
```

```csharp
await new GeneralUpdateBootstrap()
    .SetConfig("update_config.json")
    .SetOption(Option.AppType, AppType.Client)
    .LaunchAsync();
```

### SetSource

```csharp
public GeneralUpdateBootstrap SetSource(
    string updateUrl,
    string appSecretKey,
    string? reportUrl = null,
    string? scheme = null,
    string? token = null)
```

`SetSource` 是轻配置入口，适合把应用身份信息放到 `generalupdate.manifest.json`，只在代码中指定服务端入口和密钥。

```csharp
await new GeneralUpdateBootstrap()
    .SetSource(
        updateUrl: "https://update.example.com/api/upgrade/verification",
        appSecretKey: "your-app-secret",
        reportUrl: "https://update.example.com/api/upgrade/report",
        scheme: "Bearer",
        token: "access-token")
    .SetOption(Option.AppType, AppType.Client)
    .LaunchAsync();
```

### UseDiffPipeline

```csharp
public GeneralUpdateBootstrap UseDiffPipeline(Action<DiffPipelineBuilder>? configure)
```

`UseDiffPipeline` 用于替换或调整差分补丁管道。未调用时，Core 会创建默认管道：`BsdiffDiffer`、`DefaultCleanMatcher`、`DefaultDirtyMatcher`、并行度 `2`，并接入 Core 的差分进度事件。

```csharp
using GeneralUpdate.Core.Differential;
using GeneralUpdate.Core.Models;
using GeneralUpdate.Core.Pipeline;
using GeneralUpdate.Differential.Differ;

await new GeneralUpdateBootstrap()
    .SetConfig(request)
    .UseDiffPipeline(builder =>
    {
        builder
            .UseDiffer(new StreamingHdiffDiffer())
            .UseCleanMatcher(new DefaultCleanMatcher())
            .UseDirtyMatcher(new DefaultDirtyMatcher())
            .WithParallelism(4)
            .WithStopOnFirstError(true)
            .WithProgress(new Progress<DiffProgress>(p =>
            {
                Console.WriteLine($"{p.Completed}/{p.Total}: {p.FileName}");
            }));
    })
    .SetOption(Option.PatchEnabled, true)
    .LaunchAsync();
```

## 配置模型：UpdateRequest

`UpdateRequest` 是外部调用者最常用的配置对象。它继承 `UpdateConfiguration`，并在 `Validate()` 中检查关键字段。

### 必填或强烈建议配置的属性

| 属性 | 说明 |
| --- | --- |
| `UpdateUrl` | 更新检查 API 地址。必须是绝对 URL。 |
| `UpdateAppName` | 升级程序文件名，默认 `Update.exe`。如果你的升级程序叫 `UpgradeSample.exe`，必须显式设置。 |
| `MainAppName` | 主程序文件名。用于升级后重新启动，也用于识别要更新的应用。 |
| `ClientVersion` | 当前主程序版本。 |
| `AppSecretKey` | 应用密钥，用于和服务端约定认证。 |
| `InstallPath` | 应用安装目录，默认当前应用基目录。生产环境建议显式设置。 |

### 可选属性

| 属性 | 说明 |
| --- | --- |
| `ReportUrl` | 更新状态上报 API。 |
| `UpdateLogUrl` | 更新日志页面地址。 |
| `UpgradeClientVersion` | 升级程序自身版本。 |
| `ProductId` | 产品标识，同一服务端管理多个产品时使用。 |
| `UpdatePath` | 升级程序所在目录；为空时使用 `InstallPath`。 |
| `Bowl` | 更新前需要关闭的辅助进程名。 |
| `Scheme` / `Token` | 请求认证信息，可与内置认证提供器配合。 |
| `Files` | 更新时跳过的指定文件。 |
| `Formats` | 更新时跳过的扩展名，例如 `.log`。 |
| `Directories` | 更新时跳过的目录。 |

### 使用 UpdateRequestBuilder

`UpdateRequestBuilder` 提供链式构建 API，并在 `Build()` 时执行校验。

```csharp
using GeneralUpdate.Core.Configuration;

var request = new UpdateRequestBuilder()
    .SetUpdateUrl("https://update.example.com/api/upgrade/verification")
    .SetReportUrl("https://update.example.com/api/upgrade/report")
    .SetUpgradeAppName("UpgradeSample.exe")
    .SetMainAppName("ClientSample.exe")
    .SetClientVersion("1.0.0")
    .SetAppSecretKey("your-app-secret")
    .SetProductId("your-product-id")
    .SetInstallPath(AppDomain.CurrentDomain.BaseDirectory)
    .SetFiles(new List<string> { "appsettings.json" })
    .SetFormats(new List<string> { ".log", ".tmp" })
    .SetDirectories(new List<string> { "logs" })
    .Build();
```

`UpdateRequestBuilder.Create()` 会尝试从应用运行目录的 `update_config.json` 读取配置。如果文件不存在，会抛出 `FileNotFoundException`。

```csharp
var request = UpdateRequestBuilder.Create().Build();
```

## 应用身份清单：generalupdate.manifest.json

`generalupdate.manifest.json` 是由 `GeneralUpdate.Tools` 生成、由 Core 消费的应用身份清单。它把“主程序叫什么、当前版本是多少、升级程序叫什么、产品标识是什么、升级程序放在哪个目录”等稳定元数据从代码配置中移出来，代码里只保留服务端地址、密钥、令牌等运行时或敏感参数。

推荐把它放在应用安装目录，也就是 `UpdateRequest.InstallPath` 指向的目录。默认情况下 `InstallPath` 是 `AppDomain.CurrentDomain.BaseDirectory`，因此普通桌面应用通常把清单放在主程序输出目录根部。

```text
MyProduct/
├─ ClientSample.exe
├─ generalupdate.manifest.json
└─ update/
   └─ UpgradeSample.exe
```

### 清单结构

Tools 生成的 JSON 使用小驼峰字段名，Core 中对应类型是 `ManifestInfo`。

```json
{
  "mainAppName": "ClientSample.exe",
  "clientVersion": "1.0.0",
  "appType": "Client",
  "updateAppName": "UpgradeSample.exe",
  "upgradeClientVersion": "1.0.0",
  "productId": "sample-product",
  "updatePath": "update/"
}
```

| JSON 字段 | Core 字段 | 说明 |
| --- | --- | --- |
| `mainAppName` | `MainAppName` | 主程序可执行文件名。升级完成后用于重新启动主程序，也用于识别当前产品。 |
| `clientVersion` | `ClientVersion` | 当前主程序版本。Core 用它向服务端询问是否有主程序更新。 |
| `appType` | `AppType` | 当前进程角色字符串，例如 `Client`、`Upgrade`、`OssClient`、`OssUpgrade`。 |
| `updateAppName` | `UpdateAppName` | 升级程序文件名，默认 `Update.exe`。 |
| `upgradeClientVersion` | `UpgradeClientVersion` | 升级程序自身版本。Core 用它判断是否需要先更新升级程序。 |
| `productId` | `ProductId` | 产品标识。一个更新服务管理多个产品时用于区分产品。 |
| `updatePath` | `UpdatePath` | 升级程序所在目录；可以是相对 `InstallPath` 的目录，例如 `update/`。 |

清单刻意不包含 `UpdateUrl`、`ReportUrl`、`AppSecretKey`、`Scheme`、`Token` 等服务端和认证信息。这样可以让 Tools 负责构建可发布的身份元数据，而密钥仍由应用代码、配置中心或部署环境提供。

### Tools 如何生成清单

`GeneralUpdate.Tools` 的配置生成流程会解析主程序和升级程序的 `.csproj`，校验版本号，然后写出 `generalupdate.manifest.json`。

| Tools 阶段 | 作用 |
| --- | --- |
| `CsprojParseStep` | 解析主程序 `.csproj`；如果提供升级程序 `.csproj`，也会一起解析。 |
| `SemverValidateStep` | 校验 `ClientVersion` 和 `UpgradeClientVersion` 必须符合 semver，例如 `1.0.0`。 |
| `ManifestBuildStep` | 如果 UI 中没有手动填写 `MainAppName` / `UpdateAppName`，使用 `.csproj` 的 `AssemblyName` 补齐。 |
| `FileEmitStep` | 把清单写到输出目录，文件名固定为 `generalupdate.manifest.json`。 |

配置界面的发布样例流程还会调用 `SamplePublisherService.PublishAsync(...)`，把主程序输出、升级程序输出和清单一起组织到可运行样例目录中。因此新手不需要从零手写完整 `UpdateRequest`，可以先用 Tools 生成清单，再在应用代码中补充服务端入口和密钥。

### 配合引导类使用

使用清单后，业务代码不需要关心 `MainAppName`、`ClientVersion`、`UpdateAppName`、`UpgradeClientVersion`、`ProductId`、`UpdatePath` 这些身份字段，也不需要手动读取 `generalupdate.manifest.json`。引导类启动更新流程时会在内部读取 `InstallPath/generalupdate.manifest.json`，并把清单中的应用身份信息带入后续的版本检查、下载、启动升级程序和版本回写流程。

默认安装目录就是当前应用目录时，只需要把服务端入口和密钥传给 `SetSource`：

```csharp
await new GeneralUpdateBootstrap()
    .SetSource(
        updateUrl: "https://update.example.com/api/upgrade/verification",
        appSecretKey: "your-app-secret",
        reportUrl: "https://update.example.com/api/upgrade/report")
    .SetOption(Option.AppType, AppType.Client)
    .LaunchAsync();
```

如果应用的实际安装目录不是当前进程基目录，只需要在 `UpdateRequest` 中补充 `InstallPath`，仍然不需要把清单中的身份字段重复写进代码：

```csharp
using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;

var request = new UpdateRequest
{
    UpdateUrl = "https://update.example.com/api/upgrade/verification",
    ReportUrl = "https://update.example.com/api/upgrade/report",
    AppSecretKey = "your-app-secret",
    InstallPath = @"C:\Program Files\MyProduct"
};

await new GeneralUpdateBootstrap()
    .SetConfig(request)
    .SetOption(Option.AppType, AppType.Client)
    .LaunchAsync();
```

推荐的职责拆分是：

| 由 manifest 提供 | 由代码或环境提供 |
| --- | --- |
| `MainAppName`、`ClientVersion`、`UpdateAppName`、`UpgradeClientVersion`、`ProductId`、`UpdatePath` | `UpdateUrl`、`ReportUrl`、`AppSecretKey`、`Scheme`、`Token`、事件、扩展点、运行选项 |

### 版本回写

在 `generalupdate.manifest.json` 体系下，清单同时也是本地版本状态文件。开发者只需要在首次发布时通过 Tools 生成清单，不需要在每次更新完成后再写业务代码去修改本地版本号。更新成功后，Core 会把已应用的新版本自动写回安装目录下的同一个 `generalupdate.manifest.json`：

| 场景 | 回写字段 |
| --- | --- |
| 主程序更新完成 | `ClientVersion` |
| 升级程序自身更新完成 | `UpgradeClientVersion` |

这样下一次轮询或启动时，引导类会基于清单中的最新本地版本继续向服务端验证，而不是继续使用打包时的旧版本。回写的意义是把“本地版本号维护”收进 Core 的更新流程里，避免开发者在应用代码中额外维护 `ClientVersion` 或 `UpgradeClientVersion`。这个行为依赖安装目录可写；如果应用安装在受限目录，需要确保升级程序拥有写入清单的权限。

## 运行选项：Option

Core 使用强类型 `Option<T>` 注册运行时选项，并通过 `SetOption` 设置值。

```csharp
await new GeneralUpdateBootstrap()
    .SetConfig(request)
    .SetOption(Option.AppType, AppType.Client)
    .SetOption(Option.MaxConcurrency, 4)
    .SetOption(Option.VerifyChecksum, true)
    .LaunchAsync();
```

| 选项 | 类型 | 默认值 | 说明 |
| --- | --- | --- | --- |
| `Option.AppType` | `AppType` | `Client` | 当前进程角色。 |
| `Option.DiffMode` | `DiffMode` | `Serial` | 差分执行模式。 |
| `Option.Encoding` | `Encoding` | `UTF8` | 压缩包处理编码。 |
| `Option.Format` | `Format` | `Zip` | 更新包格式。 |
| `Option.DownloadTimeout` | `int?` | `30` | 下载超时时间，单位秒。 |
| `Option.PatchEnabled` | `bool?` | `true` | 是否启用差分补丁处理。 |
| `Option.BackupEnabled` | `bool?` | `true` | 更新前是否备份被替换文件。 |
| `Option.Silent` | `bool` | `false` | 是否启用静默轮询更新。 |
| `Option.SilentPollIntervalMinutes` | `int` | `60` | 静默模式轮询间隔。 |
| `Option.LaunchClientAfterUpdate` | `bool` | `true` | 升级后是否启动主程序。 |
| `Option.MaxConcurrency` | `int` | `3` | 下载最大并发数。 |
| `Option.EnableResume` | `bool` | `true` | 是否启用断点续传。 |
| `Option.RetryCount` | `int` | `3` | 下载重试次数。 |
| `Option.VerifyChecksum` | `bool` | `true` | 是否校验下载文件 Hash。 |
| `Option.RetryInterval` | `TimeSpan` | `1s` | 下载重试间隔。 |

如果传入 `null` 给可空选项，`SetOption` 会移除当前设置，后续读取回到默认值。

## 事件 API

事件适合观察更新过程，不应该承载复杂业务流程。复杂流程建议封装成 `IUpdateHooks` 或 cookbook 中的完整方案。

### 单个事件回调

| 方法 | 参数类型 | 触发时机 |
| --- | --- | --- |
| `AddListenerUpdateInfo` | `UpdateInfoEventArgs` | 服务端版本信息返回后。 |
| `AddListenerUpdatePrecheck` | `Func<UpdateInfoEventArgs, bool>` | 下载开始前，返回 `true` 继续，返回 `false` 中止。 |
| `AddListenerMultiDownloadStatistics` | `MultiDownloadStatisticsEventArgs` | 下载过程中持续触发。 |
| `AddListenerMultiDownloadCompleted` | `MultiDownloadCompletedEventArgs` | 单个版本下载结束。 |
| `AddListenerMultiAllDownloadCompleted` | `MultiAllDownloadCompletedEventArgs` | 所有下载任务结束。 |
| `AddListenerMultiDownloadError` | `MultiDownloadErrorEventArgs` | 下载失败。 |
| `AddListenerProgress` | `ProgressEventArgs` | 下载进度或差分补丁进度变化。 |
| `AddListenerException` | `ExceptionEventArgs` | Core 捕获异常。 |

```csharp
await new GeneralUpdateBootstrap()
    .SetConfig(request)
    .AddListenerUpdateInfo((_, e) =>
    {
        Console.WriteLine($"Versions from server: {e.Info?.Body?.Count ?? 0}");
    })
    .AddListenerUpdatePrecheck(e =>
    {
        var hasUpdate = (e.Info?.Body?.Count ?? 0) > 0;
        var enoughDisk = DriveInfo.GetDrives()
            .Where(d => d.IsReady)
            .Any(d => d.AvailableFreeSpace > 1024L * 1024 * 1024);

        return hasUpdate && enoughDisk;
    })
    .AddListenerMultiDownloadStatistics((_, e) =>
    {
        Console.WriteLine($"{e.ProgressPercentage}% {e.Speed} {e.BytesReceived}/{e.TotalBytesToReceive}");
    })
    .AddListenerMultiDownloadCompleted((_, e) =>
    {
        Console.WriteLine(e.IsCompleted ? "Download completed." : "Download failed.");
    })
    .AddListenerProgress((_, e) =>
    {
        if (e.Progress != null)
            Console.WriteLine($"Download: {e.Progress.Percentage}%");

        if (e.DiffProgress != null)
            Console.WriteLine($"Patch: {e.DiffProgress.Completed}/{e.DiffProgress.Total}");
    })
    .AddListenerException((_, e) =>
    {
        Console.WriteLine(e.Message);
        Console.WriteLine(e.Exception);
    })
    .LaunchAsync();
```

### 批量事件监听器

实现 `IUpdateEventListener` 可以把事件处理集中到一个类。若只关心部分事件，继承 `UpdateEventListenerBase` 更简单。

```csharp
using GeneralUpdate.Core.Download;
using GeneralUpdate.Core.Event;

public sealed class ConsoleUpdateListener : UpdateEventListenerBase
{
    public override void OnUpdateInfo(UpdateInfoEventArgs args)
    {
        Console.WriteLine($"Update count: {args.Info?.Body?.Count ?? 0}");
    }

    public override void OnDownloadStatistics(MultiDownloadStatisticsEventArgs args)
    {
        Console.WriteLine($"{args.ProgressPercentage}% {args.Speed}");
    }

    public override void OnException(ExceptionEventArgs args)
    {
        Console.WriteLine(args.Exception);
    }
}

await new GeneralUpdateBootstrap()
    .SetConfig(request)
    .AddEventListener<ConsoleUpdateListener>()
    .LaunchAsync();
```

## 扩展点总览

扩展点由 `AbstractBootstrap` 提供，所有注册方法都返回当前 bootstrap，可链式调用。

| 注册方法 | 接口 | 影响范围 |
| --- | --- | --- |
| `Hooks<T>()` | `IUpdateHooks` | 更新生命周期前后置逻辑。 |
| `UpdateReporter<T>()` | `IUpdateReporter` | 更新状态上报。 |
| `SslPolicy<T>()` | `ISslValidationPolicy` | HTTPS 证书校验。 |
| `UpdateAuth<T>()` | `IHttpAuthProvider` | HTTP 请求认证。 |
| `DownloadSource<T>()` | `IDownloadSource` | 版本清单和下载资源来源。 |
| `DownloadPolicy<T>()` | `IDownloadPolicy` | 下载重试、超时、熔断等策略。 |
| `DownloadExecutor<T>()` | `IDownloadExecutor` | 单文件下载实现。 |
| `DownloadPipeline<T>()` | `IDownloadPipeline` | 下载后处理，例如校验、解密、扫描。 |
| `DownloadOrchestrator<T>()` | `IDownloadOrchestrator` | 批量下载完整编排。 |
| `Strategy<T>()` | `IStrategy` | 自定义平台级更新策略。 |

> 通过这些方法注册的类型必须有无参构造函数，因为 Core 使用 `new()` 或反射创建实例。需要复杂依赖时，建议在自定义类型内部读取配置，或在应用层封装一个无参适配器。

## 生命周期钩子：IUpdateHooks

`IUpdateHooks` 适合处理“更新前检查、下载完成后处理、更新完成后清理、启动应用前准备、异常处理”等业务逻辑。它也是一个非常灵活的开放点：在 Linux 或 macOS 上，更新后的可执行文件可能需要重新赋予执行权限，或者需要先执行企业内部的授权脚本、签名校验脚本、权限修复脚本，再启动主程序；这些操作都可以放在 `OnBeforeStartAppAsync` 中完成。

```csharp
using GeneralUpdate.Core.Hooks;

public sealed class ProductUpdateHooks : IUpdateHooks
{
    public Task<bool> OnBeforeUpdateAsync(HookContext ctx)
    {
        Console.WriteLine($"Before update: {ctx.CurrentVersion} -> {ctx.TargetVersion}");
        return Task.FromResult(true);
    }

    public Task OnDownloadCompletedAsync(DownloadContext ctx)
    {
        Console.WriteLine($"Downloaded {ctx.AssetName}, success={ctx.Success}, path={ctx.LocalPath}");
        return Task.CompletedTask;
    }

    public Task OnAfterUpdateAsync(HookContext ctx)
    {
        File.WriteAllText(Path.Combine(ctx.InstallPath, "last-update.txt"), DateTimeOffset.Now.ToString("O"));
        return Task.CompletedTask;
    }

    public Task OnUpdateErrorAsync(HookContext ctx, Exception ex)
    {
        File.AppendAllText(Path.Combine(ctx.InstallPath, "update-error.log"), ex + Environment.NewLine);
        return Task.CompletedTask;
    }

    public Task OnBeforeStartAppAsync(HookContext ctx)
    {
        Console.WriteLine($"Starting app from {ctx.InstallPath}");
        return Task.CompletedTask;
    }
}

await new GeneralUpdateBootstrap()
    .SetConfig(request)
    .Hooks<ProductUpdateHooks>()
    .LaunchAsync();
```

Linux/macOS 场景可以直接注册内置的 `UnixPermissionHooks`，让 Core 在启动应用前执行 `chmod +x`：

```csharp
await new GeneralUpdateBootstrap()
    .SetConfig(request)
    .Hooks<UnixPermissionHooks>()
    .LaunchAsync();
```

如果需要执行自己的赋权脚本，可以封装一个无参 hook 适配器，再通过 `Hooks<T>()` 注册：

```csharp
using GeneralUpdate.Core.Hooks;

public sealed class ProductPermissionHooks : IUpdateHooks
{
    private readonly CustomPermissionHooks _inner =
        new("/opt/my-product/scripts/fix-permissions.sh");

    public Task OnBeforeStartAppAsync(HookContext ctx)
        => _inner.OnBeforeStartAppAsync(ctx);

    public Task<bool> OnBeforeUpdateAsync(HookContext ctx) => Task.FromResult(true);
    public Task OnDownloadCompletedAsync(DownloadContext ctx) => Task.CompletedTask;
    public Task OnAfterUpdateAsync(HookContext ctx) => Task.CompletedTask;
    public Task OnUpdateErrorAsync(HookContext ctx, Exception ex) => Task.CompletedTask;
}

await new GeneralUpdateBootstrap()
    .SetConfig(request)
    .Hooks<ProductPermissionHooks>()
    .LaunchAsync();
```

内置实现包括：

| 类型 | 说明 |
| --- | --- |
| `NoOpUpdateHooks` | 默认空实现。 |
| `UnixPermissionHooks` | 在 Unix-like 系统启动前执行 `chmod +x`。 |
| `CustomPermissionHooks` | 执行自定义权限脚本；该类型构造函数需要参数，不适合直接用 `Hooks<T>()` 注册，可自行包装无参适配器。 |

## 状态上报：IUpdateReporter

`IUpdateReporter` 用于把更新状态上报到服务端。

```csharp
using GeneralUpdate.Core.Download.Reporting;

public sealed class ConsoleUpdateReporter : IUpdateReporter
{
    public Task ReportAsync(UpdateReport report, CancellationToken token = default)
    {
        Console.WriteLine($"Report: record={report.RecordId}, status={report.Status}, type={report.Type}");
        return Task.CompletedTask;
    }
}

await new GeneralUpdateBootstrap()
    .SetConfig(request)
    .UpdateReporter<ConsoleUpdateReporter>()
    .LaunchAsync();
```

内置 `HttpUpdateReporter` 会向 `ReportUrl` 发送 JSON：

```json
{
  "recordId": 123,
  "status": 1,
  "type": 1
}
```

状态值：

| 枚举 | 值 | 说明 |
| --- | --- | --- |
| `UpdateStatus.Updating` | `1` | 更新中。 |
| `UpdateStatus.Success` | `2` | 更新成功。 |
| `UpdateStatus.Failure` | `3` | 更新失败。 |

## HTTP 认证：IHttpAuthProvider

`IHttpAuthProvider` 可以为 Core 发出的 HTTP 请求追加认证头。

```csharp
using GeneralUpdate.Core.Security;

public sealed class StaticBearerAuthProvider : IHttpAuthProvider
{
    public Task ApplyAuthAsync(HttpRequestMessage request, CancellationToken token = default)
    {
        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "access-token");

        return Task.CompletedTask;
    }
}

await new GeneralUpdateBootstrap()
    .SetConfig(request)
    .UpdateAuth<StaticBearerAuthProvider>()
    .LaunchAsync();
```

Core 内置的认证类型包括 `NoOpAuthProvider`、`BearerTokenAuthProvider`、`ApiKeyAuthProvider` 和 `HmacAuthProvider`。这些类型中部分构造函数需要参数，因此如果要通过 `UpdateAuth<T>()` 注册，通常需要写一个无参包装类。

## HTTPS 证书策略：ISslValidationPolicy

`ISslValidationPolicy` 用于控制 HTTPS 证书校验。默认 `StrictSslValidationPolicy` 只接受没有 SSL policy errors 的证书。

```csharp
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using GeneralUpdate.Core.Security;

public sealed class DevelopmentSslPolicy : ISslValidationPolicy
{
    public bool ValidateCertificate(
        X509Certificate2? certificate,
        X509Chain? chain,
        SslPolicyErrors sslPolicyErrors)
    {
        return sslPolicyErrors == SslPolicyErrors.None
            || certificate?.Issuer.Contains("CN=Local Dev Root") == true;
    }
}

await new GeneralUpdateBootstrap()
    .SetConfig(request)
    .SslPolicy<DevelopmentSslPolicy>()
    .LaunchAsync();
```

生产环境不建议无条件返回 `true`，否则会绕过 HTTPS 的安全保证。

## 下载来源：IDownloadSource

`IDownloadSource` 负责返回待下载资源列表。适合接入私有服务、文件服务器、配置中心或自定义云存储。

```csharp
using GeneralUpdate.Core.Download.Abstractions;
using GeneralUpdate.Core.Download.Models;

public sealed class StaticDownloadSource : IDownloadSource
{
    public Task<DownloadSourceResult> ListAsync(CancellationToken token = default)
    {
        var assets = new[]
        {
            new DownloadAsset(
                Name: "app-1.0.1.zip",
                Url: "https://cdn.example.com/releases/app-1.0.1.zip",
                Size: 25_000_000,
                SHA256: "expected-sha256",
                Version: "1.0.1")
        };

        return Task.FromResult(new DownloadSourceResult
        {
            Assets = assets,
            HasMainUpdate = true,
            HasUpgradeUpdate = false
        });
    }
}

await new GeneralUpdateBootstrap()
    .SetConfig(request)
    .DownloadSource<StaticDownloadSource>()
    .LaunchAsync();
```

## 下载重试策略：IDownloadPolicy

`IDownloadPolicy` 包装单次下载动作，适合实现重试、超时、熔断或限流。

```csharp
using GeneralUpdate.Core.Download.Abstractions;

public sealed class TwoAttemptDownloadPolicy : IDownloadPolicy
{
    public async Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> action,
        CancellationToken token = default)
    {
        try
        {
            return await action(token);
        }
        catch when (!token.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(2), token);
            return await action(token);
        }
    }
}

await new GeneralUpdateBootstrap()
    .SetConfig(request)
    .DownloadPolicy<TwoAttemptDownloadPolicy>()
    .LaunchAsync();
```

如果同时注册 `DownloadOrchestrator<T>()`，自定义 orchestrator 会接管完整下载流程，`DownloadPolicy<T>()` 是否生效取决于 orchestrator 自己是否使用该策略。

## 单文件下载：IDownloadExecutor

`IDownloadExecutor` 负责把一个 `DownloadAsset` 下载到目标路径。适合支持 FTP、SFTP、私有协议或自定义 HTTP 客户端。

```csharp
using GeneralUpdate.Core.Download.Abstractions;
using GeneralUpdate.Core.Download.Models;

public sealed class MirrorDownloadExecutor : IDownloadExecutor
{
    private readonly HttpClient _client = new();

    public async Task<DownloadResult> ExecuteAsync(
        DownloadAsset asset,
        string destPath,
        IProgress<DownloadProgress>? progress = null,
        CancellationToken token = default)
    {
        var started = DateTimeOffset.Now;
        await using var input = await _client.GetStreamAsync(asset.Url, token);
        await using var output = File.Create(destPath);
        await input.CopyToAsync(output, token);

        var fileInfo = new FileInfo(destPath);
        return new DownloadResult(asset, destPath, fileInfo.Length, DateTimeOffset.Now - started, 0, true, null);
    }
}

await new GeneralUpdateBootstrap()
    .SetConfig(request)
    .DownloadExecutor<MirrorDownloadExecutor>()
    .LaunchAsync();
```

## 下载后处理：IDownloadPipeline

`IDownloadPipeline` 在文件下载完成后运行。适合做 Hash 校验、解密、病毒扫描、格式转换等。

```csharp
using GeneralUpdate.Core.Download.Abstractions;

public sealed class AntivirusPipeline : IDownloadPipeline
{
    public Task<string> ProcessAsync(string downloadedPath, CancellationToken token = default)
    {
        if (!File.Exists(downloadedPath))
            throw new FileNotFoundException("Downloaded file not found.", downloadedPath);

        Console.WriteLine($"Scanning {downloadedPath}");
        return Task.FromResult(downloadedPath);
    }
}

await new GeneralUpdateBootstrap()
    .SetConfig(request)
    .DownloadPipeline<AntivirusPipeline>()
    .LaunchAsync();
```

Core 在创建下载管道时会优先尝试使用 `string` 构造函数传入期望 Hash；如果没有该构造函数，则使用无参构造函数。

## 批量下载编排：IDownloadOrchestrator

`IDownloadOrchestrator` 是下载子系统的最高层扩展点。注册后，它会接管批量下载、并发控制、重试、进度和结果汇总。

```csharp
using GeneralUpdate.Core.Download.Abstractions;
using GeneralUpdate.Core.Download.Executors;
using GeneralUpdate.Core.Download.Models;

public sealed class SerialDownloadOrchestrator : IDownloadOrchestrator
{
    private readonly IDownloadExecutor _executor = new HttpDownloadExecutor(new HttpClient());

    public async Task<DownloadReport> ExecuteAsync(
        DownloadPlan plan,
        string destDir,
        int maxConcurrency = 3,
        IProgress<DownloadProgress>? progress = null,
        CancellationToken token = default)
    {
        var results = new List<DownloadResult>();
        var started = DateTimeOffset.Now;

        foreach (var asset in plan.Assets)
        {
            var destPath = Path.Combine(destDir, asset.Name);
            results.Add(await _executor.ExecuteAsync(asset, destPath, progress, token));
        }

        return new DownloadReport(
            results,
            results.Where(r => r.Success).Sum(r => r.DownloadedBytes),
            DateTimeOffset.Now - started,
            results.Count(r => r.Success),
            results.Count(r => !r.Success));
    }
}

await new GeneralUpdateBootstrap()
    .SetConfig(request)
    .DownloadOrchestrator<SerialDownloadOrchestrator>()
    .LaunchAsync();
```

只有当你需要完整替换下载行为时才建议实现 orchestrator。多数情况下替换 `IDownloadExecutor`、`IDownloadPolicy` 或 `IDownloadPipeline` 就够了。

## 平台策略：IStrategy

`IStrategy` 是最高级别的更新策略接口。Core 内置 `ClientStrategy`、`UpdateStrategy`、`OssStrategy` 以及 Windows/Linux/macOS 平台策略。只有在你需要替换平台级文件操作或启动逻辑时，才应实现它。

```csharp
using GeneralUpdate.Core.Configuration;
using GeneralUpdate.Core.Download.Reporting;
using GeneralUpdate.Core.Hooks;
using GeneralUpdate.Core.Strategy;

public sealed class LoggingStrategy : IStrategy
{
    private UpdateContext? _context;

    public IUpdateHooks Hooks { get; set; } = new NoOpUpdateHooks();
    public IUpdateReporter Reporter { get; set; } = new HttpUpdateReporter();

    public void Create(UpdateContext parameter)
    {
        _context = parameter;
    }

    public async Task ExecuteAsync()
    {
        if (_context == null)
            throw new InvalidOperationException("Strategy was not initialized.");

        Console.WriteLine($"Custom strategy executing in {_context.InstallPath}");
        await Hooks.OnBeforeUpdateAsync(new HookContext(
            _context.UpdateAppName,
            _context.InstallPath,
            _context.ClientVersion,
            _context.LastVersion,
            _context.AppType ?? AppType.Client));
    }

    public Task StartAppAsync()
    {
        Console.WriteLine("Custom start app logic.");
        return Task.CompletedTask;
    }
}

await new GeneralUpdateBootstrap()
    .SetConfig(request)
    .Strategy<LoggingStrategy>()
    .LaunchAsync();
```

## 静默更新选项

静默更新通过选项启用，不需要额外接口。启用后，`Client` 角色会启动后台轮询并立即返回。

```csharp
await new GeneralUpdateBootstrap()
    .SetConfig(request)
    .SetOption(Option.AppType, AppType.Client)
    .SetOption(Option.Silent, true)
    .SetOption(Option.SilentPollIntervalMinutes, 30)
    .SetOption(Option.LaunchClientAfterUpdate, true)
    .LaunchAsync();
```

适合把“是否启用、何时提示用户、如何处理退出时升级”等完整策略写到 cookbook，而组件文档只需要说明相关 API。

## 与 GeneralUpdate.Tools 的关系

Core 消费更新清单和更新包；`GeneralUpdate.Tools` 负责辅助生成和验证这些产物。

| Tools 能力 | Core 中对应消费点 |
| --- | --- |
| Patch Package | `Option.PatchEnabled`、`UseDiffPipeline`、差分补丁处理。 |
| Manifest Generator | `ManifestInfo`、`AppMetadataDiscoverer`、版本回写。 |
| Extension Package | 作为更新包内容或扩展包分发，由下载和部署流程消费。 |
| OSS Config | `OssClient` / `OssUpgrade` 角色读取 OSS 配置并下载。 |
| Hash / Simulation / Report | 对应 `Option.VerifyChecksum`、下载后校验和状态上报。 |

## 相关示例

- [Upgrade sample](https://github.com/GeneralLibrary/GeneralUpdate-Samples/blob/main/src/Upgrade/Program.cs)
- [OSS upgrade sample](https://github.com/GeneralLibrary/GeneralUpdate-Samples/tree/main/src/OSS/OSSUpgradeSample)
- [GeneralUpdate repository](https://github.com/GeneralLibrary/GeneralUpdate)
- [GeneralUpdate.Tools repository](https://github.com/GeneralLibrary/GeneralUpdate.Tools)
