---
sidebar_position: 4
title: Avalonia Android 实战手册
---

# GeneralUpdate.Avalonia.Android 实战手册

这篇手册面向需要在 Avalonia 应用中集成 Android APK 自动更新的开发者。目标是用最少的代码跑通"检查更新 → 下载 APK → 校验 SHA256 → 启动安装器"的完整流程。

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
① 检查版本              ② 下载 APK             ③ 安装
┌──────────┐          ┌──────────┐          ┌──────────────┐
│  Client  │──POST──→ │  Server  │          │  Android     │
│(Avalonia)│←─JSON─── │(更新服务)│          │  Package     │
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
| Client（Avalonia） | 你的 Avalonia Android 应用 | 检查版本 → 下载 APK → 校验 → 启动安装器 |
| Server | 更新服务（如 Samples 中的 Server） | 返回版本信息、提供 APK 下载 |
| Android Package Installer | Android 系统组件 | 安装 APK（验证签名、替换应用） |

## Phase 1：环境准备

### 安装清单

| 项 | 要求 | 验证命令 |
| --- | --- | --- |
| .NET SDK | 10.0+ | `dotnet --version` |
| Android SDK | API 26+ | `dotnet workload list`（应包含 `android`） |

### NuGet 包

```bash
dotnet add package GeneralUpdate.Avalonia.Android
```

## Phase 2：FileProvider 配置

APK 文件需要通过 FileProvider 传递给 Android Package Installer。在 `AndroidManifest.xml` 中添加：

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

创建 `Resources/xml/file_paths.xml`：

```xml
<?xml version="1.0" encoding="utf-8"?>
<paths>
    <cache-path name="cache" path="update/" />
</paths>
```

> **注意**：`authorities` 中的 `${applicationId}.fileprovider` 和代码中传入的 `FileProviderAuthority` 必须一致。

## Phase 3：编写更新代码

在 Avalonia ViewModel 或 Service 中集成更新逻辑：

```csharp
using GeneralUpdate.Avalonia.Android;
using GeneralUpdate.Avalonia.Android.Models;
using GeneralUpdate.Avalonia.Android.Abstractions;

public class UpdateViewModel : ViewModelBase
{
    private IAndroidBootstrap? _bootstrap;

    public async Task CheckAndUpdateAsync()
    {
        // 1. 创建更新引导实例
        var options = new AndroidUpdateOptions
        {
            FileProviderAuthority = "com.myapp.fileprovider"
        };

        _bootstrap = GeneralUpdateBootstrap.CreateDefault(options);

        // 2. 订阅事件
        _bootstrap.AddListenerDownloadProgressChanged += (_, e) =>
        {
            // 更新 UI 进度条
            Console.WriteLine($"Download: {e.ProgressPercentage:F1}%");
        };

        // 3. 构造更新包信息（通常从服务端获取）
        var package = new UpdatePackageInfo
        {
            Version = "2.0.0",
            DownloadUrl = "http://10.0.2.2:5000/packages/app-v2.0.0.apk",
            Sha256 = "服务端返回的SHA256哈希值",
            FileSize = 50_000_000,
            FileName = "app-v2.0.0.apk"
        };

        // 4. 检查更新
        var checkResult = await _bootstrap.ValidateAsync(package, "1.0.0");
        if (!checkResult.UpdateFound)
        {
            Console.WriteLine("已是最新版本");
            return;
        }

        // 5. 下载并校验
        var downloadResult = await _bootstrap.DownloadAndVerifyAsync(package);
        if (!downloadResult.Success)
        {
            Console.WriteLine($"下载失败: {downloadResult.Message}");
            return;
        }

        // 6. 检查安装权限 (Android 8+)
        if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
        {
            var ctx = Android.App.Application.Context;
            var pm = ctx.PackageManager;
            if (pm != null && !pm.CanRequestPackageInstalls())
            {
                // 引导用户开启"安装未知应用"权限
                var intent = new Android.Content.Intent(
                    Android.Provider.Settings.ActionManageUnknownAppSources)
                    .SetData(Android.Net.Uri.Parse("package:" + ctx.PackageName));
                intent.AddFlags(Android.Content.ActivityFlags.NewTask);
                ctx.StartActivity(intent);
                return;
            }
        }

        // 7. 启动安装器
        var installResult = await _bootstrap.LaunchInstallerAsync(
            package, downloadResult.FilePath!);

        if (installResult.Success)
            Console.WriteLine("安装器已启动，请在设备上确认安装。");
    }
}
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
3. 在模拟器或真机上运行 Avalonia Android 应用
4. 观察控制台输出：检查更新 → 下载 → 校验 → 启动安装器

### 预期输出

```
Checking for updates...
Update v2.0.0 found.
Downloading... 45% 1.2MB/s
Downloading... 100%
SHA256 verified.
Installer launched.
```

## 下一步

- [GeneralUpdate.Avalonia.Android 组件文档](../doc/GeneralUpdate.Avalonia.Android)：完整 API 参考、配置说明和高级用法
- [Android 更新示例](https://github.com/GeneralLibrary/GeneralUpdate-Samples/tree/main/UI/AndroidUpdate)：完整的 Avalonia Android 更新示例项目
