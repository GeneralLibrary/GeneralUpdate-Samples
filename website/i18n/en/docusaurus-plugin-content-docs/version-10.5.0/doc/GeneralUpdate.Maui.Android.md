---
sidebar_position: 10
---

# GeneralUpdate.Maui.Android

**Namespace:** `GeneralUpdate.Maui.Android` | **Main Entry:** `GeneralUpdateBootstrap.CreateDefault()` / `AddGeneralUpdateMauiAndroid()` | **NuGet Package:** `GeneralUpdate.Maui.Android`

## 1. Component Overview

### 1.1 Overview

**GeneralUpdate.Maui.Android** is an Android auto-update component designed for .NET MAUI applications. It provides version discovery, resumable APK download, SHA256 integrity verification, and system Package Installer trigger in a unified update orchestration API.

Unlike desktop file-replacement updates, Android APK updates must go through the system Package Installer. This component encapsulates this platform difference and provides a two-phase API: `ValidateAsync` for version checking and `ExecuteUpdateAsync` for the complete download→verify→install workflow.

**Core Capabilities:**

| Capability | Description |
| --- | --- |
| Version Check | Built-in `System.Version` parsing, auto-detects if target version is newer |
| Resumable APK Download | `HttpRangeDownloader` with HTTP Range headers and progress reporting |
| SHA256 Verification | Auto file hash verification, corrupted files auto-deleted on failure |
| System Installer Trigger | `AndroidApkInstaller` launches system Package Installer via FileProvider + Intent |
| DI Integration | `AddGeneralUpdateMauiAndroid()` extension registers all services in `IServiceCollection` |
| HTTP Transport Config | `HttpDownloadOptions` with SSL policies, proxy, timeout, auth, exponential backoff |
| Multi-Protocol Auth | HMAC-SHA256, Bearer Token, API Key, HTTP Basic |
| Event Notifications | Version discovery, download progress, completion stages, failure reasons |
| Concurrency Safety | `Interlocked` guard prevents concurrent `ExecuteUpdateAsync` calls |
| Temp File Management | `.downloading` temp extension, atomic rename on completion |

**Solved Business Pain Points:**
- MAUI Android apps need built-in auto-update capability outside Google Play
- Unstable networks during large APK downloads require resume support
- Downloaded APKs need integrity verification for security
- Deep DI container integration simplifies adoption in MAUI apps

:::info MAUI Update ≠ Desktop File Replacement
On Android, APKs must be installed via the Android Package Installer. The system handles signature verification and app replacement. This component handles the platform difference, but **APK signing and version management must still be handled in your build pipeline**.
:::

**Use Cases:**
- MAUI Android app auto-update (enterprise distribution, beta testing, side-loading)
- CDN / OSS distributed APK packages
- DI-managed MAUI applications

### 1.2 Environment & Dependencies

| Item | Description |
| --- | --- |
| **NuGet Package** | `GeneralUpdate.Maui.Android` |
| **Target Framework** | `net10.0` + `net10.0-android` (multi-target; Android min API 21) |
| **Dependencies** | `Microsoft.Extensions.DependencyInjection.Abstractions`, `Microsoft.Extensions.Http`, `Xamarin.AndroidX.Core` |
| **Platform Support** | Android 5.0 (API 21) and above |

---

## 2. Feature List

