---
sidebar_position: 5
---

# GeneralUpdate.Core

## Component role

`GeneralUpdate.Core` is the execution core of GeneralUpdate. It connects update checking, package download, checksum verification, extraction or patch merge, file replacement, process restart, and reporting through a single entry point: `GeneralUpdateBootstrap`.

Core can run inside the main application for the `Client` / `OssClient` workflow, or inside an independent updater process for the `Upgrade` / `OssUpgrade` workflow. A typical desktop deployment is:

1. The main application starts the update check.
2. Core obtains the server manifest and downloads update packages.
3. Core starts an independent updater process after the main application is ready to exit.
4. The updater process replaces files and restarts the main application.

> Firmware update is outside the scope of this page. Driver installation belongs to `GeneralUpdate.Drivelution` and is documented separately.

**Namespace:** `GeneralUpdate.Core`
**Primary entry point:** `GeneralUpdateBootstrap`
**NuGet package:** `GeneralUpdate.Core`

```bash
dotnet add package GeneralUpdate.Core
```

## When to use Core

| Scenario | Fit |
| --- | --- |
| Desktop application self-update | Yes. Use the main app to check updates and the updater process to replace files. |
| Multi-version sequential update | Yes. Core can process the version sequence returned by the server. |
| Differential package update | Yes. Use differential packages with `Option.PatchEnabled` / `Option.DiffMode`. |
| OSS or cloud-storage distribution | Yes. Use `OssClient` / `OssUpgrade`. |
| Firmware flashing | No. This page does not cover firmware update components. |

## Runtime roles

Core uses `Option.AppType` to decide the process role:

| AppType | Role | Description |
| --- | --- | --- |
| `Client` | Main application workflow | Checks versions, downloads packages, prepares upgrade context, and starts the updater. Default value. |
| `Upgrade` | Independent updater workflow | Reads IPC data from the main app, replaces files, and starts the main app. |
| `OssClient` | OSS client workflow | Checks OSS update configuration and starts the OSS updater. |
| `OssUpgrade` | OSS updater workflow | Downloads from OSS and deploys packages. |

The updater process usually does not call `SetConfig` manually. When it is launched by the main application, Core reads the encrypted file IPC contract and restores paths, versions, temporary directories, download settings, and package lists automatically.

## Minimal updater process

`GeneralUpdate-Samples/src/Upgrade/Program.cs` shows the minimal independent updater shape. Register the events you care about and call `LaunchAsync()`:

```csharp
using GeneralUpdate.Common.Download;
using GeneralUpdate.Common.Internal;
using GeneralUpdate.Common.Shared.Object;
using GeneralUpdate.Core;

try
{
    Console.WriteLine($"Updater started at {DateTime.Now}");

    _ = await new GeneralUpdateBootstrap()
        .AddListenerMultiDownloadStatistics(OnMultiDownloadStatistics)
        .AddListenerMultiDownloadCompleted(OnMultiDownloadCompleted)
        .AddListenerMultiAllDownloadCompleted(OnMultiAllDownloadCompleted)
        .AddListenerMultiDownloadError(OnMultiDownloadError)
        .AddListenerException(OnException)
        .LaunchAsync();
}
catch (Exception ex)
{
    Console.WriteLine(ex);
}

void OnMultiDownloadStatistics(object sender, MultiDownloadStatisticsEventArgs args)
{
    var version = args.Version as VersionInfo;
    Console.WriteLine(
        $"Version: {version?.Version}, Speed: {args.Speed}, Progress: {args.ProgressPercentage}%");
}

void OnMultiDownloadCompleted(object sender, MultiDownloadCompletedEventArgs args)
{
    var version = args.Version as VersionInfo;
    Console.WriteLine(args.IsComplated
        ? $"Version {version?.Version} download completed."
        : $"Version {version?.Version} download failed.");
}

void OnMultiAllDownloadCompleted(object sender, MultiAllDownloadCompletedEventArgs args)
{
    Console.WriteLine(args.IsAllDownloadCompleted
        ? "All download tasks completed."
        : $"Download failed. Failed versions: {args.FailedVersions.Count}");
}

void OnMultiDownloadError(object sender, MultiDownloadErrorEventArgs args)
{
    var version = args.Version as VersionInfo;
    Console.WriteLine($"Version {version?.Version} download error: {args.Exception}");
}

void OnException(object sender, ExceptionEventArgs args)
{
    Console.WriteLine(args.Exception);
}
```

