---
sidebar_position: 1
sidebar_label: 📋 Overview
title: 🚀 Agent Skills Overview
---

# 🚀 GeneralUpdate Agent Skills Suite

**Claude Code Skill Suite** — Helps .NET developers integrate the [GeneralUpdate](https://github.com/GeneralLibrary/GeneralUpdate) auto-update system into any .NET application in 5 minutes.

Covers 50+ real-world issues discovered through GitHub/Gitee, providing production-ready code generation + deep troubleshooting.

> **Current Version: 0.0.1-bate.1** — targets NuGet `GeneralUpdate.Core ≥ 10.4.6` stable release  
> Compatibility: `v10.4.6` (latest NuGet stable release)  
> All 32 template files have passed `dotnet build` verification (0 errors).

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
            └── generalupdate-troubleshoot/   ← 🩺 Troubleshooting
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

## Skills Overview

| Skill | Command | Description | Coverage |
|-------|---------|-------------|----------|
| 🚀 **generalupdate-init** | `/generalupdate-init` | Dual-project scaffold + Bootstrap config (4 methods) | 4 update scenes + 4 config modes + full API |
| 🎨 **generalupdate-ui** | `/generalupdate-ui` | Auto-detect UI framework, generate full-state update window (11 states) | 6 UI frameworks + full state machine + bridge code |
| ⚙️ **generalupdate-strategy** | `/generalupdate-strategy` | 6 strategy decision tree + mixed combinations + platform differences | 6 strategies + 4 combinations + platform reference |
| 🔧 **generalupdate-advanced** | `/generalupdate-advanced` | 10+ extension points + 4 IPC + Bowl + AOT | 10+ extension points + architecture guide |
| 🩺 **generalupdate-troubleshoot** | `/generalupdate-troubleshoot` | 50+ known issue diagnosis + 6-step universal troubleshooting | 8 Critical + 11 High + 20 Medium + 12 Low |

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
```

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
└── generalupdate-troubleshoot/ (2 files)
    ├── SKILL.md
    └── reference.md            ← 50+ symptom checklist (C/H/M/L levels)
```

---

## API Compatibility Notes

> ⚠️ **NuGet Reference Rules**:
> - Core only: `dotnet add package GeneralUpdate.Core`
> - With Bowl: reference **only** `GeneralUpdate.Bowl` (transitively includes Core — they cannot coexist)
> - Differential is embedded in Core, **no need** for a separate `GeneralUpdate.Differential` reference

> ⚠️ **API Surface**: v10.4.6 stable API differs fundamentally from the dev branch (v10.5.0-beta.2). The stable release does NOT support:
> - ❌ Programmatic `Option` config system (only `Configinfo` properties)
> - ❌ `IUpdateHooks` lifecycle hooks
> - ❌ `IStrategy` replaceable strategy interface
> - ❌ `SilentPollOrchestrator`
> - ❌ `ProcessContract` / IPC replacement interfaces

---

## Feedback & Suggestions

If you encounter any issues or have suggestions for improvement, please submit an Issue:

- **GitHub Issues**: [GeneralUpdate-Samples/issues](https://github.com/GeneralLibrary/GeneralUpdate-Samples/issues) — Report bugs, request features
- **GeneralUpdate Issues**: [GeneralUpdate/issues](https://github.com/GeneralLibrary/GeneralUpdate/issues) — Core library bugs and feature requests

For faster resolution, please include:
- GeneralUpdate version (e.g. v10.4.6)
- Platform (Windows / Linux / macOS)
- Update strategy (Standard / OSS / Silent / Differential / Push)
- Full error log (available from `%TEMP%/GeneralUpdate/logs/`)

---

## Version History

### 0.0.1-bate.1 — 2026-06-16

Initial beta release. All templates rewritten for NuGet v10.4.6 stable API.

## Related Projects

- [GeneralUpdate](https://github.com/GeneralLibrary/GeneralUpdate) — .NET auto-update core library
- [GeneralSpacestation](https://github.com/JusterZhu/GeneralSpacestation) — Update server
- [GeneralUpdate-Samples](https://github.com/GeneralLibrary/GeneralUpdate-Samples) — Sample projects