| Feature | Description | Type | Required | Notes |
| --- | --- | --- | --- | --- |
| Version Check | Compare current vs target version | Core | Yes | `ValidateAsync`, based on `System.Version` |
| Update Execution | Download→Verify→Install workflow | Core | Yes | `ExecuteUpdateAsync`, concurrency safe |
| APK Resume Download | HTTP Range resume + progress | Core | Auto | `HttpRangeDownloader` |
| SHA256 Verification | Auto-verify, delete on mismatch | Core | Auto | Via `IHashValidator` |
| APK Install Trigger | FileProvider + Intent to system installer | Core | Auto | `AndroidApkInstaller`, auto permission check |
| DI Integration | One-call service registration | Extension | Recommended | `AddGeneralUpdateMauiAndroid()` |
| HTTP Transport Config | SSL, proxy, timeout, retry, auth | Extension | Optional | `HttpDownloadOptions` |
| Multi-Protocol Auth | HMAC/Bearer/ApiKey/Basic | Extension | Optional | Global or per-package |
| Progress Notification | Bytes, speed, percentage, remaining | Core | Optional | `IProgress<DownloadStatistics>` |
| Completion Notification | Download, verification, install, workflow | Core | Optional | `AddListenerUpdateCompleted` |
| Failure Notification | Categorized failure reason | Core | Optional | `AddListenerUpdateFailed` |
| Validation Notification | Update found event | Core | Optional | `AddListenerValidate` |
| State Query | Real-time state query | Core | Optional | `CurrentState` property |
| Concurrency Guard | Prevent duplicate execution | Core | Auto | `Interlocked` atomic operation |
| Corrupt File Auto-Cleanup | Auto-delete on verification failure | Extension | Optional | `UpdateOptions.DeleteCorruptedPackageOnFailure` |
| Progress Interval Control | Custom progress callback frequency | Extension | Optional | `UpdateOptions.ProgressReportInterval` |
| Custom Downloader | Custom download implementation | Extension | Optional | Implement `IUpdateDownloader` |
| Custom Hash Validator | Custom hash implementation | Extension | Optional | Implement `IHashValidator` |
| Custom Installer | Custom install logic | Extension | Optional | Implement `IApkInstaller` |
| Custom Storage Provider | Custom file path/storage | Extension | Optional | Implement `IUpdateStorageProvider` |
| Custom Logger | Custom logging | Extension | Optional | Implement `IUpdateLogger` |
| Custom SSL Policy | Custom certificate validation | Extension | Optional | Implement `ISslValidationPolicy` |
| Custom HTTP Auth | Custom auth provider | Extension | Optional | Implement `IHttpAuthProvider` |

---

## 3. API Configuration

### 3.1 Properties

**UpdateOptions:**

| Field | Type | Default | Required | Description |
| --- | --- | --- | --- | --- |
| `CurrentVersion` | `string` | `""` | **Yes** | Current app version |
| `DownloadDirectory` | `string?` | `null` | Optional | Download directory, auto-selected if null |
| `TemporaryFileExtension` | `string` | `".downloading"` | Optional | Temp file extension |
| `DeleteCorruptedPackageOnFailure` | `bool` | `true` | Optional | Auto-delete corrupted packages |
| `ProgressReportInterval` | `TimeSpan` | `500ms` | Optional | Progress report interval |
| `InstallOptions` | `AndroidInstallOptions` | `new()` | Optional | Install options |

**AndroidInstallOptions:**

| Field | Type | Default | Required | Description |
| --- | --- | --- | --- | --- |
| `FileProviderAuthority` | `string` | `""` | **Yes** | FileProvider authority matching AndroidManifest |
| `MimeType` | `string` | `"application/vnd.android.package-archive"` | Optional | APK MIME type |

**HttpDownloadOptions:**

| Field | Type | Default | Required | Description |
| --- | --- | --- | --- | --- |
| `SslValidationPolicy` | `ISslValidationPolicy?` | `null` | Optional | Custom SSL validation |
| `DownloadTimeout` | `TimeSpan` | `10min` | Optional | Overall download timeout |
| `Proxy` | `IWebProxy?` | `null` | Optional | HTTP proxy |
| `UseProxy` | `bool` | `false` | Optional | Whether to use proxy |
| `AuthProvider` | `IHttpAuthProvider?` | `null` | Optional | Global auth provider |

**UpdatePackageInfo:**

