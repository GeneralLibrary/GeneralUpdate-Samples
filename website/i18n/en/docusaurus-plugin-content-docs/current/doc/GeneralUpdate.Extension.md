---
sidebar_position: 7
---

# GeneralUpdate.Extension

**Namespace:** `GeneralUpdate.Extension` | **Main Entry Point:** `GeneralExtensionHost` (implements `IExtensionHost`) | **NuGet Package:** `GeneralUpdate.Extension`

## 1. Component Overview

### 1.1 Introduction

**GeneralUpdate.Extension** is an extension management component for .NET applications, designed to give host apps VS Code-like extension ecosystem capabilities: query extensions from a remote service, download extension packages, install or update to local directories, and handle version compatibility, platform matching, dependency resolution, SHA256 verification, failure rollback, and event notifications.

It's suited for scenarios where the main app and optional capabilities are distributed separately — reports, authentication, industry plugins, customer-customized modules, script executors, etc.

**Core Capabilities:**

| Capability | Description |
| --- | --- |
| Extension Query | Paginated query of available extensions from server API with multi-condition filtering |
| One-Click Update | `UpdateExtensionAsync` chains query→compatibility→dependencies→download→hash verify→install→catalog update |
| Safe Installation | Zip Slip path traversal protection, pre-install backup, auto-rollback to old version on failure |
| Batch Updates | `UpdateExtensionsAsync` processes multiple extensions sequentially, returns per-extension success/failure |
| Version Compatibility | `MinHostVersion` ≤ `HostVersion` ≤ `MaxHostVersion` required for installation |
| Platform Matching | `[Flags] TargetPlatform` bitwise check against current OS |
| Dependency Resolution | Topological sort dependency tree with circular dependency detection, recursive install of missing deps |
| Resumable Download | HTTP Range support for resuming interrupted downloads |
| Local Catalog | Per-extension `manifest.json` with atomic write (`.tmp` → rename), persistence and loading |
| Lifecycle Hooks | Business logic injection before/after install, activate/deactivate, uninstall |
| Auto-Update Policy | `SetGlobalAutoUpdate` / `SetAutoUpdate` toggles for global or per-extension auto-update |
| DI Integration | `ExtensionHostBuilder` registers default services; all services replaceable via DI |

**Business Problems Solved:**
- Main app bloat; non-core features should be independently updatable extensions
- Different customers need different feature combinations; extension ecosystem enables on-demand installation
- Extensions have inter-dependencies; need automatic dependency management and version compatibility
- Need a unified extension management framework to reduce redundant development

**Use Cases:**
- IDE plugin marketplace
- Enterprise ERP/CRM industry modules (report templates, auth methods, data exports)
- Independently distributed customer-customized features
- Componentized publishing for script executors/tool suites

### 1.2 Environment & Dependencies

| Item | Description |
| --- | --- |
| **Version** | `10.5.0-beta.2` |
| **Target Framework** | `netstandard2.0` (.NET Framework 4.6.1+ / .NET Core 2.0+ / .NET 5+) |
| **Dependencies** | `Microsoft.Extensions.DependencyInjection`, `Microsoft.Extensions.Logging.Abstractions`, `Microsoft.Extensions.Options`, `Newtonsoft.Json`, `System.Net.Http`, `System.IO.Compression`, `System.IO.Compression.ZipFile` |
| **Compatibility** | All .NET Standard 2.0 platforms |

---

## 2. Feature List

