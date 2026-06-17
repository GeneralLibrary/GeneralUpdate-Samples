---
sidebar_position: 4
sidebar_label: ⚙️ Strategy
title: ⚙️ generalupdate-strategy — Update Strategy Guide
---

# ⚙️ GeneralUpdate Strategy Complete Guide

> ⚠️ **Targeting NuGet v10.5.0-beta.4**. This version uses `UpdateRequest` configuration with support for the programmable `Option` system.

---

## 📋 User Requirements Gathering

```
### Deployment Environment
- Has backend service: ______ (Yes/No/Planned)
- Server type: ______ (GeneralSpacestation / Custom API / S3/MinIO / None)
- Number of clients: ______ (Tens/Hundreds/Thousands/10k+)
- Clients run 24/7: ______ (Yes/No)

### Update Requirements
- Need bandwidth savings: ______ (Yes/No → recommend Differential)
- Need to skip intermediate versions: ______ (Yes/No → recommend CVP)
- Need server-initiated updates: ______ (Yes/No → recommend SignalR)
- Need user-transparent updates: ______ (Yes/No → recommend Silent)
- Need to show update progress: ______ (Yes/No → recommend Standard + UI)

### Constraints
- Target platform: ______ (Windows/Linux/macOS/Multi-platform)
- Network environment: ______ (Intranet/Internet/Offline)
- Need crash recovery: ______ (Yes/No → pair with Bowl)
```

---

## Detailed Strategy Decision Tree

```
Does your app have a backend service?
├── Yes
│   ├── Need server-push updates?
│   │   └── YES → ⑥ SignalR Push (needs additional SignalR Hub)
│   └── NO
│       ├── Need bandwidth savings?
│       │   ├── YES → ④ Differential Update (patches, 60-90% smaller)
│       │   └── NO
│       │       ├── Need to skip intermediate versions?
│       │       │   ├── YES → ⑤ Cross Version CVP (server-side builds)
│       │       │   └── NO
│       │       │       └── ① Standard Client-Server (recommended for beginners)
│       └── Need silent background updates?
│           └── YES → ③ Silent Update (based on Standard or OSS + polling)
│
└── No (only object storage S3/MinIO)
    ├── Need bandwidth savings?
    │   ├── YES → ④ Differential Update (OSS + diff patches)
    │   └── NO
    │       └── ② OSS Standard (lowest cost, zero server)
    │
    └── Need silent background updates?
        └── YES → ③ Silent Update (OSS + periodic check)

### Mixed Strategy Combinations

| Scenario | Strategy Combo | Description |
|----------|---------------|-------------|
| Standard web app | ① Standard + 🎨 UI | Has backend, show progress |
| No server, save bandwidth | ② OSS + ④ Differential | Zero server + incremental updates |
| Long-running background service | ③ Silent (based on ① or ②) | User transparent |
| Forced upgrade | ⑤ CVP + ⑥ SignalR | Skip old versions, push proactively |
| Enterprise high-reliability | ① Standard + Bowl + ③ Silent | Full chain |
```

---

## 6 Strategy Comparison

| Strategy | Server | Description |
|----------|:------:|-------------|
| **① Standard Client-Server** | ✅ GeneralSpacestation | Medium/large apps with backend (recommended starting point) |
| **② OSS Object Storage** | ❌ S3/MinIO only | No backend, lowest cost |
| **③ Silent Update** | ✅ Same as ① or ② | Background silent upgrade |
| **④ Differential Update** | ✅ Needs differential builds | Incremental patches to save bandwidth |
| **⑤ Cross Version (CVP)** | ✅ Needs CVP builds | Skip intermediate versions |
| **⑥ SignalR Push** | ✅ Needs SignalR Hub | Server-initiated updates |

---

## Integration Code

All strategies use the same configuration pattern:

```csharp
using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;

var config = new UpdateRequest
{
    UpdateUrl = "https://your-server.com/api",
    AppSecretKey = "your-secret-key",
    MainAppName = "MyApp.exe",
    ClientVersion = "1.0.0.0",
    ProductId = "my-product-001",
    InstallPath = ".",
};

await new GeneralUpdateBootstrap()
    .SetConfig(config)
    .AddListener*(...)
    .LaunchAsync();
```

