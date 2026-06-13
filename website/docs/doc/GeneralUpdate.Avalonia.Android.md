---
sidebar_position: 9
---

# GeneralUpdate.Avalonia.Android

**命名空间:** `GeneralUpdate.Avalonia.Android` | **主要入口:** `GeneralUpdateBootstrap.CreateDefault()` | **NuGet 包:** `GeneralUpdate.Avalonia.Android`

## 1. 组件简介

### 1.1 组件概述

**GeneralUpdate.Avalonia.Android** 是面向 Avalonia 应用的 Android 平台自动更新组件，专为 Android APK 的更新场景设计。它提供版本对比、可断点续传的 APK 下载、SHA256 完整性校验、以及 Android Package Installer 自动启动的全流程编排能力。

Android 平台的更新与桌面应用不同：APK 安装必须通过 Android 系统安装器完成，下载过程可能被用户中断或切换网络，安装前需要检查 "安装未知应用" 权限。该组件封装了这些平台差异，让 Avalonia 开发者只需提供更新包信息即可完成更新。

**核心能力：**

| 能力 | 说明 |
| --- | --- |
| 版本对比 | 内置 `SystemVersionComparer`，通过 `IVersionComparer` 接口支持自定义版本策略 |
| 断点续传下载 | `HttpResumableApkDownloader` 支持 HTTP Range 请求头，中断后可续传 |
| SHA256 完整性校验 | 下载完成后自动计算文件 SHA256，与服务端声明的哈希值比对，防止文件损坏或篡改 |
| APK 安装编排 | 通过 `AndroidApkInstaller` 调用系统 Package Installer，自动处理 FileProvider URI |
| HTTP 传输配置 | `HttpDownloadOptions` 支持 SSL/TLS 证书策略、代理、超时、重试和多种认证方案 |
| 多协议认证 | 支持 HMAC-SHA256、Bearer Token、API Key、HTTP Basic 四种认证方案，支持全局和单包粒度 |
| 事件通知 | 版本发现、下载进度、完成、失败等事件通知 |
| 状态快照 | `GetSnapshot()` 随时获取当前更新状态、失败原因和消息 |
| UI 线程调度 | `IUpdateEventDispatcher` 支持将事件调度到 UI 线程（如 Avalonia 的 `SynchronizationContext`） |
| 渐进式下载 | 临时文件 `.part` 和 Sidecar `.json` 机制，记录已完成字节数，支持进程级断点续传 |

**解决的业务痛点：**
- Avalonia 应用发布 Android 版本后，需要可靠的内置更新能力，但 Android APK 安装必须经由系统安装器
- 下载大 APK 包时可能被用户中断、网络切换，需要断点续传能力降低重复下载流量
- 需要对下载的 APK 进行完整性校验，防止传输损坏或被篡改
- 需要灵活的认证方案对接不同服务端的鉴权要求

:::info Android 更新 ≠ 桌面文件替换
Android 平台不能像桌面端那样直接替换文件。APK 必须通过 Android Package Installer 安装，系统会处理签名验证、权限授予和应用替换。本组件封装了这些平台差异，但 **APK 签名、版本号递增等仍需要你在构建流水线中保证**。
:::

**业务使用场景：**
- Avalonia 跨平台应用的 Android APK 自动更新
- 企业内部 Android 应用的静默或引导式更新
- 需要通过 CDN / OSS 分发 APK 包的场景

### 1.2 环境与依赖

| 项目 | 说明 |
| --- | --- |
| **NuGet 包** | `GeneralUpdate.Avalonia.Android` |
| **目标框架** | `net10.0-android`（最低 Android API 26+） |
| **依赖包** | `Xamarin.AndroidX.Core`（FileProvider 支持） |
| **兼容性** | Android 8.0 (API 26) 及以上版本 |

---

## 2. 组件功能列表

