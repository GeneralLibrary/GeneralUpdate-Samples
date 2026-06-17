---
sidebar_position: 2
sidebar_label: 🚀 Integration
title: 🚀 generalupdate-init — Quick Integration Guide
---

# 🚀 GeneralUpdate Integration Complete Guide

Helps developers integrate GeneralUpdate auto-update into any .NET application from scratch, covering all configuration methods, deployment scenarios, and production considerations.

> ⚠️ **Targeting NuGet v10.5.0-beta.4**. `Configinfo` has been replaced by `UpdateRequest`, namespace moved to `GeneralUpdate.Core.Configuration`.

---

## 📋 User Requirements Gathering

Before generating code, the following information MUST be collected. **Uncertain fields MUST be clarified:**

```
### Project Status
- Existing project type: ______ (new / existing / migrating from old version)
- .NET version: ______
- UI framework: ______ (WPF/WinForms/Avalonia/MAUI/Console/None)
- Target platform: ______ (Windows/Linux/macOS/Multi-platform)

### Update Requirements
- Need progress UI: ______ (Yes/No)
- Has backend service: ______ (Yes/No)
- Strategy preference: ______ (Standard/OSS/Silent/Differential/CVP/Push)
- Need Bowl crash daemon: ______ (Yes/No)

### Existing Configuration (if any)
- NuGet already installed: ______ (Yes/No, version)
- UpdateRequest already configured: ______ (Yes/No)
- manifest.json already exists: ______ (Yes/No)
```

---

## Workflow (Execute in Order)

### Step 1: Project Detection

```
├── Check .csproj → target framework, UI type, NuGet references
├── Check for existing generalupdate.manifest.json
├── Check for existing UpdateRequest/Bootstrap configuration code
└── Check project structure → is there a separate Upgrade project?
```

### Step 2: Select Integration Mode

| Mode | Use Case | Output |
|------|----------|--------|
| **[Minimal]** | Quick start for new users, console/service apps | 3 lines of Bootstrap code |
| **[Standard]** | Need precise control over update process | UpdateRequest + full event listeners |
| **[Scaffold]** | Team project from scratch | Full Client + Upgrade dual-project structure |

### Step 3: Generate Output

```
├── NuGet install commands (choose Core/Bowl by platform)
├── Bootstrap configuration code (by mode)
├── manifest.json template
├── Deployment checklist
└── Known issue warnings (for your config combination)
```

### Step 4: Guide Next Steps

```
├── Need UI → /generalupdate-ui
├── Choose strategy → /generalupdate-strategy
├── Need Bowl daemon → /generalupdate-advanced
└── Having issues → /generalupdate-troubleshoot
```

---

## Core Concept: 4 Update Scenes

GeneralUpdate decides the update strategy based on the package type returned by the server:

| Scene | Behavior |
|-------|----------|
| **None** | No update needed, launch main app directly |
| **UpgradeOnly** | Only update the upgrade app itself: Client extracts Upgrade package in-place |
| **MainOnly** | Only update the main app: Client → IPC → Launch Upgrade process |
| **Both** | Update both |

**Dual-Process Architecture**:
```
App.exe (Client) is responsible for:
  ├── Version verification (HTTP request to server)
  ├── Downloading all update packages
  ├── IPC write (encrypted file to pass parameters to Upgrade)
  └── Launching Upgrade.exe then exiting

Upgrade.exe (Upgrade process) is responsible for:
  ├── Reading the IPC file
  ├── Applying the update (extract/patch/replace files)
  └── Launching the main app then exiting
```

---

## UpdateRequest Reference

### Complete UpdateRequest Properties

```csharp
// Method A: Directly construct UpdateRequest (recommended)
var config = new UpdateRequest
{
    // === Required ===
    UpdateUrl = "https://your-server.com/Upgrade/Verification",
    AppSecretKey = "your-secret-key",
    MainAppName = "MyApp.exe",
    ClientVersion = "1.0.0.0",
    ProductId = "my-product-001",
    InstallPath = ".",
    
    // === Optional ===
    ReportUrl = "https://your-server.com/Upgrade/Report",
    UpdateLogUrl = "https://your-server.com/Upgrade/Log",
    UpgradeClientVersion = "1.0.0.0",
    
    // === Authentication ===
    AuthScheme = AuthScheme.Hmac,  // Hmac / Bearer / ApiKey / Basic
    Token = "your-token",
    BasicUsername = "user",
    BasicPassword = "pass",
    
    // === Blacklist (excluded from backup/copy) ===
    Files = new List<string> { "*.log", "*.tmp" },
    Formats = new List<string> { ".pdb" },
    Directories = new List<string> { "logs", "cache" },
};

// Method B: Using the builder pattern
var config = UpdateRequestBuilder.Create()
    .SetUpdateUrl("https://your-server.com/api")
    .SetAppSecretKey("your-secret-key")
    .SetMainAppName("MyApp.exe")
    .SetClientVersion("1.0.0.0")
    .SetProductId("my-product-001")
    .SetInstallPath(".")
    .Build();

// Method C: Zero-config — discover from manifest.json
await new GeneralUpdateBootstrap()
    .SetSource(
        updateUrl: "https://your-server.com/api",
        appSecretKey: "your-secret-key")
    .AddListenerUpdateInfo(...)
    .LaunchAsync();
```

