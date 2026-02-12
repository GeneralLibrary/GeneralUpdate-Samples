---
sidebar_position: 4
---

# GeneralUpdate.ClientCore

## Component Overview

**GeneralUpdate.ClientCore** is one of the core components of the GeneralUpdate framework, providing rich client-side update functionalities. This component runs in the main application and is responsible for checking updates, downloading update packages, validating integrity, and then launching the upgrade assistant (GeneralUpdate.Core) to perform actual file replacement operations upon completion. The design philosophy of ClientCore is to enable the main program to safely check and prepare updates without affecting the current running state.

**Namespace:** `GeneralUpdate.ClientCore`  
**Assembly:** `GeneralUpdate.ClientCore.dll`

```csharp
public class GeneralClientBootstrap : AbstractBootstrap<GeneralClientBootstrap, IStrategy>
```

---

## Core Features

### 1. Multi-Version Download Management
- Support downloading multiple version update packages simultaneously
- Resume download and download speed limiting
- Real-time download progress and statistics

### 2. Flexible Configuration Options
- Blacklist mechanism (files, formats, directories)
- Custom update strategies and operations
- Support binary differential updates and full updates

### 3. Comprehensive Event Notifications
- Download progress, completion, and error events
- Support user-defined skip update options
- Exception and error monitoring throughout the process

### 4. Multi-Platform Support
- Windows, Linux, macOS platform support
- Automatic platform detection and strategy selection

![Multi Download](imgs/muti_donwload.png)

---

## Quick Start

### Installation

Install GeneralUpdate.ClientCore via NuGet:

```bash
dotnet add package GeneralUpdate.ClientCore
```

### Initialization and Usage

The following example demonstrates how to configure and launch update checking in the main program:

```csharp
using System.Text;
using GeneralUpdate.ClientCore;
using GeneralUpdate.Common.Download;
using GeneralUpdate.Common.Internal;
using GeneralUpdate.Common.Internal.Bootstrap;
using GeneralUpdate.Common.Shared.Object;

try
{
    Console.WriteLine($"Main program initialization, {DateTime.Now}!");
    
    // Configure update parameters
    var configinfo = new Configinfo
    {
        // Update verification API address
        UpdateUrl = "http://127.0.0.1:5000/Upgrade/Verification",
        // Update report API address
        ReportUrl = "http://127.0.0.1:5000/Upgrade/Report",
        // Main application name
        MainAppName = "ClientSample.exe",
        // Upgrade program name
        AppName = "UpgradeSample.exe",
        // Current client version
        ClientVersion = "1.0.0.0",
        // Upgrade client version
        UpgradeClientVersion = "1.0.0.0",
        // Installation path
        InstallPath = Thread.GetDomain().BaseDirectory,
        // Product ID (for multi-product branch management)
        ProductId = "2d974e2a-31e6-4887-9bb1-b4689e98c77a",
        // App secret key (for server verification)
        AppSecretKey = "dfeb5833-975e-4afb-88f1-6278ee9aeff6"
    };
    
    // Launch update process
    await new GeneralClientBootstrap()
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
        // Set configuration
        .SetConfig(configinfo)
        // Set options
        .Option(UpdateOption.DownloadTimeOut, 60)
        .Option(UpdateOption.Encoding, Encoding.Default)
        // Launch async update
        .LaunchAsync();
        
    Console.WriteLine($"Main program started, {DateTime.Now}!");
}
catch (Exception e)
{
    Console.WriteLine(e.Message + "\n" + e.StackTrace);
}

// Event handler methods
void OnMultiDownloadStatistics(object arg1, MultiDownloadStatisticsEventArgs arg2)
{
    var version = arg2.Version as VersionInfo;
    Console.WriteLine($"Download version: {version.Version}, Speed: {arg2.Speed}, " +
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
    Console.WriteLine($"Update exception: {arg2.Exception}");
}
```

---

## Core API Reference

### GeneralClientBootstrap Class Methods

#### LaunchAsync Method

Launch the update process asynchronously.

```csharp
public async Task<GeneralClientBootstrap> LaunchAsync()
```