| 功能名称 | 功能描述 | 类型 | 是否必填 | 备注限制 |
| --- | --- | --- | --- | --- |
| 版本对比检查 | 比对当前版本与目标版本，判断是否有更新可用 | 基础 | 必选 | `ValidateAsync`，支持自定义版本比较器 |
| APK 断点续传下载 | 从服务端下载 APK 包，支持 HTTP Range 断点续传 | 基础 | 必选 | `DownloadAndVerifyAsync` |
| SHA256 校验 | 下载完成后自动计算 SHA256 与服务端哈希比对 | 基础 | 自动 | 通过 `IHashValidator` 实现 |
| APK 安装触发 | 调用 Android Package Installer 安装 APK | 基础 | 必选 | `LaunchInstallerAsync` |
| HTTP 传输配置 | SSL 证书校验、代理、超时、重试、认证 | 拓展 | 可选 | `HttpDownloadOptions` |
| 多协议认证 | HMAC-SHA256 / Bearer / API Key / Basic | 拓展 | 可选 | 全局或单包粒度配置 |
| 进度通知 | 下载进度（速度、字节数、百分比） | 基础 | 可选 | `AddListenerDownloadProgressChanged` |
| 完成通知 | 版本发现、下载完成、安装完成事件 | 基础 | 可选 | `AddListenerUpdateCompleted` |
| 失败通知 | 失败原因、异常信息、失败包信息 | 基础 | 可选 | `AddListenerUpdateFailed` |
| 验证通知 | 发现可用更新时触发 | 基础 | 可选 | `AddListenerValidate` |
| 状态快照 | 随时获取当前更新状态 | 拓展 | 可选 | `GetSnapshot()` |
| UI 线程调度 | 自定义事件调度策略 | 拓展 | 可选 | 实现 `IUpdateEventDispatcher` |
| 自定义下载器 | 自定义下载实现 | 拓展 | 可选 | 实现 `IUpdateDownloader` |
| 自定义哈希验证 | 自定义哈希算法 | 拓展 | 可选 | 实现 `IHashValidator` |
| 自定义版本比较 | 自定义版本对比逻辑 | 拓展 | 可选 | 实现 `IVersionComparer` |
| 自定义 APK 安装器 | 自定义安装实现 | 拓展 | 可选 | 实现 `IApkInstaller` |
| 自定义文件存储 | 自定义文件读写实现 | 拓展 | 可选 | 实现 `IFileStorage` |
| 自定义 SSL 策略 | 自定义 HTTPS 证书校验 | 拓展 | 可选 | 实现 `ISslValidationPolicy` |
| 自定义 HTTP 认证 | 自定义 HTTP 请求认证 | 拓展 | 可选 | 实现 `IHttpAuthProvider` |

---

## 3. API 配置说明

### 3.1 配置字段（属性 Props）

**AndroidUpdateOptions：**

| 字段名 | 数据类型 | 默认值 | 是否必填 | 说明 |
| --- | --- | --- | --- | --- |
| `DownloadDirectoryPath` | `string` | `""`（自动使用 `CacheDir/update` 或 `TempPath/update`） | 可选 | 下载文件存放目录，为空时自动选择 |
| `FileProviderAuthority` | `string` | `""` | **是** | Android FileProvider authority，需与 AndroidManifest 配置一致 |
| `TemporaryFileExtension` | `string` | `".part"` | 可选 | 临时下载文件扩展名 |
| `SidecarExtension` | `string` | `".json"` | 可选 | 断点续传元数据文件扩展名 |
| `DownloadBufferSize` | `int` | `65536`（64 KB） | 可选 | 下载缓冲区大小（字节） |
| `SpeedSmoothingWindowSeconds` | `int` | `4` | 可选 | 下载速度平滑窗口（秒） |

**HttpDownloadOptions：**

| 字段名 | 数据类型 | 默认值 | 是否必填 | 说明 |
| --- | --- | --- | --- | --- |
| `SslValidationPolicy` | `ISslValidationPolicy?` | `null` | 可选 | 自定义 SSL 证书验证策略；`null` 使用系统默认 |
| `RequestTimeout` | `TimeSpan` | `30s` | 可选 | 单次 HTTP 请求超时 |
| `DownloadTimeout` | `TimeSpan` | `10min` | 可选 | 整体下载操作超时 |
| `Proxy` | `IWebProxy?` | `null` | 可选 | HTTP 代理 |
| `UseProxy` | `bool` | `false` | 可选 | 是否启用配置的代理 |
| `MaxRetryAttempts` | `int` | `3` | 可选 | 最大重试次数（3 表示初始 1 次 + 2 次重试） |
| `RetryBaseDelay` | `TimeSpan` | `1s` | 可选 | 指数退避基本延迟：`baseDelay * 2^attempt` |
| `AuthProvider` | `IHttpAuthProvider?` | `null` | 可选 | 全局 HTTP 认证提供器，单包认证优先级更高 |

