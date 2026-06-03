---
sidebar_position: 5
---

# GeneralUpdate.Core

**命名空间:** `GeneralUpdate.Core` | **主要入口:** `GeneralUpdateBootstrap` | **NuGet 包:** `GeneralUpdate.Core`

## 1. 组件简介

### 1.1 组件概述

**GeneralUpdate.Core** 是 GeneralUpdate 生态的更新执行核心引擎，负责客户端应用的全生命周期更新管理。它提供可编程的启动器、配置模型、事件通知系统、下载子系统（支持并发、断点续传、重试、校验、后处理管道）、差分补丁管道、版本回写、IPC 进程通信以及平台策略扩展。

**核心能力：**

| 能力 | 说明 |
| --- | --- |
| 多策略更新执行 | 内置标准 Client/Upgrade 更新、OSS 对象存储更新、静默后台轮询更新三种执行策略 |
| 配置驱动 | 通过 `UpdateRequest` 强类型配置或 `SetSource` 轻配置入口，配合 `generalupdate.manifest.json` 实现极简接入 |
| 下载子系统 | 可替换的下载来源、执行器、重试策略、后处理管道和批量编排器，默认支持并发下载、断点续传、SHA256 校验 |
| 差分补丁管道 | 文件级二进制差分（BSDIFF 4.0 / Streaming HDiff），目录级对比与批量补丁分发，支持并行处理 |
| 事件通知 | 7 种事件回调（版本发现、下载进度、完成、错误、异常等），支持批量事件监听器注册 |
| 扩展点体系 | 10 个可替换接口：生命周期钩子、状态上报、SSL 证书策略、HTTP 认证、下载来源/策略/执行器/管道/编排器、平台策略 |
| 版本清单体系 | `generalupdate.manifest.json` 自动发现应用身份，更新后自动回写本地版本，无需业务代码维护版本号 |
| IPC 进程通信 | 主程序与升级程序之间通过加密文件传递更新上下文，保障升级流程的原子性和安全性 |
| SignalR 实时推送 | 基于 SignalR 的版本更新实时推送（`UpgradeHubService`），支持点对点和广播推送、自动重连、多事件订阅 |

**解决的业务痛点：**
- 桌面应用需要可靠的自动更新能力，但手写更新逻辑涉及版本对比、下载、校验、解压、文件替换、进程重启等多个复杂环节
- 大型应用的分发包体积大，全量更新带宽成本高，需要差分更新降低下载量
- 需要灵活的更新策略（静默后台、用户手动触发、OSS/CDN 分发）
- 升级程序版本独立演进，需要主程序和升级程序的版本协调
- 多产品线需要统一的更新框架，减少重复开发

**业务使用场景：**
- WPF / WinForms / Avalonia / WinUI 桌面应用的自动更新
- 企业内部工具的统一版本管理
- 通过 CDN / OSS 分发更新包的客户端应用
- 需要差分更新降低带宽消耗的大型客户端

### 1.2 环境与依赖

| 项目 | 说明 |
| --- | --- |
| **版本** | `10.5.0-beta.2` |
| **目标框架** | `netstandard2.0`（兼容 .NET Framework 4.6.1+ / .NET Core 2.0+ / .NET 5+） |
| **依赖包** | `GeneralUpdate.Differential`（差分算法）、`System.Text.Json`、`Microsoft.Extensions.Logging.Abstractions` |
| **兼容性** | Windows（主支持）/ Linux / macOS；支持 x86 / x64 / ARM64 |

---

## 2. 组件功能列表

| 功能名称 | 功能描述 | 类型 | 是否必填 | 备注限制 |
| --- | --- | --- | --- | --- |
| 标准 Client 更新 | 主程序检查版本、下载更新包、启动升级程序替换文件 | 基础 | 必选 | 需要服务端版本检查 API |
| 标准 Upgrade 更新 | 独立升级程序读取 IPC 上下文并执行文件替换、差分补丁、版本回写 | 基础 | 必选 | 由主程序启动，通过加密 IPC 传递上下文 |
| OSS Client 更新 | 从 OSS/CDN 下载版本配置，对比后启动升级程序 | 基础 | 可选 | 版本配置文件托管在对象存储，不依赖服务端 API |
| OSS Upgrade 更新 | OSS 模式下的升级程序，下载并解压资源包 | 基础 | 可选 | 配合 OssClient 使用 |
| 静默后台更新 | 后台轮询版本、静默下载、进程退出时触发升级 | 基础 | 可选 | 需设置 `Option.Silent = true` |
| 差分补丁管道 | 文件级二进制差分生成与应用，目录级批量补丁分发 | 基础 | 可选 | 需要 `Option.PatchEnabled = true` |
| 并发下载 | 多资源包并发下载，支持断点续传和 SHA256 校验 | 基础 | 可选 | 通过 `Option.MaxConcurrency` 控制 |
| 事件通知回调 | 版本发现、下载进度、完成、异常等 7 种事件 | 基础 | 可选 | 通过 `AddListener*` 方法注册 |
| 应用身份清单 | `generalupdate.manifest.json` 自动发现与版本回写 | 拓展 | 推荐 | 由 `GeneralUpdate.Tools` 生成 |
| 下载来源扩展 | 自定义版本清单和下载资源来源 | 拓展 | 可选 | 实现 `IDownloadSource` |
| 下载执行器扩展 | 自定义单文件下载实现（HTTP/FTP/SFTP 等） | 拓展 | 可选 | 实现 `IDownloadExecutor` |
| 下载重试策略扩展 | 自定义重试、超时、熔断策略 | 拓展 | 可选 | 实现 `IDownloadPolicy` |
| 下载后处理管道扩展 | 下载完成后自定义校验、解密、扫描等 | 拓展 | 可选 | 实现 `IDownloadPipeline` |
| 批量下载编排扩展 | 完全替换批量下载并发控制逻辑 | 拓展 | 可选 | 实现 `IDownloadOrchestrator` |
| 生命周期钩子 | 更新前、下载后、更新后、异常、启动前的业务逻辑注入 | 拓展 | 可选 | 实现 `IUpdateHooks` |
| 状态上报扩展 | 更新状态上报到自有服务端 | 拓展 | 可选 | 实现 `IUpdateReporter` |
| HTTP 认证扩展 | 自定义 HTTP 请求认证头 | 拓展 | 可选 | 实现 `IHttpAuthProvider` |
| SSL 证书策略扩展 | 自定义 HTTPS 证书校验逻辑 | 拓展 | 可选 | 实现 `ISslValidationPolicy` |
| 平台策略扩展 | 替换平台级文件操作或启动逻辑 | 拓展 | 可选 | 实现 `IStrategy` |
| SignalR 实时推送 | 服务端主动推送版本更新通知，客户端订阅接收，支持点对点和广播推送 | 拓展 | 可选 | `UpgradeHubService`，命名空间 `GeneralUpdate.Core.Hubs` |
| 推送重连机制 | 断线自动重连（随机退避策略），连接生命周期管理 | 拓展 | 可选 | `RandomRetryPolicy` |
| 推送事件订阅 | 接收消息、在线状态、重连通知、关闭通知四种事件 | 拓展 | 可选 | 通过 `AddListener*` 方法注册 |

