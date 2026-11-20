---
sidebar_position: 3
title: Beginner cookbook
---

# GeneralUpdate beginner cookbook

This cookbook is for first-time GeneralUpdate users. The goal is not to explain every API at once, but to take you from zero to a complete loop: write Client → write Upgrade → generate config with Tools → start Server → one command to verify the full "discover → download → apply → return to new version" flow.

:::info What You'll Learn
After completing this cookbook, you'll have a **working end-to-end auto-update system**: a Client that checks for updates, an Upgrade process that applies them, an auto-generated manifest, and a local test Server.
:::

<iframe
  src="//player.bilibili.com/player.html?bvid=BV1EgEP6oExY&page=1"
  width="100%"
  height="480"
  style={{ borderRadius: '8px', border: 'none' }}
  allowFullScreen
  scrolling="no"
/>

## Update flow overview

```
┌──────────────────────────────────────────────────────────────────────┐
│                  GeneralUpdate Complete Update Flow                  │
└──────────────────────────────────────────────────────────────────────┘

  ① Version Check         ② Download             ③ Apply
  ┌──────────┐           ┌──────────┐           ┌──────────┐
  │  Client  │──POST──→  │  Server  │           │ Upgrade  │
  │ (Main App)│←─JSON───  │(Update   │           │ (Upgrade │
  │          │           │ Service) │           │  Process) │
  └────┬─────┘           └────┬─────┘           └────┬─────┘
       │                      │                      │
       │  POST /Upgrade/Verification               │                      │
       │  {version, platform} │                      │
       │ ────────────────────→│                      │
       │                      │                      │
       │  [{version, url,     │                      │
       │    hash, size}]      │                      │
       │ ←────────────────────│                      │
       │                      │                      │
       │  GET /File/Download  │                      │
       │ ────────────────────→│                      │
       │                      │                      │
       │  .zip packages       │                      │
       │ ←────────────────────│                      │
       │                      │                      │
       │  [Write IPC contract]│                      │
       │  [Launch Upgrade.exe]│                      │
       │ ────────────────────────────────────────────→│
       │                      │                      │
       │                      │    [Read IPC data]   │
       │                      │    [Verify Hash]     │
       │                      │    [Extract → Apply] │
       │                      │    [Launch new Client]│
       │                      │                      │
       │ ←────────────────────────────────────────────│
       │  [New version running ✓]                    │
       │                      │                      │
```

| Role | Location in project | Responsibility |
| --- | --- | --- |
| Client | `MyApp.exe` (your main app) | Check updates → download packages → launch Upgrade |
| Upgrade | `MyApp.Upgrade.exe` (standalone upgrade process) | Verify hash → extract & overwrite → launch new Client |
| Server | `GeneralUpdate-Samples/src/Server` | Return version info, accept reports, serve package downloads |
| Tools | GeneralUpdate.Tools | Generate `generalupdate.manifest.json`, wire Client and Upgrade together |

---

## Phase 1: Environment setup

### Prerequisites

| Item | Requirement | Verify |
| --- | --- | --- |
| .NET SDK | 8.0+ (10.0 recommended) | `dotnet --version` |
| Git | Any version | `git --version` |

### Create projects

Open a terminal and create two console projects:

```bash
dotnet new console -n MyApp
dotnet new console -n MyApp.Upgrade

cd MyApp
dotnet add package GeneralUpdate.Core

cd ../MyApp.Upgrade
dotnet add package GeneralUpdate.Core
```

**Expected result**: Both projects have `GeneralUpdate.Core` in their NuGet references.

---

## Phase 2: Integrate Client code

> **Paste the following into `MyApp/Program.cs`. Client is responsible for: checking for updates → downloading packages → launching Upgrade.**