**UpdatePackageInfo：**

| 字段名 | 数据类型 | 默认值 | 是否必填 | 说明 |
| --- | --- | --- | --- | --- |
| `Version` | `string` | — | **是** | 目标版本号 |
| `DownloadUrl` | `string` | — | **是** | APK 下载地址 |
| `Sha256` | `string` | — | **是** | APK 的 SHA256 哈希值，用于完整性校验 |
| `FileSize` | `long` | `0` | 可选 | APK 文件大小（字节） |
| `FileName` | `string?` | `null` | 可选 | 下载后的文件名 |
| `IsForced` | `bool` | `false` | 可选 | 是否强制更新 |
| `VersionName` | `string?` | `null` | 可选 | 版本展示名称 |
| `Description` | `string?` | `null` | 可选 | 版本描述 |
| `PublishTime` | `DateTimeOffset?` | `null` | 可选 | 发布时间 |
| `AuthScheme` | `AuthScheme?` | `null` | 可选 | 单包认证方案，优先于全局配置 |
| `AuthToken` | `string?` | `null` | 可选 | Bearer 或 API Key 认证令牌 |
| `AuthSecretKey` | `string?` | `null` | 可选 | HMAC-SHA256 签名密钥 |
| `BasicUsername` | `string?` | `null` | 可选 | HTTP Basic 用户名 |
| `BasicPassword` | `string?` | `null` | 可选 | HTTP Basic 密码 |

**UpdateState 枚举：**

| 枚举值 | 数值 | 说明 |
| --- | --- | --- |
| `None` | 0 | 初始状态 |
| `Checking` | 1 | 正在检查更新 |
| `UpdateAvailable` | 2 | 发现可用更新 |
| `Downloading` | 3 | 正在下载 |
| `Verifying` | 4 | 正在校验 |
| `ReadyToInstall` | 5 | 准备安装 |
| `Installing` | 6 | 正在安装 |
| `Completed` | 7 | 更新完成 |
| `Failed` | 8 | 更新失败 |
| `Canceled` | 9 | 已取消 |

**AuthScheme 枚举：**

| 枚举值 | 说明 |
| --- | --- |
| `Hmac` | HMAC-SHA256 签名认证 |
| `Bearer` | Bearer Token 认证 |
| `ApiKey` | API Key 认证 |
| `Basic` | HTTP Basic 认证 |

### 3.2 实例方法

**GeneralUpdateBootstrap（静态工厂）：**

| 方法名 | 入参明细 | 使用场景 | 注意事项 |
| --- | --- | --- | --- |
| `CreateDefault(AndroidUpdateOptions, ...)` | `options` — 更新选项；可选参数：`contextProvider`, `activityProvider`, `httpClient`, `versionComparer`, `eventDispatcher`, `logger`, `httpOptions` | 创建默认的 Android 更新引导实例 | 所有可选参数为 null 时使用内置默认实现 |

**IAndroidBootstrap：**

| 方法名 | 入参明细 | 使用场景 | 注意事项 |
| --- | --- | --- | --- |
| `ValidateAsync(UpdatePackageInfo, string, CancellationToken)` | `packageInfo` — 更新包信息；`currentVersion` — 当前版本；`ct` — 取消令牌 | 检查是否有可用的更新 | 返回 `UpdateCheckResult`，包含 `UpdateFound` 和 `TargetVersion` |
| `DownloadAndVerifyAsync(UpdatePackageInfo, CancellationToken)` | `packageInfo` — 更新包信息；`ct` — 取消令牌 | 下载 APK 并校验 SHA256 | 返回 `UpdateOperationResult`，成功时 `FilePath` 为 APK 路径 |
| `LaunchInstallerAsync(UpdatePackageInfo, string, CancellationToken)` | `packageInfo` — 更新包信息；`apkFilePath` — APK 文件路径；`ct` — 取消令牌 | 启动 Android Package Installer | 需要 FileProvider authority 配置正确 |
| `GetSnapshot()` | 无 | 获取当前更新状态快照 | 返回 `UpdateStateSnapshot(State, FailureReason, Message)` |
| `Dispose()` | 无 | 释放下载器等资源 | — |

