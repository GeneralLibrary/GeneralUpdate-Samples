---
sidebar_position: 9
---

# GeneralUpdate.Avalonia.Android

**Namespace:** `GeneralUpdate.Avalonia.Android` | **Main Entry:** `GeneralUpdateBootstrap.CreateDefault()` | **NuGet Package:** `GeneralUpdate.Avalonia.Android`

## 1. Component Overview

### 1.1 Overview

**GeneralUpdate.Avalonia.Android** is a UI-free Android auto-update core library for Avalonia 12+ applications (`net10.0-android`), purpose-built for the Android APK update scenario. It provides server-queried version resolution, resumable APK download with HTTP Range support, SHA256 integrity verification, Android Package Installer handoff, and an **installation result loop** that confirms whether an update actually took effect.

Android updates differ from desktop updates: APK installation must go through the system installer, downloads can be interrupted or move between networks, "install unknown apps" permission must be checked before installing, and the app process is killed while the installer runs. This component encapsulates those platform differences and additionally solves the "installer launched ≠ installation succeeded" confirmation problem.

**Core Capabilities:**

| Capability | Description |
| --- | --- |
| Server version query | The component queries `UpdateServerOptions` itself; callers only supply the version installed on the device |
| Version Comparison | Built-in `SystemVersionComparer` with custom strategy support via `IVersionComparer` |
| One-shot preparation | `PrepareUpdateAsync` runs query → pre-check → download → verification under one operation lock |
| Resumable Download | `HttpResumableApkDownloader` uses HTTP Range plus sidecar metadata to resume after interruption |
| SHA256 Verification | Computes the file hash on completion, compares it with the declared hash and validates the file size |
| APK Installation | `AndroidApkInstaller` launches the system Package Installer via a FileProvider URI |
| Installation result loop | `CheckInstallationAsync` / `ResetInstallationAsync` reconcile a persisted record with the device's actual version |
| Update pre-check | `AddListenerUpdatePrecheck` hands update info to business logic before downloading and can skip the update |
| HTTP Transport Config | `HttpDownloadOptions` supports SSL/TLS policies, proxy, timeout, retry, and authentication |
| Multi-Protocol Auth | HMAC-SHA256, Bearer Token, API Key, and HTTP Basic at global or per-package scope |
| Event Notifications | Validation, download progress, completion, installation confirmation, and failure events |
| State Snapshot | `GetSnapshot()` for the current update state, failure reason, and message |
| UI Thread Dispatching | `IUpdateEventDispatcher` dispatches events to the UI thread (for example Avalonia's `Dispatcher.UIThread`) |
| Serialized operations | An internal `SemaphoreSlim(1,1)` gate queues concurrent calls; state access is thread safe |
| Replaceable abstractions | Version comparison, download, hash, install, storage, package source, event dispatching and logging are all replaceable |

**Solved Business Pain Points:**
- Avalonia apps on Android need reliable in-app updates, but APK installation must go through the system installer
- Callers should not have to re-implement "call the version API + pick the newest package + compare versions"
- Large APK downloads can be interrupted or switch networks — resume support reduces traffic
- Downloaded APKs need integrity checks to prevent corruption or tampering
- **The installer returning `Success` does not mean the user completed the installation** — cross-process confirmation is required
- Flexible authentication schemes are needed for various server requirements

:::info Android Update ≠ Desktop File Replacement
On Android, APKs must be installed via the Android Package Installer, which handles signature verification, permission granting, and app replacement. This component encapsulates those platform differences, but **APK signing, a stable package name and an increasing `versionCode` must still be handled in your build pipeline**. The component also provides no silent install, auto-restart or rollback.
:::

**Business Scenarios:**
- In-app Android APK updates for cross-platform Avalonia applications
- Staged, confirmable updates for enterprise Android apps
- Distributing APKs through CDN / OSS
- Integrating with the GeneralUpdate sample server `/Upgrade/Verification` protocol or a static JSON endpoint

### 1.2 Environment and Dependencies

| Item | Description |
| --- | --- |
| **NuGet Package** | `GeneralUpdate.Avalonia.Android` (repository version `v0.0.1-beta.10`) |
| **Target Framework** | `net10.0-android` (`SupportedOSPlatformVersion` = 26.0) |
| **.NET SDK** | 10.0+ |
| **Avalonia** | 12+ |
| **Dependencies** | `Xamarin.AndroidX.Core` (FileProvider support) |
| **Compatibility** | Android 8.0 (API 26) and above |

---

## 2. Feature List

| Feature | Description | Type | Required | Notes |
| --- | --- | --- | --- | --- |
| Server version query | Query the server configured by `UpdateServerOptions` and pick the newest eligible APK | Basic | Yes | `ValidateAsync(currentVersion)`; inject `IUpdatePackageSource` to replace it |
| Version comparison | Compare the current version with the target version | Basic | Yes | Custom `IVersionComparer` supported |
| One-shot preparation | Query, pre-check, download and verify in a single call | Extended | No | `PrepareUpdateAsync(currentVersion)` |
| Resumable APK download | Download the APK with HTTP Range resume | Basic | Yes | `DownloadAndVerifyAsync(packageInfo)` |
| SHA256 verification | Compute the SHA256 and compare it with the declared hash | Basic | Automatic | Implemented by `IHashValidator` |
| File size verification | Validate the actual file length when `FileSize > 0` | Basic | Automatic | Mismatch returns `FileIoError` and deletes the file |
| APK installer handoff | Launch the Android Package Installer | Basic | Yes | `LaunchInstallerAsync(packageInfo, apkFilePath)` |
| Installation reconciliation | Reconcile the persisted record with the device's actual version | Basic | Recommended | `CheckInstallationAsync(currentVersion)` |
| Installation reset | Explicitly clear the installation record (including corrupt data) | Extended | No | `ResetInstallationAsync()` |
| Update pre-check | Intercept the update before download and optionally skip it | Extended | No | `AddListenerUpdatePrecheck` |
| HTTP transport config | SSL validation, proxy, timeout, retry, authentication | Extended | No | `HttpDownloadOptions` |
| Multi-protocol auth | HMAC-SHA256 / Bearer / API Key / Basic | Extended | No | Global or per-package scope |
| Progress notification | Download progress (speed, bytes, percentage, status) | Basic | No | `AddListenerDownloadProgressChanged` |
| Completion notification | Download+verification finished / installer launched | Basic | No | `AddListenerUpdateCompleted` |
| Installation confirmation | Raised on the first durable confirmation | Basic | No | `AddListenerInstallationConfirmed` |
| Failure notification | Failure reason, exception, failing package info | Basic | No | `AddListenerUpdateFailed` |
| Validation notification | Raised when an update becomes available | Basic | No | `AddListenerValidate` |
| State snapshot | Read the current update state at any time | Extended | No | `GetSnapshot()` |
| UI thread dispatching | Custom event dispatching strategy | Extended | No | Implement `IUpdateEventDispatcher` |
| Custom package source | Custom version query protocol | Extended | No | Implement `IUpdatePackageSource` |
| Custom downloader | Custom download implementation | Extended | No | Implement `IUpdateDownloader` |
| Custom hash validation | Custom hash algorithm | Extended | No | Implement `IHashValidator` |
| Custom version comparison | Custom version comparison logic | Extended | No | Implement `IVersionComparer` |
| Custom APK installer | Custom install implementation | Extended | No | Implement `IApkInstaller` |
| Custom file storage | Custom file read/write implementation | Extended | No | Implement `IFileStorage` |
| Custom installation store | Custom installation record persistence | Extended | No | Implement `IInstallationStore` |
| Custom SSL policy | Custom HTTPS certificate validation | Extended | No | Implement `ISslValidationPolicy` |
| Custom HTTP authentication | Custom HTTP request authentication | Extended | No | Implement `IHttpAuthProvider` |

---

## 3. API Configuration

### 3.1 Configuration Options

**AndroidUpdateOptions:**

| Property | Type | Default | Required | Description |
| --- | --- | --- | --- | --- |
| `UpdateServer` | `UpdateServerOptions?` | `null` | Conditional | Server queried by `ValidateAsync`; when `null` and no custom package source is injected, validation fails with `InvalidMetadata` |
| `DownloadDirectoryPath` | `string` | `""` (falls back to `<CacheDir>/update` or `TempPath/update`) | No | Download directory; auto-selected when empty |
| `InstallationStateFilePath` | `string` | `""` (falls back to `<FilesDir>/update/installation.json`) | No | Installation record path. **Do not place it in the download cache**, which Android may clear |
| `TemporaryFileExtension` | `string` | `".part"` | No | Temporary download file extension |
| `SidecarExtension` | `string` | `".json"` | No | Resume metadata file extension |
| `FileProviderAuthority` | `string` | `""` | Conditional | Android FileProvider authority; must match AndroidManifest. Launching the installer with an empty value returns `InvalidMetadata` |
| `AllowInsecureHttpDownloads` | `bool` | `false` | No | Allow plaintext HTTP APK downloads for trusted development only; global auth is never sent over HTTP even when enabled |
| `Language` | `UpdateLanguage` | `English` | No | Language of built-in messages (`English` / `Chinese`) |
| `DownloadBufferSize` | `int` | `65536` (64 KB) | No | Download buffer size in bytes; must be greater than 0 |
| `SpeedSmoothingWindowSeconds` | `int` | `4` | No | Sliding window used for download speed smoothing |

**UpdateServerOptions:**

| Property | Type | Default | Required | Description |
| --- | --- | --- | --- | --- |
| `RequestUrl` | `string` | — | **Yes** | Absolute HTTP(S) URL. Under the default protocol this is the verification endpoint (e.g. `https://example.com/Upgrade/Verification`) |
| `AppKey` | `string` | `""` | No | Application key sent in the verification body (request field only; it does not enable HMAC signing) |
| `AppType` | `int` | `1` | No | Application type sent in the verification body |
| `Platform` | `int` | `0` | Conditional | Android platform identifier configured on the server; **must match your deployment** |
| `ProductId` | `string` | `""` | No | Product identifier sent in the verification body |
| `UseJsonEndpoint` | `bool` | `false` | No | GET a single `UpdatePackageInfo` JSON instead of POSTing the verification protocol |

**HttpDownloadOptions:**

| Property | Type | Default | Required | Description |
| --- | --- | --- | --- | --- |
| `SslValidationPolicy` | `ISslValidationPolicy?` | `null` | No | Custom SSL validation policy; `null` uses the system default. With an external `HttpClient`, configure it on its handler |
| `RequestTimeout` | `TimeSpan` | `30s` | No | Timeout for verification requests and download HEAD probes |
| `DownloadTimeout` | `TimeSpan` | `10min` | No | Overall timeout for the whole download, including retry backoff |
| `Proxy` | `IWebProxy?` | `null` | No | HTTP proxy; requires `UseProxy = true` |
| `UseProxy` | `bool` | `false` | No | Whether to use the configured proxy |
| `MaxRetryAttempts` | `int` | `3` | No | Maximum download attempts for transient HEAD/GET/response-stream failures (3 = 1 initial + 2 retries); metadata queries are not retried |
| `RetryBaseDelay` | `TimeSpan` | `1s` | No | Exponential backoff base: `min(30s, baseDelay * 2^attempt)` |
| `AuthProvider` | `IHttpAuthProvider?` | `null` | No | Global authentication; only sent to HTTPS download URLs on the same origin as the verification endpoint. Per-package auth takes precedence |

**UpdatePackageInfo:**

| Property | Type | Default | Required | Description |
| --- | --- | --- | --- | --- |
| `Version` | `string` | — | **Yes** | Target version |
| `DownloadUrl` | `string` | — | **Yes** | APK download URL (HTTPS required by default) |
| `Sha256` | `string` | — | **Yes** | 64-character hexadecimal SHA256 checksum |
| `FileSize` | `long` | `0` | No | APK size in bytes; `0` means unknown, otherwise it is verified |
| `FileName` | `string?` | `null` | No | File name of the downloaded APK |
| `IsForced` | `bool` | `false` | No | Forced update; when `true` the pre-check callback is not invoked |
| `VersionName` | `string?` | `null` | No | Display name of the version |
| `Description` | `string?` | `null` | No | Release notes |
| `PublishTime` | `DateTimeOffset?` | `null` | No | Publish time |
| `AuthScheme` | `AuthScheme?` | `null` | No | Per-package auth scheme, taking precedence over the global configuration |
| `AuthToken` | `string?` | `null` | No | Bearer or API Key token |
| `AuthSecretKey` | `string?` | `null` | No | HMAC-SHA256 secret key |
| `BasicUsername` | `string?` | `null` | No | HTTP Basic user name |
| `BasicPassword` | `string?` | `null` | No | HTTP Basic password |

**UpdateState enum:**

| Value | Number | Description |
| --- | --- | --- |
| `None` | 0 | Initial state / no installation record |
| `Checking` | 1 | Querying the update server |
| `UpdateAvailable` | 2 | An update is available |
| `Downloading` | 3 | Downloading the package |
| `Verifying` | 4 | Verifying the package |
| `ReadyToInstall` | 5 | Downloaded and verified, ready to install |
| `Installing` | 6 | Installer intent launched |
| `Completed` | 7 | Flow completed (also used when there is no update or the pre-check skipped it) |
| `Failed` | 8 | Update failed |
| `Canceled` | 9 | Canceled |
| `InstallationPending` | 10 | An installation record exists but the install is not confirmed yet |
| `Installed` | 11 | The device reached or exceeded the installation target |

**UpdateFailureReason enum:**

| Value | Number | Description |
| --- | --- | --- |
| `None` | 0 | No failure |
| `NetworkError` | 1 | Network error or timeout |
| `Canceled` | 2 | Caller cancellation |
| `InvalidMetadata` | 3 | Invalid metadata (missing URL/SHA256, no package source, no FileProvider authority, …) |
| `FileIoError` | 4 | File I/O error (including size mismatch and installation record write failure) |
| `HashMismatch` | 5 | SHA256 verification failed |
| `ServerDoesNotSupportRange` | 6 | The server does not support Range resume |
| `InstallPermissionDenied` | 7 | Missing "install unknown apps" permission |
| `InstallLaunchFailed` | 8 | Failed to launch the installer intent |
| `VersionComparisonFailed` | 9 | Version comparison failed |
| `Unknown` | 10 | Unknown reason |

**AuthScheme enum:**

| Value | Description |
| --- | --- |
| `Hmac` | HMAC-SHA256 signature auth (`X-Update-Timestamp` + `X-Update-Signature`) |
| `Bearer` | Bearer token auth |
| `ApiKey` | API key auth (default header `X-Api-Key`) |
| `Basic` | HTTP Basic auth |

**UpdateLanguage enum:**

| Value | Description |
| --- | --- |
| `English` | Built-in messages in English (default) |
| `Chinese` | Built-in messages in Simplified Chinese |

### 3.2 Result Model Hierarchy

```
UpdateOperationResult (base)
├── Success, State, FailureReason, Message, PackageInfo, FilePath, Exception
├── UpdateCheckResult        → + UpdateFound, CurrentVersion, TargetVersion
├── UpdatePreparationResult  → + UpdateFound, IsReadyToInstall
├── InstallationCheckResult  → + CurrentVersion, Record, IsInstalled, HasPendingInstallation
├── DownloadResult
├── HashValidationResult     → + ActualSha256, ExpectedSha256
└── InstallResult
```

| Model | Key members | Description |
| --- | --- | --- |
| `UpdateCheckResult` | `UpdateFound` / `CurrentVersion` / `TargetVersion` | Version validation result; `PackageInfo` is the package metadata returned by the server |
| `UpdatePreparationResult` | `IsReadyToInstall` | One-shot result; `true` only when `Success && State == ReadyToInstall && PackageInfo != null && FilePath != null` |
| `InstallationCheckResult` | `Record` / `IsInstalled` / `HasPendingInstallation` | Installation reconciliation result |
| `InstallationRecord` | `TargetVersion` / `RequestedAt` / `InstalledVersion` / `ConfirmedAt` | Persisted installation attempt; contains no download credentials |

### 3.3 Instance Methods

**GeneralUpdateBootstrap (static factory):**

| Method | Parameters | Usage | Notes |
| --- | --- | --- | --- |
| `CreateDefault(options, ...)` | `options` — update options; optional: `contextProvider`, `activityProvider`, `httpClient`, `versionComparer`, `eventDispatcher`, `logger`, `httpOptions`, `packageSource`, `installationStore` | Create the default Android update bootstrap | All optional parameters fall back to built-in defaults; `DownloadBufferSize <= 0` throws `ArgumentOutOfRangeException` |

Default injection chain:

| Abstraction | Default implementation |
| --- | --- |
| `IAndroidContextProvider` | `DefaultAndroidContextProvider` |
| `IAndroidActivityProvider` | `NullAndroidActivityProvider` |
| `IUpdateLogger` | `NoOpUpdateLogger` |
| `IFileStorage` | `PhysicalFileStorage` |
| `IUpdateDownloader` | `HttpResumableApkDownloader` |
| `IHashValidator` | `Sha256HashValidator` |
| `IApkInstaller` | `AndroidApkInstaller` |
| `IVersionComparer` | `SystemVersionComparer` |
| `IUpdatePackageSource` | `HttpUpdatePackageClient` |
| `IInstallationStore` | `JsonFileInstallationStore` |
| `IUpdateEventDispatcher` | `ImmediateEventDispatcher` |

**IAndroidBootstrap:**

| Method | Parameters | Returns | Usage | Notes |
| --- | --- | --- | --- | --- |
| `PrepareUpdateAsync(currentVersion, ct)` | `currentVersion` — device version; `ct` — cancellation token | `UpdatePreparationResult` | Query, compare, pre-check, download and verify in one call | Runs under one operation lock; `Success = true` with `IsReadyToInstall = false` means no update or skipped; **never** opens the installer or a permission page |
| `ValidateAsync(currentVersion, ct)` | `currentVersion` — device version; `ct` — cancellation token | `UpdateCheckResult` | Check the version only, or drive the phased flow | Package metadata comes from `AndroidUpdateOptions.UpdateServer` or an injected package source; callers no longer build `UpdatePackageInfo`. The returned `PackageInfo` feeds the following methods |
| `DownloadAndVerifyAsync(packageInfo, ct)` | `packageInfo` — package metadata; `ct` — cancellation token | `UpdateOperationResult` | Download the APK and verify size and SHA256 | On success `State = ReadyToInstall` and `FilePath` points at the APK |
| `LaunchInstallerAsync(packageInfo, apkFilePath, ct)` | `packageInfo`, `apkFilePath`, `ct` | `InstallResult` | Persist the installation record and launch the system installer | The record is written atomically before launching; a write failure returns `FileIoError` without launching. `Success = true` only means the installer was launched |
| `CheckInstallationAsync(currentVersion, ct)` | `currentVersion` — version read from PackageManager; `ct` | `InstallationCheckResult` | Reconcile the installation result at startup or on return from the installer | Works offline; no record returns `Success && State == None`; a corrupt record is reported explicitly instead of pretending there is no record |
| `ResetInstallationAsync(ct)` | `ct` | `UpdateOperationResult` | Explicitly clear the installation record (including corrupt data) | Only clears the record; does not modify the installed app, downloaded files or server configuration |
| `AddListenerUpdatePrecheck(func)` | `func` — `Func<UpdateInfoEventArgs, bool>` | `IAndroidBootstrap` | Register the update pre-check callback | `true` skips, `false` continues; forced updates bypass it; chainable |
| `GetSnapshot()` | none | `UpdateStateSnapshot` | Read the current state snapshot | Returns `(State, FailureReason, Message)` |
| `Dispose()` | none | `void` | Release downloader / package source resources | Cancel and await active work before disposing; disposing early rejects new and queued calls and defers resource release until the active operation exits |

### 3.4 Events

| Event | EventArgs | Raised when | Usage |
| --- | --- | --- | --- |
| `AddListenerValidate` | `ValidateEventArgs` — `PackageInfo`, `CurrentVersion` | Validation found an available update | Show the new version in the UI; not raised when the pre-check skips the update |
| `AddListenerDownloadProgressChanged` | `DownloadProgressChangedEventArgs` — `ProgressPercentage`, `DownloadSpeedBytesPerSecond`, `DownloadedBytes`, `RemainingBytes`, `TotalBytes`, `PackageInfo`, `StatusDescription` | Download progress updates | Drive a progress bar |
| `AddListenerUpdateCompleted` | `UpdateCompletedEventArgs` — `Result` (`UpdateOperationResult`) | Download+verification finished (`ReadyToInstall`) or the installer was launched (`Installing`) | **Not proof of a completed installation**, only of a finished phase |
| `AddListenerInstallationConfirmed` | `InstallationConfirmedEventArgs` — `Result` (`InstallationCheckResult`) | `CheckInstallationAsync` first durably confirms the target version is installed | Raised only once; repeated checks or restarts do not re-raise it; not a reliable message queue |
| `AddListenerUpdateFailed` | `UpdateFailedEventArgs` — `Result` (`UpdateOperationResult`) | Any step fails | Carries the failure reason and exception |

---

## 4. Extension Samples (Advanced Usage)

### 4.1 Extensible Capabilities at a Glance

Every service can be replaced through the optional parameters of `CreateDefault`:

| Interface | Default implementation | Description |
| --- | --- | --- |
| `IVersionComparer` | `SystemVersionComparer` | Version comparison strategy |
| `IUpdatePackageSource` | `HttpUpdatePackageClient` | Where package metadata comes from (protocol / custom API) |
| `IUpdateDownloader` | `HttpResumableApkDownloader` | APK download implementation |
| `IHashValidator` | `Sha256HashValidator` | SHA256 verification |
| `IApkInstaller` | `AndroidApkInstaller` | APK installer handoff |
| `IFileStorage` | `PhysicalFileStorage` | File system operations |
| `IInstallationStore` | `JsonFileInstallationStore` | Installation record persistence |
| `IUpdateEventDispatcher` | `ImmediateEventDispatcher` | Event dispatcher (invokes inline) |
| `IUpdateLogger` | `NoOpUpdateLogger` | Logger |
| `IAndroidContextProvider` | `DefaultAndroidContextProvider` | Android Context provider |
| `IAndroidActivityProvider` | `NullAndroidActivityProvider` | Android Activity provider |
| `ISslValidationPolicy` | Configured through `HttpDownloadOptions` | SSL certificate validation policy |
| `IHttpAuthProvider` | `HttpDownloadOptions` or per-package config | HTTP authentication provider |

To replace every stage, use the dependency-injection constructor of `AndroidBootstrap`:

```csharp
new AndroidBootstrap(
    versionComparer, downloader, hashValidator, apkInstaller, fileStorage,
    packageSource, installationStore, eventDispatcher?, logger?);
```

The bootstrap disposes the downloader and package source it owns when they implement `IDisposable`; all other injected dependencies (including the store) remain the host's responsibility. Do not share bootstrap-owned service instances across updaters.

### 4.2 Scenario Samples

#### Scenario 1: Custom version comparison strategy

**Scenario:** the app uses `year.month.day.build` version strings that `System.Version` cannot parse.

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

**Notes**
- `compareResult` follows `target.CompareTo(current)`: a positive value means an update is available
- Returning `false` with an error message fails validation with `VersionComparisonFailed`

#### Scenario 2: Custom event dispatcher (Avalonia UI thread)

**Scenario:** event callbacks run on a background thread by default; UI updates must be dispatched to the UI thread.

```csharp
using GeneralUpdate.Avalonia.Android.Abstractions;

public sealed class AvaloniaEventDispatcher : IUpdateEventDispatcher
{
    public void Dispatch(Action callback)
    {
        if (Avalonia.Threading.Dispatcher.UIThread.CheckAccess())
        {
            callback();
        }
        else
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(callback);
        }
    }
}

// Usage
var bootstrap = GeneralUpdateBootstrap.CreateDefault(
    options,
    eventDispatcher: new AvaloniaEventDispatcher());
```

#### Scenario 3: Custom package source (in-house version API)

**Scenario:** the enterprise already has its own version API and does not want to adopt the GeneralUpdate protocol or expose a static JSON endpoint.

```csharp
using GeneralUpdate.Avalonia.Android.Abstractions;
using GeneralUpdate.Avalonia.Android.Models;

public sealed class EnterprisePackageSource : IUpdatePackageSource
{
    public async Task<UpdatePackageInfo?> GetLatestAsync(
        string currentVersion, CancellationToken cancellationToken = default)
    {
        // Call your own API. Return null when there is no eligible package;
        // transport or metadata errors must be thrown, never silently swallowed as null.
        return await Task.FromResult<UpdatePackageInfo?>(null);
    }
}

// Usage
var bootstrap = GeneralUpdateBootstrap.CreateDefault(
    options,
    packageSource: new EnterprisePackageSource());
```

#### Scenario 4: Custom installation record store

**Scenario:** the installation record must live in a database or system preferences instead of a JSON file.

```csharp
using GeneralUpdate.Avalonia.Android.Abstractions;
using GeneralUpdate.Avalonia.Android.Models;

public sealed class DatabaseInstallationStore : IInstallationStore
{
    public Task<InstallationRecord?> LoadAsync(CancellationToken ct = default) { /* ... */ }
    public Task SaveAsync(InstallationRecord record, CancellationToken ct = default) { /* atomic write */ }
    public Task ClearAsync(CancellationToken ct = default) { /* ... */ }
}

var bootstrap = GeneralUpdateBootstrap.CreateDefault(
    options,
    installationStore: new DatabaseInstallationStore());
```

#### Scenario 5: Custom downloader (private file server)

**Scenario:** a private file server requires a custom authentication header during download.

```csharp
using GeneralUpdate.Avalonia.Android.Abstractions;
using GeneralUpdate.Avalonia.Android.Models;

public sealed class EnterpriseDownloader : IUpdateDownloader
{
    public async Task<DownloadResult> DownloadAsync(
        UpdatePackageInfo packageInfo,
        Action<DownloadProgressInfo>? progressCallback,
        CancellationToken cancellationToken = default)
    {
        // Implement the download and progress reporting yourself and return a DownloadResult.
        // Unlike the default downloader, a custom implementation must guarantee HTTPS,
        // resume support and progress semantics on its own.
        return await Task.FromResult(new DownloadResult
        {
            Success = false,
            State = UpdateState.Failed,
            FailureReason = UpdateFailureReason.NetworkError
        });
    }
}
```

---

## 5. Common Usage Samples

### 5.1 Quick Start (minimal demo)

```csharp
using GeneralUpdate.Avalonia.Android;
using GeneralUpdate.Avalonia.Android.Models;

var options = new AndroidUpdateOptions
{
    DownloadDirectoryPath = Path.Combine(
        Android.App.Application.Context.CacheDir!.AbsolutePath!, "update"),
    FileProviderAuthority = "com.example.app.generalupdate.fileprovider",

    // ValidateAsync queries this server internally; the caller only supplies the current version
    UpdateServer = new UpdateServerOptions
    {
        RequestUrl = "https://example.com/Upgrade/Verification",
        AppKey = "your-app-key",
        Platform = androidPlatformId,   // Android platform id configured on the server
        ProductId = "your-product-id"
    }
};

using var bootstrap = GeneralUpdateBootstrap.CreateDefault(options);
bootstrap.AddListenerUpdateFailed += (_, args) => Console.Error.WriteLine(args.Result.Message);

// The version installed on the device
var context = global::Android.App.Application.Context;
var currentVersion = context.PackageManager?.GetPackageInfo(
    context.PackageName!, global::Android.Content.PM.PackageInfoFlags.Activities)?.VersionName
    ?? throw new InvalidOperationException("Cannot read the installed version.");

var prepared = await bootstrap.PrepareUpdateAsync(currentVersion, CancellationToken.None);
if (prepared.IsReadyToInstall && prepared.PackageInfo is { } package && prepared.FilePath is { } path)
{
    await bootstrap.LaunchInstallerAsync(package, path, CancellationToken.None);
}
```

### 5.2 Parameter Combinations (phased API)

```csharp
using GeneralUpdate.Avalonia.Android;
using GeneralUpdate.Avalonia.Android.Models;
using GeneralUpdate.Avalonia.Android.Services;

var options = new AndroidUpdateOptions
{
    DownloadDirectoryPath = Path.Combine(
        Android.App.Application.Context.CacheDir!.AbsolutePath!, "myapp-updates"),
    FileProviderAuthority = "com.myapp.fileprovider",
    TemporaryFileExtension = ".downloading",
    Language = UpdateLanguage.Chinese
};

// Bootstrap with HTTP configuration
var httpOptions = new HttpDownloadOptions
{
    RequestTimeout = TimeSpan.FromSeconds(60),
    DownloadTimeout = TimeSpan.FromMinutes(30),
    MaxRetryAttempts = 5,
    RetryBaseDelay = TimeSpan.FromSeconds(2),
    // Self-signed certificate in development
    SslValidationPolicy = new AllowAllSslValidationPolicy()
};

var bootstrap = GeneralUpdateBootstrap.CreateDefault(
    options,
    httpOptions: httpOptions);

// Event listeners
bootstrap.AddListenerDownloadProgressChanged += (_, e) =>
{
    Console.WriteLine(
        $"Download: {e.ProgressPercentage:F1}% " +
        $"({FormatSize(e.DownloadedBytes)}/{FormatSize(e.TotalBytes)}) " +
        $"{FormatSpeed(e.DownloadSpeedBytesPerSecond)} {e.StatusDescription}");
};

bootstrap.AddListenerUpdateFailed += (_, e) =>
    Console.WriteLine($"Failed: {e.Result.Message} (reason: {e.Result.FailureReason})");

bootstrap.AddListenerUpdateCompleted += (_, e) =>
    Console.WriteLine($"Completed: {e.Result.State}");

// Phased execution: check → download+verify → launch installer
var check = await bootstrap.ValidateAsync(currentVersion);
if (!check.UpdateFound || check.PackageInfo is null) return;

var download = await bootstrap.DownloadAndVerifyAsync(check.PackageInfo);
if (!download.Success || download.FilePath is null) return;

await bootstrap.LaunchInstallerAsync(check.PackageInfo, download.FilePath);

static string FormatSize(long bytes) => bytes switch
{
    < 1024 => $"{bytes} B",
    < 1048576 => $"{bytes / 1024.0:F1} KB",
    _ => $"{bytes / 1048576.0:F1} MB"
};

static string FormatSpeed(double bytesPerSecond) =>
    $"{bytesPerSecond / 1048576.0:F1} MB/s";
```

### 5.3 Production-grade sample

A complete update workflow with HTTP authentication, the pre-check hook, permission guidance and installation confirmation:

```csharp
using GeneralUpdate.Avalonia.Android;
using GeneralUpdate.Avalonia.Android.Abstractions;
using GeneralUpdate.Avalonia.Android.Models;
using GeneralUpdate.Avalonia.Android.Services;

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
            RetryBaseDelay = TimeSpan.FromSeconds(1),
            AuthProvider = new BearerTokenAuthProvider(serverToken)
        };

        var options = new AndroidUpdateOptions
        {
            FileProviderAuthority = fileProviderAuthority,
            UpdateServer = new UpdateServerOptions
            {
                RequestUrl = "https://update.example.com/Upgrade/Verification",
                AppKey = "your-app-key",
                Platform = androidPlatformId,
                ProductId = "your-product-id"
            }
        };

        _bootstrap = GeneralUpdateBootstrap.CreateDefault(options, httpOptions: httpOptions);

        // Business interception before download: only pull non-forced updates on Wi-Fi
        _bootstrap.AddListenerUpdatePrecheck(args =>
        {
            if (!IsWifiConnected())
            {
                StatusChanged?.Invoke(this, $"Waiting for Wi-Fi before updating to {args.PackageInfo.Version}");
                return true;   // true = skip this update
            }

            return false;      // false = continue
        });

        // Installation confirmation: raised only on the first durable confirmation
        _bootstrap.AddListenerInstallationConfirmed += (_, args) =>
        {
            StatusChanged?.Invoke(this,
                $"Updated to {args.Result.Record?.TargetVersion} (device: {args.Result.CurrentVersion})");
        };

        _bootstrap.AddListenerDownloadProgressChanged += (_, e) =>
            ProgressChanged?.Invoke(this, e.ProgressPercentage);

        _bootstrap.AddListenerUpdateFailed += (_, e) =>
            StatusChanged?.Invoke(this, $"Failed: {e.Result.Message} ({e.Result.FailureReason})");
    }

    /// <summary>Call at startup or after returning from the installer to reconcile offline.</summary>
    public async Task<bool> ConfirmInstallationAsync(CancellationToken ct = default)
    {
        if (_bootstrap is null) throw new InvalidOperationException("Not initialized.");

        var currentVersion = ReadInstalledVersion();
        var installation = await _bootstrap.CheckInstallationAsync(currentVersion, ct);
        if (!installation.Success)
        {
            StatusChanged?.Invoke(this, $"Installation reconciliation failed: {installation.Message}");
            return false;
        }

        if (installation.HasPendingInstallation)
        {
            StatusChanged?.Invoke(this, "The previous installation has not taken effect yet; you can retry.");
            return false;
        }

        return installation.IsInstalled;
    }

    public async Task<bool> UpdateAsync(string currentVersion, CancellationToken ct = default)
    {
        if (_bootstrap is null) throw new InvalidOperationException("Not initialized.");

        StatusChanged?.Invoke(this, "Checking for updates...");
        var prepared = await _bootstrap.PrepareUpdateAsync(currentVersion, ct);

        if (!prepared.Success)
        {
            StatusChanged?.Invoke(this, $"Update failed: {prepared.Message}");
            return false;
        }

        if (!prepared.UpdateFound)
        {
            StatusChanged?.Invoke(this, "Already up to date.");
            return true;
        }

        if (!prepared.IsReadyToInstall || prepared.PackageInfo is not { } package ||
            prepared.FilePath is not { } path)
        {
            StatusChanged?.Invoke(this, "Update was skipped.");
            return false;
        }

        // Install permission check (Android 8.0+)
        if (!HasInstallPermission())
        {
            OpenUnknownSourcesSettings();
            StatusChanged?.Invoke(this, "Please allow install from unknown apps.");
            return false;
        }

        StatusChanged?.Invoke(this, "Installing...");
        var install = await _bootstrap.LaunchInstallerAsync(package, path, ct);
        if (install.Success)
        {
            StatusChanged?.Invoke(this, "Installer launched.");
            return true;
        }

        StatusChanged?.Invoke(this, $"Install failed: {install.Message}");
        return false;
    }

    private static string ReadInstalledVersion()
    {
        var context = Android.App.Application.Context;
        return context.PackageManager?.GetPackageInfo(
            context.PackageName!, Android.Content.PM.PackageInfoFlags.Activities)?.VersionName
            ?? throw new InvalidOperationException("Cannot read the installed version.");
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

    private static bool IsWifiConnected() => true; // implemented by the host

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

### Server version query protocol

`ValidateAsync(currentVersion, ct)` only receives the version installed on the device. The component queries
`AndroidUpdateOptions.UpdateServer`, picks the newest eligible full APK and compares versions.

**Default protocol**: `POST` to `RequestUrl` with body `version / appKey / appType / platform / productId`, responding with:

```json
{
  "code": 200,
  "body": [
    {
      "version": "2.3.0",
      "name": "2.3.0",
      "url": "https://example.com/app-release.apk",
      "hash": "0123456789abcdef... (64 hex characters)",
      "size": 52428800,
      "updateLog": "Release notes",
      "releaseDate": "2026-06-01T00:00:00Z",
      "isForcibly": false,
      "packageType": 2,
      "format": "apk",
      "authScheme": "bearer",
      "authToken": "per-package-token"
    }
  ]
}
```

Mapping: `version / url / hash / size / name / updateLog / releaseDate / isForcibly / authScheme / authToken`. The client
keeps the newest non-frozen full APK:

- entries with `isFreeze = true` are skipped
- `packageType` must be `2`, `0` or omitted
- `format` must be `apk` / `.apk`; when omitted the URL path must end with `.apk`
- ZIP, differential and driver packages are never handed to the Android installer
- an empty `body`, no eligible entry, or HTTP 204 all mean "no update"

:::warning Never hardcode the platform id
`Platform` must match the Android platform identifier configured in your deployment; the sample values are placeholders.
When your protocol differs, use the static JSON endpoint or a custom `IUpdatePackageSource` instead.
:::

**Static JSON endpoint**: set `UpdateServer.UseJsonEndpoint = true` and point `RequestUrl` at a JSON document. The
component performs a `GET` and reads a single `UpdatePackageInfo` (property names are case insensitive):

```json
{
  "version": "2.3.0",
  "downloadUrl": "https://example.com/app-release.apk",
  "sha256": "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef",
  "description": "Release notes",
  "isForced": false
}
```

- `sha256` must be the real 64-character hexadecimal SHA-256 of the APK, not MD5
- `fileSize` may be omitted or `0` (unknown); when present it is in bytes
- HTTP 204 or a JSON `null` body means no package
- request, protocol and metadata errors are reported through `UpdateCheckResult.Success = false`, `FailureReason` and `AddListenerUpdateFailed`, and **never** trigger the pre-check

### Update pre-check hook

`AddListenerUpdatePrecheck` mirrors `GeneralUpdate.Core`'s `AddListenerUpdatePrecheck` / `ClientStrategy.UseUpdatePrecheck`.
It runs after `ValidateAsync` finds a newer version and **before** the APK is downloaded:

```csharp
bootstrap.AddListenerUpdatePrecheck(args =>
{
    // args.PackageInfo    — Version / DownloadUrl / Sha256 / FileSize / IsForced, …
    // args.CurrentVersion — version currently installed on the device
    // args.Result         — the UpdateCheckResult ValidateAsync is about to return
    // args.IsForced       — whether the publisher marked it as forced

    if (args.PackageInfo.Version == "1.2.0" && !IsWifiConnected())
    {
        return true;   // skip: fetch 1.2.0 later on Wi-Fi
    }

    return false;
});
```

Returning `true` skips the update (same `CanSkip` semantics as `GeneralUpdate.Core`). When skipped, `ValidateAsync`
returns `UpdateFound == false` with state `UpdateState.Completed` and does **not** raise `AddListenerValidate`, so the usual
`if (check.UpdateFound) { ... }` flow never starts a download. Forced updates (`UpdatePackageInfo.IsForced = true`) never
invoke the callback.

### Installation result loop

`CreateDefault` atomically writes the last installation target to `<FilesDir>/update/installation.json` (not the APK
cache, which Android may clear). Override the location with `InstallationStateFilePath`; **use only one bootstrap instance per record file**.

Key points:

1. `LaunchInstallerAsync` writes the installation record **before** launching the installer; a write failure returns `FileIoError` and the installer is not launched
2. After installation the process is killed by the system; the host reads the actual version from PackageManager on the next launch or on return from the installer
3. `CheckInstallationAsync(currentVersion)` reconciles offline, without network access

| Result | Meaning |
| --- | --- |
| `Success && State == None` | No installation record; **not proof of a successful install** |
| `HasPendingInstallation` | The installed version is below the target; the install is unconfirmed — it may have been canceled, failed or still be running. Re-check and retry |
| `IsInstalled` | The device reached or exceeded the target, state is `Installed`, and the confirmation is persisted |
| `!Success` | Invalid device version, or record read/write failure; reported through `AddListenerUpdateFailed`, never disguised as "no record" |

`AddListenerInstallationConfirmed` is raised only on the first durable confirmation; repeated checks or process restarts do
not re-raise it, and it is not a reliable message queue — restore UI state from the returned `InstallationCheckResult`.
When a record is corrupt, the host should inform the user and explicitly call `ResetInstallationAsync()`.

:::danger Installation confirmation is not signature verification
Reconciling the device version is not APK signature verification or an app health check, and the component provides no
silent install, auto-restart or rollback. Production APKs must keep the same package name, a compatible signature and an
increasing `versionCode`.
:::

### Authentication

Four schemes are supported, configurable per package on `UpdatePackageInfo`:

| Scheme | AuthScheme | Required fields | Headers |
| --- | --- | --- | --- |
| HMAC-SHA256 | `Hmac` | `AuthSecretKey` | `X-Update-Timestamp` + `X-Update-Signature` |
| Bearer Token | `Bearer` | `AuthToken` | `Authorization: Bearer <token>` |
| API Key | `ApiKey` | `AuthToken` | `X-Api-Key: <key>` |
| HTTP Basic | `Basic` | `BasicUsername` + `BasicPassword` | `Authorization: Basic <base64>` |

A global provider can be set through `HttpDownloadOptions.AuthProvider` (built-in `BearerTokenAuthProvider`,
`ApiKeyAuthProvider`, `BasicAuthProvider`, `HmacAuthProvider`, or `HttpAuthProviderFactory`). Per-package configuration
takes precedence over the global provider.

:::info Where credentials are sent
Global authentication is only sent to **HTTPS download URLs on the same origin** as the verification endpoint;
cross-origin downloads and plaintext HTTP downloads never receive it.
:::

### SSL policies

| Policy | Class | Usage |
| --- | --- | --- |
| System default (recommended) | `null` | Production |
| Allow all (development only) | `AllowAllSslValidationPolicy` | Self-signed certificates in development |
| Strict | `StrictSslValidationPolicy` | Conservatively rejects every validation error |
| Custom | Implement `ISslValidationPolicy` | Private CA or certificate pinning |

### Resumable download

The downloader uses a temporary file (`.part`) and a metadata file (`.json`):

```
<CacheDir>/update/
├── app-v2.0.0.apk.part       # partially downloaded file
├── app-v2.0.0.apk.json       # resume metadata (URL, SHA256, file name, expected size, ETag, LastModified)
└── app-v2.0.0.apk            # final file after completion
```

- A `HEAD` probe discovers `Accept-Ranges` / `ETag` / `Content-Length` / `Last-Modified`; a 405/501 response falls back to GET
- On restart the sidecar decides whether resume is possible: any mismatch in file name, `ETag` or `LastModified` discards the temp file
- With Range support the download continues from the existing length (`Range: bytes={existingLength}-`); a `200 OK` instead of `206` restarts from zero
- On completion the `.part` file is atomically renamed (overwriting) and the sidecar is deleted
- Transient HEAD/GET/response-stream errors retry and resume; metadata queries, permanent HTTP errors (such as 401) and caller cancellation do not retry

### Concurrency and lifetime

- Every public operation runs inside one `SemaphoreSlim(1,1)` gate; state is read thread-safely through `GetSnapshot()`
- Cancel and await the active operation before `Dispose`; disposing early rejects new and queued calls and defers resource release until the operation exits, but does not cancel it
- An externally supplied `HttpClient` stays host-owned and its `Timeout` is never rewritten; TLS/proxy must be configured on its handler, and combining those with `httpOptions` throws `ArgumentException`
- Without a supplied client the component creates and disposes one transport client shared by queries and downloads
- Timeouts report `Failed/NetworkError`, caller cancellation reports `Canceled`, and cancellation while waiting for the operation lock throws

### Android integration

1. **Install permission (Android 8.0+)**: declare it in `AndroidManifest.xml`; without it `LaunchInstallerAsync` returns `InstallPermissionDenied`:

```xml
<uses-permission android:name="android.permission.REQUEST_INSTALL_PACKAGES" />
```

2. **FileProvider**: `android:authorities` must equal `AndroidUpdateOptions.FileProviderAuthority`, and the paths must cover `DownloadDirectoryPath` (default `<CacheDir>/update`); otherwise `LaunchInstallerAsync` returns `InstallLaunchFailed`:

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

`Resources/xml/generalupdate_file_paths.xml`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<paths>
    <cache-path name="update_cache" path="update/" />
    <files-path name="update_files" path="update/" />
</paths>
```

3. **Current Activity**: `CreateDefault` defaults to `NullAndroidActivityProvider`, in which case the installer is launched through `Application.Context` with `FLAG_ACTIVITY_NEW_TASK`. Passing a provider that implements `IAndroidActivityProvider` and returns the current `Activity` is more robust:

```csharp
using var bootstrap = GeneralUpdateBootstrap.CreateDefault(options, activityProvider: myActivityProvider);
```

### Platform differences

| Item | Description |
| --- | --- |
| Minimum API | 26 (Android 8.0) |
| Install permission | Android 8.0+ requires a `CanRequestPackageInstalls()` check |
| FileProvider | APK files must reach the system installer through a FileProvider URI |
| Plaintext HTTP | Downloads are rejected by default; `AllowInsecureHttpDownloads` must be enabled explicitly |
| Process lifetime | The process is killed after installation, so the result must be reconciled on the next launch |

---

## Related Resources

- [GeneralUpdate.Avalonia repository](https://github.com/GeneralLibrary/GeneralUpdate.Avalonia)
- [Android update sample](https://github.com/GeneralLibrary/GeneralUpdate-Samples/tree/main/UI/AndroidUpdate)
- [Avalonia.Android execution flow](Avalonia.Android-flow)
- [Avalonia Android cookbook](../quickstart/Avalonia%20Android%20cookbook)
- [Semantic Versioning](https://semver.org/)
- [FileProvider documentation](https://developer.android.com/training/secure-file-sharing/setup-sharing)
