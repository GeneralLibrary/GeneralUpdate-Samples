---
sidebar_position: 5
---

# GeneralUpdate.Core

**命名空间:** `GeneralUpdate.Core` | **主要入口:** `GeneralUpdateBootstrap` | **NuGet 包:** `GeneralUpdate.Core`

`GeneralUpdate.Core` 是 GeneralUpdate 的更新执行核心，重点提供可编程的启动器、配置模型、事件模型、下载子系统扩展点、生命周期钩子、状态上报、差分管道和平台策略扩展。本页聚焦组件 API、属性和扩展方式；完整端到端上手流程会放到 cookbook 中。

## 文档大纲与知识点导航 {#knowledge-map}

如果你是第一次阅读 Core 文档，可以先看这个导航，再跳到对应知识点。本文按照“入口与配置 -> 执行策略 -> 差分/下载/并发 -> 扩展点 -> 工具链关系”的顺序组织。

| 你想了解什么 | 推荐阅读 |
| --- | --- |
| Core 到底负责什么、不负责什么 | [组件能力边界](#组件能力边界) |
| 如何启动一次标准更新 | [入口类：GeneralUpdateBootstrap](#入口类generalupdatebootstrap)、[标准更新策略](#standard-update-strategy) |
| 如何用极简配置接入更新 | [generalupdate.manifest.json](#应用身份清单generalupdatemanifestjson)、[极简配置理念](#极简配置理念)、[配合引导类使用](#配合引导类使用) |
| `Client`、`Upgrade`、`OssClient`、`OssUpgrade` 有什么区别 | [执行策略总览](#execution-strategies) |
| 静默更新到底什么时候下载、什么时候替换 | [静默更新策略](#silent-update-strategy) |
| 差分算法有哪些，如何选择 | [差分算法与补丁管道](#differential-pipeline) |
| 下载和差分如何并发、多线程 | [下载并发与差分并行](#download-diff-concurrency) |
| 如何接收更新过程事件通知 | [事件 API](#事件-api) |
| 如何关闭日志降低性能损耗 | [日志与性能](#logging-performance) |
| 如何扩展下载、校验、认证、Hook 或平台策略 | [扩展点总览](#扩展点总览) |
| Tools 生成的产物在 Core 中怎么消费 | [与 GeneralUpdate.Tools 的关系](#与-generalupdatetools-的关系) |

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

## 执行策略总览 {#execution-strategies}

Core 内置三类上层执行策略：标准更新策略、OSS 更新策略和静默更新策略。它们不是互相独立的 API，而是由 `LaunchAsync()` 根据 `Option.AppType`、`Option.Silent` 和当前配置自动选择。

| 策略 | 触发条件 | 主要角色 | 适用场景 |
| --- | --- | --- | --- |
| [标准更新策略](#standard-update-strategy) | `Option.AppType = AppType.Client` 或 `AppType.Upgrade`，且 `Option.Silent = false` | 主程序检查/下载，升级程序替换文件 | 有服务端版本检查 API、需要标准更新和回写版本的桌面应用 |
| [OSS 更新策略](#oss-update-strategy) | `Option.AppType = AppType.OssClient` 或 `AppType.OssUpgrade` | 主程序下载 OSS 版本配置，升级程序下载并解压资源 | 版本配置和包都托管在对象存储/CDN 的应用 |
| [静默更新策略](#silent-update-strategy) | `Option.AppType = AppType.Client` 且 `Option.Silent = true` | 主程序后台轮询，退出时启动升级程序 | 希望用户使用期间无打扰下载，退出或合适时机再替换 |

### 标准更新策略 {#standard-update-strategy}

标准更新由 `ClientStrategy` 和 `UpdateStrategy` 配合完成。`ClientStrategy` 运行在主程序中，负责发现本地清单、请求服务端版本、生成下载计划、下载更新包、准备 IPC 上下文并启动升级程序；`UpdateStrategy` 运行在独立升级程序中，负责读取 IPC 上下文、解压、应用差分补丁、替换文件、回写版本并按需启动主程序。

标准流程的核心顺序如下：

1. 主程序调用 `SetConfig(...)` 或 `SetSource(...)` 后执行 `LaunchAsync()`。
2. Core 自动读取安装目录下的 `generalupdate.manifest.json`，补齐未显式提供的 `MainAppName`、`UpdateAppName`、`ClientVersion`、`UpgradeClientVersion`、`ProductId`、`InstallPath` 等身份字段。
3. `ClientStrategy` 使用 `DownloadSource` 获取服务端资源列表，并分别比较主程序版本和升级程序版本。
4. `DownloadPlanBuilder` 过滤冻结包、按版本排序、检查 `MinClientVersion`，形成下载计划。
5. 下载阶段通过 `IDownloadOrchestrator` 批量下载资源，默认支持并发、断点续传、重试和 SHA256 校验。
6. Core 按资源 `AppType` 拆分为升级程序包和主程序包，再根据场景分派。
7. 升级程序包可以在主程序侧先应用并回写 `UpgradeClientVersion`；主程序包会通过加密 IPC 交给升级程序替换。
8. 升级程序完成替换后，Core 基于清单体系自动回写 `ClientVersion`，开发者不需要在业务代码中维护本地版本号。

| 场景 | 判断结果 | Core 行为 |
| --- | --- | --- |
| `None` | 主程序和升级程序都无需更新 | 分发“无更新”事件并结束。 |
| `UpgradeOnly` | 只有升级程序需要更新 | 主程序下载升级程序包，直接应用到升级程序目录，回写 `UpgradeClientVersion`，主程序继续运行。 |
| `MainOnly` | 只有主程序需要更新 | 主程序下载主程序包，写入 IPC 上下文，启动升级程序替换主程序文件。 |
| `Both` | 主程序和升级程序都需要更新 | 先更新升级程序并回写 `UpgradeClientVersion`，再把主程序包交给新的升级程序处理。 |

```csharp
await new GeneralUpdateBootstrap()
    .SetSource(
        updateUrl: "https://update.example.com/api/upgrade/verification",
        appSecretKey: "your-app-secret",
        reportUrl: "https://update.example.com/api/upgrade/report")
    .SetOption(Option.AppType, AppType.Client)
    .SetOption(Option.PatchEnabled, true)
    .SetOption(Option.DiffMode, DiffMode.Parallel)
    .SetOption(Option.MaxConcurrency, 4)
    .LaunchAsync();
```

### OSS 更新策略 {#oss-update-strategy}

OSS 更新由同一个 `OssStrategy` 根据角色分成 `OssClient` 和 `OssUpgrade` 两段。它适合把版本配置 JSON 和更新包放在 OSS、S3、MinIO、CDN 或静态文件服务器上，不依赖标准服务端版本检查 API。

| 角色 | 本地行为 | 关键配置 |
| --- | --- | --- |
| `AppType.OssClient` | 从 `UpdateUrl` 下载 OSS 版本配置到安装目录，比较远端最新版本和本地 `ClientVersion`，需要更新时启动升级程序并退出。 | `UpdateUrl` 指向版本配置文件地址；`MainAppName` / `UpdateAppName` 可由 manifest 提供。 |
| `AppType.OssUpgrade` | 读取本地版本配置或自定义 `DownloadSource`，筛选高于本地版本的资源，下载到安装目录，解压 ZIP，删除压缩包，启动主程序并退出。 | 安装目录可写；资源列表中的版本号必须可比较。 |

OSS 版本配置文件会保存为 `{MainAppName}_versions.json` 或 `{UpdateAppName}_versions.json`。如果注册了 `DownloadSource<T>()`，OSS 升级侧可以跳过默认文件读取逻辑，改为由你的下载源返回资源列表；如果注册了 `DownloadOrchestrator<T>()`，下载过程也可以完全替换。

```csharp
await new GeneralUpdateBootstrap()
    .SetSource(
        updateUrl: "https://cdn.example.com/myapp_versions.json",
        appSecretKey: "oss-mode-secret")
    .SetOption(Option.AppType, AppType.OssClient)
    .LaunchAsync();
```

```csharp
await new GeneralUpdateBootstrap()
    .SetOption(Option.AppType, AppType.OssUpgrade)
    .LaunchAsync();
```

### 静默更新策略 {#silent-update-strategy}

静默更新只在 `AppType.Client` 下生效。启用后，`LaunchAsync()` 会进入静默启动分支，创建和标准更新相同的 `ClientStrategy`，但把 `LaunchAfterPrepare` 设为 `false`，再交给 `SilentPollOrchestrator` 做后台轮询。

静默模式不会重新实现更新逻辑；它只是把“检查和下载”放到后台，把“启动升级程序替换文件”延后到进程退出时。这样用户可以继续使用当前进程，更新包先准备好，真正替换发生在应用退出之后。

| 阶段 | 标准更新 | 静默更新 |
| --- | --- | --- |
| 版本检查 | 用户触发后立即执行一次 | 后台按 `Option.SilentPollIntervalMinutes` 周期执行 |
| 下载 | 发现更新后立即下载 | 发现更新后后台下载 |
| 启动升级程序 | 主程序准备完成后立即启动 | 主程序退出时由 `ProcessExit` 处理启动 |
| 用户体验 | 适合显式“检查更新/立即更新” | 适合无打扰准备更新 |

```csharp
await new GeneralUpdateBootstrap()
    .SetSource(
        updateUrl: "https://update.example.com/api/upgrade/verification",
        appSecretKey: "your-app-secret")
    .SetOption(Option.AppType, AppType.Client)
    .SetOption(Option.Silent, true)
    .SetOption(Option.SilentPollIntervalMinutes, 30)
    .SetOption(Option.LaunchClientAfterUpdate, true)
    .LaunchAsync();
```

静默更新仍然会使用你注册的 `IUpdateHooks`、`IUpdateReporter`、下载扩展、证书策略、认证策略和差分管道。需要注意的是，静默模式适合“下载准备无感知”，不等于“文件替换无感知”；主程序文件仍应由独立升级程序在主程序退出后替换。

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

`UseDiffPipeline` 用于替换或调整差分补丁管道。未调用时，引导类会创建默认管道：`BsdiffDiffer`、`DefaultCleanMatcher`、`DefaultDirtyMatcher`、并行度 `2`，并接入 Core 的差分进度事件。关于算法差异、补丁阶段和并发设置，请看 [差分算法与补丁管道](#differential-pipeline)。

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

## 差分算法与补丁管道 {#differential-pipeline}

Core 的差分能力分两层：`IBinaryDiffer` 负责“单个文件如何生成/应用补丁”，`DiffPipeline` 负责“目录中哪些文件需要补丁、哪些文件是新增/删除、如何并行处理多个文件”。普通使用者只需要打开 `Option.PatchEnabled`；需要调优性能或兼容性时，再通过 `UseDiffPipeline(...)` 调整。

### 差分算法类型

| 算法/实现 | 默认位置 | 特点 | 适合场景 |
| --- | --- | --- | --- |
| `BsdiffDiffer` | `GeneralUpdateBootstrap` 默认使用 | 经典 BSDIFF 4.0 算法，默认使用 BZip2 压缩，补丁格式兼容性强。 | 追求稳定兼容、已有包体系基于 BSDIFF 的项目。 |
| `StreamingHdiffDiffer` | `DiffPipeline` 直接构造时的默认 differ；也可通过 `UseDiffPipeline` 显式选择 | 使用块级 Hash 索引做候选匹配，典型复杂度更低，默认使用 Deflate，并生成可由 Dirty 阶段读取的 BSDIFF 兼容补丁格式。 | 大文件较多、希望降低生成补丁时内存和 CPU 压力的项目。 |
| 自定义 `IBinaryDiffer` | 通过 `UseDiffPipeline(builder => builder.UseDiffer(...))` 接入 | 完全替换单文件差分算法。 | 企业内部已有补丁格式、加密补丁或专用二进制差分算法。 |

`BsdiffDiffer` 还支持替换压缩提供器：`BZip2CompressionProvider` 是兼容默认值，`DeflateCompressionProvider` 更偏向速度，`.NET 6+` 可使用 `BrotliCompressionProvider` 在压缩率和解压速度之间取得更好平衡。选择压缩提供器时要确保生成补丁和应用补丁的运行时都能识别对应格式版本。

```csharp
using GeneralUpdate.Core.Pipeline;
using GeneralUpdate.Differential.Abstractions;
using GeneralUpdate.Differential.Differ;

await new GeneralUpdateBootstrap()
    .SetConfig(request)
    .UseDiffPipeline(builder =>
    {
        builder
            .UseDiffer(new BsdiffDiffer(new DeflateCompressionProvider()))
            .WithParallelism(4);
    })
    .SetOption(Option.PatchEnabled, true)
    .LaunchAsync();
```

### Clean 和 Dirty 两个阶段

| 阶段 | 方法 | 运行位置 | 作用 |
| --- | --- | --- | --- |
| Clean | `DiffPipeline.CleanAsync(oldDir, newDir, patchDir)` | 发布侧/工具侧 | 对比旧版本和新版本目录，生成 `.patch` 文件，复制新增文件，并写入 `generalupdate.delete.json` 删除清单。 |
| Dirty | `DiffPipeline.DirtyAsync(appDir, patchDir)` | 客户端升级侧 | 读取补丁目录，把 `.patch` 应用到旧文件，复制新增文件，按删除清单移除旧文件。 |

Core 主要消费 Dirty 阶段；差分包生成建议交给 `GeneralUpdate.Tools` 或发布流水线完成。标准更新中，下载完成后升级程序会先解压包，再在 `PatchEnabled = true` 时通过 `PatchMiddleware` 调用 `DiffPipeline.DirtyAsync(...)` 应用补丁。

### 下载并发与差分并行 {#download-diff-concurrency}

Core 支持两个层面的多线程能力：下载阶段可以并发下载多个更新资源，差分阶段可以并行处理多个文件补丁。内置标准流程是“先完成当前下载计划，再进入解压/差分/替换阶段”；如果你需要把下载和应用做成更细粒度的流水线，可以通过自定义 `IDownloadOrchestrator` 或自定义 `IStrategy` 接管。

| 层级 | 控制 API | 默认行为 | 说明 |
| --- | --- | --- | --- |
| 批量下载并发 | `Option.DiffMode` + `Option.MaxConcurrency` | `DiffMode.Serial` 会强制下载并发为 `1`；`DiffMode.Parallel` 使用 `MaxConcurrency`，并被限制在 `1` 到 `Environment.ProcessorCount * 2` 之间。 | 由 `DefaultDownloadOrchestrator` 使用 `SemaphoreSlim` 控制，支持重试、断点续传和校验。 |
| 差分文件并行 | `UseDiffPipeline(...WithParallelism(n))` | 引导类默认 `2`。 | `DiffPipeline` 对多个文件创建任务，并用 `SemaphoreSlim` 限制同时生成/应用补丁的文件数。 |
| 下载后处理 | `DownloadPipeline<T>()` | 默认 SHA256 校验。 | 每个资源下载成功后执行，可替换为解密、扫描、二次校验等。 |

```csharp
await new GeneralUpdateBootstrap()
    .SetConfig(request)
    .SetOption(Option.DiffMode, DiffMode.Parallel)
    .SetOption(Option.MaxConcurrency, 6)
    .UseDiffPipeline(builder =>
    {
        builder
            .UseDiffer(new StreamingHdiffDiffer())
            .WithParallelism(4);
    })
    .LaunchAsync();
```

并发值不是越大越好。网络慢但磁盘快时可以提高 `Option.MaxConcurrency`；补丁文件很多且磁盘是 SSD 时可以提高 `WithParallelism`；机械硬盘、低配终端或后台静默更新建议降低并发，避免影响主程序响应。

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

`generalupdate.manifest.json` 是由 `GeneralUpdate.Tools` 生成、由 Core 消费的应用身份清单。它的核心价值是**帮开发者节约接入和维护时间**：Tools 把“主程序叫什么、当前版本是多少、升级程序叫什么、产品标识是什么、升级程序放在哪个目录”等稳定元数据生成到清单里，Core 在运行时自动消费这些信息，业务代码只需要补充服务端地址、密钥、令牌等运行时或敏感参数。

换句话说，使用 manifest 后，接入 GeneralUpdate 不再需要手写一大段完整 `UpdateRequest`。发布时让 Tools 生成 `generalupdate.manifest.json`，运行时再配少量敏感信息，就可以直接启动更新流程。这是 Core 推荐的极简配置方式。

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

### 极简配置理念 {#极简配置理念}

manifest 体系把更新配置拆成两部分：

| 配置类型 | 由谁提供 | 为什么这样拆 |
| --- | --- | --- |
| 稳定身份信息 | `GeneralUpdate.Tools` 生成到 `generalupdate.manifest.json` | 这些字段来自项目、版本和发布目录，重复手写容易出错，也会增加每个应用接入更新的时间。 |
| 运行时/敏感信息 | 应用代码、配置中心、环境变量或部署系统提供 | 服务端地址、密钥、Token 可能因环境变化，也不应该由 Tools 固化到可发布清单里。 |

因此最常见的接入路径是：

1. 用 `GeneralUpdate.Tools` 生成并随应用发布 `generalupdate.manifest.json`。
2. 在应用启动更新时只配置 `UpdateUrl`、`AppSecretKey`、`ReportUrl`，以及必要的认证信息。
3. 让 `GeneralUpdateBootstrap` 在内部读取 manifest，自动补齐应用身份、版本、升级程序位置，并在更新成功后回写本地版本。

这种方式把开发者需要关心的配置压缩到“敏感信息 + 少量运行选项”，既减少样板代码，也避免多个应用重复维护主程序名、升级程序名、本地版本号和产品标识。

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
| `Option.DiffMode` | `DiffMode` | `Serial` | 执行模式。`Serial` 会让默认下载编排器串行下载；`Parallel` 允许按 `Option.MaxConcurrency` 并发下载。 |
| `Option.Encoding` | `Encoding` | `UTF8` | 压缩包处理编码。 |
| `Option.Format` | `Format` | `Zip` | 更新包格式。 |
| `Option.DownloadTimeout` | `int?` | `30` | 下载超时时间，单位秒。 |
| `Option.PatchEnabled` | `bool?` | `true` | 是否启用差分补丁处理。 |
| `Option.BackupEnabled` | `bool?` | `true` | 更新前是否备份被替换文件。 |
| `Option.Silent` | `bool` | `false` | 是否启用静默轮询更新。 |
| `Option.SilentPollIntervalMinutes` | `int` | `60` | 静默模式轮询间隔。 |
| `Option.LaunchClientAfterUpdate` | `bool` | `true` | 升级后是否启动主程序。 |
| `Option.MaxConcurrency` | `int` | `3` | 默认下载编排器最大并发数，实际值会被限制到合理范围。 |
| `Option.EnableResume` | `bool` | `true` | 是否启用断点续传。 |
| `Option.RetryCount` | `int` | `3` | 下载重试次数。 |
| `Option.VerifyChecksum` | `bool` | `true` | 是否校验下载文件 Hash。 |
| `Option.RetryInterval` | `TimeSpan` | `1s` | 下载重试间隔。 |

如果传入 `null` 给可空选项，`SetOption` 会移除当前设置，后续读取回到默认值。

## 事件 API {#事件-api}

事件适合观察更新过程，不应该承载复杂业务流程。复杂流程建议封装成 `IUpdateHooks` 或 cookbook 中的完整方案。

### 单个事件回调

单个事件回调适合在启动器链式配置中直接订阅某一个通知。Core 内部通过全局 `EventManager` 按 `EventArgs` 类型分发事件；同一类型可以注册多个回调，某个回调抛出的异常会被记录到 `GeneralTracer`，不会阻断其他回调。

这些回调通常可能在更新流程线程、下载任务线程或差分任务线程中触发，不会自动切回 UI 线程。WPF、Avalonia、WinUI、MAUI 等客户端需要在回调里把 UI 更新切回 Dispatcher / SynchronizationContext；耗时业务也建议投递到队列或后台任务，避免阻塞下载和差分并行度。

| 方法 | 参数类型 | 当前代码中的触发时机 | 关键字段 | 推荐用途 |
| --- | --- | --- | --- | --- |
| `AddListenerUpdateInfo` | `UpdateInfoEventArgs` | 标准 `Client` 策略完成版本对比后触发。没有可更新内容时也会触发一次，`Info.Code` 为 `404`、`Info.Body` 为空列表；有更新时 `Info.Body` 是需要下载的 `VersionEntry` 列表。 | `Info.Code`、`Info.Message`、`Info.Body`；`VersionEntry` 包含 `RecordId`、`Name`、`Version`、`Url`、`Hash`、`AppType`、`IsForcibly`、`UpgradeMode`、`FromVersion`、`ToVersion` 等。 | 展示更新说明、版本数量、强制更新提示，或者记录服务端返回的版本元数据。不要在这里做文件替换。 |
| `AddListenerUpdatePrecheck` | `Func<UpdateInfoEventArgs, bool>` | `UpdateInfo` 事件之后、Hook 和下载之前执行。按当前 `ClientStrategy.CanSkip` 实现：非强制更新下返回 `true` 表示跳过本次更新，返回 `false` 表示继续；强制更新不会进入跳过判断。 | 入参同 `UpdateInfoEventArgs`，可以读取本次更新涉及的所有 `VersionEntry`，包括版本号、更新说明、Hash、包地址、升级模式、跨版本范围等。 | 做下载前的轻量决策，也可以整理版本信息弹窗给用户，让用户阅读更新内容后决定是否继续。需要异步、可取消或有副作用的流程请使用 `IUpdateHooks.OnBeforeUpdateAsync`。 |
| `AddListenerProgress` | `ProgressEventArgs` | 默认下载通道报告 `DownloadProgress` 时触发；差分 Clean / Dirty 管道报告 `DiffProgress` 时也会触发。同一个事件参数中 `Progress` 和 `DiffProgress` 只会有一个非空。 | 下载：`Progress.AssetName`、`BytesDownloaded`、`TotalBytes`、`Percentage`、`Status`。差分：`DiffProgress.Completed`、`Total`、`CurrentFile`、`Percentage`、`IsComplete`、`Error`。 | 更新进度条、状态文本、下载速度/大小展示、差分补丁进度展示。默认下载进度应优先使用这个事件。 |
| `AddListenerMultiDownloadCompleted` | `MultiDownloadCompletedEventArgs` | `DownloadProgressReporter` 收到 `DownloadStatus.Completed` 时触发。当前默认桥接中 `Version` 实际携带 `AssetName`，自定义下载器也可以放入自己的对象。 | `Version`、`IsCompleted`。 | 标记某个资源包下载完成、追加下载日志。不要把它当作“全部资源下载完成”。 |
| `AddListenerMultiAllDownloadCompleted` | `MultiAllDownloadCompletedEventArgs` | `DefaultDownloadOrchestrator` 等待所有下载任务结束后触发一次。并发下载时它在所有任务都完成、失败结果收集完之后触发。 | `IsAllDownloadCompleted`；`FailedVersions` 是失败明细列表，元素为 `(asset, errorMessage)`。 | 在所有资源下载结束后刷新整体 UI、输出失败汇总、决定是否展示重试入口。 |
| `AddListenerMultiDownloadError` | `MultiDownloadErrorEventArgs` | `DownloadProgressReporter` 收到 `DownloadStatus.Failed` 时触发。当前默认桥接中的 `Version` 可能是 `AssetName`。 | `Exception`、`Version`。 | 记录单个资源下载失败、展示失败项、触发外部监控。整体是否成功仍以 `MultiAllDownloadCompleted` 为准。 |
| `AddListenerMultiDownloadStatistics` | `MultiDownloadStatisticsEventArgs` | 兼容旧下载统计或自定义下载实现的事件。当前默认下载编排主要通过 `AddListenerProgress` 分发下载进度，不会额外合成统计事件。 | `Version`、`Remaining`、`Speed`、`TotalBytesToReceive`、`BytesReceived`、`ProgressPercentage`。 | 如果你接入了仍会分发该事件的下载器，可以用它展示剩余时间和速度；新代码建议优先监听 `AddListenerProgress`。 |
| `AddListenerException` | `ExceptionEventArgs` | `GeneralUpdateBootstrap`、各平台策略、标准策略、OSS 策略和更新策略捕获异常时触发。 | `Exception`、`Message`。 | 上报异常、展示错误信息、写入业务日志。这个事件表示异常已经被 Core 捕获并通知，不等同于自动重试。 |

`UpdateInfoEventArgs.Info.Body` 中的元素是 Core 经过版本对比、应用类型筛选和下载计划构建后需要处理的版本包，不是简单的原始 HTTP 响应透传。需要关注下载 URL、Hash、强制更新、跨版本差分范围时，可以直接读取 `VersionEntry` 上的属性。

`AddListenerUpdatePrecheck` 的返回值容易误解：以当前代码为准，返回 `true` 是“可以跳过”，不是“继续下载”。它适合放在“下载前确认”这个场景里：先从 `UpdateInfoEventArgs.Info.Body` 整理本次更新涉及的版本号、更新日志、包大小、升级类型等内容，弹窗给用户阅读；用户确认更新时返回 `false` 继续，用户选择稍后、磁盘不足或当前网络不允许时返回 `true` 跳过非强制更新。如果只是展示服务端版本信息、不需要决定是否跳过，可以只监听 `AddListenerUpdateInfo`。

```csharp
await new GeneralUpdateBootstrap()
    .SetConfig(request)
    .AddListenerUpdateInfo((_, e) =>
    {
        Console.WriteLine($"Versions from server: {e.Info?.Body?.Count ?? 0}");
    })
    .AddListenerUpdatePrecheck(e =>
    {
        var versions = e.Info?.Body;
        var hasUpdate = (versions?.Count ?? 0) > 0;
        var enoughDisk = DriveInfo.GetDrives()
            .Where(d => d.IsReady)
            .Any(d => d.AvailableFreeSpace > 1024L * 1024 * 1024);
        var userRejected = versions != null && !ShowUpdateDialog(versions);

        // 当前实现中返回 true 表示跳过非强制更新，返回 false 表示继续。
        return !hasUpdate || !enoughDisk || userRejected;
    })
    .AddListenerMultiDownloadCompleted((_, e) =>
    {
        Console.WriteLine($"{e.Version}: {(e.IsCompleted ? "completed" : "failed")}");
    })
    .AddListenerMultiAllDownloadCompleted((_, e) =>
    {
        Console.WriteLine(e.IsAllDownloadCompleted
            ? "All downloads completed."
            : $"Failed downloads: {e.FailedVersions.Count}");
    })
    .AddListenerMultiDownloadError((_, e) =>
    {
        Console.WriteLine($"Download failed: {e.Version}");
        Console.WriteLine(e.Exception);
    })
    .AddListenerProgress((_, e) =>
    {
        if (e.Progress != null)
            Console.WriteLine($"Download {e.Progress.AssetName}: {e.Progress.Percentage:F1}% {e.Progress.Status}");

        if (e.DiffProgress != null)
            Console.WriteLine($"Patch: {e.DiffProgress.Completed}/{e.DiffProgress.Total} {e.DiffProgress.CurrentFile}");
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

    public override void OnProgress(ProgressEventArgs args)
    {
        if (args.Progress != null)
            Console.WriteLine($"{args.Progress.AssetName}: {args.Progress.Percentage:F1}%");
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

## 日志与性能 {#logging-performance}

Core 内置 `GeneralTracer`，默认开启。它基于 `System.Diagnostics.Trace` 输出日志：Windows 下会写入调试输出窗口，同时写到控制台，并在应用基目录创建 `Logs/generalupdate-trace yyyy-MM-dd.log` 文件。文件监听器使用后台队列写入，但每条日志仍会做开关检查、时间格式化、调用栈定位和入队/输出，因此在性能敏感场景可以关闭。

| API | 作用 |
| --- | --- |
| `GeneralTracer.SetTracingEnabled(false)` | 关闭 Core 日志输出。关闭后 `Debug` / `Info` / `Warn` / `Error` / `Fatal` 会快速返回，同时 Trace listener 被过滤。 |
| `GeneralTracer.SetTracingEnabled(true)` | 重新开启日志输出。适合诊断版本、灰度排查或用户反馈问题时启用。 |
| `GeneralTracer.IsTracingEnabled()` | 查询当前日志开关状态。 |
| `GeneralTracer.Dispose()` | 释放文件监听器并清空 Trace listeners，通常只在测试、工具进程退出或你明确接管 Trace listeners 时使用。 |

建议在应用启动早期设置日志开关，确保更新流程中的下载、校验、差分和替换日志都遵循同一个策略。

```csharp
using GeneralUpdate.Core;

if (performanceMode)
{
    GeneralTracer.SetTracingEnabled(false);
}

await new GeneralUpdateBootstrap()
    .SetSource(
        updateUrl: "https://update.example.com/api/upgrade/verification",
        appSecretKey: "your-app-secret")
    .SetOption(Option.AppType, AppType.Client)
    .LaunchAsync();
```

关闭日志适合低功耗设备、I/O 较慢的终端、静默后台轮询、批量自动化更新或对启动耗时极敏感的产品形态。排查线上问题时建议临时开启，因为 Core 的日志会记录策略分派、下载编排、校验、差分、Hook 和异常路径。

## 扩展点总览

扩展点由 `AbstractBootstrap` 提供，所有注册方法都返回当前 bootstrap，可链式调用。

| 注册方法 | 接口 | 影响范围 |
| --- | --- | --- |
| `Hooks<T>()` | `IUpdateHooks` | 更新生命周期前后置逻辑。 |
| `UpdateReporter<T>()` | `IUpdateReporter` | 更新状态上报。 |
| `SslPolicy<T>()` | `ISslValidationPolicy` | HTTPS 证书校验。 |
| `HttpAuth<T>()` | `IHttpAuthProvider` | HTTP 请求认证。 |
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
    .HttpAuth<StaticBearerAuthProvider>()
    .LaunchAsync();
```

Core 内置的认证类型包括 `NoOpAuthProvider`、`BearerTokenAuthProvider`、`ApiKeyAuthProvider` 和 `HmacAuthProvider`。这些类型中部分构造函数需要参数，因此如果要通过 `HttpAuth<T>()` 注册，通常需要写一个无参包装类。

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

> 静默更新不是单独的扩展接口，而是内置执行策略。配置方式和生命周期见 [静默更新策略](#silent-update-strategy)。

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