### 3.3 回调事件

| 事件名称 | 回调参数 | 触发时机 | 使用说明 |
| --- | --- | --- | --- |
| `AddListenerValidate` | `ValidateEventArgs` — `PackageInfo`, `CurrentVersion` | 版本对比完成后，发现可用更新时触发 | 可用于 UI 展示新版本信息 |
| `AddListenerDownloadProgressChanged` | `DownloadProgressChangedEventArgs` — `ProgressPercentage`, `DownloadSpeedBytesPerSecond`, `DownloadedBytes`, `RemainingBytes`, `TotalBytes`, `PackageInfo`, `StatusDescription` | 下载进度更新时 | 可用于进度条展示 |
| `AddListenerUpdateCompleted` | `UpdateCompletedEventArgs` — `Result`（`UpdateOperationResult`） | 下载完成或安装完成时 | 可用于 UI 切换 |
| `AddListenerUpdateFailed` | `UpdateFailedEventArgs` — `Result`（`UpdateOperationResult`） | 任何阶段失败时 | 包含失败原因和异常信息 |

---

## 4. 扩展示例（高阶用法）

### 4.1 组件可扩展能力总览

所有服务均可通过 `CreateDefault` 的可选参数替换：

| 接口 | 默认实现 | 说明 |
| --- | --- | --- |
| `IVersionComparer` | `SystemVersionComparer` | 版本对比策略 |
| `IUpdateDownloader` | `HttpResumableApkDownloader` | APK 下载实现 |
| `IHashValidator` | `Sha256HashValidator` | SHA256 哈希校验 |
| `IApkInstaller` | `AndroidApkInstaller` | APK 安装触发 |
| `IFileStorage` | `PhysicalFileStorage` | 文件存储操作 |
| `IUpdateEventDispatcher` | `ImmediateEventDispatcher` | 事件调度器（直接调用） |
| `IUpdateLogger` | `NoOpUpdateLogger` | 日志记录器 |
| `IAndroidContextProvider` | `DefaultAndroidContextProvider` | Android Context 提供器 |
| `IAndroidActivityProvider` | `NullAndroidActivityProvider` | Android Activity 提供器 |
| `ISslValidationPolicy` | 通过 `HttpDownloadOptions` 配置 | SSL 证书验证策略 |
| `IHttpAuthProvider` | 通过 `HttpDownloadOptions` 或单包配置 | HTTP 认证提供器 |

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
- 实现失败时返回 `false` 和错误信息，引导跳过更新检查

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

#### 场景 3：自定义下载器（私有文件服务器）

【场景说明】企业内部使用私有文件服务器，需要在下载时注入自定义认证头。

```csharp
using GeneralUpdate.Avalonia.Android.Abstractions;
using GeneralUpdate.Avalonia.Android.Models;

public sealed class EnterpriseDownloader : IUpdateDownloader
{
    private readonly HttpClient _httpClient;
    private readonly string _enterpriseToken;

    public EnterpriseDownloader(HttpClient httpClient, string enterpriseToken)
    {
        _httpClient = httpClient;
        _enterpriseToken = enterpriseToken;
    }

    public async Task<DownloadResult> DownloadAsync(
        UpdatePackageInfo packageInfo,
        Action<DownloadProgressInfo>? progressCallback,
        CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, packageInfo.DownloadUrl);
        request.Headers.Add("X-Enterprise-Token", _enterpriseToken);
        
        // 自行实现下载和进度报告...
        // 返回 DownloadResult
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
    FileProviderAuthority = "com.myapp.fileprovider"
};

var bootstrap = GeneralUpdateBootstrap.CreateDefault(options);

// 1. 检查更新
var updatePackage = new UpdatePackageInfo
{
    Version = "2.0.0",
    DownloadUrl = "https://update.example.com/app-v2.0.0.apk",
    Sha256 = "expected-sha256-hash",
    FileSize = 50_000_000,
    FileName = "app_update_2.0.0.apk"
};

var checkResult = await bootstrap.ValidateAsync(
    updatePackage, currentVersion: "1.0.0");

if (!checkResult.UpdateFound)
{
    Console.WriteLine("Already up to date.");
    return;
}

// 2. 下载并校验
var downloadResult = await bootstrap.DownloadAndVerifyAsync(updatePackage);
if (!downloadResult.Success)
{
    Console.WriteLine($"Download failed: {downloadResult.Message}");
    return;
}

// 3. 安装
var installResult = await bootstrap.LaunchInstallerAsync(
    updatePackage, downloadResult.FilePath!);

if (installResult.Success)
    Console.WriteLine("Installer launched.");
```

