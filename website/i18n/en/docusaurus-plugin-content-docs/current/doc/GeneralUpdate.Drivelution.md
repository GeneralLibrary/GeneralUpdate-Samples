---
sidebar_position: 12
---

### Definition

Namespace: GeneralUpdate.Drivelution

Assembly: GeneralUpdate.Drivelution.dll



GeneralUpdate.Drivelution is a cross-platform driver update component that provides automatic detection, validation, backup, installation, and rollback capabilities for device drivers. It supports Windows, Linux, and MacOS platforms and offers a unified API interface to simplify the driver update process while ensuring security and reliability.

```c#
public static class GeneralDrivelution
```



### Applicable Scenarios

GeneralUpdate.Drivelution is suitable for the following scenarios:

- **Enterprise device management**: Batch update drivers for multiple devices in corporate environments
- **IoT device updates**: Remote driver updates for embedded systems and IoT devices
- **Hardware vendor software**: Integrate driver update functionality into hardware vendor applications
- **System maintenance tools**: Build automated driver update and maintenance tools
- **Cross-platform applications**: Applications that need to manage drivers on multiple operating systems



### Installation & Configuration

#### Installation

Install via NuGet package manager:

```bash
dotnet add package GeneralUpdate.Drivelution
```

Or add the following to your project file:

```xml
<PackageReference Include="GeneralUpdate.Drivelution" Version="*" />
```

#### Configuration

Create a configuration options object (optional, defaults are used if not specified):

```c#
var options = new DrivelutionOptions
{
    LogLevel = "Info",                          // Log level (Debug/Info/Warn/Error/Fatal)
    LogFilePath = "./Logs/drivelution-.log",    // Log file path
    EnableConsoleLogging = true,                // Enable console logging
    EnableFileLogging = true,                   // Enable file logging
    DefaultBackupPath = "./DriverBackups",      // Default backup path
    DefaultRetryCount = 3,                      // Default retry count
    DefaultRetryIntervalSeconds = 5,            // Default retry interval (seconds)
    DefaultTimeoutSeconds = 300,                // Default timeout (seconds)
    AutoCleanupBackups = true,                  // Auto cleanup old backups
    BackupsToKeep = 5                           // Number of backups to keep
};
```



### Quick Start

#### Basic Usage (Hello World)

The simplest driver update example:

```c#
using GeneralUpdate.Drivelution;
using GeneralUpdate.Drivelution.Abstractions.Models;

// 1. Create driver information
var driverInfo = new DriverInfo
{
    Name = "MyDevice Driver",
    Version = "1.2.0",
    FilePath = "C:\\Drivers\\mydevice.inf",  // Windows: .inf, Linux: .ko, MacOS: .kext
    TargetOS = "Windows",
    Architecture = "x64",
    Hash = "abc123...",                       // SHA256 hash of the driver file
    HashAlgorithm = "SHA256"
};

// 2. Quick update with default settings
var result = await GeneralDrivelution.QuickUpdateAsync(driverInfo);

// 3. Check result
if (result.Success)
{
    Console.WriteLine("Driver updated successfully!");
    Console.WriteLine($"Duration: {result.DurationMs}ms");
}
else
{
    Console.WriteLine($"Update failed: {result.Error?.Message}");
}
```

#### Advanced Usage with Custom Strategy

```c#
using GeneralUpdate.Drivelution;
using GeneralUpdate.Drivelution.Abstractions.Models;
using GeneralUpdate.Drivelution.Abstractions.Configuration;

// 1. Create configuration options
var options = new DrivelutionOptions
{
    LogLevel = "Debug",
    DefaultBackupPath = "C:\\DriverBackups"
};

// 2. Create updater instance
var updater = GeneralDrivelution.Create(options);

// 3. Define driver information
var driverInfo = new DriverInfo
{
    Name = "Graphics Driver",
    Version = "2.1.0",
    FilePath = "C:\\Drivers\\graphics.inf",
    TargetOS = "Windows",
    Architecture = "x64",
    Hash = "def456...",
    HashAlgorithm = "SHA256",
    HardwareId = "PCI\\VEN_10DE&DEV_1234",
    Description = "NVIDIA Graphics Driver"
};

// 4. Define update strategy
var strategy = new UpdateStrategy
{
    RequireBackup = true,                      // Enable backup
    BackupPath = "C:\\DriverBackups\\graphics",
    RetryCount = 3,                            // Retry 3 times on failure
    RetryIntervalSeconds = 5,                  // Wait 5 seconds between retries
    RestartMode = RestartMode.Prompt,          // Prompt user to restart
    SkipSignatureValidation = false,           // Enable signature validation
    TimeoutSeconds = 600                       // 10-minute timeout
};

// 5. Execute update
var result = await updater.UpdateAsync(driverInfo, strategy);

// 6. Handle result
if (result.Success)
{
    Console.WriteLine($"✓ Update succeeded in {result.DurationMs}ms");
    if (!string.IsNullOrEmpty(result.BackupPath))
    {
        Console.WriteLine($"✓ Backup saved to: {result.BackupPath}");
    }
}
else
{
    Console.WriteLine($"✗ Update failed: {result.Error?.Code}");
    Console.WriteLine($"  Message: {result.Error?.Message}");
    
    if (result.RolledBack)
    {
        Console.WriteLine("✓ System rolled back to previous state");
    }
}
```