| Field | Type | Default | Required | Description |
| --- | --- | --- | --- | --- |
| `Version` | `string` | `""` | **Yes** | Target version |
| `DownloadUrl` | `string` | `""` | **Yes** | APK download URL |
| `Sha256` | `string` | `""` | **Yes** | SHA256 hash |
| `PackageSize` | `long?` | `null` | Optional | Package size (bytes) |
| `ApkFileName` | `string?` | `null` | Optional | APK file name |
| `ForceUpdate` | `bool` | `false` | Optional | Force update |
| `VersionName` | `string?` | `null` | Optional | Display name |
| `ReleaseNotes` | `string?` | `null` | Optional | Release notes |
| `PublishTime` | `DateTimeOffset?` | `null` | Optional | Publish time |
| `AuthScheme` | `AuthScheme?` | `null` | Optional | Auth scheme |
| `AuthToken` | `string?` | `null` | Optional | Auth token |
| `AuthSecretKey` | `string?` | `null` | Optional | HMAC secret key |
| `BasicUsername` | `string?` | `null` | Optional | Basic username |
| `BasicPassword` | `string?` | `null` | Optional | Basic password |

**UpdateState Enum:**

| Value | Description |
| --- | --- |
| `None` | Idle |
| `Checking` | Checking for updates |
| `UpdateAvailable` | Update found |
| `Downloading` | Downloading |
| `Verifying` | Verifying integrity |
| `ReadyToInstall` | Ready to install |
| `Installing` | Installing |
| `Completed` | Completed |
| `Failed` | Failed |
| `Canceled` | Canceled |

**UpdateFailureReason Enum:**

| Value | Description |
| --- | --- |
| `Unknown` | Unknown error |
| `InvalidInput` | Invalid input parameters |
| `AlreadyInProgress` | Update already in progress |
| `Network` | Network error |
| `Download` | Download error |
| `FileAccess` | File access error |
| `IntegrityCheckFailed` | SHA256 verification failed |
| `InstallPermissionDenied` | Install permission denied |
| `Installation` | Installation error |
| `Canceled` | User canceled |
| `NoUpdateAvailable` | No update available |

**UpdateCompletionStage Enum:**

| Value | Description |
| --- | --- |
| `DownloadCompleted` | Download complete |
| `VerificationCompleted` | Verification complete |
| `InstallationTriggered` | Installer launched |
| `WorkflowCompleted` | Workflow complete |

### 3.2 Instance Methods

**GeneralUpdateBootstrap (Static Factory):**

| Method | Parameters | Use Case | Notes |
| --- | --- | --- | --- |
| `CreateDefault(HttpClient?, IUpdateLogger?, HttpDownloadOptions?)` | `httpClient` — optional external HttpClient; `logger` — optional logger; `httpOptions` — optional HTTP config | Create default bootstrap | When `httpOptions` is set, `httpClient` is ignored |
| `AddGeneralUpdateMauiAndroid(IServiceCollection, HttpClient?)` | `services` — DI container; `httpClient` — optional HttpClient | Register all update services in DI | Returns `IServiceCollection` for chaining |

**IAndroidBootstrap:**

| Method | Parameters | Use Case | Notes |
| --- | --- | --- | --- |
| `ValidateAsync(UpdatePackageInfo, UpdateOptions, CancellationToken)` | `packageInfo`, `options`, `ct` | Check for updates | Returns `UpdateCheckResult` with `IsUpdateAvailable` |
| `ExecuteUpdateAsync(UpdatePackageInfo, UpdateOptions, CancellationToken)` | `packageInfo`, `options`, `ct` | Execute full update workflow | Concurrency guard prevents duplicate calls |
| `Dispose()` | None | Release resources | — |
| `CurrentState` | Property (`UpdateState`) | Query current state | Thread-safe (`Volatile.Read`) |

### 3.3 Events