### 5.2 基础参数组合示例

```csharp
using GeneralUpdate.Avalonia.Android;
using GeneralUpdate.Avalonia.Android.Abstractions;
using GeneralUpdate.Avalonia.Android.Models;
using GeneralUpdate.Avalonia.Android.Services;

var options = new AndroidUpdateOptions
{
    DownloadDirectoryPath = Path.Combine(
        Android.App.Application.Context.CacheDir!.AbsolutePath, "myapp-updates"),
    FileProviderAuthority = "com.myapp.fileprovider",
    TemporaryFileExtension = ".downloading",
    DownloadBufferSize = 128 * 1024  // 128 KB buffer
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
        $"{FormatSpeed(e.DownloadSpeedBytesPerSecond)}");
};

bootstrap.AddListenerUpdateFailed += (_, e) =>
{
    Console.WriteLine($"Failed: {e.Result.Message} (reason: {e.Result.FailureReason})");
};

bootstrap.AddListenerUpdateCompleted += (_, e) =>
{
    Console.WriteLine($"Completed: {e.Result.State}");
};

// 执行更新
var package = new UpdatePackageInfo
{
    Version = "2.0.0",
    DownloadUrl = "https://update.example.com/app-v2.0.0.apk",
    Sha256 = "expected-sha256-hash",
    FileSize = 50_000_000,
    FileName = "app-v2.0.0.apk"
};

var checkResult = await bootstrap.ValidateAsync(package, "1.0.0");
if (!checkResult.UpdateFound) return;

var downloadResult = await bootstrap.DownloadAndVerifyAsync(package);
if (!downloadResult.Success) return;

await bootstrap.LaunchInstallerAsync(package, downloadResult.FilePath!);

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

完整更新工作流，包含 HTTP 认证、断点续传、权限检查和进度 UI：

```csharp
using GeneralUpdate.Avalonia.Android;
using GeneralUpdate.Avalonia.Android.Enums;
using GeneralUpdate.Avalonia.Android.Models;

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
            AuthProvider = new Services.BearerTokenAuthProvider(serverToken)
        };

        _bootstrap = GeneralUpdateBootstrap.CreateDefault(
            new AndroidUpdateOptions
            {
                FileProviderAuthority = fileProviderAuthority
            },
            httpOptions: httpOptions);

        WireEvents();
    }

    public async Task<bool> UpdateAsync(
        UpdatePackageInfo package, string currentVersion,
        CancellationToken ct = default)
    {
        if (_bootstrap == null)
            throw new InvalidOperationException("Not initialized.");

        // Phase 1: 版本检查
        StatusChanged?.Invoke(this, "Checking for updates...");
        var checkResult = await _bootstrap.ValidateAsync(package, currentVersion, ct);
        if (!checkResult.UpdateFound)
        {
            StatusChanged?.Invoke(this, "Already up to date.");
            return true;
        }

        StatusChanged?.Invoke(this, $"Update v{package.Version} found.");

        // Phase 2: 下载并校验
        StatusChanged?.Invoke(this, "Downloading...");
        var downloadResult = await _bootstrap.DownloadAndVerifyAsync(package, ct);
        if (!downloadResult.Success)
        {
            StatusChanged?.Invoke(this, 
                $"Download failed: {downloadResult.Message}");
            return false;
        }

        StatusChanged?.Invoke(this, "Download complete, verifying...");

        // Phase 3: 检查安装权限 (Android 8+)
        if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
        {
            var ctx = Android.App.Application.Context;
            var pm = ctx.PackageManager;
            if (pm != null && !pm.CanRequestPackageInstalls())
            {
                StatusChanged?.Invoke(this, 
                    "Please allow install from unknown apps.");
                var intent = new Android.Content.Intent(
                    Android.Provider.Settings.ActionManageUnknownAppSources)
                    .SetData(Android.Net.Uri.Parse("package:" + ctx.PackageName));
                intent.AddFlags(Android.Content.ActivityFlags.NewTask);
                ctx.StartActivity(intent);
                return false;
            }
        }

        // Phase 4: 启动安装
        StatusChanged?.Invoke(this, "Installing...");
        var installResult = await _bootstrap.LaunchInstallerAsync(
            package, downloadResult.FilePath!, ct);

        if (installResult.Success)
        {
            StatusChanged?.Invoke(this, "Installer launched.");
            return true;
        }

        StatusChanged?.Invoke(this, 
            $"Install failed: {installResult.Message}");
        return false;
    }

    private void WireEvents()
    {
        if (_bootstrap == null) return;

        _bootstrap.AddListenerDownloadProgressChanged += (_, e) =>
        {
            ProgressChanged?.Invoke(this, e.ProgressPercentage);
        };

        _bootstrap.AddListenerUpdateFailed += (_, e) =>
        {
            StatusChanged?.Invoke(this, 
                $"Failed: {e.Result.Message} (reason: {e.Result.FailureReason})");
        };
    }

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

