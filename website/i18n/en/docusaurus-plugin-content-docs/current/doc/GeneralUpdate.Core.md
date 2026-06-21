---
sidebar_position: 5
---

# GeneralUpdate.Core

**Namespace:** `GeneralUpdate.Core` | **Main Entry Point:** `GeneralUpdateBootstrap` | **NuGet Package:** `GeneralUpdate.Core`

## 1. Component Overview

### 1.1 Introduction

**GeneralUpdate.Core** is the update execution engine of the GeneralUpdate ecosystem, responsible for full lifecycle update management of client applications. It provides a programmable launcher, configuration models, event notification system, download subsystem (supporting concurrency, resume, retry, verification, and post-processing pipelines), differential patch pipeline, version write-back, IPC process communication, and platform strategy extensions.

**Core Capabilities:**

| Capability | Description |
| --- | --- |
| Multi-Strategy Update Execution | Built-in standard Client/Upgrade, OSS object storage, and silent background polling strategies |
| Configuration-Driven | Strongly-typed `UpdateRequest` or lightweight `SetSource` entry, with `generalupdate.manifest.json` for minimal configuration |
| Download Subsystem | Pluggable download sources, executors, retry policies, post-processing pipelines, and orchestrators; defaults include concurrent downloads, resume, and SHA256 verification |
| Differential Patch Pipeline | File-level binary diff (BSDIFF 4.0 / Streaming HDiff), directory-level comparison and batch patch distribution with parallel processing |
| Event Notifications | 7 event callbacks (version discovery, download progress, completion, error, exception, etc.) with batch listener registration |
| Extension Points | 10 pluggable interfaces: lifecycle hooks, status reporting, SSL certificate policy, HTTP authentication, download source/policy/executor/pipeline/orchestrator, platform strategy |
| Manifest System | `generalupdate.manifest.json` for auto-discovery of app identity and automatic version write-back |
| IPC Communication | Encrypted file-based context passing between main app and upgrade process |
| SignalR Real-Time Push | Version update push via SignalR (`UpgradeHubService`), supporting peer-to-peer and broadcast, auto-reconnect, multi-event subscription |

**Business Problems Solved:**
- Desktop apps need reliable auto-update, but hand-writing update logic involves version comparison, download, verification, extraction, file replacement, and process restart
- Full updates for large apps have high bandwidth costs; differential updates reduce download size
- Flexible update strategies needed (silent background, user-triggered, OSS/CDN distribution)
- Upgrade process versioning needs coordination between main app and upgrade process

**Use Cases:**
- Auto-update for WPF / WinForms / Avalonia / WinUI desktop applications
- Unified version management for enterprise internal tools
- Client apps distributing update packages via CDN / OSS
- Large clients needing differential updates to reduce bandwidth

:::info Core's Role in the Ecosystem
GeneralUpdate.Core is the **core engine** of the ecosystem. It doesn't generate patches (that's Tools/Differential's job) and doesn't recover from crashes (that's Bowl's job) — it **orchestrates the entire update flow**: from version checking, downloading, and verification, to launching the Upgrade process for file replacement.
:::

### 1.2 Environment & Dependencies

| Item | Description |
| --- | --- |
| **Version** | `10.5.0-beta.7` |
| **Target Framework** | `netstandard2.0` (.NET Framework 4.6.1+ / .NET Core 2.0+ / .NET 5+) |
| **Dependencies** | `GeneralUpdate.Differential`, `System.Text.Json`, `Microsoft.Extensions.Logging.Abstractions` |
| **Compatibility** | Windows (primary) / Linux / macOS; x86 / x64 / ARM64 |

---

## 2. Feature List