```csharp
using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;
using GeneralUpdate.Core.Event;

// ================================================================
// Client — main app entry point, handles update detection & download
// ================================================================

// Only the 3 secrets need to be in code. Identity fields (MainAppName,
// ClientVersion, UpdateAppName, etc.) are auto-discovered by the framework
// from generalupdate.manifest.json — no manual code required.
var request = new UpdateRequest
{
    UpdateUrl    = "http://localhost:5000/Upgrade/Verification",
    ReportUrl    = "http://localhost:5000/Upgrade/Report",
    AppSecretKey = "dfeb5833-975e-4afb-88f1-6278ee9aeff6",
};

await new GeneralUpdateBootstrap()
    .SetConfig(request)
    .SetOption(Option.AppType, AppType.Client)
    .AddListenerUpdateInfo((_, e) =>
    {
        Console.WriteLine($"Found {e.Info?.Body?.Count ?? 0} available update(s)");
        if (e.Info?.Body != null)
        {
            foreach (var v in e.Info.Body)
                Console.WriteLine($"  v{v.Version} — {v.Name} ({v.Size} bytes)");
        }
    })
    .AddListenerMultiDownloadStatistics((_, e) =>
    {
        Console.WriteLine($"\rDownload: {e.ProgressPercentage:F0}% {e.Speed}");
    })
    .AddListenerMultiDownloadCompleted((_, e) =>
    {
        Console.WriteLine();
        Console.WriteLine($"Download {(e.IsCompleted ? "✓ done" : "✗ failed")}");
    })
    .AddListenerException((_, e) =>
    {
        Console.WriteLine($"Error: {e.Exception.Message}");
    })
    .LaunchAsync();
```

### Why only 3 parameters?

`ClientStrategy` internally calls `AppMetadataDiscoverer.Discover()` during execution, which reads identity fields from `generalupdate.manifest.json` (generated by Tools in Phase 4) and fills in the `UpdateRequest` automatically:

| Field | Who handles it | Notes |
| --- | --- | --- |
| `UpdateUrl` | **You (code)** | Server verification endpoint |
| `ReportUrl` | **You (code)** | Server report endpoint |
| `AppSecretKey` | **You (code)** | Application secret |
| `MainAppName` | **Framework auto-discover** | Read from manifest |
| `ClientVersion` | **Framework auto-discover** | Read from manifest |
| `UpdateAppName` | **Framework auto-discover** | Read from manifest |
| `ProductId` | **Framework auto-discover** | Read from manifest |
| `UpdatePath` | **Framework auto-discover** | Read from manifest |
| `InstallPath` | **Default** | `AppDomain.CurrentDomain.BaseDirectory` |

If you prefer an even shorter form, use `SetSource()`:

```csharp
await new GeneralUpdateBootstrap()
    .SetSource("http://localhost:5000/Upgrade/Verification",
               "dfeb5833-975e-4afb-88f1-6278ee9aeff6",
               "http://localhost:5000/Upgrade/Report")
    .SetOption(Option.AppType, AppType.Client)
    // ... listeners
    .LaunchAsync();
```

Client does NOT apply updates directly — after download: writes IPC contract → launches Upgrade → exits itself.

---

## Phase 3: Integrate Upgrade code

> **Paste the following into `MyApp.Upgrade/Program.cs`. Upgrade is simpler than Client — no `SetConfig()` needed, all parameters are passed via encrypted IPC from Client.**

```csharp
using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;
using GeneralUpdate.Core.Event;

// ================================================================
// Upgrade — standalone upgrade process (MyApp.Upgrade.exe)
// No SetConfig() needed — params come via encrypted IPC from Client
// ================================================================

await new GeneralUpdateBootstrap()
    .SetOption(Option.AppType, AppType.Upgrade)
    .AddListenerMultiDownloadStatistics((_, e) =>
    {
        Console.WriteLine($"\rApplying: {e.ProgressPercentage:F0}%");
    })
    .AddListenerMultiDownloadCompleted((_, e) =>
    {
        Console.WriteLine();
        Console.WriteLine($"Patch {(e.IsCompleted ? "✓ done" : "✗ failed")}");
    })
    .AddListenerException((_, e) =>
    {
        Console.WriteLine($"Error: {e.Exception.Message}");
    })
    .LaunchAsync();
```

### Client vs Upgrade

| | Client | Upgrade |
| --- | --- | --- |
| `AppType` | `AppType.Client` | `AppType.Upgrade` |
| `SetConfig()` | ✅ Required — sets server URL, version, etc. | ❌ Not needed — received via IPC from Client |
| Responsibility | Check version → download → launch Upgrade | Verify hash → extract & overwrite → launch new Client |
| Started by | User | Client process |

