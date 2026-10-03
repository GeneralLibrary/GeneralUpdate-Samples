---
sidebar_position: 9
---

# GeneralUpdate.Avalonia.Android

**命名空间:** `GeneralUpdate.Avalonia.Android` | **主要入口:** `GeneralUpdateBootstrap.CreateDefault()` | **NuGet 包:** `GeneralUpdate.Avalonia.Android`

## 1. 组件简介

### 1.1 组件概述

**GeneralUpdate.Avalonia.Android** 是面向 Avalonia 12+ 应用的 Android 平台自动更新核心库（无 UI，`net10.0-android`），专为 Android APK 的更新场景设计。它提供版本查询与对比、可断点续传的 APK 下载、SHA256 完整性校验、Android Package Installer 拉起，以及**安装结果闭环核对**的全流程编排能力。

Android 平台的更新与桌面应用不同：APK 安装必须通过 Android 系统安装器完成，下载过程可能被用户中断或切换网络，安装前需要检查"安装未知应用"权限，而且安装期间当前进程会被系统结束。该组件封装了这些平台差异，并额外解决了"安装器已拉起 ≠ 安装成功"的确认问题。

**核心能力：**

| 能力 | 说明 |
| --- | --- |
| 服务端版本查询 | 组件按 `UpdateServerOptions` 内部请求服务端，调用方只需提供设备上的当前版本 |
| 版本对比 | 内置 `SystemVersionComparer`，通过 `IVersionComparer` 接口支持自定义版本策略 |
| 一体化流程 | `PrepareUpdateAsync` 在同一个操作锁内完成查询 → 预检查 → 下载 → 校验 |
| 断点续传下载 | `HttpResumableApkDownloader` 基于 HTTP Range + Sidecar 元数据，中断后可续传 |
| SHA256 完整性校验 | 下载完成后自动计算 SHA256，与服务端声明的哈希比对，并校验文件大小 |
| APK 安装编排 | 通过 `AndroidApkInstaller` 调用系统 Package Installer，自动处理 FileProvider URI |
| 安装结果闭环 | `CheckInstallationAsync` / `ResetInstallationAsync` 依据持久化记录核对本次更新是否真正生效 |
| 更新前回调 | `AddListenerUpdatePrecheck` 在下载前把更新信息交给业务层，可跳过本次更新 |
| HTTP 传输配置 | `HttpDownloadOptions` 支持 SSL/TLS 证书策略、代理、超时、重试和认证 |
| 多协议认证 | HMAC-SHA256、Bearer Token、API Key、HTTP Basic，可全局或单包粒度配置 |
| 事件通知 | 验证、下载进度、完成、安装确认、失败五类事件 |
| 状态快照 | `GetSnapshot()` 随时获取当前更新状态、失败原因和消息 |
| UI 线程调度 | `IUpdateEventDispatcher` 支持把事件调度到 UI 线程（如 Avalonia 的 `Dispatcher.UIThread`） |
| 操作串行化 | 内部 `SemaphoreSlim(1,1)` 操作门，并发调用自动排队，线程安全 |
| 可替换抽象 | 版本比较、下载、哈希、安装、存储、包源、事件调度、日志全部可替换 |

**解决的业务痛点：**
- Avalonia 应用发布 Android 版本后，需要可靠的内置更新能力，但 Android APK 安装必须经由系统安装器
- 调用方不应重复实现"查询版本接口 + 挑选最新包 + 比较版本"的样板代码
- 下载大 APK 时可能被中断或切换网络，需要断点续传降低流量消耗
- 需要校验下载包的完整性，防止传输损坏或被篡改
- **安装器返回 `Success` 并不代表用户完成了安装**，需要跨进程的安装结果确认机制
- 需要灵活的认证方案对接不同服务端的鉴权要求

:::info Android 更新 ≠ 桌面文件替换
Android 平台不能像桌面端那样直接替换文件。APK 必须通过 Android Package Installer 安装，系统会处理签名验证、权限授予和应用替换。本组件封装了这些平台差异，但 **APK 签名、包名一致、`versionCode` 递增等仍需要你在构建流水线中保证**。组件也不提供静默安装、自动重启或回滚。
:::

**业务使用场景：**
- Avalonia 跨平台应用的 Android APK 自动更新
- 企业内部 Android 应用的分阶段、可确认式更新
- 需要通过 CDN / OSS 分发 APK 包的场景
- 需要对接 GeneralUpdate 示例服务端 `/Upgrade/Verification` 协议或静态 JSON 端点的场景

### 1.2 环境与依赖

| 项目 | 说明 |
| --- | --- |
| **NuGet 包** | `GeneralUpdate.Avalonia.Android`（仓库当前版本 `v0.0.1-beta.10`） |
| **目标框架** | `net10.0-android`（`SupportedOSPlatformVersion` = 26.0） |
| **.NET SDK** | 10.0+ |
| **Avalonia** | 12+ |
| **依赖包** | `Xamarin.AndroidX.Core`（FileProvider 支持） |
| **兼容性** | Android 8.0 (API 26) 及以上版本 |

---

## 2. 组件功能列表