| Feature | Description | Type | Required | Notes |
| --- | --- | --- | --- | --- |
| Standard Client Update | Main app checks version, downloads packages, launches upgrade process | Core | Required | Requires server version check API |
| Standard Upgrade Update | Standalone upgrade process reads IPC context and executes file replacement, diff patches, version write-back | Core | Required | Launched by main app via encrypted IPC |
| OSS Client Update | Download version config from OSS/CDN, compare, launch upgrade process | Core | Optional | Version config hosted on object storage |
| OSS Upgrade Update | OSS-mode upgrade process downloads and extracts resource packages | Core | Optional | Paired with OssClient |
| Silent Background Update | Background polling, silent download, upgrade on process exit | Core | Optional | Set `Option.Silent = true` |
| Differential Patch Pipeline | File-level binary diff generation & application, directory-level batch distribution | Core | Optional | Requires `Option.PatchEnabled = true` |
| Concurrent Downloads | Multi-asset concurrent download with resume & SHA256 verification | Core | Optional | Controlled via `Option.MaxConcurrency` |
| Event Callbacks | 7 event types: version info, progress, completion, errors, exceptions | Core | Optional | Registered via `AddListener*` methods |
| App Identity Manifest | `generalupdate.manifest.json` auto-discovery & version write-back | Extended | Recommended | Generated by `GeneralUpdate.Tools` |
| Custom Download Source | Custom version list and download resource source | Extended | Optional | Implement `IDownloadSource` |
| Custom Download Executor | Custom single-file download (HTTP/FTP/SFTP etc.) | Extended | Optional | Implement `IDownloadExecutor` |
| Custom Retry Policy | Custom retry, timeout, circuit-breaking strategy | Extended | Optional | Implement `IDownloadPolicy` |
| Custom Download Pipeline | Post-download processing (verification, decryption, scanning) | Extended | Optional | Implement `IDownloadPipeline` |
| Custom Download Orchestrator | Fully replace batch download concurrency control | Extended | Optional | Implement `IDownloadOrchestrator` |
| Lifecycle Hooks | Business logic injection: before/after update, download complete, error, before start | Extended | Optional | Implement `IUpdateHooks` |
| Status Reporting | Report update status to your own server | Extended | Optional | Implement `IUpdateReporter` |
| HTTP Authentication | Custom HTTP request authentication headers | Extended | Optional | Implement `IHttpAuthProvider` |
| SSL Certificate Policy | Custom HTTPS certificate validation logic | Extended | Optional | Implement `ISslValidationPolicy` |
| Platform Strategy | Replace platform-level file operations or launch logic | Extended | Optional | Implement `IStrategy` |
| SignalR Real-Time Push | Server proactively pushes version update notifications to connected clients | Extended | Optional | `UpgradeHubService`, namespace `GeneralUpdate.Core.Hubs` |
| Push Reconnect Mechanism | Auto-reconnect on disconnect (random backoff strategy), connection lifecycle management | Extended | Optional | `RandomRetryPolicy` |
| Push Event Subscription | Four events: receive message, online status, reconnect notification, close notification | Extended | Optional | Registered via `AddListener*` methods |

---

## 3. API Configuration Reference

### 3.1 Configuration Properties (Props)

**UpdateRequest Properties:**

| Field | Type | Default | Required | Values | Description |
| --- | --- | --- | --- | --- | --- |
| `UpdateUrl` | `string` | — | Yes | Valid absolute URL | Update check API endpoint |
| `UpdateAppName` | `string` | `"Update.exe"` | Recommended | Valid filename | Upgrade process filename |
| `MainAppName` | `string` | — | Recommended | Valid filename | Main app filename for restart & identification |
| `ClientVersion` | `string` | — | Recommended | SemVer format | Current main app version |
| `AppSecretKey` | `string` | — | Recommended | — | App key for server authentication |
| `InstallPath` | `string` | `BaseDirectory` | Optional | Valid directory path | Application install root |
| `ReportUrl` | `string` | `null` | Optional | Valid absolute URL | Status report API |
| `UpdateLogUrl` | `string` | `null` | Optional | Valid absolute URL | Changelog page URL |
| `UpgradeClientVersion` | `string` | — | Optional | SemVer format | Upgrade process version |
| `ProductId` | `string` | — | Optional | — | Product identifier for multi-product servers |
| `UpdatePath` | `string` | `InstallPath` | Optional | Valid directory path | Upgrade process location |
| `Bowl` | `string` | `null` | Optional | Valid filename | Auxiliary process name to close before update |
| `Scheme` | `string` | `null` | Optional | `"Bearer"` etc. | Auth scheme (used with `Token`) |
| `Token` | `string` | `null` | Optional | — | Auth token |
| `AuthScheme` | `AuthScheme` | `Hmac` | Optional | `Hmac`, `Bearer`, `Basic` | Auth scheme enum; `Hmac` for GeneralSpacestation server-side signed auth |
| `BasicUsername` | `string` | `null` | Optional | — | HTTP Basic auth username (requires `AuthScheme = Basic`) |
| `BasicPassword` | `string` | `null` | Optional | — | HTTP Basic auth password (requires `AuthScheme = Basic`) |
| `Files` | `List<string>` | `null` | Optional | — | Files to skip during update |
| `Formats` | `List<string>` | `null` | Optional | — | Extensions to skip during update |
| `Directories` | `List<string>` | `null` | Optional | — | Directories to skip during update |
| `DriverDirectory` | `string` | `null` | Optional | Valid directory path | Driver update directory |

