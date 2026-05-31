---
sidebar_position: 5
---

# GeneralUpdate.Core

`GeneralUpdate.Core` is the update execution core of GeneralUpdate. It provides the programmable bootstrapper, configuration model, event model, download extension points, lifecycle hooks, reporting, differential pipeline, and platform strategy extension model. This page focuses on component APIs and extension examples; end-to-end workflows belong in the cookbook.

**Namespace:** `GeneralUpdate.Core`
**Primary entry point:** `GeneralUpdateBootstrap`
**NuGet package:** `GeneralUpdate.Core`

```bash
dotnet add package GeneralUpdate.Core
```

## Responsibility boundary

Core executes updates. It does not generate update packages or manage the server backend.

| Capability | Owned by Core | Notes |
| --- | --- | --- |
| Load update configuration | Yes | Via `UpdateRequest`, JSON config, `SetSource`, or IPC. |
| Query server version metadata | Yes | `Client` / `OssClient` builds the download plan. |
| Download update packages | Yes | Source, executor, policy, pipeline, and orchestrator are replaceable. |
| Verify and apply patches | Yes | Hash verification, archive handling, and differential patching are supported. |
| Replace files and restart app | Yes | `Upgrade` / `OssUpgrade` is used by the updater process. |
| Generate differential packages | No | Use `GeneralUpdate.Tools`. |

## Entry point: GeneralUpdateBootstrap

`GeneralUpdateBootstrap` is Core's main facade. It inherits `AbstractBootstrap<GeneralUpdateBootstrap, IStrategy>`, so it includes both its own methods and the extension registration methods from the base class.

```csharp
using GeneralUpdate.Core;

var bootstrap = new GeneralUpdateBootstrap();
```

### Method overview

| Method | Purpose | Typical use |
| --- | --- | --- |
| `LaunchAsync()` | Starts the update workflow using the current `Option.AppType`. | Final call in every Core scenario. |
| `Cancel()` | Requests cancellation of the current update operation. | A Cancel button in a desktop UI. |
| `SetConfig(UpdateRequest)` | Configures updates with a strongly typed object. | Explicit main-app configuration. |
| `SetConfig(string)` | Loads `UpdateRequest` from a JSON file. | `update_config.json` or another config file. |
| `SetSource(...)` | Supplies only URL, secret, report URL, scheme, and token. | Lightweight setup. |
| `SetOption(Option<T>, T)` | Sets runtime options. | Role, timeout, concurrency, patching, silent update. |
| `UseDiffPipeline(Action<DiffPipelineBuilder>)` | Customizes the differential pipeline. | Replace differ, set parallelism, collect patch progress. |
| `AddListenerUpdatePrecheck(Func<UpdateInfoEventArgs, bool>)` | Runs a pre-download decision callback. | Disk-space check, network check, user confirmation. |
| `AddListener...` | Registers a single event callback. | UI, logs, telemetry. |
| `AddEventListener<TListener>()` | Registers all event callbacks through a listener class. | Encapsulated event handling. |

### LaunchAsync

```csharp
public Task<GeneralUpdateBootstrap> LaunchAsync()
```

`LaunchAsync` reads `Option.AppType` and selects a role strategy:

| `Option.AppType` | Strategy | Description |
| --- | --- | --- |
| `AppType.Client` | `ClientStrategy` | Main-app side: check version, download packages, prepare context, launch updater. |
| `AppType.Upgrade` | `UpdateStrategy` | Updater side: read IPC context and replace files. |
| `AppType.OssClient` | `OssStrategy` | OSS main-app update role. |
| `AppType.OssUpgrade` | `OssStrategy` | OSS updater role. |

```csharp
await new GeneralUpdateBootstrap()
    .SetOption(Option.AppType, AppType.Upgrade)
    .AddListenerException((_, e) => Console.WriteLine(e.Exception))
    .LaunchAsync();
```

When the updater is launched by the main app, Core restores the update context through encrypted file IPC. In that case the updater normally does not call `SetConfig` again.

### Cancel

```csharp
public void Cancel()
```

`Cancel` signals the internal `CancellationTokenSource`. Strategies observe the token at safe checkpoints.

```csharp
private GeneralUpdateBootstrap? _bootstrap;

async Task StartUpdateAsync(UpdateRequest request)
{
    _bootstrap = new GeneralUpdateBootstrap()
        .SetConfig(request)
        .AddListenerException((_, e) => Console.WriteLine(e.Exception));

    await _bootstrap.LaunchAsync();
}

void CancelUpdate()
{
    _bootstrap?.Cancel();
}
```

### SetConfig(UpdateRequest)

```csharp
public GeneralUpdateBootstrap SetConfig(UpdateRequest configInfo)
```

