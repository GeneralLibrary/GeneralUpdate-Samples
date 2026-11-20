---
sidebar_position: 10
---

# GeneralUpdate.Maui.Android

**命名空间:** `GeneralUpdate.Maui.Android` | **主要入口:** `GeneralUpdateBootstrap.CreateDefault()` / `AddGeneralUpdateMauiAndroid()` | **NuGet 包:** `GeneralUpdate.Maui.Android`

## 1. 组件简介

### 1.1 组件概述

**GeneralUpdate.Maui.Android** 是面向 .NET MAUI 应用的 Android 平台自动更新组件，为 Android APK 提供版本发现、可恢复下载、SHA256 完整性校验以及系统 Package Installer 触发的一站式更新编排。

与桌面应用的文件替换式更新不同，Android APK 更新必须经由系统 Package Installer 完成。该组件封装了这一平台差异，并提供两阶段 API：`ValidateAsync` 做版本检查，`ExecuteUpdateAsync` 串起下载→校验→安装的完整流程。

**核心能力：**

| 能力 | 说明 |
| --- | --- |
| 版本对比检查 | 内置 `System.Version` 解析，自动判断当前版本是否低于目标版本 |
| 可恢复 APK 下载 | `HttpRangeDownloader` 基于 HTTP Range 请求头实现断点续传，支持进度报告 |
| SHA256 完整性校验 | 下载后自动计算文件 SHA256 与服务端哈希比对，损坏文件自动删除 |
| 系统安装器触发 | `AndroidApkInstaller` 通过 FileProvider + Intent 启动系统 Package Installer |
| DI 容器集成 | `AddGeneralUpdateMauiAndroid()` 扩展方法一键注册所有服务到 `IServiceCollection` |
| HTTP 传输配置 | `HttpDownloadOptions` 支持 SSL 证书策略、代理、超时、认证，支持指数退避重试 |
| 多协议认证 | 支持 HMAC-SHA256、Bearer Token、API Key、HTTP Basic 四种认证方案 |
| 事件通知 | 版本发现、下载进度、完成阶段、失败原因等全方位事件 |
| 并发安全 | `ExecuteUpdateAsync` 内置 `Interlocked` 并发保护，重复调用直接返回 `AlreadyInProgress` |
| 临时文件管理 | 下载使用 `.downloading` 临时扩展名，完成后原子替换为目标文件 |

**解决的业务痛点：**
- MAUI Android 应用需要内置自动更新能力，但 APK 安装必须绕过 Google Play 通过系统安装器完成
- 大 APK 下载过程中网络不稳定，需要断点续传减少用户流量消耗
- 需要确保下载的 APK 未被篡改或损坏，保障更新安全
- 需要与 MAUI 的 DI 容器深度集成，简化接入代码

:::info MAUI 更新 ≠ 桌面文件替换
Android 平台无法像桌面端那样直接替换可执行文件。APK 必须通过 Android Package Installer 安装，系统会处理签名验证和应用替换。MAUI 组件封装了这一差异，但 **APK 签名、版本号递增** 仍然需要你在构建流水线中保证。
:::

**业务使用场景：**
- MAUI Android 应用的自动更新（企业内部分发、Beta 测试、旁加载）
- 需要通过 CDN / OSS 分发 APK 包的应用
- 需要 DI 容器管理的 MAUI 应用

### 1.2 环境与依赖

| 项目 | 说明 |
| --- | --- |
| **NuGet 包** | `GeneralUpdate.Maui.Android` |
| **目标框架** | `net10.0` + `net10.0-android`（多目标；Android 最低 API 21） |
| **依赖包** | `Microsoft.Extensions.DependencyInjection.Abstractions`、`Microsoft.Extensions.Http`、`Xamarin.AndroidX.Core` |
| **兼容性** | Android 5.0 (API 21) 及以上版本 |

---

## 2. 组件功能列表