**Option Runtime Options:**

| Field | Type | Default | Required | Values | Description |
| --- | --- | --- | --- | --- | --- |
| `Option.AppType` | `AppType` | `Client` | Yes | `Client(1)`, `Upgrade(2)`, `OssClient(3)`, `OssUpgrade(4)` | Current process role |
| `Option.DiffMode` | `DiffMode` | `Serial` | Optional | `Serial`, `Parallel` | Download execution mode |
| `Option.Encoding` | `Encoding` | `UTF8` | Optional | `Encoding` instance | Archive processing encoding |
| `Option.Format` | `Format` | `Zip` | Optional | `Zip` | Package format |
| `Option.DownloadTimeout` | `int?` | `30` | Optional | Positive integer (sec) | Download timeout |
| `Option.PatchEnabled` | `bool?` | `true` | Optional | `true` / `false` | Enable differential patching |
| `Option.BackupEnabled` | `bool?` | `true` | Optional | `true` / `false` | Backup files before update |
| `Option.Silent` | `bool` | `false` | Optional | `true` / `false` | Enable silent polling |
| `Option.SilentPollIntervalMinutes` | `int` | `60` | Optional | Positive integer | Polling interval (minutes) |
| `Option.LaunchClientAfterUpdate` | `bool` | `true` | Optional | `true` / `false` | Launch main app after upgrade |
| `Option.MaxConcurrency` | `int` | `3` | Optional | `1` ~ `ProcessorCount × 2` | Max download concurrency |
| `Option.EnableResume` | `bool` | `true` | Optional | `true` / `false` | Enable HTTP Range resume |
| `Option.RetryCount` | `int` | `3` | Optional | Non-negative integer | Download retry count |
| `Option.VerifyChecksum` | `bool` | `true` | Optional | `true` / `false` | Verify download file hash |
| `Option.RetryInterval` | `TimeSpan` | `1s` | Optional | Positive `TimeSpan` | Download retry interval |

### 3.2 Instance Methods

**GeneralUpdateBootstrap:**

