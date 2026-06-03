---
sidebar_position: 13
title: Android 自动更新
---

# Android 自动更新

## 组件概览

GeneralUpdate 为 .NET Android 应用提供了两套独立的自动更新组件：

| 组件 | NuGet 包 | 适用框架 | 仓库 |
|------|----------|----------|------|
| **GeneralUpdate.Maui.Android** | `GeneralUpdate.Maui.Android` | .NET MAUI（Android） | [GitHub](https://github.com/GeneralLibrary/GeneralUpdate.Maui) |
| **GeneralUpdate.Avalonia.Android** | `GeneralUpdate.Avalonia.Android` | Avalonia 12+（Android） | [GitHub](https://github.com/GeneralLibrary/GeneralUpdate.Avalonia) |

两个组件都是**无 UI** 的 Android 自动更新核心库，专注于可复用的更新编排。它们不提供任何界面，由调用方自行实现更新提示 UI。

### 共同特性

- **版本校验**：对比当前版本与远程版本，判断是否需要更新
- **断点续传下载**：基于 HTTP Range 的 APK 断点续传下载
- **SHA256 完整性校验**：自动验证下载文件的哈希值
- **系统安装器触发**：通过 `Intent` + `FileProvider` 调起 Android 系统安装器
- **事件通知**：下载进度、完成、失败等生命周期事件

### 更新流程（端到端）

```text
┌──────────────┐    ┌──────────────┐    ┌──────────────┐    ┌──────────────┐
│ 1. 版本验证   │ -> │ 2. 断点下载   │ -> │ 3. 哈希校验   │ -> │ 4. 触发安装   │
│ ValidateAsync │    │ Download     │    │ SHA256 Check │    │ Installer    │
└──────────────┘    └──────────────┘    └──────────────┘    └──────────────┘
```

---

## GeneralUpdate.Maui.Android

### 安装

目标框架：`net10.0; net10.0-android`

```bash
dotnet add package GeneralUpdate.Maui.Android
```

### 依赖注入（DI）

```csharp
using GeneralUpdate.Maui.Android.Services;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddGeneralUpdateMauiAndroid();

using var provider = services.BuildServiceProvider();
var bootstrap = provider.GetRequiredService<IAndroidBootstrap>();
```

也支持直接创建（无需 DI 容器）：

```csharp
var bootstrap = GeneralUpdateBootstrap.CreateDefault();
```

### 快速开始

```csharp
using GeneralUpdate.Maui.Android.Models;
using GeneralUpdate.Maui.Android.Services;

var bootstrap = GeneralUpdateBootstrap.CreateDefault();

// ── 事件监听 ──────────────────────────────────
bootstrap.AddListenerValidate += (_, e) =>
{
    Console.WriteLine($"发现新版本: {e.PackageInfo.Version}");
};

bootstrap.AddListenerDownloadProgressChanged += (_, e) =>
{
    var s = e.Statistics;
    Console.WriteLine(
        $"{s.ProgressPercentage:F2}% | {s.DownloadedBytes}/{s.TotalBytes} | " +
        $"剩余: {s.RemainingBytes} | 速度: {s.BytesPerSecond:F0} B/s");
};

bootstrap.AddListenerUpdateCompleted += (_, e) =>
{
    Console.WriteLine($"阶段={e.Stage}, 文件={e.PackagePath}");
};

bootstrap.AddListenerUpdateFailed += (_, e) =>
{
    Console.WriteLine($"失败: {e.Reason}, {e.Message}");
};

// ── 构造更新包信息 ────────────────────────────
var package = new UpdatePackageInfo
{
    Version = "2.0.0",
    VersionName = "2.0",
    ReleaseNotes = "性能和稳定性改进",
    DownloadUrl = "https://example.com/app-release.apk",
    Sha256 = "3A0D2F...F9C2",
    PackageSize = 52_428_800
};

// ── 执行更新 ──────────────────────────────────
var options = new UpdateOptions
{
    CurrentVersion = "1.5.0",
    InstallOptions = new AndroidInstallOptions
    {
        FileProviderAuthority = $"{AppInfo.PackageName}.fileprovider"
    }
};

var check = await bootstrap.ValidateAsync(package, options, CancellationToken.None);
if (check.IsUpdateAvailable)
{
    var result = await bootstrap.ExecuteUpdateAsync(package, options, CancellationToken.None);
    Console.WriteLine(result.IsSuccess ? "更新流程已完成。" : $"更新失败: {result.Message}");
}
```

### 核心 API

#### IAndroidBootstrap

| 方法 | 说明 |
|------|------|
| `ValidateAsync(package, options, ct)` | 校验远程版本是否高于当前版本 |
| `ExecuteUpdateAsync(package, options, ct)` | 执行完整更新流程（下载 + 校验 + 安装） |

#### 事件

| 事件 | 触发时机 |
|------|----------|
| `AddListenerValidate` | 检测到更高版本时触发 |
| `AddListenerDownloadProgressChanged` | 下载进度更新（速度、已下载字节、剩余字节、百分比） |
| `AddListenerUpdateCompleted` | 工作流里程碑：`DownloadCompleted`、`VerificationCompleted`、`InstallationTriggered` |
| `AddListenerUpdateFailed` | 更新失败，附带 `UpdateFailureReason` 和错误消息 |

#### UpdateOptions

| 属性 | 类型 | 说明 |
|------|------|------|
| `CurrentVersion` | `string` | 当前应用版本号 |
| `DownloadDirectory` | `string?` | 下载目录（可选，默认使用应用缓存目录） |
| `TemporaryFileExtension` | `string` | 下载中临时文件扩展名，默认 `.downloading` |
| `DeleteCorruptedPackageOnFailure` | `bool` | 失败时是否删除损坏的包，默认 `true` |
| `ProgressReportInterval` | `TimeSpan` | 进度报告间隔，默认 500ms |
| `InstallOptions` | `AndroidInstallOptions` | Android 安装选项 |

#### UpdatePackageInfo

| 属性 | 类型 | 说明 |
|------|------|------|
| `Version` | `string` | 远程版本号（必填） |
| `VersionName` | `string?` | 版本名称（展示用） |
| `ReleaseNotes` | `string?` | 更新日志 |
| `DownloadUrl` | `string` | APK 下载地址（必填） |
| `Sha256` | `string` | APK 文件 SHA256 哈希值（必填） |
| `PackageSize` | `long?` | 包大小（字节），用于进度估算 |
| `ForceUpdate` | `bool` | 是否强制更新 |

---

## GeneralUpdate.Avalonia.Android

### 安装

目标框架：`net8.0-android`（兼容 `net9.0-android`+）

```bash
dotnet add package GeneralUpdate.Avalonia.Android
```

### Avalonia UI 线程调度

Avalonia 需要在 UI 线程上更新界面。实现 `IUpdateEventDispatcher` 将回调调度到 Avalonia UI 线程：

```csharp
using GeneralUpdate.Avalonia.Android.Abstractions;

public sealed class AvaloniaUiDispatcher : IUpdateEventDispatcher
{
    public void Dispatch(Action callback)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(callback);
    }
}
```

### 快速开始

```csharp
using GeneralUpdate.Avalonia.Android;
using GeneralUpdate.Avalonia.Android.Abstractions;
using GeneralUpdate.Avalonia.Android.Models;

// ── 配置选项 ──────────────────────────────────
var options = new AndroidUpdateOptions
{
    DownloadDirectoryPath = Path.Combine(
        Android.App.Application.Context.CacheDir!.AbsolutePath!, "update"),
    FileProviderAuthority = "com.example.app.generalupdate.fileprovider"
};

var bootstrap = GeneralUpdateBootstrap.CreateDefault(
    options,
    eventDispatcher: new AvaloniaUiDispatcher());

// ── 事件监听 ──────────────────────────────────
bootstrap.AddListenerValidate += (_, e) =>
{
    Console.WriteLine($"发现新版本: {e.PackageInfo.Version} → 当前: {e.CurrentVersion}");
};

bootstrap.AddListenerDownloadProgressChanged += (_, e) =>
{
    var d = e.Download;
    Console.WriteLine($"{d.ProgressPercentage:F1}% | {d.Downloaded}/{d.Total} | 速度: {d.Speed:F0} B/s");
};

bootstrap.AddListenerUpdateCompleted += (_, e) =>
{
    Console.WriteLine($"完成: {e.Result.Stage}, 文件: {e.Result.FilePath}");
};

bootstrap.AddListenerUpdateFailed += (_, e) =>
{
    Console.WriteLine($"失败: {e.Result.Reason}, {e.Result.Message}");
};

// ── 构造更新包信息 ────────────────────────────
var packageInfo = new UpdatePackageInfo
{
    Version = "2.3.0",
    VersionName = "2.3",
    Description = "新增暗色模式支持，修复若干问题",
    DownloadUrl = "https://example.com/app-release.apk",
    Sha256 = "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef",
    FileSize = 52_428_800,
    FileName = "app-release.apk",
    IsForced = false
};

// ── 三阶段更新流程 ────────────────────────────
var check = await bootstrap.ValidateAsync(packageInfo, "2.2.1", CancellationToken.None);
if (check.UpdateFound)
{
    var prepared = await bootstrap.DownloadAndVerifyAsync(packageInfo, CancellationToken.None);
    if (prepared.Success && prepared.FilePath is not null)
    {
        await bootstrap.LaunchInstallerAsync(packageInfo, prepared.FilePath, CancellationToken.None);
    }
}
```

### 核心 API

#### IAndroidBootstrap

Avalonia 采用**三阶段 API**，将下载验证与安装触发分离，给予调用方更多控制：

| 方法 | 说明 |
|------|------|
| `ValidateAsync(packageInfo, currentVersion, ct)` | 版本校验，返回 `UpdateCheckResult` |
| `DownloadAndVerifyAsync(packageInfo, ct)` | 下载 APK 并校验 SHA256，返回 `UpdateOperationResult` |
| `LaunchInstallerAsync(packageInfo, apkFilePath, ct)` | 调起 Android 系统安装器 |
| `GetSnapshot()` | 获取当前更新状态快照 |

#### 事件

| 事件 | 参数 | 说明 |
|------|------|------|
| `AddListenerValidate` | `ValidateEventArgs` | 版本验证完成 |
| `AddListenerDownloadProgressChanged` | `DownloadProgressChangedEventArgs` | 下载进度（速度、已下载、剩余、百分比、状态消息） |
| `AddListenerUpdateCompleted` | `UpdateCompletedEventArgs` | 各阶段完成通知 |
| `AddListenerUpdateFailed` | `UpdateFailedEventArgs` | 更新失败详情 |

#### AndroidUpdateOptions

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `DownloadDirectoryPath` | `string` | 空（使用应用缓存） | APK 下载目录 |
| `TemporaryFileExtension` | `string` | `.part` | 下载临时文件扩展名 |
| `SidecarExtension` | `string` | `.json` | 断点续传 sidecar 元数据扩展名 |
| `FileProviderAuthority` | `string` | 空（必填） | Android `FileProvider` authority |
| `DownloadBufferSize` | `int` | `64 * 1024` | 下载缓冲区大小 |
| `SpeedSmoothingWindowSeconds` | `int` | `4` | 速度平滑窗口（秒） |

#### UpdatePackageInfo

| 属性 | 类型 | 说明 |
|------|------|------|
| `Version` | `string` | 远程版本号（必填） |
| `VersionName` | `string?` | 版本名称 |
| `Description` | `string?` | 更新描述 |
| `DownloadUrl` | `string` | APK 下载地址（必填） |
| `FileSize` | `long` | 文件大小（字节） |
| `Sha256` | `string` | APK SHA256 哈希（必填） |
| `FileName` | `string?` | 下载文件名 |
| `IsForced` | `bool` | 是否强制更新 |
| `PublishTime` | `DateTimeOffset?` | 发布时间 |

---

## Android Project 配置

两个组件都需要在 Android 项目中配置 `FileProvider`：

### AndroidManifest.xml

```xml
<application ...>
  <provider
    android:name="androidx.core.content.FileProvider"
    android:authorities="${applicationId}.generalupdate.fileprovider"
    android:exported="false"
    android:grantUriPermissions="true">
    <meta-data
      android:name="android.support.FILE_PROVIDER_PATHS"
      android:resource="@xml/generalupdate_file_paths" />
  </provider>
</application>
```

> 提示：如果应用已有 `FileProvider`，可复用现有的 authority。

### Resources/xml/generalupdate_file_paths.xml

```xml
<?xml version="1.0" encoding="utf-8"?>
<paths>
  <cache-path name="update_cache" path="update/" />
  <files-path name="update_files" path="update/" />
</paths>
```

---

## Maui vs Avalonia 对比

| 维度 | GeneralUpdate.Maui.Android | GeneralUpdate.Avalonia.Android |
|------|---------------------------|-------------------------------|
| 目标框架 | `net10.0-android` | `net8.0-android`（+ 更高版本） |
| DI 支持 | ✅ 内置 `AddGeneralUpdateMauiAndroid()` | 手动创建 |
| API 风格 | 两阶段：`ValidateAsync` + `ExecuteUpdateAsync`（合并下载+校验+安装） | 三阶段：`ValidateAsync` + `DownloadAndVerifyAsync` + `LaunchInstallerAsync` |
| UI 线程调度 | 通过 .NET MAUI 自动处理 | 需自行实现 `IUpdateEventDispatcher` |
| 断点续传元数据 | 内置 | Sidecar JSON 文件（`.json` 扩展名） |
| 速度平滑 | 默认 500ms 报告间隔 | 可配置平滑窗口（秒） |
| 状态快照 | — | `GetSnapshot()` 返回 `UpdateStateSnapshot` |
| 版本比较 | 内置 | 可替换 `IVersionComparer`（默认 `SystemVersionComparer`） |

### 选型建议

- **.NET MAUI 项目**：选择 `GeneralUpdate.Maui.Android`，享受 DI 集成和更简洁的两阶段 API
- **Avalonia 项目**：选择 `GeneralUpdate.Avalonia.Android`，具有更细粒度的三阶段控制和可替换策略
- **需要最大灵活性的场景**：Avalonia 的分离式 API 和可替换组件提供更多定制空间

---

## 服务端要求

两个组件都是**纯客户端库**，对服务端无强制要求。你需要提供一个可下载 APK 的 HTTP(S) 端点和一个返回以下信息的版本检查接口（自行实现）：

```json
{
  "version": "2.0.0",
  "versionName": "2.0",
  "releaseNotes": "性能和稳定性改进",
  "downloadUrl": "https://cdn.example.com/app-release.apk",
  "sha256": "3a0d2f...f9c2",
  "packageSize": 52428800,
  "forceUpdate": false
}
```

> 建议：可将版本信息托管在 OSS 静态文件服务器上，与 GeneralUpdate.Core 的 OSS 模式共用 `versions.json`。

---

## 常见问题

### 下载中断后能否恢复？

可以。两个组件都支持 HTTP Range 断点续传。Maui 使用单纯的 Range 请求恢复；Avalonia 额外使用 sidecar JSON 文件记录断点元数据。

### SHA256 校验失败怎么办？

组件会自动删除损坏的 APK 文件。Maui 由 `DeleteCorruptedPackageOnFailure` 控制（默认 `true`）；Avalonia 始终会丢弃校验失败的文件。校验失败后会抛出 `UpdateFailed` 事件。

### 如何实现强制更新？

在 `UpdatePackageInfo` 中设置 `ForceUpdate = true`（Maui）或 `IsForced = true`（Avalonia）。你的应用自行决定在收到强制更新标记后是否阻止用户继续使用旧版本。

### FileProvider authority 怎么填？

格式为 `{应用包名}.{任意后缀}`，例如 `com.example.app.generalupdate.fileprovider`。`AndroidManifest.xml` 中的 `authorities` 必须与代码中的 `FileProviderAuthority` 一致。

### 需要哪些 Android 权限？

```xml
<uses-permission android:name="android.permission.INTERNET" />
<uses-permission android:name="android.permission.REQUEST_INSTALL_PACKAGES" />
<uses-permission android:name="android.permission.WRITE_EXTERNAL_STORAGE" />
```

> Android 8.0+ 需要 `REQUEST_INSTALL_PACKAGES` 才能安装来自未知来源的 APK。

---

## 相关资源

- [GeneralUpdate.Maui 仓库](https://github.com/GeneralLibrary/GeneralUpdate.Maui)
- [GeneralUpdate.Avalonia 仓库](https://github.com/GeneralLibrary/GeneralUpdate.Avalonia)
- [GeneralUpdate.Core 组件文档](./GeneralUpdate.Core.md)
- [GeneralUpdate.Core 组件文档](./GeneralUpdate.Core.md)
- [入门实战手册](../quickstart/Beginner cookbook.md)
