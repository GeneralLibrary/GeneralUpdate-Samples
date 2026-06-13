---
sidebar_position: 9
---

# GeneralUpdate.Avalonia.Android

**Namespace:** `GeneralUpdate.Avalonia.Android` | **Main Entry:** `GeneralUpdateBootstrap.CreateDefault()` | **NuGet Package:** `GeneralUpdate.Avalonia.Android`

## 1. Component Overview

### 1.1 Overview

**GeneralUpdate.Avalonia.Android** is an Android auto-update component designed for Avalonia applications. It provides version comparison, resumable APK download with HTTP Range support, SHA256 integrity verification, and Android Package Installer launch orchestration.

Unlike desktop applications where files can be directly replaced, Android APK installation must go through the system Package Installer. This component encapsulates these platform differences, allowing Avalonia developers to implement updates simply by providing package metadata.

**Core Capabilities:**

| Capability | Description |
| --- | --- |
| Version Comparison | Built-in `SystemVersionComparer` with custom strategy support via `IVersionComparer` |
| Resumable Download | `HttpResumableApkDownloader` with HTTP Range headers and progress reporting |
| SHA256 Verification | Automatic file hash verification against the server-provided hash on download completion |
| APK Installation | `AndroidApkInstaller` launches system Package Installer via FileProvider URI |
| HTTP Transport Config | `HttpDownloadOptions` supports SSL/TLS policies, proxy, timeout, retry, and authentication |
| Multi-Protocol Auth | HMAC-SHA256, Bearer Token, API Key, and HTTP Basic authentication |
| Event Notifications | Version discovery, download progress, completion, and failure events |
| State Snapshot | `GetSnapshot()` for current update state, failure reason, and message |
| UI Thread Dispatching | `IUpdateEventDispatcher` for dispatching events to UI thread |
| Progressive Download | Temporary `.part` files and Sidecar `.json` metadata for process-level resume |

**Solved Business Pain Points:**
- Avalonia apps on Android need reliable in-app updates, but APK installation must go through the system installer
- Large APK downloads can be interrupted — resume support reduces re-download traffic
- Downloaded APKs need integrity checks to prevent corruption or tampering
- Flexible authentication schemes are needed for various server requirements

:::info Android Update ≠ Desktop File Replacement
On Android, APKs must be installed via the Android Package Installer, which handles signature verification, permission granting, and app replacement. This component encapsulates these platform differences, but **APK signing and version incrementation must still be handled in your build pipeline**.
:::

**Use Cases:**
- Avalonia cross-platform app APK auto-update on Android
- Enterprise in-house Android app distribution
- CDN / OSS distributed APK packages

### 1.2 Environment & Dependencies

| Item | Description |
| --- | --- |
| **NuGet Package** | `GeneralUpdate.Avalonia.Android` |
| **Target Framework** | `net10.0-android` (min Android API 26+) |
| **Dependencies** | `Xamarin.AndroidX.Core` (FileProvider support) |
| **Platform Support** | Android 8.0 (API 26) and above |

---

## 2. Feature List

