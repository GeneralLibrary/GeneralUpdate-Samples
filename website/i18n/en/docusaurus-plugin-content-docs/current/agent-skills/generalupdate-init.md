---
sidebar_position: 2
sidebar_label: 🚀 Integration
title: 🚀 generalupdate-init — Quick Integration Guide
---

# 🚀 GeneralUpdate Integration Complete Guide

Helps developers integrate GeneralUpdate auto-update into any .NET application from scratch, covering all configuration methods, deployment scenarios, and production considerations.

> ⚠️ **Targeting NuGet v10.4.6 stable**. Dev branch (v10.5.0-beta.2) has different APIs.

---

## Workflow

```
1. Project Detection
   ├── Check .csproj → target framework, UI type
   └── Check existing configuration → NuGet installed? Existing manifest?
   
2. Select Integration Mode
   ├── [Minimal] (new Configinfo + SetConfig + LaunchAsync) — recommended for new users
   ├── [Standard] (Configinfo + event listeners) — fine control
   └── [Scaffold] (full dual-project structure) — team projects from scratch

3. Generate Output
   ├── NuGet installation commands
   ├── Bootstrap configuration code
   ├── manifest.json template
   └── Deployment checklist

4. Next Steps
   ├── Need UI → generalupdate-ui
   ├── Choose strategy → generalupdate-strategy
   └── Having issues → generalupdate-troubleshoot
```

---

## Core Concept: 4 Update Scenes

| Scene | Behavior |
|-------|----------|
| **None** | No update needed, launch main app directly |
| **UpgradeOnly** | Only update the upgrade app itself |
| **MainOnly** | Only update the main app |
| **Both** | Update both |

### Dual-Process Architecture

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

## Configinfo Reference

### Complete Configinfo Properties

```csharp
var config = new Configinfo
{
    // === Required ===
    UpdateUrl = "https://your-server.com/Upgrade/Verification",
    AppSecretKey = "your-secret-key",
    AppName = "MyApp.exe",
    MainAppName = "MyApp.exe",
    ClientVersion = "1.0.0.0",
    ProductId = "my-product-001",
    InstallPath = ".",
    
    // === Optional ===
    ReportUrl = "https://your-server.com/Upgrade/Report",
    UpdateLogUrl = "https://your-server.com/Upgrade/Log",
    UpgradeClientVersion = "1.0.0.0",
    
    // === Authentication ===
    Scheme = "Bearer",
    Token = "your-token",
    
    // === Blacklist (excluded from backup/copy) ===
    BlackFiles = new List<string> { "*.log", "*.tmp" },
    BlackFormats = new List<string> { ".pdb" },
    SkipDirectorys = new List<string> { "logs", "cache" },
};
```

### Event Listener Checklist

```csharp
// All 6 events
.AddListenerUpdateInfo((_, e) => {
    /* Version verification results */
})
.AddListenerMultiDownloadStatistics((_, e) => {
    /* Batch download progress */
})
.AddListenerMultiDownloadCompleted((_, e) => {
    /* Per-version download complete */
})
.AddListenerMultiDownloadError((_, e) => {
    /* Download error */
})
.AddListenerMultiAllDownloadCompleted((_, e) => {
    /* All downloads complete */
})
.AddListenerException((_, e) => {
    /* Exception handler */
})
```

---

## Integration Code

### Minimal Integration

```csharp
using GeneralUpdate.Core;
using GeneralUpdate.Common.Shared.Object;

var config = new Configinfo
{
    UpdateUrl = "https://your-server.com/api",
    AppSecretKey = "your-32-char-secret-key-here!",
    AppName = "MyApp.exe",
    MainAppName = "MyApp.exe",
    ClientVersion = "1.0.0.0",
    ProductId = "my-product-001",
    InstallPath = "."
};

await new GeneralUpdateBootstrap()
    .SetConfig(config)
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

### NuGet Type Conflicts

`GeneralUpdate.Core` and `GeneralUpdate.Bowl` **cannot be referenced together** (CS0433 type conflicts).

Choose based on requirements:
- Using Core: `dotnet add package GeneralUpdate.Core`
- Using Bowl: **only reference** `GeneralUpdate.Bowl` (transitively includes all Core functionality)
- Differential types are embedded in Core, **no need** for separate `GeneralUpdate.Differential` package

---

## Related Skills

- [🎨 generalupdate-ui — UI framework auto-detection + update window code generation](./generalupdate-ui.md)
- [⚙️ generalupdate-strategy — 6 update strategy selection and configuration](./generalupdate-strategy.md)
- [🔧 generalupdate-advanced — Advanced customization](./generalupdate-advanced.md)
- [🩺 generalupdate-troubleshoot — Known issue diagnosis](./generalupdate-troubleshoot.md)
