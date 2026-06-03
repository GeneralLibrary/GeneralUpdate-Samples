---
sidebar_position: 8
---

# GeneralUpdate.Drivelution

**Namespace:** `GeneralUpdate.Drivelution` | **Main Entry Point:** `GeneralDrivelution` (static class) | **NuGet Package:** `GeneralUpdate.Drivelution`

## 1. Component Overview

### 1.1 Introduction

**GeneralUpdate.Drivelution** is a cross-platform driver update component. It structures the error-prone steps of driver updates into a unified pipeline: platform detection → permission check → file validation (hash/signature/compatibility) → backup → install → verification → rollback on failure, invoking native system tools on Windows, Linux, and macOS respectively.

Driver updates differ fundamentally from application file replacement. Drivers affect the kernel, device nodes, system extensions, or driver repositories, requiring special attention to administrator privileges, signature trust, target OS and CPU architecture, install command return values, reboot requirements, and failure recovery paths.

**Core Capabilities:**

| Capability | Description |
| --- | --- |
| Cross-Platform Abstraction | `GeneralDrivelution.Create()` auto-detects platform and creates the appropriate implementation |
| Standard Update Pipeline | Permission check → file validation → backup → install → verify → rollback |
| File Validation | File existence, SHA256/MD5 hash, Authenticode/GPG/codesign signature, OS/arch compatibility |
| Backup & Rollback | `RequireBackup` defaults to `true`; auto-rollback on failure with `RollbackAsync` for explicit recovery |
| Windows Installation | Via `pnputil.exe /add-driver /install` for INF driver packages |
| Linux Installation | Supports `.ko` (`insmod`/`modprobe`), `.deb` (`dpkg -i`), `.rpm` (`rpm -ivh`/`dnf install`) |
| macOS Installation | Supports `.kext` (`kextload`), `.dext` (SystemExtensions), `.pkg` (`installer`) |
| Batch Updates | `BatchUpdateAsync` with `BatchMode.Sequential` and `BatchMode.Parallel` |
| Progress Reporting | Via `IProgress<UpdateProgress>` reporting step-level progress, status, and messages |
| Trace Logging | `GeneralTracer` with console + daily-rotating file output |

**Business Problems Solved:**
- Hardware vendor clients need to deliver driver updates alongside apps, but OS-specific driver installation methods vary widely
- Driver installation requires administrator privileges, signature verification, architecture compatibility, and other safeguards
- Failed driver installations need reliable rollback to avoid leaving devices unusable

**Use Cases:**
- Hardware vendor clients: network cards, capture cards, USB devices, virtual device drivers
- Enterprise/industrial sites: batch scan driver packages and update by manifest
- Installer/maintenance tools: unified handling of Windows/Linux/macOS driver differences

### 1.2 Environment & Dependencies

| Item | Description |
| --- | --- |
| **Version** | `10.5.0-beta.2` |
| **Target Frameworks** | `net8.0` / `net10.0` (multi-target) |
| **Dependencies** | `Microsoft.Extensions.DependencyInjection`, `Microsoft.Extensions.Logging.Abstractions`, `Microsoft.Extensions.Options` |
| **Compatibility** | Windows (full, admin required) / Linux (root/sudo) / macOS (SIP & system extension policies apply) |

---

## 2. Feature List

| Feature | Description | Type | Required | Notes |
| --- | --- | --- | --- | --- |
| Single Driver Quick Update | `QuickUpdateAsync` with default safe strategy | Core | Optional | Custom strategy recommended for production |
| Custom Strategy Update | `UpdateAsync` with `UpdateStrategy` and `DrivelutionOptions` | Core | Recommended | Control backup, retry, timeout, restart |
| Driver Validation | File existence, SHA256 hash, signature, OS/arch compatibility | Core | Optional | `ValidateAsync`; can skip via strategy flags |
| Driver Backup | Backup driver files before update | Core | Automatic | `RequireBackup` defaults to `true` |
| Driver Rollback | Restore driver from backup path | Core | Optional | `RollbackAsync(backupPath)` |
| Directory Scanning | Scan and parse driver info (`.inf`/`.ko`/`.kext` etc.) | Core | Optional | `GetDriversFromDirectoryAsync` |
| Batch Updates | Batch update by manifest, sequential or parallel | Extended | Optional | `BatchUpdateAsync` |
| Platform Info Query | Get current OS/arch/version/support status | Core | Optional | `GetPlatformInfo()` |
| Restart Behavior Control | `UpdateStrategy.RestartMode` sets restart intent | Extended | Optional | Pipeline does not auto-restart; use `RestartHelper` |
| DI Registration | `AddDrivelution` extension registers platform services | Extended | Optional | Supports Generic Host / ASP.NET Core |
| Trace Logging | `GeneralTracer` runtime diagnostic logs | Extended | Optional | Enabled by default, can be disabled |