`SetConfig(UpdateRequest)` validates the request and maps it into the internal `UpdateContext`. For non-`Upgrade` roles it also initializes the temporary update directory and blacklist matcher.

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
    ProductId = "your-product-id",
    Files = new List<string> { "appsettings.json" },
    Formats = new List<string> { ".log", ".tmp" },
    Directories = new List<string> { "logs", "cache" }
};

await new GeneralUpdateBootstrap()
    .SetConfig(request)
    .SetOption(Option.AppType, AppType.Client)
    .LaunchAsync();
```

### SetConfig(string)

```csharp
public GeneralUpdateBootstrap SetConfig(string filePath)
```

`SetConfig(string)` reads a UTF-8 JSON `UpdateRequest`. A bare filename is resolved from the app base directory; relative and absolute paths are used as provided.

```json
{
  "updateUrl": "https://update.example.com/api/upgrade/verification",
  "reportUrl": "https://update.example.com/api/upgrade/report",
  "updateAppName": "UpgradeSample.exe",
  "mainAppName": "ClientSample.exe",
  "installPath": "C:\\Program Files\\MyApp",
  "clientVersion": "1.0.0",
  "appSecretKey": "your-app-secret",
  "productId": "your-product-id"
}
```

```csharp
await new GeneralUpdateBootstrap()
    .SetConfig("update_config.json")
    .SetOption(Option.AppType, AppType.Client)
    .LaunchAsync();
```

### SetSource

```csharp
public GeneralUpdateBootstrap SetSource(
    string updateUrl,
    string appSecretKey,
    string? reportUrl = null,
    string? scheme = null,
    string? token = null)
```

`SetSource` is a lightweight entry point when identity metadata is provided by `generalupdate.manifest.json`, leaving only the server endpoint and secret in application code.

```csharp
await new GeneralUpdateBootstrap()
    .SetSource(
        updateUrl: "https://update.example.com/api/upgrade/verification",
        appSecretKey: "your-app-secret",
        reportUrl: "https://update.example.com/api/upgrade/report",
        scheme: "Bearer",
        token: "access-token")
    .SetOption(Option.AppType, AppType.Client)
    .LaunchAsync();
```

### UseDiffPipeline

```csharp
public GeneralUpdateBootstrap UseDiffPipeline(Action<DiffPipelineBuilder>? configure)
```

`UseDiffPipeline` customizes differential patch processing. Without it, Core builds a default pipeline using `BsdiffDiffer`, `DefaultCleanMatcher`, `DefaultDirtyMatcher`, parallelism `2`, and the Core progress reporter.

```csharp
using GeneralUpdate.Core.Differential;
using GeneralUpdate.Core.Models;
using GeneralUpdate.Core.Pipeline;
using GeneralUpdate.Differential.Differ;

await new GeneralUpdateBootstrap()
    .SetConfig(request)
    .UseDiffPipeline(builder =>
    {
        builder
            .UseDiffer(new StreamingHdiffDiffer())
            .UseCleanMatcher(new DefaultCleanMatcher())
            .UseDirtyMatcher(new DefaultDirtyMatcher())
            .WithParallelism(4)
            .WithStopOnFirstError(true)
            .WithProgress(new Progress<DiffProgress>(p =>
            {
                Console.WriteLine($"{p.Completed}/{p.Total}: {p.FileName}");
            }));
    })
    .SetOption(Option.PatchEnabled, true)
    .LaunchAsync();
```

## Configuration model: UpdateRequest

`UpdateRequest` is the main external configuration object. It inherits `UpdateConfiguration` and validates required fields in `Validate()`.

### Required or strongly recommended properties

| Property | Description |
| --- | --- |
| `UpdateUrl` | Version-check API URL. Must be an absolute URL. |
| `UpdateAppName` | Updater executable name. Defaults to `Update.exe`. |
| `MainAppName` | Main application executable name. |
| `ClientVersion` | Current main application version. |
| `AppSecretKey` | Secret shared with the update server. |
| `InstallPath` | Application install directory. Defaults to the current app base directory. |

### Optional properties

| Property | Description |
| --- | --- |
| `ReportUrl` | Update status report API. |
| `UpdateLogUrl` | Update log page URL. |
| `UpgradeClientVersion` | Updater application's own version. |
| `ProductId` | Product identifier when one server manages multiple products. |
| `UpdatePath` | Directory containing the updater; falls back to `InstallPath`. |
| `Bowl` | Helper process name to close before update. |
| `Scheme` / `Token` | Request authentication metadata. |
| `Files` | Specific files to skip during update. |
| `Formats` | File extensions to skip, such as `.log`. |
| `Directories` | Directories to skip. |

### UpdateRequestBuilder

`UpdateRequestBuilder` provides a fluent builder and validates on `Build()`.

```csharp
using GeneralUpdate.Core.Configuration;