| Method | Parameters | Returns | Use Case | Notes |
| --- | --- | --- | --- | --- |
| `LaunchAsync()` | None | `Task<GeneralUpdateBootstrap>` | Final entry for all Core scenarios | Auto-selects strategy based on `Option.AppType` |
| `Cancel()` | None | `void` | UI "Cancel Update" button | Triggers internal `CancellationTokenSource` |
| `SetConfig(UpdateRequest)` | `configInfo` | `GeneralUpdateBootstrap` | Explicit update configuration | Calls `Validate()` on key fields |
| `SetConfig(string)` | `filePath` — JSON config file path | `GeneralUpdateBootstrap` | Read config from file | Supports relative/absolute paths; UTF-8 JSON |
| `SetSource(...)` | `updateUrl`, `appSecretKey`, `reportUrl?`, `scheme?`, `token?`, `authScheme?`, `basicUsername?`, `basicPassword?`, `installPath?` | `GeneralUpdateBootstrap` | Lightweight entry with manifest | Identity info filled by manifest; supports HMAC / Bearer / Basic auth methods |
| `SetOption(Option<T>, T)` | `option` — key, `value` — value | `GeneralUpdateBootstrap` | Set runtime options | Pass `null` to reset nullable options |
| `UseDiffPipeline(Action<DiffPipelineBuilder>)` | `configure` — delegate | `GeneralUpdateBootstrap` | Replace or tune diff pipeline | Default used if not called |
| `AddListenerUpdateInfo(...)` | `EventHandler<UpdateInfoEventArgs>` | `GeneralUpdateBootstrap` | Receive server version info | Also fires when no update available |
| `AddListenerUpdatePrecheck(...)` | `Func<UpdateInfoEventArgs, bool>` | `GeneralUpdateBootstrap` | Pre-download check | Return `true` to skip non-forced update |
| `AddListenerProgress(...)` | `EventHandler<ProgressEventArgs>` | `GeneralUpdateBootstrap` | Progress bar, status text | Contains both download & diff progress |
| `AddListenerMultiDownloadCompleted(...)` | `EventHandler<MultiDownloadCompletedEventArgs>` | `GeneralUpdateBootstrap` | Mark single asset download completion | Not "all downloads complete" |
| `AddListenerMultiAllDownloadCompleted(...)` | `EventHandler<MultiAllDownloadCompletedEventArgs>` | `GeneralUpdateBootstrap` | Post-all-downloads processing | Includes `FailedVersions` summary |
| `AddListenerMultiDownloadError(...)` | `EventHandler<MultiDownloadErrorEventArgs>` | `GeneralUpdateBootstrap` | Log single download failure | Overall success still determined by `MultiAllDownloadCompleted` |
| `AddListenerMultiDownloadStatistics(...)` | `EventHandler<MultiDownloadStatisticsEventArgs>` | `GeneralUpdateBootstrap` | Display speed & ETA | Prefer `AddListenerProgress` for new code |
| `AddListenerException(...)` | `EventHandler<ExceptionEventArgs>` | `GeneralUpdateBootstrap` | Report exceptions, show errors | Notification only; no automatic retry |
| `AddEventListener<TListener>()` | Generic — listener type | `GeneralUpdateBootstrap` | Batch register event listeners | `T` must implement `IUpdateEventListener` |
| `Hooks<T>()` | Generic — hook type | `GeneralUpdateBootstrap` | Register lifecycle hooks | `T` needs parameterless constructor |
| `UpdateReporter<T>()` | Generic — reporter type | `GeneralUpdateBootstrap` | Register status reporter | — |
| `SslPolicy<T>()` | Generic — SSL policy type | `GeneralUpdateBootstrap` | Custom HTTPS certificate validation | Don't unconditionally return `true` in production |
| `HttpAuth<T>()` | Generic — auth provider type | `GeneralUpdateBootstrap` | Custom HTTP auth | — |
| `DownloadSource<T>()` | Generic — download source type | `GeneralUpdateBootstrap` | Custom version list source | — |
| `DownloadPolicy<T>()` | Generic — download policy type | `GeneralUpdateBootstrap` | Custom retry/timeout policy | — |
| `DownloadExecutor<T>()` | Generic — executor type | `GeneralUpdateBootstrap` | Custom single-file download | — |
| `DownloadPipeline<T>()` | Generic — pipeline type | `GeneralUpdateBootstrap` | Custom post-download processing | — |
| `DownloadOrchestrator<T>()` | Generic — orchestrator type | `GeneralUpdateBootstrap` | Fully replace batch download | Only when complete replacement needed |
| `Strategy<T>()` | Generic — strategy type | `GeneralUpdateBootstrap` | Custom platform strategy | — |

**UpgradeHubService:**

| Method | Parameters | Returns | Use Case | Notes |
| --- | --- | --- | --- | --- |
| `UpgradeHubService(string, string?, string?)` | `url` — SignalR Hub URL; `token` — optional ID4 auth token; `appkey` — optional client unique ID | — (constructor) | Create push service instance | `appkey` used for server-side targeted push; recommended to use a fixed GUID |
| `StartAsync()` | None | `Task` | Establish SignalR long-lived connection | Can re-call after `StopAsync` |
| `StopAsync()` | None | `Task` | Gracefully stop connection, retain reconnect ability | Suitable when app goes to background |
| `DisposeAsync()` | None | `Task` | Fully release Hub and all resources | Cannot be reused after disposal |
| `AddListenerReceive(Action<string>)` | `receiveMessageCallback` | `void` | Subscribe to server push messages | Message content is JSON string from server |
| `AddListenerOnline(Action<string>)` | `onlineMessageCallback` | `void` | Subscribe to online/offline status changes | — |
| `AddListenerReconnected(Func<string?, Task>?)` | `reconnectedCallback` | `void` | Subscribe to reconnect success notification | Parameter is new connectionId (may be null) |
| `AddListenerClosed(Func<Exception?, Task>)` | `closeCallback` | `void` | Subscribe to connection close notification | Exception is null for normal close |

### 3.3 Callback Events