#### SetConfig Method

Set update configuration information.

```csharp
public GeneralClientBootstrap SetConfig(Configinfo configinfo)
```

#### Option Method

Set update options.

```csharp
public GeneralClientBootstrap Option(UpdateOption option, object value)
```

#### SetBlacklist Method

Set update blacklist to specify files that should not be updated.

```csharp
public GeneralClientBootstrap SetBlacklist(List<string> blackFiles = null, 
                                           List<string> blackFormats = null)
```

#### AddListenerMultiDownloadStatistics Method

Listen for download statistics (speed, progress, remaining time, etc.).

```csharp
public GeneralClientBootstrap AddListenerMultiDownloadStatistics(
    Action<object, MultiDownloadStatisticsEventArgs> callbackAction)
```

#### AddListenerMultiDownloadCompleted Method

Listen for single update package download completion event.

```csharp
public GeneralClientBootstrap AddListenerMultiDownloadCompleted(
    Action<object, MultiDownloadCompletedEventArgs> callbackAction)
```

#### AddListenerMultiAllDownloadCompleted Method

Listen for all download tasks completion event.

```csharp
public GeneralClientBootstrap AddListenerMultiAllDownloadCompleted(
    Action<object, MultiAllDownloadCompletedEventArgs> callbackAction)
```

#### AddListenerMultiDownloadError Method

Listen for download error events.

```csharp
public GeneralClientBootstrap AddListenerMultiDownloadError(
    Action<object, MultiDownloadErrorEventArgs> callbackAction)
```

#### AddListenerException Method

Listen for all exceptions during the update process.

```csharp
public GeneralClientBootstrap AddListenerException(
    Action<object, ExceptionEventArgs> callbackAction)
```

#### AddCustomOption Method

Add custom asynchronous operations that can execute custom logic before or after updates.

```csharp
public GeneralClientBootstrap AddCustomOption(Func<Task> customFunc)
```

#### SetCustomSkipOption Method

Set custom skip options, allowing users to decide whether to continue with the update.

```csharp
public GeneralClientBootstrap SetCustomSkipOption(Func<bool> customSkipFunc)
```

---

## Configuration Class Details

### Configinfo Class

```csharp
public class Configinfo
{
    /// <summary>
    /// Update check API address
    /// </summary>
    public string UpdateUrl { get; set; }
    
    /// <summary>
    /// Update status report API address
    /// </summary>
    public string ReportUrl { get; set; }
    
    /// <summary>
    /// Application name to be launched (upgrade program)
    /// </summary>
    public string AppName { get; set; }
    
    /// <summary>
    /// Main application name to be launched
    /// </summary>
    public string MainAppName { get; set; }
    
    /// <summary>
    /// Update log webpage address
    /// </summary>
    public string UpdateLogUrl { get; set; }
    
    /// <summary>
    /// Application secret key, agreed upon with server for authentication and product branching
    /// </summary>
    public string AppSecretKey { get; set; }
    
    /// <summary>
    /// Current client version number
    /// </summary>
    public string ClientVersion { get; set; }
    
    /// <summary>
    /// Current upgrade client version number
    /// </summary>
    public string UpgradeClientVersion { get; set; }
    
    /// <summary>
    /// Installation path (used for update file logic)
    /// </summary>
    public string InstallPath { get; set; }
    
    /// <summary>
    /// Blacklist file list, these files will be skipped during updates
    /// </summary>
    public List<string> BlackFiles { get; set; }
    
    /// <summary>
    /// Blacklist file format list, files with these formats will be skipped during updates
    /// </summary>
    public List<string> BlackFormats { get; set; }
    
    /// <summary>
    /// Directory path list to be skipped that do not require updates
    /// </summary>
    public List<string> SkipDirectorys { get; set; }
    
    /// <summary>
    /// Unique ID of the current product branch
    /// </summary>
    public string ProductId { get; set; }
    
    /// <summary>
    /// Bowl monitoring process path, starts after updates to check if the client starts normally
    /// </summary>
    public string Bowl { get; set; }
    
    /// <summary>
    /// Scheme used for passing token in HTTP requests (e.g., Bearer)
    /// </summary>
    public string Scheme { get; set; }
    
    /// <summary>
    /// Token used for authentication in HTTP requests
    /// </summary>
    public string Token { get; set; }
    
    /// <summary>
    /// Script for Linux platform to assign permissions to files after update completion
    /// </summary>
    public string Script { get; set; }
}
```

