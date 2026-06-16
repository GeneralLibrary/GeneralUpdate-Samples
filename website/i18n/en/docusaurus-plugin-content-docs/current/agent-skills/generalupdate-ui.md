---
sidebar_position: 3
sidebar_label: 🎨 UI Generation
title: 🎨 generalupdate-ui — Update UI Generation
---

# 🎨 GeneralUpdate Update UI Generation — Full State Coverage

Auto-detects the developer's UI framework and generates a complete update window with real GeneralUpdate.Core event bindings.

> ⚠️ Targeting NuGet v10.4.6 stable. `RealDownloadService.cs` is an abstract bridge template requiring manual adaptation.

---

## UI State Machine (Covered by All Templates)

```
                   ┌─────────────┐
                   │    Idle     │ ← Initial state
                   └──────┬──────┘
                          │ Auto/manual trigger
                          ▼
                   ┌─────────────┐
            ┌─────│  Checking    │ ← "Checking for updates..."
            │     └──────┬──────┘
            │            │
            │     ┌──────┴──────┐
            │     ▼             ▼
            │  ┌────────┐  ┌──────────┐
            │  │ Latest │  │  Found!  │ ← Show version/size
            │  └────────┘  └────┬─────┘
            │                   │ User clicks "Update"
            │                   ▼
            │            ┌──────────────┐
            │      ┌─────│ Downloading  │ ← Progress bar/speed/time
            │      │     └──────┬───────┘
            │      │            │
            │      │     ┌──────┴──────┐
            │      │     ▼             ▼
            │      │  ┌────────┐  ┌──────────┐
            │      │  │ Paused │  │  Error   │ ← Show error + "Retry"
            │      │  └───┬────┘  └────┬─────┘
            │      │      │ Resume      │ Retry
            │      │      ▼             ▼
            │      │  ┌──────────────┐
            │      │  │ Downloading  │
            │      │  └──────────────┘
            │      │
            │      │     ┌──────────────┐
            │      └────→│  Applying    │ ← "Installing update..."
            │             └──────┬───────┘
            │                    │
            │             ┌──────┴──────┐
            │             ▼             ▼
            │       ┌─────────┐  ┌──────────┐
            │       │ Success │  │  Failed  │
            │       └────┬────┘  └──────────┘
            │            │
            │            ▼
            │       ┌──────────┐
            │       │ Restart  │ ← Restart app
            │       └──────────┘
            │
            └── Back to Idle
```

---

## Core Bridge: RealDownloadService

All UI templates share this bridge class, mapping GeneralUpdate.Core events to the `IDownloadService` interface.

### Bridge Logic (v10.4.6 stable)

```csharp
// GeneralUpdate.Core Event → DownloadStatus State Machine Mapping:

GeneralUpdateBootstrap.AddListenerMultiDownloadStatistics
    → Downloading (update ProgressPercentage/Speed/Remaining)

GeneralUpdateBootstrap.AddListenerMultiDownloadCompleted
    → Processing files (extraction/verification)

GeneralUpdateBootstrap.AddListenerMultiAllDownloadCompleted
    → Applying → Success

GeneralUpdateBootstrap.AddListenerMultiDownloadError
    → DownloadError (after N automatic retries)

GeneralUpdateBootstrap.AddListenerException
    → Failed (non-fatal exceptions don't change state)
```

---

## UI Framework Template List

| Template File | Framework | Features |
|---------------|-----------|----------|
| `SemiUrsaClientView.axaml` + `.cs` | Avalonia + SemiUrsa | Full state machine, dark toggle, animations |
| `SemiUrsaUpgradeView.axaml` + `.cs` | Avalonia + SemiUrsa (Upgrade) | Waiting UI |
| `LayUIStyle.xaml` + `.cs` | WPF + LayUI.Wpf | Glass effect, progress bar |
| `WPFDevelopersStyle.xaml` + `.cs` | WPF + WPFDevelopers | Circular progress, breathing light animation |
| `AntdUIStyle.cs` | WinForms + AntdUI | Dark theme, wave progress button |
| `MauiUpdatePage.xaml` + `.cs` | MAUI | Dark mode, AppThemeBinding |
| `DownloadViewModels.cs` | All frameworks shared | MVVM ViewModel |
| `RealDownloadService.cs` | All frameworks shared | **Core bridge** |

---

## Related Skills

- [🚀 generalupdate-init — Bootstrap configuration](./generalupdate-init.md)
- [🩺 generalupdate-troubleshoot — UI display issues](./generalupdate-troubleshoot.md)