---

## 3. API 配置说明

### 3.1 配置字段（属性 Props）

**UpdateRequest 配置属性：**

| 字段名 | 数据类型 | 默认值 | 是否必填 | 枚举/取值范围 | 说明 |
| --- | --- | --- | --- | --- | --- |
| `UpdateUrl` | `string` | — | 是 | 有效绝对 URL | 更新检查 API 地址 |
| `UpdateAppName` | `string` | `"Update.exe"` | 推荐 | 有效文件名 | 升级程序文件名，如与实际不同必须显式设置 |
| `MainAppName` | `string` | — | 推荐 | 有效文件名 | 主程序文件名，用于重启和识别 |
| `ClientVersion` | `string` | — | 推荐 | SemVer 格式 | 当前主程序版本 |
| `AppSecretKey` | `string` | — | 推荐 | — | 应用密钥，用于服务端认证 |
| `InstallPath` | `string` | `AppDomain.CurrentDomain.BaseDirectory` | 可选 | 有效目录路径 | 应用安装根目录 |
| `ReportUrl` | `string` | `null` | 可选 | 有效绝对 URL | 更新状态上报 API |
| `UpdateLogUrl` | `string` | `null` | 可选 | 有效绝对 URL | 更新日志页面地址 |
| `UpgradeClientVersion` | `string` | — | 可选 | SemVer 格式 | 升级程序自身版本 |
| `ProductId` | `string` | — | 可选 | — | 产品标识，多产品时用于区分 |
| `UpdatePath` | `string` | `InstallPath` | 可选 | 有效目录路径 | 升级程序所在目录 |
| `Bowl` | `string` | `null` | 可选 | 有效文件名 | 更新前需关闭的辅助进程名 |
| `Scheme` | `string` | `null` | 可选 | `"Bearer"` 等 | 认证方案 |
| `Token` | `string` | `null` | 可选 | — | 认证令牌 |
| `Files` | `List<string>` | `null` | 可选 | — | 更新时跳过的指定文件列表 |
| `Formats` | `List<string>` | `null` | 可选 | — | 更新时跳过的扩展名列表 |
| `Directories` | `List<string>` | `null` | 可选 | — | 更新时跳过的目录列表 |
| `DriverDirectory` | `string` | `null` | 可选 | 有效目录路径 | 驱动更新目录 |

**Option 运行时选项：**

| 字段名 | 数据类型 | 默认值 | 是否必填 | 枚举/取值范围 | 说明 |
| --- | --- | --- | --- | --- | --- |
| `Option.AppType` | `AppType` | `Client` | 是 | `Client(1)`, `Upgrade(2)`, `OssClient(3)`, `OssUpgrade(4)` | 当前进程角色 |
| `Option.DiffMode` | `DiffMode` | `Serial` | 可选 | `Serial`, `Parallel` | 下载执行模式 |
| `Option.Encoding` | `Encoding` | `UTF8` | 可选 | `Encoding` 实例 | 压缩包处理编码 |
| `Option.Format` | `Format` | `Zip` | 可选 | `Zip` | 更新包格式 |
| `Option.DownloadTimeout` | `int?` | `30` | 可选 | 正整数（秒） | 下载超时时间 |
| `Option.PatchEnabled` | `bool?` | `true` | 可选 | `true` / `false` | 是否启用差分补丁 |
| `Option.BackupEnabled` | `bool?` | `true` | 可选 | `true` / `false` | 更新前是否备份被替换文件 |
| `Option.Silent` | `bool` | `false` | 可选 | `true` / `false` | 是否启用静默轮询更新 |
| `Option.SilentPollIntervalMinutes` | `int` | `60` | 可选 | 正整数 | 静默模式轮询间隔（分钟） |
| `Option.LaunchClientAfterUpdate` | `bool` | `true` | 可选 | `true` / `false` | 升级后是否启动主程序 |
| `Option.MaxConcurrency` | `int` | `3` | 可选 | `1` ~ `ProcessorCount × 2` | 下载最大并发数 |
| `Option.EnableResume` | `bool` | `true` | 可选 | `true` / `false` | 是否启用断点续传 |
| `Option.RetryCount` | `int` | `3` | 可选 | 非负整数 | 下载重试次数 |
| `Option.VerifyChecksum` | `bool` | `true` | 可选 | `true` / `false` | 是否校验下载文件 Hash |
| `Option.RetryInterval` | `TimeSpan` | `1s` | 可选 | `TimeSpan` 正值 | 下载重试间隔 |

### 3.2 实例方法

**GeneralUpdateBootstrap：**