| Feature | Description | Type | Required | Notes |
| --- | --- | --- | --- | --- |
| Version Check | Compare current vs target version | Core | Yes | `ValidateAsync`, supports custom comparers |
| APK Resume Download | Resumable download with HTTP Range | Core | Yes | `DownloadAndVerifyAsync` |
| SHA256 Verification | Auto-verification on completion | Core | Auto | Via `IHashValidator` |
| APK Install Trigger | Launch system Package Installer | Core | Yes | `LaunchInstallerAsync` |
| HTTP Transport Config | SSL, proxy, timeout, retry, auth | Extension | Optional | `HttpDownloadOptions` |
| Multi-Protocol Auth | HMAC/Bearer/ApiKey/Basic | Extension | Optional | Global or per-package |
| Progress Notification | Speed, bytes, percentage | Core | Optional | `AddListenerDownloadProgressChanged` |
| Completion Notification | Version found, download done, install done | Core | Optional | `AddListenerUpdateCompleted` |
| Failure Notification | Failure reason, exception, package info | Core | Optional | `AddListenerUpdateFailed` |
| Validation Notification | Update available event | Core | Optional | `AddListenerValidate` |
| State Snapshot | Get current update state anytime | Extension | Optional | `GetSnapshot()` |
| UI Thread Dispatch | Custom event dispatch strategy | Extension | Optional | Implement `IUpdateEventDispatcher` |
| Custom Downloader | Custom download implementation | Extension | Optional | Implement `IUpdateDownloader` |
| Custom Hash Validator | Custom hash algorithm | Extension | Optional | Implement `IHashValidator` |
| Custom Version Comparer | Custom version comparison | Extension | Optional | Implement `IVersionComparer` |
| Custom APK Installer | Custom install implementation | Extension | Optional | Implement `IApkInstaller` |
| Custom File Storage | Custom file I/O | Extension | Optional | Implement `IFileStorage` |
| Custom SSL Policy | Custom certificate validation | Extension | Optional | Implement `ISslValidationPolicy` |
| Custom HTTP Auth | Custom request authentication | Extension | Optional | Implement `IHttpAuthProvider` |

---

## 3. API Configuration

### 3.1 Properties

**AndroidUpdateOptions:**

| Field | Type | Default | Required | Description |
| --- | --- | --- | --- | --- |
| `DownloadDirectoryPath` | `string` | `""` (auto: `CacheDir/update` or `TempPath/update`) | Optional | Download directory path |
| `FileProviderAuthority` | `string` | `""` | **Yes** | FileProvider authority matching AndroidManifest |
| `TemporaryFileExtension` | `string` | `".part"` | Optional | Temp file extension |
| `SidecarExtension` | `string` | `".json"` | Optional | Resume metadata file extension |
| `DownloadBufferSize` | `int` | `65536` (64 KB) | Optional | Download buffer size (bytes) |
| `SpeedSmoothingWindowSeconds` | `int` | `4` | Optional | Speed smoothing window (seconds) |

**HttpDownloadOptions:**

| Field | Type | Default | Required | Description |
| --- | --- | --- | --- | --- |
| `SslValidationPolicy` | `ISslValidationPolicy?` | `null` | Optional | Custom SSL validation; `null` uses system default |
| `RequestTimeout` | `TimeSpan` | `30s` | Optional | Per-request timeout |
| `DownloadTimeout` | `TimeSpan` | `10min` | Optional | Overall download timeout |
| `Proxy` | `IWebProxy?` | `null` | Optional | HTTP proxy |
| `UseProxy` | `bool` | `false` | Optional | Whether to use configured proxy |
| `MaxRetryAttempts` | `int` | `3` | Optional | Max retry count (3 = 1 initial + 2 retries) |
| `RetryBaseDelay` | `TimeSpan` | `1s` | Optional | Exponential backoff base: `baseDelay * 2^attempt` |
| `AuthProvider` | `IHttpAuthProvider?` | `null` | Optional | Global auth provider; per-package takes precedence |

**UpdatePackageInfo:**

| Field | Type | Default | Required | Description |
| --- | --- | --- | --- | --- |
| `Version` | `string` | — | **Yes** | Target version |
| `DownloadUrl` | `string` | — | **Yes** | APK download URL |
| `Sha256` | `string` | — | **Yes** | SHA256 hash for integrity check |
| `FileSize` | `long` | `0` | Optional | File size in bytes |
| `FileName` | `string?` | `null` | Optional | Downloaded file name |
| `IsForced` | `bool` | `false` | Optional | Force update |
| `VersionName` | `string?` | `null` | Optional | Display version name |
| `Description` | `string?` | `null` | Optional | Version description |
| `PublishTime` | `DateTimeOffset?` | `null` | Optional | Publish time |
| `AuthScheme` | `AuthScheme?` | `null` | Optional | Per-package auth scheme |
| `AuthToken` | `string?` | `null` | Optional | Bearer or API Key token |
| `AuthSecretKey` | `string?` | `null` | Optional | HMAC-SHA256 secret key |
| `BasicUsername` | `string?` | `null` | Optional | HTTP Basic username |
| `BasicPassword` | `string?` | `null` | Optional | HTTP Basic password |