| Event | Callback Parameters | Trigger Timing | Usage Notes |
| --- | --- | --- | --- |
| `AddListenerUpdateInfo` | `UpdateInfoEventArgs` — `Info.Code`, `Info.Body` (`VersionEntry` list) | After version comparison in standard Client strategy | No update → `Code = 404`; has update → `Body` contains `VersionEntry` list |
| `AddListenerUpdatePrecheck` | `Func<UpdateInfoEventArgs, bool>` — return `true` to skip (non-forced), `false` to continue | After `UpdateInfo`, before download | For disk space check, network check, user confirmation dialog |
| `AddListenerProgress` | `ProgressEventArgs` — `Progress` (download) or `DiffProgress` (diff) | Download progress or diff progress updates | Only one of `Progress` / `DiffProgress` is non-null per event |
| `AddListenerMultiDownloadCompleted` | `MultiDownloadCompletedEventArgs` — `Version`, `IsCompleted` | Single asset download completion | Not "all downloads complete" |
| `AddListenerMultiAllDownloadCompleted` | `MultiAllDownloadCompletedEventArgs` — `IsAllDownloadCompleted`, `FailedVersions` | After all download tasks complete | Failure details in `FailedVersions` |
| `AddListenerMultiDownloadError` | `MultiDownloadErrorEventArgs` — `Exception`, `Version` | Single download failure | Record failures for display/monitoring |
| `AddListenerMultiDownloadStatistics` | `MultiDownloadStatisticsEventArgs` — `Speed`, `Remaining`, `BytesReceived` | Legacy/compat download statistics | New code should use `AddListenerProgress` |
| `AddListenerException` | `ExceptionEventArgs` — `Exception`, `Message` | When strategies catch exceptions | Notification only; no automatic retry |

**UpgradeHubService Push Events:**

| Event | Callback Parameters | Trigger Timing | Usage Notes |
| --- | --- | --- | --- |
| `AddListenerReceive` | `Action<string>` — message content (JSON string) | When server pushes version update | Message format determined by server |
| `AddListenerOnline` | `Action<string>` — status description | When online/offline status changes | Use for UI status display |
| `AddListenerReconnected` | `Func<string?, Task>?` — new connectionId | After successful reconnect | Can refresh client state |
| `AddListenerClosed` | `Func<Exception?, Task>` — close reason (null = normal) | When connection closes | Use for logging and cleanup |

---

## 4. Advanced Examples

### 4.1 Extension Points Overview

Core provides 10 extension registration methods via `AbstractBootstrap`, all returning the bootstrap instance for fluent chaining. All registered types must have parameterless constructors.

| Extension Interface | Registration Method | Scope |
| --- | --- | --- |
| `IUpdateHooks` | `Hooks<T>()` | Update lifecycle hooks |
| `IUpdateReporter` | `UpdateReporter<T>()` | Status reporting |
| `ISslValidationPolicy` | `SslPolicy<T>()` | HTTPS certificate validation |
| `IHttpAuthProvider` | `HttpAuth<T>()` | HTTP request authentication |
| `IDownloadSource` | `DownloadSource<T>()` | Version list & download source |
| `IDownloadPolicy` | `DownloadPolicy<T>()` | Download retry/timeout/circuit-breaker |
| `IDownloadExecutor` | `DownloadExecutor<T>()` | Single file download |
| `IDownloadPipeline` | `DownloadPipeline<T>()` | Post-download processing |
| `IDownloadOrchestrator` | `DownloadOrchestrator<T>()` | Batch download orchestration |
| `IStrategy` | `Strategy<T>()` | Platform-level update strategy |

### 4.2 Examples by Scenario

#### Scenario 1: Custom Diff Algorithm with Parallelism

**Description:** Large projects wanting faster client-side patch application with `StreamingHdiffDiffer` and parallelism 4.

```csharp
using GeneralUpdate.Core;
using GeneralUpdate.Core.Differential;
using GeneralUpdate.Core.Pipeline;
using GeneralUpdate.Differential.Differ;

await new GeneralUpdateBootstrap()
    .SetConfig(request)
    .UseDiffPipeline(builder =>
    {
        builder
            .UseDiffer(new StreamingHdiffDiffer())
            .WithParallelism(4)
            .WithStopOnFirstError(true);
    })
    .SetOption(Option.PatchEnabled, true)
    .SetOption(Option.AppType, AppType.Client)
    .LaunchAsync();
```

#### Scenario 2: Custom Lifecycle Hooks

**Description:** Check disk space before update, write logs after update, grant execute permissions on Linux/macOS.