---

## 3. API Configuration Reference

### 3.1 Configuration Properties (Props)

**DriverInfo:**

| Field | Type | Default | Required | Values | Description |
| --- | --- | --- | --- | --- | --- |
| `Name` | `string` | `""` | Yes | — | Driver name |
| `Version` | `string` | `""` | Recommended | SemVer format | Driver version |
| `FilePath` | `string` | `""` | Yes | Valid file path | Win: `.inf`; Linux: `.ko`/`.deb`/`.rpm`; macOS: `.kext`/`.dext`/`.pkg` |
| `TargetOS` | `string` | `""` | Optional | `"Windows"`/`"Linux"`/`"MacOS"` | No restriction when empty |
| `Architecture` | `string` | `""` | Optional | `"x64"`/`"amd64"`/`"x86"`/`"arm64"`/`"arm"` | Common alias normalization; no restriction when empty |
| `HardwareId` | `string` | `""` | Optional | — | Hardware ID or module alias |
| `Hash` | `string` | `""` | Optional | SHA256/MD5 value | Hash verified when non-empty and `SkipHashValidation = false` |
| `HashAlgorithm` | `string` | `"SHA256"` | Optional | `"SHA256"` / `"MD5"` | Hash algorithm |
| `TrustedPublishers` | `List<string>` | `new()` | Optional | — | Signature verified when non-empty and `SkipSignatureValidation = false` |
| `Description` | `string` | `""` | Optional | — | Driver description |
| `ReleaseDate` | `DateTime` | — | Optional | — | Release date |
| `Metadata` | `Dictionary<string, string>` | `new()` | Optional | — | Extended metadata |

**UpdateStrategy:**

| Field | Type | Default | Required | Values | Description |
| --- | --- | --- | --- | --- | --- |
| `RequireBackup` | `bool` | `true` | Optional | `true` / `false` | Execute backup step |
| `BackupPath` | `string` | `""` | Recommended | Valid directory path | Backup root path |
| `RestartMode` | `RestartMode` | `Prompt` | Optional | `None`, `Prompt`, `Delayed`, `Immediate` | Pipeline does not auto-restart system |
| `SkipHashValidation` | `bool` | `false` | Optional | `true` / `false` | Skip hash check (debug only) |
| `SkipSignatureValidation` | `bool` | `false` | Optional | `true` / `false` | Skip signature check (debug only) |
| `TimeoutSeconds` | `int` | `300` | Optional | Positive integer | Single update timeout |
| `RetryCount` | `int` | `3` | Optional | Positive integer | Retry count (actual retry from `DrivelutionOptions`) |
| `Mode` | `UpdateMode` | `Full` | Optional | `Full`, `Incremental` | Update mode |
| `ForceUpdate` | `bool` | `false` | Optional | `true` / `false` | Force update |
| `Priority` | `int` | `0` | Optional | — | Priority |

**DrivelutionOptions:**

| Field | Type | Default | Required | Description |
| --- | --- | --- | --- | --- |
| `DefaultBackupPath` | `string` | `"./DriverBackups"` | Optional | Default backup path |
| `DefaultRetryCount` | `int` | `3` | Optional | Default retry count |
| `DefaultRetryIntervalSeconds` | `int` | `5` | Optional | Default retry interval |
| `DefaultTimeoutSeconds` | `int` | `300` | Optional | Default timeout |
| `DebugModeSkipSignature` | `bool` | `false` | Optional | Debug: skip signature |
| `DebugModeSkipHash` | `bool` | `false` | Optional | Debug: skip hash |
| `ForceTerminateOnPermissionFailure` | `bool` | `true` | Optional | Terminate on permission failure |
| `AutoCleanupBackups` | `bool` | `true` | Optional | Auto-clean old backups |
| `BackupsToKeep` | `int` | `5` | Optional | Number of backups to keep |
| `UseExponentialBackoff` | `bool` | `false` | Optional | Use exponential backoff |

### 3.2 Instance Methods

**GeneralDrivelution (static):**