| 方法名 | 入参明细 | 返回值 | 使用场景 | 注意事项 |
| --- | --- | --- | --- | --- |
| `LaunchAsync()` | 无（读取已配置的 Option 和 Config） | `Task<GeneralUpdateBootstrap>` | 所有 Core 使用场景的最终入口 | 会根据 `Option.AppType` 自动选择执行策略 |
| `Cancel()` | 无 | `void` | UI 中提供"取消更新"按钮 | 触发内部 `CancellationTokenSource` |
| `SetConfig(UpdateRequest)` | `configInfo` — 更新配置对象 | `GeneralUpdateBootstrap` | 主程序内显式配置更新参数 | 会调用 `Validate()` 检查关键字段 |
| `SetConfig(string)` | `filePath` — JSON 配置文件路径 | `GeneralUpdateBootstrap` | 从文件读取更新配置 | 支持相对路径和绝对路径；UTF-8 JSON 格式 |
| `SetSource(...)` | `updateUrl`, `appSecretKey`, `reportUrl?`, `scheme?`, `token?` | `GeneralUpdateBootstrap` | 轻配置入口，配合 manifest 使用 | 只提供服务端入口和密钥，身份信息由 manifest 补齐 |
| `SetOption(Option<T>, T)` | `option` — 选项键, `value` — 选项值 | `GeneralUpdateBootstrap` | 设置运行时选项 | 传入 `null` 给可空选项会移除当前设置 |
| `UseDiffPipeline(Action<DiffPipelineBuilder>)` | `configure` — 差分管道配置委托 | `GeneralUpdateBootstrap` | 替换或调整差分补丁管道 | 未调用时使用默认配置 |
| `AddListenerUpdateInfo(...)` | `EventHandler<UpdateInfoEventArgs>` | `GeneralUpdateBootstrap` | 接收服务端版本信息 | 无更新时也会触发（`Info.Code = 404`） |
| `AddListenerUpdatePrecheck(...)` | `Func<UpdateInfoEventArgs, bool>` | `GeneralUpdateBootstrap` | 下载前预检查 | 返回 `true` 表示跳过非强制更新 |
| `AddListenerProgress(...)` | `EventHandler<ProgressEventArgs>` | `GeneralUpdateBootstrap` | 更新进度条、状态文本 | 同时包含下载进度和差分进度 |
| `AddListenerMultiDownloadCompleted(...)` | `EventHandler<MultiDownloadCompletedEventArgs>` | `GeneralUpdateBootstrap` | 标记单个资源下载完成 | 不要当做"全部下载完成" |
| `AddListenerMultiAllDownloadCompleted(...)` | `EventHandler<MultiAllDownloadCompletedEventArgs>` | `GeneralUpdateBootstrap` | 全部下载完成后的后续处理 | 包含失败汇总 `FailedVersions` |
| `AddListenerMultiDownloadError(...)` | `EventHandler<MultiDownloadErrorEventArgs>` | `GeneralUpdateBootstrap` | 记录单个资源下载失败 | 整体成功仍以 `MultiAllDownloadCompleted` 为准 |
| `AddListenerMultiDownloadStatistics(...)` | `EventHandler<MultiDownloadStatisticsEventArgs>` | `GeneralUpdateBootstrap` | 展示下载速度和剩余时间 | 新代码优先使用 `AddListenerProgress` |
| `AddListenerException(...)` | `EventHandler<ExceptionEventArgs>` | `GeneralUpdateBootstrap` | 上报异常、展示错误信息 | 仅通知，不自动重试 |
| `AddEventListener<TListener>()` | 泛型参数 — 监听器类型 | `GeneralUpdateBootstrap` | 批量注册事件监听器 | `T` 必须实现 `IUpdateEventListener`，推荐继承 `UpdateEventListenerBase` |
| `Hooks<T>()` | 泛型参数 — Hook 类型 | `GeneralUpdateBootstrap` | 注册生命周期钩子 | `T` 必须实现 `IUpdateHooks` 且有无参构造函数 |
| `UpdateReporter<T>()` | 泛型参数 — Reporter 类型 | `GeneralUpdateBootstrap` | 注册状态上报器 | `T` 必须实现 `IUpdateReporter` |
| `SslPolicy<T>()` | 泛型参数 — SSL 策略类型 | `GeneralUpdateBootstrap` | 自定义 HTTPS 证书校验 | 生产环境不建议无条件返回 `true` |
| `HttpAuth<T>()` | 泛型参数 — 认证提供器类型 | `GeneralUpdateBootstrap` | 自定义 HTTP 请求认证 | `T` 必须实现 `IHttpAuthProvider` |
| `DownloadSource<T>()` | 泛型参数 — 下载来源类型 | `GeneralUpdateBootstrap` | 自定义版本清单来源 | `T` 必须实现 `IDownloadSource` |
| `DownloadPolicy<T>()` | 泛型参数 — 下载策略类型 | `GeneralUpdateBootstrap` | 自定义下载重试/超时策略 | `T` 必须实现 `IDownloadPolicy` |
| `DownloadExecutor<T>()` | 泛型参数 — 下载执行器类型 | `GeneralUpdateBootstrap` | 自定义单文件下载实现 | `T` 必须实现 `IDownloadExecutor` |
| `DownloadPipeline<T>()` | 泛型参数 — 下载管道类型 | `GeneralUpdateBootstrap` | 自定义下载后处理 | `T` 必须实现 `IDownloadPipeline` |
| `DownloadOrchestrator<T>()` | 泛型参数 — 下载编排器类型 | `GeneralUpdateBootstrap` | 完全替换批量下载逻辑 | 只有需要完整替换下载行为时才建议实现 |
| `Strategy<T>()` | 泛型参数 — 策略类型 | `GeneralUpdateBootstrap` | 自定义平台级更新策略 | `T` 必须实现 `IStrategy` |

**DiffPipelineBuilder：**

