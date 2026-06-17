---
sidebar_position: 3
sidebar_label: 🎨 UI Generation
title: 🎨 generalupdate-ui — Update UI Generation
---

# 🎨 GeneralUpdate Update UI Generation — Full State Coverage

Auto-detects the developer's UI framework and generates a complete update window with real GeneralUpdate.Core event bindings.

> ⚠️ Targeting NuGet v10.5.0-beta.4. `RealDownloadService.cs` uses `UpdateRequest` and the correct namespaces.

---

## 📋 User Requirements Gathering

```
### UI Framework (Required)
- Target framework: ______ (WPF/WinForms/Avalonia/MAUI/Console/Not sure)
- Preferred UI library: ______ (Default / LayUI.Wpf / WPFDevelopers / AntdUI / SemiUrsa / Native)
- Existing project template: ______ (Yes/No, if no start with generalupdate-init)

### Update Scenario (Required)
- Update window role: ______ (Client / Upgrade / Both)
- Manual update trigger needed: ______ (Yes/No, auto-check on startup)
- Dark mode support: ______ (Yes/No)

### Advanced UI Needs (Optional)
- Custom brand color/logo: ______ (Yes/No)
- Multi-language support: ______ (Yes/No)
- Accessibility support: ______ (Yes/No)
```

---

## Workflow

```
1. Framework Detection
   ├── Scan .csproj → PackageReference to identify UI library
   ├── If unrecognized → Ask the user
   └── If no UI framework → Console progress bar

2. State Code Generation
   ├── IDownloadService bridge interface
   ├── RealDownloadService bridge code (manually adapt to GeneralUpdate.Core events)
   ├── ViewModel (MVVM) or Code-Behind
   └── Window/Page XAML

3. Integration Guidance
   ├── How to introduce GeneralUpdateBootstrap
   └── Bootstrap configuration (pair with generalupdate-init)
```

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

### Bridge Logic (v10.5.0-beta.4)

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

## ✅ Integration Verification Checklist

### Event Bridging
- [ ] All 7 events bound (including AddListenerProgress)
- [ ] Bridge code uses correct EventArgs types (check namespaces `GeneralUpdate.Core.Download` / `GeneralUpdate.Core.Event`)
- [ ] `IsCompleted` property name is correct (v10.5.0-beta.4 uses `IsCompleted`)

### Thread Safety
- [ ] UI updates executed on correct thread (WPF/Avalonia use `Dispatcher`, WinForms use `Invoke`, MAUI use `MainThread`)
- [ ] No blocking operations in `MultiDownloadStatistics` event (UI updates only)
- [ ] "Applying" state after download has timeout protection (> 30s show progress hint)

### State Machine Coverage
- [ ] All 11 states implemented (Idle → Checking → Latest/Found → Downloading → Paused → Error → Retrying → Applying → Success/Failed → Restart)
- [ ] Download error retry count limited (no more than 3 times)
- [ ] User can cancel the update operation

### Framework-Specific Checks
- [ ] **Avalonia**: ViewModel implements `INotifyPropertyChanged`, bindings use `{Binding}`
- [ ] **WPF**: Use `Dispatcher.Invoke` to update bound properties
- [ ] **WinForms AntdUI**: Use `Control.Invoke` for cross-thread updates
- [ ] **MAUI**: Check `Platform.CurrentActivity` lifecycle on Android

---

## ⚠️ Anti-Pattern Checklist

| # | Anti-Pattern | Consequence | Correct Approach |
|---|-------------|-------------|------------------|
| 1 | **Using the same generic ViewModel across frameworks** | Thread model incompatibility, cross-thread exceptions | Adapt Dispatcher/Invoke/MainThread per framework |
| 2 | **File IO or network requests in download statistics event** | Blocks update flow, UI stuttering | Only update bound UI properties |
| 3 | **Progress bar jumps directly to 100%** | User sees no intermediate progress, poor UX | Use `e.ProgressPercentage` for incremental updates |
| 4 | **Not handling MultiDownloadError event** | No user feedback on download failure, stuck waiting | At least show error + retry button |
| 5 | **Not distinguishing Client vs Upgrade UI** | Upgrade shows unnecessary "download progress" | Upgrade should show "Installing, please wait" only |
| 6 | **Using RealDownloadService.cs directly without adaptation** | Event bindings don't work | Must adjust `IDownloadService` implementation per project |
| 7 | **Starting update in ViewModel constructor (Avalonia/WPF)** | UI not initialized, bindings don't work | Trigger check in Loaded event or View layer |

---

## Related Skills

- `/generalupdate-init` — Bootstrap configuration
- `/generalupdate-strategy` — Silent mode if no UI needed
- `/generalupdate-troubleshoot` — UI display issues