| 功能名称 | 功能描述 | 类型 | 是否必填 | 备注限制 |
| --- | --- | --- | --- | --- |
| 版本对比检查 | 对比当前版本与目标版本，判断是否有更新可用 | 基础 | 必选 | `ValidateAsync`，基于 `System.Version` 解析 |
| 更新执行 | 串起下载→校验→安装全流程 | 基础 | 必选 | `ExecuteUpdateAsync`，并发保护 |
| APK 可恢复下载 | HTTP Range 断点续传 + 进度报告 | 基础 | 自动 | `HttpRangeDownloader` |
| SHA256 校验 | 下载后自动校验，损坏文件自动删除 | 基础 | 自动 | 通过 `IHashValidator`，支持进度回调 |
| APK 安装触发 | FileProvider + Intent 启动系统安装器 | 基础 | 自动 | `AndroidApkInstaller`，自动检查安装权限 |
| DI 容器集成 | 一键注册所有更新服务 | 拓展 | 推荐 | `AddGeneralUpdateMauiAndroid()` |
| HTTP 传输配置 | SSL、代理、超时、重试、认证 | 拓展 | 可选 | `HttpDownloadOptions` |
| 多协议认证 | HMAC-SHA256 / Bearer / API Key / Basic | 拓展 | 可选 | 全局或单包粒度配置 |
| 进度通知 | 下载字节数、速度、百分比、剩余时间 | 基础 | 可选 | `IProgress<DownloadStatistics>` 回调 |
| 完成通知 | 各完成阶段（下载完成、校验完成、安装触发、工作流完成） | 基础 | 可选 | `AddListenerUpdateCompleted` |
| 失败通知 | 失败原因分类、异常信息 | 基础 | 可选 | `AddListenerUpdateFailed` |
| 验证通知 | 发现可用更新时触发 | 基础 | 可选 | `AddListenerValidate` |
| 状态查询 | 实时查询当前更新状态 | 基础 | 可选 | `CurrentState` 属性 |
| 并发保护 | 防止重复执行更新操作 | 基础 | 自动 | `Interlocked` 原子操作 |
| 损坏文件自动清理 | 校验失败自动删除损坏包 | 拓展 | 可选 | `UpdateOptions.DeleteCorruptedPackageOnFailure` |
| 进度报告间隔控制 | 自定义进度回调频率 | 拓展 | 可选 | `UpdateOptions.ProgressReportInterval` |
| 自定义下载器 | 自定义下载实现 | 拓展 | 可选 | 实现 `IUpdateDownloader` |
| 自定义哈希验证 | 自定义哈希实现 | 拓展 | 可选 | 实现 `IHashValidator` |
| 自定义安装器 | 自定义安装逻辑 | 拓展 | 可选 | 实现 `IApkInstaller` |
| 自定义存储提供器 | 自定义文件路径和存储操作 | 拓展 | 可选 | 实现 `IUpdateStorageProvider` |
| 自定义日志 | 自定义日志输出 | 拓展 | 可选 | 实现 `IUpdateLogger` |
| 自定义 SSL 策略 | 自定义 HTTPS 证书校验 | 拓展 | 可选 | 实现 `ISslValidationPolicy` |
| 自定义 HTTP 认证 | 自定义 HTTP 请求认证 | 拓展 | 可选 | 实现 `IHttpAuthProvider` |

---

## 3. API 配置说明

### 3.1 配置字段（属性 Props）

**UpdateOptions：**

| 字段名 | 数据类型 | 默认值 | 是否必填 | 说明 |
| --- | --- | --- | --- | --- |
| `CurrentVersion` | `string` | `""` | **是** | 当前应用版本号 |
| `DownloadDirectory` | `string?` | `null` | 可选 | 下载目录，为空时自动选择 |
| `TemporaryFileExtension` | `string` | `".downloading"` | 可选 | 临时下载文件扩展名 |
| `DeleteCorruptedPackageOnFailure` | `bool` | `true` | 可选 | 校验失败时自动删除损坏包 |
| `ProgressReportInterval` | `TimeSpan` | `500ms` | 可选 | 进度报告间隔 |
| `InstallOptions` | `AndroidInstallOptions` | `new()` | 可选 | 安装选项 |

