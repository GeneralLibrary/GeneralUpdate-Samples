---
sidebar_position: 2
sidebar_label: 🚀 Integration
title: 🚀 generalupdate-init — Quick Integration Guide
---

# 🚀 GeneralUpdate Integration Guide

Helps developers integrate GeneralUpdate auto-update into any .NET application. **Start from scratch — no prior knowledge of GeneralUpdate internals required.**

---

## First: Why do you need two programs?

GeneralUpdate uses a **dual-process architecture**. Understanding this is key to everything else:

```
Your App.exe (Client — main application)
    │
    ├── Normally: runs your business logic
    │
    ├── When an update is found:
    │   1. Downloads the update package
    │   2. Writes download info to an IPC file (temporary file)
    │   3. Launches the "upgrade program" (UpgradeApp.exe)
    │   4. Exits itself (Windows won't let a program overwrite itself)
    │
    ▼
UpgradeApp.exe (Upgrade — update installer)
    │
    ├── Reads the IPC file
    ├── Extracts/replaces files (main program has exited, files are free)
    ├── Launches the main program
    └── Exits itself
```

> **IPC file** (Inter-Process Communication) = a temporary file. The Client writes update info into it; the Upgrade reads it to perform the actual update.

> **manifest.json** = a manifest file that tells GeneralUpdate what your main program is called and where the upgrade program lives.

The reason is simple: **Windows doesn't allow a running program to overwrite its own .exe**, so a separate "helper" process is needed to do the file replacement.

---

## What you'll need

```
1. Your .NET project (WPF / WinForms / Avalonia / MAUI / Console — any works)
2. .NET SDK (8.0+)
3. A backend service (optional — you can use OSS object storage instead)
```

---

## Install NuGet

```bash
dotnet add package GeneralUpdate.Core --version 10.5.0-beta.7
```

> If you need the crash daemon (Bowl), add one more package: `dotnet add package GeneralUpdate.Bowl --version 10.5.0-beta.7`  
> Differential update is built into Core — **no need** for a separate `GeneralUpdate.Differential` package.

---

## Minimum setup: 3 lines to get it running

This is the simplest integration, suitable for console apps or just verifying functionality:

```csharp
using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;

var config = new UpdateRequest
{
    UpdateUrl = "https://your-server.com/api",   // Server API endpoint
    AppSecretKey = "your-secret-key",             // Secret key (≥ 16 chars)
    MainAppName = "MyApp.exe",                    // Your main program filename
    ClientVersion = "1.0.0.0",                    // Current version
    ProductId = "my-product-001",                 // Your product ID
    InstallPath = "."                              // Install directory (. = current dir)
};

await new GeneralUpdateBootstrap().SetConfig(config).LaunchAsync();
```

These 6 fields are **required** — missing any will cause an error:

| Field | What to put | Why it's needed |
|-------|-------------|-----------------|
| `UpdateUrl` | Server API URL | The program queries this for new versions |
| `AppSecretKey` | Your chosen key | Used for IPC encrypted communication; must match between Client & Upgrade |
| `MainAppName` | `"MyApp.exe"` | Tells the Upgrade which exe to launch after updating |
| `ClientVersion` | `"1.0.0.0"` | Server uses this to decide which versions to send |
| `ProductId` | `"my-app-001"` | Differentiates products (if you have multiple apps) |
| `InstallPath` | `"."` | Where to extract update packages |

---

## Full setup: with event listeners

If you want to see download progress, handle errors, or add logging, attach event listeners:

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
    InstallPath = AppDomain.CurrentDomain.BaseDirectory,  // Use this in production
};

await new GeneralUpdateBootstrap()
    .SetConfig(config)
    .AddListenerUpdateInfo((_, e) =>
    {
        // Fires when update info is available
        Console.WriteLine($"Found {e.Info?.Body?.Count ?? 0} versions");
    })
    .AddListenerMultiDownloadStatistics((_, e) =>
    {
        // Download progress (bind to your progress bar)
        Console.WriteLine($"Progress: {e.ProgressPercentage}% | Speed: {e.Speed}");
    })
    .AddListenerMultiDownloadCompleted((_, e) =>
    {
        Console.WriteLine($"Version {e.Version} downloaded");
    })
    .AddListenerMultiDownloadError((_, e) =>
    {
        Console.WriteLine($"Download failed: {e.Exception?.Message}");
    })
    .AddListenerMultiAllDownloadCompleted((_, e) =>
    {
        Console.WriteLine("All downloads complete, starting update");
    })
    .AddListenerException((_, e) =>
    {
        Console.WriteLine($"Error: {e.Message}");
    })
    .AddListenerProgress((_, e) =>
    {
        // v10.5.0-beta.7 new — the 7th event
    })
    .LaunchAsync();
