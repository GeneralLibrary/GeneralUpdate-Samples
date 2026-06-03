---
sidebar_position: 12
---

# GeneralUpdate.Extension

## Overview

**GeneralUpdate.Extension** is the extension management component for .NET applications. It gives a host application a VS Code-like extension model: query extensions from a remote service, download packages, install or update them into a local directory, and handle host-version compatibility, platform matching, extension dependencies, SHA256 validation, rollback, and update events.

It is useful when optional capabilities should ship independently from the main app, such as report modules, authentication plugins, industry-specific integrations, customer customizations, or scripting extensions. The host integrates `GeneralExtensionHost`; extension packages can then be produced by Tools or CI/CD, published as ZIP files, and consumed by the Extension component.

**Namespace:** `GeneralUpdate.Extension`

**Assembly:** `GeneralUpdate.Extension.dll`
**NuGet package:** `GeneralUpdate.Extension`

```csharp
public interface IExtensionHost
{
    IExtensionCatalog ExtensionCatalog { get; }
    event EventHandler<ExtensionUpdateEventArgs>? ExtensionUpdateStatusChanged;

    Task<HttpResponseDTO<PagedResultDTO<ExtensionDTO>>> QueryExtensionsAsync(ExtensionQueryDTO query);
    Task<bool> DownloadExtensionAsync(string extensionId, string savePath);
    Task<bool> UpdateExtensionAsync(string extensionId);
    Task<bool> InstallExtensionAsync(string extensionPath, bool rollbackOnFailure = true);
    Task<Dictionary<string, bool>> UpdateExtensionsAsync(IEnumerable<string> extensionIds, CancellationToken cancellationToken = default);
    bool IsExtensionCompatible(ExtensionMetadata extension);
    void SetAutoUpdate(string extensionId, bool autoUpdate);
    void SetGlobalAutoUpdate(bool enabled);
}
```

---

## Navigation

