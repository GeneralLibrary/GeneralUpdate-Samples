---
sidebar_position: 5
---

# GeneralUpdate.Core

## Component Overview

**GeneralUpdate.Core** is one of the most essential components of the GeneralUpdate framework, providing complete upgrade execution capabilities. Unlike ClientCore, the Core component runs as an independent upgrade assistant program and is responsible for performing actual file replacement, version upgrades, and system update operations after the main program closes. Through process startup and parameter passing, Core receives update instructions from ClientCore and safely completes the main program's upgrade tasks.

**Namespace:** `GeneralUpdate.Core`  
**Assembly:** `GeneralUpdate.Core.dll`

```csharp
public class GeneralUpdateBootstrap : AbstractBootstrap<GeneralUpdateBootstrap, IStrategy>
```

---

## Core Features

### 1. File Replacement and Version Management
- Safe file replacement mechanism to avoid file locking issues
- Support multi-version incremental upgrades
- Automatic handling of file dependencies

### 2. Driver Upgrade Support
- Optional driver upgrade functionality
- Field mapping table configuration
- Safe driver installation process

### 3. Comprehensive Event Notifications
- Real-time download progress monitoring
- Multi-version download management
- Complete exception and error capture

### 4. Cross-Platform Support
- Full support for Windows, Linux, macOS platforms
- Automatic platform detection and strategy adaptation

![Multi Download](imgs/muti_donwload.png)

---

## Quick Start

### Installation

Install GeneralUpdate.Core via NuGet:

```bash
dotnet add package GeneralUpdate.Core
```

### Initialization and Usage

The following example demonstrates how to configure and launch the upgrade process in the upgrade assistant program:

```csharp
using GeneralUpdate.Common.Download;
using GeneralUpdate.Common.Internal;
using GeneralUpdate.Common.Shared.Object;
using GeneralUpdate.Core;

try
{
    Console.WriteLine($"Upgrade program initialization, {DateTime.Now}!");
    Console.WriteLine("Current directory: " + Thread.GetDomain().BaseDirectory);
    
    // Launch upgrade process
    await new GeneralUpdateBootstrap()
        // Listen for download statistics
        .AddListenerMultiDownloadStatistics(OnMultiDownloadStatistics)
        // Listen for single download completion
        .AddListenerMultiDownloadCompleted(OnMultiDownloadCompleted)
        // Listen for all downloads completion
        .AddListenerMultiAllDownloadCompleted(OnMultiAllDownloadCompleted)
        // Listen for download errors
        .AddListenerMultiDownloadError(OnMultiDownloadError)
        // Listen for exceptions
        .AddListenerException(OnException)
        // Launch async upgrade
        .LaunchAsync();
        
    Console.WriteLine($"Upgrade program started, {DateTime.Now}!");
    await Task.Delay(2000);
}
catch (Exception e)
{
    Console.WriteLine(e.Message + "\n" + e.StackTrace);
}

// Event handler methods
void OnMultiDownloadStatistics(object arg1, MultiDownloadStatisticsEventArgs arg2)
{
    var version = arg2.Version as VersionInfo;
    Console.WriteLine($"Current download version: {version.Version}, Download speed: {arg2.Speed}, " +
                     $"Remaining time: {arg2.Remaining}, Progress: {arg2.ProgressPercentage}%");
}

void OnMultiDownloadCompleted(object arg1, MultiDownloadCompletedEventArgs arg2)
{
    var version = arg2.Version as VersionInfo;
    Console.WriteLine(arg2.IsComplated ? 
        $"Version {version.Version} download complete!" : 
        $"Version {version.Version} download failed!");
}

void OnMultiAllDownloadCompleted(object arg1, MultiAllDownloadCompletedEventArgs arg2)
{
    Console.WriteLine(arg2.IsAllDownloadCompleted ? 
        "All download tasks completed!" : 
        $"Download tasks failed! Failed count: {arg2.FailedVersions.Count}");
}

void OnMultiDownloadError(object arg1, MultiDownloadErrorEventArgs arg2)
{
    var version = arg2.Version as VersionInfo;
    Console.WriteLine($"Version {version.Version} download error: {arg2.Exception}");
}

void OnException(object arg1, ExceptionEventArgs arg2)
{
    Console.WriteLine($"Upgrade exception: {arg2.Exception}");
}
```

