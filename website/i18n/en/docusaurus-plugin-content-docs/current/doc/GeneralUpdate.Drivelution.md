---
sidebar_position: 12
---

### Definition

Namespace: `GeneralUpdate.Drivelution`

Assembly: `GeneralUpdate.Drivelution.dll`

```c#
public static class GeneralDrivelution
```

`GeneralUpdate.Drivelution` is a cross-platform component for operating-system driver updates. It turns the risky parts of driver servicing into a unified pipeline: platform detection, permission checks, file validation, backup, installation, post-install verification, and rollback entry points. Windows, Linux, and macOS implementations call the native tools that each platform expects for driver installation.

Driver updates are different from normal application file updates. Application updates usually download, extract, replace files, and restart a process. Driver updates can affect the kernel, device nodes, system extensions, or the system driver store, so they also need administrator/root permissions, trusted signatures, target OS and CPU architecture checks, command-result handling, restart planning, and a recovery path. Drivelution covers operating-system driver updates only; it does not handle device-internal flashing workflows.

### Capability overview

| Capability | Current implementation |
| --- | --- |
| Platform adaptation | `GeneralDrivelution.Create()` automatically chooses the Windows, Linux, or macOS implementation. |
| Standard pipeline | Windows/Linux run a permission step before `Validate -> Backup -> Install -> Verify`. macOS currently includes a `CheckSudo` step and then runs system commands; actual installation still depends on system permissions. |
| Validation | File existence, optional hash validation, optional signature validation, target OS and architecture compatibility. |
| Backup | `UpdateStrategy.RequireBackup` defaults to `true`; the backup root comes from `UpdateStrategy.BackupPath`. |
| Installation | Windows uses `pnputil.exe`; Linux uses `insmod`/`modprobe`, `dpkg`, `rpm`/`dnf`; macOS uses system tools such as `kextload` and `installer`. |
| Rollback | `RollbackAsync(backupPath)` is exposed. Windows reinstalls backed-up `.inf` files, Linux restores `.ko` modules, and macOS restores `.kext` bundles. |
| Batch/parallel work | `BatchUpdateAsync` supports `BatchMode.Sequential` and `BatchMode.Parallel` for large driver sets. |
| Logging | `GeneralTracer` writes to console and `Logs\generalupdate-trace yyyy-MM-dd.log` by default, and can be disabled with `SetTracingEnabled(false)`. |

### When to use Drivelution

Use Drivelution when:

- A hardware-vendor client needs to ship NIC, capture-card, USB, virtual-device, or similar drivers.
- Enterprise or industrial environments need to scan driver packages and update them from a manifest-like list.
- An installer, maintenance tool, or device-management service needs one API across Windows/Linux/macOS.
- You need hash, signature, OS, and architecture validation before installation, plus a backup path for recovery.

Do not use Drivelution as a generic application file updater. If you only need to update your own exe, dll, assets, or plugins, use the `GeneralUpdate.Core` application update flow instead.

### Installation

```bash
dotnet add package GeneralUpdate.Drivelution
```

Or add the package reference:

```xml
<PackageReference Include="GeneralUpdate.Drivelution" Version="*" />
```

### Quick start: update one driver

```c#
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
{
    Console.WriteLine($"Driver updated. Duration={result.DurationMs}ms");
}
else
{
    Console.WriteLine($"Driver update failed: {result.Error?.Message}");
    Console.WriteLine(string.Join(Environment.NewLine, result.StepLogs));
}
```

`QuickUpdateAsync` creates the updater for the current platform and uses safe defaults: backup is required, failures can be retried 3 times, and the retry interval is 5 seconds. In production, pass an explicit `UpdateStrategy`, especially for backup location, timeout, and restart behavior.

### Custom strategy

```c#
using GeneralUpdate.Drivelution;
using GeneralUpdate.Drivelution.Abstractions.Configuration;
using GeneralUpdate.Drivelution.Abstractions.Models;

var options = new DrivelutionOptions
{
    DefaultBackupPath = @"C:\DriverBackups",
    DefaultRetryCount = 3,
    DefaultRetryIntervalSeconds = 5,
    DefaultTimeoutSeconds = 600,
    UseExponentialBackoff = true,
    ForceTerminateOnPermissionFailure = true
};

var updater = GeneralDrivelution.Create(options);

var strategy = new UpdateStrategy
{
    RequireBackup = true,
    BackupPath = @"C:\DriverBackups\graphics",
    RetryCount = 3,
    RetryIntervalSeconds = 5,
    TimeoutSeconds = 600,
    RestartMode = RestartMode.Prompt,
    SkipHashValidation = false,
    SkipSignatureValidation = false
};

var progress = new Progress<UpdateProgress>(p =>
{
    Console.WriteLine($"{p.Percentage}% {p.StepName}: {p.Message}");
});

var result = await updater.UpdateAsync(driver, strategy, progress);

if (!result.Success && result.BackupPath is not null)
{
    await updater.RollbackAsync(result.BackupPath);
}
```

