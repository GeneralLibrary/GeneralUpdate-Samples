---
sidebar_position: 1
sidebar_label: 📋 Overview
title: 🚀 Agent Skills Overview
---

# 🚀 GeneralUpdate Agent Skills Suite

**Claude Code Skill Suite** — Helps .NET developers integrate the [GeneralUpdate](https://github.com/GeneralLibrary/GeneralUpdate) auto-update system into any .NET application in 5 minutes.

Covers 50+ real-world issues discovered through GitHub/Gitee, providing production-ready code generation + deep troubleshooting.

> **Current Version: 0.0.2-beta.1** — targets NuGet `GeneralUpdate.Core 10.5.0-beta.4`  
> Compatibility: `v10.5.0-beta.4` (latest NuGet preview release)  
> All templates have passed `dotnet build` verification (0 errors).

---

## Installing Skills

### Get the Skill Files

All Skill files are located in the `GeneralUpdate-Samples` repository under the `tmpgeneralupdate-skill-codegen` directory:

```
samples-repo/
└── src/Hub/Samples/
    └── tmpgeneralupdate-skill-codegen/
        ├── SKILL.md                          ← Suite entry point (required)
        └── .claude/skills/
            ├── generalupdate-init/           ← 🚀 Integration Guide
            ├── generalupdate-ui/             ← 🎨 UI Generation
            ├── generalupdate-strategy/       ← ⚙️ Strategy Guide
            ├── generalupdate-advanced/       ← 🔧 Advanced Customization
            ├── generalupdate-troubleshoot/   ← 🩺 Troubleshooting
            ├── generalupdate-migration/      ← 🔄 Migration Guide
            └── generalupdate-security-audit/ ← 🔒 Security Audit
```

### Install into Claude Code

**Option 1 (Recommended)**: Clone the entire skill suite into your project's `.claude` directory:

```bash
# Run in your project root
mkdir -p .claude/skills
cp -r <samples-path>/src/Hub/Samples/tmpgeneralupdate-skill-codegen/.claude/skills/* .claude/skills/
cp <samples-path>/src/Hub/Samples/tmpgeneralupdate-skill-codegen/SKILL.md .claude/
```

**Option 2**: If you already have a `.claude` directory, just copy the skills subdirectory:

```bash
# Run in your project root
cp -r <samples-path>/src/Hub/Samples/tmpgeneralupdate-skill-codegen/.claude/skills .claude/
```

### Verify Installation

Start Claude Code and type `/generalupdate-init`. If you see the integration guide output, installation was successful. You can also let Skills auto-activate by describing your needs, for example:

> *"Add auto-update to my WPF application"*

---

## Prerequisites

1. **Claude Code**: Install and configure [Claude Code CLI](https://claude.com/claude-code)
2. **.NET SDK**: Target project must be .NET 8+ (.NET 10 recommended)
3. **GeneralUpdate Server**: For standard strategies, deploy [GeneralSpacestation](https://github.com/JusterZhu/GeneralSpacestation) or a compatible backend service
4. **Dual-Process Architecture**: Understand the Client + Upgrade dual-process concept

---

## 🧭 Developer Integration Roadmap

**Which situation are you in? Find your entry point and follow the steps:**

| Your Scenario | Start Here | What to Do | Next Step After |
|--------------|------------|------------|-----------------|
| 🆕 **First time, starting from scratch** | `/generalupdate-init` | ① Pick integration mode → ② Generate Bootstrap → ③ Deploy | `/generalupdate-ui` (add UI) |
| 🎨 **Already integrated, need UI** | `/generalupdate-ui` | ① Auto-detect framework → ② Generate window → ③ Bridge events | `/generalupdate-strategy` (pick strategy) |
| ⚙️ **Need a strategy (OSS/Silent/Diff)** | `/generalupdate-strategy` | ① Decision tree → ② Configure server → ③ Example code | `/generalupdate-init` (config Bootstrap) |
| 🔧 **Need advanced (Bowl/IPC/Hooks)** | `/generalupdate-advanced` | ① Pick extension → ② Generate template code → ③ Integrate | Deploy & verify |
| 🩺 **Update failing/errors/exceptions** | `/generalupdate-troubleshoot` | ① Collect symptoms → ② Match known issues → ③ Fix | Return to relevant skill |
| 🔄 **Have v9.x, need to migrate to v10** | `/generalupdate-migration` | Follow migration paths + API comparison table | `/generalupdate-troubleshoot` (check issues) |
| 🔒 **Need security audit** | `/generalupdate-security-audit` | 14 security checks + audit report | `/generalupdate-init` (fix findings) |

---

## Skills Overview

| Skill | Command | Description | Coverage |
|-------|---------|-------------|----------|
| 🚀 **generalupdate-init** | `/generalupdate-init` | Dual-project scaffold + Bootstrap config (4 methods) | 4 update scenes + 4 config modes + full API |
| 🎨 **generalupdate-ui** | `/generalupdate-ui` | Auto-detect UI framework, generate full-state update window (11 states) | 6 UI frameworks + full state machine + bridge code |
| ⚙️ **generalupdate-strategy** | `/generalupdate-strategy` | 6 strategy decision tree + mixed combinations + platform differences | 6 strategies + 4 combinations + platform reference |
| 🔧 **generalupdate-advanced** | `/generalupdate-advanced` | 10+ extension points + 4 IPC + Bowl + AOT | 10+ extension points + architecture guide |
| 🩺 **generalupdate-troubleshoot** | `/generalupdate-troubleshoot` | 50+ known issue diagnosis + BM25 search engine | 8 Critical + 11 High + 20 Medium + 12 Low |
| 🔄 **generalupdate-migration** | `/generalupdate-migration` | v9.x → v10 / dev-branch → stable migration | 2 migration paths + API comparison table |
| 🔒 **generalupdate-security-audit** | `/generalupdate-security-audit` | Security audit + remediation | 14 security matrix + audit report template |

---

## Quick Start

In Claude Code, just describe your needs and the corresponding Skill will auto-activate:

```
"Add auto-update to my WPF application"
→ Auto-activates generalupdate-init + generalupdate-ui

"Update succeeded but app crashes on startup"
→ Auto-activates generalupdate-troubleshoot

"Configure OSS silent update"
→ Auto-activates generalupdate-strategy

"Add Bowl crash daemon + custom Hooks"
→ Auto-activates generalupdate-advanced

"Migrate my v9.x project to v10"
→ Auto-activates generalupdate-migration
```

---

## Generic Integration Verification Checklist

Regardless of which skill you use, check these items after integration:

### Bootstrap Configuration
- [ ] All 6 required `UpdateRequest` fields are set (UpdateUrl, AppSecretKey, MainAppName, ClientVersion, ProductId, InstallPath)
- [ ] `UpdateUrl` points to a server API that returns valid version info
- [ ] `AppSecretKey` length ≥ 16 characters, consistent with server
- [ ] `AppType` is set correctly (Client = 1, Upgrade = 2)

### NuGet & Build
- [ ] Client and Upgrade projects use the **exact same** GeneralUpdate NuGet version
- [ ] Project builds with `dotnet build` (0 errors)

### Deployment Structure
- [ ] UpgradeApp.exe exists in the publish directory (required from first release)
- [ ] `generalupdate.manifest.json` has `UpdateAppName` including `.exe`
- [ ] IPC file (`UpdateInfo.msg`) path is consistent between Client/Upgrade
- [ ] `Encoding` is set to `Encoding.UTF8` (prevents garbled text on Linux/macOS)

---

## Data Sources

All skill content is based on real-world data:

- **GitHub Issues**: #308–#517 (refactoring, bugs, features, tests)
- **Gitee Issues**: 30 real user feedback (Chinese community pain points)
- **Full Code Audit**: 17 CRITICAL/HIGH + 14 MEDIUM + 10 INFO findings
- **Samples Source Code**: CompleteUpdateSample, SilentUpdateSample, OssSample, DifferentialSample, PushSample, BowlSample, ExtensionSample, CompressSample, ImDiskQuickInstallSample
- **UI Samples**: SemiUrsa, LayUI, AntdUI, WPFDevelopers, MauiUpdate, AndroidUpdate

---

## Skill File Structure

```
.claude/skills/
├── generalupdate-init/         (7 files)
│   ├── SKILL.md
│   ├── reference.md
│   └── templates/
│       ├── MinimalIntegration.cs
│       ├── FullIntegration.cs
│       ├── generalupdate.manifest.json
│       └── project-scaffold/
│
├── generalupdate-ui/           (10 files)
│   ├── SKILL.md
│   └── templates/
│       ├── RealDownloadService.cs
│       ├── DownloadViewModels.cs
│       ├── SemiUrsaClientView.axaml / SemiUrsaUpgradeView.axaml
│       ├── LayUIStyle.xaml / WPFDevelopersStyle.xaml
│       ├── AntdUIStyle.cs
│       └── MauiUpdatePage.xaml/.cs
│
├── generalupdate-strategy/     (7 files)
│   ├── SKILL.md
│   └── examples/
│       ├── ClientServerStrategy.cs / OssStrategy.cs
│       ├── SilentStrategy.cs / DifferentialStrategy.cs
│       └── CrossVersionStrategy.cs / PushStrategy.cs
│
├── generalupdate-advanced/     (6 files)
│   ├── SKILL.md
│   ├── reference.md
│   └── templates/
│       ├── CustomHooks.cs / CustomStrategy.cs
│       ├── BowlIntegration.cs / NamedPipeIPC.cs
│
├── generalupdate-troubleshoot/ (5+ files)
│   ├── SKILL.md
│   ├── reference.md            ← 50+ symptom checklist (C/H/M/L levels)
│   ├── scripts/search.py       ← BM25 search engine
│   ├── scripts/core.py         ← BM25 algorithm core
│   └── data/issues.csv         ← 51 known issues database
│
├── generalupdate-migration/    (1 file)
│   └── SKILL.md                ← v9.x→v10 / dev-branch→stable migration
│
└── generalupdate-security-audit/ (1 file)
    └── SKILL.md                ← 14 security audit matrix
```

---

## API Compatibility Notes

> ⚠️ **NuGet Reference Rules (v10.5.0-beta.4)**:
> - Core only: `dotnet add package GeneralUpdate.Core --version 10.5.0-beta.4`
> - With Bowl: reference **both** `GeneralUpdate.Core` and `GeneralUpdate.Bowl` (no type conflicts in v10.5.0-beta.4)
> - Differential is embedded in Core, **no need** for a separate `GeneralUpdate.Differential` reference

> ⚠️ **API Surface**: v10.5.0-beta.4 adopts a completely new configuration system:
> - ✅ `UpdateRequest` / `UpdateRequestBuilder` — replaces old Configinfo
> - ✅ `SetSource(updateUrl, appSecretKey)` — zero-config entry point
> - ✅ `SetOption<T>(Option<T>, T)` — programmable configuration system
> - ✅ `IUpdateHooks` — lifecycle hooks (`Hooks<T>()`)
> - ✅ `IStrategy` — replaceable strategy interface (`Strategy<T>()`)
> - ✅ `UseDiffPipeline(Action<DiffPipelineBuilder>)` — differential pipeline config
> - ✅ `SilentPollOrchestrator` — silent polling
> - ✅ `AddListenerProgress` — 7th event listener
> - ❌ `Configinfo` class has been removed

---

## Feedback & Suggestions

If you encounter any issues or have suggestions for improvement, please submit an Issue:

- **GitHub Issues**: [GeneralUpdate-Samples/issues](https://github.com/GeneralLibrary/GeneralUpdate-Samples/issues) — Report bugs, request features
- **GeneralUpdate Issues**: [GeneralUpdate/issues](https://github.com/GeneralLibrary/GeneralUpdate/issues) — Core library bugs and feature requests

For faster resolution, please include:
- GeneralUpdate version (e.g. v10.5.0-beta.4)
- Platform (Windows / Linux / macOS)
- Update strategy (Standard / OSS / Silent / Differential / Push)
- Full error log (available from `Logs/generalupdate-trace-*.log`)

---

## Version History

### 0.0.2-beta.1 — 2026-06-16

Updated for GeneralUpdate v10.5.0-beta.4 API:
- Configinfo → UpdateRequest (namespace: `GeneralUpdate.Core.Configuration`)
- Event args moved to `GeneralUpdate.Core.Download` and `GeneralUpdate.Core.Event`
- Added SetSource(), SetOption(), `Hooks<T>()`, `Strategy<T>()` API coverage
- Updated all strategy examples to use the new API
- Updated CustomHooks.cs and CustomStrategy.cs to show v10.5 capabilities
- Fixed IsComplated → IsCompleted
- NuGet version bumped to `10.5.0-beta.4`
- Added 2 new skills: generalupdate-migration, generalupdate-security-audit

### 0.0.1-beta.1 — 2026-06-16

Initial beta release. All templates rewritten for NuGet v10.4.6 stable API.

## Related Projects

- [GeneralUpdate](https://github.com/GeneralLibrary/GeneralUpdate) — .NET auto-update core library
- [GeneralSpacestation](https://github.com/JusterZhu/GeneralSpacestation) — Update server
- [GeneralUpdate-Samples](https://github.com/GeneralLibrary/GeneralUpdate-Samples) — Sample projects