| 功能名称 | 功能描述 | 类型 | 是否必填 | 备注限制 |
| --- | --- | --- | --- | --- |
| 服务端版本查询 | 按 `UpdateServerOptions` 请求服务端，选中最新可用 APK | 基础 | 必选 | `ValidateAsync(currentVersion)`；也可注入 `IUpdatePackageSource` 替代 |
| 版本对比检查 | 比对当前版本与目标版本，判断是否有更新可用 | 基础 | 必选 | 支持自定义 `IVersionComparer` |
| 一体化更新准备 | 一次调用完成查询、预检查、下载与校验 | 拓展 | 可选 | `PrepareUpdateAsync(currentVersion)` |
| APK 断点续传下载 | 从服务端下载 APK 包，支持 HTTP Range 断点续传 | 基础 | 必选 | `DownloadAndVerifyAsync(packageInfo)` |
| SHA256 校验 | 下载完成后自动计算 SHA256 与服务端哈希比对 | 基础 | 自动 | 通过 `IHashValidator` 实现 |
| 文件大小校验 | `FileSize > 0` 时校验实际文件长度 | 基础 | 自动 | 不匹配返回 `FileIoError` 并删除文件 |
| APK 安装触发 | 调用 Android Package Installer 安装 APK | 基础 | 必选 | `LaunchInstallerAsync(packageInfo, apkFilePath)` |
| 安装结果确认 | 依据持久化安装记录核对本机实际版本 | 基础 | 推荐 | `CheckInstallationAsync(currentVersion)` |
| 安装记录重置 | 显式清除升级记录（含损坏数据） | 拓展 | 可选 | `ResetInstallationAsync()` |
| 更新前回调 | 下载前拦截更新，可跳过本次更新 | 拓展 | 可选 | `AddListenerUpdatePrecheck` |
| HTTP 传输配置 | SSL 证书校验、代理、超时、重试、认证 | 拓展 | 可选 | `HttpDownloadOptions` |
| 多协议认证 | HMAC-SHA256 / Bearer / API Key / Basic | 拓展 | 可选 | 全局或单包粒度配置 |
| 进度通知 | 下载进度（速度、字节数、百分比、状态描述） | 基础 | 可选 | `AddListenerDownloadProgressChanged` |
| 完成通知 | 下载校验完成 / 安装器拉起完成 | 基础 | 可选 | `AddListenerUpdateCompleted` |
| 安装确认通知 | 首次持久化确认安装成功时触发 | 基础 | 可选 | `AddListenerInstallationConfirmed` |
| 失败通知 | 失败原因、异常信息、失败包信息 | 基础 | 可选 | `AddListenerUpdateFailed` |
| 验证通知 | 发现可用更新时触发 | 基础 | 可选 | `AddListenerValidate` |
| 状态快照 | 随时获取当前更新状态 | 拓展 | 可选 | `GetSnapshot()` |
| UI 线程调度 | 自定义事件调度策略 | 拓展 | 可选 | 实现 `IUpdateEventDispatcher` |
| 自定义包源 | 自定义版本查询协议 | 拓展 | 可选 | 实现 `IUpdatePackageSource` |
| 自定义下载器 | 自定义下载实现 | 拓展 | 可选 | 实现 `IUpdateDownloader` |
| 自定义哈希验证 | 自定义哈希算法 | 拓展 | 可选 | 实现 `IHashValidator` |
| 自定义版本比较 | 自定义版本对比逻辑 | 拓展 | 可选 | 实现 `IVersionComparer` |
| 自定义 APK 安装器 | 自定义安装实现 | 拓展 | 可选 | 实现 `IApkInstaller` |
| 自定义文件存储 | 自定义文件读写实现 | 拓展 | 可选 | 实现 `IFileStorage` |
| 自定义安装记录存储 | 自定义升级记录持久化 | 拓展 | 可选 | 实现 `IInstallationStore` |
| 自定义 SSL 策略 | 自定义 HTTPS 证书校验 | 拓展 | 可选 | 实现 `ISslValidationPolicy` |
| 自定义 HTTP 认证 | 自定义 HTTP 请求认证 | 拓展 | 可选 | 实现 `IHttpAuthProvider` |

---

## 3. API 配置说明

### 3.1 配置字段（属性 Props）

**AndroidUpdateOptions：**

| 字段名 | 数据类型 | 默认值 | 是否必填 | 说明 |
| --- | --- | --- | --- | --- |
| `UpdateServer` | `UpdateServerOptions?` | `null` | 条件必填 | `ValidateAsync` 查询的服务端配置；为 `null` 且未注入自定义包源时，验证以 `InvalidMetadata` 失败 |
| `DownloadDirectoryPath` | `string` | `""`（自动使用 `<CacheDir>/update` 或 `TempPath/update`） | 可选 | 下载文件存放目录，为空时自动选择 |
| `InstallationStateFilePath` | `string` | `""`（自动使用 `<FilesDir>/update/installation.json`） | 可选 | 安装记录持久化路径。**不要放在下载缓存目录**，Android 可能清理缓存 |
| `TemporaryFileExtension` | `string` | `".part"` | 可选 | 临时下载文件扩展名 |
| `SidecarExtension` | `string` | `".json"` | 可选 | 断点续传元数据文件扩展名 |
| `FileProviderAuthority` | `string` | `""` | 条件必填 | Android FileProvider authority，必须与 AndroidManifest 一致；调用安装器时为空返回 `InvalidMetadata` |
| `AllowInsecureHttpDownloads` | `bool` | `false` | 可选 | 允许以明文 HTTP 下载 APK，仅限可信开发环境；即使开启，全局认证也不会通过 HTTP 发送 |
| `Language` | `UpdateLanguage` | `English` | 可选 | 内建提示消息语言（`English` / `Chinese`） |
| `DownloadBufferSize` | `int` | `65536`（64 KB） | 可选 | 下载缓冲区大小（字节），必须大于 0 |
| `SpeedSmoothingWindowSeconds` | `int` | `4` | 可选 | 下载速度平滑窗口（秒） |

**UpdateServerOptions：**

| 字段名 | 数据类型 | 默认值 | 是否必填 | 说明 |
| --- | --- | --- | --- | --- |
| `RequestUrl` | `string` | — | **是** | 绝对 HTTP(S) 地址。默认协议下是版本验证端点（如 `https://example.com/Upgrade/Verification`） |
| `AppKey` | `string` | `""` | 可选 | 验证请求体中的应用标识（仅请求字段，不启用 HMAC 签名） |
| `AppType` | `int` | `1` | 可选 | 验证请求体中的应用类型 |
| `Platform` | `int` | `0` | 条件必填 | 服务端配置的 Android 平台编号，**必须与实际部署一致** |
| `ProductId` | `string` | `""` | 可选 | 验证请求体中的产品标识 |
| `UseJsonEndpoint` | `bool` | `false` | 可选 | 改为 GET 单个 `UpdatePackageInfo` JSON，而不是 POST 验证协议 |

**HttpDownloadOptions：**

| 字段名 | 数据类型 | 默认值 | 是否必填 | 说明 |
| --- | --- | --- | --- | --- |
| `SslValidationPolicy` | `ISslValidationPolicy?` | `null` | 可选 | 自定义 SSL 证书验证策略；`null` 使用系统默认。使用外部 `HttpClient` 时必须配置在其 handler 上 |
| `RequestTimeout` | `TimeSpan` | `30s` | 可选 | 验证请求与下载 HEAD 探测的超时 |
| `DownloadTimeout` | `TimeSpan` | `10min` | 可选 | 整个下载操作（含重试等待）的超时 |
| `Proxy` | `IWebProxy?` | `null` | 可选 | HTTP 代理，需同时设置 `UseProxy = true` |
| `UseProxy` | `bool` | `false` | 可选 | 是否启用配置的代理 |
| `MaxRetryAttempts` | `int` | `3` | 可选 | 瞬态 HEAD/GET/响应流失败的最大下载尝试次数（3 = 首次 1 次 + 重试 2 次）；不作用于元数据查询 |
| `RetryBaseDelay` | `TimeSpan` | `1s` | 可选 | 指数退避基本延迟：`min(30s, baseDelay * 2^attempt)` |
| `AuthProvider` | `IHttpAuthProvider?` | `null` | 可选 | 全局认证提供器；仅发送给与验证端点同源的 HTTPS 下载地址，单包认证优先级更高 |