var request = new UpdateRequestBuilder()
    .SetUpdateUrl("https://update.example.com/api/upgrade/verification")
    .SetReportUrl("https://update.example.com/api/upgrade/report")
    .SetUpgradeAppName("UpgradeSample.exe")
    .SetMainAppName("ClientSample.exe")
    .SetClientVersion("1.0.0")
    .SetAppSecretKey("your-app-secret")
    .SetProductId("your-product-id")
    .SetInstallPath(AppDomain.CurrentDomain.BaseDirectory)
    .SetFiles(new List<string> { "appsettings.json" })
    .SetFormats(new List<string> { ".log", ".tmp" })
    .SetDirectories(new List<string> { "logs" })
    .Build();
```

`UpdateRequestBuilder.Create()` attempts to load `update_config.json` from the app runtime directory and throws `FileNotFoundException` if it is missing.

```csharp
var request = UpdateRequestBuilder.Create().Build();
```

## Application identity manifest: generalupdate.manifest.json

`generalupdate.manifest.json` is the application identity manifest generated by `GeneralUpdate.Tools` and consumed by Core. It moves stable metadata such as the main executable name, current version, updater executable name, product ID, and updater directory out of code configuration. Application code can then focus on runtime and sensitive values such as server URLs, secrets, and tokens.

Place the file in the application install directory, the same directory referenced by `UpdateRequest.InstallPath`. By default, `InstallPath` is `AppDomain.CurrentDomain.BaseDirectory`, so desktop applications normally place the manifest in the main app output root.

```text
MyProduct/
├─ ClientSample.exe
├─ generalupdate.manifest.json
└─ update/
   └─ UpgradeSample.exe
```

### Manifest structure

The JSON generated by Tools uses camelCase property names. The Core-side type is `ManifestInfo`.

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

| JSON field | Core field | Description |
| --- | --- | --- |
| `mainAppName` | `MainAppName` | Main application executable name. Used to restart the app after update and identify the current product. |
| `clientVersion` | `ClientVersion` | Current main application version. Core sends it to the server to check whether a main-app update exists. |
| `appType` | `AppType` | Current process role, such as `Client`, `Upgrade`, `OssClient`, or `OssUpgrade`. |
| `updateAppName` | `UpdateAppName` | Updater executable name. Defaults to `Update.exe`. |
| `upgradeClientVersion` | `UpgradeClientVersion` | Updater application's own version. Core uses it to decide whether the updater must be updated first. |
| `productId` | `ProductId` | Product identifier for servers that manage multiple products. |
| `updatePath` | `UpdatePath` | Directory containing the updater. It can be relative to `InstallPath`, for example `update/`. |

The manifest intentionally does not include `UpdateUrl`, `ReportUrl`, `AppSecretKey`, `Scheme`, or `Token`. This keeps Tools responsible for build-time identity metadata while secrets still come from application code, a configuration service, or the deployment environment.

### How Tools generates the manifest

The `GeneralUpdate.Tools` configuration flow parses the main-app and updater `.csproj` files, validates version values, and emits `generalupdate.manifest.json`.

| Tools step | Responsibility |
| --- | --- |
| `CsprojParseStep` | Parses the main application `.csproj`; if an updater `.csproj` is provided, parses it as well. |
| `SemverValidateStep` | Validates that `ClientVersion` and `UpgradeClientVersion` use semver, for example `1.0.0`. |
| `ManifestBuildStep` | Fills missing `MainAppName` / `UpdateAppName` from `.csproj` `AssemblyName` values. |
| `FileEmitStep` | Writes the manifest to the output directory with the fixed name `generalupdate.manifest.json`. |

The sample publishing flow in the configuration UI also calls `SamplePublisherService.PublishAsync(...)` to place the main-app output, updater output, and manifest into one runnable sample directory. New users therefore do not need to hand-write a complete `UpdateRequest`: they can generate the manifest with Tools and only add server endpoints and secrets in application code.

### Using the manifest with the bootstrap

With the manifest in place, application code does not need to care about identity fields such as `MainAppName`, `ClientVersion`, `UpdateAppName`, `UpgradeClientVersion`, `ProductId`, or `UpdatePath`, and it does not need to load `generalupdate.manifest.json` manually. The bootstrap reads `InstallPath/generalupdate.manifest.json` internally during the update workflow and carries that identity metadata into version checking, downloading, updater launch, and version write-back.

When the install directory is the current application directory, pass only the server endpoint and secret to `SetSource`:

```csharp
await new GeneralUpdateBootstrap()
    .SetSource(
        updateUrl: "https://update.example.com/api/upgrade/verification",
        appSecretKey: "your-app-secret",
        reportUrl: "https://update.example.com/api/upgrade/report")
    .SetOption(Option.AppType, AppType.Client)
    .LaunchAsync();
