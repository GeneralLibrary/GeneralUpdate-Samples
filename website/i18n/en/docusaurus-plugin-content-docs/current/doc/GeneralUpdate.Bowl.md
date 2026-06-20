---
sidebar_position: 3
---

# GeneralUpdate.Bowl

**Namespace:** `GeneralUpdate.Bowl` | **Main Entry Point:** `new Bowl().LaunchAsync(BowlContext, CancellationToken)` | **NuGet Package:** `GeneralUpdate.Bowl`

## 1. Component Overview

### 1.1 Introduction

**GeneralUpdate.Bowl** is a startup watchdog component that runs after an upgrade completes. It does not download, extract, or replace update packages. Instead, after new version files are deployed and the main process is about to start or has started, it monitors the target process for startup crashes. If a crash is detected, it generates a memory dump, writes a failure report JSON, exports system diagnostics, and in Upgrade mode automatically restores the backup directory to the install directory, preventing users from being stuck on a non-bootable new version.

**Core Capabilities:**

| Capability | Description |
| --- | --- |
| Process Crash Monitoring | Attaches to target process via ProcDump (Windows/Linux) or lldb (macOS) to catch unhandled startup exceptions |
| Memory Dump Snapshots | Supports Full / Mini / Heap dump types, selectable by file size and completeness |
| Crash Report Generation | Auto-generates `{version}_fail.json` report with monitoring parameters and tool output |
| System Diagnostics Export | On Windows, exports driver list, system info, and recent system event logs |
| Automatic Rollback | In Upgrade mode, copies backup directory back to install directory for one-click rollback |
| Failed Version Marking | Writes `UpgradeFail` marker; Core skips this version until server provides a higher one |
| Event Callback | `OnCrash` callback for uploading diagnostics, notifying users, or recording audit info |
| Standalone Monitoring | Normal mode only captures crashes and generates reports without auto-restore |

**Business Problems Solved:**
- New version crashes during startup after upgrade, leaving users unable to use the app and unable to roll back
- Developers lack crash site information (dump, system environment) to diagnose "won't start after upgrade" issues
- Need automated rollback mechanism to reduce upgrade risk and avoid manual intervention

**Use Cases:**
- Desktop app post-upgrade startup health check with automatic rollback protection
- General process startup crash monitoring and diagnostic collection
- Automatic diagnostics after CI/CD smoke test failures

### 1.2 Environment & Dependencies

| Item | Description |
| --- | --- |
| **Version** | `10.5.0-beta.6` |
| **Target Framework** | `netstandard2.0` (.NET Framework 4.6.1+ / .NET Core 2.0+ / .NET 5+) |
| **Dependencies** | `System.Collections.Immutable`, `System.Text.Json` |
| **Bundled Tools** | Windows: `procdump.exe` / `procdump64.exe` / `procdump64a.exe`; Linux: `procdump` deb/rpm + `install.sh`; macOS: `/usr/bin/lldb` |
| **Compatibility** | Windows (full) / Linux (deb/rpm distros) / macOS (basic, SIP and debug permissions apply) |

---

## 2. Feature List

| Feature | Description | Type | Required | Notes |
| --- | --- | --- | --- | --- |
| Upgrade Mode Monitoring | Monitor new version startup crash, auto-restore backup, mark failed version | Core | Recommended | `WorkModel = "Upgrade"` |
| Standalone Monitoring | Capture crash and generate report only, no auto-restore | Core | Optional | `WorkModel = "Normal"` |
| Full Dump | Complete memory snapshot, most detail | Core | Optional | `DumpType.Full`, largest file |
| Mini Dump | Small memory snapshot, faster generation | Core | Optional | `DumpType.Mini`, recommended for production |
| Heap Dump | Mini dump with heap information | Core | Optional | `DumpType.Heap`, between Mini and Full |
| Crash Report JSON | Auto-generate structured crash report file | Core | Automatic | Output to `FailDirectory` |
| System Diagnostics Export | Export driver list, system info, event logs (Windows) | Extended | Automatic | Windows only |
| Auto Backup Restore | Copy backup directory back to install directory on crash | Core | Optional | `AutoRestore = true` |
| Failed Version Marking | Write upgrade failure version; Core skips subsequently | Core | Automatic | Upgrade mode only |
| Crash Callback | Business callback triggered on crash detection | Extended | Optional | `OnCrash` callback function |
| Trace Logging | `GeneralTracer` runtime diagnostic logs | Extended | Optional | Enabled by default, can be disabled |

---

## 3. API Configuration Reference

### 3.1 Configuration Properties (Props)

**BowlContext:**

