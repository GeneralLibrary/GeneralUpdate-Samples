---
sidebar_position: 4
title: Avalonia Android 实战手册
---

# GeneralUpdate.Avalonia.Android 实战手册

这篇手册面向需要在 Avalonia 应用中集成 Android APK 自动更新的开发者。目标是用最少的代码跑通"查询服务端 → 下载 APK → 校验 SHA256 → 拉起安装器 → 下次启动确认安装"的完整闭环。

<iframe
  src="//player.bilibili.com/player.html?bvid=BV1HtJF6XET3&page=1"
  width="100%"
  height="480"
  style={{ borderRadius: '8px', border: 'none' }}
  allowFullScreen
  scrolling="no"
/>

:::info 前置知识
这篇手册假设你已经有一个 Avalonia 项目并配置好了 Android 目标框架。如果你还没有 Avalonia Android 项目，请先参考 [Avalonia 官方文档](https://docs.avaloniaui.net/) 创建。
:::

## 更新流程

```
① 查询版本（组件内部完成）      ② 下载并校验 APK          ③ 安装与确认
┌──────────────┐             ┌──────────┐              ┌──────────────────┐
│   Client     │──POST──────→│  Server  │              │  Android         │
│ (Avalonia)   │←──JSON──────│(更新服务) │              │  Package         │
└──────┬───────┘             └────┬─────┘              │  Installer       │
       │                          │                    └────────┬─────────┘
       │  ValidateAsync(currentVersion) 由组件发起请求           │
       │  拿到 PackageInfo（version/url/hash/size…）             │
       │                          │                             │
       │  GET /app-v2.0.0.apk（支持 Range，可断点续传）           │
       │ ────────────────────────→│                             │
       │  APK 数据流 ←─────────────│                             │
       │                          │                             │
       │  文件大小 + SHA256 校验通过后写入安装记录                 │
       │  拉起 Package Installer ─────────────────────────────→  │
       │                          │                             │
       │  ★ 下次启动 CheckInstallationAsync 核对本机版本 ←───────│
       ▼                          ▼                             ▼
```

| 角色 | 定义 | 负责什么 |
| --- | --- | --- |
| Client（Avalonia） | 你的 Avalonia Android 应用 | 查询版本 → 下载 APK → 校验 → 拉起安装器 → 核对安装结果 |
| Server | 更新服务 | 返回版本信息、提供支持 Range 的 APK 下载 |
| Android Package Installer | Android 系统组件 | 安装 APK（验证签名、替换应用） |

## Phase 1：环境准备

### 安装清单

| 项 | 要求 | 验证命令 |
| --- | --- | --- |
| .NET SDK | 10.0+ | `dotnet --version` |
| Android workload | 已安装 | `dotnet workload list`（应包含 `android`） |
| Android SDK | API 26+ | Android Studio SDK Manager |
| Avalonia | 12+ | 项目引用的 Avalonia 包版本 |
| 目标框架 | `net10.0-android` | `.csproj` 的 `TargetFramework` |

### NuGet 包

```bash
dotnet add package GeneralUpdate.Avalonia.Android
```

## Phase 2：Android 接入配置

要真正走完一次更新，宿主必须配置好下面三项，缺一项就会停在半路。

### 2.1 声明安装权限（Android 8.0+）

在 `AndroidManifest.xml` 中：

```xml
<uses-permission android:name="android.permission.REQUEST_INSTALL_PACKAGES" />
```

用户可能仍未授予，此时 `LaunchInstallerAsync` 返回 `FailureReason = InstallPermissionDenied`。用下面的代码引导用户开启"允许安装未知应用"，授权后重试即可：

```csharp
var context = Android.App.Application.Context;
context.StartActivity(new Android.Content.Intent(
        Android.Provider.Settings.ActionManageUnknownAppSources,
        Android.Net.Uri.Parse("package:" + context.PackageName))
    .AddFlags(Android.Content.ActivityFlags.NewTask));
```

### 2.2 配置 FileProvider

APK 文件需要通过 FileProvider 传递给 Android Package Installer。`authorities` 必须与代码中的 `FileProviderAuthority` 完全一致：

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

创建 `Resources/xml/generalupdate_file_paths.xml`，paths 必须覆盖下载目录（默认 `<CacheDir>/update`）：

```xml
<?xml version="1.0" encoding="utf-8"?>
<paths>
    <cache-path name="update_cache" path="update/" />
    <files-path name="update_files" path="update/" />
</paths>
```

> **注意**：authority 或 paths 不一致时，`LaunchInstallerAsync` 返回 `InstallLaunchFailed`。

:::warning 明文 HTTP 仅限本地演示
组件默认拒绝下载非 HTTPS 的 APK（返回 `InvalidMetadata`）。仅在可信开发环境（如 `http://127.0.0.1:5080`）
才设置 `AllowInsecureHttpDownloads = true`，并同时给应用加 `android:usesCleartextTraffic="true"`。生产环境务必使用 HTTPS。
:::

## Phase 3：配置服务端查询

更新包信息不再由调用方构造，而是由组件按 `AndroidUpdateOptions.UpdateServer` 查询：

```csharp
var options = new AndroidUpdateOptions
{
    FileProviderAuthority = "com.example.app.generalupdate.fileprovider",
    DownloadDirectoryPath = Path.Combine(
        Android.App.Application.Context.CacheDir!.AbsolutePath!, "update"),

    UpdateServer = new UpdateServerOptions
    {
        RequestUrl = "https://example.com/Upgrade/Verification",
        AppKey = "your-app-key",
        AppType = 1,
        Platform = androidPlatformId,   // 必须与服务端配置一致，不要写死
        ProductId = "your-product-id"
    }
};
```

支持的两种协议：

| 模式 | 配置 | 说明 |
| --- | --- | --- |
| GeneralUpdate 验证协议（默认） | `UseJsonEndpoint = false` | `POST` 请求体 `version/appKey/appType/platform/productId`，响应 `{"code":200,"body":[...]}`；组件挑选最新非冻结的完整 APK |
| 静态 JSON 端点 | `UseJsonEndpoint = true` | `GET` 单个 `UpdatePackageInfo` JSON；HTTP 204 或 `null` 表示无更新 |

静态 JSON 端点示例：

```json
{
  "version": "2.3.0",
  "downloadUrl": "https://example.com/app-release.apk",
  "sha256": "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef",
  "description": "更新说明",
  "isForced": false
}
```

> `sha256` 必须是 APK 实际的 64 位十六进制 SHA-256（不能是 MD5）；`fileSize` 可省略或为 `0`。

如果你的服务端协议完全不同，实现 `IUpdatePackageSource.GetLatestAsync(currentVersion, ct)`，并通过
`CreateDefault(options, packageSource: mySource)` 注入即可，无需改造服务端。

## Phase 4：编写更新代码

### 4.1 一体化写法（推荐）

在 Avalonia ViewModel 或 Service 中：

```csharp
using GeneralUpdate.Avalonia.Android;
using GeneralUpdate.Avalonia.Android.Models;
using GeneralUpdate.Avalonia.Android.Services;

public sealed class UpdateService
{
    private IAndroidBootstrap? _bootstrap;

    public void Initialize(string fileProviderAuthority, string token)
    {
        var options = new AndroidUpdateOptions
        {
            FileProviderAuthority = fileProviderAuthority,
            DownloadDirectoryPath = Path.Combine(
                Android.App.Application.Context.CacheDir!.AbsolutePath!, "update"),
            UpdateServer = new UpdateServerOptions
            {
                RequestUrl = "https://example.com/Upgrade/Verification",
                AppKey = "your-app-key",
                Platform = androidPlatformId,
                ProductId = "your-product-id"
            }
        };

        _bootstrap = GeneralUpdateBootstrap.CreateDefault(
            options,
            httpOptions: new HttpDownloadOptions
            {
                AuthProvider = new BearerTokenAuthProvider(token),
                MaxRetryAttempts = 3,
                RetryBaseDelay = TimeSpan.FromSeconds(1)
            });

        _bootstrap.AddListenerDownloadProgressChanged += (_, e) =>
            Console.WriteLine($"Download: {e.ProgressPercentage:F1}% {e.StatusDescription}");

        _bootstrap.AddListenerUpdateFailed += (_, e) =>
            Console.WriteLine($"Failed: {e.Result.Message} ({e.Result.FailureReason})");

        _bootstrap.AddListenerInstallationConfirmed += (_, e) =>
            Console.WriteLine($"升级已确认：目标 {e.Result.Record?.TargetVersion}，当前 {e.Result.CurrentVersion}");
    }

    /// <summary>启动时或从安装器返回时调用：离线核对上次安装是否真正生效。</summary>
    public async Task CheckInstallationAsync()
    {
        if (_bootstrap is null) return;
        var installation = await _bootstrap.CheckInstallationAsync(ReadInstalledVersion());
        Console.WriteLine($"State={installation.State} Installed={installation.IsInstalled} " +
                          $"Pending={installation.HasPendingInstallation}");
    }

    /// <summary>检查并准备升级包；返回准备好的包与路径时，由调用方确认后再安装。</summary>
    public async Task<(UpdatePackageInfo Package, string FilePath)?> PrepareAsync()
    {
        if (_bootstrap is null) return null;

        var prepared = await _bootstrap.PrepareUpdateAsync(ReadInstalledVersion());
        if (!prepared.Success)
        {
            Console.WriteLine($"准备失败：{prepared.Message}");
            return null;
        }

        if (!prepared.UpdateFound)
        {
            Console.WriteLine("已是最新版本");
            return null;
        }

        // 权限检查（Android 8+），未授权时引导用户
        if (!HasInstallPermission())
        {
            OpenUnknownSourcesSettings();
            return null;
        }

        return prepared.IsReadyToInstall && prepared.PackageInfo is { } p && prepared.FilePath is { } path
            ? (p, path)
            : null;
    }

    public async Task InstallAsync(UpdatePackageInfo package, string filePath)
    {
        if (_bootstrap is null) return;
        var install = await _bootstrap.LaunchInstallerAsync(package, filePath);
        Console.WriteLine(install.Success
            ? "安装器已启动，请在设备上确认安装。"
            : $"安装拉起失败：{install.Message}");
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
}
```

`PrepareUpdateAsync` 在同一个操作锁内完成查询、版本比较、pre-check、下载与 SHA256 校验，不会打开安装器或权限页面。

### 4.2 分阶段写法（需要中间 UI 控制时）

```csharp
// ① 只查版本
var check = await bootstrap.ValidateAsync(currentVersion);
if (!check.UpdateFound || check.PackageInfo is null) return;
Console.WriteLine($"发现新版本 {check.TargetVersion}：{check.PackageInfo.Description}");

// ② 用户确认后下载并校验
var download = await bootstrap.DownloadAndVerifyAsync(check.PackageInfo);
if (!download.Success || download.FilePath is null) return;
Console.WriteLine("下载并校验完成，可以安装。");

// ③ 拉起安装器
await bootstrap.LaunchInstallerAsync(check.PackageInfo, download.FilePath);
```

### 4.3 更新前回调（业务拦截）

```csharp
bootstrap.AddListenerUpdatePrecheck(args =>
{
    // 非强制更新且不在 Wi-Fi 时跳过本次更新
    if (!IsWifiConnected())
    {
        Console.WriteLine($"等待 Wi-Fi 后再更新到 {args.PackageInfo.Version}");
        return true;    // true = 跳过，ValidateAsync 返回 UpdateFound = false
    }

    return false;       // false = 继续
});
```

强制更新（`UpdatePackageInfo.IsForced = true`）不会调用该回调。

## Phase 5：一键端到端演示

`GeneralUpdate.Avalonia` 仓库自带一个可运行的 Android 示例，连接一台已授权 USB 调试的设备或启动模拟器后执行：

```cmd
samples\GeneralUpdate.Avalonia.Android.Sample\run-demo.cmd
```

脚本会自动检查 .NET 10 与 Android workload、构建 `1.0.0` 与 `2.0.0` 两个 APK、启动实现
GeneralSpacestation 验证协议的本地演示服务（`http://127.0.0.1:5080`），并用 `adb reverse tcp:5080 tcp:5080`
映射端口，最后安装并启动 `1.0.0`。

演示服务的固定参数：

| 参数 | 值 |
| --- | --- |
| RequestUrl | `http://127.0.0.1:5080/Upgrade/Verification` |
| AppKey | `demo-client` |
| ProductId | `demo-product` |
| Platform | `3` |
| AppType | `1` |

如果你想对接 GeneralUpdate 示例服务端，也可以运行本仓库 Samples 的 Server：

```bash
git clone https://github.com/GeneralLibrary/GeneralUpdate-Samples.git
cd GeneralUpdate-Samples/src/Server
dotnet run
```

Server 默认在 `http://localhost:5000` 启动。Android 模拟器中请使用 `http://10.0.2.2:5000` 访问宿主机的 localhost。

## Phase 6：端到端验证

1. 在设备上点击"检查并自动升级"
2. 按系统提示允许"安装未知应用"
3. 在安装器中确认安装 `2.0.0`
4. 重新打开应用，确认"当前版本"已变为 `2.0.0`，并显示"升级已确认"

### 预期输出

```
Checking for updates...
Update v2.0.0 found.
Downloading 45% 1.2 MB/s
Downloading 100% Download completed
SHA256 verified.
Installer launched.
（安装完成后重新打开应用）
升级已确认：目标 2.0.0，当前 2.0.0
State=Installed Installed=True Pending=False
```

### 闭环验收清单

| 场景 | 预期表现 |
| --- | --- |
| 在系统安装器中取消安装 | 应用仍为旧版本，`HasPendingInstallation = true`，可重试 |
| 授权页返回后继续 | 服务端配置与升级目标保留，可重新触发下载与安装 |
| 安装完成后断网打开应用 | 本机版本核对仍能确认升级；服务端查询失败单独显示，不会覆盖已确认结果 |
| 重复打开已升级的应用 | 显示上次确认结果，但不会重复触发 `AddListenerInstallationConfirmed` |
| 安装记录损坏 | 明确报错并停止升级；由用户确认后调用 `ResetInstallationAsync()` 清除 |

### 常见问题排查

| 现象 | 原因 | 处理 |
| --- | --- | --- |
| `FailureReason = InstallPermissionDenied` | 未授予"安装未知应用"权限 | 用 `Settings.ActionManageUnknownAppSources` 引导用户授权后重试 |
| `FailureReason = InstallLaunchFailed` | FileProvider authority 或 paths 与下载目录不匹配 | 检查 `AndroidManifest.xml` 与 `generalupdate_file_paths.xml` |
| `FailureReason = InvalidMetadata` | 未配置 `UpdateServer`、元数据缺少 URL/SHA256、或 APK 下载地址不是 HTTPS | 检查服务端响应字段与 HTTPS 配置 |
| `FailureReason = NetworkError` | 查询或下载网络失败/超时 | 检查网络、`RequestTimeout` / `DownloadTimeout` 与重试配置 |
| `FailureReason = VersionComparisonFailed` | 版本号不是合法的 `System.Version` | 使用语义化版本，或替换 `IVersionComparer` |
| 校验失败 `HashMismatch` | 服务端 `sha256` 与 APK 实际哈希不一致 | 重新计算 APK 的 64 位十六进制 SHA-256 |
| 每次都要重新下载 | 服务端不支持 Range | 让服务端返回 `Accept-Ranges: bytes`，续传才能生效 |

:::caution 安装确认不等于安装成功
`LaunchInstallerAsync` 返回 `Success = true` 只表示安装器已拉起。只有 `CheckInstallationAsync` 返回
`IsInstalled` 才代表本机版本已达到目标。更新包必须保持同包名、兼容签名且 `versionCode` 递增；
组件不提供静默安装、安装后自动拉起应用、健康检查或自动回滚。
:::

## 下一步

- [GeneralUpdate.Avalonia.Android 组件文档](../doc/GeneralUpdate.Avalonia.Android)：完整 API 参考、配置说明和高级用法
- [Avalonia.Android 执行流程](../doc/Avalonia.Android-flow)：内部机制、断点续传与安装结果闭环详解
- [GeneralUpdate.Avalonia 仓库](https://github.com/GeneralLibrary/GeneralUpdate.Avalonia)：源码与示例工程
