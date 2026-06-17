---
sidebar_position: 5
sidebar_label: 🔧 Advanced
title: 🔧 generalupdate-advanced — Advanced Customization
---

# 🔧 GeneralUpdate Advanced Customization Reference

Covers extension point architecture, Pipeline, differential engine, Bowl crash daemon, event system, and filesystem tools.

> ⚠️ **API Version Note**: This guide is based on **NuGet v10.5.0-beta.4**.
> All the following features are **available** in v10.5.0-beta.4:
> - ✅ `IUpdateHooks` lifecycle hooks (`Hooks<T>()`)
> - ✅ `IStrategy` custom strategy injection (`Strategy<T>()`)
> - ✅ `SilentPollOrchestrator` silent poller (`Option.Silent`)
> - ✅ `Option` programmable config system
> - ✅ `ISslValidationPolicy` SSL policy interface
> - ✅ `IHttpAuthProvider` HTTP auth provider
> - ✅ `DiffPipelineBuilder` differential pipeline config
>
> Namespace and usage for each feature is noted in each section.

---

## 📋 User Requirements Gathering

```
### Customization Target (Required)
- What customization is needed: ______ (Bowl crash daemon / IPC replacement / Pipeline customization / Custom strategy / AOT / Drivelution / Blacklist / Auth provider / Differential engine)
- GeneralUpdate version used: ______ (v10.4.6 stable / v10.5.0+ dev)
- .NET version: ______ (.NET 6/8/9/10)

### Bowl (if selected)
- Monitored process name: ______
- Work mode: ______ (Normal / Upgrade)
- Need crash Dump: ______ (Yes/No)
- Backup directory path: ______

### IPC Replacement (if selected)
- Replacement method: ______ (NamedPipe / SharedMemory / Custom)
- Target platform: ______ (Windows / Linux / macOS / Cross-platform)
- Security requirements: ______ (Encryption / Signing / None)

### AOT (if selected)
- Current trim warnings: ______ (Yes/No)
- Using reflection: ______ (Yes/No)
- JSON serialization needs: ______ (Yes/No)
```

---

## 1. Pipeline System (v10.5.0-beta.4 available)

GeneralUpdate uses the Pipeline pattern for update package verification, extraction, and patch application.

### PipelineBuilder API

```csharp
using GeneralUpdate.Core.Pipeline;

var context = new PipelineContext();
context.Add("ZipFilePath", @"C:\temp\update.zip");
context.Add("Hash", "sha256-hex-value");
context.Add("Format", 0);  // 0=Zip
context.Add("Encoding", System.Text.Encoding.UTF8);
context.Add("SourcePath", @"C:\Program Files\MyApp");
context.Add("PatchEnabled", true);

await new PipelineBuilder(context)
    .UseMiddleware<HashMiddleware>()      // Hash verification
    .UseMiddleware<CompressMiddleware>()  // Extraction
    .UseMiddleware<PatchMiddleware>()     // Differential patch
    .Build();
```

| Middleware | Class | Namespace | Function |
|-----------|-------|-----------|----------|
| Hash verification | `HashMiddleware` | `GeneralUpdate.Core.Pipeline` | SHA256 integrity check |
| Extraction | `CompressMiddleware` | `GeneralUpdate.Core.Pipeline` | ZIP extraction |
| Differential patch | `PatchMiddleware` | `GeneralUpdate.Core.Pipeline` | Apply BSDIFF/HDiffPatch patches |
| Driver update | `DrivelutionMiddleware` | `GeneralUpdate.Core.Pipeline` | Windows driver installation |

---

## 2. Strategy System (v10.5.0-beta.4 available)

GeneralUpdate has three built-in platform strategies via the `IStrategy` interface:

| Strategy | Class | Platform |
|----------|-------|----------|
| Windows | `WindowsStrategy` | Windows |
| Linux | `LinuxStrategy` | Linux |
| OSS | `OSSStrategy` | Cross-platform (object storage) |

> ✅ Supports custom strategy injection via `bootstrap.Strategy<T>()`.
> Custom strategies need to implement the `IStrategy` interface.

---

## 3. Bowl Crash Daemon (v10.5.0-beta.4)

Bowl is a crash monitoring component configured via `BowlContext`.

```csharp
using GeneralUpdate.Bowl;

var context = new BowlContext
{
    ProcessNameOrId = "MyApp.exe",
    DumpFileName = "v1.0.0.0_fail.dmp",
    FailFileName = "v1.0.0.0_fail.json",
    TargetPath = @"C:\Program Files\MyApp",
    FailDirectory = @"C:\Program Files\MyApp\fail",
    BackupDirectory = @"C:\Program Files\MyApp\backup",
    WorkModel = "Upgrade",
    TimeoutMs = 30_000,
    AutoRestore = true,
    OnCrash = async (info, ct) => Console.WriteLine($"Crash: {info.DumpFilePath}"),
};

var bowl = new Bowl();
var result = await bowl.LaunchAsync(context);
Console.WriteLine($"Result: Success={result.Success}, Restored={result.Restored}");
```