### UpdateOption Enum

```csharp
public enum UpdateOption
{
    /// <summary>
    /// Update package file format (default is Zip)
    /// </summary>
    Format,
    
    /// <summary>
    /// Compression encoding format
    /// </summary>
    Encoding,
    
    /// <summary>
    /// Download timeout duration (seconds). Default is 30 seconds if not specified
    /// </summary>
    DownloadTimeOut,
    
    /// <summary>
    /// Whether to enable binary differential update function, enabled by default; 
    /// if set to false, full overwrite installation will be performed
    /// </summary>
    Patch,
    
    /// <summary>
    /// Whether to enable backup function before update, enabled by default; 
    /// if set to false, no backup will be performed
    /// </summary>
    BackUp
}
```

---

## Practical Usage Examples

### Example 1: Basic Update Process

```csharp
using GeneralUpdate.ClientCore;
using GeneralUpdate.Common.Internal;

var config = new Configinfo
{
    UpdateUrl = "http://your-server.com/api/update/check",
    ReportUrl = "http://your-server.com/api/update/report",
    MainAppName = "MyApp.exe",
    AppName = "Updater.exe",
    ClientVersion = "1.0.0.0",
    UpgradeClientVersion = "1.0.0.0",
    InstallPath = AppDomain.CurrentDomain.BaseDirectory,
    ProductId = "your-product-id",
    AppSecretKey = "your-secret-key"
};

await new GeneralClientBootstrap()
    .SetConfig(config)
    .AddListenerException((sender, args) => 
    {
        Console.WriteLine($"Update exception: {args.Exception.Message}");
    })
    .LaunchAsync();
```

### Example 2: Update with Blacklist

```csharp
using GeneralUpdate.ClientCore;
using GeneralUpdate.Common.Internal;

var config = new Configinfo
{
    UpdateUrl = "http://your-server.com/api/update/check",
    ClientVersion = "1.0.0.0",
    InstallPath = AppDomain.CurrentDomain.BaseDirectory,
    // Blacklist configuration
    BlackFiles = new List<string> { "config.json", "userdata.db" },
    BlackFormats = new List<string> { ".log", ".cache" },
    SkipDirectorys = new List<string> { "logs", "temp" }
};

await new GeneralClientBootstrap()
    .SetConfig(config)
    .LaunchAsync();
```

### Example 3: Custom Update Options

```csharp
using System.Text;
using GeneralUpdate.ClientCore;
using GeneralUpdate.Common.Internal;
using GeneralUpdate.Common.Internal.Bootstrap;

var config = new Configinfo
{
    UpdateUrl = "http://your-server.com/api/update/check",
    ClientVersion = "1.0.0.0",
    InstallPath = AppDomain.CurrentDomain.BaseDirectory
};

await new GeneralClientBootstrap()
    .SetConfig(config)
    // Set download timeout
    .Option(UpdateOption.DownloadTimeOut, 120)
    // Set encoding format
    .Option(UpdateOption.Encoding, Encoding.UTF8)
    // Enable binary differential update
    .Option(UpdateOption.Patch, true)
    // Enable backup
    .Option(UpdateOption.BackUp, true)
    .LaunchAsync();
```

### Example 4: Complete Event Listening