**UpdatePackageInfo：**

| 字段名 | 数据类型 | 默认值 | 是否必填 | 说明 |
| --- | --- | --- | --- | --- |
| `Version` | `string` | — | **是** | 目标版本号 |
| `DownloadUrl` | `string` | — | **是** | APK 下载地址（默认要求 HTTPS） |
| `Sha256` | `string` | — | **是** | 64 位十六进制 SHA256，用于完整性校验 |
| `FileSize` | `long` | `0` | 可选 | APK 文件大小（字节），`0` 表示未知；已知时参与校验 |
| `FileName` | `string?` | `null` | 可选 | 下载后的文件名 |
| `IsForced` | `bool` | `false` | 可选 | 是否强制更新；为 `true` 时不会调用 pre-check 回调 |
| `VersionName` | `string?` | `null` | 可选 | 版本展示名称 |
| `Description` | `string?` | `null` | 可选 | 版本描述 / 更新日志 |
| `PublishTime` | `DateTimeOffset?` | `null` | 可选 | 发布时间 |
| `AuthScheme` | `AuthScheme?` | `null` | 可选 | 单包认证方案，优先于全局配置 |
| `AuthToken` | `string?` | `null` | 可选 | Bearer 或 API Key 认证令牌 |
| `AuthSecretKey` | `string?` | `null` | 可选 | HMAC-SHA256 签名密钥 |
| `BasicUsername` | `string?` | `null` | 可选 | HTTP Basic 用户名 |
| `BasicPassword` | `string?` | `null` | 可选 | HTTP Basic 密码 |

**UpdateState 枚举：**

| 枚举值 | 数值 | 说明 |
| --- | --- | --- |
| `None` | 0 | 初始状态 / 无安装记录 |
| `Checking` | 1 | 正在检查更新 |
| `UpdateAvailable` | 2 | 发现可用更新 |
| `Downloading` | 3 | 正在下载 |
| `Verifying` | 4 | 正在校验 |
| `ReadyToInstall` | 5 | 已下载并校验完成，可安装 |
| `Installing` | 6 | 安装器已拉起 |
| `Completed` | 7 | 流程完成（无更新或被 pre-check 跳过也为此状态） |
| `Failed` | 8 | 更新失败 |
| `Canceled` | 9 | 已取消 |
| `InstallationPending` | 10 | 有安装记录但尚未确认安装成功 |
| `Installed` | 11 | 本机版本已达到或超过安装目标 |

**UpdateFailureReason 枚举：**

| 枚举值 | 数值 | 说明 |
| --- | --- | --- |
| `None` | 0 | 无失败 |
| `NetworkError` | 1 | 网络错误或超时 |
| `Canceled` | 2 | 主动取消 |
| `InvalidMetadata` | 3 | 元数据无效（缺少 URL/SHA256、未配置包源或 FileProvider authority 等） |
| `FileIoError` | 4 | 文件读写错误（含文件大小不匹配、安装记录写入失败） |
| `HashMismatch` | 5 | SHA256 校验失败 |
| `ServerDoesNotSupportRange` | 6 | 服务端不支持 Range 续传 |
| `InstallPermissionDenied` | 7 | 缺少"安装未知应用"权限 |
| `InstallLaunchFailed` | 8 | 安装器 Intent 拉起失败 |
| `VersionComparisonFailed` | 9 | 版本比较失败 |
| `Unknown` | 10 | 未知原因 |

**AuthScheme 枚举：**

| 枚举值 | 说明 |
| --- | --- |
| `Hmac` | HMAC-SHA256 签名认证（`X-Update-Timestamp` + `X-Update-Signature`） |
| `Bearer` | Bearer Token 认证 |
| `ApiKey` | API Key 认证（默认 header `X-Api-Key`） |
| `Basic` | HTTP Basic 认证 |

**UpdateLanguage 枚举：**

| 枚举值 | 说明 |
| --- | --- |
| `English` | 内建提示消息使用英文（默认） |
| `Chinese` | 内建提示消息使用简体中文 |

### 3.2 结果模型继承层次

```
UpdateOperationResult（基类）
├── Success, State, FailureReason, Message, PackageInfo, FilePath, Exception
├── UpdateCheckResult        → + UpdateFound, CurrentVersion, TargetVersion
├── UpdatePreparationResult  → + UpdateFound, IsReadyToInstall
├── InstallationCheckResult  → + CurrentVersion, Record, IsInstalled, HasPendingInstallation
├── DownloadResult
├── HashValidationResult     → + ActualSha256, ExpectedSha256
└── InstallResult
```

| 模型 | 关键成员 | 说明 |
| --- | --- | --- |
| `UpdateCheckResult` | `UpdateFound` / `CurrentVersion` / `TargetVersion` | 版本校验结果，`PackageInfo` 为服务端返回的包元数据 |
| `UpdatePreparationResult` | `IsReadyToInstall` | 一体化流程结果，仅当 `Success && State == ReadyToInstall && PackageInfo != null && FilePath != null` 时为 `true` |
| `InstallationCheckResult` | `Record` / `IsInstalled` / `HasPendingInstallation` | 安装结果核对结果 |
| `InstallationRecord` | `TargetVersion` / `RequestedAt` / `InstalledVersion` / `ConfirmedAt` | 持久化的安装记录，不含下载凭据 |

### 3.3 实例方法

**GeneralUpdateBootstrap（静态工厂）：**

| 方法名 | 入参明细 | 使用场景 | 注意事项 |
| --- | --- | --- | --- |
| `CreateDefault(options, ...)` | `options` — 更新选项；可选参数：`contextProvider`, `activityProvider`, `httpClient`, `versionComparer`, `eventDispatcher`, `logger`, `httpOptions`, `packageSource`, `installationStore` | 创建默认的 Android 更新引导实例 | 所有可选参数为 `null` 时使用内置默认实现；`DownloadBufferSize <= 0` 抛 `ArgumentOutOfRangeException` |

默认注入链：

| 抽象接口 | 默认实现 |
| --- | --- |
| `IAndroidContextProvider` | `DefaultAndroidContextProvider` |
| `IAndroidActivityProvider` | `NullAndroidActivityProvider` |
| `IUpdateLogger` | `NoOpUpdateLogger` |
| `IFileStorage` | `PhysicalFileStorage` |
| `IUpdateDownloader` | `HttpResumableApkDownloader` |
| `IHashValidator` | `Sha256HashValidator` |
| `IApkInstaller` | `AndroidApkInstaller` |
| `IVersionComparer` | `SystemVersionComparer` |
| `IUpdatePackageSource` | `HttpUpdatePackageClient` |
| `IInstallationStore` | `JsonFileInstallationStore` |
| `IUpdateEventDispatcher` | `ImmediateEventDispatcher` |