| Method | Parameters | Returns | Use Case | Notes |
| --- | --- | --- | --- | --- |
| `Create(DrivelutionOptions?)` | `options` | `IGeneralDrivelution` | Create platform driver updater | Auto-detects platform |
| `Create(IServiceProvider)` | `serviceProvider` | `IGeneralDrivelution` | From DI container | Falls back to auto-detect if not registered |
| `QuickUpdateAsync(DriverInfo, UpdateStrategy?, IProgress<UpdateProgress>?, CancellationToken)` | `driverInfo`, `strategy?`, `progress?`, `ct` | `Task<UpdateResult>` | Quick single driver update | Uses safe defaults |
| `ValidateAsync(DriverInfo, CancellationToken)` | `driverInfo`, `ct` | `Task<bool>` | Standalone validation | — |
| `GetPlatformInfo()` | None | `PlatformInfo` | Query platform info | OS/arch/version/support status |
| `GetDriversFromDirectoryAsync(string, string?, CancellationToken)` | `path`, `searchPattern?`, `ct` | `Task<List<DriverInfo>>` | Scan directory for drivers | Default pattern varies by platform |
| `BatchUpdateAsync(IEnumerable<DriverInfo>, UpdateStrategy, BatchMode, IProgress<UpdateProgress>?, CancellationToken)` | `drivers`, `strategy`, `mode`, `progress?`, `ct` | `Task<BatchUpdateResult>` | Batch update multiple drivers | Parallel may contend for system resources |

**IGeneralDrivelution:**

| Method | Parameters | Returns | Use Case |
| --- | --- | --- | --- |
| `UpdateAsync(...)` | Same as `QuickUpdateAsync` | `Task<UpdateResult>` | Full update pipeline |
| `ValidateAsync(...)` | — | `Task<bool>` | Standalone validation |
| `BackupAsync(DriverInfo, string, CancellationToken)` | `driverInfo`, `backupPath`, `ct` | `Task<bool>` | Standalone backup |
| `RollbackAsync(string, CancellationToken)` | `backupPath`, `ct` | `Task<bool>` | Restore from backup |
| `GetDriversFromDirectoryAsync(...)` | — | `Task<List<DriverInfo>>` | Scan directory |
| `BatchUpdateAsync(...)` | — | `Task<BatchUpdateResult>` | Batch update |

### 3.3 Callback Events

Drivelution reports progress via `IProgress<UpdateProgress>` rather than a dedicated event system.

| Progress Field | Type | Description |
| --- | --- | --- |
| `CurrentStatus` | `UpdateStatus` | Current status (`Validating`/`BackingUp`/`Updating`/`Verifying`/`Succeeded`/`Failed`/`RolledBack`) |
| `StepName` | `string` | Current step name |
| `Percentage` | `int` | Progress percentage (0-100) |
| `Message` | `string` | Progress message |
| `StepIndex` | `int` | Current step index |
| `TotalSteps` | `int` | Total step count |

---

## 4. Advanced Examples

### 4.1 Extension Points Overview

| Extension Interface | Description |
| --- | --- |
| `IGeneralDrivelution` | Fully replace driver updater behavior |
| `IDriverValidator` | Custom file validation logic |
| `IDriverBackup` | Custom backup/restore strategy |
| `ICommandRunner` | Custom system command executor |
| `INetworkDownloader` | Reserved interface (network download) |
| `BaseDriverUpdater` | Abstract base class for new platform implementations |

### 4.2 Examples by Scenario

#### Scenario 1: Custom Strategy + Rollback

**Description:** Production driver update with all security checks enabled; explicit rollback on failure.

```csharp
using GeneralUpdate.Drivelution;
using GeneralUpdate.Drivelution.Abstractions.Configuration;
using GeneralUpdate.Drivelution.Abstractions.Models;

var updater = GeneralDrivelution.Create(new DrivelutionOptions
{
    DefaultBackupPath = @"C:\DriverBackups",
    DefaultRetryCount = 3,
    DefaultTimeoutSeconds = 600,
    UseExponentialBackoff = true
});

var strategy = new UpdateStrategy
{
    RequireBackup = true,
    BackupPath = @"C:\DriverBackups\graphics",
    TimeoutSeconds = 600,
    RestartMode = RestartMode.Prompt
};

var driver = new DriverInfo
{
    Name = "Graphics Driver",
    Version = "2.1.0",
    FilePath = @"C:\Drivers\graphics.inf",
    TargetOS = "Windows",
    Architecture = "x64",
    Hash = "expected-sha256...",
    TrustedPublishers = { "Contoso Hardware Inc." }
};

var progress = new Progress<UpdateProgress>(p =>
    Console.WriteLine($"[{p.StepName}] {p.Percentage}%: {p.Message}"));

var result = await updater.UpdateAsync(driver, strategy, progress);

if (!result.Success && result.BackupPath is not null)
    await updater.RollbackAsync(result.BackupPath);

if (result.Success && RestartHelper.IsRestartRequired(strategy.RestartMode))
    await RestartHelper.HandleRestartAsync(strategy.RestartMode, 60, "Driver updated. Restart now?");
```

#### Scenario 2: DI Container Integration

**Description:** Register driver update services via DI in ASP.NET Core / Generic Host apps.