---

## Core API Reference

### GeneralUpdateBootstrap Class Methods

#### LaunchAsync Method

Launch the upgrade process asynchronously.

```csharp
public async Task<GeneralUpdateBootstrap> LaunchAsync()
```

**Return Value:**
- Returns the current GeneralUpdateBootstrap instance, supporting method chaining

#### Option Method

Set upgrade options.

```csharp
public GeneralUpdateBootstrap Option(UpdateOption option, object value)
```

**Parameters:**
- `option`: Upgrade option enum
- `value`: Option value

**Example:**
```csharp
.Option(UpdateOption.Drive, true)  // Enable driver upgrade
```

#### AddListenerMultiDownloadStatistics Method

Listen for download statistics (speed, progress, remaining time, etc.).

```csharp
public GeneralUpdateBootstrap AddListenerMultiDownloadStatistics(
    Action<object, MultiDownloadStatisticsEventArgs> callbackAction)
```

#### AddListenerMultiDownloadCompleted Method

Listen for single update package download completion event.

```csharp
public GeneralUpdateBootstrap AddListenerMultiDownloadCompleted(
    Action<object, MultiDownloadCompletedEventArgs> callbackAction)
```

#### AddListenerMultiAllDownloadCompleted Method

Listen for all version downloads completion event.

```csharp
public GeneralUpdateBootstrap AddListenerMultiAllDownloadCompleted(
    Action<object, MultiAllDownloadCompletedEventArgs> callbackAction)
```

#### AddListenerMultiDownloadError Method

Listen for download error events for each version.

```csharp
public GeneralUpdateBootstrap AddListenerMultiDownloadError(
    Action<object, MultiDownloadErrorEventArgs> callbackAction)
```

#### AddListenerException Method

Listen for all internal exceptions in the upgrade component.

```csharp
public GeneralUpdateBootstrap AddListenerException(
    Action<object, ExceptionEventArgs> callbackAction)
```

#### SetFieldMappings Method

Set field mapping table for parsing driver package information.

```csharp
public GeneralUpdateBootstrap SetFieldMappings(Dictionary<string, string> fieldMappings)
```

**Parameters:**
- `fieldMappings`: Field mapping dictionary, key is English field name, value is localized field name

---

## Configuration Class Details

### UpdateOption Enum

```csharp
public enum UpdateOption
{
    /// <summary>
    /// Whether to enable driver upgrade functionality
    /// </summary>
    Drive
}
```

### Packet Class

Upgrade package information class, passed from ClientCore to Core via parameters:

```csharp
public class Packet
{
    /// <summary>
    /// Main update check API address
    /// </summary>
    public string MainUpdateUrl { get; set; }
    
    /// <summary>
    /// Application type: 1=ClientApp, 2=UpdateApp
    /// </summary>
    public int AppType { get; set; }
    
    /// <summary>
    /// Update check API address
    /// </summary>
    public string UpdateUrl { get; set; }
    
    /// <summary>
    /// Name of the application to be launched
    /// </summary>
    public string AppName { get; set; }
    
    /// <summary>
    /// Main application name
    /// </summary>
    public string MainAppName { get; set; }
    
    /// <summary>
    /// Update package file format (default is Zip)
    /// </summary>
    public string Format { get; set; }
    
    /// <summary>
    /// Indicates if the update application needs to be upgraded
    /// </summary>
    public bool IsUpgradeUpdate { get; set; }
    
    /// <summary>
    /// Indicates if the main application needs to be updated
    /// </summary>
    public bool IsMainUpdate { get; set; }
    
    /// <summary>
    /// Update log webpage URL
    /// </summary>
    public string UpdateLogUrl { get; set; }
    
    /// <summary>
    /// List of version information that needs updating
    /// </summary>
    public List<VersionInfo> UpdateVersions { get; set; }
    
    /// <summary>
    /// File operation encoding format
    /// </summary>
    public Encoding Encoding { get; set; }
    
    /// <summary>
    /// Download timeout duration (seconds)
    /// </summary>
    public int DownloadTimeOut { get; set; }
    
    /// <summary>
    /// Application secret key, agreed upon with the server
    /// </summary>
    public string AppSecretKey { get; set; }
    
    /// <summary>
    /// Current client version
    /// </summary>
    public string ClientVersion { get; set; }
    
    /// <summary>
    /// Latest version
    /// </summary>
    public string LastVersion { get; set; }
    
    /// <summary>
    /// Installation path (used for update file logic)
    /// </summary>
    public string InstallPath { get; set; }
    
    /// <summary>
    /// Temporary storage path for downloaded files
    /// </summary>
    public string TempPath { get; set; }
    
    /// <summary>
    /// Configuration parameters for the upgrade terminal program (Base64 encoded)
    /// </summary>
    public string ProcessBase64 { get; set; }
    
    /// <summary>
    /// Platform to which the current strategy belongs (Windows/Linux/Mac)
    /// </summary>
    public string Platform { get; set; }
    
    /// <summary>
    /// Files in the blacklist
    /// </summary>
    public List<string> BlackFiles { get; set; }
    
    /// <summary>
    /// File formats in the blacklist
    /// </summary>
    public List<string> BlackFormats { get; set; }
    
    /// <summary>
    /// Indicates if the driver upgrade feature is enabled
    /// </summary>
    public bool DriveEnabled { get; set; }
}
```