```csharp
using GeneralUpdate.Core.Hooks;

public sealed class ProductUpdateHooks : IUpdateHooks
{
    public Task<bool> OnBeforeUpdateAsync(HookContext ctx)
    {
        var drive = new DriveInfo(Path.GetPathRoot(ctx.InstallPath)!);
        if (drive.AvailableFreeSpace < 500L * 1024 * 1024)
            return Task.FromResult(false); // Reject update
        return Task.FromResult(true);
    }

    public Task OnDownloadCompletedAsync(DownloadContext ctx) => Task.CompletedTask;

    public Task OnAfterUpdateAsync(HookContext ctx)
    {
        File.AppendAllText(
            Path.Combine(ctx.InstallPath, "update-history.log"),
            $"{DateTimeOffset.Now:O} {ctx.CurrentVersion} -> {ctx.TargetVersion}{Environment.NewLine}");
        return Task.CompletedTask;
    }

    public Task OnUpdateErrorAsync(HookContext ctx, Exception ex)
    {
        File.AppendAllText(Path.Combine(ctx.InstallPath, "update-error.log"), $"{ex}{Environment.NewLine}");
        return Task.CompletedTask;
    }

    public Task OnBeforeStartAppAsync(HookContext ctx) => Task.CompletedTask;
}

await new GeneralUpdateBootstrap()
    .SetConfig(request)
    .Hooks<ProductUpdateHooks>()
    .SetOption(Option.AppType, AppType.Client)
    .LaunchAsync();
```

#### Scenario 3: Custom Download Source (Private Service/Config Center)

**Description:** Pull download asset lists from an internal config center instead of the standard version check API.

```csharp
using GeneralUpdate.Core.Download.Abstractions;
using GeneralUpdate.Core.Download.Models;

public sealed class ConfigCenterDownloadSource : IDownloadSource
{
    public async Task<DownloadSourceResult> ListAsync(CancellationToken token = default)
    {
        var assets = new[]
        {
            new DownloadAsset(
                Name: "MyApp-2.0.0.zip",
                Url: "https://cdn.internal.example.com/releases/MyApp-2.0.0.zip",
                Size: 50_000_000,
                SHA256: "abc123...",
                Version: "2.0.0")
        };
        return new DownloadSourceResult
        {
            Assets = assets,
            HasMainUpdate = true,
            HasUpgradeUpdate = false
        };
    }
}

await new GeneralUpdateBootstrap()
    .SetConfig(request)
    .DownloadSource<ConfigCenterDownloadSource>()
    .SetOption(Option.AppType, AppType.Client)
    .LaunchAsync();
```

#### Scenario 4: Custom HTTP Authentication

**Description:** Append JWT Bearer Token to all HTTP requests from Core.

```csharp
using GeneralUpdate.Core.Security;

public sealed class JwtAuthProvider : IHttpAuthProvider
{
    private readonly string _token = Environment.GetEnvironmentVariable("UPDATE_JWT_TOKEN") ?? "";

    public Task ApplyAuthAsync(HttpRequestMessage request, CancellationToken token = default)
    {
        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
        return Task.CompletedTask;
    }
}

await new GeneralUpdateBootstrap()
    .SetConfig(request)
    .HttpAuth<JwtAuthProvider>()
    .SetOption(Option.AppType, AppType.Client)
    .LaunchAsync();
```

#### Scenario 5: Silent Update + Process Exit Trigger

**Description:** Main app polls for updates in background, triggers upgrade on process exit.

```csharp
using GeneralUpdate.Core;

var bootstrap = new GeneralUpdateBootstrap()
    .SetSource(
        updateUrl: "https://update.example.com/api/upgrade/verification",
        appSecretKey: "your-app-secret")
    .SetOption(Option.AppType, AppType.Client)
    .SetOption(Option.Silent, true)
    .SetOption(Option.SilentPollIntervalMinutes, 30)
    .SetOption(Option.LaunchClientAfterUpdate, true)
    .AddListenerException((_, e) => Console.WriteLine($"Update error: {e.Message}"));

await bootstrap.LaunchAsync();

// On app exit: launch upgrade if prepared
AppDomain.CurrentDomain.ProcessExit += (_, _) =>
{
    if (bootstrap.SilentOrchestrator?.HasPreparedUpdate == true)
        bootstrap.SilentOrchestrator.TryLaunchUpgrade();
};
```

#### Scenario 6: SignalR Real-Time Push + Standard Update

**Description:** Use `UpgradeHubService` for server push notifications alongside `GeneralUpdateBootstrap` for standard updates. Server can push notifications immediately when new versions are available.