```csharp
using GeneralUpdate.Drivelution.Core;

builder.Services.AddDrivelution(options =>
{
    options.DefaultBackupPath = "./DriverBackups";
    options.DefaultTimeoutSeconds = 600;
    options.DefaultRetryCount = 3;
});

// Use in controller or service
app.MapPost("/drivers/update", async (DriverInfo driver, IGeneralDrivelution updater) =>
{
    var result = await updater.UpdateAsync(driver, new UpdateStrategy());
    return result.Success ? Results.Ok(result) : Results.BadRequest(result.Error);
});
```

#### Scenario 3: Batch Parallel Scanning + Sequential Installation

**Description:** Large projects scan and validate all drivers in parallel, then install core drivers sequentially by risk group.

```csharp
var drivers = await GeneralDrivelution.GetDriversFromDirectoryAsync(@"C:\DriverPackages");

var validDrivers = new List<DriverInfo>();
foreach (var d in drivers)
    if (await GeneralDrivelution.ValidateAsync(d))
        validDrivers.Add(d);

var coreDrivers = validDrivers.Where(d => IsCoreDriver(d)).ToList();
var optionalDrivers = validDrivers.Except(coreDrivers).ToList();

// Core drivers: sequential install
if (coreDrivers.Any())
    await GeneralDrivelution.BatchUpdateAsync(coreDrivers, new UpdateStrategy { RequireBackup = true }, BatchMode.Sequential);

// Optional drivers: parallel install
if (optionalDrivers.Any())
    await GeneralDrivelution.BatchUpdateAsync(optionalDrivers, new UpdateStrategy { RequireBackup = true }, BatchMode.Parallel);
```

---

## 5. Basic Usage Examples

### 5.1 Quick Start (Minimal Demo)

```csharp
using GeneralUpdate.Drivelution;
using GeneralUpdate.Drivelution.Abstractions.Models;

var driver = new DriverInfo
{
    Name = "MyDevice Driver",
    Version = "1.2.0",
    FilePath = @"C:\Drivers\mydevice.inf",
    TargetOS = "Windows",
    Architecture = "x64",
    Hash = "driver-file-sha256",
    HashAlgorithm = "SHA256",
    TrustedPublishers = { "Contoso Hardware" }
};

var result = await GeneralDrivelution.QuickUpdateAsync(driver);

if (result.Success)
    Console.WriteLine($"Driver updated successfully in {result.DurationMs}ms.");
else
    Console.WriteLine($"Update failed: {result.Error?.Message}");
```

### 5.2 Basic Parameter Combination

```csharp
var platform = GeneralDrivelution.GetPlatformInfo();
Console.WriteLine($"OS: {platform.OperatingSystem}, Arch: {platform.Architecture}");

var drivers = await GeneralDrivelution.GetDriversFromDirectoryAsync(@"C:\Drivers");
Console.WriteLine($"Found {drivers.Count} driver(s).");

var updater = GeneralDrivelution.Create(new DrivelutionOptions
{
    DefaultBackupPath = @"C:\DriverBackups",
    DefaultRetryCount = 3,
    DefaultTimeoutSeconds = 600
});

var strategy = new UpdateStrategy
{
    RequireBackup = true,
    BackupPath = @"C:\DriverBackups\mydevice",
    TimeoutSeconds = 300
};

var result = await updater.UpdateAsync(drivers[0], strategy,
    new Progress<UpdateProgress>(p => Console.WriteLine($"{p.StepName}: {p.Percentage}%")));
```

### 5.3 Production-Ready Example

See the [Chinese documentation](GeneralUpdate.Drivelution.md#53-真实业务落地示例) for a full workflow covering scan, validate, install, rollback, and restart.

---

## 6. Global Configuration

### Platform Quick Reference

| Platform | Driver Format | Install Command | Signature Verification | Permission Required |
| --- | --- | --- | --- | --- |
| Windows | `.inf` | `pnputil.exe /add-driver /install` | Authenticode | Administrator |
| Linux | `.ko` / `.deb` / `.rpm` | `insmod`/`modprobe` / `dpkg -i` / `rpm -ivh` | GPG (`.sig`/`.asc`) | root/sudo |
| macOS | `.kext` / `.dext` / `.pkg` | `kextload` / SystemExtensions / `installer` | `codesign -v` | root (SIP & user approval) |

### Logging Configuration

```csharp
GeneralTracer.SetTracingEnabled(false);  // Disable for performance
GeneralTracer.SetTracingEnabled(true);   // Re-enable for troubleshooting
```

---

## Related Resources

- [Driver Update Sample](https://github.com/GeneralLibrary/GeneralUpdate-Samples/tree/main/src/Hub/Samples/ImDiskQuickInstallSample.cs)
- [GeneralUpdate Repository](https://github.com/GeneralLibrary/GeneralUpdate)
- [Driver Guide](../guide/Driver.md)