### Core Modules & API Description

#### Main Entry Class: GeneralDrivelution

The static entry class provides the following methods:

| Method | Description |
| ------ | ----------- |
| `Create(options?)` | Creates a driver updater instance with automatic platform detection |
| `Create(logger, options?)` | Creates a driver updater instance with custom logger |
| `QuickUpdateAsync(driverInfo)` | Quick driver update with default settings |
| `QuickUpdateAsync(driverInfo, strategy)` | Quick driver update with custom strategy |
| `ValidateAsync(driverInfo)` | Validates driver file |
| `GetPlatformInfo()` | Gets current platform information |
| `GetDriversFromDirectoryAsync(path)` | Reads driver information from local directory |

#### Core Interface: IGeneralDrivelution

The updater interface provides the following capabilities:

| Method | Description |
| ------ | ----------- |
| `UpdateAsync(driverInfo, strategy, token?)` | Updates driver asynchronously |
| `ValidateAsync(driverInfo, token?)` | Validates driver asynchronously |
| `BackupAsync(driverInfo, backupPath, token?)` | Backs up driver asynchronously |
| `RollbackAsync(backupPath, token?)` | Rolls back driver asynchronously |
| `GetDriversFromDirectoryAsync(path, pattern?, token?)` | Reads driver information from directory |

#### Data Models

##### DriverInfo

Driver information model:

| Property | Type | Description |
| -------- | ---- | ----------- |
| `Name` | string | Driver name |
| `Version` | string | Driver version (follows SemVer 2.0) |
| `FilePath` | string | Driver file path |
| `TargetOS` | string | Target operating system |
| `Architecture` | string | System architecture (x86, x64, ARM, ARM64) |
| `HardwareId` | string | Hardware ID (Windows hardware ID or Linux PCI/USB device ID) |
| `Hash` | string | File hash for integrity validation |
| `HashAlgorithm` | string | Hash algorithm (SHA256, MD5) |
| `TrustedPublishers` | List\<string\> | Trusted publishers list |
| `Description` | string | Driver description |
| `ReleaseDate` | DateTime | Driver release date |
| `Metadata` | Dictionary | Additional metadata |

##### UpdateStrategy

Update strategy model:

| Property | Type | Description |
| -------- | ---- | ----------- |
| `Mode` | UpdateMode | Update mode (Full/Incremental) |
| `ForceUpdate` | bool | Whether to force update |
| `RequireBackup` | bool | Whether backup is required |
| `BackupPath` | string | Backup path |
| `RetryCount` | int | Retry count on failure |
| `RetryIntervalSeconds` | int | Retry interval (seconds) |
| `Priority` | int | Update priority (for batch updates) |
| `RestartMode` | RestartMode | Restart mode after update |
| `SkipSignatureValidation` | bool | Skip signature validation (debug mode only) |
| `SkipHashValidation` | bool | Skip hash validation (debug mode only) |
| `TimeoutSeconds` | int | Timeout (seconds) |

##### UpdateResult

Update result model:

| Property | Type | Description |
| -------- | ---- | ----------- |
| `Success` | bool | Whether update succeeded |
| `Status` | UpdateStatus | Update status |
| `Error` | ErrorInfo? | Error information |
| `StartTime` | DateTime | Update start time |
| `EndTime` | DateTime | Update end time |
| `DurationMs` | long | Update duration (milliseconds) |
| `BackupPath` | string? | Backup path (if backed up) |
| `RolledBack` | bool | Whether rolled back |
| `Message` | string | Additional message |
| `StepLogs` | List\<string\> | Update step logs |