**AndroidInstallOptions：**

| 字段名 | 数据类型 | 默认值 | 是否必填 | 说明 |
| --- | --- | --- | --- | --- |
| `FileProviderAuthority` | `string` | `""` | **是** | FileProvider authority，需与 AndroidManifest 一致 |
| `MimeType` | `string` | `"application/vnd.android.package-archive"` | 可选 | APK 文件的 MIME 类型 |

**HttpDownloadOptions：**

| 字段名 | 数据类型 | 默认值 | 是否必填 | 说明 |
| --- | --- | --- | --- | --- |
| `SslValidationPolicy` | `ISslValidationPolicy?` | `null` | 可选 | 自定义 SSL 证书验证策略 |
| `DownloadTimeout` | `TimeSpan` | `10min` | 可选 | 整体下载超时 |
| `Proxy` | `IWebProxy?` | `null` | 可选 | HTTP 代理 |
| `UseProxy` | `bool` | `false` | 可选 | 是否启用代理 |
| `AuthProvider` | `IHttpAuthProvider?` | `null` | 可选 | 全局 HTTP 认证提供器 |

**UpdatePackageInfo：**

| 字段名 | 数据类型 | 默认值 | 是否必填 | 说明 |
| --- | --- | --- | --- | --- |
| `Version` | `string` | `""` | **是** | 目标版本号 |
| `DownloadUrl` | `string` | `""` | **是** | APK 下载地址 |
| `Sha256` | `string` | `""` | **是** | SHA256 哈希值 |
| `PackageSize` | `long?` | `null` | 可选 | 包大小（字节） |
| `ApkFileName` | `string?` | `null` | 可选 | APK 文件名 |
| `ForceUpdate` | `bool` | `false` | 可选 | 是否强制更新 |
| `VersionName` | `string?` | `null` | 可选 | 展示名称 |
| `ReleaseNotes` | `string?` | `null` | 可选 | 发布说明 |
| `PublishTime` | `DateTimeOffset?` | `null` | 可选 | 发布时间 |
| `AuthScheme` | `AuthScheme?` | `null` | 可选 | 认证方案 |
| `AuthToken` | `string?` | `null` | 可选 | 认证令牌 |
| `AuthSecretKey` | `string?` | `null` | 可选 | HMAC 密钥 |
| `BasicUsername` | `string?` | `null` | 可选 | Basic 用户名 |
| `BasicPassword` | `string?` | `null` | 可选 | Basic 密码 |

**UpdateState 枚举：**

| 枚举值 | 说明 |
| --- | --- |
| `None` | 初始/空闲状态 |
| `Checking` | 正在检查更新 |
| `UpdateAvailable` | 发现可用更新 |
| `Downloading` | 正在下载 |
| `Verifying` | 正在校验 |
| `ReadyToInstall` | 准备安装 |
| `Installing` | 正在安装 |
| `Completed` | 更新完成 |
| `Failed` | 更新失败 |
| `Canceled` | 已取消 |

**UpdateFailureReason 枚举：**

| 枚举值 | 说明 |
| --- | --- |
| `Unknown` | 未知错误 |
| `InvalidInput` | 输入参数无效 |
| `AlreadyInProgress` | 已有更新操作正在进行 |
| `Network` | 网络错误 |
| `Download` | 下载错误 |
| `FileAccess` | 文件访问错误 |
| `IntegrityCheckFailed` | 完整性校验失败 |
| `InstallPermissionDenied` | 安装权限被拒绝 |
| `Installation` | 安装过程错误 |
| `Canceled` | 用户取消 |
| `NoUpdateAvailable` | 无可用更新 |