```csharp
using GeneralUpdate.Core;
using GeneralUpdate.Core.Hubs;

// 1. Start SignalR push listener
var hub = new UpgradeHubService(
    "http://localhost:5000/UpgradeHub",
    appkey: "dfeb5833-975e-4afb-88f1-6278ee9aeff6");

hub.AddListenerReceive(async (message) =>
{
    Console.WriteLine($"Push notification: {message}");
    // Trigger update check or notify user in UI
});

hub.AddListenerOnline((info) =>
    Console.WriteLine($"Online status: {info}"));

hub.AddListenerReconnected((connectionId) =>
{
    Console.WriteLine($"Reconnected, connectionId={connectionId}");
    return Task.CompletedTask;
});

hub.AddListenerClosed((exception) =>
{
    Console.WriteLine(exception != null
        ? $"Connection closed abnormally: {exception.Message}"
        : "Connection closed normally");
    return Task.CompletedTask;
});

await hub.StartAsync();

// 2. Standard update flow
await new GeneralUpdateBootstrap()
    .SetSource(
        updateUrl: "https://update.example.com/api/upgrade/verification",
        appSecretKey: "your-app-secret")
    .SetOption(Option.AppType, AppType.Client)
    .AddListenerException((_, e) => Console.WriteLine(e.Exception))
    .LaunchAsync();

// 3. Cleanup on exit
// await hub.StopAsync();
// await hub.DisposeAsync();
```

#### Scenario 7: DI Container Registration for UpgradeHubService

**Description:** Register `IUpgradeHubService` in Prism / Generic Host / ASP.NET Core DI containers.

```csharp
using GeneralUpdate.Core.Hubs;

// Prism example
protected override void RegisterTypes(IContainerRegistry containerRegistry)
{
    containerRegistry.Register<IUpgradeHubService, UpgradeHubService>();
}

public MainWindowViewModel(IUpgradeHubService hubService)
{
    hubService.AddListenerReceive((message) =>
        Console.WriteLine($"Push: {message}"));
    _ = hubService.StartAsync();
}

// Generic Host / ASP.NET Core example
builder.Services.AddSingleton<IUpgradeHubService>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new UpgradeHubService(
        config["HubUrl"]!,
        appkey: config["AppSecretKey"]);
});
```

---

## 5. Basic Usage Examples

### 5.1 Quick Start (Minimal Demo)

Minimal config using manifest for identity auto-discovery:

```csharp
using GeneralUpdate.Core;

await new GeneralUpdateBootstrap()
    .SetSource(
        updateUrl: "https://update.example.com/api/upgrade/verification",
        appSecretKey: "your-app-secret")
    .SetOption(Option.AppType, AppType.Client)
    .LaunchAsync();
```

Upgrade process entry point (`Update.exe`):

```csharp
await new GeneralUpdateBootstrap()
    .SetOption(Option.AppType, AppType.Upgrade)
    .AddListenerException((_, e) => Console.WriteLine(e.Exception))
    .LaunchAsync();
```

### 5.2 Basic Parameter Combination

```csharp
using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;

var request = new UpdateRequest
{
    UpdateUrl = "https://update.example.com/api/upgrade/verification",
    ReportUrl = "https://update.example.com/api/upgrade/report",
    UpdateAppName = "UpgradeSample.exe",
    MainAppName = "ClientSample.exe",
    InstallPath = AppDomain.CurrentDomain.BaseDirectory,
    ClientVersion = "1.0.0",
    AppSecretKey = "your-app-secret",
    ProductId = "your-product-id"
};

await new GeneralUpdateBootstrap()
    .SetConfig(request)
    .SetOption(Option.AppType, AppType.Client)
    .SetOption(Option.DiffMode, DiffMode.Parallel)
    .SetOption(Option.MaxConcurrency, 4)
    .SetOption(Option.PatchEnabled, true)
    .AddListenerProgress((_, e) =>
    {
        if (e.Progress != null)
            Console.WriteLine($"{e.Progress.AssetName}: {e.Progress.Percentage:F1}%");
    })
    .AddListenerException((_, e) => Console.WriteLine(e.Exception))
    .LaunchAsync();
```

### 5.2.1 SignalR Real-Time Push Quick Start

```csharp
using GeneralUpdate.Core.Hubs;

// Create push client
var hub = new UpgradeHubService(
    "http://localhost:5000/UpgradeHub",
    appkey: Guid.NewGuid().ToString());

// Subscribe to push messages
hub.AddListenerReceive((message) =>
{
    Console.WriteLine($"Push notification: {message}");
});

// Establish connection
await hub.StartAsync();

Console.WriteLine("Connected, waiting for server push...");
Console.ReadLine();

// Stop connection (retain reconnect ability)
await hub.StopAsync();

// Release resources (cannot be reused)
await hub.DisposeAsync();
```

### 5.3 Production-Ready Example