**IAndroidBootstrap：**

| 方法名 | 入参明细 | 返回类型 | 使用场景 | 注意事项 |
| --- | --- | --- | --- | --- |
| `PrepareUpdateAsync(currentVersion, ct)` | `currentVersion` — 设备当前版本；`ct` — 取消令牌 | `UpdatePreparationResult` | 一体化完成查询、比较、预检查、下载与校验 | 在一个操作锁内完成；`Success` 为 `true` 但 `IsReadyToInstall` 为 `false` 表示无更新或被跳过；**不会**打开安装器或权限页面 |
| `ValidateAsync(currentVersion, ct)` | `currentVersion` — 设备当前版本；`ct` — 取消令牌 | `UpdateCheckResult` | 只检查版本，或需要分阶段控制 | 包元数据由 `AndroidUpdateOptions.UpdateServer` 或注入的包源提供，调用方不再构造 `UpdatePackageInfo`；返回的 `PackageInfo` 可直接传给后续方法 |
| `DownloadAndVerifyAsync(packageInfo, ct)` | `packageInfo` — 更新包信息；`ct` — 取消令牌 | `UpdateOperationResult` | 下载 APK 并校验大小与 SHA256 | 成功时 `State = ReadyToInstall`、`FilePath` 为 APK 路径 |
| `LaunchInstallerAsync(packageInfo, apkFilePath, ct)` | `packageInfo` — 更新包信息；`apkFilePath` — APK 路径；`ct` — 取消令牌 | `InstallResult` | 写入安装记录并拉起系统安装器 | 先原子写入安装记录再拉起；写入失败返回 `FileIoError` 且不拉起安装器；`Success = true` 只表示安装器已拉起 |
| `CheckInstallationAsync(currentVersion, ct)` | `currentVersion` — 从 PackageManager 读取的实际版本；`ct` — 取消令牌 | `InstallationCheckResult` | 启动时或从安装器返回时核对安装结果 | 不依赖网络；无记录返回 `Success && State == None`；记录损坏明确报错，不伪装成"无记录" |
| `ResetInstallationAsync(ct)` | `ct` — 取消令牌 | `UpdateOperationResult` | 显式清除升级记录（含损坏数据） | 仅清除记录，不修改已安装应用、下载文件或服务端配置 |
| `AddListenerUpdatePrecheck(func)` | `func` — `Func<UpdateInfoEventArgs, bool>` | `IAndroidBootstrap` | 注册更新前回调 | 返回 `true` 跳过、`false` 继续；强制更新不调用；可链式注册 |
| `GetSnapshot()` | 无 | `UpdateStateSnapshot` | 获取当前状态快照 | 返回 `(State, FailureReason, Message)` |
| `Dispose()` | 无 | `void` | 释放下载器 / 包源等资源 | 关闭前先取消并等待操作完成；提前 `Dispose` 会拒绝新调用与排队调用，并延迟到当前操作退出后释放资源 |

### 3.4 回调事件

| 事件名称 | 回调参数 | 触发时机 | 使用说明 |
| --- | --- | --- | --- |
| `AddListenerValidate` | `ValidateEventArgs` — `PackageInfo`, `CurrentVersion` | 版本校验发现可用更新时 | 可用于 UI 展示新版本信息；被 pre-check 跳过时不触发 |
| `AddListenerDownloadProgressChanged` | `DownloadProgressChangedEventArgs` — `ProgressPercentage`, `DownloadSpeedBytesPerSecond`, `DownloadedBytes`, `RemainingBytes`, `TotalBytes`, `PackageInfo`, `StatusDescription` | 下载进度更新时 | 可用于进度条展示 |
| `AddListenerUpdateCompleted` | `UpdateCompletedEventArgs` — `Result`（`UpdateOperationResult`） | 下载校验完成（`ReadyToInstall`）或安装器拉起完成（`Installing`）时 | **不代表安装成功**，仅表示阶段完成 |
| `AddListenerInstallationConfirmed` | `InstallationConfirmedEventArgs` — `Result`（`InstallationCheckResult`） | `CheckInstallationAsync` 首次持久化确认目标版本已安装时 | 只触发一次，重复核对或进程重启不会重复触发；不是可靠消息队列 |
| `AddListenerUpdateFailed` | `UpdateFailedEventArgs` — `Result`（`UpdateOperationResult`） | 任何阶段失败时 | 包含失败原因和异常信息 |

---

## 4. 扩展示例（高阶用法）

### 4.1 组件可扩展能力总览

所有服务均可通过 `CreateDefault` 的可选参数替换：

| 接口 | 默认实现 | 说明 |
| --- | --- | --- |
| `IVersionComparer` | `SystemVersionComparer` | 版本对比策略 |
| `IUpdatePackageSource` | `HttpUpdatePackageClient` | 更新包元数据来源（协议 / 接口出参） |
| `IUpdateDownloader` | `HttpResumableApkDownloader` | APK 下载实现 |
| `IHashValidator` | `Sha256HashValidator` | SHA256 哈希校验 |
| `IApkInstaller` | `AndroidApkInstaller` | APK 安装触发 |
| `IFileStorage` | `PhysicalFileStorage` | 文件存储操作 |
| `IInstallationStore` | `JsonFileInstallationStore` | 安装记录持久化 |
| `IUpdateEventDispatcher` | `ImmediateEventDispatcher` | 事件调度器（直接调用） |
| `IUpdateLogger` | `NoOpUpdateLogger` | 日志记录器 |
| `IAndroidContextProvider` | `DefaultAndroidContextProvider` | Android Context 提供器 |
| `IAndroidActivityProvider` | `NullAndroidActivityProvider` | Android Activity 提供器 |
| `ISslValidationPolicy` | 通过 `HttpDownloadOptions` 配置 | SSL 证书验证策略 |
| `IHttpAuthProvider` | 通过 `HttpDownloadOptions` 或单包配置 | HTTP 认证提供器 |

需要替换全部环节时，可直接使用 `AndroidBootstrap` 的依赖注入构造函数：

```csharp
new AndroidBootstrap(
    versionComparer, downloader, hashValidator, apkInstaller, fileStorage,
    packageSource, installationStore, eventDispatcher?, logger?);
```

Bootstrap 负责释放实现了 `IDisposable` 的 downloader 与 package source；其余注入依赖（包括 store）由宿主自行管理，不要跨升级器共享由 Bootstrap 拥有的服务实例。

### 4.2 分场景示例

#### 场景 1：自定义版本比较策略