**UpdateCompletionStage 枚举：**

| 枚举值 | 说明 |
| --- | --- |
| `DownloadCompleted` | 下载完成 |
| `VerificationCompleted` | 校验完成 |
| `InstallationTriggered` | 安装器已启动 |
| `WorkflowCompleted` | 工作流完成 |

### 3.2 实例方法

**GeneralUpdateBootstrap（静态工厂）：**

| 方法名 | 入参明细 | 使用场景 | 注意事项 |
| --- | --- | --- | --- |
| `CreateDefault(HttpClient?, IUpdateLogger?, HttpDownloadOptions?)` | `httpClient` — 可选外部 HttpClient；`logger` — 可选日志器；`httpOptions` — 可选 HTTP 配置 | 创建默认的 Android 更新引导实例 | `httpOptions` 生效时 `httpClient` 参数被忽略 |
| `AddGeneralUpdateMauiAndroid(IServiceCollection, HttpClient?)` | `services` — DI 容器；`httpClient` — 可选外部 HttpClient | 在 MAUI DI 容器中注册所有更新服务 | 返回 `IServiceCollection` 支持链式调用 |

**IAndroidBootstrap：**

| 方法名 | 入参明细 | 使用场景 | 注意事项 |
| --- | --- | --- | --- |
| `ValidateAsync(UpdatePackageInfo, UpdateOptions, CancellationToken)` | `packageInfo` — 更新包信息；`options` — 更新选项；`ct` — 取消令牌 | 检查是否有可用的更新 | 返回 `UpdateCheckResult`，包含 `IsUpdateAvailable` |
| `ExecuteUpdateAsync(UpdatePackageInfo, UpdateOptions, CancellationToken)` | `packageInfo` — 更新包信息；`options` — 更新选项；`ct` — 取消令牌 | 执行完整更新：下载→校验→安装 | 内置并发保护，重复调用返回 `AlreadyInProgress` |
| `Dispose()` | 无 | 释放下载器等资源 | — |
| `CurrentState` | 属性（`UpdateState`） | 查询当前更新状态 | 线程安全（`Volatile.Read`） |

### 3.3 回调事件

| 事件名称 | 回调参数 | 触发时机 | 使用说明 |
| --- | --- | --- | --- |
| `AddListenerValidate` | `ValidateEventArgs` — `PackageInfo` | 版本对比完成，发现可用更新时 | 可用于 UI 展示新版本信息 |
| `AddListenerDownloadProgressChanged` | `DownloadProgressChangedEventArgs` — `PackageInfo`, `Statistics`（`DownloadProgressInfo`：`ProgressPercentage`, `DownloadedBytes`, `TotalBytes`, `BytesPerSecond`, `RemainingBytes`）, `StatusDescription` | 下载进度更新时 | 通过 `IProgress<DownloadStatistics>` 回调 |
| `AddListenerUpdateCompleted` | `UpdateCompletedEventArgs` — `PackageInfo`, `Stage`（`UpdateCompletionStage`）, `PackagePath` | 各阶段完成时 | 分为下载完成、校验完成、安装触发、工作流完成四个阶段 |
| `AddListenerUpdateFailed` | `UpdateFailedEventArgs` — `Reason`, `Message`, `Exception`, `PackageInfo` | 任何阶段失败时 | 包含结构化的失败原因 |

---

## 4. 扩展示例（高阶用法）

### 4.1 组件可扩展能力总览

所有服务可通过 `AddGeneralUpdateMauiAndroid()` 注册后，通过 DI 容器替换：