**UpdateState Enum:**

| Value | Ordinal | Description |
| --- | --- | --- |
| `None` | 0 | Initial state |
| `Checking` | 1 | Checking for updates |
| `UpdateAvailable` | 2 | Update found |
| `Downloading` | 3 | Downloading |
| `Verifying` | 4 | Verifying integrity |
| `ReadyToInstall` | 5 | Ready to install |
| `Installing` | 6 | Installing |
| `Completed` | 7 | Update completed |
| `Failed` | 8 | Update failed |
| `Canceled` | 9 | Canceled |

**AuthScheme Enum:**

| Value | Description |
| --- | --- |
| `Hmac` | HMAC-SHA256 signature authentication |
| `Bearer` | Bearer Token authentication |
| `ApiKey` | API Key authentication |
| `Basic` | HTTP Basic authentication |

### 3.2 Instance Methods

**GeneralUpdateBootstrap (Static Factory):**

| Method | Parameters | Use Case | Notes |
| --- | --- | --- | --- |
| `CreateDefault(AndroidUpdateOptions, ...)` | `options` — update options; optional: `contextProvider`, `activityProvider`, `httpClient`, `versionComparer`, `eventDispatcher`, `logger`, `httpOptions` | Create default Android bootstrap instance | Null params use built-in defaults |

**IAndroidBootstrap:**

| Method | Parameters | Use Case | Notes |
| --- | --- | --- | --- |
| `ValidateAsync(UpdatePackageInfo, string, CancellationToken)` | `packageInfo`, `currentVersion`, `ct` | Check for updates | Returns `UpdateCheckResult` with `UpdateFound` and `TargetVersion` |
| `DownloadAndVerifyAsync(UpdatePackageInfo, CancellationToken)` | `packageInfo`, `ct` | Download APK and verify SHA256 | `FilePath` contains APK path on success |
| `LaunchInstallerAsync(UpdatePackageInfo, string, CancellationToken)` | `packageInfo`, `apkFilePath`, `ct` | Launch Android Package Installer | Requires correct FileProvider authority |
| `GetSnapshot()` | None | Get current state snapshot | Returns `UpdateStateSnapshot(State, FailureReason, Message)` |
| `Dispose()` | None | Release downloader resources | — |

### 3.3 Events

| Event | Args | Triggered | Usage |
| --- | --- | --- | --- |
| `AddListenerValidate` | `ValidateEventArgs` — `PackageInfo`, `CurrentVersion` | When update is found after version comparison | UI display of new version info |
| `AddListenerDownloadProgressChanged` | `DownloadProgressChangedEventArgs` — `ProgressPercentage`, `DownloadSpeedBytesPerSecond`, `DownloadedBytes`, `RemainingBytes`, `TotalBytes`, `PackageInfo`, `StatusDescription` | On download progress update | Progress bar display |
| `AddListenerUpdateCompleted` | `UpdateCompletedEventArgs` — `Result` (`UpdateOperationResult`) | On download or install completion | UI state transition |
| `AddListenerUpdateFailed` | `UpdateFailedEventArgs` — `Result` (`UpdateOperationResult`) | On any stage failure | Error display with reason and exception |

---

## 4. Advanced Examples

### 4.1 Extensibility Overview

All services can be replaced via `CreateDefault` optional parameters:

| Interface | Default Implementation | Description |
| --- | --- | --- |
| `IVersionComparer` | `SystemVersionComparer` | Version comparison strategy |
| `IUpdateDownloader` | `HttpResumableApkDownloader` | APK download |
| `IHashValidator` | `Sha256HashValidator` | SHA256 hash verification |
| `IApkInstaller` | `AndroidApkInstaller` | APK install trigger |
| `IFileStorage` | `PhysicalFileStorage` | File storage operations |
| `IUpdateEventDispatcher` | `ImmediateEventDispatcher` | Event dispatcher (direct invoke) |
| `IUpdateLogger` | `NoOpUpdateLogger` | Logger |
| `IAndroidContextProvider` | `DefaultAndroidContextProvider` | Android Context provider |
| `IAndroidActivityProvider` | `NullAndroidActivityProvider` | Android Activity provider |
| `ISslValidationPolicy` | Via `HttpDownloadOptions` | SSL validation policy |
| `IHttpAuthProvider` | Via `HttpDownloadOptions` or per-package | HTTP auth provider |