```

If the actual install directory is not the current process base directory, provide `InstallPath` through `UpdateRequest` while still keeping manifest identity fields out of code:

```csharp
using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;

var request = new UpdateRequest
{
    UpdateUrl = "https://update.example.com/api/upgrade/verification",
    ReportUrl = "https://update.example.com/api/upgrade/report",
    AppSecretKey = "your-app-secret",
    InstallPath = @"C:\Program Files\MyProduct"
};

await new GeneralUpdateBootstrap()
    .SetConfig(request)
    .SetOption(Option.AppType, AppType.Client)
    .LaunchAsync();
```

The recommended responsibility split is:

| Provided by manifest | Provided by code or environment |
| --- | --- |
| `MainAppName`, `ClientVersion`, `UpdateAppName`, `UpgradeClientVersion`, `ProductId`, `UpdatePath` | `UpdateUrl`, `ReportUrl`, `AppSecretKey`, `Scheme`, `Token`, events, extension points, runtime options |

### Version write-back

The manifest is not only read at startup. After a successful update, Core writes the new version back to the same `generalupdate.manifest.json` under the install directory:

| Scenario | Field written back |
| --- | --- |
| Main application update completed | `ClientVersion` |
| Updater self-update completed | `UpgradeClientVersion` |

On the next polling cycle or process start, Core therefore validates from the latest applied version instead of the build-time version. This requires the install directory to be writable; if the application is installed under a restricted directory, ensure the updater has permission to update the manifest file.

## Runtime options: Option

Core uses strongly typed `Option<T>` values and sets them with `SetOption`.

```csharp
await new GeneralUpdateBootstrap()
    .SetConfig(request)
    .SetOption(Option.AppType, AppType.Client)
    .SetOption(Option.MaxConcurrency, 4)
    .SetOption(Option.VerifyChecksum, true)
    .LaunchAsync();
```

| Option | Type | Default | Description |
| --- | --- | --- | --- |
| `Option.AppType` | `AppType` | `Client` | Current process role. |
| `Option.DiffMode` | `DiffMode` | `Serial` | Differential execution mode. |
| `Option.Encoding` | `Encoding` | `UTF8` | Package processing encoding. |
| `Option.Format` | `Format` | `Zip` | Package archive format. |
| `Option.DownloadTimeout` | `int?` | `30` | Download timeout in seconds. |
| `Option.PatchEnabled` | `bool?` | `true` | Enables differential patch handling. |
| `Option.BackupEnabled` | `bool?` | `true` | Backs up replaced files. |
| `Option.Silent` | `bool` | `false` | Enables silent polling updates. |
| `Option.SilentPollIntervalMinutes` | `int` | `60` | Silent polling interval. |
| `Option.LaunchClientAfterUpdate` | `bool` | `true` | Starts the main app after update. |
| `Option.MaxConcurrency` | `int` | `3` | Max concurrent downloads. |
| `Option.EnableResume` | `bool` | `true` | Enables resumable downloads. |
| `Option.RetryCount` | `int` | `3` | Download retry count. |
| `Option.VerifyChecksum` | `bool` | `true` | Verifies downloaded file hashes. |
| `Option.RetryInterval` | `TimeSpan` | `1s` | Delay between retries. |

Passing `null` to a nullable option removes the custom value and falls back to the default.

## Events

Events are for observing update state. Complex business flow should be implemented with `IUpdateHooks` or documented in cookbook workflows.

### Individual callbacks

| Method | Argument type | Trigger |
| --- | --- | --- |
| `AddListenerUpdateInfo` | `UpdateInfoEventArgs` | After server version metadata is returned. |
| `AddListenerUpdatePrecheck` | `Func<UpdateInfoEventArgs, bool>` | Before download starts; `true` continues, `false` aborts. |
| `AddListenerMultiDownloadStatistics` | `MultiDownloadStatisticsEventArgs` | During download. |
| `AddListenerMultiDownloadCompleted` | `MultiDownloadCompletedEventArgs` | One version download completes. |
| `AddListenerMultiAllDownloadCompleted` | `MultiAllDownloadCompletedEventArgs` | All download tasks complete. |
| `AddListenerMultiDownloadError` | `MultiDownloadErrorEventArgs` | Download failure. |
| `AddListenerProgress` | `ProgressEventArgs` | Download or differential progress changes. |
| `AddListenerException` | `ExceptionEventArgs` | Core catches an exception. |

```csharp
await new GeneralUpdateBootstrap()
    .SetConfig(request)
    .AddListenerUpdateInfo((_, e) =>
    {
        Console.WriteLine($"Versions from server: {e.Info?.Body?.Count ?? 0}");
    })
    .AddListenerUpdatePrecheck(e =>
    {
        var hasUpdate = (e.Info?.Body?.Count ?? 0) > 0;
        var enoughDisk = DriveInfo.GetDrives()
            .Where(d => d.IsReady)
            .Any(d => d.AvailableFreeSpace > 1024L * 1024 * 1024);

        return hasUpdate && enoughDisk;
    })
    .AddListenerMultiDownloadStatistics((_, e) =>
    {
        Console.WriteLine($"{e.ProgressPercentage}% {e.Speed} {e.BytesReceived}/{e.TotalBytesToReceive}");
    })
    .AddListenerMultiDownloadCompleted((_, e) =>
    {
        Console.WriteLine(e.IsCompleted ? "Download completed." : "Download failed.");
    })
    .AddListenerProgress((_, e) =>
    {
        if (e.Progress != null)
            Console.WriteLine($"Download: {e.Progress.Percentage}%");

        if (e.DiffProgress != null)
            Console.WriteLine($"Patch: {e.DiffProgress.Completed}/{e.DiffProgress.Total}");
    })
    .AddListenerException((_, e) =>
    {
        Console.WriteLine(e.Message);
        Console.WriteLine(e.Exception);
    })
    .LaunchAsync();