| 接口 | 默认实现 | 说明 |
| --- | --- | --- |
| `IUpdateDownloader` | `HttpRangeDownloader` | 可恢复 APK 下载 |
| `IHashValidator` | `Sha256Validator` | SHA256 哈希校验 |
| `IApkInstaller` | `AndroidApkInstaller` | APK 安装触发 |
| `IUpdateStorageProvider` | `UpdateFileStore` | 文件路径和存储操作 |
| `IUpdateLogger` | `NullUpdateLogger` | 日志记录器 |
| `IUpdateBootstrap` | `AndroidBootstrap` | 更新编排（通过 DI 注册） |
| `ISslValidationPolicy` | 通过 `HttpDownloadOptions` 配置 | SSL 证书验证策略 |
| `IHttpAuthProvider` | 通过 `HttpDownloadOptions` 或单包配置 | HTTP 认证提供器 |

### 4.2 分场景示例

#### 场景 1：DI 容器集成

【场景说明】在 MAUI 应用中将更新服务注册到 DI 容器，便于管理和测试。

```csharp
using GeneralUpdate.Maui.Android.Abstractions;
using GeneralUpdate.Maui.Android.Models;
using GeneralUpdate.Maui.Android.Services;
using Microsoft.Extensions.Logging;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts => { });

        // 注册更新服务
        builder.Services.AddGeneralUpdateMauiAndroid();
        
        // 注册应用服务
        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddSingleton<MainPage>();

        return builder.Build();
    }
}

// 在 ViewModel 中使用
public class MainViewModel
{
    private readonly IAndroidBootstrap _bootstrap;

    public MainViewModel(IAndroidBootstrap bootstrap)
    {
        _bootstrap = bootstrap;
        WireEvents();
    }

    public async Task CheckAndUpdateAsync()
    {
        var package = new UpdatePackageInfo
        {
            Version = "2.0.0",
            DownloadUrl = "https://example.com/app-v2.apk",
            Sha256 = "expected-sha256"
        };

        var options = new UpdateOptions
        {
            CurrentVersion = "1.0.0",
            InstallOptions = new AndroidInstallOptions
            {
                FileProviderAuthority = "com.myapp.fileprovider"
            }
        };

        var result = await _bootstrap.ExecuteUpdateAsync(package, options, CancellationToken.None);
        Console.WriteLine(result.IsSuccess ? "Update succeeded!" : $"Failed: {result.Message}");
    }

    private void WireEvents()
    {
        _bootstrap.AddListenerDownloadProgressChanged += (_, args) =>
        {
            Console.WriteLine($"Progress: {args.Statistics.ProgressPercentage:F1}%");
        };
    }
}
```

#### 场景 2：自定义 HTTP 认证

【场景说明】企业内部分发的 APK 需要 Bearer Token 认证。

```csharp
using GeneralUpdate.Maui.Android.Abstractions;
using GeneralUpdate.Maui.Android.Models;
using GeneralUpdate.Maui.Android.Services;

// 方式一：使用全局认证提供器
var httpOptions = new HttpDownloadOptions
{
    AuthProvider = new Services.BearerTokenAuthProvider("your-token-here")
};

var bootstrap = GeneralUpdateBootstrap.CreateDefault(
    httpClient: null,
    logger: null,
    httpOptions: httpOptions);

// 方式二：使用单包认证
var package = new UpdatePackageInfo
{
    Version = "2.0.0",
    DownloadUrl = "https://example.com/app-v2.apk",
    Sha256 = "expected-sha256",
    AuthScheme = Enums.AuthScheme.Bearer,
    AuthToken = "your-token-here"
};
```

#### 场景 3：非容器环境下的直接使用

【场景说明】简单应用不依赖 DI 容器，直接使用静态工厂创建。

