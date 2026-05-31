---
sidebar_position: 3
---

# GeneralUpdate.Bowl

## Overview

**GeneralUpdate.Bowl** is the startup guard that runs after an application update. It does not download, unpack, or replace update packages. Instead, it watches the target process when the new files have been installed and the main application is starting. If startup crashes, Bowl captures a dump, writes a failure report, exports diagnostics, and, in upgrade mode, restores the backup directory to the installation directory so users are not left on a broken version.

**Namespace:** `GeneralUpdate.Bowl`

**Assembly:** `GeneralUpdate.Bowl.dll`

**Current main entry:** `new Bowl().LaunchAsync(BowlContext context, CancellationToken ct = default)`

## Navigation

| Topic | Use it for |
| --- | --- |
| [Lifecycle placement](#lifecycle-placement) | Where Bowl belongs in the update flow |
| [Quick start](#quick-start) | Start monitoring with the current `BowlContext` API |
| [Crash detection and recovery flow](#crash-detection-and-recovery-flow) | What Bowl does after a crash |
| [BowlContext options](#bowlcontext-options) | Configuration fields and recommended values |
| [Output files](#output-files) | Where dumps, reports, diagnostics, and trace logs are written |
| [Crash callback](#crash-callback) | Upload reports or notify users on crash |
| [Trace logging switch](#trace-logging-switch) | Disable tracing for performance-sensitive scenarios |
| [Platform differences](#platform-differences) | Windows, Linux, and macOS behavior |
| [Recovery scenario](#recovery-scenario) | A practical failed-update rollback example |
| [Migrating from the old API](#migrating-from-the-old-api) | Move from `MonitorParameter` to `BowlContext` |

## Lifecycle placement

In the full GeneralUpdate flow, Bowl belongs **after file replacement and before users rely on the newly installed version**:

1. Core obtains update information, downloads packages, validates them, and applies updates.
2. The Core/Upgrade process prepares to start the main application.
3. Bowl starts as guard logic, attaches to the target process, and waits for startup exceptions.
4. If the main application starts normally, no dump is produced and Bowl returns the monitoring result.
5. If the main application crashes during startup, Bowl runs the failure pipeline and restores the backup when configured.

In the current Core code, the Windows `UpdateStrategy` starts the main application after the update and also starts the configured Bowl helper process. The Linux/macOS Core strategies do not provide the same automatic Bowl helper launch, so use your launcher, service script, or a separate process to call `LaunchAsync` explicitly on those platforms.

:::tip
Bowl is a post-update health check and rollback guard. It is not firmware recovery, OS restore, or an update package installer. Its scope is application startup crash diagnostics and application-directory backup restoration.
:::

## Quick start

### Install

```bash
dotnet add package GeneralUpdate.Bowl
```

### Upgrade-mode monitoring

Upgrade mode is intended for an upgrader or Bowl helper process. `BackupDirectory` points to the pre-update backup, `TargetPath` points to the current installation directory, and `ExtendedField` usually stores the version being monitored.

```csharp
using GeneralUpdate.Bowl;

var version = "2.0.0";
var installPath = AppDomain.CurrentDomain.BaseDirectory;

var context = new BowlContext
{
    ProcessNameOrId = "MyApp.exe",
    DumpFileName = $"{version}_fail.dmp",
    FailFileName = $"{version}_fail.json",
    TargetPath = installPath,
    FailDirectory = Path.Combine(installPath, "fail", version),
    BackupDirectory = Path.Combine(installPath, version),
    WorkModel = "Upgrade",
    ExtendedField = version,
    TimeoutMs = 30_000,
    DumpType = DumpType.Full,
    AutoRestore = true,
    OnCrash = (info, ct) =>
    {
        Console.WriteLine($"Crash dump: {info.DumpFilePath}");
        Console.WriteLine($"Crash report: {info.CrashReportPath}");
        return Task.CompletedTask;
    }
};

BowlResult result = await new Bowl().LaunchAsync(context);

if (result.DumpCaptured && result.Restored)
{
    Console.WriteLine("The upgraded version crashed and the backup was restored.");
}
```

### Standalone monitoring

`Normal` mode only captures crash artifacts and invokes callbacks. It does not restore backups and does not write the `UpgradeFail` failed-version marker.

```csharp
var context = new BowlContext
{
    ProcessNameOrId = "MyWorker.exe",
    DumpFileName = "startup_fail.dmp",
    FailFileName = "startup_fail.json",
    TargetPath = AppDomain.CurrentDomain.BaseDirectory,
    FailDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fail", "startup"),
    BackupDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "backup"),
    WorkModel = "Normal",
    TimeoutMs = 15_000,
    DumpType = DumpType.Mini,
    AutoRestore = false
};

BowlResult result = await new Bowl().LaunchAsync(context);
```

## Crash detection and recovery flow

`LaunchAsync` uses a simple signal: the platform strategy starts the monitoring tool and writes to `FailDirectory`; Bowl then checks whether `{FailDirectory}/{DumpFileName}` exists. If the dump file exists, startup is treated as failed.

| Stage | Current implementation |
| --- | --- |
| Prepare monitoring | Selects `WindowsBowlStrategy`, `LinuxBowlStrategy`, or `MacBowlStrategy` based on the OS |
| Capture exception | Windows uses ProcDump; Linux tries to install and call ProcDump; macOS uses basic `lldb` support |
| Detect crash | Checks for the configured dump file in `FailDirectory` |
| Generate report | Writes `{version}_fail.json` with monitoring parameters and tool output |
| Export diagnostics | On Windows, runs `Applications/Windows/export.bat` for driver info, system info, and recent system logs |
| Restore backup | Only when `WorkModel == "Upgrade"` and `AutoRestore == true`, copies `BackupDirectory` back over `TargetPath` |
| Mark failed version | In upgrade mode, writes `UpgradeFail = ExtendedField`; Core later skips updates at or below that failed version |
| Notify application | If `OnCrash` is configured, passes dump path, report path, version, and exit code |

`TimeoutMs` is the monitoring child process timeout. If the timeout expires and no dump exists, Bowl does not run the recovery pipeline. In integrations, treat `DumpCaptured` as the primary crash signal instead of relying only on `Success`.

## BowlContext options

| Option | Meaning | Recommendation |
| --- | --- | --- |
| `ProcessNameOrId` | Target process name or PID | Process name works on Windows; PID is preferred on Linux |
| `DumpFileName` | Dump file name | Include the version, for example `2.0.0_fail.dmp` |
| `FailFileName` | Crash report JSON file name | Match the dump version, for example `2.0.0_fail.json` |
| `TargetPath` | Current application installation root | Backup restoration copies files back here |
| `FailDirectory` | Failure artifact output directory | Use `Path.Combine(TargetPath, "fail", version)` |
| `BackupDirectory` | Pre-update backup directory | Must exist and be complete when `AutoRestore` is enabled |
| `WorkModel` | `Upgrade` or `Normal` | Use `Upgrade` for post-update rollback; use `Normal` for standalone crash capture |
| `ExtendedField` | Extension field, currently used mainly as version | Written to `UpgradeFail` in upgrade mode |
| `TimeoutMs` | Monitoring child process timeout | Normalizes to 30000 ms by default; increase for slow-starting apps |
| `DumpType` | `Full`, `Mini`, or `Heap` | Use `Mini` for smaller production artifacts; use `Full` for hard issues |
| `AutoRestore` | Whether to restore backups automatically | Set explicitly to `true` for upgrade rollback |
| `OnCrash` | Single crash callback | Use it to upload reports, notify users, or write business logs |

### Choosing DumpType

| Type | Windows ProcDump flag | Characteristics |
| --- | --- | --- |
| `Full` | `-ma` | Most complete data, largest file, best for hard-to-reproduce issues |
| `Mini` | `-mm` | Smaller and faster, a good production default |
| `Heap` | `-mh` | Mini dump with heap information; between Mini and Full |

## Output files

For failed upgrades, store artifacts by version:

```text
MyApp/
  fail/
    2.0.0/
      2.0.0_fail.dmp
      2.0.0_fail.json
      driverInfo.txt
      systeminfo.txt
      systemlog.evtx
  Logs/
    generalupdate-trace 2026-01-01.log
```

| File | Source | Contents |
| --- | --- | --- |
| `{version}_fail.dmp` | ProcDump or lldb | Memory snapshot from the crash |
| `{version}_fail.json` | `CrashReporter` | Mapped `BowlContext` parameters and monitoring tool output lines |
| `driverInfo.txt` | Windows `driverquery` | Windows driver list |
| `systeminfo.txt` | Windows `systeminfo` | OS, hardware, memory, and related system information |
| `systemlog.evtx` | Windows `wevtutil` | Windows System event log for the last day |
| `Logs/generalupdate-trace yyyy-MM-dd.log` | `GeneralTracer` | Bowl runtime trace log |

Non-Windows platforms currently do not export `driverInfo.txt`, `systeminfo.txt`, or `systemlog.evtx`, but Bowl still attempts to produce the dump and failure JSON.

The failure JSON is generated by the current `CrashReporter`:

```json
{
  "Parameter": {
    "TargetPath": "C:\\Program Files\\MyApp",
    "FailDirectory": "C:\\Program Files\\MyApp\\fail\\2.0.0",
    "BackupDirectory": "C:\\Program Files\\MyApp\\2.0.0",
    "ProcessNameOrId": "MyApp.exe",
    "DumpFileName": "2.0.0_fail.dmp",
    "FailFileName": "2.0.0_fail.json",
    "WorkModel": "Upgrade",
    "ExtendedField": "2.0.0"
  },
  "ProcdumpOutPutLines": [
    "ProcDump v11.0 - Sysinternals process dump utility",
    "[10:00:03] Dump 1 initiated: C:\\Program Files\\MyApp\\fail\\2.0.0\\2.0.0_fail.dmp",
    "[10:00:03] Dump count reached."
  ]
}
```

## Crash callback

`OnCrash` is a single crash callback. It fires only after Bowl detects a dump and receives a `CrashInfo` payload:

```csharp
public readonly record struct CrashInfo
{
    public string DumpFilePath { get; init; }
    public string CrashReportPath { get; init; }
    public string Version { get; init; }
    public int ExitCode { get; init; }
}
```

Common uses:

| Scenario | Approach |
| --- | --- |
| Upload diagnostics | Package the dump, JSON report, and Windows diagnostics, then upload them to your internal log platform |
| Notify users | Tell the user the new version failed to start and the previous version was restored |
| Audit business events | Record `Version`, `ExitCode`, and report paths in your own log system |

Callback exceptions are written to the trace log and do not stop `LaunchAsync` from returning its final `BowlResult`. Use the `CancellationToken` for cancellation.

## Trace logging switch

Bowl uses the public `GeneralTracer` for runtime tracing. By default it writes to the console and creates a daily file under the runtime directory:

```text
Logs/generalupdate-trace yyyy-MM-dd.log
```

For startup-performance, disk-write, or console-output sensitive scenarios, disable tracing:

```csharp
GeneralTracer.SetTracingEnabled(false);

var result = await new Bowl().LaunchAsync(context);

GeneralTracer.SetTracingEnabled(true);
```

Disabling tracing reduces Bowl's own diagnostic logs, but dump and failure JSON generation do not depend on this switch. Keep tracing enabled while investigating update failures; disable it in stable production paths according to your performance policy.

## Platform differences

| Platform | Monitoring tool | Diagnostic export | Notes |
| --- | --- | --- | --- |
| Windows | Bundled ProcDump: `procdump.exe`, `procdump64.exe`, `procdump64a.exe` | Supports `driverInfo.txt`, `systeminfo.txt`, `systemlog.evtx` | Tool path comes from `TargetPath/Applications/Windows`; sufficient dump permissions are required |
| Linux | Bundled deb/rpm packages + `install.sh`, then `procdump` | Currently no-op | Package mapping covers Ubuntu, Debian, RHEL, CentOS, Fedora, and ClearOS; the script may require `sudo` |
| macOS | `/usr/bin/lldb` | Currently no-op | Affected by SIP, debugging permission, and signing policy; current support is basic |

The NuGet package outputs `Applications/**/*` as content. If you self-deploy, make sure these files are not trimmed, otherwise the platform strategy may report that monitoring tooling is unavailable or fail to start the tool process.

## Recovery scenario

Suppose a user upgrades from `1.0.0` to `2.0.0` and the new version crashes immediately:

1. The update flow first stores the previous version in `BackupDirectory`, for example `C:\Program Files\MyApp\2.0.0`.
2. The new version is copied into `TargetPath`.
3. The main application starts, while Bowl monitors startup with `ProcessNameOrId = "MyApp.exe"`.
4. ProcDump captures the unhandled exception and writes `fail\2.0.0\2.0.0_fail.dmp`.
5. Bowl writes `2.0.0_fail.json`; on Windows it also exports driver info, system info, and recent system logs.
6. Because `WorkModel == "Upgrade"` and `AutoRestore == true`, Bowl copies `BackupDirectory` back over `TargetPath`.
7. Bowl writes `UpgradeFail = "2.0.0"`; the next Core check skips this known-failed version while the server still returns `2.0.0` or lower, until a higher version is available.
8. `OnCrash` can upload the diagnostic package or tell the user that the app has been restored to a working version.

The goal is to reduce the risk of "the update succeeded but the new app cannot start": users return to a runnable version, and developers get the dump plus context needed to fix the issue.

## Migrating from the old API

The old `GeneralUpdate.Bowl.Strategys.MonitorParameter` type is obsolete. Prefer `BowlContext` and the async entry point:

```csharp
var oldParameter = new GeneralUpdate.Bowl.Strategys.MonitorParameter
{
    ProcessNameOrId = "MyApp.exe",
    DumpFileName = "2.0.0_fail.dmp",
    FailFileName = "2.0.0_fail.json",
    TargetPath = installPath,
    FailDirectory = Path.Combine(installPath, "fail", "2.0.0"),
    BackupDirectory = Path.Combine(installPath, "2.0.0"),
    WorkModel = "Upgrade",
    ExtendedField = "2.0.0"
};

BowlContext context = Bowl.MapToContext(oldParameter);
BowlResult result = await new Bowl().LaunchAsync(context);
```

For new code, create `BowlContext` directly instead of depending on `MonitorParameter`.

## Related resources

- **Samples:** [GeneralUpdate-Samples / Bowl](https://github.com/GeneralLibrary/GeneralUpdate-Samples/tree/main/src/Bowl)
- **Main repository:** [GeneralUpdate](https://github.com/GeneralLibrary/GeneralUpdate)