```

### Listener class

Implement `IUpdateEventListener` to centralize event handling. Inherit `UpdateEventListenerBase` if you only need some events.

```csharp
using GeneralUpdate.Core.Download;
using GeneralUpdate.Core.Event;

public sealed class ConsoleUpdateListener : UpdateEventListenerBase
{
    public override void OnUpdateInfo(UpdateInfoEventArgs args)
    {
        Console.WriteLine($"Update count: {args.Info?.Body?.Count ?? 0}");
    }

    public override void OnDownloadStatistics(MultiDownloadStatisticsEventArgs args)
    {
        Console.WriteLine($"{args.ProgressPercentage}% {args.Speed}");
    }

    public override void OnException(ExceptionEventArgs args)
    {
        Console.WriteLine(args.Exception);
    }
}

await new GeneralUpdateBootstrap()
    .SetConfig(request)
    .AddEventListener<ConsoleUpdateListener>()
    .LaunchAsync();
```

## Extension points

All extension registration methods are provided by `AbstractBootstrap` and can be chained.

| Registration method | Interface | Scope |
| --- | --- | --- |
| `Hooks<T>()` | `IUpdateHooks` | Lifecycle callbacks. |
| `UpdateReporter<T>()` | `IUpdateReporter` | Update status reporting. |
| `SslPolicy<T>()` | `ISslValidationPolicy` | HTTPS certificate validation. |
| `UpdateAuth<T>()` | `IHttpAuthProvider` | HTTP request authentication. |
| `DownloadSource<T>()` | `IDownloadSource` | Version manifest and asset source. |
| `DownloadPolicy<T>()` | `IDownloadPolicy` | Retry, timeout, circuit breaker. |
| `DownloadExecutor<T>()` | `IDownloadExecutor` | Single-file download implementation. |
| `DownloadPipeline<T>()` | `IDownloadPipeline` | Post-download verification or transformation. |
| `DownloadOrchestrator<T>()` | `IDownloadOrchestrator` | Full batch download orchestration. |
| `Strategy<T>()` | `IStrategy` | Platform-level update strategy. |

Registered types must have a parameterless constructor because Core creates them with `new()` or reflection. If dependencies are required, wrap them in a parameterless adapter.

## Lifecycle hooks: IUpdateHooks

`IUpdateHooks` is best for business logic before update, after download, after update, before app start, and on errors. It is also a flexible open extension point: on Linux or macOS, updated executables may need execute permission restored, or an organization may need to run an internal authorization, signature-check, or permission-repair script before the main app starts. Put that work in `OnBeforeStartAppAsync`.

```csharp
using GeneralUpdate.Core.Hooks;

public sealed class ProductUpdateHooks : IUpdateHooks
{
    public Task<bool> OnBeforeUpdateAsync(HookContext ctx)
    {
        Console.WriteLine($"Before update: {ctx.CurrentVersion} -> {ctx.TargetVersion}");
        return Task.FromResult(true);
    }

    public Task OnDownloadCompletedAsync(DownloadContext ctx)
    {
        Console.WriteLine($"Downloaded {ctx.AssetName}, success={ctx.Success}, path={ctx.LocalPath}");
        return Task.CompletedTask;
    }