【场景说明】应用使用 `year.month.day.build` 格式版本号，默认的 `System.Version` 解析会失败，需要自定义版本比较器。

【示例代码】

```csharp
using GeneralUpdate.Avalonia.Android.Abstractions;

public sealed class CustomDateVersionComparer : IVersionComparer
{
    public bool TryCompare(string currentVersion, string targetVersion,
        out int compareResult, out string? errorMessage)
    {
        try
        {
            var current = ParseVersion(currentVersion);
            var target = ParseVersion(targetVersion);
            compareResult = target.CompareTo(current);
            errorMessage = null;
            return true;
        }
        catch (Exception ex)
        {
            compareResult = 0;
            errorMessage = ex.Message;
            return false;
        }
    }

    private static DateTime ParseVersion(string version) =>
        DateTime.ParseExact(version, "yyyy.MM.dd.fff", null);
}

// 使用
var bootstrap = GeneralUpdateBootstrap.CreateDefault(
    options,
    versionComparer: new CustomDateVersionComparer());
```

【效果&注意事项】
- 返回的 `compareResult` 遵循 `target.CompareTo(current)` 语义：正数表示有新版本
- 实现失败时返回 `false` 和错误信息，校验以 `VersionComparisonFailed` 失败

#### 场景 2：自定义事件调度器（Avalonia UI 线程调度）

【场景说明】在 Avalonia 应用中，事件回调默认在后台线程触发，更新 UI 需要调度到 UI 线程。

```csharp
using GeneralUpdate.Avalonia.Android.Abstractions;

public sealed class AvaloniaEventDispatcher : IUpdateEventDispatcher
{
    public void Dispatch(Action callback)
    {
        if (Avalonia.Threading.Dispatcher.UIThread.CheckAccess())
        {
            callback();
        }
        else
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(callback);
        }
    }
}

// 使用
var bootstrap = GeneralUpdateBootstrap.CreateDefault(
    options,
    eventDispatcher: new AvaloniaEventDispatcher());
```

#### 场景 3：自定义包源（自有版本接口）

【场景说明】企业已有自己的版本接口，不想改造为 GeneralUpdate 协议，也不想让服务端额外提供静态 JSON 端点。

```csharp
using GeneralUpdate.Avalonia.Android.Abstractions;
using GeneralUpdate.Avalonia.Android.Models;

public sealed class EnterprisePackageSource : IUpdatePackageSource
{
    public async Task<UpdatePackageInfo?> GetLatestAsync(
        string currentVersion, CancellationToken cancellationToken = default)
    {
        // 请求自有接口。没有可用包时返回 null；
        // 网络或协议错误必须抛异常，不要静默返回 null。
        return await Task.FromResult<UpdatePackageInfo?>(null);
    }
}

// 使用
var bootstrap = GeneralUpdateBootstrap.CreateDefault(
    options,
    packageSource: new EnterprisePackageSource());
```

#### 场景 4：自定义安装记录存储

【场景说明】需要把安装记录存到数据库或系统偏好设置，而不是 JSON 文件。

```csharp
using GeneralUpdate.Avalonia.Android.Abstractions;
using GeneralUpdate.Avalonia.Android.Models;

public sealed class DatabaseInstallationStore : IInstallationStore
{
    public Task<InstallationRecord?> LoadAsync(CancellationToken ct = default) { /* ... */ }
    public Task SaveAsync(InstallationRecord record, CancellationToken ct = default) { /* 原子写入 */ }
    public Task ClearAsync(CancellationToken ct = default) { /* ... */ }
}

var bootstrap = GeneralUpdateBootstrap.CreateDefault(
    options,
    installationStore: new DatabaseInstallationStore());
```

#### 场景 5：自定义下载器（私有文件服务器）

【场景说明】企业内部使用私有文件服务器，需要在下载时注入自定义认证头。

```csharp
using GeneralUpdate.Avalonia.Android.Abstractions;
using GeneralUpdate.Avalonia.Android.Models;

public sealed class EnterpriseDownloader : IUpdateDownloader
{
    public async Task<DownloadResult> DownloadAsync(
        UpdatePackageInfo packageInfo,
        Action<DownloadProgressInfo>? progressCallback,
        CancellationToken cancellationToken = default)
    {
        // 自行实现下载与进度上报，返回 DownloadResult。
        // 与默认下载器不同，自定义实现需自行保证 HTTPS、续传与进度语义。
        return await Task.FromResult(new DownloadResult
        {
            Success = false,
            State = UpdateState.Failed,
            FailureReason = UpdateFailureReason.NetworkError
        });
    }
}
```

---

## 5. 常规使用示例

### 5.1 快速入门示例（最简 demo）

```csharp
using GeneralUpdate.Avalonia.Android;
using GeneralUpdate.Avalonia.Android.Models;

var options = new AndroidUpdateOptions
{
    DownloadDirectoryPath = Path.Combine(
        Android.App.Application.Context.CacheDir!.AbsolutePath!, "update"),
    FileProviderAuthority = "com.example.app.generalupdate.fileprovider",

    // ValidateAsync 据此在组件内部请求服务端，调用方只需要提供当前版本
    UpdateServer = new UpdateServerOptions
    {
        RequestUrl = "https://example.com/Upgrade/Verification",
        AppKey = "your-app-key",
        Platform = androidPlatformId,   // 服务端配置的 Android 平台编号
        ProductId = "your-product-id"
    }
};

using var bootstrap = GeneralUpdateBootstrap.CreateDefault(options);
bootstrap.AddListenerUpdateFailed += (_, args) => Console.Error.WriteLine(args.Result.Message);

// 设备上的当前版本
var context = global::Android.App.Application.Context;
var currentVersion = context.PackageManager?.GetPackageInfo(
    context.PackageName!, global::Android.Content.PM.PackageInfoFlags.Activities)?.VersionName
    ?? throw new InvalidOperationException("无法读取本机版本。");

var prepared = await bootstrap.PrepareUpdateAsync(currentVersion, CancellationToken.None);
if (prepared.IsReadyToInstall && prepared.PackageInfo is { } package && prepared.FilePath is { } path)
{
    await bootstrap.LaunchInstallerAsync(package, path, CancellationToken.None);
}
```

### 5.2 基础参数组合示例（分阶段 API）