```csharp
using GeneralUpdate.Maui.Android;
using GeneralUpdate.Maui.Android.Models;

// 直接创建
var bootstrap = GeneralUpdateBootstrap.CreateDefault();

// 配置更新选项
var options = new UpdateOptions
{
    CurrentVersion = "1.0.0",
    InstallOptions = new AndroidInstallOptions
    {
        FileProviderAuthority = "com.myapp.fileprovider"
    }
};

var package = new UpdatePackageInfo
{
    Version = "2.0.0",
    DownloadUrl = "https://example.com/app-v2.apk",
    Sha256 = "expected-sha256",
    PackageSize = 50_000_000,
    ApkFileName = "app-v2.apk"
};

// 执行更新
var result = await bootstrap.ExecuteUpdateAsync(package, options, CancellationToken.None);
```

---

## 5. 常规使用示例

### 5.1 快速入门示例（最简 demo）

```csharp
using GeneralUpdate.Maui.Android;
using GeneralUpdate.Maui.Android.Models;

// 创建实例
var bootstrap = GeneralUpdateBootstrap.CreateDefault();

// 更新包信息（通常从服务端 API 获取）
var package = new UpdatePackageInfo
{
    Version = "2.0.0",
    DownloadUrl = "https://update.example.com/app-v2.0.0.apk",
    Sha256 = "a1b2c3d4e5f6..." // 服务端返回的 SHA256
};

// 更新选项
var options = new UpdateOptions
{
    CurrentVersion = "1.0.0",
    InstallOptions = new AndroidInstallOptions
    {
        FileProviderAuthority = "com.myapp.fileprovider"
    }
};

// 检查更新
var checkResult = await bootstrap.ValidateAsync(package, options, CancellationToken.None);
if (!checkResult.IsUpdateAvailable)
{
    Console.WriteLine("Already up to date.");
    return;
}

// 执行更新（下载 → 校验 → 安装）
var result = await bootstrap.ExecuteUpdateAsync(package, options, CancellationToken.None);
Console.WriteLine(result.IsSuccess ? "Update completed." : $"Update failed: {result.Message}");
```

### 5.2 基础参数组合示例

```csharp
using GeneralUpdate.Maui.Android;
using GeneralUpdate.Maui.Android.Enums;
using GeneralUpdate.Maui.Android.Models;

var httpOptions = new HttpDownloadOptions
{
    DownloadTimeout = TimeSpan.FromMinutes(30),
    AuthProvider = new GeneralUpdate.Maui.Android.Services.BearerTokenAuthProvider("token")
};

var bootstrap = GeneralUpdateBootstrap.CreateDefault(
    httpClient: null,
    logger: null,
    httpOptions: httpOptions);

// 事件监听
bootstrap.AddListenerDownloadProgressChanged += (_, e) =>
{
    var s = e.Statistics;
    Console.WriteLine($"Progress: {s.ProgressPercentage:F1}% | " +
                      $"{FormatSize(s.DownloadedBytes)}/{FormatSize(s.TotalBytes)} | " +
                      $"{FormatSpeed(s.BytesPerSecond)}");
};

bootstrap.AddListenerUpdateCompleted += (_, e) =>
{
    Console.WriteLine($"Stage completed: {e.Stage}");
};

bootstrap.AddListenerUpdateFailed += (_, e) =>
{
    Console.WriteLine($"Failed: {e.Reason} — {e.Message}");
};

var options = new UpdateOptions
{
    CurrentVersion = "1.0.0",
    TemporaryFileExtension = ".downloading",
    DeleteCorruptedPackageOnFailure = true,
    ProgressReportInterval = TimeSpan.FromMilliseconds(250),
    InstallOptions = new AndroidInstallOptions
    {
        FileProviderAuthority = "com.myapp.fileprovider"
    }
};

var package = new UpdatePackageInfo
{
    Version = "2.0.0",
    DownloadUrl = "https://cdn.example.com/app-v2.0.0.apk",
    Sha256 = "expected-sha256",
    PackageSize = 50_000_000,
    ApkFileName = "app-v2.0.0.apk",
    ForceUpdate = false,
    ReleaseNotes = "Bug fixes and performance improvements"
};

var result = await bootstrap.ExecuteUpdateAsync(package, options, CancellationToken.None);

if (!result.IsSuccess)
{
    Console.WriteLine($"Update failed: {result.FailureReason}");
}

// 辅助方法
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

完整 MAUI Android 更新工作流，包含服务端 API 对接、事件处理和错误分类：

```csharp
using GeneralUpdate.Maui.Android;
using GeneralUpdate.Maui.Android.Abstractions;
using GeneralUpdate.Maui.Android.Enums;
using GeneralUpdate.Maui.Android.Models;
using System.Net.Http.Json;