### 4.2 Scenario Examples

#### Scenario 1: Custom Version Comparer

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

// Usage
var bootstrap = GeneralUpdateBootstrap.CreateDefault(
    options,
    versionComparer: new CustomDateVersionComparer());
```

#### Scenario 2: Custom Event Dispatcher (Avalonia UI Thread)

```csharp
using GeneralUpdate.Avalonia.Android.Abstractions;

public sealed class AvaloniaEventDispatcher : IUpdateEventDispatcher
{
    public void Dispatch(Action callback)
    {
        if (Avalonia.Threading.Dispatcher.UIThread.CheckAccess())
            callback();
        else
            Avalonia.Threading.Dispatcher.UIThread.Post(callback);
    }
}

var bootstrap = GeneralUpdateBootstrap.CreateDefault(
    options,
    eventDispatcher: new AvaloniaEventDispatcher());
```

#### Scenario 3: Custom Downloader

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
        // Custom download and progress reporting...
    }
}
```

---

## 5. Basic Usage Examples

### 5.1 Quick Start (Minimal Demo)

```csharp
using GeneralUpdate.Avalonia.Android;
using GeneralUpdate.Avalonia.Android.Models;

var options = new AndroidUpdateOptions
{
    FileProviderAuthority = "com.myapp.fileprovider"
};

var bootstrap = GeneralUpdateBootstrap.CreateDefault(options);

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

var downloadResult = await bootstrap.DownloadAndVerifyAsync(updatePackage);
if (!downloadResult.Success)
{
    Console.WriteLine($"Download failed: {downloadResult.Message}");
    return;
}

var installResult = await bootstrap.LaunchInstallerAsync(
    updatePackage, downloadResult.FilePath!);

if (installResult.Success)
    Console.WriteLine("Installer launched.");
```

### 5.2 Parameter Combination Example

```csharp
using GeneralUpdate.Avalonia.Android;
using GeneralUpdate.Avalonia.Android.Abstractions;
using GeneralUpdate.Avalonia.Android.Models;
using GeneralUpdate.Avalonia.Android.Services;

var options = new AndroidUpdateOptions
{
    FileProviderAuthority = "com.myapp.fileprovider",
    DownloadBufferSize = 128 * 1024
};

var httpOptions = new HttpDownloadOptions
{
    RequestTimeout = TimeSpan.FromSeconds(60),
    DownloadTimeout = TimeSpan.FromMinutes(30),
    MaxRetryAttempts = 5,
    SslValidationPolicy = new AllowAllSslValidationPolicy()
};

var bootstrap = GeneralUpdateBootstrap.CreateDefault(
    options, httpOptions: httpOptions);

bootstrap.AddListenerDownloadProgressChanged += (_, e) =>
{
    Console.WriteLine($"Download: {e.ProgressPercentage:F1}% " +
        $"({e.DownloadedBytes}/{e.TotalBytes})");
};

var package = new UpdatePackageInfo
{
    Version = "2.0.0",
    DownloadUrl = "https://cdn.example.com/app-v2.0.0.apk",
    Sha256 = "expected-sha256-hash",
    FileSize = 50_000_000,
    FileName = "app-v2.0.0.apk"
};

var checkResult = await bootstrap.ValidateAsync(package, "1.0.0");
if (!checkResult.UpdateFound) return;

var downloadResult = await bootstrap.DownloadAndVerifyAsync(package);
if (!downloadResult.Success) return;

await bootstrap.LaunchInstallerAsync(package, downloadResult.FilePath!);
```