| Feature | Description | Type | Required | Notes |
| --- | --- | --- | --- | --- |
| Extension Query | Paginated query of extensions from server API | Core | Recommended | Multi-condition filtering supported |
| Extension Download | Download extension ZIP from server with resume support | Core | Automatic | Triggered by `DownloadExtensionAsync` or one-click update |
| Extension Install | Safe ZIP extraction with Zip Slip protection and rollback | Core | Automatic | Only `.zip` format accepted |
| One-Click Update | Auto chain: query→compatibility→deps→download→verify→install | Core | Recommended | `UpdateExtensionAsync` |
| Batch Update | Sequential multi-extension update | Extended | Optional | `UpdateExtensionsAsync` |
| Extension Uninstall | Remove from catalog and delete extension directory | Core | Optional | `UninstallExtensionAsync` |
| Version Compatibility Check | Host version must fall within extension's Min/Max range | Core | Automatic | Checked automatically during update |
| Platform Matching | Auto-detect current OS, match extension's supported platforms | Core | Automatic | `PlatformMatcher` via `RuntimeInformation` |
| Recursive Dependency Install | Recursively update missing dependencies | Core | Automatic | Dependencies must be queryable from same server |
| Circular Dependency Detection | Detect cycles during topological sort | Core | Automatic | `DependencyResolver` |
| SHA256 Verification | Verify downloaded file integrity | Core | Automatic | When server `Hash` is non-empty |
| Local Catalog Management | Per-extension `manifest.json` with atomic write | Core | Automatic | Stored in extension directory |
| Auto-Update Policy | Global/per-extension auto-update toggles | Extended | Optional | In-memory only; no background polling |
| Lifecycle Hooks | Business logic before/after install/activate/deactivate/uninstall | Extended | Optional | Implement `IExtensionLifecycleHooks` or extend `DefaultExtensionLifecycleHooks` |
| DI Builder | `ExtensionHostBuilder` registers and replaces all services | Extended | Optional | Custom `IExtensionServiceFactory` supported |
| Download Queue Management | Concurrent download control (default 3) | Extended | Optional | `DownloadQueueManager` |

---

## 3. API Configuration Reference

### 3.1 Configuration Properties (Props)

**ExtensionHostOptions:**

| Field | Type | Default | Required | Values | Description |
| --- | --- | --- | --- | --- | --- |
| `ServerUrl` | `string` | — | Yes | Valid absolute URL | Extension service root; client calls `{ServerUrl}/Query` and `{ServerUrl}/Download/{extensionId}` |
| `Scheme` | `string` | `""` | Optional | `"Bearer"` etc. | Auth scheme; no auth header when empty |
| `Token` | `string` | `""` | Optional | — | Auth token; must both be non-empty with `Scheme` |
| `HostVersion` | `string` | — | Recommended | SemVer format | Host app version for compatibility |
| `ExtensionsDirectory` | `string` | — | Yes | Valid directory path | Download, install, and `.backup` location |
| `CatalogPath` | `string` | `null` | Optional | Valid directory path | Catalog scan path; defaults to `ExtensionsDirectory` |

**ExtensionMetadata (Local Model):**

| Field | Type | Default | Required | Description |
| --- | --- | --- | --- | --- |
| `Id` | `string` | — | Yes | Unique extension ID; key for dependency, query, update, uninstall |
| `Name` | `string` | `null` | Recommended | Stable name for directory and package naming |
| `DisplayName` | `string` | `null` | Optional | Display name |
| `Version` | `string` | `null` | Recommended | Extension version; suggested `1.2.3` format |
| `Format` | `string` | `null` | Recommended | Package format; install requires `.zip` |
| `Hash` | `string` | `null` | Recommended | SHA256; verified during update when non-empty |
| `Publisher` | `string` | `null` | Optional | Publisher |
| `Categories` | `string` | `null` | Optional | Comma-separated categories |
| `SupportedPlatforms` | `TargetPlatform` | `All` | Recommended | `[Flags]`: `Windows(1)`, `Linux(2)`, `MacOS(4)`, `All(7)` |
| `MinHostVersion` | `string` | `null` | Optional | Minimum host version |
| `MaxHostVersion` | `string` | `null` | Optional | Maximum host version |
| `Dependencies` | `string` | `null` | Optional | Comma-separated dependency extension IDs |
| `IsPreRelease` | `bool` | `false` | Optional | Whether pre-release |
| `CustomProperties` | `string` | `null` | Optional | Custom properties as JSON string |

**ExtensionQueryDTO (Query Filters):**