```csharp
using GeneralUpdate.Avalonia.Android;
using GeneralUpdate.Avalonia.Android.Models;
using GeneralUpdate.Avalonia.Android.Services;

var options = new AndroidUpdateOptions
{
    DownloadDirectoryPath = Path.Combine(
        Android.App.Application.Context.CacheDir!.AbsolutePath!, "myapp-updates"),
    FileProviderAuthority = "com.myapp.fileprovider",
    TemporaryFileExtension = ".downloading",
    Language = UpdateLanguage.Chinese
};

// 带 HTTP 配置的启动
var httpOptions = new HttpDownloadOptions
{
    RequestTimeout = TimeSpan.FromSeconds(60),
    DownloadTimeout = TimeSpan.FromMinutes(30),
    MaxRetryAttempts = 5,
    RetryBaseDelay = TimeSpan.FromSeconds(2),
    // 开发环境使用自签名证书
    SslValidationPolicy = new AllowAllSslValidationPolicy()
};

var bootstrap = GeneralUpdateBootstrap.CreateDefault(
    options,
    httpOptions: httpOptions);

// 事件监听
bootstrap.AddListenerDownloadProgressChanged += (_, e) =>
{
    Console.WriteLine(
        $"Download: {e.ProgressPercentage:F1}% " +
        $"({FormatSize(e.DownloadedBytes)}/{FormatSize(e.TotalBytes)}) " +
        $"{FormatSpeed(e.DownloadSpeedBytesPerSecond)} {e.StatusDescription}");
};

bootstrap.AddListenerUpdateFailed += (_, e) =>
    Console.WriteLine($"Failed: {e.Result.Message} (reason: {e.Result.FailureReason})");

bootstrap.AddListenerUpdateCompleted += (_, e) =>
    Console.WriteLine($"Completed: {e.Result.State}");

// 分阶段执行：版本检查 → 下载校验 → 拉起安装器
var check = await bootstrap.ValidateAsync(currentVersion);
if (!check.UpdateFound || check.PackageInfo is null) return;

var download = await bootstrap.DownloadAndVerifyAsync(check.PackageInfo);
if (!download.Success || download.FilePath is null) return;

await bootstrap.LaunchInstallerAsync(check.PackageInfo, download.FilePath);

static string FormatSize(long bytes) => bytes switch
{
    < 1024 => $"{bytes} B",
    < 1048576 => $"{bytes / 1024.0:F1} KB",
    _ => $"{bytes / 1048576.0:F1} MB"
};

static string FormatSpeed(double bytesPerSecond) =>
    $"{bytesPerSecond / 1048576.0:F1} MB/s";
```

### 5.3 真实业务落地示例

完整更新工作流，包含 HTTP 认证、更新前回调、权限引导和安装结果确认：

```csharp
using GeneralUpdate.Avalonia.Android;
using GeneralUpdate.Avalonia.Android.Abstractions;
using GeneralUpdate.Avalonia.Android.Models;
using GeneralUpdate.Avalonia.Android.Services;

public sealed class AppUpdateService
{
    private IAndroidBootstrap? _bootstrap;
    private bool _disposed;

    public event EventHandler<double>? ProgressChanged;
    public event EventHandler<string>? StatusChanged;

    public void Initialize(string fileProviderAuthority, string serverToken)
    {
        var httpOptions = new HttpDownloadOptions
        {
            MaxRetryAttempts = 3,
            RetryBaseDelay = TimeSpan.FromSeconds(1),
            AuthProvider = new BearerTokenAuthProvider(serverToken)
        };

        var options = new AndroidUpdateOptions
        {
            FileProviderAuthority = fileProviderAuthority,
            UpdateServer = new UpdateServerOptions
            {
                RequestUrl = "https://update.example.com/Upgrade/Verification",
                AppKey = "your-app-key",
                Platform = androidPlatformId,
                ProductId = "your-product-id"
            }
        };

        _bootstrap = GeneralUpdateBootstrap.CreateDefault(options, httpOptions: httpOptions);

        // 下载前的业务拦截：仅 Wi-Fi 时拉取非强制更新
        _bootstrap.AddListenerUpdatePrecheck(args =>
        {
            if (!IsWifiConnected())
            {
                StatusChanged?.Invoke(this, $"等待 Wi-Fi 后再更新到 {args.PackageInfo.Version}");
                return true;   // true = 跳过本次更新
            }

            return false;      // false = 继续下载
        });

        // 安装结果确认：只在首次持久化确认时触发
        _bootstrap.AddListenerInstallationConfirmed += (_, args) =>
        {
            StatusChanged?.Invoke(this,
                $"已升级到 {args.Result.Record?.TargetVersion}（本机 {args.Result.CurrentVersion}）");
        };

        _bootstrap.AddListenerDownloadProgressChanged += (_, e) =>
            ProgressChanged?.Invoke(this, e.ProgressPercentage);

        _bootstrap.AddListenerUpdateFailed += (_, e) =>
            StatusChanged?.Invoke(this, $"失败：{e.Result.Message}（{e.Result.FailureReason}）");
    }

    /// <summary>启动时或从安装器返回时调用，离线核对上次安装结果。</summary>
    public async Task<bool> ConfirmInstallationAsync(CancellationToken ct = default)
    {
        if (_bootstrap is null) throw new InvalidOperationException("Not initialized.");

        var currentVersion = ReadInstalledVersion();
        var installation = await _bootstrap.CheckInstallationAsync(currentVersion, ct);
        if (!installation.Success)
        {
            StatusChanged?.Invoke(this, $"安装记录核对失败：{installation.Message}");
            return false;
        }

        if (installation.HasPendingInstallation)
        {
            StatusChanged?.Invoke(this, "上次安装尚未生效，可重新检查更新并重试。");
            return false;
        }

        return installation.IsInstalled;
    }

    public async Task<bool> UpdateAsync(string currentVersion, CancellationToken ct = default)
    {
        if (_bootstrap is null) throw new InvalidOperationException("Not initialized.");

        StatusChanged?.Invoke(this, "Checking for updates...");
        var prepared = await _bootstrap.PrepareUpdateAsync(currentVersion, ct);

        if (!prepared.Success)
        {
            StatusChanged?.Invoke(this, $"Update failed: {prepared.Message}");
            return false;
        }

        if (!prepared.UpdateFound)
        {
            StatusChanged?.Invoke(this, "Already up to date.");
            return true;
        }

        if (!prepared.IsReadyToInstall || prepared.PackageInfo is not { } package ||
            prepared.FilePath is not { } path)
        {
            StatusChanged?.Invoke(this, "Update was skipped.");
            return false;
        }

        // 安装权限检查（Android 8.0+）
        if (!HasInstallPermission())
        {
            OpenUnknownSourcesSettings();
            StatusChanged?.Invoke(this, "Please allow install from unknown apps.");
            return false;
        }

        StatusChanged?.Invoke(this, "Installing...");
        var install = await _bootstrap.LaunchInstallerAsync(package, path, ct);
        if (install.Success)
        {
            StatusChanged?.Invoke(this, "Installer launched.");
            return true;
        }

        StatusChanged?.Invoke(this, $"Install failed: {install.Message}");
        return false;
    }

    private static string ReadInstalledVersion()
    {
        var context = Android.App.Application.Context;
        return context.PackageManager?.GetPackageInfo(
            context.PackageName!, Android.Content.PM.PackageInfoFlags.Activities)?.VersionName
            ?? throw new InvalidOperationException("无法读取本机版本。");
    }

    private static bool HasInstallPermission()
    {
        if (Android.OS.Build.VERSION.SdkInt < Android.OS.BuildVersionCodes.O) return true;
        var pm = Android.App.Application.Context.PackageManager;
        return pm is not null && pm.CanRequestPackageInstalls();
    }

    private static void OpenUnknownSourcesSettings()
    {
        var ctx = Android.App.Application.Context;
        ctx.StartActivity(new Android.Content.Intent(
                Android.Provider.Settings.ActionManageUnknownAppSources,
                Android.Net.Uri.Parse("package:" + ctx.PackageName))
            .AddFlags(Android.Content.ActivityFlags.NewTask));
    }

    private static bool IsWifiConnected() => true; // 由宿主实现

    public void Dispose()
    {
        if (_disposed) return;
        _bootstrap?.Dispose();
        _disposed = true;
    }
}
```

