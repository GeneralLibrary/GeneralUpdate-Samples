---
sidebar_position: 4
title: Avalonia Android Cookbook
---

# GeneralUpdate.Avalonia.Android Cookbook

This guide is for developers who need to integrate Android APK auto-update into their Avalonia applications. The goal is to complete the full loop — "query the server → download APK → verify SHA256 → launch the installer → confirm the installation on next launch" — with minimal code.

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
① Version query (inside the component)  ② Download + verify     ③ Install + confirm
┌──────────────┐             ┌──────────┐              ┌──────────────────┐
│   Client     │──POST──────→│  Server  │              │  Android         │
│ (Avalonia)   │←──JSON──────│ (Update  │              │  Package         │
└──────┬───────┘             │ Service) │              │  Installer       │
       │                     └────┬─────┘              └────────┬─────────┘
       │  ValidateAsync(currentVersion) — the component sends the request
       │  and receives PackageInfo (version/url/hash/size/…)
       │                          │                             │
       │  GET /app-v2.0.0.apk (Range supported, resumable)      │
       │ ────────────────────────→│                             │
       │  APK stream ←────────────│                             │
       │                          │                             │
       │  After size + SHA256 verification, persist record      │
       │  launch Package Installer ───────────────────────────→ │
       │                          │                             │
       │  ★ Next launch: CheckInstallationAsync reconciles ←────│
       ▼                          ▼                             ▼
```

| Role | Definition | Responsibilities |
| --- | --- | --- |
| Client (Avalonia) | Your Avalonia Android app | Query version → download APK → verify → launch installer → confirm result |
| Server | Update service | Return version metadata, serve the APK with Range support |
| Android Package Installer | Android system component | Install the APK (verify signature, replace the app) |

## Phase 1: Environment Setup

### Requirements

| Item | Requirement | Verify |
| --- | --- | --- |
| .NET SDK | 10.0+ | `dotnet --version` |
| Android workload | Installed | `dotnet workload list` (should include `android`) |
| Android SDK | API 26+ | Android Studio SDK Manager |
| Avalonia | 12+ | Avalonia package version referenced by your project |
| Target framework | `net10.0-android` | `TargetFramework` in `.csproj` |

### NuGet Package

```bash
dotnet add package GeneralUpdate.Avalonia.Android
```

## Phase 2: Android Integration Configuration

The host must configure all three items below; missing any one of them stops the update half way.

### 2.1 Declare the install permission (Android 8.0+)

In `AndroidManifest.xml`:

```xml
<uses-permission android:name="android.permission.REQUEST_INSTALL_PACKAGES" />
```

The user may still not have granted it. In that case `LaunchInstallerAsync` returns `FailureReason = InstallPermissionDenied`. Use the following code to send the user to "install unknown apps", then retry:

```csharp
var context = Android.App.Application.Context;
context.StartActivity(new Android.Content.Intent(
        Android.Provider.Settings.ActionManageUnknownAppSources,
        Android.Net.Uri.Parse("package:" + context.PackageName))
    .AddFlags(Android.Content.ActivityFlags.NewTask));
```

### 2.2 Configure FileProvider

The APK must be handed to the Android Package Installer through a FileProvider. `authorities` must match the `FileProviderAuthority` used in code:

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

Create `Resources/xml/generalupdate_file_paths.xml`; the paths must cover the download directory (default `<CacheDir>/update`):

```xml
<?xml version="1.0" encoding="utf-8"?>
<paths>
    <cache-path name="update_cache" path="update/" />
    <files-path name="update_files" path="update/" />
</paths>
```

> **Note**: if the authority or paths do not match, `LaunchInstallerAsync` returns `InstallLaunchFailed`.

:::warning Plaintext HTTP is for local demos only
The component rejects non-HTTPS APK downloads by default (returning `InvalidMetadata`). Only set
`AllowInsecureHttpDownloads = true` in a trusted development environment (for example `http://127.0.0.1:5080`),
together with `android:usesCleartextTraffic="true"`. Always use HTTPS in production.
:::

## Phase 3: Configure the Server Query