> Note: `UpdateStrategy.RetryCount` and `RetryIntervalSeconds` exist on the strategy model. The current pipeline retry policy is created from `DrivelutionOptions.DefaultRetryCount`, `DefaultRetryIntervalSeconds`, and `UseExponentialBackoff`. Configure `DrivelutionOptions` when you need to control retry behavior consistently.

### Dependency injection

Generic Host, ASP.NET Core, and custom containers can register the current-platform implementation:

```c#
using GeneralUpdate.Drivelution.Core;

builder.Services.AddDrivelution(options =>
{
    options.DefaultBackupPath = "./DriverBackups";
    options.DefaultTimeoutSeconds = 600;
});

var updater = GeneralDrivelution.Create(builder.Services.BuildServiceProvider());
```

`AddDrivelution` registers `ICommandRunner`, the platform `IDriverValidator`, `IDriverBackup`, and `IGeneralDrivelution`.

### API overview

#### `GeneralDrivelution`

| Method | Description |
| --- | --- |
| `Create(DrivelutionOptions? options = null)` | Detects the current OS and creates the platform driver updater. |
| `Create(IServiceProvider serviceProvider)` | Resolves `IGeneralDrivelution` from DI; falls back to automatic platform creation if not registered. |
| `QuickUpdateAsync(driverInfo, strategy?, progress?, token?)` | Updates one driver with default or custom strategy. |
| `ValidateAsync(driverInfo, token?)` | Validates a driver with the current platform validator. |
| `GetPlatformInfo()` | Returns platform, OS, architecture, system version, and support status. |
| `GetDriversFromDirectoryAsync(path, pattern?, token?)` | Scans a directory and parses driver information. |
| `BatchUpdateAsync(drivers, strategy, mode, progress?, token?)` | Updates multiple drivers sequentially or in parallel. |

#### `IGeneralDrivelution`

| Method | Description |
| --- | --- |
| `UpdateAsync(driverInfo, strategy, progress?, token?)` | Runs the full update pipeline. |
| `ValidateAsync(driverInfo, token?)` | Validates a driver only. |
| `BackupAsync(driverInfo, backupPath, token?)` | Backs up the driver file. |
| `RollbackAsync(backupPath, token?)` | Attempts platform-specific recovery from a backup. |
| `GetDriversFromDirectoryAsync(path, pattern?, token?)` | Scans a directory. |
| `BatchUpdateAsync(drivers, strategy, mode, progress?, token?)` | Processes multiple drivers. |

### Data models

#### `DriverInfo`

| Property | Description |
| --- | --- |
| `Name` | Driver name. |
| `Version` | Driver version. Directory scanning tries to read it from INF, `modinfo`, package metadata, or plist data, and falls back to `1.0.0`. |
| `FilePath` | Driver file path. Windows usually uses `.inf`, Linux uses `.ko`/`.deb`/`.rpm`, and macOS uses `.kext`/`.dext`/`.pkg`. |
| `TargetOS` | Target operating system. Empty means no OS restriction; otherwise it must contain the current OS name, such as `Windows`, `Linux`, or `MacOS`. |
| `Architecture` | Target architecture. Common aliases are normalized: `x64/amd64/x86_64`, `x86/i386/i686`, `arm64/aarch64`, `arm/armv7`. |
| `HardwareId` | Hardware ID or module alias. Windows parses INF metadata; Linux can read `modinfo alias`. |
| `Hash` / `HashAlgorithm` | Integrity validation. `SHA256` and compatibility `MD5` are supported. |
| `TrustedPublishers` | Trusted publisher list. Signature validation runs only when this list is not empty and signature validation is not skipped. |
| `Description`, `ReleaseDate`, `Metadata` | Display and extension data. |

#### `UpdateStrategy`

| Property | Description |
| --- | --- |
| `RequireBackup` | Whether to run the backup step. Defaults to `true`. |
| `BackupPath` | Backup root. The pipeline creates `backup_{Name}_{yyyyMMddHHmmss}` under this path. |
| `RestartMode` | Restart intent: `None`, `Prompt`, `Delayed`, or `Immediate`. The current update pipeline does not restart the system automatically; call `RestartHelper.HandleRestartAsync(...)` after success if needed. |
| `SkipHashValidation` | Skips hash validation. Recommended only for debugging or controlled environments. |
| `SkipSignatureValidation` | Skips signature validation. Recommended only for debugging or controlled environments. |
| `TimeoutSeconds` | Per-update timeout; values less than or equal to 0 use `DrivelutionOptions.DefaultTimeoutSeconds`. |
| `Mode`, `ForceUpdate`, `Priority` | Reserved strategy fields that can be used by upper-level scheduling or UI logic. |