| Field | Type | Default | Required | Description |
| --- | --- | --- | --- | --- |
| `Id` | `string?` | `null` | Optional | Exact match by ID |
| `Name` | `string?` | `null` | Optional | Partial match by name |
| `Publisher` | `string?` | `null` | Optional | Partial match by publisher |
| `Category` | `string?` | `null` | Optional | Filter by category |
| `Platform` | `TargetPlatform?` | `null` | Optional | Filter by target platform |
| `HostVersion` | `string?` | `null` | Optional | For server-side compatibility check |
| `PageNumber` | `int` | `1` | Optional | Page number (1-based) |
| `PageSize` | `int` | `10` | Optional | Page size |

### 3.2 Instance Methods

**IExtensionHost:**

| Method | Parameters | Returns | Use Case | Notes |
| --- | --- | --- | --- | --- |
| `QueryExtensionsAsync(ExtensionQueryDTO)` | `query` | `Task<HttpResponseDTO<PagedResultDTO<ExtensionDTO>>>` | Search/browse extensions | Response data in `Body.Items` |
| `DownloadExtensionAsync(string, string)` | `extensionId`, `savePath` | `Task<bool>` | Download extension separately | Supports HTTP Range resume |
| `UpdateExtensionAsync(string)` | `extensionId` | `Task<bool>` | One-click update (recommended) | Chains full update pipeline |
| `InstallExtensionAsync(string, bool)` | `extensionPath`, `rollbackOnFailure` | `Task<bool>` | Manual local install | Only `.zip` accepted |
| `UpdateExtensionsAsync(IEnumerable<string>, CancellationToken)` | `extensionIds`, `ct` | `Task<Dictionary<string, bool>>` | Batch update | Sequential processing in order |
| `UninstallExtensionAsync(string, CancellationToken)` | `extensionId`, `ct` | `Task<bool>` | Uninstall extension | Removes from catalog and deletes directory |
| `ActivateExtensionAsync(string, CancellationToken)` | `extensionId`, `ct` | `Task` | Activate extension | Invokes lifecycle hooks |
| `DeactivateExtensionAsync(string, CancellationToken)` | `extensionId`, `ct` | `Task` | Deactivate extension | Invokes lifecycle hooks |
| `IsExtensionCompatible(ExtensionMetadata)` | `extension` | `bool` | Check compatibility | Based on HostVersion vs Min/MaxHostVersion |
| `SetAutoUpdate(string, bool)` | `extensionId`, `autoUpdate` | `void` | Set per-extension auto-update | In-memory only; no background polling |
| `SetGlobalAutoUpdate(bool)` | `enabled` | `void` | Set global default | In-memory only |

**ExtensionHostBuilder:**

| Method | Parameters | Returns | Use Case |
| --- | --- | --- | --- |
| `ConfigureOptions(Action<ExtensionHostOptions>)` | `configure` | `ExtensionHostBuilder` | Configure via lambda |
| `WithOptions(ExtensionHostOptions)` | `options` | `ExtensionHostBuilder` | Set options directly |
| `ConfigureServices(Action<IServiceCollection>)` | `configure` | `ExtensionHostBuilder` | Replace or add DI services |
| `Build()` | None | `IExtensionHost` | Build host instance with auto-registered defaults |

### 3.3 Callback Events

| Event | Callback Parameters | Trigger Timing | Usage Notes |
| --- | --- | --- | --- |
| `ExtensionUpdateStatusChanged` | `ExtensionUpdateEventArgs` — `ExtensionId`, `ExtensionName`, `Status`, `Progress`(0-100), `ErrorMessage` | During extension update lifecycle | Status: `Queued`→`Updating`→`UpdateSuccessful`/`UpdateFailed` |

**ExtensionUpdateStatus Enum:**

| Value | Description |
| --- | --- |
| `Queued` (0) | Queued for update |
| `Updating` (1) | Downloading / updating |
| `UpdateSuccessful` (2) | Update succeeded |
| `UpdateFailed` (3) | Update failed |

---

## 4. Advanced Examples

### 4.1 Extension Points Overview

All services are replaceable via `ExtensionHostBuilder.ConfigureServices()`:

| Service Interface | Default Implementation | Description |
| --- | --- | --- |
| `IExtensionHttpClient` | `ExtensionHttpClient` | HTTP communication |
| `IVersionCompatibilityChecker` | `VersionCompatibilityChecker` | Version compatibility check |
| `IDownloadQueueManager` | `DownloadQueueManager` | Download queue management |
| `IPlatformMatcher` | `PlatformMatcher` | Platform detection |
| `IPlatformServices` | `RuntimePlatformServices` | Runtime platform info |
| `IExtensionMetadataMapper` | `DefaultExtensionMetadataMapper` | DTO→model mapping |
| `IExtensionCatalog` | `ExtensionCatalog` | Local extension catalog |
| `IDependencyResolver` | `DependencyResolver` | Dependency resolution |
| `IExtensionLifecycleHooks` | `DefaultExtensionLifecycleHooks` | Lifecycle hooks (all virtual) |
| `IExtensionServiceFactory` | `ExtensionServiceFactory` | Service factory |

### 4.2 Examples by Scenario

#### Scenario 1: Custom Lifecycle Hooks

**Description:** Custom logic before/after install: check license before install, initialize extension database after install.

```csharp
using GeneralUpdate.Extension.Core;
using GeneralUpdate.Extension.Common.Models;

public sealed class LicensedLifecycleHooks : DefaultExtensionLifecycleHooks
{
    public override async Task<bool> OnBeforeInstallAsync(
        ExtensionMetadata extension, string? packagePath,
        CancellationToken cancellationToken = default)
    {
        if (!LicenseManager.IsLicensed(extension.Id))
            return false;  // Block installation
        return true;
    }

    public override async Task OnAfterInstallAsync(
        ExtensionMetadata extension, CancellationToken cancellationToken = default)
    {
        if (extension.CustomProperties != null)
        {
            var props = Newtonsoft.Json.JsonConvert
                .DeserializeObject<Dictionary<string, string>>(extension.CustomProperties);
            if (props?.ContainsKey("DbInitScript") == true)
                await DatabaseInitializer.RunAsync(props["DbInitScript"], cancellationToken);
        }
        Console.WriteLine($"Extension '{extension.DisplayName}' installed successfully.");
    }
}

var host = new ExtensionHostBuilder()
    .WithOptions(options)
    .ConfigureServices(services =>
    {
        services.AddSingleton<IExtensionLifecycleHooks, LicensedLifecycleHooks>();
    })
    .Build();
```

#### Scenario 2: Custom HTTP Client with Shared Connection Pool

**Description:** Share `HttpClient` connection pool with main app; switch to POST query.

```csharp
using GeneralUpdate.Extension.Communication;

var sharedClient = new HttpClient();

var httpClient = new ExtensionHttpClient(
    serverUrl: "https://extensions.mycompany.com/Extension",
    scheme: "Bearer",
    token: "jwt-token",
    httpClient: sharedClient,
    ownsHttpClient: false)
{
    UsePostForQuery = true
};

var host = new ExtensionHostBuilder()
    .WithOptions(options)
    .ConfigureServices(services =>
    {
        services.AddSingleton<IExtensionHttpClient>(httpClient);
    })
    .Build();
```

#### Scenario 3: Dependency Resolution + Conditional Batch Update

**Description:** When user selects an extension to install, auto-resolve dependencies and install them together.

```csharp
var host = new GeneralExtensionHost(options);
host.ExtensionCatalog.LoadInstalledExtensions();

var response = await host.QueryExtensionsAsync(new ExtensionQueryDTO { Id = "report-extension" });

var ext = response.Body?.Items.FirstOrDefault();
if (ext == null) return;

var resolver = new DependencyResolver(host.ExtensionCatalog);
var deps = resolver.ResolveDependencies(
    new ExtensionMetadata { Id = ext.Id, Dependencies = string.Join(",", ext.Dependencies ?? []) });
var missing = resolver.GetMissingDependencies(
    new ExtensionMetadata { Id = ext.Id, Dependencies = string.Join(",", ext.Dependencies ?? []) });

var updateOrder = new List<string>();
updateOrder.AddRange(missing);
updateOrder.Add(ext.Id);

var results = await host.UpdateExtensionsAsync(updateOrder);
foreach (var (id, success) in results)
    Console.WriteLine($"  {id}: {(success ? "OK" : "FAILED")}");
```