| 方法名 | 入参明细 | 返回值 | 使用场景 | 注意事项 |
| --- | --- | --- | --- | --- |
| `UseDiffer(IBinaryDiffer)` | `differ` — 差分算法实例 | `DiffPipelineBuilder` | 替换文件级差分算法 | 可选 `BsdiffDiffer` / `StreamingHdiffDiffer` / 自定义 |
| `UseCleanMatcher(ICleanMatcher)` | `matcher` — Clean 匹配器 | `DiffPipelineBuilder` | 自定义 Clean 阶段的文件匹配逻辑 | 默认 `DefaultCleanMatcher` |
| `UseDirtyMatcher(IDirtyMatcher)` | `matcher` — Dirty 匹配器 | `DiffPipelineBuilder` | 自定义 Dirty 阶段的补丁匹配逻辑 | 默认 `DefaultDirtyMatcher` |
| `WithParallelism(int)` | `degree` — 并行度 | `DiffPipelineBuilder` | 设置差分文件并行处理数 | 默认 2；建议 1-8 |
| `WithStopOnFirstError(bool)` | `stop` — 是否首次错误即停止 | `DiffPipelineBuilder` | 错误策略控制 | 默认 `false` |
| `WithProgress(IProgress<DiffProgress>)` | `progress` — 进度报告器 | `DiffPipelineBuilder` | 接入差分进度回调 | 可配合 `Progress<T>` 使用 |
| `Build()` | 无 | `DiffPipeline` | 构建差分管道实例 | 一般在 `UseDiffPipeline` 回调内部调用 |

**UpdateRequestBuilder：**

| 方法名 | 入参明细 | 返回值 | 使用场景 | 注意事项 |
| --- | --- | --- | --- | --- |
| `Create()` | 无 | `UpdateRequestBuilder` | 从 `update_config.json` 读取配置 | 文件不存在会抛出 `FileNotFoundException` |
| `SetUpdateUrl(string)` | `url` | `UpdateRequestBuilder` | 设置更新地址 | 必须为绝对 URL |
| `SetUpgradeAppName(string)` | `name` | `UpdateRequestBuilder` | 设置升级程序文件名 | — |
| `SetMainAppName(string)` | `name` | `UpdateRequestBuilder` | 设置主程序文件名 | — |
| `SetClientVersion(string)` | `version` | `UpdateRequestBuilder` | 设置客户端版本 | SemVer 格式 |
| `SetAppSecretKey(string)` | `key` | `UpdateRequestBuilder` | 设置应用密钥 | — |
| `SetInstallPath(string)` | `path` | `UpdateRequestBuilder` | 设置安装目录 | — |
| `SetProductId(string)` | `id` | `UpdateRequestBuilder` | 设置产品标识 | — |
| `SetReportUrl(string)` | `url` | `UpdateRequestBuilder` | 设置上报地址 | — |
| `SetUpdateLogUrl(string)` | `url` | `UpdateRequestBuilder` | 设置更新日志地址 | — |
| `SetUpgradeClientVersion(string)` | `version` | `UpdateRequestBuilder` | 设置升级程序版本 | — |
| `SetBowl(string)` | `bowl` | `UpdateRequestBuilder` | 设置 Bowl 进程名 | — |
| `SetDriverDirectory(string)` | `path` | `UpdateRequestBuilder` | 设置驱动目录 | — |
| `SetScheme(string)` | `scheme` | `UpdateRequestBuilder` | 设置认证方案 | — |
| `SetToken(string)` | `token` | `UpdateRequestBuilder` | 设置认证令牌 | — |
| `SetFiles(List<string>)` | `files` | `UpdateRequestBuilder` | 设置跳过文件列表 | — |
| `SetFormats(List<string>)` | `formats` | `UpdateRequestBuilder` | 设置跳过扩展名列表 | — |
| `SetDirectories(List<string>)` | `dirs` | `UpdateRequestBuilder` | 设置跳过目录列表 | — |
| `Build()` | 无 | `UpdateRequest` | 构建并校验配置对象 | 会执行 `Validate()` |

**UpgradeHubService：**

| 方法名 | 入参明细 | 返回值 | 使用场景 | 注意事项 |
| --- | --- | --- | --- | --- |
| `UpgradeHubService(string, string?, string?)` | `url` — SignalR Hub 地址；`token` — 可选 ID4 认证令牌；`appkey` — 可选客户端唯一标识 | —（构造函数） | 创建推送服务实例 | `appkey` 用于服务端定向推送，推荐使用固定 GUID |
| `StartAsync()` | 无 | `Task` | 建立 SignalR 长连接，开始接收推送 | 可重复调用（先 `StopAsync` 后重新 `StartAsync`） |
| `StopAsync()` | 无 | `Task` | 优雅停止连接，保留重连能力 | 适合应用进入后台时调用 |
| `DisposeAsync()` | 无 | `Task` | 彻底释放 Hub 及所有资源 | 释放后不可再复用 |
| `AddListenerReceive(Action<string>)` | `receiveMessageCallback` — 接收消息回调 | `void` | 订阅服务端推送的版本更新消息 | 消息内容为服务端推送的 JSON 字符串 |
| `AddListenerOnline(Action<string>)` | `onlineMessageCallback` — 状态回调 | `void` | 订阅在线/离线状态变化通知 | — |
| `AddListenerReconnected(Func<string?, Task>?)` | `reconnectedCallback` — 重连回调 | `void` | 订阅断线重连成功通知 | 参数为新的 connectionId（可能为 null） |
| `AddListenerClosed(Func<Exception?, Task>)` | `closeCallback` — 关闭回调 | `void` | 订阅连接关闭通知 | 正常关闭时异常参数为 null |

### 3.3 回调事件

