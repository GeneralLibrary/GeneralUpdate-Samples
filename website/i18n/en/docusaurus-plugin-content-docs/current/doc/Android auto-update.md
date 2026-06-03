---
sidebar_position: 13
title: Android Auto-Update
---

# Android Auto-Update

## Overview

GeneralUpdate provides two independent auto-update components for .NET Android applications:

| Component | NuGet Package | Framework | Repository |
|-----------|---------------|-----------|------------|
| **GeneralUpdate.Maui.Android** | `GeneralUpdate.Maui.Android` | .NET MAUI (Android) | [GitHub](https://github.com/GeneralLibrary/GeneralUpdate.Maui) |
| **GeneralUpdate.Avalonia.Android** | `GeneralUpdate.Avalonia.Android` | Avalonia 12+ (Android) | [GitHub](https://github.com/GeneralLibrary/GeneralUpdate.Avalonia) |

Both libraries are **UI-less** Android auto-update cores focused on reusable update orchestration. They do not provide any UI — callers implement their own update prompt UI.

### Shared capabilities

- **Version validation**: compare current version against remote to decide whether to update
- **Resumable APK download**: HTTP Range-based resume support
- **SHA256 integrity verification**: automatic hash validation of downloaded files
- **Android installer triggering**: `Intent` + `FileProvider` to launch system installer
- **Event notifications**: download progress, completion, failure lifecycle events

### End-to-end update flow

```text
┌──────────────┐    ┌──────────────┐    ┌──────────────┐    ┌──────────────┐
│ 1. Validate  │ -> │ 2. Download  │ -> │ 3. Verify    │ -> │ 4. Install   │
│ ValidateAsync│    │ (resumable)  │    │ SHA256 check │    │ Intent call  │
└──────────────┘    └──────────────┘    └──────────────┘    └──────────────┘
```

---

## GeneralUpdate.Maui.Android

### Installation

Target frameworks: `net10.0; net10.0-android`

```bash
dotnet add package GeneralUpdate.Maui.Android
```

### Dependency Injection

```csharp
using GeneralUpdate.Maui.Android.Services;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddGeneralUpdateMauiAndroid();

using var provider = services.BuildServiceProvider();
var bootstrap = provider.GetRequiredService<IAndroidBootstrap>();
```

Direct creation is also supported:

```csharp
var bootstrap = GeneralUpdateBootstrap.CreateDefault();
```

### Quick start

```csharp
using GeneralUpdate.Maui.Android.Models;
using GeneralUpdate.Maui.Android.Services;

var bootstrap = GeneralUpdateBootstrap.CreateDefault();

// ── Event listeners ───────────────────────────
bootstrap.AddListenerValidate += (_, e) =>
{
    Console.WriteLine($"New version available: {e.PackageInfo.Version}");
};

bootstrap.AddListenerDownloadProgressChanged += (_, e) =>
{
    var s = e.Statistics;
    Console.WriteLine(
        $"{s.ProgressPercentage:F2}% | {s.DownloadedBytes}/{s.TotalBytes} | " +
        $"remaining: {s.RemainingBytes} | speed: {s.BytesPerSecond:F0} B/s");
};

bootstrap.AddListenerUpdateCompleted += (_, e) =>
{
    Console.WriteLine($"Stage={e.Stage}, File={e.PackagePath}");
};

bootstrap.AddListenerUpdateFailed += (_, e) =>
{
    Console.WriteLine($"Failed: {e.Reason}, {e.Message}");
};

// ── Build update package metadata ─────────────
var package = new UpdatePackageInfo
{
    Version = "2.0.0",
    VersionName = "2.0",
    ReleaseNotes = "Performance and stability improvements",
    DownloadUrl = "https://example.com/app-release.apk",
    Sha256 = "3A0D2F...F9C2",
    PackageSize = 52_428_800
};

// ── Execute update ────────────────────────────
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
    Console.WriteLine(result.IsSuccess ? "Update workflow completed." : $"Update failed: {result.Message}");
}
```

### Core API

#### IAndroidBootstrap

| Method | Description |
|--------|-------------|
| `ValidateAsync(package, options, ct)` | Validates whether the remote version is higher than current |
| `ExecuteUpdateAsync(package, options, ct)` | Executes the full update flow (download + verify + install) |

#### Events

| Event | When fired |
|-------|-----------|
| `AddListenerValidate` | A higher version is detected |
| `AddListenerDownloadProgressChanged` | Periodic download statistics (speed, downloaded, remaining, percentage) |
| `AddListenerUpdateCompleted` | Workflow milestones: `DownloadCompleted`, `VerificationCompleted`, `InstallationTriggered` |
| `AddListenerUpdateFailed` | Update failure with `UpdateFailureReason` and error message |

#### UpdateOptions

| Property | Type | Description |
|----------|------|-------------|
| `CurrentVersion` | `string` | Current application version |
| `DownloadDirectory` | `string?` | Download directory (optional, defaults to app cache) |
| `TemporaryFileExtension` | `string` | Temp file extension during download, default `.downloading` |
| `DeleteCorruptedPackageOnFailure` | `bool` | Whether to delete corrupted package on failure, default `true` |
| `ProgressReportInterval` | `TimeSpan` | Progress report interval, default 500ms |
| `InstallOptions` | `AndroidInstallOptions` | Android install options |

#### UpdatePackageInfo

| Property | Type | Description |
|----------|------|-------------|
| `Version` | `string` | Remote version (required) |
| `VersionName` | `string?` | Display version name |
| `ReleaseNotes` | `string?` | Release notes |
| `DownloadUrl` | `string` | APK download URL (required) |
| `Sha256` | `string` | APK SHA256 hash (required) |
| `PackageSize` | `long?` | Package size in bytes, used for progress estimation |
| `ForceUpdate` | `bool` | Whether this is a forced update |

---

## GeneralUpdate.Avalonia.Android

### Installation

Target framework: `net8.0-android` (compatible with `net9.0-android`+)

```bash
dotnet add package GeneralUpdate.Avalonia.Android
```

### Avalonia UI thread dispatching

Avalonia requires UI updates on the UI thread. Implement `IUpdateEventDispatcher` to marshal callbacks:

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

### Quick start

```csharp
using GeneralUpdate.Avalonia.Android;
using GeneralUpdate.Avalonia.Android.Abstractions;
using GeneralUpdate.Avalonia.Android.Models;

// ── Configure options ─────────────────────────
var options = new AndroidUpdateOptions
{
    DownloadDirectoryPath = Path.Combine(
        Android.App.Application.Context.CacheDir!.AbsolutePath!, "update"),
    FileProviderAuthority = "com.example.app.generalupdate.fileprovider"
};

var bootstrap = GeneralUpdateBootstrap.CreateDefault(
    options,
    eventDispatcher: new AvaloniaUiDispatcher());

// ── Event listeners ───────────────────────────
bootstrap.AddListenerValidate += (_, e) =>
{
    Console.WriteLine($"New version: {e.PackageInfo.Version} → current: {e.CurrentVersion}");
};

bootstrap.AddListenerDownloadProgressChanged += (_, e) =>
{
    var d = e.Download;
    Console.WriteLine($"{d.ProgressPercentage:F1}% | {d.Downloaded}/{d.Total} | speed: {d.Speed:F0} B/s");
};

bootstrap.AddListenerUpdateCompleted += (_, e) =>
{
    Console.WriteLine($"Completed: {e.Result.Stage}, file: {e.Result.FilePath}");
};

bootstrap.AddListenerUpdateFailed += (_, e) =>
{
    Console.WriteLine($"Failed: {e.Result.Reason}, {e.Result.Message}");
};

// ── Build update package metadata ─────────────
var packageInfo = new UpdatePackageInfo
{
    Version = "2.3.0",
    VersionName = "2.3",
    Description = "Added dark mode support and bug fixes",
    DownloadUrl = "https://example.com/app-release.apk",
    Sha256 = "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef",
    FileSize = 52_428_800,
    FileName = "app-release.apk",
    IsForced = false
};

// ── Three-phase update flow ───────────────────
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

### Core API

#### IAndroidBootstrap

Avalonia uses a **three-phase API** separating download/verify from install, giving callers more control:

| Method | Description |
|--------|-------------|
| `ValidateAsync(packageInfo, currentVersion, ct)` | Version check, returns `UpdateCheckResult` |
| `DownloadAndVerifyAsync(packageInfo, ct)` | Download APK and verify SHA256, returns `UpdateOperationResult` |
| `LaunchInstallerAsync(packageInfo, apkFilePath, ct)` | Trigger Android system installer |
| `GetSnapshot()` | Get current update state snapshot |

#### Events

| Event | Args | Description |
|-------|------|-------------|
| `AddListenerValidate` | `ValidateEventArgs` | Version validation result |
| `AddListenerDownloadProgressChanged` | `DownloadProgressChangedEventArgs` | Download progress (speed, downloaded, remaining, percentage, status) |
| `AddListenerUpdateCompleted` | `UpdateCompletedEventArgs` | Phase completion notification |
| `AddListenerUpdateFailed` | `UpdateFailedEventArgs` | Failure details |

#### AndroidUpdateOptions

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `DownloadDirectoryPath` | `string` | Empty (uses app cache) | APK download directory |
| `TemporaryFileExtension` | `string` | `.part` | Download temp file extension |
| `SidecarExtension` | `string` | `.json` | Resume sidecar metadata extension |
| `FileProviderAuthority` | `string` | Empty (required) | Android `FileProvider` authority |
| `DownloadBufferSize` | `int` | `64 * 1024` | Download buffer size |
| `SpeedSmoothingWindowSeconds` | `int` | `4` | Speed smoothing window in seconds |

#### UpdatePackageInfo

| Property | Type | Description |
|----------|------|-------------|
| `Version` | `string` | Remote version (required) |
| `VersionName` | `string?` | Display version name |
| `Description` | `string?` | Update description |
| `DownloadUrl` | `string` | APK download URL (required) |
| `FileSize` | `long` | File size in bytes |
| `Sha256` | `string` | APK SHA256 hash (required) |
| `FileName` | `string?` | Download file name |
| `IsForced` | `bool` | Whether this is a forced update |
| `PublishTime` | `DateTimeOffset?` | Publish timestamp |

---

## Android project configuration

Both components require `FileProvider` configuration in your Android project:

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

> Tip: if your app already has a `FileProvider`, you can reuse the existing authority.

### Resources/xml/generalupdate_file_paths.xml

```xml
<?xml version="1.0" encoding="utf-8"?>
<paths>
  <cache-path name="update_cache" path="update/" />
  <files-path name="update_files" path="update/" />
</paths>
```

---

## Maui vs Avalonia comparison

| Dimension | GeneralUpdate.Maui.Android | GeneralUpdate.Avalonia.Android |
|-----------|---------------------------|-------------------------------|
| Target framework | `net10.0-android` | `net8.0-android` (+ higher) |
| DI support | ✅ built-in `AddGeneralUpdateMauiAndroid()` | Manual creation |
| API style | Two-phase: `ValidateAsync` + `ExecuteUpdateAsync` (combined download+verify+install) | Three-phase: `ValidateAsync` + `DownloadAndVerifyAsync` + `LaunchInstallerAsync` |
| UI thread dispatch | Handled automatically by .NET MAUI | Requires custom `IUpdateEventDispatcher` |
| Resume metadata | Built-in | Sidecar JSON file (`.json` extension) |
| Speed smoothing | Default 500ms report interval | Configurable smoothing window (seconds) |
| State snapshot | — | `GetSnapshot()` returns `UpdateStateSnapshot` |
| Version comparison | Built-in | Replaceable `IVersionComparer` (default `SystemVersionComparer`) |

### Choosing between the two

- **.NET MAUI project**: use `GeneralUpdate.Maui.Android` for DI integration and a simpler two-phase API
- **Avalonia project**: use `GeneralUpdate.Avalonia.Android` for finer-grained three-phase control and replaceable strategies
- **Maximum flexibility**: Avalonia's separated API and replaceable components offer more customization

---

## Server requirements

Both libraries are **client-only** — no mandatory server-side dependencies. You need an HTTP(S) endpoint to serve the APK download and a version-check endpoint (implement it yourself) returning:

```json
{
  "version": "2.0.0",
  "versionName": "2.0",
  "releaseNotes": "Performance and stability improvements",
  "downloadUrl": "https://cdn.example.com/app-release.apk",
  "sha256": "3a0d2f...f9c2",
  "packageSize": 52428800,
  "forceUpdate": false
}
```

> Tip: host version metadata on an OSS static file server and share `versions.json` with GeneralUpdate.Core's OSS mode.

---

## FAQ

### Can downloads resume after interruption?

Yes. Both libraries support HTTP Range resume. Maui uses plain Range-request resume; Avalonia additionally uses a sidecar JSON file for resume metadata.

### What happens when SHA256 validation fails?

The corrupted APK is automatically deleted. Maui controls this via `DeleteCorruptedPackageOnFailure` (default `true`); Avalonia always discards failed files. An `UpdateFailed` event is raised on failure.

### How do I implement forced updates?

Set `ForceUpdate = true` (Maui) or `IsForced = true` (Avalonia) on `UpdatePackageInfo`. Your app decides whether to block users from continuing with the old version on a forced update.

### What should the FileProvider authority be?

Format: `{package-name}.{arbitrary-suffix}`, e.g. `com.example.app.generalupdate.fileprovider`. The `authorities` value in `AndroidManifest.xml` must match `FileProviderAuthority` in code.

### Which Android permissions are required?

```xml
<uses-permission android:name="android.permission.INTERNET" />
<uses-permission android:name="android.permission.REQUEST_INSTALL_PACKAGES" />
<uses-permission android:name="android.permission.WRITE_EXTERNAL_STORAGE" />
```

> Android 8.0+ requires `REQUEST_INSTALL_PACKAGES` to install APKs from unknown sources.

---

## Related resources

- [GeneralUpdate.Maui repository](https://github.com/GeneralLibrary/GeneralUpdate.Maui)
- [GeneralUpdate.Avalonia repository](https://github.com/GeneralLibrary/GeneralUpdate.Avalonia)
- [GeneralUpdate.Core component docs](./GeneralUpdate.Core.md)
- [GeneralUpdate.Core component docs](./GeneralUpdate.Core.md)
- [Beginner cookbook](../quickstart/Beginner cookbook.md)
