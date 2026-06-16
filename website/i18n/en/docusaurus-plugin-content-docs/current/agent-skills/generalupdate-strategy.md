---
sidebar_position: 4
sidebar_label: ⚙️ Strategy
title: ⚙️ generalupdate-strategy — Update Strategy Guide
---

# ⚙️ GeneralUpdate Strategy Complete Guide

> ⚠️ **Targeting NuGet v10.4.6 stable**. This version uses `Configinfo` configuration, with no programmatic `Option` system.

---

## Strategy Decision Tree

```
Does your app have a backend service?
├── Yes
│   ├── Need server-push updates? → SignalR Push
│   └── No → Standard Client-Server
│
└── No (only object storage S3/MinIO/OSS)
    └── OSS Standard
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

## Mixed Strategy Combinations

| Combination | Typical Scenario |
|-------------|------------------|
| **OSS + Silent** | Game launcher / kiosk — pulls from CDN, no user interaction |
| **Standard + Differential** | Full download on first update, patches thereafter |
| **CVP + Differential** | Cross-version jump as a single binary patch |
| **Standard + Push** | Backend pushes notification, user clicks to download |

---

## Platform Differences

| Platform | Notes |
|----------|-------|
| **Windows** | Full functionality. **Bowl** crash daemon (restarts app bus on unhandled exception). Full filesystem access. |
| **Linux / macOS** | ⚠️ v10.4.6 has no `UnixPermissionHooks`, need manual `chmod +x`. Target .NET 8+. |

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

## Related Skills

- [🚀 generalupdate-init — Bootstrap configuration](./generalupdate-init.md)
- [🎨 generalupdate-ui — Update UI generation](./generalupdate-ui.md)
- [🩺 generalupdate-troubleshoot — Issue diagnosis](./generalupdate-troubleshoot.md)
- [🔧 generalupdate-advanced — Advanced customization](./generalupdate-advanced.md)