##### DrivelutionOptions

Configuration options:

| Property | Type | Description |
| -------- | ---- | ----------- |
| `DefaultBackupPath` | string | Default backup path |
| `LogLevel` | string | Log level (Debug/Info/Warn/Error/Fatal) |
| `LogFilePath` | string | Log file path |
| `EnableConsoleLogging` | bool | Enable console logging |
| `EnableFileLogging` | bool | Enable file logging |
| `DefaultRetryCount` | int | Default retry count |
| `DefaultRetryIntervalSeconds` | int | Default retry interval (seconds) |
| `DefaultTimeoutSeconds` | int | Default timeout (seconds) |
| `DebugModeSkipSignature` | bool | Skip signature validation in debug mode |
| `DebugModeSkipHash` | bool | Skip hash validation in debug mode |
| `ForceTerminateOnPermissionFailure` | bool | Force terminate on permission check failure |
| `AutoCleanupBackups` | bool | Auto cleanup old backups |
| `BackupsToKeep` | int | Number of backups to keep |
| `TrustedCertificateThumbprints` | List\<string\> | Trusted certificate thumbprints (for signature validation) |
| `TrustedGpgKeys` | List\<string\> | Trusted GPG public keys (for Linux) |



### Cross-Platform Usage

GeneralUpdate.Drivelution supports Windows, Linux, and MacOS platforms with automatic platform detection and adaptation.

#### Windows Platform

**Supported Driver Types:**
- INF-based drivers (*.inf)
- PnP drivers
- Kernel-mode drivers
- User-mode drivers

**Example:**

```c#
var driverInfo = new DriverInfo
{
    Name = "USB Device Driver",
    Version = "3.0.1",
    FilePath = "C:\\Drivers\\usbdevice.inf",
    TargetOS = "Windows",
    Architecture = "x64",
    Hash = "sha256_hash_here",
    HardwareId = "USB\\VID_1234&PID_5678"
};

var strategy = new UpdateStrategy
{
    RequireBackup = true,
    RestartMode = RestartMode.Prompt
};

var updater = GeneralDrivelution.Create();
var result = await updater.UpdateAsync(driverInfo, strategy);
```

**Windows-Specific Features:**
- Automatic signature validation using Windows Authenticode
- Hardware ID matching for device drivers
- Integration with Windows Driver Store
- Support for driver installation via PnPUtil

#### Linux Platform

**Supported Driver Types:**
- Kernel modules (*.ko)
- Device Tree overlays
- Firmware files

**Example:**

```c#
var driverInfo = new DriverInfo
{
    Name = "Network Adapter Driver",
    Version = "1.5.0",
    FilePath = "/lib/modules/drivers/netadapter.ko",
    TargetOS = "Linux",
    Architecture = "x64",
    Hash = "sha256_hash_here"
};

var strategy = new UpdateStrategy
{
    RequireBackup = true,
    RestartMode = RestartMode.Delayed  // Delay restart for Linux systems
};

var updater = GeneralDrivelution.Create();
var result = await updater.UpdateAsync(driverInfo, strategy);
```

**Linux-Specific Features:**
- GPG signature validation
- Kernel module dependency checking
- Integration with modprobe and insmod
- Support for DKMS (Dynamic Kernel Module Support)

**Required Permissions:**
```bash
# Run with sudo or root privileges
sudo dotnet run MyApp.dll
```

#### MacOS Platform

**Supported Driver Types:**
- Kernel extensions (*.kext)
- DriverKit extensions

**Example:**

```c#
var driverInfo = new DriverInfo
{
    Name = "Audio Device Driver",
    Version = "2.3.0",
    FilePath = "/Library/Extensions/AudioDevice.kext",
    TargetOS = "MacOS",
    Architecture = "ARM64",
    Hash = "sha256_hash_here"
};

var strategy = new UpdateStrategy
{
    RequireBackup = true,
    RestartMode = RestartMode.Prompt
};

var updater = GeneralDrivelution.Create();
var result = await updater.UpdateAsync(driverInfo, strategy);
```

**MacOS-Specific Features:**
- Code signature validation using codesign
- System Integrity Protection (SIP) awareness
- Support for notarized drivers
- Integration with kextutil

**Required Permissions:**
```bash
# Run with administrator privileges
sudo dotnet run MyApp.dll
```

#### Platform Detection

Automatically detect current platform:

```c#
var platformInfo = GeneralDrivelution.GetPlatformInfo();

Console.WriteLine($"Platform: {platformInfo.Platform}");
Console.WriteLine($"OS: {platformInfo.OperatingSystem}");
Console.WriteLine($"Architecture: {platformInfo.Architecture}");
Console.WriteLine($"Supported: {platformInfo.IsSupported}");
```

#### Cross-Platform Code Example

Write once, run everywhere:

```c#
// This code works on Windows, Linux, and MacOS
var updater = GeneralDrivelution.Create();

// Platform-specific driver paths can be determined at runtime
var driverPath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) 
    ? "C:\\Drivers\\device.inf"
    : RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
        ? "/lib/modules/device.ko"
        : "/Library/Extensions/device.kext";

var driverInfo = new DriverInfo
{
    Name = "Device Driver",
    Version = "1.0.0",
    FilePath = driverPath,
    Hash = "calculated_hash"
};

var result = await updater.UpdateAsync(driverInfo, new UpdateStrategy());
```



### Frequently Asked Questions (FAQ)

#### Q1: What platforms are supported?

**A:** GeneralUpdate.Drivelution supports Windows (7/8/10/11/Server), Linux (various distributions), and MacOS (10.13+). The component automatically detects the platform and uses the appropriate driver update strategy.

#### Q2: Do I need administrator/root privileges?

**A:** Yes, driver updates require elevated privileges on all platforms:
- **Windows**: Run as Administrator
- **Linux**: Run with sudo or as root
- **MacOS**: Run with sudo or as root

#### Q3: How do I calculate the driver file hash?

**A:** You can use built-in utilities or the following code:

```c#
using System.Security.Cryptography;

string CalculateFileHash(string filePath)
{
    using var sha256 = SHA256.Create();
    using var stream = File.OpenRead(filePath);
    var hash = sha256.ComputeHash(stream);
    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
}
```

#### Q4: What happens if the update fails?

**A:** If `RequireBackup` is enabled in the update strategy, the system will automatically roll back to the previous driver version. The `UpdateResult` object will contain detailed error information.

#### Q5: Can I update multiple drivers at once?

**A:** Yes, you can update multiple drivers sequentially or in parallel:

```c#
var drivers = new List<DriverInfo> { driver1, driver2, driver3 };
var tasks = drivers.Select(d => updater.UpdateAsync(d, strategy));
var results = await Task.WhenAll(tasks);
```

#### Q6: How do I handle driver signature validation?

**A:** By default, signature validation is enabled. To skip validation (not recommended in production):

```c#
var strategy = new UpdateStrategy
{
    SkipSignatureValidation = true  // Only for testing!
};
```

For production, ensure drivers are properly signed:
- **Windows**: Use Microsoft Authenticode
- **Linux**: Use GPG signing
- **MacOS**: Use Apple code signing

#### Q7: Does the system need to restart after updating drivers?

**A:** It depends on the driver type and platform. You can control restart behavior via `RestartMode`:
- `RestartMode.None`: No restart required
- `RestartMode.Prompt`: Prompt user to restart
- `RestartMode.Delayed`: Schedule restart for later
- `RestartMode.Immediate`: Restart immediately

#### Q8: How do I validate a driver before updating?

**A:** Use the `ValidateAsync` method:

```c#
var isValid = await updater.ValidateAsync(driverInfo);
if (isValid)
{
    // Proceed with update
    var result = await updater.UpdateAsync(driverInfo, strategy);
}
```

#### Q9: Can I customize logging?

**A:** Yes, you can provide a custom Serilog logger:

```c#
var logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/driver-update.log")
    .CreateLogger();

var updater = GeneralDrivelution.Create(logger, options);
```

#### Q10: Where are driver backups stored?

**A:** Backups are stored in the path specified by:
1. `UpdateStrategy.BackupPath` (if specified)
2. `DrivelutionOptions.DefaultBackupPath` (if configured)
3. Default: `./DriverBackups` directory

Old backups are automatically cleaned up if `AutoCleanupBackups` is enabled.

#### Q11: How do I get driver information from a directory?

**A:** Use the `GetDriversFromDirectoryAsync` method:

```c#
// Get all drivers from directory
var drivers = await updater.GetDriversFromDirectoryAsync("C:\\Drivers");

// Get drivers with specific pattern
var infDrivers = await updater.GetDriversFromDirectoryAsync(
    "C:\\Drivers", 
    "*.inf"
);
```

#### Q12: Is the component thread-safe?

**A:** Yes, you can safely use the updater instance across multiple threads. Each update operation is independent and can run concurrently.