#### `UpdateResult`

| Property | Description |
| --- | --- |
| `Success` / `Status` | Success flag and status: `NotStarted`, `Validating`, `BackingUp`, `Updating`, `Verifying`, `Succeeded`, `Failed`, `RolledBack`. |
| `Error` | Error type, code, message, details, and stack trace. |
| `BackupPath` | Backup path for this update. |
| `RolledBack` | Whether the pipeline entered the rollback path after failure. If you need strong recovery, explicitly call `RollbackAsync(BackupPath)`. |
| `StepLogs` | Step-by-step logs suitable for an installer result page or diagnostics upload. |
| `DurationMs` | Total duration. |

### Update pipeline

`BaseDriverUpdater.UpdateAsync` runs the current platform steps in order:

1. Platform permission step: `CheckPermissions` on Windows, `CheckSudo` on Linux, and `CheckSudo` on macOS.
2. `Validate`: file existence, hash, signature, and compatibility.
3. `Backup`: runs when `RequireBackup == true`.
4. `Install`: calls the platform installation command.
5. `Verify`: checks installation result. Windows runs `pnputil.exe /enum-drivers`; inconclusive verification logs a warning but does not fail the whole update.

Each step reports `StepName`, `Percentage`, `Message`, `StepIndex`, and `TotalSteps` through `IProgress<UpdateProgress>`. When a step fails or an exception occurs, `UpdateResult.Error` is mapped to displayable error information. If a backup path exists, the pipeline enters the rollback path and records it in `StepLogs`.

### Validation strategy

Validation is conditional:

- File existence is always checked.
- If `DriverInfo.Hash` is not empty and `SkipHashValidation == false`, the file hash is computed and compared with the expected value.
- If `DriverInfo.TrustedPublishers.Count > 0` and `SkipSignatureValidation == false`, signature validation runs.
- Compatibility is always checked; empty `TargetOS` or `Architecture` means that dimension is unrestricted.

Platform signature behavior:

| Platform | Signature validation |
| --- | --- |
| Windows | Uses Authenticode-related logic and checks trusted publishers. |
| Linux | Looks for sibling `.sig` or `.asc` files and validates GPG signatures; unsigned files are accepted when no trusted publisher is configured. |
| macOS | Runs `codesign -v`, then `codesign -v --deep` if needed; when trusted publishers are provided, it matches `codesign -dvv` output. |

### Platform differences

#### Windows

The Windows implementation targets INF driver packages:

- Default scan pattern: `*.inf`.
- Permission: the process must run as administrator, otherwise `CheckPermissions` fails.
- Installation: `pnputil.exe /add-driver <path> /install`.
- Verification: `pnputil.exe /enum-drivers`; inconclusive verification logs a warning but does not block the update.
- Metadata: parses `DriverVer`, `DriverDesc`, and `HardwareId`, and computes SHA256.
- Rollback: `RollbackAsync` scans backed-up `.inf` files and reinstalls them with PnPUtil.

#### Linux

The Linux implementation supports kernel modules and distro packages:

- Default scanning includes `.ko`; when no search pattern is specified it also scans `.deb` and `.rpm`.
- Permission: sudo/root is required for normal driver installation.
- `.ko` installation: tries `insmod <module.ko>` first, then falls back to `modprobe <moduleName>`.
- `.deb` installation: `dpkg -i <package.deb>`.
- `.rpm` installation: tries `rpm -ivh <package.rpm>`, then falls back to `dnf install -y <package.rpm>`.
- Metadata: `.ko` uses `modinfo`; `.deb` uses `dpkg-deb -I`; `.rpm` uses `rpm -qip`.
- Rollback: currently focuses on `.ko`, tries `modprobe -r <moduleName>`, then loads the backed-up module with `insmod <backup.ko>`.

#### macOS

The macOS implementation targets kernel extensions, DriverKit extensions, and packages:

- Default scanning includes `.kext`, `.dext`, and `.pkg`.
- `.kext` installation: copies to `/Library/Extensions/`, sets `root:wheel` and `755`, runs `kextload`, then `kextcache -i /`.
- `.dext` installation: copies to `/Library/SystemExtensions/`; DriverKit extensions usually still require user approval in system security settings.
- `.pkg` installation: `/usr/sbin/installer -pkg <pkg> -target /`.
- Signature: uses `codesign`.
- Limitations: newer macOS versions impose SIP, user approval, and system-extension policies. A command succeeding does not necessarily mean the user approval flow is complete.
- Rollback: currently focuses on `.kext`, copies it back to `/Library/Extensions/`, and attempts `kextload`.