---

## Phase 4: Generate project structure with Tools

> **With Client and Upgrade code written, use Tools' Config tab to `dotnet publish` both projects and generate `generalupdate.manifest.json`.**

### Steps

Open **GeneralUpdate.Tools** → switch to the **Config** tab:

1. **Client .csproj Path**: Click Browse and select `MyApp.csproj`
2. **Upgrade .csproj Path**: Click Browse and select `MyApp.Upgrade.csproj`
3. Click **Analyze**: Tools parses both .csproj files and auto-fills:
   - `MainAppName`: from Client project's AssemblyName (e.g. `MyApp.exe`)
   - `UpdateAppName`: from Upgrade project's AssemblyName (e.g. `MyApp.Upgrade.exe`)
   - `ClientVersion` / `UpgradeClientVersion`: defaults to `1.0.0`, adjust as needed
4. Review the auto-filled fields; confirm `UpdatePath` is `update/` (the default)
5. Click **Generate Sample**: Tools executes:
   - `dotnet publish` Client → output root
   - `dotnet publish` Upgrade → `update/` subdirectory
   - Writes `generalupdate.manifest.json` to output root

### Published directory structure

Tools can also `dotnet publish` both projects and assemble the output for you (via the Config tab's Sample Publisher). The final structure:

```
publish/
├── MyApp.exe                   # Client publish output (Phase 2)
├── MyApp.dll
├── GeneralUpdate.Core.dll
├── generalupdate.manifest.json # Generated by Tools, placed at root, auto-discovered
└── update/                     # Upgrade publish output (Phase 3), subdirectory name from manifest's updatePath
    ├── MyApp.Upgrade.exe
    └── MyApp.Upgrade.dll
```

> **Note**: `MyApp.Upgrade.exe` lives in the `update/` subdirectory — not at the root. The framework locates and launches it based on the manifest's `updatePath` field (default `"update/"`).

:::warning Wrong Directory Structure Breaks the Update
The Upgrade process (.exe) **must** be placed in the subdirectory specified by manifest's `updatePath`. If Upgrade is not at that location, the Client cannot find and launch the upgrade process, breaking the entire update chain. Common mistake: putting Upgrade in the root directory or a differently-named subdirectory.
:::

### `generalupdate.manifest.json` example

```json
{
  "mainAppName": "MyApp.exe",
  "clientVersion": "1.0.0.0",
  "updateAppName": "MyApp.Upgrade.exe",
  "upgradeClientVersion": "1.0.0.0",
  "appType": "Client",
  "productId": "2d974e2a-31e6-4887-9bb1-b4689e98c77a",
  "updatePath": "update/"
}
```

> **Note: manifest stores identity/structure only — never secrets.** `UpdateUrl`, `ReportUrl`, and `AppSecretKey` are always set in code, via Phase 2's `request.XXX = "..."` assignments.

---

## Phase 5: Start the Server

> **Client / Upgrade / manifest are all ready — start the Server for end-to-end verification.**

```bash
git clone https://github.com/GeneralLibrary/GeneralUpdate-Samples.git
cd GeneralUpdate-Samples/src/Server
dotnet run
```

Expected output:

```
╔══════════════════════════════════════════════════╗
║     GeneralUpdate Sample Upgrade Server          ║
╠══════════════════════════════════════════════════╣
║  Verification: http://localhost:5000/Upgrade/Verification
║  Report:       http://localhost:5000/Upgrade/Report
║  Download:     http://localhost:5000/File/Download/{hash}
║  Packages:     N version(s) loaded
╚══════════════════════════════════════════════════╝
```

### Server endpoints

| Endpoint | Method | Purpose |
| --- | --- | --- |
| `/Upgrade/Verification` | POST | Client reports current version → Server returns available updates |
| `/Upgrade/Report` | POST | Client reports update result (success / failure) |
| `/File/Download/{hash}` | GET | Download package (supports HTTP Range for resume) |

The Server reads package metadata from `wwwroot/packages/versions.json` and serves them.

**Quick check**:

```bash
curl -X POST http://localhost:5000/Upgrade/Verification \
  -H "Content-Type: application/json" \
  -d '{"Version":"1.0.0.0"}'
```

Should return a JSON response with available update versions.

---

## Phase 6: End-to-end verification

```
# Terminal 1 — ensure Server is running
cd GeneralUpdate-Samples/src/Server && dotnet run

# Terminal 2 — run Client
cd MyApp && dotnet run
```

### Expected complete output

```
Client:
  [UpdateInfo] Found 1 available update(s)
    v2.0.0.0 — client_1.0.0_to_2.0.0 (xxxxx bytes)
  Download: 45% 1.2MB/s
  Download: 100%
  Download ✓ done
  → Launching MyApp.Upgrade.exe...

Upgrade:
  Applying: 50%
  Applying: 100%
  Patch ✓ done
  → Launching MyApp.exe (new version)...

Client (new version):
  MyApp v2.0.0.0 ✓
```

### Troubleshooting

| Symptom | Check |
| --- | --- |
| Client can't reach Server | `curl -X POST http://localhost:5000/Upgrade/Verification` to confirm Server is running |
| "Already up to date" response | Make sure `versions.json` has a version higher than `ClientVersion` |
| Download finishes but Upgrade doesn't start | Is `MyApp.Upgrade.exe` in the `update/` subdirectory? Is manifest's `updatePath` correct? |
| Upgrade crashes | Check directory write permissions, antivirus interference |
| Port already in use | Change Server args to `--Urls http://0.0.0.0:5001` and update Client's `UpdateUrl` |

:::tip Troubleshooting Checklist
Use this table to diagnose common issues quickly. Most failures trace back to one of these five checks.
:::

---

## Next steps

After this flow works, read these pages in order:

1. [GeneralUpdate.Core](../doc/GeneralUpdate.Core): update strategies, event notifications, silent updates, and manifest-based minimal configuration
2. [GeneralUpdate.Differential](../doc/GeneralUpdate.Differential): differential algorithms, parallel processing, and Clean/Dirty
3. [GeneralUpdate.Bowl](../doc/GeneralUpdate.Bowl): crash monitoring, backup, and failure recovery
4. [GeneralUpdate.Tools](./GeneralUpdate.PacketTool): complete guides for patches, hashes, OSS config, and simulation

## Installation directory notes

The update process requires read/write access to the application directory. To avoid Windows UAC (User Account Control) prompts, install your application in one of the following non-system-protected directories:

| Recommended directory | Example path |
| --- | --- |
| User AppData (recommended) | `C:\Users\<username>\AppData\Local\<YourApp>\` |
| User home directory | `C:\Users\<username>\<YourApp>\` |
| Custom non-system drive | `D:\<YourApp>\` |

**Not recommended:**

- `C:\Program Files\` — requires administrator privileges to write
- `C:\Program Files (x86)\` — requires administrator privileges to write
- `C:\Windows\` — system directory, write-protected
- System drive root (e.g. `C:\`) — may trigger permission restrictions

> **Tip**: If you encounter "Access Denied" or UAC prompts during updates, check whether the application is installed in a non-recommended directory. Moving the application to a user directory usually resolves the issue.

### Encrypted Files and Packed Executables

> **Note**: Packed executables (e.g. Themida, VMProtect), obfuscated code, or encrypted binaries **cannot benefit from binary differential updates**. Packing/encryption causes the entire file to differ between versions, making diff algorithms ineffective — the patch will be the same size as a full file replacement. For differential updates, ensure your published binaries are neither packed nor encrypted.

## Sample repositories
| Repository | URL |
| --- | --- |
| Samples (Server + Hub) | [GeneralUpdate-Samples](https://github.com/GeneralLibrary/GeneralUpdate-Samples) |
| Tools (GUI config tool) | [GeneralUpdate.Tools](https://github.com/Juster/GeneralUpdate.Tools) |
| Core (NuGet package source) | [GeneralUpdate](https://github.com/GeneralLibrary/GeneralUpdate) |