| 事件名称 | 回调参数 | 触发时机 | 使用说明 |
| --- | --- | --- | --- |
| `AddListenerUpdateInfo` | `UpdateInfoEventArgs` — `Info.Code`, `Info.Body`（`VersionEntry` 列表） | 标准 Client 策略完成版本对比后触发 | 无更新时 `Code = 404`；有更新时 `Body` 包含待下载的 `VersionEntry` 列表 |
| `AddListenerUpdatePrecheck` | `Func<UpdateInfoEventArgs, bool>` — 返回 `true` 跳过（非强制），`false` 继续 | `UpdateInfo` 事件之后、下载之前 | 用于磁盘空间检查、网络检测、用户确认弹窗；强制更新不进入跳过逻辑 |
| `AddListenerProgress` | `ProgressEventArgs` — `Progress`（下载）或 `DiffProgress`（差分） | 下载进度或差分进度更新时 | 同一参数中 `Progress` 和 `DiffProgress` 只有一个非空 |
| `AddListenerMultiDownloadCompleted` | `MultiDownloadCompletedEventArgs` — `Version`, `IsCompleted` | 单个资源包下载完成时 | 不要当作"全部资源下载完成"的判断依据 |
| `AddListenerMultiAllDownloadCompleted` | `MultiAllDownloadCompletedEventArgs` — `IsAllDownloadCompleted`, `FailedVersions` | 所有下载任务结束后触发一次 | 失败明细在 `FailedVersions` 中 |
| `AddListenerMultiDownloadError` | `MultiDownloadErrorEventArgs` — `Exception`, `Version` | 单个资源下载失败时 | 记录失败项用于展示或监控 |
| `AddListenerMultiDownloadStatistics` | `MultiDownloadStatisticsEventArgs` — `Speed`, `Remaining`, `BytesReceived` | 兼容旧下载统计/自定义下载实现 | 新代码建议使用 `AddListenerProgress` |
| `AddListenerException` | `ExceptionEventArgs` — `Exception`, `Message` | 各策略捕获异常时 | 仅通知，不等同于自动重试 |

**UpgradeHubService 推送事件：**

| 事件名称 | 回调参数 | 触发时机 | 使用说明 |
| --- | --- | --- | --- |
| `AddListenerReceive` | `Action<string>` — 消息内容（JSON 字符串） | 服务端推送版本更新消息时 | 消息格式由服务端决定，建议 JSON 格式 |
| `AddListenerOnline` | `Action<string>` — 状态描述 | 在线/离线状态变化时 | 用于 UI 状态展示 |
| `AddListenerReconnected` | `Func<string?, Task>?` — 新的 connectionId | 断线重连成功后 | 可用于刷新客户端状态 |
| `AddListenerClosed` | `Func<Exception?, Task>` — 关闭原因（null 为正常关闭） | 连接关闭时 | 用于记录日志和清理资源 |

---

## 4. 扩展示例（高阶用法）

### 4.1 组件可扩展能力总览

Core 通过 `AbstractBootstrap` 基类提供 10 个扩展注册方法，全部返回当前 bootstrap 实例，支持链式调用。所有注册的类型必须具有无参构造函数。

| 扩展接口 | 注册方法 | 影响范围 |
| --- | --- | --- |
| `IUpdateHooks` | `Hooks<T>()` | 更新生命周期前后置逻辑 |
| `IUpdateReporter` | `UpdateReporter<T>()` | 更新状态上报 |
| `ISslValidationPolicy` | `SslPolicy<T>()` | HTTPS 证书校验 |
| `IHttpAuthProvider` | `HttpAuth<T>()` | HTTP 请求认证 |
| `IDownloadSource` | `DownloadSource<T>()` | 版本清单和下载资源来源 |
| `IDownloadPolicy` | `DownloadPolicy<T>()` | 下载重试、超时、熔断策略 |
| `IDownloadExecutor` | `DownloadExecutor<T>()` | 单文件下载实现 |
| `IDownloadPipeline` | `DownloadPipeline<T>()` | 下载后处理（校验、解密、扫描） |
| `IDownloadOrchestrator` | `DownloadOrchestrator<T>()` | 批量下载完整编排 |
| `IStrategy` | `Strategy<T>()` | 自定义平台级更新策略 |

### 4.2 分场景示例

#### 场景 1：自定义差分算法与并行度

【场景说明】大型项目希望使用 `StreamingHdiffDiffer` 获得更快的客户端补丁应用速度，并设置并行度为 4。

【示例代码】

```csharp
using GeneralUpdate.Core;
using GeneralUpdate.Core.Differential;
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
            .WithStopOnFirstError(true);
    })
    .SetOption(Option.PatchEnabled, true)
    .SetOption(Option.AppType, AppType.Client)
    .LaunchAsync();
```

【效果&注意事项】
- `StreamingHdiffDiffer` 默认使用 Deflate 压缩，客户端应用补丁更快
- `WithStopOnFirstError(true)` 表示任意补丁失败立即停止所有并行任务
- 并行度 4 适合多核 CPU + SSD 环境

#### 场景 2：自定义生命周期钩子

【场景说明】在更新前检查磁盘空间、更新后写日志、Linux/macOS 下启动前赋予执行权限。

【示例代码】

```csharp
using GeneralUpdate.Core.Hooks;

public sealed class ProductUpdateHooks : IUpdateHooks
{
    public Task<bool> OnBeforeUpdateAsync(HookContext ctx)
    {
        var drive = new DriveInfo(Path.GetPathRoot(ctx.InstallPath)!);
        if (drive.AvailableFreeSpace < 500L * 1024 * 1024)
        {
            Console.WriteLine("Insufficient disk space for update.");
            return Task.FromResult(false); // 拒绝更新
        }
        return Task.FromResult(true);
    }

    public Task OnDownloadCompletedAsync(DownloadContext ctx)
    {
        Console.WriteLine($"Downloaded {ctx.AssetName}: {ctx.Success}");
        return Task.CompletedTask;
    }

    public Task OnAfterUpdateAsync(HookContext ctx)
    {
        File.AppendAllText(
            Path.Combine(ctx.InstallPath, "update-history.log"),
            $"{DateTimeOffset.Now:O} {ctx.CurrentVersion} -> {ctx.TargetVersion}{Environment.NewLine}");
        return Task.CompletedTask;
    }

    public Task OnUpdateErrorAsync(HookContext ctx, Exception ex)
    {
        File.AppendAllText(
            Path.Combine(ctx.InstallPath, "update-error.log"),
            $"{ex}{Environment.NewLine}");
        return Task.CompletedTask;
    }

    public Task OnBeforeStartAppAsync(HookContext ctx)
    {
        // Linux/macOS 下可用 UnixPermissionHooks
        return Task.CompletedTask;
    }
}

await new GeneralUpdateBootstrap()
    .SetConfig(request)
    .Hooks<ProductUpdateHooks>()
    .SetOption(Option.AppType, AppType.Client)
    .LaunchAsync();
```