### Batch and parallel updates

Batch mode is useful when a large project splits driver packages into a list:

```c#
var drivers = await GeneralDrivelution.GetDriversFromDirectoryAsync(@"C:\Drivers");

var batch = await GeneralDrivelution.BatchUpdateAsync(
    drivers,
    strategy,
    BatchMode.Parallel,
    progress);

Console.WriteLine(batch);
```

`BatchMode.Sequential` processes drivers one by one and is safer for core drivers, dependent drivers, or risk-sensitive installs. `BatchMode.Parallel` uses `Task.WhenAll` for multiple drivers and can improve throughput for independent scanning, validation, and installation work, but underlying system tools may still contend for the driver store, package-manager locks, or kernel module resources. For large projects, parallelize scanning and validation first, then group or serialize high-risk installation stages.

### Restart behavior

`UpdateStrategy.RestartMode` expresses the restart intent after driver installation:

| Value | Meaning |
| --- | --- |
| `None` | No restart required. |
| `Prompt` | The app should prompt the user. Current `RestartHelper.PromptUserForRestart` writes a prompt and returns `false`, so GUI apps should handle the dialog themselves. |
| `Delayed` | Waits, then calls the platform restart command. |
| `Immediate` | Calls the platform restart command immediately. |

`UpdateAsync` does not call `RestartHelper` automatically, so it will not restart the system right after installation. Decide in your application layer based on driver type and result:

```c#
if (result.Success && RestartHelper.IsRestartRequired(strategy.RestartMode))
{
    await RestartHelper.HandleRestartAsync(
        strategy.RestartMode,
        delaySeconds: 60,
        message: "Driver update completed. Restart now?");
}
```

### Logging and performance switch

Drivelution uses `GeneralTracer` for internal diagnostics:

- Enabled by default.
- Console output through `TextWriterTraceListener(Console.Out)`.
- File output under the application base directory: `Logs\generalupdate-trace yyyy-MM-dd.log`, rotated by date.
- Windows debug output through `WindowsOutputDebugListener`.
- `DefaultTraceListener` is added when a debugger is attached.

Driver updates often involve external commands and elevated permissions, so logs are important for troubleshooting. However, `GeneralTracer` creates timestamps, stack-frame location data, and Trace Listener writes. In performance-sensitive scans, batch validation, or high-parallelism scenarios, disable it temporarily:

```c#
GeneralTracer.SetTracingEnabled(false);

// Run performance-sensitive scanning or batch validation.

GeneralTracer.SetTracingEnabled(true);
```

If you need to bridge logs into your own UI or logging stack, you can wrap the `DrivelutionLogger.LogMessage` event. The main update pipeline currently uses `GeneralTracer`.

### Recommended practices

| Scenario | Recommendation |
| --- | --- |
| Production updates | Keep `RequireBackup = true`, set an explicit `BackupPath`, and do not skip hash or signature validation. |
| First integration | Call `ValidateAsync` and `GetPlatformInfo()` first, then show target OS, architecture, version, and publisher in the UI. |
| Windows | Start the process as administrator and prefer vendor-signed INF packages. |
| Linux | Verify root/sudo, kernel version, and package-manager locks; install core modules sequentially. |
| macOS | Warn users that system-extension approval may be required; kext behavior is heavily affected by SIP and system policies. |
| Large driver sets | Scan and validate in parallel, group installation by risk, and keep `StepLogs` and `BackupPath` on failure. |
| High-performance flows | Temporarily disable `GeneralTracer` during bulk scans, then enable it again. |

### FAQ

#### Why did signature validation not run?

Signature validation runs only when `DriverInfo.TrustedPublishers` is not empty and `SkipSignatureValidation == false`. To require signature validation, provide trusted publishers and ensure the platform has the corresponding signature file or system signature metadata.

#### Why did the system not restart after setting `RestartMode`?

`RestartMode` is currently a strategy field. The update pipeline does not restart the system automatically. Call `RestartHelper.HandleRestartAsync(...)` after a successful `UpdateAsync`, or handle restart with your own GUI or service logic.

#### Is `BatchMode.Parallel` always faster?

No. Parallel work improves throughput for scanning, validation, and independent tasks, but installation calls system tools and may hit driver-store locks, package-manager locks, module dependencies, or restart requirements. For large projects, validate in parallel first, then control installation concurrency by group.

#### How should rollback be designed?

Keep the backup path before updating, and read `UpdateResult.BackupPath` and `StepLogs` on failure. If your business flow requires strong recovery, explicitly call `RollbackAsync(backupPath)` and tell the user that a restart or device replug may still be required.