| Property | Type | Description |
|----------|------|-------------|
| `ProcessNameOrId` | string | Monitored process name or PID (required) |
| `TargetPath` | string | App install root directory (required) |
| `DumpFileName` | string | Dump file name (required) |
| `FailFileName` | string | Failure report file name (required) |
| `FailDirectory` | string | Crash report output directory (required) |
| `BackupDirectory` | string | Backup directory (required) |
| `WorkModel` | string | "Upgrade" or "Normal" |
| `TimeoutMs` | int | Monitor timeout(ms), default 30000 |
| `AutoRestore` | bool | Auto-rollback on crash |
| `DumpType` | DumpType | Mini / Full |
| `OnCrash` | delegate | Crash callback |

> ⚠️ In NuGet v10.5.0-beta.4, Bowl and Core have **no type conflicts** and can be referenced together.

---

## 4. EventManager (v10.5.0-beta.4 available)

EventManager is a global singleton providing event publish/subscribe:

```csharp
using GeneralUpdate.Core.Event;

// Add listener
EventManager.Instance.AddListener((object? sender, UpdateInfoEventArgs e) => { });

// Dispatch event manually
EventManager.Instance.Dispatch(this, new ExceptionEventArgs(ex, "Custom error"));

// Clear all listeners
EventManager.Instance.Clear();

// Dispose
EventManager.Instance.Dispose();
```

> ⚠️ EventManager is a global singleton. After `Dispose()`, `Instance` is still accessible (found by code audit).

---

## 5. Filesystem Tools (v10.5.0-beta.4 available)

### BlackList

`UpdateRequest` supports file exclusion through these properties:

```csharp
var config = new UpdateRequest
{
    // ...
    Files = new List<string> { "*.log", "*.tmp" },
    Formats = new List<string> { ".pdb", ".vshost.exe" },
    Directories = new List<string> { "logs", "cache", "temp" },
};
```

The blacklist is internally converted to `BlackPolicy` records via `ToBlackPolicy()`.

### FileTree (File Tree Diff)

```csharp
using GeneralUpdate.Core.FileSystem;

var tree = new FileTree();
var snapshot = tree.CreateSnapshot(@"C:\Program Files\MyApp");
```

---

## 6. Differential Engine (v10.5.0-beta.4 available, no extra package needed)

Differential types are embedded in `GeneralUpdate.Core`, **no need** for a separate `GeneralUpdate.Differential` package.

### DiffPipelineBuilder (recommended)

```csharp
using GeneralUpdate.Core.Pipeline;

var pipeline = new DiffPipelineBuilder()
    .UseDiffer(new StreamingHdiffDiffer())     // Diff algorithm
    .UseCleanMatcher(new DefaultCleanMatcher()) // File matcher (server side)
    .UseDirtyMatcher(new DefaultDirtyMatcher()) // File matcher (client side)
    .WithParallelism(4)
    .WithStopOnFirstError(true)
    .WithProgress(new Progress<DiffProgress>(p =>
        Console.WriteLine($"[{p.Completed}/{p.Total}] {p.FileName}")))
    .Build();

// Server side: generate patches
await pipeline.CleanAsync(oldDir, newDir, patchDir);

// Client side: apply patches
await pipeline.DirtyAsync(appDir, patchDir);
```

### Bootstrap Integration

```csharp
new GeneralUpdateBootstrap()
    .SetConfig(config)
    .UseDiffPipeline(pipeline =>
    {
        pipeline.WithParallelism(2)
                .WithStopOnFirstError(true);
    })
    .LaunchAsync();
```

### Custom Matchers

```csharp
using GeneralUpdate.Core.Differential;

var cleanMatcher = new DefaultCleanMatcher();  // or implement ICleanMatcher
var dirtyMatcher = new DefaultDirtyMatcher();  // or implement IDirtyMatcher
```

---

## 7. AOT / NativeAOT Compatibility

GeneralUpdate.Core v10.5.0-beta.4 supports .NET Native AOT (`net8.0` and `net10.0`):

```xml
<PropertyGroup>
  <IsAotCompatible>true</IsAotCompatible>
  <EnableTrimAnalyzer>true</EnableTrimAnalyzer>
</PropertyGroup>
```

JSON serialization contexts (reduce AOT size):