【效果&注意事项】
- `OnBeforeUpdateAsync` 返回 `false` 会中断本次更新
- `OnBeforeStartAppAsync` 适合 Linux/macOS 的 `chmod +x` 操作
- 内置 `UnixPermissionHooks` 可直接使用

#### 场景 3：自定义下载来源（私有服务/配置中心）

【场景说明】从企业内部配置中心获取下载资源列表，而不是调用标准版本检查 API。

【示例代码】

```csharp
using GeneralUpdate.Core.Download.Abstractions;
using GeneralUpdate.Core.Download.Models;

public sealed class ConfigCenterDownloadSource : IDownloadSource
{
    public async Task<DownloadSourceResult> ListAsync(CancellationToken token = default)
    {
        // 从配置中心拉取资源列表...
        var assets = new[]
        {
            new DownloadAsset(
                Name: "MyApp-2.0.0.zip",
                Url: "https://cdn.internal.example.com/releases/MyApp-2.0.0.zip",
                Size: 50_000_000,
                SHA256: "abc123...",
                Version: "2.0.0")
        };

        return new DownloadSourceResult
        {
            Assets = assets,
            HasMainUpdate = true,
            HasUpgradeUpdate = false
        };
    }
}

await new GeneralUpdateBootstrap()
    .SetConfig(request)
    .DownloadSource<ConfigCenterDownloadSource>()
    .SetOption(Option.AppType, AppType.Client)
    .LaunchAsync();
```

【效果&注意事项】
- 自定义 `IDownloadSource` 会完全替换默认的 HTTP 版本检查逻辑
- 需要同时注册 `DownloadOrchestrator<T>()` 时，orchestrator 会接管完整下载流程

#### 场景 4：自定义 HTTP 认证

【场景说明】为 Core 发出的 HTTP 请求追加 JWT Bearer Token 认证头。

【示例代码】

```csharp
using GeneralUpdate.Core.Security;

public sealed class JwtAuthProvider : IHttpAuthProvider
{
    private readonly string _token;

    public JwtAuthProvider()
    {
        // 从配置或环境变量读取 token
        _token = Environment.GetEnvironmentVariable("UPDATE_JWT_TOKEN") ?? "";
    }

    public Task ApplyAuthAsync(HttpRequestMessage request, CancellationToken token = default)
    {
        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
        return Task.CompletedTask;
    }
}

await new GeneralUpdateBootstrap()
    .SetConfig(request)
    .HttpAuth<JwtAuthProvider>()
    .SetOption(Option.AppType, AppType.Client)
    .LaunchAsync();
```

【效果&注意事项】
- 认证提供器在每次 HTTP 请求前被调用
- 需要在无参构造函数中自行读取配置

#### 场景 5：静默更新 + 进程退出触发升级

【场景说明】主程序启动后后台轮询更新，下载完成后不打扰用户，待进程退出时启动升级程序。

【示例代码】

```csharp
using GeneralUpdate.Core;

// 主程序启动时
var bootstrap = new GeneralUpdateBootstrap()
    .SetSource(
        updateUrl: "https://update.example.com/api/upgrade/verification",
        appSecretKey: "your-app-secret")
    .SetOption(Option.AppType, AppType.Client)
    .SetOption(Option.Silent, true)
    .SetOption(Option.SilentPollIntervalMinutes, 30)
    .SetOption(Option.LaunchClientAfterUpdate, true)
    .AddListenerException((_, e) =>
    {
        Console.WriteLine($"Update error: {e.Message}");
    });

await bootstrap.LaunchAsync();

// 应用退出时检查是否有准备好的更新
AppDomain.CurrentDomain.ProcessExit += (_, _) =>
{
    if (bootstrap.SilentOrchestrator?.HasPreparedUpdate == true)
    {
        bootstrap.SilentOrchestrator.TryLaunchUpgrade();
    }
};
```

【效果&注意事项】
- 静默模式在 `AppType.Client` 下生效
- 文件替换发生在主程序退出之后
- 轮询间隔建议不低于 30 分钟

#### 场景 6：SignalR 实时推送 + 常规更新联动

【场景说明】客户端同时使用 `UpgradeHubService` 接收服务端实时推送和 `GeneralUpdateBootstrap` 执行常规更新。服务端有新版时可立即推送通知，客户端无需等待轮询即可触发更新。

【示例代码】