---

## Practical Usage Examples

### Example 1: Basic Upgrade Process

```csharp
using GeneralUpdate.Common.Download;
using GeneralUpdate.Common.Internal;
using GeneralUpdate.Common.Shared.Object;
using GeneralUpdate.Core;

try
{
    Console.WriteLine("Upgrade program initialization...");
    
    // Launch upgrade process
    await new GeneralUpdateBootstrap()
        .AddListenerMultiDownloadStatistics((sender, args) =>
        {
            var version = args.Version as VersionInfo;
            Console.WriteLine($"[{version.Version}] Download progress: {args.ProgressPercentage}%");
        })
        .AddListenerException((sender, args) =>
        {
            Console.WriteLine($"Upgrade exception: {args.Exception.Message}");
        })
        .LaunchAsync();
        
    Console.WriteLine("Upgrade complete!");
}
catch (Exception e)
{
    Console.WriteLine($"Upgrade failed: {e.Message}");
}
```

### Example 2: Enable Driver Upgrade

```csharp
using GeneralUpdate.Core;
using GeneralUpdate.Common.Internal.Bootstrap;

// Chinese field mapping table
var fieldMappingsCN = new Dictionary<string, string>
{
    { "DriverName", "驱动名称" },
    { "DriverVersion", "驱动版本" },
    { "DriverDescription", "驱动描述" },
    { "InstallPath", "安装路径" }
};

await new GeneralUpdateBootstrap()
    // Set field mappings
    .SetFieldMappings(fieldMappingsCN)
    // Enable driver update
    .Option(UpdateOption.Drive, true)
    .AddListenerException((sender, args) =>
    {
        Console.WriteLine($"Upgrade exception: {args.Exception.Message}");
    })
    .LaunchAsync();
```

### Example 3: Complete Event Listening

```csharp
using GeneralUpdate.Core;
using GeneralUpdate.Common.Download;
using GeneralUpdate.Common.Shared.Object;

await new GeneralUpdateBootstrap()
    // Download statistics
    .AddListenerMultiDownloadStatistics((sender, args) =>
    {
        var version = args.Version as VersionInfo;
        Console.WriteLine($"[{version.Version}]");
        Console.WriteLine($"  Speed: {args.Speed}");
        Console.WriteLine($"  Progress: {args.ProgressPercentage}%");
        Console.WriteLine($"  Downloaded: {args.BytesReceived} / {args.TotalBytesToReceive}");
        Console.WriteLine($"  Remaining time: {args.Remaining}");
    })
    // Single download completed
    .AddListenerMultiDownloadCompleted((sender, args) =>
    {
        var version = args.Version as VersionInfo;
        string status = args.IsComplated ? "✓ Success" : "✗ Failed";
        Console.WriteLine($"Version {version.Version} download {status}");
    })
    // All downloads completed
    .AddListenerMultiAllDownloadCompleted((sender, args) =>
    {
        if (args.IsAllDownloadCompleted)
        {
            Console.WriteLine("✓ All versions downloaded, starting installation...");
        }
        else
        {
            Console.WriteLine($"✗ Download failed, {args.FailedVersions.Count} versions failed:");
            foreach (var version in args.FailedVersions)
            {
                Console.WriteLine($"  - {version}");
            }
        }
    })
    // Download error
    .AddListenerMultiDownloadError((sender, args) =>
    {
        var version = args.Version as VersionInfo;
        Console.WriteLine($"✗ Version {version.Version} error:");
        Console.WriteLine($"  {args.Exception.Message}");
    })
    // Exception handling
    .AddListenerException((sender, args) =>
    {
        Console.WriteLine("⚠ Upgrade process exception:");
        Console.WriteLine($"  Error: {args.Exception.Message}");
        Console.WriteLine($"  Stack: {args.Exception.StackTrace}");
    })
    .LaunchAsync();
```