### AppType Roles

`AppType` is an enum (v10.5.0-beta.4):

| Value | Name | Description |
|-------|------|-------------|
| 1 | `AppType.Client` | Standard client (main app) |
| 2 | `AppType.Upgrade` | Standard upgrade app |
| 3 | `AppType.OssClient` | OSS client mode (silent) |
| 4 | `AppType.OssUpgrade` | OSS upgrade mode |

### Event Listener Checklist

```csharp
// All 7 events
.AddListenerUpdateInfo((_, e) => {
    /* Version verification results (e.Info?.Body contains VersionEntry list) */
})
.AddListenerMultiDownloadStatistics((_, e) => {
    /* Batch download progress (e.ProgressPercentage, e.Speed, e.Remaining) */
})
.AddListenerMultiDownloadCompleted((_, e) => {
    /* Per-version download complete (e.Version, e.IsCompleted) */
})
.AddListenerMultiDownloadError((_, e) => {
    /* Download error (e.Exception, e.Version) */
})
.AddListenerMultiAllDownloadCompleted((_, e) => {
    /* All downloads complete (e.IsAllDownloadCompleted, e.FailedVersions) */
})
.AddListenerException((_, e) => {
    /* Exception (e.Message, e.Exception) */
})
.AddListenerProgress((_, e) => {
    /* Progress (e.Progress or e.DiffProgress, v10.5+) */
})
```

---

## Integration Code

### Method A: Minimal — Using UpdateRequest

```csharp
using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;

var config = new UpdateRequest
{
    UpdateUrl = "https://your-server.com/api",
    AppSecretKey = "your-32-char-secret-key-here!",
    MainAppName = "MyApp.exe",
    ClientVersion = "1.0.0.0",
    ProductId = "my-product-001",
    InstallPath = "."
};

await new GeneralUpdateBootstrap()
    .SetConfig(config)
    .LaunchAsync();
```

### Method B: Standard — UpdateRequest + Event Listeners

```csharp
using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;
using GeneralUpdate.Core.Download;

var config = new UpdateRequest
{
    UpdateUrl = "https://your-server.com/Upgrade/Verification",
    AppSecretKey = "your-secret-key",
    MainAppName = "MyApp.exe",
    ClientVersion = "1.0.0.0",
    ProductId = "my-product-001",
    InstallPath = AppDomain.CurrentDomain.BaseDirectory,
    ReportUrl = "https://your-server.com/Upgrade/Report",
};

await new GeneralUpdateBootstrap()
    .SetConfig(config)
    .AddListenerUpdateInfo((_, e) =>
    {
        Console.WriteLine($"Found {e.Info?.Body?.Count ?? 0} versions");
    })
    .AddListenerMultiDownloadStatistics((_, e) =>
    {
        Console.WriteLine($"Progress: {e.ProgressPercentage}% | {e.Speed}");
    })
    .AddListenerMultiDownloadCompleted((_, e) =>
    {
        Console.WriteLine($"Version {e.Version} download complete");
    })
    .AddListenerMultiAllDownloadCompleted((_, e) =>
    {
        Console.WriteLine($"All done (IsAllDownloadCompleted={e.IsAllDownloadCompleted})");
    })
    .AddListenerMultiDownloadError((_, e) =>
    {
        Console.WriteLine($"Download failed: version {e.Version} — {e.Exception?.Message}");
    })
    .AddListenerException((_, e) =>
    {
        Console.WriteLine($"Exception: {e.Message}");
    })
    .LaunchAsync();
```

### Upgrade Process Configuration

```csharp
using GeneralUpdate.Core;

// Upgrade mode reads config from IPC file, no SetConfig needed
await new GeneralUpdateBootstrap()
    .AddListenerException((_, e) =>
        Console.WriteLine($"Error: {e.Message}"))
    .LaunchAsync();
```

---