Package metadata is no longer built by the caller; the component queries it according to `AndroidUpdateOptions.UpdateServer`:

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
        Platform = androidPlatformId,   // must match your deployment; never hardcode
        ProductId = "your-product-id"
    }
};
```

Two supported protocols:

| Mode | Configuration | Behavior |
| --- | --- | --- |
| GeneralUpdate verification protocol (default) | `UseJsonEndpoint = false` | `POST` body `version/appKey/appType/platform/productId`, response `{"code":200,"body":[...]}`; the component picks the newest non-frozen full APK |
| Static JSON endpoint | `UseJsonEndpoint = true` | `GET` a single `UpdatePackageInfo` JSON document; HTTP 204 or `null` means no update |

Example static JSON endpoint:

```json
{
  "version": "2.3.0",
  "downloadUrl": "https://example.com/app-release.apk",
  "sha256": "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef",
  "description": "Release notes",
  "isForced": false
}
```

> `sha256` must be the real 64-character hexadecimal SHA-256 of the APK (not MD5); `fileSize` may be omitted or `0`.

If your server speaks a completely different protocol, implement `IUpdatePackageSource.GetLatestAsync(currentVersion, ct)`
and inject it with `CreateDefault(options, packageSource: mySource)` — no server changes required.

## Phase 4: Write the Update Code

### 4.1 One-shot style (recommended)

In an Avalonia ViewModel or service:

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
            Console.WriteLine($"Installation confirmed: target {e.Result.Record?.TargetVersion}, " +
                              $"installed {e.Result.CurrentVersion}");
    }

    /// <summary>Call at startup or after returning from the installer: offline reconciliation.</summary>
    public async Task CheckInstallationAsync()
    {
        if (_bootstrap is null) return;
        var installation = await _bootstrap.CheckInstallationAsync(ReadInstalledVersion());
        Console.WriteLine($"State={installation.State} Installed={installation.IsInstalled} " +
                          $"Pending={installation.HasPendingInstallation}");
    }

    /// <summary>Check and prepare the package; the caller confirms before installing.</summary>
    public async Task<(UpdatePackageInfo Package, string FilePath)?> PrepareAsync()
    {
        if (_bootstrap is null) return null;

        var prepared = await _bootstrap.PrepareUpdateAsync(ReadInstalledVersion());
        if (!prepared.Success)
        {
            Console.WriteLine($"Preparation failed: {prepared.Message}");
            return null;
        }

        if (!prepared.UpdateFound)
        {
            Console.WriteLine("Already up to date.");
            return null;
        }

        // Install permission check (Android 8+); guide the user when not granted
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
            ? "Installer launched. Confirm the installation on the device."
            : $"Failed to launch installer: {install.Message}");
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
}
```

`PrepareUpdateAsync` performs the query, version comparison, pre-check, download and SHA256 verification under a single
operation lock. It never opens the installer or a permission page.

### 4.2 Phased style (when you need intermediate UI)

```csharp
// ① Check the version only
var check = await bootstrap.ValidateAsync(currentVersion);
if (!check.UpdateFound || check.PackageInfo is null) return;
Console.WriteLine($"New version {check.TargetVersion}: {check.PackageInfo.Description}");

// ② After user confirmation, download and verify
var download = await bootstrap.DownloadAndVerifyAsync(check.PackageInfo);
if (!download.Success || download.FilePath is null) return;
Console.WriteLine("Downloaded and verified. Ready to install.");

// ③ Launch the installer
await bootstrap.LaunchInstallerAsync(check.PackageInfo, download.FilePath);
```

### 4.3 Update pre-check (business interception)

```csharp
bootstrap.AddListenerUpdatePrecheck(args =>
{
    // Skip non-forced updates while not on Wi-Fi
    if (!IsWifiConnected())
    {
        Console.WriteLine($"Waiting for Wi-Fi before updating to {args.PackageInfo.Version}");
        return true;    // true = skip; ValidateAsync returns UpdateFound = false
    }

    return false;       // false = continue
});
```

Forced updates (`UpdatePackageInfo.IsForced = true`) never invoke this callback.

## Phase 5: One-Command End-to-End Demo

The `GeneralUpdate.Avalonia` repository ships a runnable Android sample. Connect a USB-debugging device or start an emulator, then run:

```cmd
samples\GeneralUpdate.Avalonia.Android.Sample\run-demo.cmd
```