public sealed class MauiUpdateService : IDisposable
{
    private readonly HttpClient _httpClient;
    private IAndroidBootstrap? _bootstrap;
    private readonly string _fileProviderAuthority;
    private bool _disposed;

    public MauiUpdateService(HttpClient httpClient, string fileProviderAuthority)
    {
        _httpClient = httpClient;
        _fileProviderAuthority = fileProviderAuthority;
    }

    // 初始化（接收 HttpDownloadOptions 实现可选认证）
    public void Initialize(HttpDownloadOptions? httpOptions = null)
    {
        _bootstrap = GeneralUpdateBootstrap.CreateDefault(
            httpClient: null,
            logger: null,
            httpOptions: httpOptions);
        WireEvents();
    }

    // 从服务端获取更新包信息并执行更新
    public async Task<UpdateResult> CheckAndUpdateAsync(
        string serverUrl, string currentVersion,
        CancellationToken ct = default)
    {
        if (_bootstrap == null)
            throw new InvalidOperationException("Call Initialize() first.");

        try
        {
            // 1. 从服务端获取更新包信息
            var package = await FetchPackageFromServerAsync(serverUrl, currentVersion, ct);
            if (package == null)
                return UpdateResult.NoUpdate();

            // 2. 检查版本
            var options = new UpdateOptions
            {
                CurrentVersion = currentVersion,
                InstallOptions = new AndroidInstallOptions
                {
                    FileProviderAuthority = _fileProviderAuthority
                }
            };

            var checkResult = await _bootstrap.ValidateAsync(package, options, ct);
            if (!checkResult.IsUpdateAvailable)
                return UpdateResult.NoUpdate();

            // 3. 执行更新
            var execResult = await _bootstrap.ExecuteUpdateAsync(package, options, ct);

            return execResult.IsSuccess
                ? UpdateResult.Success()
                : UpdateResult.Failure(execResult.FailureReason, execResult.Message);
        }
        catch (OperationCanceledException)
        {
            return UpdateResult.Canceled();
        }
        catch (HttpRequestException ex)
        {
            return UpdateResult.Failure(UpdateFailureReason.Network, ex.Message);
        }
    }

    private async Task<UpdatePackageInfo?> FetchPackageFromServerAsync(
        string serverUrl, string currentVersion, CancellationToken ct)
    {
        // 调用服务端版本检查 API
        var request = new
        {
            Version = currentVersion,
            AppType = 1,
            Platform = 4,
            ProductId = "your-product-id"
        };

        var response = await _httpClient.PostAsJsonAsync(
            $"{serverUrl}/Upgrade/Verification", request, ct);

        response.EnsureSuccessStatusCode();

        // 解析响应体
        var body = await response.Content.ReadFromJsonAsync<ServerResponse>(ct);
        // ... 映射到 UpdatePackageInfo

        return null; // 实际开发中替换为真实映射逻辑
    }

    private void WireEvents()
    {
        if (_bootstrap == null) return;

        _bootstrap.AddListenerDownloadProgressChanged += (_, e) =>
        {
            var s = e.Statistics;
            OnProgressChanged?.Invoke(this, 
                (s.ProgressPercentage, s.BytesPerSecond));
        };

        _bootstrap.AddListenerUpdateFailed += (_, e) =>
        {
            OnStatusChanged?.Invoke(this, 
                $"Failed [{e.Reason}]: {e.Message}");
        };

        _bootstrap.AddListenerUpdateCompleted += (_, e) =>
        {
            OnStatusChanged?.Invoke(this, $"Stage: {e.Stage}");
        };
    }

