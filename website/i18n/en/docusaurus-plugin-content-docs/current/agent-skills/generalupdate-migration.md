---
sidebar_position: 7
sidebar_label: 🔄 Migration
title: 🔄 generalupdate-migration — Migration Guide
---

# 🔄 GeneralUpdate Migration Guide

Guide developers through migrating GeneralUpdate from older versions to the latest API (v10.5.0-beta.6).

> ⚠️ **Target Version: NuGet v10.5.0-beta.6**
> Dev branch (v10.5.0-beta.x) APIs differ fundamentally from stable.

---

## 📋 Pre-Migration Requirements

```
### Current State
- Current GeneralUpdate version: ______ (v9.x / v10.0-10.3 / v10.5.0-beta.x / Not sure)
- Current .NET version: ______
- UI framework: ______
- Using Bowl: ______ (Yes/No)
- Using Differential: ______ (Yes/No)

### Target State
- Target version: ______ (v10.5.0-beta.6 / Stay on dev branch)
- Need new features (Bowl/IPC replacement/AOT): ______
```

---

## Migration Paths

### Path A: v9.x → v10.5.0-beta.6

This is the biggest jump. v9.x and v10 architectures are completely different.

```
v9.x (single process, HttpClient direct)
         ↓
    Breaking Changes:
    ├── Single process → dual-process architecture (Client + Upgrade)
    ├── HttpClient direct → GeneralSpacestation server
    ├── No IPC → AES encrypted IPC file
    ├── No manifest.json → manifest required
    └── All API namespaces renamed
         ↓
v10.5.0-beta.6 (dual-process, UpdateRequest + Bootstrap)
```

**Migration steps:**

```csharp
// ❌ v9.x syntax (no longer exists)
// var updater = new GeneralUpdater("https://api/method");
// updater.Start();

// ✅ v10.5.0-beta.6 syntax
await new GeneralUpdateBootstrap()
    .SetConfig(new UpdateRequest
    {
        UpdateUrl = "https://your-server.com/Upgrade/Verification",
        AppSecretKey = "your-secret-key",
        MainAppName = "MyApp.exe",
        ClientVersion = "1.0.0.0",
        ProductId = "my-product-001",
        InstallPath = "."
    })
    .LaunchAsync();
```

| v9.x API | v10.5.0-beta.6 Equivalent | Notes |
|----------|---------------------------|-------|
| `GeneralUpdater` | `GeneralUpdateBootstrap` | Completely renamed |
| `SetApiUrl()` / `SetMethod()` | `UpdateRequest.UpdateUrl` | Unified into UpdateRequest |
| `CheckUpdateAsync()` | `.LaunchAsync()` | Returns Bootstrap instance |
| Single-process update | Client + Upgrade dual-process | Must create separate Upgrade project |
| N/A | `generalupdate.manifest.json` | Must ship with first release |

### Path B: v10.5.0-beta.x (dev branch) → v10.5.0-beta.6

| Dev Branch API (v10.5.0-beta.x) | Stable Replacement (v10.5.0-beta.6) | Handling |
|--------------------------------|-----------------------------------|----------|
| `new Option()` / `SetOption()` | Exists and compatible | Use directly, namespace adjusted |
| `.Hooks<T>()` / `IUpdateHooks` | Exists and compatible | Use directly |
| `.Strategy<T>()` / `IStrategy` | Exists and compatible | Use directly |
| `SilentPollOrchestrator` | Exists | `Option.Silent` + `SetOption()` |
| `ISslValidationPolicy` | Exists | Use directly |
| `Configinfo` | Removed | Use `UpdateRequest` instead |
| `OssClient (AppType=3,4)` | Exists | Use `AppType.OssClient` and `AppType.OssUpgrade` |
| Hardcoded version | `UpdateRequest.ClientVersion` | Use `Assembly.GetEntryAssembly()?.GetName()?.Version` |

---

## Migration Verification Checklist

### Build Verification
- [ ] `dotnet build` with no errors
- [ ] No risk of `MissingMethodException`
- [ ] No `CS0433` type conflicts (no conflict between Core + Bowl in v10.5.0-beta.6)

### Architecture Verification
- [ ] Project split into Client + Upgrade independent projects
- [ ] Upgrade project `AppType = 2`
- [ ] Client project `AppType = 1`
- [ ] `generalupdate.manifest.json` exists and configured correctly

### Runtime Verification
- [ ] Version check API returns normally
- [ ] Upgrade process starts after download
- [ ] Main app restarts after update completes
- [ ] IPC file encoding set to `Encoding.UTF8`

---

## ⚠️ Migration Anti-Patterns

| # | Anti-Pattern | Consequence | Correct Approach |
|---|-------------|-------------|------------------|
| 1 | **Updating NuGet version without modifying code** | Many build errors | Clean old API references first, then upgrade NuGet |
| 2 | **Assuming v9.x config object is UpdateRequest** | UpdateRequest property names completely different | Rewrite UpdateRequest per documentation |
| 3 | **Using dev-branch APIs on v10.4.6 stable** | MissingMethodException | Check API availability table |
| 4 | **Not testing Upgrade process after migration** | Main app updates but Upgrade crashes | Test both sides |
| 5 | **Keeping old v9.x references without deleting** | Type conflicts | Clean csproj and re-add references |

---

## Related Skills

- `/generalupdate-init` — Integration guide from scratch
- `/generalupdate-troubleshoot` — Migration issue diagnosis
- `/generalupdate-security-audit` — Post-migration security audit