```csharp
using GeneralUpdate.Core.JsonContext;

// Use built-in JsonSerializerContext
// VersionRespJsonContext, ProcessContractJsonContext, HttpParameterJsonContext etc.
```

---

## 8. Drivelution (Windows Driver Updates)

The `GeneralUpdate.Drivelution` package provides Windows driver management:

```csharp
using GeneralUpdate.Drivelution;

// Scan driver directory
var allDrivers = GeneralDrivelution.ScanDirectory(driverDir);

// Validate driver
var isValid = GeneralDrivelution.ValidateDriver(driverPath);

// Install driver (DIFx → SetupAPI → PnPUtil cascade)
var result = GeneralDrivelution.InstallDriver(driverPath);
```

---

## Feature Availability Index

| Feature | Availability | Reference |
|---------|-------------|-----------|
| Pipeline | ✅ v10.5.0-beta.4 | `GeneralUpdate.Core.Pipeline` |
| Strategy System | ✅ v10.5.0-beta.4 | `GeneralUpdate.Core.Strategy` |
| FileTree | ✅ v10.5.0-beta.4 | `GeneralUpdate.Core.FileSystem` |
| BlackList | ✅ v10.5.0-beta.4 | `UpdateRequest.Files/Formats/Directories` → `ToBlackPolicy()` |
| Differential Engine | ✅ Embedded in Core | `DiffPipelineBuilder` / `DiffPipeline` |
| AOT | ✅ v10.5.0-beta.4 | `JsonSerializerContext` subclasses |
| EventManager | ✅ v10.5.0-beta.4 | `GeneralUpdate.Core.Event` |
| Bowl Crash Daemon | ✅ v10.5.0-beta.4 | `GeneralUpdate.Bowl.Bowl` |
| IUpdateHooks | ✅ v10.5.0-beta.4 | `GeneralUpdate.Core.Hooks` — `Hooks<T>()` |
| Custom Strategy Injection | ✅ v10.5.0-beta.4 | `Strategy<T>()` |
| IPC Replacement Interface | ❌ Not yet supported | Use NamedPipe alternative |
| SilentPollOrchestrator | ✅ v10.5.0-beta.4 | `Option.Silent` + `SetOption()` |
| Option System | ✅ v10.5.0-beta.4 | `SetOption<T>(Option<T>, T)` |

---

## ✅ Advanced Customization Verification Checklist

### Bowl Crash Daemon
- [ ] With Bowl: reference both `GeneralUpdate.Core` and `GeneralUpdate.Bowl` (no conflict in v10.5.0-beta.4)
- [ ] `BowlContext.ProcessNameOrId` matches the actual process name
- [ ] `TargetPath` set to app install root directory, not a subdirectory
- [ ] `WorkModel` correct for the scenario (Normal/Upgrade)
- [ ] `FailDirectory` has write permissions
- [ ] Linux/macOS: Bowl is Windows-only

### Pipeline Customization
- [ ] `PipelineContext` key names spelled correctly
- [ ] Middleware order correct: Hash → Compress → Patch → Drivelution
- [ ] `Encoding` set to `Encoding.UTF8`

### AOT/NativeAOT
- [ ] Enabled `<IsAotCompatible>true</IsAotCompatible>`
- [ ] Added `[DynamicDependency]` or `[RequiresUnreferencedCode]` for reflection paths
- [ ] Used built-in `JsonSerializerContext` subclasses (reduce trimming)

---

## ⚠️ Anti-Pattern Checklist

| # | Anti-Pattern | Consequence | Correct Approach |
|---|-------------|-------------|------------------|
| 1 | **Using dev-branch APIs (IUpdateHooks etc.) on v10.4.6 stable** | Build failure / MissingMethodException | Check API availability table |
| 2 | **PipelineContext key spelling errors** | Pipeline runs abnormally, values not passed | Use library constants or documented key names |
| 3 | **Bowl WorkModel set to Upgrade but process is main app** | Monitoring logic error | Normal=main process, Upgrade=upgrade process |
| 4 | **Using default encryption key for IPC on Windows** | Encryption can be cracked | Use strong key (≥ 32 chars) |
| 5 | **Different source file structure when generating patches** | Patch application fails | Source and target file structure must be consistent |
| 6 | **Heavy reflection in AOT without DynamicDependency** | TypeLoadException at runtime | Use source generators or explicit preserve markers |
| 7 | **PatchMiddleware before CompressMiddleware in Pipeline** | Trying to patch without extraction | Order must be Compress→Patch |

---

## Related Skills

- `/generalupdate-init` — Bootstrap configuration
- `/generalupdate-strategy` — Strategy selection
- `/generalupdate-troubleshoot` — Issue diagnosis