    public Task OnAfterUpdateAsync(HookContext ctx)
    {
        File.WriteAllText(Path.Combine(ctx.InstallPath, "last-update.txt"), DateTimeOffset.Now.ToString("O"));
        return Task.CompletedTask;
    }

    public Task OnUpdateErrorAsync(HookContext ctx, Exception ex)
    {
        File.AppendAllText(Path.Combine(ctx.InstallPath, "update-error.log"), ex + Environment.NewLine);
        return Task.CompletedTask;
    }

    public Task OnBeforeStartAppAsync(HookContext ctx)
    {
        Console.WriteLine($"Starting app from {ctx.InstallPath}");
        return Task.CompletedTask;
    }
}

await new GeneralUpdateBootstrap()
    .SetConfig(request)
    .Hooks<ProductUpdateHooks>()
    .LaunchAsync();
```

On Linux/macOS, you can register the built-in `UnixPermissionHooks` to let Core run `chmod +x` before starting the app:

```csharp
await new GeneralUpdateBootstrap()
    .SetConfig(request)
    .Hooks<UnixPermissionHooks>()
    .LaunchAsync();
```

If you need to run your own permission script, wrap it in a parameterless hook adapter and register that adapter with `Hooks<T>()`:

```csharp
using GeneralUpdate.Core.Hooks;

public sealed class ProductPermissionHooks : IUpdateHooks
{
    private readonly CustomPermissionHooks _inner =
        new("/opt/my-product/scripts/fix-permissions.sh");

    public Task OnBeforeStartAppAsync(HookContext ctx)
        => _inner.OnBeforeStartAppAsync(ctx);

    public Task<bool> OnBeforeUpdateAsync(HookContext ctx) => Task.FromResult(true);
    public Task OnDownloadCompletedAsync(DownloadContext ctx) => Task.CompletedTask;
    public Task OnAfterUpdateAsync(HookContext ctx) => Task.CompletedTask;
    public Task OnUpdateErrorAsync(HookContext ctx, Exception ex) => Task.CompletedTask;
}

await new GeneralUpdateBootstrap()
    .SetConfig(request)
    .Hooks<ProductPermissionHooks>()
    .LaunchAsync();
```

Built-in hook types:

| Type | Description |
| --- | --- |
| `NoOpUpdateHooks` | Default no-op implementation. |
| `UnixPermissionHooks` | Runs `chmod +x` before starting the app on Unix-like systems. |
| `CustomPermissionHooks` | Runs a custom permission script; wrap it before using `Hooks<T>()` because its constructor requires arguments. |

## Status reporting: IUpdateReporter

`IUpdateReporter` reports update status to a server or local telemetry.

```csharp
using GeneralUpdate.Core.Download.Reporting;

public sealed class ConsoleUpdateReporter : IUpdateReporter
{
    public Task ReportAsync(UpdateReport report, CancellationToken token = default)
    {
        Console.WriteLine($"Report: record={report.RecordId}, status={report.Status}, type={report.Type}");
        return Task.CompletedTask;
    }
}

await new GeneralUpdateBootstrap()
    .SetConfig(request)
    .UpdateReporter<ConsoleUpdateReporter>()
    .LaunchAsync();
```

The built-in `HttpUpdateReporter` posts JSON to `ReportUrl`:

```json
{
  "recordId": 123,
  "status": 1,
  "type": 1
}
```

| Enum | Value | Description |
| --- | --- | --- |
| `UpdateStatus.Updating` | `1` | Updating. |
| `UpdateStatus.Success` | `2` | Update succeeded. |
| `UpdateStatus.Failure` | `3` | Update failed. |

## HTTP authentication: IHttpAuthProvider

`IHttpAuthProvider` adds authentication to outgoing Core HTTP requests.

```csharp
using GeneralUpdate.Core.Security;

public sealed class StaticBearerAuthProvider : IHttpAuthProvider
{
    public Task ApplyAuthAsync(HttpRequestMessage request, CancellationToken token = default)
    {
        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "access-token");

        return Task.CompletedTask;
    }
}

await new GeneralUpdateBootstrap()
    .SetConfig(request)
    .UpdateAuth<StaticBearerAuthProvider>()
    .LaunchAsync();
```

Core includes `NoOpAuthProvider`, `BearerTokenAuthProvider`, `ApiKeyAuthProvider`, and `HmacAuthProvider`. Some require constructor parameters, so create a parameterless wrapper when registering through `UpdateAuth<T>()`.

## HTTPS certificate policy: ISslValidationPolicy

`ISslValidationPolicy` controls HTTPS certificate validation. The default `StrictSslValidationPolicy` accepts only certificates without SSL policy errors.

```csharp
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using GeneralUpdate.Core.Security;