---

## 6. 全局配置

### 服务端版本校验协议

`ValidateAsync(currentVersion, ct)` 只接收设备上已安装的版本号，组件按 `AndroidUpdateOptions.UpdateServer` 的配置请求服务端、选出最新完整 APK 并比较版本。

**默认协议**：`POST` 到 `RequestUrl`，请求体为 `version / appKey / appType / platform / productId`，响应形如：

```json
{
  "code": 200,
  "body": [
    {
      "version": "2.3.0",
      "name": "2.3.0",
      "url": "https://example.com/app-release.apk",
      "hash": "0123456789abcdef...（64 位十六进制 SHA-256）",
      "size": 52428800,
      "updateLog": "更新说明",
      "releaseDate": "2026-06-01T00:00:00Z",
      "isForcibly": false,
      "packageType": 2,
      "format": "apk",
      "authScheme": "bearer",
      "authToken": "per-package-token"
    }
  ]
}
```

客户端映射关系：`version / url / hash / size / name / updateLog / releaseDate / isForcibly / authScheme / authToken`，并按版本挑选最新的非冻结完整 APK：

- `isFreeze` 为 `true` 的条目跳过（冻结版本不推送）
- `packageType` 仅接受 `2`、`0` 或省略
- `format` 为 `apk` / `.apk`；省略时要求 URL 路径以 `.apk` 结尾
- ZIP、差分包、驱动包不会交给 Android 安装器
- `body` 为空数组、没有符合条件条目或 HTTP 204 均表示"无更新"

:::warning 平台编号不要写死
`Platform` 必须与你的服务端实际配置的 Android 平台编号一致，示例中的取值仅作占位。协议不同时，请改用静态 JSON 端点或自定义 `IUpdatePackageSource`。
:::

**静态 JSON 端点**：设置 `UpdateServer.UseJsonEndpoint = true`，`RequestUrl` 指向 JSON 地址，组件用 `GET` 读取一个 `UpdatePackageInfo`（字段名不区分大小写）：

```json
{
  "version": "2.3.0",
  "downloadUrl": "https://example.com/app-release.apk",
  "sha256": "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef",
  "description": "更新说明",
  "isForced": false
}
```

- `sha256` 必须是 APK 实际的 64 位十六进制 SHA-256，不能使用 MD5
- `fileSize` 可省略或为 `0`（未知），已知时单位为字节
- HTTP 204 或 JSON `null` 表示无包
- 请求、协议与元数据错误通过 `UpdateCheckResult.Success = false`、`FailureReason` 与 `AddListenerUpdateFailed` 上报，**不会**触发 pre-check

### 更新前回调（Pre-check Hook）

`AddListenerUpdatePrecheck` 对应 `GeneralUpdate.Core` 的 `AddListenerUpdatePrecheck` / `ClientStrategy.UseUpdatePrecheck`，在 `ValidateAsync` 发现更高版本之后、下载 APK **之前**触发：

```csharp
bootstrap.AddListenerUpdatePrecheck(args =>
{
    // args.PackageInfo    — Version / DownloadUrl / Sha256 / FileSize / IsForced 等
    // args.CurrentVersion — 设备当前版本
    // args.Result         — ValidateAsync 即将返回的 UpdateCheckResult
    // args.IsForced       — 是否强制更新

    if (args.PackageInfo.Version == "1.2.0" && !IsWifiConnected())
    {
        return true;   // 跳过：等 Wi-Fi 后再拉取 1.2.0
    }

    return false;
});
```

返回 `true` 表示跳过本次更新（与 `GeneralUpdate.Core` 的 `CanSkip` 语义一致），跳过后 `ValidateAsync` 返回 `UpdateFound == false`、状态为 `UpdateState.Completed`，且**不会**触发 `AddListenerValidate`，因此常规的 `if (check.UpdateFound) { ... }` 流程不会进入下载。强制更新（`UpdatePackageInfo.IsForced = true`）不会调用该回调。

### 安装结果闭环

`CreateDefault` 默认把最近一次安装目标原子写入 `<FilesDir>/update/installation.json`（而不是可能被清理的 APK 缓存）。可以通过 `InstallationStateFilePath` 指定其他持久化位置；**同一记录文件只应使用一个 bootstrap 实例**。

流程要点：

1. `LaunchInstallerAsync` 在拉起安装器**之前**写入安装记录；写入失败返回 `FileIoError`，不会拉起安装器
2. 安装完成后进程会被系统结束，宿主在下次启动或从安装器返回时读取 PackageManager 的实际版本
3. 调用 `CheckInstallationAsync(currentVersion)` 离线核对，不依赖网络

| 结果 | 含义 |
| --- | --- |
| `Success && State == None` | 没有安装记录，**不表示安装成功** |
| `HasPendingInstallation` | 当前版本低于目标，安装尚未确认；可能取消、失败或仍在进行，可重新检查并重试升级 |
| `IsInstalled` | 本机版本已达到或超过目标，状态为 `Installed`，确认结果已持久化 |
| `!Success` | 本机版本无效或记录读取/写入失败，通过 `AddListenerUpdateFailed` 报错，不伪装成"无记录" |

`AddListenerInstallationConfirmed` 仅在首次持久化确认时触发，重复核对或进程重启不会重复发送；它不是可靠消息队列，界面恢复应以返回的 `InstallationCheckResult` 为准。记录损坏时，应由宿主提示用户后显式调用 `ResetInstallationAsync()` 并检查返回结果。

