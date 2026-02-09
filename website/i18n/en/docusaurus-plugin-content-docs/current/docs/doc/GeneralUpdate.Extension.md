---
sidebar_position: 12
---

### Definition

Namespace: GeneralUpdate.Extension

Assembly: GeneralUpdate.ClientCore.dll, GeneralUpdate.Core.dll



GeneralUpdate.Extension provides a comprehensive extensibility framework that allows developers to customize and extend the update process. It enables you to inject custom logic, handle events, and tailor the update behavior to meet specific application requirements. This component is essential for developers who need fine-grained control over the update workflow.

```c#
// Extension methods are available through GeneralClientBootstrap and GeneralUpdateBootstrap
public class GeneralClientBootstrap : AbstractBootstrap<GeneralClientBootstrap, IStrategy>
```



### Example

The Extension framework provides various ways to extend functionality [[View Example]](https://github.com/GeneralLibrary/GeneralUpdate-Samples/blob/main/src/Client/Program.cs)。

```c#
var configinfo = new Configinfo
{
    UpdateUrl = "http://127.0.0.1:5000/Upgrade/Verification",
    AppName = "YourApp.exe",
    ClientVersion = "1.0.0.0",
    // ... other configuration
};

await new GeneralClientBootstrap()
    // Add custom event listeners
    .AddListenerMultiDownloadStatistics(OnDownloadProgress)
    .AddListenerMultiDownloadCompleted(OnDownloadCompleted)
    .AddListenerException(OnException)
    // Add custom operations
    .AddCustomOption(CheckEnvironment)
    .SetCustomSkipOption(ShowSkipDialog)
    .SetConfig(configinfo)
    .LaunchAsync();
```



### Core Extension Capabilities

GeneralUpdate.Extension provides the following extensibility features.

#### Event Listeners

| Method                                 | Description                                                  |
| -------------------------------------- | ------------------------------------------------------------ |
| AddListenerMultiDownloadStatistics()   | Subscribe to download progress notifications including speed, remaining time, bytes received, and progress percentage. Ideal for displaying real-time download information to users. |
| AddListenerMultiDownloadCompleted()    | Event triggered when a single update package download completes (successfully or unsuccessfully). Allows you to handle per-version download completion. |
| AddListenerMultiAllDownloadCompleted() | Notification when all update packages have been downloaded. This is the final checkpoint before the update installation begins. |
| AddListenerMultiDownloadError()        | Listen for download errors for each version. Provides exception details to help diagnose network or server issues. |
| AddListenerException()                 | Global exception handler that catches any errors during the entire update process. Critical for error logging and user notification. |

#### Custom Operations

| Method                | Description                                                  |
| --------------------- | ------------------------------------------------------------ |
| AddCustomOption()     | Inject custom asynchronous operations into the update workflow. Executed before the update starts. Perfect for environment checks, pre-update backups, or any preparatory tasks. Returns a Task<bool> where false cancels the update. |
| SetCustomSkipOption() | Allows users to decide whether to skip non-mandatory updates. Provides a way to show custom UI for user consent. Only effective when the server doesn't enforce mandatory updates. |
| SetBlacklist()        | Define files or file formats that should never be updated. Useful for preserving user data, configuration files, or third-party dependencies. |

#### Configuration Options

| Method        | Description                                                  |
| ------------- | ------------------------------------------------------------ |
| Option()      | Configure update behavior such as timeout duration, encoding format, patch enablement, and backup settings. Provides granular control over the update process. |
| Config()      | Set core update parameters including server URLs, version information, authentication keys, and installation paths. |
| GetOption()   | Retrieve current update configuration settings for inspection or modification. |
| SetConfig()   | Apply a complete Configinfo object to configure the update client. |



### Event Arguments

#### MultiDownloadStatisticsEventArgs

Provides detailed download progress information.

| Property           | Type        | Description                                          |
| ------------------ | ----------- | ---------------------------------------------------- |
| Version            | VersionInfo | Current version being downloaded                     |
| Speed              | string      | Download speed (e.g., "2.5 MB/s")                    |
| Remaining          | string      | Estimated time remaining (e.g., "00:02:30")          |
| BytesReceived      | long        | Number of bytes downloaded so far                    |
| TotalBytesToReceive| long        | Total size of the download in bytes                  |
| ProgressPercentage | double      | Download progress as a percentage (0-100)            |

#### MultiDownloadCompletedEventArgs

Indicates completion status of a single update package.

| Property     | Type        | Description                                          |
| ------------ | ----------- | ---------------------------------------------------- |
| Version      | VersionInfo | Version information for the completed download       |
| IsComplated  | bool        | Whether the download completed successfully          |

#### MultiAllDownloadCompletedEventArgs

Summary of all download tasks.

| Property               | Type            | Description                                |
| ---------------------- | --------------- | ------------------------------------------ |
| IsAllDownloadCompleted | bool            | Whether all downloads completed successfully|
| FailedVersions         | List<VersionInfo>| List of versions that failed to download   |

#### MultiDownloadErrorEventArgs

Error information for failed downloads.

| Property  | Type        | Description                                          |
| --------- | ----------- | ---------------------------------------------------- |
| Version   | VersionInfo | Version that encountered an error                    |
| Exception | Exception   | The exception that occurred during download          |

#### ExceptionEventArgs

Global exception information.

| Property  | Type      | Description                                          |
| --------- | --------- | ---------------------------------------------------- |
| Exception | Exception | Exception that occurred during the update process    |



### Custom Operation Example

Here's a complete example showing how to use custom operations:

```c#
private async Task<bool> CheckEnvironment()
{
    try
    {
        // Check if required dependencies are installed
        if (!IsDependencyInstalled("SomeLibrary"))
        {
            Console.WriteLine("Required dependency is missing!");
            return false; // Cancel update
        }
        
        // Verify disk space
        var requiredSpace = 500 * 1024 * 1024; // 500 MB
        if (GetAvailableDiskSpace() < requiredSpace)
        {
            Console.WriteLine("Insufficient disk space!");
            return false; // Cancel update
        }
        
        // Create backup of critical files
        await BackupUserData();
        
        return true; // Proceed with update
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Environment check failed: {ex.Message}");
        return false;
    }
}

// Register the custom operation
await new GeneralClientBootstrap()
    .AddCustomOption(CheckEnvironment)
    .SetConfig(configinfo)
    .LaunchAsync();
```



### Skip Option Example

Allow users to skip non-mandatory updates:

```c#
private async Task<bool> ShowSkipDialog()
{
    // Show a custom dialog to the user
    var result = await ShowUpdateDialog(
        "A new update is available!",
        "Would you like to update now?",
        new[] { "Update Now", "Skip" }
    );
    
    return result == "Update Now";
}

// Register the skip option handler
await new GeneralClientBootstrap()
    .SetCustomSkipOption(ShowSkipDialog)
    .SetConfig(configinfo)
    .LaunchAsync();
```



### Blacklist Configuration

Exclude specific files or formats from updates:

```c#
var configinfo = new Configinfo
{
    // ... other configuration
    BlackFiles = new List<string> 
    { 
        "userconfig.json",
        "license.dat",
        "custom.db"
    },
    BlackFormats = new List<string> 
    { 
        ".log",
        ".temp",
        ".cache"
    },
    BlackDirectories = new List<string>
    {
        "UserData",
        "Plugins\\Custom"
    }
};

// Or use the SetBlacklist method
await new GeneralClientBootstrap()
    .SetBlacklist(blackFiles: new List<string> { "config.ini" })
    .SetConfig(configinfo)
    .LaunchAsync();
```



### Complete Extension Example

Here's a comprehensive example using multiple extension features:

```c#
using GeneralUpdate.ClientCore;
using GeneralUpdate.Common.Shared.Object;

public class UpdateManager
{
    public async Task StartUpdateWithExtensions()
    {
        var configinfo = new Configinfo
        {
            UpdateUrl = "http://127.0.0.1:5000/Upgrade/Verification",
            ReportUrl = "http://127.0.0.1:5000/Upgrade/Report",
            AppName = "MyApp.exe",
            MainAppName = "MyApp.exe",
            ClientVersion = "1.0.0.0",
            ProductId = "your-product-id",
            AppSecretKey = "your-secret-key",
            InstallPath = AppDomain.CurrentDomain.BaseDirectory,
            BlackFiles = new List<string> { "userdata.db" }
        };

        try
        {
            await new GeneralClientBootstrap()
                // Register event listeners
                .AddListenerMultiDownloadStatistics(OnDownloadProgress)
                .AddListenerMultiDownloadCompleted(OnDownloadCompleted)
                .AddListenerMultiAllDownloadCompleted(OnAllDownloadsCompleted)
                .AddListenerMultiDownloadError(OnDownloadError)
                .AddListenerException(OnException)
                // Add custom operations
                .AddCustomOption(PerformPreUpdateChecks)
                .SetCustomSkipOption(AskUserForUpdatePermission)
                // Configure options
                .SetConfig(configinfo)
                .Option(UpdateOption.DownloadTimeOut, 120)
                .Option(UpdateOption.Patch, true)
                .Option(UpdateOption.BackUp, true)
                // Launch the update
                .LaunchAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Update failed: {ex.Message}");
        }
    }

    private void OnDownloadProgress(object sender, MultiDownloadStatisticsEventArgs e)
    {
        var version = e.Version as VersionInfo;
        Console.WriteLine($"Downloading {version.Version}: {e.ProgressPercentage:F2}% at {e.Speed}");
        // Update UI progress bar here
    }

    private void OnDownloadCompleted(object sender, MultiDownloadCompletedEventArgs e)
    {
        var version = e.Version as VersionInfo;
        if (e.IsComplated)
        {
            Console.WriteLine($"Version {version.Version} downloaded successfully!");
        }
        else
        {
            Console.WriteLine($"Failed to download version {version.Version}");
        }
    }

    private void OnAllDownloadsCompleted(object sender, MultiAllDownloadCompletedEventArgs e)
    {
        if (e.IsAllDownloadCompleted)
        {
            Console.WriteLine("All updates downloaded successfully!");
        }
        else
        {
            Console.WriteLine($"Update failed. {e.FailedVersions.Count} versions failed.");
        }
    }

    private void OnDownloadError(object sender, MultiDownloadErrorEventArgs e)
    {
        var version = e.Version as VersionInfo;
        Console.WriteLine($"Error downloading {version.Version}: {e.Exception.Message}");
    }

    private void OnException(object sender, ExceptionEventArgs e)
    {
        Console.WriteLine($"Update exception: {e.Exception.Message}");
        // Log exception to file or monitoring system
    }

    private async Task<bool> PerformPreUpdateChecks()
    {
        // Check system requirements
        Console.WriteLine("Performing pre-update checks...");
        
        // Verify network connectivity
        if (!await CheckNetworkConnection())
        {
            Console.WriteLine("No network connection available!");
            return false;
        }
        
        // Check disk space
        if (!HasSufficientDiskSpace(500 * 1024 * 1024))
        {
            Console.WriteLine("Insufficient disk space!");
            return false;
        }
        
        Console.WriteLine("Pre-update checks passed!");
        return true;
    }

    private async Task<bool> AskUserForUpdatePermission()
    {
        Console.WriteLine("New update available. Update now? (y/n)");
        var response = Console.ReadLine();
        return response?.ToLower() == "y";
    }

    private async Task<bool> CheckNetworkConnection()
    {
        // Implement network check
        return true;
    }

    private bool HasSufficientDiskSpace(long requiredBytes)
    {
        // Implement disk space check
        return true;
    }
}
```



### Best Practices

1. **Always handle exceptions**: Use `AddListenerException()` to catch and log all update errors.

2. **Validate environment before updates**: Use `AddCustomOption()` to check system requirements, dependencies, and available disk space.

3. **Provide user feedback**: Subscribe to download progress events to show real-time updates to users.

4. **Protect user data**: Use blacklist features to exclude user configuration files and data from updates.

5. **Implement retry logic**: In error handlers, implement intelligent retry mechanisms for transient failures.

6. **Test custom operations**: Thoroughly test all custom operations to ensure they don't block legitimate updates.

7. **Use appropriate timeouts**: Set reasonable timeout values based on expected file sizes and network conditions.

8. **Log everything**: Maintain detailed logs of the update process for troubleshooting.



### Common Use Cases

#### Use Case 1: Environment Validation

Before updating, verify that all prerequisites are met:

```c#
private async Task<bool> ValidateEnvironment()
{
    // Check .NET runtime version
    if (!IsRuntimeVersionSupported())
        return false;
    
    // Verify required services are running
    if (!AreServicesRunning(new[] { "ServiceA", "ServiceB" }))
        return false;
    
    // Check file permissions
    if (!HasWritePermission(installPath))
        return false;
    
    return true;
}
```

#### Use Case 2: Custom Update Notification

Show custom notifications to users during the update process:

```c#
private void NotifyUser(object sender, MultiDownloadStatisticsEventArgs e)
{
    // Show system tray notification
    ShowNotification($"Updating: {e.ProgressPercentage:F0}%");
    
    // Update application title
    UpdateWindowTitle($"Downloading update... {e.ProgressPercentage:F0}%");
}
```

#### Use Case 3: Conditional Updates

Apply updates based on specific conditions:

```c#
private async Task<bool> ShouldApplyUpdate()
{
    // Check if it's business hours
    var now = DateTime.Now;
    if (now.Hour >= 9 && now.Hour <= 17)
    {
        // Defer update during business hours
        return false;
    }
    
    // Check if critical operations are running
    if (IsCriticalOperationInProgress())
    {
        return false;
    }
    
    return true;
}
```



### Troubleshooting

#### Problem: Custom operation blocks the update indefinitely

**Solution**: Ensure your custom operations have timeouts and proper error handling:

```c#
private async Task<bool> CustomOperationWithTimeout()
{
    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
    try
    {
        await YourAsyncOperation(cts.Token);
        return true;
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("Operation timed out");
        return false;
    }
}
```

#### Problem: Events not firing

**Solution**: Ensure you register event listeners before calling `LaunchAsync()`:

```c#
// Correct order
await new GeneralClientBootstrap()
    .AddListenerException(OnException)  // Register first
    .SetConfig(configinfo)
    .LaunchAsync();  // Launch last
```

#### Problem: Skip option not working

**Solution**: Verify the server hasn't set the update as mandatory. The skip option only works for non-mandatory updates.



### Applicable To

| Product        | Version          |
| -------------- | ---------------- |
| .NET           | 5, 6, 7, 8, 9, 10|
| .NET Framework | 4.6.1            |
| .NET Standard  | 2.0              |
| .NET Core      | 2.0              |

### See Also

- [GeneralUpdate.ClientCore](./GeneralUpdate.ClientCore.md) - Main client update component
- [GeneralUpdate.Core](./GeneralUpdate.Core.md) - Core update logic
- [Quick Start Guide](../quickstart/Quik%20start.md) - Getting started with GeneralUpdate