public sealed class DevelopmentSslPolicy : ISslValidationPolicy
{
    public bool ValidateCertificate(
        X509Certificate2? certificate,
        X509Chain? chain,
        SslPolicyErrors sslPolicyErrors)
    {
        return sslPolicyErrors == SslPolicyErrors.None
            || certificate?.Issuer.Contains("CN=Local Dev Root") == true;
    }
}

await new GeneralUpdateBootstrap()
    .SetConfig(request)
    .SslPolicy<DevelopmentSslPolicy>()
    .LaunchAsync();
```

Do not unconditionally return `true` in production.

## Download source: IDownloadSource

`IDownloadSource` returns assets to download. Use it for private services, file servers, config centers, or custom cloud storage.

```csharp
using GeneralUpdate.Core.Download.Abstractions;
using GeneralUpdate.Core.Download.Models;

public sealed class StaticDownloadSource : IDownloadSource
{
    public Task<DownloadSourceResult> ListAsync(CancellationToken token = default)
    {
        var assets = new[]
        {
            new DownloadAsset(
                Name: "app-1.0.1.zip",
                Url: "https://cdn.example.com/releases/app-1.0.1.zip",
                Size: 25_000_000,
                SHA256: "expected-sha256",
                Version: "1.0.1")
        };

        return Task.FromResult(new DownloadSourceResult
        {
            Assets = assets,
            HasMainUpdate = true,
            HasUpgradeUpdate = false
        });
    }
}

await new GeneralUpdateBootstrap()
    .SetConfig(request)
    .DownloadSource<StaticDownloadSource>()
    .LaunchAsync();
```

## Retry policy: IDownloadPolicy

`IDownloadPolicy` wraps download actions and can implement retry, timeout, circuit breaker, or throttling.

```csharp
using GeneralUpdate.Core.Download.Abstractions;

public sealed class TwoAttemptDownloadPolicy : IDownloadPolicy
{
    public async Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> action,
        CancellationToken token = default)
    {
        try
        {
            return await action(token);
        }
        catch when (!token.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(2), token);
            return await action(token);
        }
    }
}

await new GeneralUpdateBootstrap()
    .SetConfig(request)
    .DownloadPolicy<TwoAttemptDownloadPolicy>()
    .LaunchAsync();
```

When `DownloadOrchestrator<T>()` is also registered, the custom orchestrator owns whether and how policy is used.

## Single-file download: IDownloadExecutor

`IDownloadExecutor` downloads one `DownloadAsset` to a destination path. Use it for FTP, SFTP, private protocols, or custom HTTP clients.

```csharp
using GeneralUpdate.Core.Download.Abstractions;
using GeneralUpdate.Core.Download.Models;

public sealed class MirrorDownloadExecutor : IDownloadExecutor
{
    private readonly HttpClient _client = new();

    public async Task<DownloadResult> ExecuteAsync(
        DownloadAsset asset,
        string destPath,
        IProgress<DownloadProgress>? progress = null,
        CancellationToken token = default)
    {
        var started = DateTimeOffset.Now;
        await using var input = await _client.GetStreamAsync(asset.Url, token);
        await using var output = File.Create(destPath);
        await input.CopyToAsync(output, token);

        var fileInfo = new FileInfo(destPath);
        return new DownloadResult(asset, destPath, fileInfo.Length, DateTimeOffset.Now - started, 0, true, null);
    }
}

await new GeneralUpdateBootstrap()
    .SetConfig(request)
    .DownloadExecutor<MirrorDownloadExecutor>()
    .LaunchAsync();
```

## Post-download pipeline: IDownloadPipeline

`IDownloadPipeline` runs after a file is downloaded. Use it for hash verification, decryption, antivirus scanning, or format conversion.

```csharp
using GeneralUpdate.Core.Download.Abstractions;

public sealed class AntivirusPipeline : IDownloadPipeline
{
    public Task<string> ProcessAsync(string downloadedPath, CancellationToken token = default)
    {
        if (!File.Exists(downloadedPath))
            throw new FileNotFoundException("Downloaded file not found.", downloadedPath);

        Console.WriteLine($"Scanning {downloadedPath}");
        return Task.FromResult(downloadedPath);
    }
}

await new GeneralUpdateBootstrap()
    .SetConfig(request)
    .DownloadPipeline<AntivirusPipeline>()
    .LaunchAsync();
```

Core first tries to construct a pipeline with a `string` constructor for the expected hash. If not available, it uses the parameterless constructor.

## Batch download orchestration: IDownloadOrchestrator

`IDownloadOrchestrator` is the highest-level download extension point. It owns batch download, concurrency, retry, progress, and result aggregation.

```csharp
using GeneralUpdate.Core.Download.Abstractions;
using GeneralUpdate.Core.Download.Executors;
using GeneralUpdate.Core.Download.Models;