```csharp
using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;
using GeneralUpdate.Core.Hubs;

// 1. 启动 SignalR 推送监听
var hub = new UpgradeHubService(
    "http://localhost:5000/UpgradeHub",
    token: null,
    appkey: "dfeb5833-975e-4afb-88f1-6278ee9aeff6");

hub.AddListenerReceive(async (message) =>
{
    Console.WriteLine($"收到实时推送: {message}");
    // 收到推送后可以立即触发更新检查
    // 或在 UI 中提示用户有新版本可用
});

hub.AddListenerOnline((info) =>
    Console.WriteLine($"在线状态: {info}"));

hub.AddListenerReconnected((connectionId) =>
{
    Console.WriteLine($"已重连，connectionId={connectionId}");
    return Task.CompletedTask;
});

hub.AddListenerClosed((exception) =>
{
    if (exception is not null)
        Console.WriteLine($"连接异常关闭: {exception.Message}");
    else
        Console.WriteLine("连接已正常关闭");
    return Task.CompletedTask;
});

await hub.StartAsync();

// 2. 常规更新流程
await new GeneralUpdateBootstrap()
    .SetSource(
        updateUrl: "https://update.example.com/api/upgrade/verification",
        appSecretKey: "your-app-secret")
    .SetOption(Option.AppType, AppType.Client)
    .AddListenerException((_, e) => Console.WriteLine(e.Exception))
    .LaunchAsync();

// 3. 应用退出时清理
// await hub.StopAsync();
// await hub.DisposeAsync();
```

【效果&注意事项】
- `UpgradeHubService` 与 `GeneralUpdateBootstrap` 互补：推送做通知，Bootstrap 做实际更新
- `appkey` 推荐与 `AppSecretKey` 保持一致，便于服务端定向推送
- `StopAsync` 保留重连能力，适合应用进入后台时调用
- `DisposeAsync` 彻底释放，适合应用退出时调用

#### 场景 7：DI 容器中注册 UpgradeHubService

【场景说明】在 Prism / Generic Host / ASP.NET Core 等 DI 容器中注册 `IUpgradeHubService`，管理推送服务生命周期。

【示例代码】

```csharp
using GeneralUpdate.Core.Hubs;

// Prism 示例
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

// Generic Host / ASP.NET Core 示例
builder.Services.AddSingleton<IUpgradeHubService>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new UpgradeHubService(
        config["HubUrl"]!,
        appkey: config["AppSecretKey"]);
});
```

【效果&注意事项】
- DI 容器管理生命周期，避免手动 Dispose
- 可将配置从 `appsettings.json` 注入

---

## 5. 常规使用示例

### 5.1 快速入门示例（最简 demo）

最简配置：使用 manifest 自动发现身份信息，只需要配置服务端入口和密钥。

```csharp
using GeneralUpdate.Core;

await new GeneralUpdateBootstrap()
    .SetSource(
        updateUrl: "https://update.example.com/api/upgrade/verification",
        appSecretKey: "your-app-secret")
    .SetOption(Option.AppType, AppType.Client)
    .LaunchAsync();
```

升级程序入口（`Update.exe`）：

```csharp
await new GeneralUpdateBootstrap()
    .SetOption(Option.AppType, AppType.Upgrade)
    .AddListenerException((_, e) => Console.WriteLine(e.Exception))
    .LaunchAsync();
```

### 5.2 基础参数组合示例

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
    .SetOption(Option.DiffMode, DiffMode.Parallel)
    .SetOption(Option.MaxConcurrency, 4)
    .SetOption(Option.PatchEnabled, true)
    .AddListenerProgress((_, e) =>
    {
        if (e.Progress != null)
            Console.WriteLine($"{e.Progress.AssetName}: {e.Progress.Percentage:F1}%");
    })
    .AddListenerException((_, e) => Console.WriteLine(e.Exception))
    .LaunchAsync();
```

### 5.2.1 SignalR 实时推送快速入门

```csharp
using GeneralUpdate.Core.Hubs;

// 创建推送客户端
var hub = new UpgradeHubService(
    "http://localhost:5000/UpgradeHub",
    appkey: Guid.NewGuid().ToString());

// 订阅推送消息
hub.AddListenerReceive((message) =>
{
    Console.WriteLine($"收到更新推送: {message}");
});

// 建立连接
await hub.StartAsync();

Console.WriteLine("已连接，等待服务端推送...");
Console.ReadLine();

// 停止连接（保留重连能力）
await hub.StopAsync();

// 释放资源（不可再复用）
await hub.DisposeAsync();
```

### 5.3 真实业务落地示例（多参数联动）

完整 Client 端更新流程，包含事件监听、差分管道、并发控制和状态上报：

```csharp
using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;
using GeneralUpdate.Core.Download;
using GeneralUpdate.Core.Download.Reporting;
using GeneralUpdate.Core.Event;
using GeneralUpdate.Core.Hooks;
using GeneralUpdate.Core.Pipeline;
using GeneralUpdate.Differential.Differ;

// 1. 构建配置
var request = new UpdateRequestBuilder()
    .SetUpdateUrl("https://update.mycompany.com/api/upgrade/verification")
    .SetReportUrl("https://update.mycompany.com/api/upgrade/report")
    .SetUpgradeAppName("MyApp.Upgrade.exe")
    .SetMainAppName("MyApp.exe")
    .SetClientVersion("1.0.0")
    .SetUpgradeClientVersion("1.0.0")
    .SetAppSecretKey("prod-secret-key")
    .SetProductId("my-product")
    .SetInstallPath(AppDomain.CurrentDomain.BaseDirectory)
    .SetScheme("Bearer")
    .SetToken(Environment.GetEnvironmentVariable("UPDATE_TOKEN") ?? "")
    .SetFiles(new List<string> { "appsettings.Development.json" })
    .SetFormats(new List<string> { ".log", ".tmp", ".pdb" })
    .SetDirectories(new List<string> { "logs", "cache", "temp" })
    .Build();

// 2. 注入业务钩子
public sealed class BusinessUpdateHooks : IUpdateHooks
{
    public Task<bool> OnBeforeUpdateAsync(HookContext ctx)
    {
        // 检查磁盘空间
        var drive = new DriveInfo(Path.GetPathRoot(ctx.InstallPath)!);
        if (drive.AvailableFreeSpace < 1024L * 1024 * 1024) // < 1GB
            return Task.FromResult(false);
        return Task.FromResult(true);
    }

    public Task OnDownloadCompletedAsync(DownloadContext ctx)
        => Task.CompletedTask;