### 5.3 Production Example

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
            AuthProvider = new Services.BearerTokenAuthProvider(serverToken)
        };

        _bootstrap = GeneralUpdateBootstrap.CreateDefault(
            new AndroidUpdateOptions { FileProviderAuthority = fileProviderAuthority },
            httpOptions: httpOptions);

        WireEvents();
    }

    public async Task<bool> UpdateAsync(
        UpdatePackageInfo package, string currentVersion,
        CancellationToken ct = default)
    {
        if (_bootstrap == null)
            throw new InvalidOperationException("Not initialized.");

        StatusChanged?.Invoke(this, "Checking for updates...");
        var checkResult = await _bootstrap.ValidateAsync(package, currentVersion, ct);
        if (!checkResult.UpdateFound)
        {
            StatusChanged?.Invoke(this, "Already up to date.");
            return true;
        }

        StatusChanged?.Invoke(this, "Downloading...");
        var downloadResult = await _bootstrap.DownloadAndVerifyAsync(package, ct);
        if (!downloadResult.Success)
        {
            StatusChanged?.Invoke(this, $"Download failed: {downloadResult.Message}");
            return false;
        }

        if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
        {
            var ctx = Android.App.Application.Context;
            var pm = ctx.PackageManager;
            if (pm != null && !pm.CanRequestPackageInstalls())
            {
                StatusChanged?.Invoke(this, "Please allow install from unknown apps.");
                var intent = new Android.Content.Intent(
                    Android.Provider.Settings.ActionManageUnknownAppSources)
                    .SetData(Android.Net.Uri.Parse("package:" + ctx.PackageName));
                intent.AddFlags(Android.Content.ActivityFlags.NewTask);
                ctx.StartActivity(intent);
                return false;
            }
        }

        StatusChanged?.Invoke(this, "Installing...");
        var installResult = await _bootstrap.LaunchInstallerAsync(
            package, downloadResult.FilePath!, ct);

        if (installResult.Success)
        {
            StatusChanged?.Invoke(this, "Installer launched.");
            return true;
        }

        StatusChanged?.Invoke(this, $"Install failed: {installResult.Message}");
        return false;
    }

    private void WireEvents()
    {
        if (_bootstrap == null) return;
        _bootstrap.AddListenerDownloadProgressChanged += (_, e) =>
            ProgressChanged?.Invoke(this, e.ProgressPercentage);
        _bootstrap.AddListenerUpdateFailed += (_, e) =>
            StatusChanged?.Invoke(this, $"Failed: {e.Result.Message}");
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

## 6. Global Configuration

### Authentication

| Scheme | AuthScheme | Required Fields |
| --- | --- | --- |
| HMAC-SHA256 | `Hmac` | `AuthSecretKey` |
| Bearer Token | `Bearer` | `AuthToken` |
| API Key | `ApiKey` | `AuthToken` |
| HTTP Basic | `Basic` | `BasicUsername` + `BasicPassword` |

### SSL Policy

| Policy | Class | Use Case |
| --- | --- | --- |
| System Default | `null` | Production |
| Allow All (dev only) | `AllowAllSslValidationPolicy` | Development with self-signed certs |
| Custom | Implement `ISslValidationPolicy` | Private CA or certificate pinning |

### Resume Download

Downloader uses temporary files (`.part`) and metadata files (`.json`):

```
CacheDir/update/
├── app-v2.0.0.apk.part       # Partial download temp file
├── app-v2.0.0.apk.json       # Resume metadata
└── app-v2.0.0.apk            # Final file
```

### AndroidManifest

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

`Resources/xml/file_paths.xml`:
```xml
<?xml version="1.0" encoding="utf-8"?>
<paths>
    <cache-path name="cache" path="update/" />
</paths>
```

### Platform Notes

| Item | Details |
| --- | --- |
| Min API | 26 (Android 8.0) |
| Install Permission | Android 8.0+ requires `CanRequestPackageInstalls()` check |
| FileProvider | Required to pass APK URI to system installer |

---

## Related Resources

- [GeneralUpdate.Avalonia Repository](https://github.com/GeneralLibrary/GeneralUpdate.Avalonia)
- [Android Update Sample](https://github.com/GeneralLibrary/GeneralUpdate-Samples/tree/main/UI/AndroidUpdate)
- [FileProvider Documentation](https://developer.android.com/training/secure-file-sharing/setup-sharing)