---

## 5. Basic Usage Examples

### 5.1 Quick Start (Minimal Demo)

```csharp
using GeneralUpdate.Extension.Core;
using GeneralUpdate.Extension.Common.DTOs;
using GeneralUpdate.Extension.Common.Models;

var options = new ExtensionHostOptions
{
    ServerUrl = "https://extensions.example.com/Extension",
    Scheme = "Bearer",
    Token = "your-token",
    HostVersion = "1.0.0",
    ExtensionsDirectory = "./extensions"
};

var host = new GeneralExtensionHost(options);

host.ExtensionUpdateStatusChanged += (sender, e) =>
    Console.WriteLine($"[{e.Status}] {e.ExtensionId}: {e.Progress}% {e.ErrorMessage}");

var response = await host.QueryExtensionsAsync(new ExtensionQueryDTO
{
    Platform = TargetPlatform.Windows,
    PageNumber = 1,
    PageSize = 20
});

if (response.Body != null)
    foreach (var ext in response.Body.Items)
        Console.WriteLine($"{ext.DisplayName} v{ext.Version}");

var success = await host.UpdateExtensionAsync("report-extension");
Console.WriteLine(success ? "Extension updated." : "Update failed.");
```

### 5.2 Basic Parameter Combination

```csharp
var host = new GeneralExtensionHost(new ExtensionHostOptions
{
    ServerUrl = "https://extensions.mycompany.com/Extension",
    Scheme = "Bearer",
    Token = Environment.GetEnvironmentVariable("EXTENSION_TOKEN") ?? "",
    HostVersion = "2.0.0",
    ExtensionsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "extensions")
});

host.ExtensionUpdateStatusChanged += (_, e) =>
{
    switch (e.Status)
    {
        case ExtensionUpdateStatus.Queued:
            Console.WriteLine($"{e.ExtensionId}: queued"); break;
        case ExtensionUpdateStatus.Updating:
            Console.WriteLine($"{e.ExtensionId}: downloading... {e.Progress}%"); break;
        case ExtensionUpdateStatus.UpdateSuccessful:
            Console.WriteLine($"{e.ExtensionName ?? e.ExtensionId}: updated"); break;
        case ExtensionUpdateStatus.UpdateFailed:
            Console.WriteLine($"{e.ExtensionId}: failed — {e.ErrorMessage}"); break;
    }
};

// Install local package
var installed = await host.InstallExtensionAsync("./downloads/report-extension_1.0.0.zip", rollbackOnFailure: true);

// Query installed extensions
host.ExtensionCatalog.LoadInstalledExtensions();
foreach (var ext in host.ExtensionCatalog.GetInstalledExtensions())
    Console.WriteLine($"{ext.DisplayName} v{ext.Version} — compatible: {host.IsExtensionCompatible(ext)}");

// Configure auto-update
host.SetGlobalAutoUpdate(true);
host.SetAutoUpdate("large-extension", false);
```

### 5.3 Production-Ready Example

Full workflow with exception handling, dependency management, and compatibility checking:

```csharp
using GeneralUpdate.Extension.Core;
using GeneralUpdate.Extension.Common.DTOs;
using GeneralUpdate.Extension.Common.Enums;
using GeneralUpdate.Extension.Common.Models;

var options = new ExtensionHostOptions
{
    ServerUrl = "https://extensions.mycompany.com/Extension",
    Scheme = "Bearer",
    Token = Configuration.GetExtensionToken(),
    HostVersion = AppInfo.CurrentVersion.ToString(),
    ExtensionsDirectory = Path.Combine(AppInfo.DataDirectory, "extensions")
};

var host = new ExtensionHostBuilder()
    .WithOptions(options)
    .ConfigureServices(services =>
        services.AddSingleton<IExtensionLifecycleHooks, AuditLifecycleHooks>())
    .Build();

host.ExtensionUpdateStatusChanged += (_, e) =>
{
    if (e.Status == ExtensionUpdateStatus.UpdateFailed)
        Log.Error($"Extension '{e.ExtensionId}' update failed: {e.ErrorMessage}");
    else if (e.Status == ExtensionUpdateStatus.UpdateSuccessful)
        Log.Info($"Extension '{e.ExtensionName ?? e.ExtensionId}' updated.");
};

host.ExtensionCatalog.LoadInstalledExtensions();
var installed = host.ExtensionCatalog.GetInstalledExtensions();
Console.WriteLine($"Loaded {installed.Count} installed extension(s).");

var response = await host.QueryExtensionsAsync(new ExtensionQueryDTO
{
    Platform = TargetPlatform.Windows | TargetPlatform.Linux,
    HostVersion = options.HostVersion,
    Status = true,
    PageNumber = 1,
    PageSize = 100
});

if (response?.Body == null) return;

var toUpdate = new List<string>();
foreach (var ext in response.Body.Items)
{
    var local = host.ExtensionCatalog.GetInstalledExtensionById(ext.Id);
    if (local == null) continue;

    var meta = new ExtensionMetadata
    {
        MinHostVersion = ext.MinHostVersion,
        MaxHostVersion = ext.MaxHostVersion
    };

    if (!host.IsExtensionCompatible(meta))
    {
        Console.WriteLine($"[INCOMPATIBLE] {ext.DisplayName}");
        continue;
    }

    if (Version.TryParse(ext.Version, out var remoteVer) &&
        Version.TryParse(local.Version, out var localVer) &&
        remoteVer > localVer && host.IsAutoUpdateEnabled(ext.Id))
    {
        Console.WriteLine($"[UPDATE] {ext.DisplayName}: {local.Version} → {ext.Version}");
        toUpdate.Add(ext.Id);
    }
}

if (toUpdate.Any())
{
    Console.WriteLine($"\nUpdating {toUpdate.Count} extension(s)...");
    var results = await host.UpdateExtensionsAsync(toUpdate);
    var succeeded = results.Count(r => r.Value);
    var failed = results.Count(r => !r.Value);
    Console.WriteLine($"Done: {succeeded} succeeded, {failed} failed.");
}
else
{
    Console.WriteLine("All extensions up to date.");
}
```

---

## 6. Global Configuration

### Server API Contract

**Query Endpoint:**

```http
GET {ServerUrl}/Query
Content-Type: application/json
Authorization: {Scheme} {Token}

Body: ExtensionQueryDTO (JSON)
```

> Note: Current implementation uses GET + JSON Body, which is non-standard HTTP. If going through proxies/gateways, may need to switch to POST or query string.

**Download Endpoint:**

```http
GET {ServerUrl}/Download/{extensionId}
Authorization: {Scheme} {Token}
Range: bytes={existingLength}-
```

### Package Structure

Recommended package name: `{Name}_{Version}.zip`

```text
report-extension_1.0.0.zip
├── manifest.json
├── extension.dll
├── extension.deps.json
├── README.md
├── CHANGELOG.md
└── LICENSE.txt
```

### Auto-Update Policy Priority

```
Per-extension setting > Global setting > Default (false)
```

### Platform Compatibility Reference

| Enum Value | Code | Description |
| --- | --- | --- |
| `TargetPlatform.None` | 0 | Matches no platform |
| `TargetPlatform.Windows` | 1 | Windows |
| `TargetPlatform.Linux` | 2 | Linux |
| `TargetPlatform.MacOS` | 4 | macOS |
| `TargetPlatform.All` | 7 | All platforms (Windows \| Linux \| MacOS) |

---

## Related Resources

- [Extension Management Sample](https://github.com/GeneralLibrary/GeneralUpdate-Samples/tree/main/src/Hub/Samples/ExtensionSample.cs)
- [GeneralUpdate Repository](https://github.com/GeneralLibrary/GeneralUpdate)
- [Packaging Guide](../guide/Packaging.md)