| Event | Args | Triggered | Usage |
| --- | --- | --- | --- |
| `AddListenerValidate` | `ValidateEventArgs` — `PackageInfo` | When update is found | UI display of new version info |
| `AddListenerDownloadProgressChanged` | `DownloadProgressChangedEventArgs` — `PackageInfo`, `Statistics`, `StatusDescription` | On download progress | Progress bar update |
| `AddListenerUpdateCompleted` | `UpdateCompletedEventArgs` — `PackageInfo`, `Stage`, `PackagePath` | On stage completion | Four stages: download, verification, install, workflow |
| `AddListenerUpdateFailed` | `UpdateFailedEventArgs` — `Reason`, `Message`, `Exception`, `PackageInfo` | On any failure | Structured failure reason |

---

## 4. Advanced Examples

### 4.1 Extensibility Overview

All services can be replaced via DI after `AddGeneralUpdateMauiAndroid()`:

| Interface | Default Implementation | Description |
| --- | --- | --- |
| `IUpdateDownloader` | `HttpRangeDownloader` | Resumable APK download |
| `IHashValidator` | `Sha256Validator` | SHA256 hash verification |
| `IApkInstaller` | `AndroidApkInstaller` | APK install trigger |
| `IUpdateStorageProvider` | `UpdateFileStore` | File path and storage operations |
| `IUpdateLogger` | `NullUpdateLogger` | Logger |
| `IUpdateBootstrap` | `AndroidBootstrap` | Update orchestration |
| `ISslValidationPolicy` | Via `HttpDownloadOptions` | SSL validation |
| `IHttpAuthProvider` | Via `HttpDownloadOptions` or per-package | HTTP auth provider |

### 4.2 Scenario Examples

#### Scenario 1: DI Container Integration

```csharp
using GeneralUpdate.Maui.Android.Abstractions;
using GeneralUpdate.Maui.Android.Models;
using GeneralUpdate.Maui.Android.Services;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts => { });

        builder.Services.AddGeneralUpdateMauiAndroid();
        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddSingleton<MainPage>();

        return builder.Build();
    }
}

public class MainViewModel
{
    private readonly IAndroidBootstrap _bootstrap;

    public MainViewModel(IAndroidBootstrap bootstrap)
    {
        _bootstrap = bootstrap;
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
}
```

#### Scenario 2: Custom HTTP Authentication

```csharp
using GeneralUpdate.Maui.Android.Models;
using GeneralUpdate.Maui.Android.Services;

// Global auth provider
var httpOptions = new HttpDownloadOptions
{
    AuthProvider = new BearerTokenAuthProvider("your-token-here")
};

var bootstrap = GeneralUpdateBootstrap.CreateDefault(
    httpClient: null, logger: null, httpOptions: httpOptions);

// Per-package auth
var package = new UpdatePackageInfo
{
    Version = "2.0.0",
    DownloadUrl = "https://example.com/app-v2.apk",
    Sha256 = "expected-sha256",
    AuthScheme = Enums.AuthScheme.Bearer,
    AuthToken = "your-token-here"
};
```

#### Scenario 3: Non-DI Direct Usage

```csharp
using GeneralUpdate.Maui.Android;
using GeneralUpdate.Maui.Android.Models;

var bootstrap = GeneralUpdateBootstrap.CreateDefault();

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
    Sha256 = "expected-sha256"
};

var result = await bootstrap.ExecuteUpdateAsync(package, options, CancellationToken.None);
```

---

## 5. Basic Usage Examples

### 5.1 Quick Start (Minimal Demo)

```csharp
using GeneralUpdate.Maui.Android;
using GeneralUpdate.Maui.Android.Models;

var bootstrap = GeneralUpdateBootstrap.CreateDefault();

var package = new UpdatePackageInfo
{
    Version = "2.0.0",
    DownloadUrl = "https://update.example.com/app-v2.0.0.apk",
    Sha256 = "a1b2c3d4e5f6..."
};

var options = new UpdateOptions
{
    CurrentVersion = "1.0.0",
    InstallOptions = new AndroidInstallOptions
    {
        FileProviderAuthority = "com.myapp.fileprovider"
    }
};

var checkResult = await bootstrap.ValidateAsync(package, options, CancellationToken.None);
if (!checkResult.IsUpdateAvailable)
{
    Console.WriteLine("Already up to date.");
    return;
}

var result = await bootstrap.ExecuteUpdateAsync(package, options, CancellationToken.None);
Console.WriteLine(result.IsSuccess ? "Update completed." : $"Update failed: {result.Message}");
```