Or use the zero-config `SetSource()` API:

```csharp
await new GeneralUpdateBootstrap()
    .SetSource(
        updateUrl: "https://your-server.com/api",
        appSecretKey: "your-secret-key")
    .AddListenerUpdateInfo(...)
    .LaunchAsync();
```

See example files in the `examples/` directory for each strategy.

---

## Platform-Specific Differences

| Platform | Notes |
|----------|-------|
| **Windows** | Full functionality |
| **Linux** | Partial functionality (no Bowl) |
| **macOS** | Same as Linux |

---

## Known Issues

| # | Issue | Workaround |
|---|-------|------------|
| 1 | OSS mode doesn't distinguish Main/Upgrade updates | Accept this behavior |
| 2 | UpgradeApp.exe must be in `update/` subdirectory | Deploy as specified |
| 3 | NuGet version mismatch causes "Method not found" | Use same version for Client and Upgrade |
| 4 | Infinite update loop | Ensure manifest.json version is correct |
| 5 | SignalR HubConnection crashes after Dispose | Set connection to null when disposing |

---

## ✅ Strategy Verification Checklist

### Strategy Fit
- [ ] Strategy matches deployment environment (backend→Standard/no backend→OSS)
- [ ] Bandwidth requirement matches strategy (large files→Diff, many versions→CVP)
- [ ] UX goals match strategy (interactive→Standard+UI, background→Silent)
- [ ] Platform compatibility confirmed (Linux/macOS doesn't support Bowl)

### OSS Strategy
- [ ] Bucket permissions set to private
- [ ] Update package URLs public or use pre-signed URLs
- [ ] Upgrade.exe placed in `update/` subdirectory (OSS-specific requirement)
- [ ] No separate Main/Upgrade package distinction (OSS limitation, accept)

### Silent Strategy
- [ ] Poll interval is reasonable (30-60 min recommended)
- [ ] System notification or tray icon for "new version available"
- [ ] Notify user to restart AFTER download, not before
- [ ] Bandwidth/battery optimization (download large packages on WiFi only)

### SignalR Push
- [ ] HubConnection lifecycle management complete
- [ ] Reconnection logic (auto-retry 3 times, increasing interval)
- [ ] Set HubConnection to null on Dispose (otherwise reconnection crash)
- [ ] Push messages have timeout protection and fallback strategy

### Differential Strategy
- [ ] Server-side patch generation mechanism (`DifferentialCore.CleanAsync`)
- [ ] Client Pipeline configured with PatchMiddleware
- [ ] Large file integer overflow risk noted (fixed in v10.4.6+)
- [ ] BSDIFF patch compatibility verified on Linux/macOS

---

## ⚠️ Anti-Pattern Checklist

| # | Anti-Pattern | Consequence | Correct Approach |
|---|-------------|-------------|------------------|
| 1 | **Choosing OSS when you have a backend** | Wasting backend capability, lose version management | Have backend → Standard strategy |
| 2 | **Low-frequency polling (once a day)** | Users wait too long for updates | Silent mode 30-60 min polling |
| 3 | **High-frequency polling (every minute)** | Wastes bandwidth and battery | Silent mode ≥ 30 min recommended |
| 4 | **Never disposing SignalR connection** | Memory leak | Dispose HubConnection on app/page close |
| 5 | **Diff patch too large (> 2GB)** | Integer overflow causes process crash | Split across multiple releases, or use full package |
| 6 | **CVP skipping versions without testing API changes** | Client data migration failure | Test version compatibility server-side |
| 7 | **OSS package name without version number** | Client version comparison logic exception | Name format `MyApp_1.0.0.0.zip` |
| 8 | **Not notifying user to restart after silent update** | User doesn't know new version is ready | Notify after download + delayed restart option |

---

## Related Skills

- `/generalupdate-init` — Bootstrap configuration
- `/generalupdate-ui` — Update UI generation
- `/generalupdate-troubleshoot` — Issue diagnosis
- `/generalupdate-advanced` — Advanced customization