```csharp
using GeneralUpdate.ClientCore;
using GeneralUpdate.Common.Download;
using GeneralUpdate.Common.Internal;
using GeneralUpdate.Common.Shared.Object;

var config = new Configinfo
{
    UpdateUrl = "http://your-server.com/api/update/check",
    ClientVersion = "1.0.0.0",
    InstallPath = AppDomain.CurrentDomain.BaseDirectory
};

await new GeneralClientBootstrap()
    .SetConfig(config)
    // Download statistics
    .AddListenerMultiDownloadStatistics((sender, args) =>
    {
        var version = args.Version as VersionInfo;
        Console.WriteLine($"[{version.Version}] Progress: {args.ProgressPercentage}% | " +
                         $"Speed: {args.Speed} | Remaining: {args.Remaining}");
    })
    // Single download completed
    .AddListenerMultiDownloadCompleted((sender, args) =>
    {
        var version = args.Version as VersionInfo;
        if (args.IsComplated)
            Console.WriteLine($"✓ Version {version.Version} download succeeded");
        else
            Console.WriteLine($"✗ Version {version.Version} download failed");
    })
    // All downloads completed
    .AddListenerMultiAllDownloadCompleted((sender, args) =>
    {
        if (args.IsAllDownloadCompleted)
            Console.WriteLine("✓ All update packages downloaded, preparing installation...");
        else
            Console.WriteLine($"✗ Download failed, {args.FailedVersions.Count} versions failed");
    })
    // Download error
    .AddListenerMultiDownloadError((sender, args) =>
    {
        var version = args.Version as VersionInfo;
        Console.WriteLine($"✗ Version {version.Version} download error: {args.Exception.Message}");
    })
    // Exception handling
    .AddListenerException((sender, args) =>
    {
        Console.WriteLine($"⚠ Update exception: {args.Exception.Message}\n{args.Exception.StackTrace}");
    })
    .LaunchAsync();
```

### Example 5: Custom Operations and Skip Options

```csharp
using GeneralUpdate.ClientCore;
using GeneralUpdate.Common.Internal;

var config = new Configinfo
{
    UpdateUrl = "http://your-server.com/api/update/check",
    ClientVersion = "1.0.0.0",
    InstallPath = AppDomain.CurrentDomain.BaseDirectory
};

await new GeneralClientBootstrap()
    .SetConfig(config)
    // Add custom operation (check environment before update)
    .AddCustomOption(async () =>
    {
        Console.WriteLine("Checking runtime environment...");
        await Task.Delay(1000);
        // Check disk space, dependencies, etc.
        Console.WriteLine("Environment check completed");
    })
    // Set user skip option
    .SetCustomSkipOption(() =>
    {
        Console.WriteLine("New version found, update now? (y/n)");
        var input = Console.ReadLine();
        return input?.ToLower() == "y";
    })
    .LaunchAsync();
```

---

## Notes and Warnings

### ⚠️ Important Notes

1. **Version Number Format**
   - Version numbers must follow semantic versioning specification (e.g., 1.0.0.0)
   - Ensure version number format is consistent between client and server

2. **Network Connection**
   - Ensure update server address is accessible
   - Recommend implementing retry mechanism to handle network fluctuations

3. **Process Management**
   - The update process will close the main program and launch the upgrade assistant
   - Ensure all user data is saved before executing the update

4. **Permission Requirements**
   - Administrator privileges may be required on Windows to replace files
   - Appropriate file system permissions are required on Linux/macOS

5. **Blacklist Usage**
   - Files and directories in the blacklist will not be updated
   - Commonly used to protect configuration files, user data, etc.

### 💡 Best Practices

- **Backup Strategy**: Always enable the BackUp option to allow rollback in case of update failure
- **Differential Update**: Enable the Patch option to reduce download size and update time
- **Error Handling**: Implement complete exception listening and error handling logic
- **User Experience**: Prompt users before updating and allow them to choose update timing
- **Testing**: Thoroughly test the update process before production deployment

---

## Applicable Platforms

| Product        | Version          |
| -------------- | ---------------- |
| .NET               | 5, 6, 7, 8, 9, 10    |
| .NET Framework     | 4.6.1                |
| .NET Standard      | 2.0                  |
| .NET Core          | 2.0                  |

---

## Related Resources

- **Sample Code**: [View GitHub Examples](https://github.com/GeneralLibrary/GeneralUpdate-Samples/blob/main/src/Client/Program.cs)
- **Main Repository**: [GeneralUpdate Project](https://github.com/GeneralLibrary/GeneralUpdate)
