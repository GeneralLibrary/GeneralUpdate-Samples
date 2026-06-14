---
sidebar_position: 4
title: Avalonia Android Cookbook
---

# GeneralUpdate.Avalonia.Android Cookbook

This guide is for developers who need to integrate Android APK auto-update into their Avalonia applications. The goal is to complete the "check update → download APK → verify SHA256 → launch installer" workflow with minimal code.

<iframe
  src="//player.bilibili.com/player.html?bvid=BV1HtJF6XET3&page=1"
  width="100%"
  height="480"
  style={{ borderRadius: '8px', border: 'none' }}
  allowFullScreen
  scrolling="no"
/>

:::info Prerequisites
This guide assumes you already have an Avalonia project configured for Android target framework. If not, please refer to the [Avalonia documentation](https://docs.avaloniaui.net/) first.
:::

## Update Flow

```
① Version Check          ② Download APK          ③ Install
┌──────────┐          ┌──────────┐          ┌──────────────┐
│  Client  │──POST──→ │  Server  │          │  Android     │
│(Avalonia)│←─JSON─── │(Update   │          │  Package     │
└────┬─────┘          │ Service) │          │  Installer   │
     │                └────┬─────┘          └──────┬───────┘
     │  Server returns pkg info                    │
     │ ←────────────────────│                      │
     │                     │                       │
     │  GET /packages/app-v2.0.0.apk              │
     │ ────────────────────→│                      │
     │  APK file (Range supported)                │
     │ ←────────────────────│                      │
     │                     │                       │
     │  After SHA256 verification                  │
     │  launch Package Installer ────────────────→ │
```

## Phase 1: Environment Setup

### Requirements

| Item | Requirement | Verify |
| --- | --- | --- |
| .NET SDK | 10.0+ | `dotnet --version` |
| Android SDK | API 26+ | `dotnet workload list` (should include `android`) |

### NuGet Package

```bash
dotnet add package GeneralUpdate.Avalonia.Android
```

## Phase 2: FileProvider Configuration

Add to `AndroidManifest.xml`:

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

Create `Resources/xml/file_paths.xml`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<paths>
    <cache-path name="cache" path="update/" />
</paths>
```

## Phase 3: Write Update Code

```csharp
using GeneralUpdate.Avalonia.Android;
using GeneralUpdate.Avalonia.Android.Models;

public class UpdateViewModel
{
    private IAndroidBootstrap? _bootstrap;

    public async Task CheckAndUpdateAsync()
    {
        var options = new AndroidUpdateOptions
        {
            FileProviderAuthority = "com.myapp.fileprovider"
        };

        _bootstrap = GeneralUpdateBootstrap.CreateDefault(options);

        _bootstrap.AddListenerDownloadProgressChanged += (_, e) =>
        {
            Console.WriteLine($"Download: {e.ProgressPercentage:F1}%");
        };

        var package = new UpdatePackageInfo
        {
            Version = "2.0.0",
            DownloadUrl = "http://10.0.2.2:5000/packages/app-v2.0.0.apk",
            Sha256 = "server-provided-sha256-hash",
            FileSize = 50_000_000,
            FileName = "app-v2.0.0.apk"
        };

        var checkResult = await _bootstrap.ValidateAsync(package, "1.0.0");
        if (!checkResult.UpdateFound)
        {
            Console.WriteLine("Already up to date.");
            return;
        }

        var downloadResult = await _bootstrap.DownloadAndVerifyAsync(package);
        if (!downloadResult.Success)
        {
            Console.WriteLine($"Download failed: {downloadResult.Message}");
            return;
        }

        if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
        {
            var ctx = Android.App.Application.Context;
            var pm = ctx.PackageManager;
            if (pm != null && !pm.CanRequestPackageInstalls())
            {
                var intent = new Android.Content.Intent(
                    Android.Provider.Settings.ActionManageUnknownAppSources)
                    .SetData(Android.Net.Uri.Parse("package:" + ctx.PackageName));
                intent.AddFlags(Android.Content.ActivityFlags.NewTask);
                ctx.StartActivity(intent);
                return;
            }
        }

        var installResult = await _bootstrap.LaunchInstallerAsync(
            package, downloadResult.FilePath!);

        if (installResult.Success)
            Console.WriteLine("Installer launched. Confirm installation on device.");
    }
}
```

## Phase 4: Start the Server

```bash
git clone https://github.com/GeneralLibrary/GeneralUpdate-Samples.git
cd GeneralUpdate-Samples/src/Server
dotnet run
```

Server runs at `http://localhost:5000`. Use `http://10.0.2.2:5000` from Android emulator to access host localhost.

## Phase 5: End-to-End Verification

1. Place APK in `Server/wwwroot/packages/`
2. Update `wwwroot/packages/versions.json` with version info
3. Run the Avalonia Android app on emulator/device
4. Observe: Check → Download → Verify → Install

### Expected Output

```
Checking for updates...
Update v2.0.0 found.
Downloading... 45% 1.2MB/s
Downloading... 100%
SHA256 verified.
Installer launched.
```

## Next Steps

- [GeneralUpdate.Avalonia.Android Component Reference](../doc/GeneralUpdate.Avalonia.Android): Full API reference and advanced usage
- [Android Update Sample](https://github.com/GeneralLibrary/GeneralUpdate-Samples/tree/main/UI/AndroidUpdate): Complete Avalonia Android update sample project