### Example 4: Custom Upgrade Process

```csharp
using GeneralUpdate.Core;
using GeneralUpdate.Common.Download;
using GeneralUpdate.Common.Shared.Object;

// Record upgrade start time
var startTime = DateTime.Now;
var downloadedVersions = new List<string>();

await new GeneralUpdateBootstrap()
    .AddListenerMultiDownloadCompleted((sender, args) =>
    {
        if (args.IsComplated)
        {
            var version = args.Version as VersionInfo;
            downloadedVersions.Add(version.Version);
        }
    })
    .AddListenerMultiAllDownloadCompleted((sender, args) =>
    {
        if (args.IsAllDownloadCompleted)
        {
            var duration = DateTime.Now - startTime;
            Console.WriteLine($"Upgrade complete!");
            Console.WriteLine($"Total time: {duration.TotalSeconds:F2} seconds");
            Console.WriteLine($"Updated versions: {string.Join(", ", downloadedVersions)}");
        }
    })
    .AddListenerException((sender, args) =>
    {
        // Log to file
        File.AppendAllText("upgrade_error.log", 
            $"[{DateTime.Now}] {args.Exception}\n");
    })
    .LaunchAsync();
```

---

## Notes and Warnings

### ⚠️ Important Notes

1. **Process Isolation**
   - Core must run as an independent process, cannot be called directly in the main program
   - The main program must be completely closed during upgrade, otherwise file replacement will fail

2. **Parameter Passing**
   - ClientCore passes configuration to Core via Base64 encoded parameters
   - Ensure parameters are not truncated or corrupted during passing

3. **File Permissions**
   - Administrator privileges may be required on Windows to replace files in system directories
   - Appropriate file system permissions are required on Linux/macOS

4. **Driver Upgrade**
   - Driver upgrade functionality requires system-level permissions
   - Recommended to thoroughly validate in test environment before use

5. **Rollback Mechanism**
   - Core does not directly provide rollback functionality, but backup files are preserved
   - For rollback, use ClientCore's backup functionality

### 💡 Best Practices

- **Logging**: Implement complete exception listening to record all issues during the upgrade process
- **Timeout Settings**: Set download timeout appropriately based on network environment
- **Progress Feedback**: Display upgrade progress to users to improve user experience
- **Error Handling**: Provide clear error messages and solutions when upgrade fails
- **Testing**: Test upgrade process stability under various network conditions

---

## Applicable Platforms

| Product        | Version       |
| -------------- | ------------- |
| .NET               | 5, 6, 7, 8, 9     |
| .NET Framework     | 4.6.1             |
| .NET Standard      | 2.0               |
| .NET Core          | 2.0               |

---

## Related Resources

- **Sample Code**: [View GitHub Examples](https://github.com/GeneralLibrary/GeneralUpdate-Samples/blob/main/src/Upgrade/Program.cs)
- **Main Repository**: [GeneralUpdate Project](https://github.com/GeneralLibrary/GeneralUpdate)
- **Related Components**: [GeneralUpdate.ClientCore](./GeneralUpdate.ClientCore.md) | [GeneralUpdate.Bowl](./GeneralUpdate.Bowl.md)