The script checks .NET 10 and the Android workload, builds the `1.0.0` and `2.0.0` APKs, starts a local demo server that
implements the GeneralSpacestation verification protocol (`http://127.0.0.1:5080`), maps the port with
`adb reverse tcp:5080 tcp:5080`, and finally installs and launches `1.0.0`.

Fixed parameters of the demo server:

| Parameter | Value |
| --- | --- |
| RequestUrl | `http://127.0.0.1:5080/Upgrade/Verification` |
| AppKey | `demo-client` |
| ProductId | `demo-product` |
| Platform | `3` |
| AppType | `1` |

To use the GeneralUpdate sample server instead, run the Server project from this Samples repository:

```bash
git clone https://github.com/GeneralLibrary/GeneralUpdate-Samples.git
cd GeneralUpdate-Samples/src/Server
dotnet run
```

The server listens on `http://localhost:5000` by default. On an Android emulator use `http://10.0.2.2:5000` to reach the host's localhost.

## Phase 6: End-to-End Verification

1. Tap "Check and auto update" in the app
2. Allow "install unknown apps" when Android asks
3. Confirm the installation of `2.0.0` in the system installer
4. Reopen the app and confirm "Current Version" is now `2.0.0` and the result reads "installation confirmed"

### Expected Output

```
Checking for updates...
Update v2.0.0 found.
Downloading 45% 1.2 MB/s
Downloading 100% Download completed
SHA256 verified.
Installer launched.
(after installation, reopen the app)
Installation confirmed: target 2.0.0, installed 2.0.0
State=Installed Installed=True Pending=False
```

### Acceptance Checklist

| Scenario | Expected behavior |
| --- | --- |
| Cancel the installation in the system installer | The app stays on the old version, `HasPendingInstallation = true`, retry is possible |
| Return after granting the permission | Server configuration and target are preserved; download and install can be retried |
| Open the app offline after installing | Offline reconciliation still confirms the upgrade; a server query failure is reported separately and does not overwrite the confirmed result |
| Reopen the already updated app | Shows the previous confirmation but does not raise `AddListenerInstallationConfirmed` again |
| Corrupt installation record | Reported as an explicit error and the update stops; the user clears it with `ResetInstallationAsync()` |

### Troubleshooting

| Symptom | Cause | Action |
| --- | --- | --- |
| `FailureReason = InstallPermissionDenied` | "Install unknown apps" not granted | Send the user to `Settings.ActionManageUnknownAppSources`, then retry |
| `FailureReason = InstallLaunchFailed` | FileProvider authority or paths do not match the download directory | Check `AndroidManifest.xml` and `generalupdate_file_paths.xml` |
| `FailureReason = InvalidMetadata` | No `UpdateServer` configured, metadata missing URL/SHA256, or the download URL is not HTTPS | Check the server response fields and HTTPS configuration |
| `FailureReason = NetworkError` | Network failure/timeout during query or download | Check the network, `RequestTimeout` / `DownloadTimeout` and retry settings |
| `FailureReason = VersionComparisonFailed` | Version string is not a valid `System.Version` | Use semantic versions or replace `IVersionComparer` |
| `HashMismatch` | Server `sha256` does not match the actual APK hash | Recompute the 64-character hexadecimal SHA-256 of the APK |
| Every attempt re-downloads the whole APK | The server does not support Range | Make the server return `Accept-Ranges: bytes` |

:::caution Installation confirmation is not proof of a completed install
`LaunchInstallerAsync` returning `Success = true` only means the installer intent was launched. Only
`CheckInstallationAsync` returning `IsInstalled` means the device actually reached the target version. The APK must keep
the same package name, a compatible signature and an increased `versionCode`; the component provides no silent install,
no auto-restart, no health check and no rollback.
:::

## Next Steps

- [GeneralUpdate.Avalonia.Android component reference](../doc/GeneralUpdate.Avalonia.Android): full API reference, configuration and advanced usage
- [Avalonia.Android execution flow](../doc/Avalonia.Android-flow): internals, resumable download and the installation result loop
- [GeneralUpdate.Avalonia repository](https://github.com/GeneralLibrary/GeneralUpdate.Avalonia): source and sample project
