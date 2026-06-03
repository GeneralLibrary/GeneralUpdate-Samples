---
sidebar_position: 1
---

# System Architecture

## Architecture Overview

GeneralUpdate is a cross-platform .NET automatic update framework using a **Client-Upgrade-Server** three-tier architecture. The entire update process involves multiple independent processes and components working together.

---

## Overall Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                        Server                                │
│  ┌──────────────┐  ┌──────────────┐  ┌───────────────────┐  │
│  │ Verification │  │ Patch Store   │  │ UpgradeHub (Push) │  │
│  │ API          │  │ /patch/*.zip  │  │ SignalR Hub       │  │
│  └──────────────┘  └──────────────┘  └───────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                   Client Process                              │
│  ┌──────────────────────────────────────────────────────┐   │
│  │              GeneralUpdate.ClientCore                  │   │
│  │  ┌──────────┐  ┌──────────┐  ┌────────────────────┐  │   │
│  │  │Version   │  │Download  │  │Event/Precheck      │  │   │
│  │  │Check     │  │Manager   │  │Notifications       │  │   │
│  │  └──────────┘  └──────────┘  └────────────────────┘  │   │
│  └──────────────────────────────────────────────────────┘   │
│                              │                               │
│              IPC (AES encrypted parameters)                   │
│                              ▼                               │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                  Upgrade Process                              │
│  ┌──────────────────────────────────────────────────────┐   │
│  │                GeneralUpdate.Core                      │   │
│  │  ┌────────────────────────────────────────────────┐  │   │
│  │  │        Pipeline (Middleware Chain)               │  │   │
│  │  │  HashMiddleware → CompressMiddleware →         │  │   │
│  │  │  PatchMiddleware → DrivelutionMiddleware       │  │   │
│  │  └────────────────────────────────────────────────┘  │   │
│  │  ┌──────────┐  ┌──────────┐  ┌──────────────────┐  │   │
│  │  │ File     │  │ Patch    │  │ Driver Update    │  │   │
│  │  │ Replace  │  │ Apply    │  │                  │  │   │
│  │  └──────────┘  └──────────┘  └──────────────────┘  │   │
│  └──────────────────────────────────────────────────────┘   │
│                              │                               │
│         Start main app + Bowl monitoring                     │
└─────────────────────────────────────────────────────────────┘
```

---

## Component Relationships

### Core Components

| Component | NuGet Package | Process | Responsibility |
|-----------|-------------|---------|----------------|
| **GeneralUpdate.ClientCore** | `GeneralUpdate.ClientCore` | Main app | Version check, download, launch upgrade |
| **GeneralUpdate.Core** | `GeneralUpdate.Core` | Separate upgrade process | File replacement, patch apply, driver update |
| **GeneralUpdate.Differential** | `GeneralUpdate.Core` (included) | Both | Binary diff algorithms (BSDiff/HDiffPatch) |
| **GeneralUpdate.Bowl** | `GeneralUpdate.Bowl` | Separate monitor process | Crash monitoring, dump generation, diagnostics |
| **GeneralUpdate.Drivelution** | `GeneralUpdate.Drivelution` | Upgrade process | Cross-platform driver update |
| **GeneralUpdate.Extension** | `GeneralUpdate.Extension` | Main app | Plugin/extension management |
| **GeneralUpdate.Tools** | Standalone desktop app | Dev tool | Patch building, simulation, config generation |

### Component Dependencies

```
GeneralUpdate.Tools ──→ GeneralUpdate.Core (DiffPipeline)
                                 │
                    ┌────────────┼────────────┐
                    ▼            ▼            ▼
           GeneralUpdate    GeneralUpdate  GeneralUpdate
           .Differential   .Drivelution   .Extension
                    │
                    ▼
           GeneralUpdate.Core (Bootstrap, Pipeline, Download, Security)
                    │
                    ▼
           GeneralUpdate.ClientCore (Client Bootstrap, Hub Service)
                    │
                    ▼
           GeneralUpdate.Bowl (Process Monitoring)
```

---

## Update Flow

### Complete Update Sequence

```
  Client                   Server                    Upgrade
  ──────                   ──────                    ───────
     │                        │                         │
     │  1. POST /Verification│                         │
     │ ─────────────────────► │                         │
     │                        │                         │
     │  2. Return version list│                        │
     │ ◄───────────────────── │                         │
     │                        │                         │
     │  3. User confirms      │                         │
     │  (AddListenerUpdatePrecheck)                      │
     │                        │                         │
     │  4. GET /patch/v2.0.0  │                         │
     │ ─────────────────────► │                         │
     │                        │                         │
     │  5. Download patch     │                         │
     │ ◄───────────────────── │                         │
     │                        │                         │
     │  6. Exit main app      │                         │
     │                        │                         │
     │  7. Launch Upgrade (IPC)│                        │
     │ ──────────────────────────────────────────────► │
     │                        │                         │
     │                        │   8. Read IPC params    │
     │                        │   9. Verify hash        │
     │                        │   10. Decompress        │
     │                        │   11. Apply patches     │
     │                        │   12. Install drivers   │
     │                        │   13. Replace files     │
     │                        │                         │
     │                        │   14. Launch new app    │
     │ ◄────────────────────────────────────────────── │
     │                        │                         │
     │  15. POST /Report      │                         │
     │ ─────────────────────► │                         │
```

### IPC Data Flow

```
Client writes: ProcessContract → JSON serialize → AES-256-CBC encrypt → temp file
                                       ↓
Upgrade reads: temp file → AES-256-CBC decrypt → JSON deserialize → ProcessContract
```

---

## Update Modes

### Full Update

When `Patch` option is `false`:

```
Download full package → Decompress → Overwrite install
```

### Incremental/Differential Update (Default)

When `Patch` option is `true` (default):

```
Download patch → Decompress → Apply BSDiff → Delete expired files
```

**Patch package structure:**
```
patch_v1.0.1.zip
├── file1.dll.patch       ← BSDiff binary patch
├── file2.exe.patch       ← BSDiff binary patch
├── newfile.dll            ← New file (included as-is)
├── delete_files.json      ← Files to delete
└── ...
```

### OSS Update Mode

Object Storage Service based updates — no server API required:

```
Client → read version.json → compare versions → download patches → upgrade
```

---

## Platform Strategy

| Platform | Strategy Class | Special Handling |
|----------|---------------|------------------|
| Windows | `WindowsStrategy` | Bowl integration, PnPUtil driver install |
| Linux | `LinuxStrategy` | chmod permission scripts, no Bowl support |
| macOS | `MacStrategy` | chmod scripts, `ConfigureAwait(false)` |

---

## Extension Points

GeneralUpdate provides rich extensibility through `AbstractBootstrap`'s CRTP pattern:

| Extension Point | Interface | Default | Purpose |
|----------------|-----------|---------|---------|
| **Strategy** | `IStrategy` | Platform-specific | OS strategy |
| **Hooks** | `IUpdateHooks` | `NoOpUpdateHooks` | Lifecycle hooks |
| **SSL Policy** | `ISslValidationPolicy` | `StrictSslValidationPolicy` | Certificate validation |
| **Download Policy** | `IDownloadPolicy` | `DefaultDownloadPolicy` | Retry/timeout logic |
| **Download Executor** | `IDownloadExecutor` | `HttpDownloadExecutor` | Protocol executor |
| **Auth Provider** | `IHttpAuthProvider` | `NoOpAuthProvider` | HTTP authentication |
| **Download Orchestrator** | `IDownloadOrchestrator` | `DefaultDownloadOrchestrator` | Batch download |

```csharp
await new GeneralClientBootstrap()
    .SslPolicy<CustomSslPolicy>()
    .DownloadPolicy<CustomRetryPolicy>()
    .HttpAuth<BearerTokenAuthProvider>()
    .Hooks<CustomUpdateHooks>()
    .SetConfig(config)
    .LaunchAsync();
```

---

## Related Resources

- **[Pipeline Guide](./Pipeline.md)** — Middleware pipeline internals
- **[Configuration Reference](./Configuration.md)** — All configuration options
- **[Security Guide](./Security.md)** — Security architecture
- **[Quick Start](../quickstart/Quik start)** — 5-minute quickstart