Full Client-side update with events, diff pipeline, concurrency control, and status reporting:

```csharp
using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;
using GeneralUpdate.Core.Pipeline;
using GeneralUpdate.Core.Models;
using GeneralUpdate.Core.Download;
using GeneralUpdate.Differential.Differ;

var request = new UpdateRequestBuilder()
    .SetUpdateUrl("https://update.mycompany.com/api/upgrade/verification")
    .SetReportUrl("https://update.mycompany.com/api/upgrade/report")
    .SetUpgradeAppName("MyApp.Upgrade.exe")
    .SetMainAppName("MyApp.exe")
    .SetClientVersion("1.0.0")
    .SetUpgradeClientVersion("1.0.0")
    .SetAppSecretKey("prod-secret-key")
    .SetProductId("my-product")
    .SetInstallPath(AppDomain.CurrentDomain.BaseDirectory)
    .Build();

var bootstrap = new GeneralUpdateBootstrap()
    .SetConfig(request)
    .SetOption(Option.AppType, AppType.Client)
    .SetOption(Option.DiffMode, DiffMode.Parallel)
    .SetOption(Option.MaxConcurrency, 4)
    .SetOption(Option.DownloadTimeout, 120)
    .SetOption(Option.PatchEnabled, true)
    .SetOption(Option.BackupEnabled, true)
    .SetOption(Option.VerifyChecksum, true)
    .SetOption(Option.RetryCount, 5)
    .SetOption(Option.RetryInterval, TimeSpan.FromSeconds(2))
    .UseDiffPipeline(builder => builder
        .UseDiffer(new StreamingHdiffDiffer())
        .WithParallelism(4))
    .AddListenerUpdateInfo((_, e) =>
    {
        Console.WriteLine(e.Info?.Code == "404"
            ? "Already up to date."
            : $"Found {e.Info?.Body?.Count ?? 0} version(s).");
    })
    .AddListenerProgress((_, e) =>
    {
        if (e.Progress != null)
            Console.WriteLine($"[Download] {e.Progress.AssetName}: {e.Progress.Percentage:F1}%");
        if (e.DiffProgress != null)
            Console.WriteLine($"[Patch] {e.DiffProgress.CurrentFile}: {e.DiffProgress.Completed}/{e.DiffProgress.Total}");
    })
    .AddListenerMultiAllDownloadCompleted((_, e) =>
    {
        Console.WriteLine(e.IsAllDownloadCompleted
            ? "All downloads completed."
            : $"Failed: {e.FailedVersions.Count}");
    })
    .AddListenerException((_, e) => Console.WriteLine($"Error: {e.Message}"));

await bootstrap.LaunchAsync();
```

---

## 6. Global Configuration

### Manifest Configuration

```json
{
  "mainAppName": "ClientSample.exe",
  "clientVersion": "1.0.0",
  "appType": "Client",
  "updateAppName": "UpgradeSample.exe",
  "upgradeClientVersion": "1.0.0",
  "productId": "sample-product",
  "updatePath": "update/"
}
```

### Configuration Priority

| Priority | Source | Description |
| --- | --- | --- |
| 1 (Highest) | Code: `SetConfig(UpdateRequest)` or `SetSource(...)` | Overrides all other sources |
| 2 | `generalupdate.manifest.json` fields | Auto-fills fields not explicitly set in code |
| 3 (Lowest) | Component internal defaults | `UpdateAppName = "Update.exe"`, `InstallPath = BaseDirectory`, etc. |

### Version Write-Back

After a successful update, Core automatically writes back the version to `generalupdate.manifest.json`:

| Scenario | Write-Back Field |
| --- | --- |
| Main app update completes | `ClientVersion` |
| Upgrade process update completes | `UpgradeClientVersion` |

### Logging Configuration

```csharp
using GeneralUpdate.Core;

// Disable logging (performance-sensitive scenarios)
GeneralTracer.SetTracingEnabled(false);

// Re-enable (troubleshooting)
GeneralTracer.SetTracingEnabled(true);

// Release logging resources
GeneralTracer.Dispose();
```

---

## Related Resources

- [GeneralUpdate Repository](https://github.com/GeneralLibrary/GeneralUpdate)
- [Samples Code](https://github.com/GeneralLibrary/GeneralUpdate-Samples)
- [GeneralUpdate.Tools](https://github.com/GeneralLibrary/GeneralUpdate.Tools)
- [Beginner Cookbook](../quickstart/Beginner%20cookbook)
- [Component docs](./GeneralUpdate.Core)