public sealed class SerialDownloadOrchestrator : IDownloadOrchestrator
{
    private readonly IDownloadExecutor _executor = new HttpDownloadExecutor(new HttpClient());

    public async Task<DownloadReport> ExecuteAsync(
        DownloadPlan plan,
        string destDir,
        int maxConcurrency = 3,
        IProgress<DownloadProgress>? progress = null,
        CancellationToken token = default)
    {
        var results = new List<DownloadResult>();
        var started = DateTimeOffset.Now;

        foreach (var asset in plan.Assets)
        {
            var destPath = Path.Combine(destDir, asset.Name);
            results.Add(await _executor.ExecuteAsync(asset, destPath, progress, token));
        }

        return new DownloadReport(
            results,
            results.Where(r => r.Success).Sum(r => r.DownloadedBytes),
            DateTimeOffset.Now - started,
            results.Count(r => r.Success),
            results.Count(r => !r.Success));
    }
}

await new GeneralUpdateBootstrap()
    .SetConfig(request)
    .DownloadOrchestrator<SerialDownloadOrchestrator>()
    .LaunchAsync();
```

Implement an orchestrator only when you need to replace the entire download behavior. Most cases only need `IDownloadExecutor`, `IDownloadPolicy`, or `IDownloadPipeline`.

## Platform strategy: IStrategy

`IStrategy` is the highest-level update strategy interface. Core includes `ClientStrategy`, `UpdateStrategy`, `OssStrategy`, and Windows/Linux/macOS platform strategies. Implement it only when you need to replace platform-level file operations or app startup logic.

```csharp
using GeneralUpdate.Core.Configuration;
using GeneralUpdate.Core.Download.Reporting;
using GeneralUpdate.Core.Hooks;
using GeneralUpdate.Core.Strategy;

public sealed class LoggingStrategy : IStrategy
{
    private UpdateContext? _context;

    public IUpdateHooks Hooks { get; set; } = new NoOpUpdateHooks();
    public IUpdateReporter Reporter { get; set; } = new HttpUpdateReporter();

    public void Create(UpdateContext parameter)
    {
        _context = parameter;
    }

    public async Task ExecuteAsync()
    {
        if (_context == null)
            throw new InvalidOperationException("Strategy was not initialized.");

        Console.WriteLine($"Custom strategy executing in {_context.InstallPath}");
        await Hooks.OnBeforeUpdateAsync(new HookContext(
            _context.UpdateAppName,
            _context.InstallPath,
            _context.ClientVersion,
            _context.LastVersion,
            _context.AppType ?? AppType.Client));
    }

    public Task StartAppAsync()
    {
        Console.WriteLine("Custom start app logic.");
        return Task.CompletedTask;
    }
}

await new GeneralUpdateBootstrap()
    .SetConfig(request)
    .Strategy<LoggingStrategy>()
    .LaunchAsync();
```

## Silent update options

Silent update is enabled through options rather than a separate interface. When enabled, the `Client` role starts background polling and returns immediately.

```csharp
await new GeneralUpdateBootstrap()
    .SetConfig(request)
    .SetOption(Option.AppType, AppType.Client)
    .SetOption(Option.Silent, true)
    .SetOption(Option.SilentPollIntervalMinutes, 30)
    .SetOption(Option.LaunchClientAfterUpdate, true)
    .LaunchAsync();
```

The full product decision around prompting users, exit-time upgrade, and rollout policy should be covered in cookbooks.

## Relationship with GeneralUpdate.Tools

Core consumes manifests and packages. `GeneralUpdate.Tools` helps generate and validate those artifacts.

| Tools capability | Core consumption point |
| --- | --- |
| Patch Package | `Option.PatchEnabled`, `UseDiffPipeline`, differential patch processing. |
| Manifest Generator | `ManifestInfo`, `AppMetadataDiscoverer`, version write-back. |
| Extension Package | Distributed as package content and consumed by download/deploy flow. |
| OSS Config | `OssClient` / `OssUpgrade` roles. |
| Hash / Simulation / Report | `Option.VerifyChecksum`, post-download verification, and status reporting. |

## Related samples

- [Upgrade sample](https://github.com/GeneralLibrary/GeneralUpdate-Samples/blob/main/src/Upgrade/Program.cs)
- [OSS upgrade sample](https://github.com/GeneralLibrary/GeneralUpdate-Samples/tree/main/src/OSS/OSSUpgradeSample)
- [GeneralUpdate repository](https://github.com/GeneralLibrary/GeneralUpdate)
- [GeneralUpdate.Tools repository](https://github.com/GeneralLibrary/GeneralUpdate.Tools)