```

> **You usually only need 3 events**: `MultiDownloadStatistics` (progress bar), `MultiDownloadError` (download failed), `Exception` (errors). Add others as needed.

---

## UpdateRequest full reference

Beyond the 6 required fields, these optional settings are available:

```csharp
var config = new UpdateRequest
{
    // === Required ===
    UpdateUrl = "...", AppSecretKey = "...", MainAppName = "...",
    ClientVersion = "1.0.0.0", ProductId = "...", InstallPath = ".",

    // === Optional: Authentication ===
    AuthScheme = AuthScheme.Hmac,         // Hmac / Bearer / ApiKey / Basic
    Token = "your-token",
    BasicUsername = "user",
    BasicPassword = "pass",

    // === Optional: Files to exclude during backup ===
    Files = new List<string> { "*.log", "*.tmp" },
    Formats = new List<string> { ".pdb" },
    Directories = new List<string> { "logs", "cache" },
};
```

You can also use the **builder pattern** (same result, more fluent syntax):

```csharp
var config = UpdateRequestBuilder.Create()
    .SetUpdateUrl("https://your-server.com/api")
    .SetAppSecretKey("your-secret-key")
    .SetMainAppName("MyApp.exe")
    .SetClientVersion("1.0.0.0")
    .SetProductId("my-product-001")
    .SetInstallPath(".")
    .Build();
```

Or use **zero-config mode** (auto-discovers from manifest.json):

```csharp
await new GeneralUpdateBootstrap()
    .SetSource(
        updateUrl: "https://your-server.com/api",
        appSecretKey: "your-secret-key")
    .AddListenerUpdateInfo(...)
    .LaunchAsync();
```

---

## 4 update scenarios (based on server response)

GeneralUpdate auto-selects its action based on what the server returns:

| Scenario | What happens |
|----------|-------------|
| **None** | No update → launch main program |
| **UpgradeOnly** | Update upgrade app only → extract and overwrite |
| **MainOnly** | Update main app only → download → IPC → Upgrade replaces files |
| **Both** | Update both |

---

## Upgrade process configuration

The Upgrade (installer) doesn't need configuration — it reads from the IPC file automatically:

```csharp
using GeneralUpdate.Core;

await new GeneralUpdateBootstrap()
    .AddListenerException((_, e) =>
        Console.WriteLine($"Error: {e.Message}"))
    .LaunchAsync();
```

> Your project needs **two separate projects**: one Client (main app) and one Upgrade (installer).

---

## Deployment checklist

Your publish directory should look like:

```
publish/
├── MyApp.exe                         ← Your main program
├── generalupdate.manifest.json       ← Update manifest
└── update/
    └── UpgradeApp.exe                ← Upgrade program (must exist from v1.0!)
```

The most common first-time mistake is **forgetting to include UpgradeApp.exe**, which makes updates impossible to execute.

---

## App type reference

`AppType` distinguishes the two processes:

| Value | Enum | Who uses it |
|-------|------|-------------|
| 1 | `AppType.Client` | Main program (your business code) |
| 2 | `AppType.Upgrade` | Upgrade program (UpgradeApp.exe) |
| 3 | `AppType.OssClient` | OSS main program |
| 4 | `AppType.OssUpgrade` | OSS upgrade program |

---

## Common beginner mistakes

| # | Mistake | Consequence | Correct approach |
|---|---------|-------------|------------------|
| 1 | Client and Upgrade use **different** NuGet versions | `Method not found` at runtime | Both projects use the **exact same** version |
| 2 | Forgot to include UpgradeApp.exe | Update detected but can't execute | Include it in `update/` from the first release |
| 3 | IPC encoding not set to UTF-8 | Garbled text on Linux/macOS | Set `Encoding.UTF8` |
| 4 | Version not in 4-part format (e.g. `1.0`) | Version comparison breaks | Always use `1.0.0.0` format |
| 5 | manifest.json exe name doesn't match | Can't find main program after update | Match the actual filename exactly |

---

## Next steps

- Need an update UI → [generalupdate-ui](generalupdate-ui)
- Choosing an update strategy → [generalupdate-strategy](generalupdate-strategy)
- Need crash daemon / advanced features → [generalupdate-advanced](generalupdate-advanced)
- Running into issues → [generalupdate-troubleshoot](generalupdate-troubleshoot)
