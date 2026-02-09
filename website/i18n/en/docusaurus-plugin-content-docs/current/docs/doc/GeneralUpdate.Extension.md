---
sidebar_position: 12
---

### Definition

Namespace: GeneralUpdate.Extension

Assembly: GeneralUpdate.Extension.dll

**NuGet Package**: GeneralUpdate.Extension



GeneralUpdate.Extension is a VS Code-inspired extension management system for .NET applications. It provides complete plugin/extension management capabilities, including extension download, installation, updates, version compatibility checking, platform support, dependency resolution, and rollback mechanisms.

```c#
public class GeneralExtensionHost : IExtensionHost
{
    public IExtensionCatalog ExtensionCatalog { get; }
    public event EventHandler<ExtensionUpdateEventArgs>? ExtensionUpdateStatusChanged;
}
```



### Quick Start

The most basic usage example: initialize the extension host and query remote extensions [[View Full Example]](https://github.com/GeneralLibrary/GeneralUpdate/blob/master/src/c%23/GeneralUpdate.Extension/Examples/ExtensionExample.cs).

```c#
using GeneralUpdate.Extension;
using GeneralUpdate.Extension.Core;
using GeneralUpdate.Extension.Common.Models;
using GeneralUpdate.Extension.Common.DTOs;

// 1. Initialize extension host
var options = new ExtensionHostOptions
{
    ServerUrl = "https://extensions.example.com/api",
    BearerToken = "your-bearer-token",          // Optional: for authentication
    HostVersion = "1.0.0",                      // Your application version
    ExtensionsDirectory = "./extensions"         // Extension installation directory
};

var host = new GeneralExtensionHost(options);

// 2. Subscribe to update events
host.ExtensionUpdateStatusChanged += (sender, e) =>
{
    Console.WriteLine($"Extension: {e.ExtensionName}");
    Console.WriteLine($"Status: {e.Status}");
    Console.WriteLine($"Progress: {e.Progress}%");
};

// 3. Query available extensions
var query = new ExtensionQueryDTO
{
    Platform = TargetPlatform.Windows,
    HostVersion = "1.0.0",
    PageNumber = 1,
    PageSize = 20
};

var result = await host.QueryExtensionsAsync(query);
if (result.Success && result.Data != null)
{
    foreach (var ext in result.Data.Items)
    {
        Console.WriteLine($"{ext.DisplayName} v{ext.Version}");
        Console.WriteLine($"Compatible: {ext.IsCompatible}");
    }
}

// 4. Update extension
bool success = await host.UpdateExtensionAsync("extension-guid");
```



### Core Features

GeneralUpdate.Extension provides the following core features.

#### Extension Host (GeneralExtensionHost)

The main extension management container that provides the entry point for all extension operations.

| Method                      | Description                                                  |
| --------------------------- | ------------------------------------------------------------ |
| QueryExtensionsAsync()      | Query available extensions from remote server. Supports filtering by name, platform, version, status, etc. |
| UpdateExtensionAsync()      | Update specified extension. Automatically handles download, compatibility check, platform validation, and installation. |
| DownloadExtensionAsync()    | Download extension package to specified path. Supports resumable download (HTTP Range requests). |
| InstallExtensionAsync()     | Install extension package. Supports automatic rollback functionality, restoring to previous state on installation failure. |
| IsExtensionCompatible()     | Check if extension is compatible with current host version. |
| SetAutoUpdate()             | Enable/disable auto-update for specified extension.         |
| SetGlobalAutoUpdate()       | Enable/disable global auto-update for all extensions.       |
| IsAutoUpdateEnabled()       | Check if auto-update is enabled for extension.              |

#### Extension Catalog (ExtensionCatalog)

Manages the catalog of locally installed extensions with persistent storage using JSON files.

| Method                              | Description                                                  |
| ----------------------------------- | ------------------------------------------------------------ |
| LoadInstalledExtensions()           | Load list of installed extensions from catalog.json.        |
| GetInstalledExtensions()            | Get list of all installed extensions.                       |
| GetInstalledExtensionById()         | Get specific extension by extension ID (GUID).              |
| GetInstalledExtensionsByPlatform()  | Get list of extensions that support specified platform.     |
| AddOrUpdateExtension()              | Add new extension or update existing extension metadata.    |
| RemoveExtension()                   | Remove extension record from catalog.                       |
| SaveCatalog()                       | Save catalog to JSON file.                                  |

#### Download Queue (DownloadQueueManager)

Manages extension download queue with support for concurrent downloads and status tracking.

| Method                  | Description                                                  |
| ----------------------- | ------------------------------------------------------------ |
| EnqueueDownload()       | Add download task to queue. Task will be automatically queued and processed. |
| GetDownloadStatus()     | Get current status of specified download task.              |
| CancelDownload()        | Cancel ongoing download task.                               |

Download queue features:
- Default concurrent download limit: 3
- Automatic status tracking (Queued, Downloading, Completed, Failed)
- Supports resumable downloads
- Event notification for download status changes



### Extension Metadata

Each extension must include the following metadata information:

| Property           | Type            | Description                                          |
| ------------------ | --------------- | ---------------------------------------------------- |
| Id                 | string          | Extension unique identifier (GUID format)           |
| Name               | string          | Extension name (unique, lowercase, no spaces)       |
| DisplayName        | string          | Human-readable display name                         |
| Version            | string          | Semantic version number (e.g., "1.2.3")             |
| Publisher          | string          | Publisher identifier                                |
| Description        | string          | Extension description                               |
| SupportedPlatforms | TargetPlatform  | Supported platform flags (Windows/Linux/MacOS/All)  |
| MinHostVersion     | string          | Minimum compatible host version                     |
| MaxHostVersion     | string          | Maximum compatible host version                     |
| Dependencies       | string          | List of dependent extension IDs (comma-separated)   |
| Format             | string          | File format (.dll, .zip, etc.)                      |
| Categories         | string          | Extension categories (comma-separated)              |
| IsPreRelease       | bool            | Whether this is a pre-release version               |
| License            | string          | License identifier (e.g., "MIT", "Apache-2.0")      |
| FileSize           | long            | File size (bytes)                                   |
| Hash               | string          | File hash value (SHA256)                            |
| DownloadUrl        | string          | Download URL                                        |



### Initialization and Configuration

#### Basic Initialization

```c#
var options = new ExtensionHostOptions
{
    ServerUrl = "https://extensions.example.com/api",
    HostVersion = "1.0.0",
    ExtensionsDirectory = "./extensions"
};

var host = new GeneralExtensionHost(options);
```

#### Initialization with Authentication

```c#
var options = new ExtensionHostOptions
{
    ServerUrl = "https://extensions.example.com/api",
    BearerToken = "your-bearer-token",
    HostVersion = "1.0.0",
    ExtensionsDirectory = "./extensions",
    CatalogPath = "./extensions/catalog.json"  // Optional: custom catalog file path
};

var host = new GeneralExtensionHost(options);
```

#### Subscribing to Events

```c#
host.ExtensionUpdateStatusChanged += (sender, e) =>
{
    switch (e.Status)
    {
        case ExtensionUpdateStatus.Queued:
            Console.WriteLine($"{e.ExtensionName} added to queue");
            break;
        case ExtensionUpdateStatus.Updating:
            Console.WriteLine($"{e.ExtensionName} updating... {e.Progress}%");
            break;
        case ExtensionUpdateStatus.UpdateSuccessful:
            Console.WriteLine($"{e.ExtensionName} update successful!");
            break;
        case ExtensionUpdateStatus.UpdateFailed:
            Console.WriteLine($"{e.ExtensionName} update failed: {e.ErrorMessage}");
            break;
    }
};
```



### Querying Remote Extensions

#### Basic Query

```c#
var query = new ExtensionQueryDTO
{
    PageNumber = 1,
    PageSize = 20
};

var result = await host.QueryExtensionsAsync(query);
if (result.Success && result.Data != null)
{
    Console.WriteLine($"Found {result.Data.TotalCount} extensions");
    foreach (var ext in result.Data.Items)
    {
        Console.WriteLine($"• {ext.DisplayName} v{ext.Version}");
        Console.WriteLine($"  Publisher: {ext.Publisher}");
        Console.WriteLine($"  Compatible: {ext.IsCompatible}");
    }
}
```

#### Advanced Query (with Filters)

```c#
var query = new ExtensionQueryDTO
{
    Name = "my-extension",                    // Search by name
    Platform = TargetPlatform.Windows,        // Only query Windows extensions
    HostVersion = "1.0.0",                    // Check compatibility with this version
    Status = true,                            // Only query enabled extensions
    BeginDate = DateTime.Now.AddMonths(-1),   // Extensions from last month
    EndDate = DateTime.Now,
    PageNumber = 1,
    PageSize = 50
};

var result = await host.QueryExtensionsAsync(query);
```



### Installing and Updating Extensions

#### Auto-Update Extension

The simplest way, completing all operations with one call:

```c#
// UpdateExtensionAsync automatically performs the following steps:
// 1. Query extension information from server
// 2. Check version compatibility
// 3. Check platform support
// 4. Download extension package
// 5. Install extension
// 6. Update local catalog

string extensionId = "550e8400-e29b-41d4-a716-446655440000";
bool success = await host.UpdateExtensionAsync(extensionId);

if (success)
{
    Console.WriteLine("Extension updated successfully!");
}
else
{
    Console.WriteLine("Extension update failed, check events for details");
}
```

#### Manual Download and Install

Step-by-step control of the installation process:

```c#
// Step 1: Download extension
string extensionId = "550e8400-e29b-41d4-a716-446655440000";
string savePath = "./downloads/my-extension.zip";

bool downloaded = await host.DownloadExtensionAsync(extensionId, savePath);

if (downloaded)
{
    // Step 2: Install extension (with rollback functionality)
    bool installed = await host.InstallExtensionAsync(
        extensionPath: savePath,
        rollbackOnFailure: true  // Automatic rollback on failure
    );
    
    if (installed)
    {
        Console.WriteLine("Extension installed successfully!");
    }
    else
    {
        Console.WriteLine("Installation failed, automatically rolled back");
    }
}
```



### Managing Installed Extensions

#### List All Extensions

```c#
var extensions = host.ExtensionCatalog.GetInstalledExtensions();

Console.WriteLine($"{extensions.Count} extensions installed:");
foreach (var ext in extensions)
{
    Console.WriteLine($"• {ext.DisplayName} v{ext.Version}");
    Console.WriteLine($"  ID: {ext.Id}");
    Console.WriteLine($"  Status: {(ext.Status == true ? "Enabled" : "Disabled")}");
    Console.WriteLine($"  Platform: {ext.SupportedPlatforms}");
}
```

#### Get Specific Extension

```c#
string extensionId = "550e8400-e29b-41d4-a716-446655440000";
var extension = host.ExtensionCatalog.GetInstalledExtensionById(extensionId);

if (extension != null)
{
    Console.WriteLine($"Extension Name: {extension.DisplayName}");
    Console.WriteLine($"Version: {extension.Version}");
    Console.WriteLine($"Publisher: {extension.Publisher}");
    Console.WriteLine($"Description: {extension.Description}");
}
else
{
    Console.WriteLine("Extension not found");
}
```

#### Filter Extensions by Platform

```c#
var windowsExtensions = host.ExtensionCatalog
    .GetInstalledExtensionsByPlatform(TargetPlatform.Windows);

Console.WriteLine($"Windows extensions: {windowsExtensions.Count}");

var linuxExtensions = host.ExtensionCatalog
    .GetInstalledExtensionsByPlatform(TargetPlatform.Linux);

Console.WriteLine($"Linux extensions: {linuxExtensions.Count}");
```



### Version Compatibility

GeneralUpdate.Extension automatically checks version compatibility to ensure extensions match the host application version.

#### Compatibility Rules

An extension is considered compatible if it meets the following conditions:
1. Extension's MinHostVersion ≤ Host version
2. Extension's MaxHostVersion ≥ Host version
3. Both conditions must be satisfied simultaneously

#### Example

Assuming host version is 1.5.0:

| Extension MinHostVersion | Extension MaxHostVersion | Result          |
| ------------------------ | ------------------------ | --------------- |
| 1.0.0                    | 2.0.0                    | ✓ Compatible    |
| 1.6.0                    | 2.0.0                    | ✗ Incompatible  |
| 1.0.0                    | 1.4.0                    | ✗ Incompatible  |
| 1.5.0                    | 1.5.0                    | ✓ Compatible    |

#### Checking Compatibility

```c#
var extension = host.ExtensionCatalog.GetInstalledExtensionById(extensionId);
if (extension != null)
{
    bool isCompatible = host.IsExtensionCompatible(extension);
    
    if (isCompatible)
    {
        Console.WriteLine("✓ Extension is compatible with current version");
    }
    else
    {
        Console.WriteLine("✗ Extension is not compatible with current version");
        Console.WriteLine($"  Required host version: {extension.MinHostVersion} - {extension.MaxHostVersion}");
        Console.WriteLine($"  Current host version: {options.HostVersion}");
    }
}
```



### Platform Support

GeneralUpdate.Extension supports multi-platform extensions, allowing different extension versions for different operating systems.

#### Platform Flags

```c#
// Single platform
TargetPlatform.Windows    // Windows only
TargetPlatform.Linux      // Linux only
TargetPlatform.MacOS      // macOS only

// Multiple platforms (using bitwise flag combination)
TargetPlatform.Windows | TargetPlatform.Linux  // Windows and Linux
TargetPlatform.All                              // All platforms
```

#### Automatic Platform Detection

The system automatically detects the current running platform and filters compatible extensions:

```c#
// Automatically filter by platform when querying
var query = new ExtensionQueryDTO
{
    Platform = TargetPlatform.Windows  // Only return extensions that support Windows
};

var result = await host.QueryExtensionsAsync(query);
```

#### Platform-Specific Extensions

```c#
// Example: Set extension metadata for different platforms
var extension = new ExtensionMetadata
{
    Name = "my-extension",
    DisplayName = "My Extension",
    Version = "1.0.0",
    // Only supports Windows and Linux
    SupportedPlatforms = TargetPlatform.Windows | TargetPlatform.Linux
};
```



### Dependency Resolution

GeneralUpdate.Extension automatically handles dependencies between extensions.

#### Dependency Features

1. **Automatic transitive dependency resolution**: If A depends on B, and B depends on C, the system automatically identifies and installs C.
2. **Circular dependency detection**: Automatically detects and prevents circular dependencies.
3. **Correct installation order**: Installs in dependency order (dependencies installed first).
4. **Missing dependency check**: Checks if all dependencies are available before installation.

#### Defining Dependencies

Use comma-separated GUID list in extension metadata:

```c#
var extension = new ExtensionMetadata
{
    Id = "550e8400-e29b-41d4-a716-446655440001",
    Name = "my-extension",
    DisplayName = "My Extension",
    Version = "1.0.0",
    // Depends on two other extensions
    Dependencies = "550e8400-e29b-41d4-a716-446655440002,550e8400-e29b-41d4-a716-446655440003"
};
```

#### Automatic Dependency Handling

```c#
// UpdateExtensionAsync automatically handles dependencies:
// 1. Identify all dependencies
// 2. Check if already installed
// 3. Download and install missing dependencies
// 4. Install in correct order

bool success = await host.UpdateExtensionAsync(extensionId);
// System will automatically install all required dependent extensions
```



### Rollback Mechanism

InstallExtensionAsync supports automatic rollback functionality, restoring to the previous state when installation fails.

#### How It Works

1. **Create backup**: Before installation, the system backs up the existing extension (if it exists).
2. **Attempt installation**: Execute extension installation operation.
3. **On success**: Delete backup files.
4. **On failure**: Automatically restore from backup, undoing all changes.

#### Using Rollback Functionality

```c#
bool success = await host.InstallExtensionAsync(
    extensionPath: "./downloads/extension.zip",
    rollbackOnFailure: true  // Enable automatic rollback
);

if (!success)
{
    Console.WriteLine("Installation failed, but automatically rolled back to previous version");
    Console.WriteLine("Your extension directory remains unchanged");
}
```

#### Best Practices

- **Always enable rollback** in production (`rollbackOnFailure: true`)
- Ensure sufficient disk space for backups
- Monitor backup directory (default: `<ExtensionsDirectory>/.backup`)



### Auto-Update Settings

GeneralUpdate.Extension supports auto-update functionality for extensions.

#### Enable Auto-Update for Individual Extension

```c#
string extensionId = "550e8400-e29b-41d4-a716-446655440000";

// Enable auto-update
host.SetAutoUpdate(extensionId, true);

// Disable auto-update
host.SetAutoUpdate(extensionId, false);

// Check status
bool autoUpdateEnabled = host.IsAutoUpdateEnabled(extensionId);
Console.WriteLine($"Auto-update: {(autoUpdateEnabled ? "Enabled" : "Disabled")}");
```

#### Global Auto-Update

```c#
// Enable auto-update for all extensions
host.SetGlobalAutoUpdate(true);

// Disable global auto-update
host.SetGlobalAutoUpdate(false);
```



### Download Queue Management

The download queue manager handles concurrent downloads, providing status tracking and cancellation capabilities.

#### Queue Features

- Default concurrent download limit: 3 simultaneous downloads
- Automatic status tracking
- Support for canceling ongoing downloads
- Supports resumable downloads (HTTP Range requests)

#### Download Status

Download tasks can be in one of the following states:

```c#
public enum ExtensionUpdateStatus
{
    Queued,            // Added to queue, waiting for download
    Updating,          // Downloading/updating
    UpdateSuccessful,  // Download/update successful
    UpdateFailed       // Download/update failed
}
```

#### Monitoring Downloads

```c#
host.ExtensionUpdateStatusChanged += (sender, e) =>
{
    Console.WriteLine($"Extension: {e.ExtensionName ?? e.ExtensionId}");
    Console.WriteLine($"Status: {e.Status}");
    
    if (e.Status == ExtensionUpdateStatus.Updating)
    {
        Console.WriteLine($"Progress: {e.Progress}%");
    }
    
    if (e.Status == ExtensionUpdateStatus.UpdateFailed)
    {
        Console.WriteLine($"Error: {e.ErrorMessage}");
    }
};
```



### Server API Requirements

GeneralUpdate.Extension requires the server to provide the following API endpoints:

#### Query Extensions

```
POST {ServerUrl}/extensions
Content-Type: application/json
Authorization: Bearer {BearerToken}

Request Body: ExtensionQueryDTO
Response: HttpResponseDTO<PagedResultDTO<ExtensionDTO>>
```

#### Download Extension

```
GET {ServerUrl}/extensions/{id}
Authorization: Bearer {BearerToken}

Response: File stream (supports HTTP Range requests for resumable downloads)
```



### Complete Usage Example

Here is a complete example demonstrating the main operations of extension management:

```c#
using GeneralUpdate.Extension;
using GeneralUpdate.Extension.Core;
using GeneralUpdate.Extension.Common.Models;
using GeneralUpdate.Extension.Common.DTOs;
using GeneralUpdate.Extension.Common.Enums;
using System;
using System.Threading.Tasks;

public class ExtensionManagerExample
{
    public static async Task Main()
    {
        // 1. Initialize extension host
        var options = new ExtensionHostOptions
        {
            ServerUrl = "https://extensions.example.com/api",
            BearerToken = "your-bearer-token",
            HostVersion = "1.0.0",
            ExtensionsDirectory = "./extensions"
        };

        var host = new GeneralExtensionHost(options);

        // 2. Subscribe to update events
        host.ExtensionUpdateStatusChanged += (sender, e) =>
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {e.ExtensionName}");
            Console.WriteLine($"  Status: {e.Status}");
            
            if (e.Status == ExtensionUpdateStatus.Updating)
            {
                Console.WriteLine($"  Progress: {e.Progress}%");
            }
            
            if (e.Status == ExtensionUpdateStatus.UpdateFailed)
            {
                Console.WriteLine($"  Error: {e.ErrorMessage}");
            }
        };

        // 3. Query remote extensions
        Console.WriteLine("=== Querying Remote Extensions ===\n");
        var query = new ExtensionQueryDTO
        {
            Platform = TargetPlatform.Windows,
            HostVersion = options.HostVersion,
            Status = true,
            PageNumber = 1,
            PageSize = 10
        };

        var queryResult = await host.QueryExtensionsAsync(query);
        if (queryResult.Success && queryResult.Data != null)
        {
            Console.WriteLine($"Found {queryResult.Data.TotalCount} extensions:\n");
            
            foreach (var ext in queryResult.Data.Items)
            {
                Console.WriteLine($"• {ext.DisplayName} v{ext.Version}");
                Console.WriteLine($"  ID: {ext.Id}");
                Console.WriteLine($"  Publisher: {ext.Publisher}");
                Console.WriteLine($"  Compatible: {ext.IsCompatible}");
                Console.WriteLine($"  Platform: {ext.SupportedPlatforms}");
                Console.WriteLine();
            }
        }

        // 4. Update specific extension
        Console.WriteLine("\n=== Updating Extension ===\n");
        string extensionId = "550e8400-e29b-41d4-a716-446655440000";
        
        bool updateSuccess = await host.UpdateExtensionAsync(extensionId);
        if (updateSuccess)
        {
            Console.WriteLine("✓ Extension updated successfully!");
        }
        else
        {
            Console.WriteLine("✗ Extension update failed");
        }

        // 5. List installed extensions
        Console.WriteLine("\n=== Installed Extensions ===\n");
        var installed = host.ExtensionCatalog.GetInstalledExtensions();
        Console.WriteLine($"Total {installed.Count} installed extensions:\n");
        
        foreach (var ext in installed)
        {
            Console.WriteLine($"• {ext.DisplayName} v{ext.Version}");
            Console.WriteLine($"  Status: {(ext.Status == true ? "Enabled" : "Disabled")}");
            Console.WriteLine($"  Platform: {ext.SupportedPlatforms}");
            
            // Check compatibility
            bool compatible = host.IsExtensionCompatible(ext);
            Console.WriteLine($"  Compatible: {(compatible ? "Yes" : "No")}");
            Console.WriteLine();
        }

        // 6. Configure auto-update
        Console.WriteLine("\n=== Configuring Auto-Update ===\n");
        host.SetGlobalAutoUpdate(true);
        Console.WriteLine("✓ Global auto-update enabled");
        
        // Disable auto-update for specific extension
        host.SetAutoUpdate(extensionId, false);
        Console.WriteLine($"✓ Auto-update disabled for extension {extensionId}");
    }
}
```



### Best Practices

1. **Always subscribe to events**: Subscribe to `ExtensionUpdateStatusChanged` event before performing any operations to monitor progress and errors.

2. **Use rollback functionality**: Always enable `rollbackOnFailure: true` when installing extensions in production.

3. **Check compatibility**: Use `IsExtensionCompatible()` to check extension compatibility before installation.

4. **Reasonable pagination**: Use appropriate `PageSize` (recommended 10-50) when querying extensions to avoid loading too much data at once.

5. **Secure token storage**: Bearer Token should be stored securely, not hardcoded in code.

6. **Monitor disk space**: Ensure sufficient disk space for downloads and backups.

7. **Verify extension metadata**: Validate extension metadata integrity and hash values before installation.

8. **Handle network errors**: Implement retry logic to handle temporary network failures.

9. **Logging**: Log all extension operations and errors for troubleshooting.

10. **Regular cleanup**: Regularly clean up download cache and backup directories to free disk space.



### Troubleshooting

#### Issue: Extension Download Failed

**Possible Causes**:
- Network connection issues
- Server unavailable
- Bearer Token invalid or expired

**Solutions**:
```c#
// Check error information in event arguments
host.ExtensionUpdateStatusChanged += (sender, e) =>
{
    if (e.Status == ExtensionUpdateStatus.UpdateFailed)
    {
        Console.WriteLine($"Download failed: {e.ErrorMessage}");
        // Log detailed information for debugging
    }
};

// Implement retry logic
int maxRetries = 3;
for (int i = 0; i < maxRetries; i++)
{
    bool success = await host.UpdateExtensionAsync(extensionId);
    if (success) break;
    
    await Task.Delay(TimeSpan.FromSeconds(5)); // Wait before retrying
}
```

#### Issue: Extension Shows as Incompatible

**Solutions**:
```c#
var extension = host.ExtensionCatalog.GetInstalledExtensionById(extensionId);
if (extension != null && !host.IsExtensionCompatible(extension))
{
    Console.WriteLine("Extension version requirements:");
    Console.WriteLine($"  Minimum host version: {extension.MinHostVersion}");
    Console.WriteLine($"  Maximum host version: {extension.MaxHostVersion}");
    Console.WriteLine($"  Current host version: {options.HostVersion}");
    
    // Check if compatible version is available
    var query = new ExtensionQueryDTO
    {
        Name = extension.Name,
        HostVersion = options.HostVersion
    };
    var result = await host.QueryExtensionsAsync(query);
    // Check for compatible version
}
```

#### Issue: Installation Failed but Did Not Rollback

**Solutions**:
Ensure rollback option is enabled:
```c#
bool success = await host.InstallExtensionAsync(
    extensionPath: savePath,
    rollbackOnFailure: true  // Must be set to true
);
```

Check backup directory permissions:
```c#
string backupDir = Path.Combine(options.ExtensionsDirectory, ".backup");
if (!Directory.Exists(backupDir))
{
    Directory.CreateDirectory(backupDir);
}
// Ensure write permissions
```



### Applies To

| Product            | Versions                              |
| ------------------ | ------------------------------------- |
| .NET Standard      | 2.0                                   |
| .NET Framework     | 4.6.1+                                |
| .NET Core          | 2.0+                                  |
| .NET               | 5, 6, 7, 8, 9, 10                     |
| Mono               | 5.4+                                  |
| Xamarin.iOS        | 10.14+                                |
| Xamarin.Android    | 8.0+                                  |

### See Also

- [GeneralUpdate.ClientCore](./GeneralUpdate.ClientCore.md) - Main client update component
- [GeneralUpdate.Core](./GeneralUpdate.Core.md) - Core update logic
- [GitHub Source Code](https://github.com/GeneralLibrary/GeneralUpdate/tree/master/src/c%23/GeneralUpdate.Extension) - Complete source code and more examples
- [Quick Start Guide](../quickstart/Quik%20start.md) - Getting started with GeneralUpdate
