---
sidebar_position: 5
title: MAUI Android Cookbook
---

# GeneralUpdate.Maui.Android Cookbook

This guide is for developers who need to integrate Android APK auto-update into their .NET MAUI applications. The goal is to complete the "check update → download APK → verify SHA256 → launch installer" workflow with minimal code.

<iframe
  src="//player.bilibili.com/player.html?bvid=BV1ntJF6XEvG&page=1"
  width="100%"
  height="480"
  style={{ borderRadius: '8px', border: 'none' }}
  allowFullScreen
  scrolling="no"
/>

:::info Prerequisites
This guide assumes you already have a .NET MAUI project configured for Android target framework. If not, please refer to the [.NET MAUI documentation](https://learn.microsoft.com/dotnet/maui/) first.
:::

## Update Flow

```
① Version Check          ② Download APK          ③ Install
┌──────────┐          ┌──────────┐          ┌──────────────┐
│  Client  │──POST──→ │  Server  │          │  Android     │
│  (MAUI)  │←─JSON─── │(Update   │          │  Package     │
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
| MAUI workload | Installed | `dotnet workload list` (should include `maui`) |

### NuGet Package

```bash
dotnet add package GeneralUpdate.Maui.Android
```

## Phase 2: FileProvider Configuration

Add to `Platforms/Android/AndroidManifest.xml`:

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

Create `Platforms/Android/Resources/xml/file_paths.xml`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<paths>
    <cache-path name="cache" path="update/" />
</paths>
```

## Phase 3: Write Update Code

### Approach A: DI Container Integration (Recommended)

In `MauiProgram.cs`:

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

        // Register update service
        builder.Services.AddGeneralUpdateMauiAndroid();
        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddSingleton<MainPage>();

        return builder.Build();
    }
}
```

In ViewModel:

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
            Sha256 = "server-provided-sha256-hash",
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

        var checkResult = await _bootstrap.ValidateAsync(package, options, CancellationToken.None);
        if (!checkResult.IsUpdateAvailable)
        {
            await Shell.Current.DisplayAlert("Info", "Already up to date.", "OK");
            return;
        }

        var result = await _bootstrap.ExecuteUpdateAsync(package, options, CancellationToken.None);

        if (result.IsSuccess)
            await Shell.Current.DisplayAlert("Update", "Installer launched. Confirm on device.", "OK");
        else
            await Shell.Current.DisplayAlert("Update Failed", result.Message, "OK");
    }

    public event EventHandler<double>? OnProgressChanged;
}
```

### Approach B: Direct Creation (No DI)

```csharp
using GeneralUpdate.Maui.Android;
using GeneralUpdate.Maui.Android.Models;

var bootstrap = GeneralUpdateBootstrap.CreateDefault();

var package = new UpdatePackageInfo
{
    Version = "2.0.0",
    DownloadUrl = "http://10.0.2.2:5000/packages/app-v2.0.0.apk",
    Sha256 = "server-provided-sha256-hash"
};

var options = new UpdateOptions
{
    CurrentVersion = "1.0.0",
    InstallOptions = new AndroidInstallOptions
    {
        FileProviderAuthority = "com.mycompany.myapp.fileprovider"
    }
};

var result = await bootstrap.ExecuteUpdateAsync(package, options, CancellationToken.None);
Console.WriteLine(result.IsSuccess ? "Update completed." : $"Update failed: {result.Message}");
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
3. Run the MAUI Android app on emulator/device
4. Observe: Check → Download → Verify → Install

### Expected Output

```
Checking for updates...
Update v2.0.0 found.
Downloading... 45% 1.2MB/s
Downloading... 100%
SHA256 verified.
Installer launched — complete install on device.
```

## Next Steps

- [GeneralUpdate.Maui.Android Component Reference](../doc/GeneralUpdate.Maui.Android): Full API reference and advanced usage
- [MAUI Android Update Sample](https://github.com/GeneralLibrary/GeneralUpdate-Samples/tree/main/UI/MauiUpdate): Complete MAUI Android update sample project