## Main application configuration

When using Core directly from the main application, pass an `UpdateRequest` explicitly:

```csharp
using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;

await new GeneralUpdateBootstrap()
    .SetConfig(new UpdateRequest
    {
        UpdateUrl = "https://update.example.com/api/upgrade/verification",
        ReportUrl = "https://update.example.com/api/upgrade/report",
        UpdateAppName = "UpgradeSample.exe",
        MainAppName = "ClientSample.exe",
        InstallPath = AppDomain.CurrentDomain.BaseDirectory,
        ClientVersion = "1.0.0",
        AppSecretKey = "your-app-secret",
        ProductId = "your-product-id"
    })
    .SetOption(Option.AppType, AppType.Client)
    .SetOption(Option.DownloadTimeout, 60)
    .SetOption(Option.MaxConcurrency, 3)
    .AddListenerUpdateInfo((sender, args) =>
    {
        Console.WriteLine($"Server returned {args.Info.Body?.Count ?? 0} update versions.");
    })
    .AddListenerException((sender, args) =>
    {
        Console.WriteLine(args.Exception);
    })
    .LaunchAsync();
```

For a simplified setup, use `SetSource`:

```csharp
await new GeneralUpdateBootstrap()
    .SetSource(
        updateUrl: "https://update.example.com/api/upgrade/verification",
        appSecretKey: "your-app-secret",
        reportUrl: "https://update.example.com/api/upgrade/report")
    .SetOption(Option.AppType, AppType.Client)
    .LaunchAsync();
```

## Execution flow

1. `SetConfig` / `SetSource` loads update URLs, app names, versions, secret keys, and install paths.
2. `LaunchAsync` selects `ClientStrategy`, `UpdateStrategy`, or an OSS strategy based on `Option.AppType`.
3. The Client workflow requests version metadata and triggers `AddListenerUpdateInfo` / `AddListenerUpdatePrecheck`.
4. Core downloads required version packages and reports speed, progress, completion, and errors through events.
5. After download, Core verifies checksums, extracts packages, merges patches, and replaces files.
6. The Upgrade workflow starts the main app according to `Option.LaunchClientAfterUpdate`.
7. If `ReportUrl` or a custom `UpdateReporter` is configured, Core reports update status.

## Common options

Use `SetOption(Option.Xxx, value)` for runtime settings:

| Option | Default | Description |
| --- | --- | --- |
| `Option.AppType` | `AppType.Client` | Current process role. |
| `Option.Encoding` | `Encoding.UTF8` | Encoding for package processing. |
| `Option.Format` | `Format.Zip` | Update package compression format. |
| `Option.DownloadTimeout` | `30` | Download timeout in seconds. |
| `Option.PatchEnabled` | `true` | Enables differential patch processing. |
| `Option.BackupEnabled` | `true` | Backs up replaced files before update. |
| `Option.MaxConcurrency` | `3` | Max concurrent file downloads. |
| `Option.EnableResume` | `true` | Enables resumable downloads. |
| `Option.RetryCount` | `3` | Download retry count. |
| `Option.RetryInterval` | `1s` | Delay between retries. |
| `Option.VerifyChecksum` | `true` | Verifies file checksums. |
| `Option.DiffMode` | `DiffMode.Serial` | Differential merge mode. |
| `Option.Silent` | `false` | Enables silent polling update mode. |
| `Option.SilentPollIntervalMinutes` | `60` | Silent polling interval. |
| `Option.LaunchClientAfterUpdate` | `true` | Starts the main app after update. |

Example:

```csharp
await new GeneralUpdateBootstrap()
    .SetConfig(request)
    .SetOption(Option.AppType, AppType.Client)
    .SetOption(Option.PatchEnabled, true)
    .SetOption(Option.BackupEnabled, true)
    .SetOption(Option.VerifyChecksum, true)
    .SetOption(Option.MaxConcurrency, 4)
    .LaunchAsync();
```

## Events

Core exposes update state through listener methods:

| Method | Trigger | Typical use |
| --- | --- | --- |
| `AddListenerUpdateInfo` | After server version metadata is returned | Show update notes, version list, or package size. |
| `AddListenerUpdatePrecheck` | Before download starts | Check disk space, network conditions, or user confirmation. |
| `AddListenerMultiDownloadStatistics` | During download | Show speed, remaining time, and percentage. |
| `AddListenerMultiDownloadCompleted` | One version package completes | Log per-version download result. |
| `AddListenerMultiAllDownloadCompleted` | All download tasks complete | Update UI state or write logs. |
| `AddListenerMultiDownloadError` | A download task fails | Capture failed version and exception. |
| `AddListenerProgress` | Generic update progress changes | Drive a unified progress bar. |
| `AddListenerException` | Core catches an exception | Log, notify the user, or report telemetry. |

You can also implement `IUpdateEventListener` and register all handlers with `AddEventListener<TListener>()`.

## Silent update

Silent mode is designed for background polling. When enabled, the `Client` workflow starts a background poll loop and returns immediately. Prepared updates continue when the process exits.

```csharp
await new GeneralUpdateBootstrap()
    .SetConfig(request)
    .SetOption(Option.AppType, AppType.Client)
    .SetOption(Option.Silent, true)
    .SetOption(Option.SilentPollIntervalMinutes, 30)
    .SetOption(Option.LaunchClientAfterUpdate, true)
    .LaunchAsync();
```

Silent mode still requires valid update URL, secret key, main app name, updater name, and install path configuration.

## Extension points

`GeneralUpdateBootstrap` inherits from `AbstractBootstrap` and can replace multiple internal components:

| Method | Purpose |
| --- | --- |
| `Strategy<T>()` | Selects a custom platform strategy. |
| `Hooks<T>()` | Adds lifecycle hooks before update, after download, after update, before app start, and on error. |
| `UpdateReporter<T>()` | Customizes update status reporting. |
| `SslPolicy<T>()` | Customizes HTTPS certificate validation. |
| `UpdateAuth<T>()` | Adds authentication to HTTP requests. |
| `DownloadSource<T>()` | Customizes manifest and file source. |
| `DownloadPolicy<T>()` | Customizes retry and timeout behavior. |
| `DownloadExecutor<T>()` | Customizes single-file download implementation. |
| `DownloadPipeline<T>()` | Adds post-download processing such as decryption, scanning, or validation. |
| `DownloadOrchestrator<T>()` | Fully owns batch download orchestration. |

Advanced projects can replace one part of the workflow without forking Core.

## Relationship with GeneralUpdate.Tools

Core consumes version manifests and update packages from the update server or OSS. `GeneralUpdate.Tools` helps produce and verify those inputs:

- Patch Package generates differential packages.
- Extension Package generates extension packages.
- OSS Config prepares cloud-storage distribution configuration.
- Simulation, report, and hash features help validate package structure and integrity before release.

Recommended workflow: use Tools to generate and validate packages, publish packages and manifests to the server or OSS, then let Core execute the update on client machines.

## Troubleshooting

### The updater starts but does nothing

Confirm it was launched by the main application. If the independent updater is started by double-clicking, it usually has no IPC context and cannot know the install path or pending versions.

### File replacement fails

The main app or Bowl process is usually still holding files. Confirm the main app has exited and configure the `Bowl` process name or shutdown logic correctly.

### Download succeeds but checksum validation fails

Confirm the package was not recompressed, truncated, or replaced on the server or OSS. When `Option.VerifyChecksum` is enabled, the received file hash must match the manifest.

### Differential packages are ignored

Confirm the server returns a differential package manifest and `Option.PatchEnabled` is `true`. Generate differential packages with `GeneralUpdate.Tools` to avoid mismatched manifests and package contents.

## Related samples

- [Upgrade sample](https://github.com/GeneralLibrary/GeneralUpdate-Samples/blob/main/src/Upgrade/Program.cs)
- [OSS upgrade sample](https://github.com/GeneralLibrary/GeneralUpdate-Samples/tree/main/src/OSS/OSSUpgradeSample)
- [GeneralUpdate repository](https://github.com/GeneralLibrary/GeneralUpdate)
- [GeneralUpdate.Tools repository](https://github.com/GeneralLibrary/GeneralUpdate.Tools)