#### Q13: What .NET versions are supported?

**A:** GeneralUpdate.Drivelution targets .NET 8.0 and is compatible with:
- .NET 8.0+
- Supports AOT (Ahead-of-Time) compilation
- Supports trimming for smaller deployments



### Example

Complete example with error handling and logging:

```c#
using GeneralUpdate.Drivelution;
using GeneralUpdate.Drivelution.Abstractions.Models;
using GeneralUpdate.Drivelution.Abstractions.Configuration;
using Serilog;

public class DriverUpdateExample
{
    public static async Task Main(string[] args)
    {
        // Configure logging
        var logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File("logs/driver-update.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        // Configure options
        var options = new DrivelutionOptions
        {
            LogLevel = "Info",
            DefaultBackupPath = "C:\\DriverBackups",
            AutoCleanupBackups = true,
            BackupsToKeep = 5
        };

        try
        {
            // Create updater
            var updater = GeneralDrivelution.Create(logger, options);

            // Get platform info
            var platformInfo = GeneralDrivelution.GetPlatformInfo();
            Console.WriteLine($"Platform: {platformInfo}");

            // Define driver information
            var driverInfo = new DriverInfo
            {
                Name = "Sample Device Driver",
                Version = "2.0.0",
                FilePath = "C:\\Drivers\\sample.inf",
                TargetOS = "Windows",
                Architecture = "x64",
                Hash = CalculateFileHash("C:\\Drivers\\sample.inf"),
                HashAlgorithm = "SHA256",
                HardwareId = "PCI\\VEN_1234&DEV_5678",
                Description = "Sample device driver for demonstration"
            };

            // Validate driver first
            Console.WriteLine("Validating driver...");
            var isValid = await updater.ValidateAsync(driverInfo);
            if (!isValid)
            {
                Console.WriteLine("Driver validation failed!");
                return;
            }

            // Define update strategy
            var strategy = new UpdateStrategy
            {
                RequireBackup = true,
                RetryCount = 3,
                RetryIntervalSeconds = 5,
                RestartMode = RestartMode.Prompt,
                TimeoutSeconds = 600
            };

            // Execute update
            Console.WriteLine("Starting driver update...");
            var result = await updater.UpdateAsync(driverInfo, strategy);

            // Handle result
            if (result.Success)
            {
                Console.WriteLine($"✓ Driver updated successfully in {result.DurationMs}ms");
                Console.WriteLine($"✓ Backup: {result.BackupPath}");
                
                foreach (var log in result.StepLogs)
                {
                    Console.WriteLine($"  - {log}");
                }
            }
            else
            {
                Console.WriteLine($"✗ Update failed: {result.Error?.Message}");
                Console.WriteLine($"  Error Code: {result.Error?.Code}");
                
                if (result.RolledBack)
                {
                    Console.WriteLine("✓ System rolled back successfully");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
            logger.Error(ex, "Driver update failed");
        }
    }

    private static string CalculateFileHash(string filePath)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        using var stream = File.OpenRead(filePath);
        var hash = sha256.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
```



### Annotations

GeneralDrivelution provides a complete driver update solution with the following key features:

#### Security Features
- ✓ Driver signature validation (platform-specific)
- ✓ File hash integrity checking
- ✓ Hardware ID matching
- ✓ Publisher trust validation
- ✓ Permission checking before operations

#### Reliability Features
- ✓ Automatic backup before updates
- ✓ Rollback on failure
- ✓ Retry mechanism with configurable attempts
- ✓ Timeout protection
- ✓ Comprehensive error handling and logging

#### Platform Features
- ✓ Windows: INF driver installation, PnPUtil integration
- ✓ Linux: Kernel module management, modprobe integration
- ✓ MacOS: KEXT installation, System Integrity Protection awareness

#### Developer Features
- ✓ Simple and intuitive API
- ✓ Async/await support
- ✓ Comprehensive logging with Serilog
- ✓ Detailed error information
- ✓ Progress tracking and step logs



### Applicable to

| Product        | Versions      |
| -------------- | ------------- |
| .NET           | 8, 9          |
| .NET Standard  | N/A           |
| .NET Core      | N/A           |
| .NET Framework | N/A           |

**Platform Support:**

| Platform       | Support Level |
| -------------- | ------------- |
| Windows        | ✓ Full        |
| Linux          | ✓ Full        |
| MacOS          | ✓ Full        |

**Note:** Requires elevated privileges (Administrator/root) on all platforms for driver operations.