### 认证方案

Avalonia 组件支持四种认证方案，可在 `UpdatePackageInfo` 单包粒度配置：

| 认证方案 | AuthScheme | 所需字段 |
| --- | --- | --- |
| HMAC-SHA256 | `Hmac` | `AuthSecretKey` |
| Bearer Token | `Bearer` | `AuthToken` |
| API Key | `ApiKey` | `AuthToken` |
| HTTP Basic | `Basic` | `BasicUsername` + `BasicPassword` |

也可通过 `HttpDownloadOptions.AuthProvider` 设置全局认证提供器。单包配置优先于全局配置。

### SSL 策略

| 策略 | 类名 | 使用场景 |
| --- | --- | --- |
| 系统默认（推荐） | `null` | 生产环境 |
| 允许所有（仅开发） | `AllowAllSslValidationPolicy` | 自签名证书开发环境 |
| 自定义 | 实现 `ISslValidationPolicy` | 私有 CA 或证书固定 |

### 断点续传机制

下载器使用临时文件（`.part`）和元数据文件（`.json`）实现断点续传：

```
CacheDir/update/
├── app-v2.0.0.apk.part       # 部分下载的临时文件
├── app-v2.0.0.apk.json       # 续传元数据（URL、ETag、预期大小、已下载字节数）
└── app-v2.0.0.apk            # 下载完成后的最终文件
```

- 中断后重新下载同一 URL 时，自动读取 `.json` 元数据判断服务端是否支持 Range
- 支持 Range 时从已下载位置继续（`Range: bytes={existingLength}-`）
- 不支持 Range 时重新下载
- 下载完成后 `.part` 重命名为最终文件名，删除 `.json` 元数据

### AndroidManifest 配置

使用 FileProvider 分享 APK 文件给 Package Installer，需要在 `AndroidManifest.xml` 中配置：

```xml
<application>
    <provider
        android:name="androidx.core.content.FileProvider"
        android:authorities="${applicationId}.fileprovider"
        android:exported="false"
        android:grantUriPermissions="true">
        <meta-data
            android:name="android.support.FILE_PROVIDER_PATHS"
            android:resource="@xml/file_paths" />
    </provider>
</application>
```

`file_paths.xml`（在 `Resources/xml/` 目录下）：

```xml
<?xml version="1.0" encoding="utf-8"?>
<paths>
    <cache-path
        name="cache"
        path="update/" />
</paths>
```

### 平台差异

| 项目 | 说明 |
| --- | --- |
| 最低 API | 26（Android 8.0） |
| 安装权限 | Android 8.0+ 需要 `CanRequestPackageInstalls()` 检查 |
| FileProvider | 必须通过 FileProvider 向系统安装器传递 APK 文件 URI |

---

## 相关资源

- [GeneralUpdate.Avalonia 仓库](https://github.com/GeneralLibrary/GeneralUpdate.Avalonia)
- [Android 更新示例](https://github.com/GeneralLibrary/GeneralUpdate-Samples/tree/main/UI/AndroidUpdate)
- [Semantic Versioning](https://semver.org/)
- [FileProvider 文档](https://developer.android.com/training/secure-file-sharing/setup-sharing)