| Topic | What it covers |
| --- | --- |
| [Quick start](#quick-start) | Minimal configuration, querying, and updating |
| [Core workflow](#core-workflow) | Query, download, install, update, rollback, and uninstall |
| [Metadata and manifest](#metadata-and-manifest) | `ExtensionMetadata`, server DTOs, and local `manifest.json` |
| [Package structure and Tools packaging](#package-structure-and-tools-packaging) | ZIP naming, package contents, producer/consumer roles |
| [Compatibility, platform, and dependencies](#compatibility-platform-and-dependencies) | Host version range, `TargetPlatform`, recursive dependency installation |
| [Events and auto-update settings](#events-and-auto-update-settings) | Status events and global/per-extension auto-update flags |
| [Server API contract](#server-api-contract) | The actual `/Query` and `/Download/{extensionId}` calls |
| [Advanced extension points](#advanced-extension-points) | DI builder, custom HttpClient, lifecycle hooks |
| [Best practices](#best-practices) | Production integration guidance |

---

## Quick start

### Install

```bash
dotnet add package GeneralUpdate.Extension
```

### Create an extension host

```csharp
using GeneralUpdate.Extension.Common.DTOs;
using GeneralUpdate.Extension.Common.Enums;
using GeneralUpdate.Extension.Common.Models;
using GeneralUpdate.Extension.Core;

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
{
    Console.WriteLine($"{e.ExtensionId} {e.Status} {e.Progress}% {e.ErrorMessage}");
};
```

`ExtensionHostOptions` currently contains:

| Property | Description |
| --- | --- |
| `ServerUrl` | Extension service base URL. The client calls `{ServerUrl}/Query` and `{ServerUrl}/Download/{extensionId}` |
| `Scheme` | Authorization scheme, for example `Bearer`. Empty values disable the header |
| `Token` | Authorization token. It is only used when both `Scheme` and `Token` are non-empty |
| `HostVersion` | Host application version used by `MinHostVersion` / `MaxHostVersion` checks |
| `ExtensionsDirectory` | Directory for downloaded packages, installed extensions, and `.backup` |
| `CatalogPath` | Optional local catalog scan path. Defaults to `ExtensionsDirectory` |

### Query and update

```csharp
var query = new ExtensionQueryDTO
{
    Platform = TargetPlatform.Windows,
    HostVersion = options.HostVersion,
    Status = true,
    PageNumber = 1,
    PageSize = 20
};

var response = await host.QueryExtensionsAsync(query);
if (response.Body != null)
{
    foreach (var extension in response.Body.Items)
    {
        Console.WriteLine($"{extension.DisplayName} v{extension.Version}, compatible: {extension.IsCompatible}");
    }
}
else
{
    Console.WriteLine(response.Message);
}

var success = await host.UpdateExtensionAsync("extension-id");
```

---

## Core workflow

### 1. Query remote extensions

`QueryExtensionsAsync` sends an `ExtensionQueryDTO` through `ExtensionHttpClient` and returns `HttpResponseDTO<PagedResultDTO<ExtensionDTO>>`. Current response data is in `Body`, not `Data`.

```csharp
var response = await host.QueryExtensionsAsync(new ExtensionQueryDTO
{
    Name = "report",
    Publisher = "general",
    Category = "Tools",
    Platform = TargetPlatform.Windows | TargetPlatform.Linux,
    HostVersion = "1.2.0",
    IsPreRelease = false,
    PageNumber = 1,
    PageSize = 10
});

if (response.Body == null)
{
    Console.WriteLine($"Query failed: {response.Code} {response.Message}");
    return;
}

Console.WriteLine($"Total: {response.Body.TotalCount}");
```

### 2. Download a package

`DownloadExtensionAsync(extensionId, savePath)` downloads the remote package to `savePath`. The lower-level downloader supports:

- HTTP Range resume when a partial file already exists;
- `Updating` progress notifications through `ExtensionUpdateStatusChanged`;
- detailed error classification in `DownloadExtensionWithResultAsync`, including network errors, 4xx, 5xx, cancellation, and I/O errors.

```csharp
var downloaded = await host.DownloadExtensionAsync(
    extensionId: "report-extension",
    savePath: "./extensions/report-extension_1.0.0.zip");
```

### 3. Install a package

`InstallExtensionAsync` accepts `.zip` packages only. The target directory is inferred from the file name: `name_version.zip` installs to `{ExtensionsDirectory}/name`.

```csharp
var installed = await host.InstallExtensionAsync(
    extensionPath: "./extensions/report-extension_1.0.0.zip",
    rollbackOnFailure: true);
```

Install steps:

1. Verify that the file exists and is a `.zip`.
2. Call `IExtensionLifecycleHooks.OnBeforeInstallAsync`; `false` cancels the install.
3. If the extension already exists and rollback is enabled, copy the old directory to `{ExtensionsDirectory}/.backup`.
4. Delete the old directory and create the target directory.
5. Safely extract the ZIP. Zip Slip path traversal entries are skipped.
6. Delete the backup and call `OnAfterInstallAsync` on success.
7. Attempt to restore from backup on failure.

### 4. Update in one call

`UpdateExtensionAsync(extensionId)` is the recommended entry point. It connects querying, compatibility checks, platform checks, recursive dependency installation, download, SHA256 validation, install, and catalog updates.

```csharp
var success = await host.UpdateExtensionAsync("report-extension");
if (!success)
{
    Console.WriteLine("Update failed. Read ExtensionUpdateStatusChanged for details.");
}
```

Full flow:

1. Raise `Queued`.
2. Query server metadata with `Id = extensionId`.
3. Map `ExtensionDTO` to `ExtensionMetadata`.
4. Check the host version against `MinHostVersion` / `MaxHostVersion`.
5. Check whether the current OS is included in `SupportedPlatforms`.
6. Recursively call `UpdateExtensionAsync(depId)` for missing dependencies.
7. Download `{Name}_{Version}{Format}` into `ExtensionsDirectory`.
8. If `Hash` is non-empty, compute and compare the downloaded file SHA256.
9. Call `InstallExtensionAsync(..., rollbackOnFailure: true)`.
10. Add or update the local catalog `manifest.json`.
11. Raise `UpdateSuccessful` on success or `UpdateFailed` on failure.

### 5. Bulk update

`UpdateExtensionsAsync` updates the given extension IDs sequentially and returns a success flag per extension. If you need parallelism, control concurrency in the application layer and call `UpdateExtensionAsync` for each extension.

```csharp
var result = await host.UpdateExtensionsAsync(new[]
{
    "report-extension",
    "auth-extension",
    "theme-extension"
}, cancellationToken);

foreach (var item in result)
{
    Console.WriteLine($"{item.Key}: {item.Value}");
}
```

### 6. Rollback

Rollback belongs to `InstallExtensionAsync`. It backs up the previous directory and restores it if replacement fails. It is useful for update/overwrite scenarios; a first install usually has no previous directory to restore.

Backup path:

```text
{ExtensionsDirectory}/.backup/{extensionName}_{yyyyMMddHHmmss}
```

### 7. Uninstall

`IExtensionHost` does not currently expose `UninstallExtensionAsync`. The available uninstall primitive is `IExtensionCatalog.RemoveInstalledExtension(extensionId)`, which removes the in-memory record and attempts to delete the extension directory.

```csharp
host.ExtensionCatalog.RemoveInstalledExtension("report-extension");
```

If you need approval, disable-before-delete, pre-uninstall checks, or cleanup hooks, wrap this in an application service and reuse the semantics of `IExtensionLifecycleHooks.OnBeforeUninstallAsync` / `OnAfterUninstallAsync`.

---

## Metadata and manifest

### ExtensionMetadata

`ExtensionMetadata` is the core local model for installed extensions, catalog persistence, and compatibility checks.

| Property | Description |
| --- | --- |
| `Id` | Unique extension ID. Dependencies, query, update, and uninstall all use this key |
| `Name` | Stable extension name, recommended for package and directory naming |
| `DisplayName` | Human-readable name |
| `Version` | Extension version. Compatibility sorting uses .NET `Version.TryParse`; prefer `1.2.3` or `1.2.3.0` |
| `FileSize` | Package size in bytes |
| `UploadTime` | Upload timestamp |
| `Status` | Enabled flag |
| `Description` | Description |
| `Format` | Package format. Current install logic requires `.zip` |
| `Hash` | Optional SHA256. Non-empty values are verified during update |
| `Publisher` | Publisher |
| `License` | License identifier |
| `Categories` | Comma-separated categories |
| `SupportedPlatforms` | `TargetPlatform` flags |
| `MinHostVersion` | Minimum host version |
| `MaxHostVersion` | Maximum host version |
| `ReleaseDate` | Release date |
| `Dependencies` | Comma-separated dependency extension IDs |
| `IsPreRelease` | Pre-release flag |
| `DownloadUrl` | Download URL metadata; the default client still downloads from `{ServerUrl}/Download/{extensionId}` |
| `CustomProperties` | Custom metadata as a JSON string |

### Server DTO and local manifest

The server query returns `ExtensionDTO`, where `Categories` and `Dependencies` are `List<string>`. The client maps those lists to comma-separated strings in local `ExtensionMetadata`.

The local catalog is not a single `catalog.json`. Current code scans child directories under `CatalogPath` and reads each extension's:

```text
manifest.json
```

`AddOrUpdateInstalledExtension` writes each extension to its own directory:

```text
{CatalogPath}/{safe-extension-name}/manifest.json
```

Writes use a `manifest.json.tmp -> manifest.json` replacement pattern. `LoadInstalledExtensions` cleans orphaned `.tmp` files and skips directories containing `.backup`.

Example manifest:

```json
{
  "Id": "report-extension",
  "Name": "report-extension",
  "DisplayName": "Report Extension",
  "Version": "1.0.0",
  "Status": true,
  "Description": "Adds PDF and Excel reports.",
  "Format": ".zip",
  "Hash": "6f5902ac237024bdd0c176cb93063dc4...",
  "Publisher": "GeneralLibrary",
  "License": "MIT",
  "Categories": "Reports,Tools",
  "SupportedPlatforms": 7,
  "MinHostVersion": "1.0.0",
  "MaxHostVersion": "2.0.0",
  "Dependencies": "base-extension",
  "IsPreRelease": false
}
```

`SupportedPlatforms` is a `[Flags]` enum. `All = Windows | Linux | MacOS = 7`.

---

## Package structure and Tools packaging

The Extension component consumes extension packages: download, validate, extract, install, rollback, and register manifests. Tools or CI/CD produces them: build the extension, generate metadata, compute SHA256, create a ZIP, and publish it to the extension service.

Recommended package name:

```text
{Name}_{Version}.zip
```

Example:

```text
report-extension_1.0.0.zip
```

Recommended ZIP contents:

```text
report-extension_1.0.0.zip
├─ manifest.json          # Recommended for business and catalog reuse
├─ extension.dll          # Extension assembly
├─ extension.deps.json    # .NET dependency description
├─ README.md
├─ CHANGELOG.md
└─ LICENSE.txt
```

`InstallExtensionAsync` does not require or parse the package `manifest.json` by itself; it focuses on safe extraction and rollback. `UpdateExtensionAsync` updates the local catalog from the server-provided `ExtensionDTO`. Therefore, the producer side must keep server metadata and ZIP contents aligned.

Recommended publishing flow:

1. Build the extension project.
2. Prepare `manifest.json` with fields matching `ExtensionMetadata`.
3. Create `{Name}_{Version}.zip`.
4. Compute the ZIP SHA256 and store it in the server `Hash`.
5. Upload the ZIP and `ExtensionDTO` metadata.
6. Let the host query with `QueryExtensionsAsync` and consume with `UpdateExtensionAsync`.

For packaging basics, see [Packaging](../guide/Packaging.md). The advanced cookbook extension release pipeline will be covered in task [#54](https://github.com/GeneralLibrary/GeneralUpdate-Samples/issues/54), where Tools packaging, manifest generation, hash calculation, upload, and staged host consumption should be connected into one workflow.

---

## Compatibility, platform, and dependencies

### Version compatibility

`VersionCompatibilityChecker.IsCompatible` compares `HostVersion` with `MinHostVersion` and `MaxHostVersion`:

- Empty `HostVersion`: no constraint, compatible;
- Unparseable `HostVersion`: incompatible;
- Non-empty unparseable `MinHostVersion`: incompatible;
- Non-empty unparseable `MaxHostVersion`: incompatible;
- Host version must satisfy `MinHostVersion <= HostVersion <= MaxHostVersion`.

| HostVersion | MinHostVersion | MaxHostVersion | Result |
| --- | --- | --- | --- |
| `1.5.0` | `1.0.0` | `2.0.0` | Compatible |
| `1.5.0` | `1.6.0` | `2.0.0` | Incompatible |
| `1.5.0` | `1.0.0` | `1.4.0` | Incompatible |
| `1.5.0` | empty | empty | Compatible |

```csharp
var extension = host.ExtensionCatalog.GetInstalledExtensionById("report-extension");
if (extension != null && host.IsExtensionCompatible(extension))
{
    Console.WriteLine("Compatible");
}
```

### Platform matching

```csharp
[Flags]
public enum TargetPlatform
{
    None = 0,
    Windows = 1,
    Linux = 2,
    MacOS = 4,
    All = Windows | Linux | MacOS
}
```

`PlatformMatcher` detects the current OS through `RuntimeInformation` and checks support with flag operations.

```csharp
var metadata = new ExtensionMetadata
{
    Id = "report-extension",
    Name = "report-extension",
    SupportedPlatforms = TargetPlatform.Windows | TargetPlatform.Linux
};
```

### Dependencies

`ExtensionMetadata.Dependencies` is a comma-separated list of extension IDs. `DependencyList` parses it into a list. `UpdateExtensionAsync` recursively installs missing dependencies before the current extension.

```csharp
var metadata = new ExtensionMetadata
{
    Id = "report-extension",
    Dependencies = "base-extension,chart-extension"
};
```

`DependencyResolver` can also resolve dependencies from the local catalog, report missing dependencies, and detect circular dependencies. The current `UpdateExtensionAsync` path installs dependencies from the current extension's server metadata, so producer-side metadata must ensure every dependency can be queried and downloaded by ID from the same extension service.

---

## Events and auto-update settings

### ExtensionUpdateStatusChanged

The update event reports status changes for one extension:

| Field | Description |
| --- | --- |
| `ExtensionId` | Extension ID |
| `ExtensionName` | Extension name; can be empty in some phases |
| `Status` | `Queued`, `Updating`, `UpdateSuccessful`, `UpdateFailed` |
| `Progress` | 0-100, updated during download |
| `ErrorMessage` | Failure reason |

```csharp
host.ExtensionUpdateStatusChanged += (sender, e) =>
{
    switch (e.Status)
    {
        case ExtensionUpdateStatus.Queued:
            Console.WriteLine($"{e.ExtensionId} queued");
            break;
        case ExtensionUpdateStatus.Updating:
            Console.WriteLine($"{e.ExtensionId} downloading {e.Progress}%");
            break;
        case ExtensionUpdateStatus.UpdateSuccessful:
            Console.WriteLine($"{e.ExtensionName ?? e.ExtensionId} updated");
            break;
        case ExtensionUpdateStatus.UpdateFailed:
            Console.WriteLine($"{e.ExtensionId} failed: {e.ErrorMessage}");
            break;
    }
};
```

### Auto-update settings

`SetGlobalAutoUpdate` sets the global default, and `SetAutoUpdate` sets a per-extension override. `IExtensionHost` exposes the setters; concrete `GeneralExtensionHost` also provides `IsAutoUpdateEnabled(extensionId)` for reading the effective value.

```csharp
var concreteHost = new GeneralExtensionHost(options);

concreteHost.SetGlobalAutoUpdate(true);
concreteHost.SetAutoUpdate("large-extension", false);

var enabled = concreteHost.IsAutoUpdateEnabled("large-extension");
```

These flags are stored in memory on the current `GeneralExtensionHost` instance. The component does not start a background polling job. Your application should decide when to scan for updates and call `UpdateExtensionAsync` according to the flags.

---

## Server API contract

`ExtensionHttpClient` currently uses two endpoints.

### Query

```http
GET {ServerUrl}/Query
Content-Type: application/json
Authorization: {Scheme} {Token}

ExtensionQueryDTO JSON body
```

This is **GET with a JSON body**. That is not common HTTP style, but the current client implements this server contract explicitly. If proxies, gateways, or API platforms reject it, client and server should be changed together to POST or query-string parameters.

Response:

```csharp
HttpResponseDTO<PagedResultDTO<ExtensionDTO>>
```

### Download

```http
GET {ServerUrl}/Download/{extensionId}
Authorization: {Scheme} {Token}
Range: bytes={existingLength}-
```

The server should return a file stream and preferably support HTTP Range for resume. The client treats `416 RequestedRangeNotSatisfiable` as an already-complete download.

---

## Advanced extension points

### ExtensionHostBuilder and DI

`ExtensionHostBuilder` registers default services and allows the application to replace any of them:

```csharp
var host = new ExtensionHostBuilder()
    .WithOptions(options)
    .ConfigureServices(services =>
    {
        services.AddSingleton<IExtensionLifecycleHooks, MyLifecycleHooks>();
        services.AddSingleton<IExtensionHttpClient>(sp =>
            new ExtensionHttpClient(options.ServerUrl, options.Scheme, options.Token, sharedHttpClient));
    })
    .Build();
```

Default registrations:

- `IExtensionHttpClient -> ExtensionHttpClient`
- `IVersionCompatibilityChecker -> VersionCompatibilityChecker`
- `IDownloadQueueManager -> DownloadQueueManager`
- `IPlatformMatcher -> PlatformMatcher`
- `IPlatformServices -> RuntimePlatformServices`
- `IExtensionMetadataMapper -> DefaultExtensionMetadataMapper`
- `IExtensionCatalog -> ExtensionCatalog`
- `IDependencyResolver -> DependencyResolver`
- `IExtensionLifecycleHooks -> DefaultExtensionLifecycleHooks`
- `IExtensionHost -> GeneralExtensionHost`

### Lifecycle hooks

`IExtensionLifecycleHooks` lets applications add logic before or after install, activation, deactivation, and uninstall. Current `GeneralExtensionHost` calls `OnBeforeInstallAsync` and `OnAfterInstallAsync`; activation, deactivation, and uninstall hooks can be reused by application-level wrappers for those flows.

```csharp
public sealed class MyLifecycleHooks : DefaultExtensionLifecycleHooks
{
    public override Task<bool> OnBeforeInstallAsync(
        ExtensionMetadata extension,
        string? packagePath,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Installing {packagePath}");
        return Task.FromResult(true);
    }
}
```

### Download queue

`DownloadQueueManager` is an independent queue type with a default max concurrency of 3. It supports `Enqueue`, `GetTask`, `CancelTask`, `GetActiveTasks`, and `DownloadStatusChanged`. The queue currently manages status and concurrency slots; the actual download in the host update path is performed by `ExtensionHttpClient`. For application-level parallel downloads, compose the queue, HTTP client, and install flow in your own service.

---

## Best practices

1. Use `.zip` extension packages in production and name them `{Name}_{Version}.zip`.
2. Store the ZIP SHA256 in server `Hash` so `UpdateExtensionAsync` can verify integrity.
3. Use parseable versions for `HostVersion`, `MinHostVersion`, and `MaxHostVersion`.
4. Use one `manifest.json` per extension directory; do not rely on the old single `catalog.json` model.
5. Set `SupportedPlatforms` to real OS support instead of defaulting everything to `All`.
6. Ensure every dependency can be queried and downloaded by ID from the same extension service.
7. For large packages, support HTTP Range on the server and show `ExtensionUpdateStatusChanged` progress in the UI.
8. Auto-update flags are policy state, not a scheduler. Scanning, timing, staged rollout, and approval belong in the application layer.
