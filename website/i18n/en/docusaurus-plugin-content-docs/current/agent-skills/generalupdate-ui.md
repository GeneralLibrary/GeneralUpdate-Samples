---
sidebar_position: 3
sidebar_label: 🎨 UI Generation
title: 🎨 generalupdate-ui — Update UI Generation
---

# 🎨 GeneralUpdate Update UI Guide

If your app needs to **show users the update progress** (instead of silently updating in the background), this page will help you generate the right UI code.

---

## First: What each program displays

GeneralUpdate uses a dual-process architecture, and each process shows a different UI:

```
Your main program (Client)
    Shows "Checking for updates → Download progress"
    User sees: Downloading 45% / Speed / Time remaining

    ↓ After download completes, launches ↓

Upgrade program (Upgrade)
    Shows "Installing update, please wait..."
    User sees: Installation progress bar / Failure message
```

> **Client**: Handles downloading — show a progress bar, speed, version info
> **Upgrade**: Handles file installation (fast) — just show a "please wait" screen

---

## Which situation are you in?

| Situation | What to do |
|-----------|-----------|
| **Already have a main app, need to add an update window** | Add an update window to your existing project (this page will help) |
| **Starting from scratch** | First use [generalupdate-init](generalupdate-init) to set up the dual-project structure, then come back for the UI |
| **No UI needed (silent update)** | Go to [generalupdate-strategy](generalupdate-strategy) for silent mode |

---

## Supported UI frameworks

| Framework | Template file | Highlights |
|-----------|--------------|------------|
| **WPF + LayUI.Wpf** | `LayUIStyle.xaml` | Glass-effect progress bar |
| **WPF + WPFDevelopers** | `WPFDevelopersStyle.xaml` | Circular progress, breathing animation |
| **Avalonia + SemiUrsa** | `SemiUrsaClientView.axaml` | Cross-platform, dark mode toggle |
| **WinForms + AntdUI** | `AntdUIStyle.cs` | Dark theme |
| **MAUI** | `MauiUpdatePage.xaml` | Mobile + desktop |

The **update logic is the same** across all frameworks — only the visual style changes.

---

## The 4 key UI states

An update window is essentially a **state machine** with 4 main states:

```
┌────────────────────────────────────────────────────┐
│  ① Checking                                         │
│  "Checking for updates..."                           │
│        │                                            │
│        ▼                                            │
│  ② Downloading (the main state)                    │
│  ┌────────────────────────────────────────────────┐ │
│  │ MyApp_2.0.0.0.zip   45%                        │ │
│  │ ████████████████░░░░░░░░░                      │ │
│  │ Speed: 3.2 MB/s  Remaining: 12s                │ │
│  └────────────────────────────────────────────────┘ │
│        │                                            │
│        ▼                                            │
│  ③ Installing                                      │
│  "Installing update, please do not power off..."    │
│        │                                            │
│        ▼                                            │
│  ④ Complete / Failed                               │
│  "Update complete!" / "Download failed, retry"      │
└────────────────────────────────────────────────────┘
```

There are additional sub-states ("Already up to date", "Paused", "Retrying"), but **beginners only need to focus on ① and ②**.

---

### How to connect update progress to your UI

GeneralUpdate fires events with progress data — you just map the numbers to your UI controls:

```csharp
// GeneralUpdate progress event → update your UI
bootstrap.AddListenerMultiDownloadStatistics((_, e) =>
{
    // e.ProgressPercentage  → your progress bar Value (0-100)
    // e.Speed              → your speed label
    // e.Remaining          → your time remaining label
    // e.BytesReceived      → bytes downloaded
    // e.TotalBytesToReceive → total bytes to download
});
```

That's it. **The core work is mapping these 5 values to your UI controls.**

---

## Info to gather before generating UI

```
### Your project
- UI framework: ______ (WPF / WinForms / Avalonia / MAUI / Console)
- Need dark mode: ______ (Yes/No)

### Update window role (see "First" section above)
- Client (main app): ______ (need to show download progress?)
- Upgrade (installer): ______ (need to show installation status?)
```

---

## Simple example: Console progress bar

Even without a GUI, a console progress bar is easy:

```csharp
await new GeneralUpdateBootstrap()
    .SetConfig(config)
    .AddListenerMultiDownloadStatistics((_, e) =>
    {
        Console.Write($"\rDownload progress: {e.ProgressPercentage}% ");
        Console.Write($"[{new string('■', (int)(e.ProgressPercentage / 5))}");
        Console.WriteLine($"{new string('□', 20 - (int)(e.ProgressPercentage / 5))}]");
        Console.Write($"Speed: {e.Speed}/s");
    })
    .LaunchAsync();
```

---

## Known issues & tips

| # | Issue | Note |
|---|-------|------|
| 1 | **Don't do heavy work in update callbacks** | Only update UI — no file I/O or network requests |
| 2 | **Cross-thread UI updates** | WPF: `Dispatcher.Invoke`, WinForms: `Control.Invoke` |
| 3 | **Upgrade doesn't need download progress** | Upgrade just installs — show "Please wait" |
| 4 | **Show a retry button on failure** | Otherwise users get stuck at 99% |
| 5 | **Don't jump progress to 100% instantly** | Users need to see intermediate progress |

---

## Advanced: Full-state UI templates

If you need a complete UI (pause, resume, retry, dark mode toggle, etc.), the skill repository provides full template code.

To generate: describe your UI framework in Claude Code, and the AI will select and generate the appropriate template.

---

## Related pages

- [generalupdate-init](generalupdate-init) — First set up the dual-project structure
- [generalupdate-strategy](generalupdate-strategy) — Silent mode (no UI needed)
- [generalupdate-advanced](generalupdate-advanced) — Advanced customization