:::danger 安装确认不是签名校验
本机版本核对不是 APK 签名校验或应用健康检查，也不提供静默安装、自动重启或回滚。生产 APK 必须保持相同包名、兼容签名和递增的 `versionCode`。
:::

### 认证方案

支持四种认证方案，可在 `UpdatePackageInfo` 单包粒度配置：

| 认证方案 | AuthScheme | 所需字段 | 请求头 |
| --- | --- | --- | --- |
| HMAC-SHA256 | `Hmac` | `AuthSecretKey` | `X-Update-Timestamp` + `X-Update-Signature` |
| Bearer Token | `Bearer` | `AuthToken` | `Authorization: Bearer <token>` |
| API Key | `ApiKey` | `AuthToken` | `X-Api-Key: <key>` |
| HTTP Basic | `Basic` | `BasicUsername` + `BasicPassword` | `Authorization: Basic <base64>` |

也可通过 `HttpDownloadOptions.AuthProvider` 设置全局认证提供器（内置 `BearerTokenAuthProvider`、`ApiKeyAuthProvider`、`BasicAuthProvider`、`HmacAuthProvider`，或用 `HttpAuthProviderFactory` 按方案创建）。单包配置优先于全局配置。

:::info 认证的发送范围
全局认证只会发送给**与验证端点同源的 HTTPS 下载地址**；跨源下载或明文 HTTP 下载不会携带全局认证。
:::

### SSL 策略

| 策略 | 类名 | 使用场景 |
| --- | --- | --- |
| 系统默认（推荐） | `null` | 生产环境 |
| 允许所有（仅开发） | `AllowAllSslValidationPolicy` | 自签名证书开发环境 |
| 严格校验 | `StrictSslValidationPolicy` | 需要显式拒绝所有异常的保守场景 |
| 自定义 | 实现 `ISslValidationPolicy` | 私有 CA 或证书固定 |

### 断点续传机制

下载器使用临时文件（`.part`）和元数据文件（`.json`）实现断点续传：

```
<CacheDir>/update/
├── app-v2.0.0.apk.part       # 部分下载的临时文件
├── app-v2.0.0.apk.json       # 续传元数据（URL、SHA256、文件名、期望大小、ETag、LastModified）
└── app-v2.0.0.apk            # 下载完成后的最终文件
```

- 先发 `HEAD` 探测服务端能力（`Accept-Ranges` / `ETag` / `Content-Length` / `Last-Modified`）；HEAD 返回 405/501 时改用 GET
- 中断后重新下载同一 URL 时，读取 sidecar 元数据判断是否可续传：文件名、`ETag`、`LastModified` 任一不一致即丢弃临时文件重新下载
- 支持 Range 时从已下载位置继续（`Range: bytes={existingLength}-`）；服务端返回 `200 OK` 而非 `206` 时自动从头下载
- 下载完成后 `.part` 原子重命名（覆盖）为最终文件名，并删除 sidecar 元数据
- 瞬态 HEAD/GET/响应流错误会重试并续传；元数据查询、永久 HTTP 错误（如 401）和主动取消不重试

### 并发与生命周期

- 所有公开操作都在同一个 `SemaphoreSlim(1,1)` 操作门内串行执行，状态通过 `GetSnapshot()` 线程安全读取
- 关闭页面时先取消并等待当前操作完成，再 `Dispose`；提前 `Dispose` 会拒绝新调用与排队调用，并延迟到当前操作退出后释放资源，不会自动取消当前操作
- 外部传入的 `HttpClient` 始终由宿主管理，组件不会改写它的 `Timeout`；TLS / 代理必须配置在它的 handler 上，同时通过 `httpOptions` 指定 TLS/代理会抛 `ArgumentException`
- 未传客户端时，组件创建并释放一个由查询与下载共享的传输客户端
- 超时报告 `Failed/NetworkError`，主动取消报告 `Canceled`；等待操作锁时取消会抛异常

### Android 接入配置

1. **安装权限（Android 8.0+）**：在 `AndroidManifest.xml` 中声明，缺少权限时 `LaunchInstallerAsync` 返回 `InstallPermissionDenied`：

```xml
<uses-permission android:name="android.permission.REQUEST_INSTALL_PACKAGES" />
```

2. **FileProvider**：`android:authorities` 必须与 `AndroidUpdateOptions.FileProviderAuthority` 完全一致，且 paths 必须覆盖 `DownloadDirectoryPath`（默认 `<CacheDir>/update`）；不一致时返回 `InstallLaunchFailed`：

```xml
<provider
    android:name="androidx.core.content.FileProvider"
    android:authorities="com.example.app.generalupdate.fileprovider"
    android:exported="false"
    android:grantUriPermissions="true">
    <meta-data
        android:name="android.support.FILE_PROVIDER_PATHS"
        android:resource="@xml/generalupdate_file_paths" />
</provider>
```

`Resources/xml/generalupdate_file_paths.xml`：

```xml
<?xml version="1.0" encoding="utf-8"?>
<paths>
    <cache-path name="update_cache" path="update/" />
    <files-path name="update_files" path="update/" />
</paths>
```

3. **当前 Activity**：`CreateDefault` 默认使用 `NullAndroidActivityProvider`，此时安装器通过 `Application.Context` + `FLAG_ACTIVITY_NEW_TASK` 拉起。传入实现 `IAndroidActivityProvider` 的 provider（返回当前 `Activity`）更稳妥：

```csharp
using var bootstrap = GeneralUpdateBootstrap.CreateDefault(options, activityProvider: myActivityProvider);
```

### 平台差异

| 项目 | 说明 |
| --- | --- |
| 最低 API | 26（Android 8.0） |
| 安装权限 | Android 8.0+ 需要 `CanRequestPackageInstalls()` 检查 |
| FileProvider | 必须通过 FileProvider 向系统安装器传递 APK 文件 URI |
| 明文 HTTP | 默认禁止下载，需显式开启 `AllowInsecureHttpDownloads` |
| 进程生命周期 | 安装完成后进程被系统结束，安装结果需在下次启动时核对 |

---

## 相关资源

- [GeneralUpdate.Avalonia 仓库](https://github.com/GeneralLibrary/GeneralUpdate.Avalonia)
- [Android 更新示例](https://github.com/GeneralLibrary/GeneralUpdate-Samples/tree/main/UI/AndroidUpdate)
- [Avalonia.Android 执行流程](Avalonia.Android-flow)
- [Avalonia Android 实战手册](../quickstart/Avalonia%20Android%20cookbook)
- [Semantic Versioning](https://semver.org/)
- [FileProvider 文档](https://developer.android.com/training/secure-file-sharing/setup-sharing)