| Field | Type | Default | Required | Values | Description |
| --- | --- | --- | --- | --- | --- |
| `ProcessNameOrId` | `string` | — | Yes | Process name or PID | Target process to monitor |
| `DumpFileName` | `string` | — | Yes | Valid filename | Dump output filename, e.g., `"{version}_fail.dmp"` |
| `FailFileName` | `string` | — | Yes | Valid filename | Crash report JSON filename, e.g., `"{version}_fail.json"` |
| `TargetPath` | `string` | — | Yes | Valid directory path | App install root; backup restored here on crash |
| `FailDirectory` | `string` | — | Yes | Valid directory path | Failure artifact output, e.g., `{TargetPath}/fail/{version}` |
| `BackupDirectory` | `string` | — | Recommended | Valid directory path | Pre-upgrade backup; must exist when `AutoRestore = true` |
| `WorkModel` | `string` | `"Upgrade"` (after `Normalize()`) | Optional | `"Upgrade"` / `"Normal"` | Work mode: upgrade rollback / standalone monitoring |
| `ExtendedField` | `string` | `null` | Optional | — | Extended field, typically the version number |
| `TimeoutMs` | `int` | `30000` (after `Normalize()`) | Optional | Positive integer (ms) | Monitoring subprocess timeout |
| `DumpType` | `DumpType` | `DumpType.Full` (after `Normalize()`) | Optional | `Full(0)`, `Mini(1)`, `Heap(2)` | Dump capture type |
| `AutoRestore` | `bool` | `false` | Optional | `true` / `false` | Auto-restore backup; set `true` in upgrade mode |
| `OnCrash` | `Func<CrashInfo, CancellationToken, Task>?` | `null` | Optional | — | Crash event callback, fires only when dump detected |

**DumpType Enum:**

| Value | Code | Windows ProcDump Flag | Characteristics |
| --- | --- | --- | --- |
| `Full` | `0` | `-ma` | Complete memory snapshot, most detail, largest file |
| `Mini` | `1` | `-mm` | Small snapshot, fast generation, recommended for production |
| `Heap` | `2` | `-mh` | Small dump with heap info, between Mini and Full |

### 3.2 Instance Methods

**Bowl:**

| Method | Parameters | Returns | Use Case | Notes |
| --- | --- | --- | --- | --- |
| `LaunchAsync(BowlContext, CancellationToken)` | `context` — execution context (call `Normalize()` first); `ct` — cancellation token | `Task<BowlResult>` | Start crash monitoring daemon | Three phases: prepare → run → handle crash if dump found |

**BowlContext:**

| Method | Parameters | Returns | Use Case | Notes |
| --- | --- | --- | --- | --- |
| `Normalize()` | None | `BowlContext` | Apply defaults (`WorkModel` → `"Upgrade"`, `TimeoutMs` → `30000`, `DumpType` → `Full`) | Returns new instance; does not modify original |

### 3.3 Callback Events

| Event | Callback Parameters | Trigger Timing | Usage Notes |
| --- | --- | --- | --- |
| `OnCrash` | `CrashInfo` — `DumpFilePath`, `CrashReportPath`, `Version`, `ExitCode`; `CancellationToken` | After dump file is detected | Use for uploading diagnostics, notifying users "new version rolled back", recording audit. Callback exceptions are logged to trace but don't block `LaunchAsync` return |

---

## 4. Advanced Examples

### 4.1 Extension Points Overview

Bowl's primary extension point is the `BowlContext.OnCrash` callback. Internal strategy interfaces (`IBowlStrategy`, `ICrashReporter`, `ISystemInfoProvider`) are internal; contribute to the GeneralUpdate repository for new platform support or custom report logic.

| Extension Point | Type | Description |
| --- | --- | --- |
| `OnCrash` Callback | `Func<CrashInfo, CancellationToken, Task>?` | Configured in `BowlContext`, triggered on crash |
| `GeneralTracer` Logging | Static class | Toggle via `SetTracingEnabled` |

### 4.2 Examples by Scenario

#### Scenario 1: Upgrade Mode Monitoring + Crash Alert Upload

**Description:** Desktop app upgrades to new version; Bowl monitors for startup crash. On crash, auto-rollback and upload diagnostics to internal log platform.

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
    TimeoutMs = 60_000,
    DumpType = DumpType.Mini,
    AutoRestore = true,
    OnCrash = async (info, ct) =>
    {
        var zipPath = Path.Combine(
            Path.GetDirectoryName(info.DumpFilePath)!,
            $"crash_{info.Version}_{DateTimeOffset.Now:yyyyMMddHHmmss}.zip");
        System.IO.Compression.ZipFile.CreateFromDirectory(
            Path.GetDirectoryName(info.DumpFilePath)!, zipPath);

        using var client = new HttpClient();
        var content = new MultipartFormDataContent();
        content.Add(new StreamContent(File.OpenRead(zipPath)), "file", Path.GetFileName(zipPath));
        content.Add(new StringContent(info.Version), "version");
        content.Add(new StringContent(info.ExitCode.ToString()), "exitCode");
        await client.PostAsync("https://logs.example.com/api/crash", content, ct);

        Console.WriteLine($"Version {info.Version} crashed (exit code {info.ExitCode}).");
        Console.WriteLine("Diagnostics uploaded. Previous version restored.");
    }
};

BowlResult result = await new Bowl().LaunchAsync(context);