### 5.2 Parameter Combination Example

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
    httpClient: null, logger: null, httpOptions: httpOptions);

bootstrap.AddListenerDownloadProgressChanged += (_, e) =>
{
    var s = e.Statistics;
    Console.WriteLine($"Progress: {s.ProgressPercentage:F1}%");
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
    ApkFileName = "app-v2.0.0.apk"
};

var result = await bootstrap.ExecuteUpdateAsync(package, options, CancellationToken.None);
```

### 5.3 Production Example

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

    public void Initialize(HttpDownloadOptions? httpOptions = null)
    {
        _bootstrap = GeneralUpdateBootstrap.CreateDefault(
            httpClient: null, logger: null, httpOptions: httpOptions);
        WireEvents();
    }

    public async Task<bool> CheckAndUpdateAsync(
        string serverUrl, string currentVersion, CancellationToken ct = default)
    {
        if (_bootstrap == null)
            throw new InvalidOperationException("Call Initialize() first.");

        try
        {
            var package = await FetchPackageFromServerAsync(serverUrl, currentVersion, ct);
            if (package == null) return true; // No update

            var options = new UpdateOptions
            {
                CurrentVersion = currentVersion,
                InstallOptions = new AndroidInstallOptions
                {
                    FileProviderAuthority = _fileProviderAuthority
                }
            };

            var checkResult = await _bootstrap.ValidateAsync(package, options, ct);
            if (!checkResult.IsUpdateAvailable) return true;

            var execResult = await _bootstrap.ExecuteUpdateAsync(package, options, ct);
            return execResult.IsSuccess;
        }
        catch (OperationCanceledException) { return false; }
        catch (HttpRequestException ex) { OnStatusChanged?.Invoke(this, $"Network: {ex.Message}"); return false; }
    }

    private Task<UpdatePackageInfo?> FetchPackageFromServerAsync(
        string serverUrl, string currentVersion, CancellationToken ct)
    {
        // Implementation: query server API for package metadata
        throw new NotImplementedException();
    }

    private void WireEvents() { /* wire bootstrap events */ }

    public event EventHandler<string>? OnStatusChanged;

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

### DI Registration

```csharp
// Default HttpClient
services.AddGeneralUpdateMauiAndroid();

// External HttpClient (shared pool)
services.AddGeneralUpdateMauiAndroid(myHttpClient);

// IHttpClientFactory
services.AddHttpClient<IUpdateDownloader, HttpRangeDownloader>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(10);
});
services.AddSingleton<IAndroidBootstrap, AndroidBootstrap>();
```

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
| Allow All (dev only) | `AllowAllSslValidationPolicy` | Development |
| Custom | Implement `ISslValidationPolicy` | Private CA |

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

`Platforms/Android/Resources/xml/file_paths.xml`:
```xml
<?xml version="1.0" encoding="utf-8"?>
<paths>
    <cache-path name="cache" path="update/" />
</paths>
```

### Platform Notes

| Item | Details |
| --- | --- |
| Min API | 21 (Android 5.0) |
| Install Permission | Android 8.0+ requires `CanRequestPackageInstalls()` check |
| FileProvider | Required to pass APK URI to system installer |

---

## Related Resources

- [GeneralUpdate.Maui Repository](https://github.com/GeneralLibrary/GeneralUpdate.Maui)
- [MAUI Android Update Sample](https://github.com/GeneralLibrary/GeneralUpdate-Samples/tree/main/UI/MauiUpdate)
- [FileProvider Documentation](https://developer.android.com/training/secure-file-sharing/setup-sharing)