## Production Deployment Checklist

### Directory Structure

```
publish/
├── MyApp.exe                  ← MainAppName (main program)
├── generalupdate.manifest.json
└── update/
    └── UpgradeApp.exe         ← Upgrade program, must ship with first release
```

### Dual-Process Verification

| Check | Description |
|-------|-------------|
| UpgradeApp.exe exists in publish directory | Must be present from first release |
| Client and Upgrade use same AppSecretKey | Required for IPC encrypted communication |
| Client and Upgrade use same NuGet version | Mismatch causes "Method not found" |
| Upgrade process doesn't need network | All data pre-downloaded by Client |

---

## ⚠️ Known Issues

### NuGet Notes (v10.5.0-beta.4)
`GeneralUpdate.Core` and `GeneralUpdate.Bowl` **can be referenced together** (no CS0433 conflict in v10.5.0-beta.4).
- Using Core: `dotnet add package GeneralUpdate.Core`
- Using Bowl: `dotnet add package GeneralUpdate.Bowl` (it does **not** transitively include Core, need to reference Core too)
- Differential types are embedded in Core, **no need** for separate `GeneralUpdate.Differential` package

### New Features in v10.5.0-beta.4
- ✅ `IUpdateHooks` lifecycle hooks — `Hooks<T>()`
- ✅ Programmable `Option` system — `SetOption(Option.Silent, true)`
- ✅ `SilentPollOrchestrator` silent poller
- ✅ `SetSource()` zero-config entry
- ✅ `UseDiffPipeline()` differential pipeline config
- ✅ `AddListenerProgress()` 7th event
- ✅ `IStrategy` custom strategy injection — `Strategy<T>()`
- ✅ `IUpdateReporter` / `IHttpAuthProvider` extension points

---

## ✅ Integration Verification Checklist

### Bootstrap Configuration
- [ ] All 6 required `UpdateRequest` fields set (UpdateUrl, AppSecretKey, MainAppName, ClientVersion, ProductId, InstallPath)
- [ ] `UpdateUrl` server API returns valid version info
- [ ] `AppSecretKey` length ≥ 16 characters, matches server
- [ ] `AppType` set correctly (Client = 1, Upgrade = 2)
- [ ] Production uses `AppDomain.CurrentDomain.BaseDirectory` as InstallPath

### NuGet & Build
- [ ] Client and Upgrade use the **exact same** GeneralUpdate NuGet version
- [ ] With Bowl: reference both `GeneralUpdate.Core` and `GeneralUpdate.Bowl` (no conflict in v10.5.0-beta.4)
- [ ] `dotnet build` succeeds (0 errors)
- [ ] No need for separate `GeneralUpdate.Differential` reference

### Deployment Structure
- [ ] UpgradeApp.exe exists in `update/` subdirectory (must from first release)
- [ ] `generalupdate.manifest.json` has `UpdateAppName` including `.exe`
- [ ] IPC file (`UpdateInfo.msg`) path is consistent between Client/Upgrade
- [ ] `Encoding` set to `Encoding.UTF8` (prevents garbled text on Linux/macOS)

---

## ⚠️ Anti-Pattern Checklist

| # | Anti-Pattern | Consequence | Correct Approach |
|---|-------------|-------------|------------------|
| 1 | **Core and Bowl NuGet versions differ** | Runtime MethodNotFoundException | Use the same NuGet version |
| 2 | **Bowl missing `GeneralUpdate.Core` reference** | Build fails, missing Core types | Bowl doesn't transitively include Core, reference Core separately |
| 3 | **Misunderstanding Bowl transitively includes Core** | Build fails | In v10.5.0-beta.4, Bowl is independent, reference Core separately |
| 4 | **Client/Upgrade NuGet versions differ** | Runtime MethodNotFoundException | Lock to exactly the same version |
| 5 | **Blocking IO in event listeners** | Upgrade process UI freezes, killed by timeout | Only update UI state, async for heavy operations |
| 6 | **IPC encoding not set to UTF-8** | Garbled text on Linux/macOS | `Encoding.UTF8` |
| 7 | **Version not in 4-part format (1.0.0.0)** | Version comparison logic exception | Always use `x.y.z.w` format |
| 8 | **manifest.json mainAppName doesn't match actual process name** | Main app not found after update | Match the actual exe name |

---

## Related Skills

- `/generalupdate-ui` — UI framework auto-detection + update window code generation
- `/generalupdate-strategy` — 6 update strategy selection and configuration
- `/generalupdate-advanced` — Advanced customization
- `/generalupdate-troubleshoot` — Known issue diagnosis
