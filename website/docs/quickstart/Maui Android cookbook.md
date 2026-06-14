---
sidebar_position: 5
title: MAUI Android 实战手册
---

# GeneralUpdate.Maui.Android 实战手册

这篇手册面向需要在 MAUI 应用中集成 Android APK 自动更新的开发者。目标是用最少的代码跑通"检查更新 → 下载 APK → 校验 SHA256 → 启动安装器"的完整流程。

:::info 前置知识
这篇手册假设你已经有一个 .NET MAUI 项目并配置好了 Android 目标框架。如果你还没有 MAUI Android 项目，请先参考 [.NET MAUI 官方文档](https://learn.microsoft.com/dotnet/maui/) 创建。
:::

## 更新流程

```
① 检查版本              ② 下载 APK             ③ 安装
┌──────────┐          ┌──────────┐          ┌──────────────┐
│  Client  │──POST──→ │  Server  │          │  Android     │
│  (MAUI)  │←─JSON─── │(更新服务)│          │  Package     │
└────┬─────┘          └────┬─────┘          │  Installer   │
     │                     │                └──────┬───────┘
     │  服务器返回包信息     │                       │
     │ ←────────────────────│                       │
     │                     │                        │
     │  GET /packages/app-v2.0.0.apk               │
     │ ────────────────────→│                       │
     │  APK 文件 (支持 Range)│                       │
     │ ←────────────────────│                       │
     │                     │                        │
     │  SHA256 校验通过后    │                       │
     │  启动 Package Installer ─────────────────→   │
     │                     │                        │
```

| 角色 | 定义 | 负责什么 |
| --- | --- | --- |
| Client（MAUI） | 你的 MAUI Android 应用 | 检查版本 → 下载 APK → 校验 → 启动安装器 |
| Server | 更新服务（如 Samples 中的 Server） | 返回版本信息、提供 APK 下载 |
| Android Package Installer | Android 系统组件 | 安装 APK（验证签名、替换应用） |

## Phase 1：环境准备

### 安装清单

| 项 | 要求 | 验证命令 |
| --- | --- | --- |
| .NET SDK | 10.0+ | `dotnet --version` |
| MAUI 工作负载 | 已安装 | `dotnet workload list`（应包含 `maui`） |

### NuGet 包

```bash
dotnet add package GeneralUpdate.Maui.Android
```

## Phase 2：FileProvider 配置

APK 文件需要通过 FileProvider 传递给 Android Package Installer。在 `Platforms/Android/AndroidManifest.xml` 中添加：

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

创建 `Platforms/Android/Resources/xml/file_paths.xml`：

```xml
<?xml version="1.0" encoding="utf-8"?>
<paths>
    <cache-path name="cache" path="update/" />
</paths>
```

## Phase 3：编写更新代码

### 方式一：DI 容器集成（推荐）

在 `MauiProgram.cs` 中注册服务：

```csharp
using GeneralUpdate.Maui.Android.Services;

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
```

在 ViewModel 中使用：

```csharp
using GeneralUpdate.Maui.Android.Abstractions;
using GeneralUpdate.Maui.Android.Models;

public class MainViewModel
{
    private readonly IAndroidBootstrap _bootstrap;

    public MainViewModel(IAndroidBootstrap bootstrap)
    {
        _bootstrap = bootstrap;

        _bootstrap.AddListenerDownloadProgressChanged += (_, args) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                OnProgressChanged?.Invoke(this, args.Statistics.ProgressPercentage);
            });
        };
    }

    public async Task CheckAndUpdateAsync()
    {
        var package = new UpdatePackageInfo
        {
            Version = "2.0.0",
            DownloadUrl = "http://10.0.2.2:5000/packages/app-v2.0.0.apk",
            Sha256 = "服务端返回的SHA256哈希值",
            PackageSize = 50_000_000,
            ApkFileName = "app-v2.0.0.apk"
        };

        var options = new UpdateOptions
        {
            CurrentVersion = "1.0.0",
            InstallOptions = new AndroidInstallOptions
            {
                FileProviderAuthority = "com.mycompany.myapp.fileprovider"
            }
        };

        // 检查更新
        var checkResult = await _bootstrap.ValidateAsync(package, options, CancellationToken.None);
        if (!checkResult.IsUpdateAvailable)
        {
            await Shell.Current.DisplayAlert("提示", "已是最新版本。", "确定");
            return;
        }

        // 执行更新（下载 → 校验 → 安装）
        var result = await _bootstrap.ExecuteUpdateAsync(package, options, CancellationToken.None);

        if (result.IsSuccess)
            await Shell.Current.DisplayAlert("更新", "安装器已启动，请在设备上确认安装。", "确定");
        else
            await Shell.Current.DisplayAlert("更新失败", result.Message, "确定");
    }

    public event EventHandler<double>? OnProgressChanged;
}
```

### 方式二：直接创建（无 DI）

```csharp
using GeneralUpdate.Maui.Android;
using GeneralUpdate.Maui.Android.Models;

var bootstrap = GeneralUpdateBootstrap.CreateDefault();

var package = new UpdatePackageInfo
{
    Version = "2.0.0",
    DownloadUrl = "http://10.0.2.2:5000/packages/app-v2.0.0.apk",
    Sha256 = "服务端返回的SHA256哈希值"
};

var options = new UpdateOptions
{
    CurrentVersion = "1.0.0",
    InstallOptions = new AndroidInstallOptions
    {
        FileProviderAuthority = "com.mycompany.myapp.fileprovider"
    }
};

// 二合一 API
var result = await bootstrap.ExecuteUpdateAsync(package, options, CancellationToken.None);
Console.WriteLine(result.IsSuccess ? "更新完成" : $"更新失败: {result.Message}");
```

## Phase 4：启动 Server

使用 Samples 仓库中的 Server：

```bash
git clone https://github.com/GeneralLibrary/GeneralUpdate-Samples.git
cd GeneralUpdate-Samples/src/Server
dotnet run
```

Server 默认在 `http://localhost:5000` 启动。Android 模拟器中请使用 `http://10.0.2.2:5000` 访问宿主机的 localhost。

## Phase 5：端到端验证

1. 将 APK 包放到 `Server/wwwroot/packages/` 目录
2. 更新 `wwwroot/packages/versions.json` 添加版本信息
3. 在模拟器或真机上运行 MAUI Android 应用
4. 观察应用输出：检查更新 → 下载 → 校验 → 启动安装器

### 预期输出

```
Checking for updates...
Update v2.0.0 found.
Downloading... 45% 1.2MB/s
Downloading... 100%
SHA256 verified.
Installer launched — complete install on device.
```

## 下一步

- [GeneralUpdate.Maui.Android 组件文档](../doc/GeneralUpdate.Maui.Android)：完整 API 参考、配置说明和高级用法
- [MAUI Android 更新示例](https://github.com/GeneralLibrary/GeneralUpdate-Samples/tree/main/UI/MauiUpdate)：完整的 MAUI Android 更新示例项目