    // 公开事件
    public event EventHandler<(double Percentage, double Speed)>? OnProgressChanged;
    public event EventHandler<string>? OnStatusChanged;

    public void Dispose()
    {
        if (_disposed) return;
        _bootstrap?.Dispose();
        _disposed = true;
    }
}

// 结果模型
public sealed record UpdateResult
{
    public bool IsSuccess { get; init; }
    public bool IsCanceled { get; init; }
    public UpdateFailureReason FailureReason { get; init; }
    public string? Message { get; init; }

    public static UpdateResult Success() => new() { IsSuccess = true };
    public static UpdateResult NoUpdate() => new() { IsSuccess = true, Message = "No update available." };
    public static UpdateResult Canceled() => new() { IsCanceled = true, Message = "Canceled." };
    public static UpdateResult Failure(UpdateFailureReason reason, string message) =>
        new() { FailureReason = reason, Message = message };
}

// 注意：此为示例结构，实际需要替换为服务端返回的格式
public sealed record ServerResponse
{
    public int Code { get; init; }
    public List<PackageEntry>? Body { get; init; }
}

public sealed record PackageEntry
{
    public string Version { get; init; } = "";
    public string Url { get; init; } = "";
    public string Hash { get; init; } = "";
    public long Size { get; init; }
}
```

---

## 6. 全局配置

### DI 注册方式

```csharp
// 方式一：使用默认 HttpClient
services.AddGeneralUpdateMauiAndroid();

// 方式二：外部传入 HttpClient（共享连接池）
services.AddGeneralUpdateMauiAndroid(myHttpClient);

// 方式三：IHttpClientFactory 注入（通过 AddHttpClient）
services.AddHttpClient<IUpdateDownloader, HttpRangeDownloader>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(10);
});
services.AddSingleton<IAndroidBootstrap, AndroidBootstrap>();
// 其他服务同理...
```

### 认证方案

MAUI 组件支持四种认证方案，可在 `UpdatePackageInfo` 单包粒度配置：

| 认证方案 | AuthScheme | 所需字段 |
| --- | --- | --- |
| HMAC-SHA256 | `Hmac` | `AuthSecretKey` |
| Bearer Token | `Bearer` | `AuthToken` |
| API Key | `ApiKey` | `AuthToken` |
| HTTP Basic | `Basic` | `BasicUsername` + `BasicPassword` |

### SSL 策略

| 策略 | 类名 | 使用场景 |
| --- | --- | --- |
| 系统默认（推荐） | `null` | 生产环境 |
| 允许所有（仅开发） | `AllowAllSslValidationPolicy` | 自签名证书开发环境 |
| 自定义 | 实现 `ISslValidationPolicy` | 私有 CA |

### AndroidManifest 配置

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

`file_paths.xml`（`Platforms/Android/Resources/xml/file_paths.xml`）：

```xml
<?xml version="1.0" encoding="utf-8"?>
<paths>
    <cache-path name="cache" path="update/" />
</paths>
```

### 平台差异

| 项目 | 说明 |
| --- | --- |
| 最低 API | 21（Android 5.0） |
| 安装权限 | Android 8.0+ 需要 `CanRequestPackageInstalls()` 检查 |
| FileProvider | 必须通过 FileProvider 向系统安装器传递 APK 文件 URI |

---

## 相关资源

- [GeneralUpdate.Maui 仓库](https://github.com/GeneralLibrary/GeneralUpdate.Maui)
- [MAUI Android 更新示例](https://github.com/GeneralLibrary/GeneralUpdate-Samples/tree/main/UI/MauiUpdate)
- [FileProvider 文档](https://developer.android.com/training/secure-file-sharing/setup-sharing)
