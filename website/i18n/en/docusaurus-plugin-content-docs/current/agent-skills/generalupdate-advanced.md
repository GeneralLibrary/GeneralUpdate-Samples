---
sidebar_position: 5
sidebar_label: 🔧 Advanced
title: 🔧 generalupdate-advanced — Advanced Customization
---

# 🔧 GeneralUpdate Advanced Customization Reference

Covers extension point architecture, Pipeline, differential engine, Bowl crash daemon, event system, and filesystem tools.

> ⚠️ **API Version Note**: This guide is based on **NuGet v10.4.6 stable**.
> 
> The following features do **NOT exist** in the stable release (but are available in dev branch v10.5.0-beta.2):
> - `IUpdateHooks` lifecycle hooks
> - `IProcessInfoProvider` IPC replacement interface
> - `SilentPollOrchestrator` silent poller
> - `Option` programmable config system (v10.4.6 uses `Configinfo` properties only)
> - `ISslValidationPolicy` SSL policy interface
>
> Feature availability is noted in each section.

---

## 1. Pipeline System (v10.4.6 available)

GeneralUpdate uses the Pipeline pattern for update package verification, extraction, and patch application.

### PipelineBuilder API

```csharp
using GeneralUpdate.Common.Internal.Pipeline;
using GeneralUpdate.Common.Internal.Strategy;

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

---

## 2. Strategy System (v10.4.6 available)

| Strategy | Class | Platform |
|----------|-------|----------|
| Windows | `WindowsStrategy` | Windows |
| Linux | `LinuxStrategy` | Linux |
| OSS | `OSSStrategy` | Cross-platform |

> ⚠️ Stable release does **not support** custom strategy injection via `bootstrap.Strategy<T>()`.

---

## 3. Bowl Crash Daemon (v10.4.6 limited)

> ⚠️ v10.4.6 Bowl only provides basic type definitions. `Bowl` class has no public `LaunchAsync` method. Full functionality is in dev branch.

### MonitorParameter Configuration

```csharp
using GeneralUpdate.Bowl;
using GeneralUpdate.Bowl.Strategys;

var param = new MonitorParameter
{
    ProcessNameOrId = "MyApp.exe",
    DumpFileName = "v1.0.0.0_fail.dmp",
    FailFileName = "v1.0.0.0_fail.json",
    TargetPath = @"C:\Program Files\MyApp",
    FailDirectory = @"C:\Program Files\MyApp\fail",
    BackupDirectory = @"C:\Program Files\MyApp\backup",
    WorkModel = "Upgrade",
};

var bowl = new Bowl();
```

---

## 4. EventManager (v10.4.6 available)

```csharp
using GeneralUpdate.Common.Internal.Event;

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

## Feature Availability Index

| Feature | Availability | Reference |
|---------|-------------|-----------|
| Pipeline | ✅ v10.4.6 | `GeneralUpdate.Common.Internal.Pipeline` |
| Strategy System | ✅ v10.4.6 | `GeneralUpdate.Common.Internal.Strategy` |
| FileTree | ✅ v10.4.6 | `GeneralUpdate.Common.FileBasic` |
| BlackList | ✅ v10.4.6 | `Configinfo.BlackFiles` etc. |
| Differential Engine | ✅ needs `GeneralUpdate.Differential` | `DifferentialCore` |
| AOT | ✅ v10.4.6 | `JsonSerializerContext` subclasses |
| EventManager | ✅ v10.4.6 | `GeneralUpdate.Common.Internal.Event` |
| Bowl Crash Daemon | ⚠️ Basic types only | `GeneralUpdate.Bowl.Bowl` |
| IUpdateHooks | ❌ v10.4.6 unsupported | Dev branch v10.5.0-beta.2 |
| Custom Strategy Injection | ❌ v10.4.6 unsupported | Dev branch v10.5.0-beta.2 |
| IPC Replacement Interface | ❌ v10.4.6 unsupported | Dev branch v10.5.0-beta.2 |
| SilentPollOrchestrator | ❌ v10.4.6 unsupported | Dev branch v10.5.0-beta.2 |
| Option System | ❌ v10.4.6 unsupported | Configinfo properties only |

---

## Related Skills

- [🚀 generalupdate-init — Bootstrap configuration](./generalupdate-init.md)
- [⚙️ generalupdate-strategy — Strategy selection](./generalupdate-strategy.md)
- [🩺 generalupdate-troubleshoot — Issue diagnosis](./generalupdate-troubleshoot.md)