    public Task OnAfterUpdateAsync(HookContext ctx)
    {
        // 写更新成功日志
        File.AppendAllText(
            Path.Combine(ctx.InstallPath, "update.log"),
            $"{DateTimeOffset.Now:O} Updated to {ctx.TargetVersion}{Environment.NewLine}");
        return Task.CompletedTask;
    }

    public Task OnUpdateErrorAsync(HookContext ctx, Exception ex)
    {
        // 上报到监控系统
        return Task.CompletedTask;
    }

    public Task OnBeforeStartAppAsync(HookContext ctx)
        => Task.CompletedTask;
}

// 3. 配置差分管道
Action<DiffPipelineBuilder> configurePipeline = builder =>
{
    builder
        .UseDiffer(new StreamingHdiffDiffer())
        .WithParallelism(4)
        .WithStopOnFirstError(true)
        .WithProgress(new Progress<DiffProgress>(p =>
        {
            Console.WriteLine($"Patch: {p.Completed}/{p.Total} {p.CurrentFile} {p.Percentage}%");
        }));
};

// 4. 启动更新
var bootstrap = new GeneralUpdateBootstrap()
    .SetConfig(request)
    .SetOption(Option.AppType, AppType.Client)
    .SetOption(Option.DiffMode, DiffMode.Parallel)
    .SetOption(Option.MaxConcurrency, 4)
    .SetOption(Option.DownloadTimeout, 120)
    .SetOption(Option.PatchEnabled, true)
    .SetOption(Option.BackupEnabled, true)
    .SetOption(Option.VerifyChecksum, true)
    .SetOption(Option.RetryCount, 5)
    .SetOption(Option.RetryInterval, TimeSpan.FromSeconds(2))
    .Hooks<BusinessUpdateHooks>()
    .UseDiffPipeline(configurePipeline)
    .AddListenerUpdateInfo((_, e) =>
    {
        if (e.Info?.Code == "404")
        {
            Console.WriteLine("Already up to date.");
            return;
        }
        Console.WriteLine($"Found {e.Info?.Body?.Count ?? 0} version(s) to download.");
    })
    .AddListenerUpdatePrecheck(e =>
    {
        var hasUpdate = (e.Info?.Body?.Count ?? 0) > 0;
        return !hasUpdate; // 有更新则继续（返回 false = 不跳过）
    })
    .AddListenerProgress((_, e) =>
    {
        if (e.Progress != null)
        {
            var pct = e.Progress.Percentage;
            var status = e.Progress.Status;
            var name = e.Progress.AssetName;
            Console.WriteLine($"[Download] {name}: {pct:F1}% ({status})");
        }
        if (e.DiffProgress != null)
        {
            var d = e.DiffProgress;
            Console.WriteLine($"[Patch] {d.CurrentFile}: {d.Completed}/{d.Total} {d.Percentage}%");
        }
    })
    .AddListenerMultiAllDownloadCompleted((_, e) =>
    {
        if (e.IsAllDownloadCompleted)
            Console.WriteLine("All downloads completed successfully.");
        else
        {
            Console.WriteLine($"Download completed with {e.FailedVersions.Count} failure(s):");
            foreach (var (asset, error) in e.FailedVersions)
                Console.WriteLine($"  - {asset.Name}: {error}");
        }
    })
    .AddListenerException((_, e) =>
    {
        Console.WriteLine($"Update error: {e.Message}");
        Console.WriteLine(e.Exception);
    });

await bootstrap.LaunchAsync();
```

---

## 6. 全局配置

Core 不依赖全局配置文件，而是通过 `generalupdate.manifest.json` 提供应用身份信息。下面是 manifest 的配置语法和优先级规则。

### Manifest 配置语法

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

### 配置优先级规则

| 优先级 | 配置来源 | 说明 |
| --- | --- | --- |
| 1（最高） | 代码中 `SetConfig(UpdateRequest)` 或 `SetSource(...)` 显式设置的值 | 覆盖所有其他来源 |
| 2 | `generalupdate.manifest.json` 中的字段 | 自动补齐代码中未显式设置的字段 |
| 3（最低） | 组件内部默认值 | `UpdateAppName = "Update.exe"`, `InstallPath = BaseDirectory` 等 |

### Manifest 字段映射

| JSON 字段 | Core 字段 | 说明 |
| --- | --- | --- |
| `mainAppName` | `MainAppName` | 主程序可执行文件名 |
| `clientVersion` | `ClientVersion` | 当前主程序版本 |
| `appType` | `AppType` | 当前进程角色（`Client`, `Upgrade`, `OssClient`, `OssUpgrade`） |
| `updateAppName` | `UpdateAppName` | 升级程序文件名 |
| `upgradeClientVersion` | `UpgradeClientVersion` | 升级程序自身版本 |
| `productId` | `ProductId` | 产品标识 |
| `updatePath` | `UpdatePath` | 升级程序所在目录（相对 `InstallPath`） |

### 版本回写机制

更新成功后，Core 自动回写客户端版本到 `generalupdate.manifest.json`：

| 场景 | 回写字段 |
| --- | --- |
| 主程序更新完成 | `ClientVersion` |
| 升级程序自身更新完成 | `UpgradeClientVersion` |

### 日志配置

```csharp
using GeneralUpdate.Core;

// 关闭日志（性能敏感场景）
GeneralTracer.SetTracingEnabled(false);

// 重新开启（排查问题）
GeneralTracer.SetTracingEnabled(true);

// 释放日志资源
GeneralTracer.Dispose();
```

---

## 相关资源

- [GeneralUpdate 仓库](https://github.com/GeneralLibrary/GeneralUpdate)
- [Samples 示例代码](https://github.com/GeneralLibrary/GeneralUpdate-Samples)
- [GeneralUpdate.Tools](https://github.com/GeneralLibrary/GeneralUpdate.Tools)
- [快速开始](../quickstart/Quik%20start.md)
- [架构指南](../guide/Architecture.md)