if (result.DumpCaptured && result.Restored)
    Console.WriteLine("Crash detected and backup restored.");
```

#### Scenario 2: Standalone Monitoring (Non-Upgrade)

**Description:** General-purpose worker process startup crash monitoring; collect diagnostics only, no auto-rollback.

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

if (result.DumpCaptured)
{
    Console.WriteLine($"Dump captured at: {result.DumpFilePath}");
    Console.WriteLine($"Report at: {result.CrashReportPath}");
}
```

---

## 5. Basic Usage Examples

### 5.1 Quick Start (Minimal Demo)

```csharp
using GeneralUpdate.Bowl;

var context = new BowlContext
{
    ProcessNameOrId = "MyApp.exe",
    DumpFileName = "fail.dmp",
    FailFileName = "fail.json",
    TargetPath = AppDomain.CurrentDomain.BaseDirectory,
    FailDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fail"),
    BackupDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "backup"),
}.Normalize();

BowlResult result = await new Bowl().LaunchAsync(context);
Console.WriteLine($"Success: {result.Success}, Dump captured: {result.DumpCaptured}");
```

### 5.2 Basic Parameter Combination

```csharp
var version = "2.0.0";
var installPath = @"C:\Program Files\MyApp";

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
        Console.WriteLine($"Crash: {info.DumpFilePath}");
        return Task.CompletedTask;
    }
};

BowlResult result = await new Bowl().LaunchAsync(context);

if (result.DumpCaptured && result.Restored)
    Console.WriteLine("The upgraded version crashed and the backup was restored.");
```

### 5.3 Production-Ready Example

Full post-upgrade Bowl daemon workflow integrated into the upgrade process:

```csharp
using GeneralUpdate.Bowl;

var version = "2.0.0";
var installPath = @"C:\Program Files\MyApp";

var bowlContext = new BowlContext
{
    ProcessNameOrId = "MyApp.exe",
    DumpFileName = $"{version}_fail.dmp",
    FailFileName = $"{version}_fail.json",
    TargetPath = installPath,
    FailDirectory = Path.Combine(installPath, "fail", version),
    BackupDirectory = Path.Combine(installPath, "backups", version),
    WorkModel = "Upgrade",
    ExtendedField = version,
    TimeoutMs = 45_000,
    DumpType = DumpType.Mini,
    AutoRestore = true,
    OnCrash = async (info, ct) =>
    {
        try
        {
            using var client = new HttpClient();
            var crashData = new
            {
                version = info.Version,
                exitCode = info.ExitCode,
                dumpPath = info.DumpFilePath,
                reportPath = info.CrashReportPath,
                timestamp = DateTimeOffset.UtcNow
            };
            await client.PostAsJsonAsync(
                "https://monitor.mycompany.com/api/crash-report", crashData, ct);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to upload crash report: {ex.Message}");
        }
    }
};

var result = await new Bowl().LaunchAsync(bowlContext);

if (result.Success)
    Console.WriteLine("New version started successfully.");
else if (result.DumpCaptured)
{
    Console.WriteLine($"New version crashed (exit code: {result.ExitCode}).");
    Console.WriteLine($"Backup restored: {result.Restored}");
    Console.WriteLine($"Dump: {result.DumpFilePath}");
}
```

---

## 6. Global Configuration

Bowl does not rely on global configuration files. All configuration is passed via `BowlContext`. Logging behavior is controlled through the static `GeneralTracer` class.

### Logging Toggle

```csharp
GeneralTracer.SetTracingEnabled(false);
var result = await new Bowl().LaunchAsync(context);
GeneralTracer.SetTracingEnabled(true);
```

### Output File Structure

```text
MyApp/
  fail/
    2.0.0/
      2.0.0_fail.dmp         # Memory dump
      2.0.0_fail.json        # Crash report JSON
      driverInfo.txt          # Windows driver list
      systeminfo.txt          # OS/hardware/memory info
      systemlog.evtx          # Windows system event log
  Logs/
    generalupdate-trace 2026-01-01.log  # Bowl trace logs
```

### Platform Differences

| Platform | Monitoring Tool | Diagnostics Export | Notes |
| --- | --- | --- | --- |
| Windows | Built-in ProcDump (`procdump.exe`/`procdump64.exe`/`procdump64a.exe`) | Supports `driverInfo.txt`, `systeminfo.txt`, `systemlog.evtx` | Requires sufficient permissions for dumps |
| Linux | Built-in deb/rpm + `install.sh` ProcDump install | Currently no-op | Supports Ubuntu/Debian/RHEL/CentOS/Fedora/ClearOS |
| macOS | `/usr/bin/lldb` | Currently no-op | Affected by SIP, debug permissions, signing policy |

---

## Related Resources

- [Bowl Sample Code](https://github.com/GeneralLibrary/GeneralUpdate-Samples/tree/main/src/Bowl)
- [GeneralUpdate Repository](https://github.com/GeneralLibrary/GeneralUpdate)
