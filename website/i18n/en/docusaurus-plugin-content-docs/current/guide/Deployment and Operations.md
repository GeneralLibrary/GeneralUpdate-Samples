---
sidebar_position: 1
---

# Deployment & Operations Guide

This document covers application packaging & deployment, platform adaptation, troubleshooting, and operational diagnostics.

---

## 1. Patch Generation

### Using GeneralUpdate.Tools (Recommended)

[GeneralUpdate.Tools](https://github.com/GeneralLibrary/GeneralUpdate.Tools) is the recommended patch generation tool with a visual interface and full verification capabilities.

**Steps:**

1. Download and launch GeneralUpdate.Tools
2. Switch to the "Patch Package" tab
3. Select the old version directory (source path)
4. Select the new version directory (target path)
5. Set the patch output path
6. Fill in the package name and version number
7. Click "Build"

See [GeneralUpdate.Tools Documentation](../quickstart/GeneralUpdate.PacketTool) for details.

### Programmatic Generation with Differential API

```csharp
using GeneralUpdate.Differential;

// Generate incremental patch
var sourcePath = @"C:\Builds\MyApp_v1.0.0";
var targetPath = @"C:\Builds\MyApp_v1.0.1";
var patchPath  = @"C:\Builds\patches\v1.0.1";

await DifferentialCore.Instance.Clean(sourcePath, targetPath, patchPath);

// Package the patchPath directory as .zip and upload to server
```

---

## 2. Client Packaging

### Directory Structure

Recommended client installation directory structure:

```
MyApp/
├── MyApp.exe                    ← Main executable
├── MyApp.Core.dll               ← Core library
├── UpgradeSample.exe            ← Upgrade helper (alongside main app)
├── GeneralUpdate.Core.dll       ← Upgrade helper dependency
├── GeneralUpdate.ClientCore.dll ← Client update component
├── GeneralUpdate.Differential.dll
├── generalupdate.manifest.json  ← Auto-generated manifest
├── appsettings.json             ← App config (add to blacklist)
└── resources/
    └── ...
```

### Client References

```xml
<!-- Main app .csproj -->
<PackageReference Include="GeneralUpdate.ClientCore" Version="10.*" />
```

```xml
<!-- Upgrade helper .csproj -->
<PackageReference Include="GeneralUpdate.Core" Version="10.*" />
```

### Upgrade Helper — Independent Project

The upgrade helper (Upgrade) must be a **standalone executable project**, compiled separately from the main application:

```
MySolution/
├── src/
│   ├── MyApp/                    ← Main app project
│   │   ├── MyApp.csproj
│   │   └── Program.cs
│   └── MyApp.Upgrade/            ← Upgrade helper project
│       ├── MyApp.Upgrade.csproj
│       └── Program.cs
└── MySolution.sln
```

**Minimal Upgrade Helper Program.cs:**

```csharp
using GeneralUpdate.Core;

await new GeneralUpdateBootstrap()
    .AddListenerException((_, args) =>
    {
        Console.WriteLine($"Upgrade error: {args.Exception.Message}");
    })
    .LaunchAsync();
```

---

## 3. Platform-Specific Packaging

### Windows

**Create installer with NSIS:**

- [NSIS Official Site](https://nsis.sourceforge.io/Download)
- NSIS script handles: creating install directory, copying files, registering shortcuts
- Place main app and upgrade helper in the same directory
- Ensure the install directory has write permission for the current user

**Avoid C: drive permission issues:**

- Default install path: `%LOCALAPPDATA%` rather than `C:\Program Files\`
- If C: drive is required, see the [Windows Permission Handling](#5-windows-permission-handling) section below

**Windows publish commands:**

```bash
dotnet publish src/MyApp/MyApp.csproj -c Release -r win-x64 --self-contained
dotnet publish src/MyApp.Upgrade/MyApp.Upgrade.csproj -c Release -r win-x64 --self-contained
```

### Linux

**.deb packages (Debian/Ubuntu):**

See [Avalonia Deployment Docs](https://docs.avaloniaui.net/docs/deployment/debian-ubuntu)

**AppImage:**

Universal portable packaging format for Linux, runs without installation.

**Linux permission handling:**

```csharp
// Use UnixPermissionHooks in the upgrade helper
await new GeneralUpdateBootstrap()
    .Hooks<UnixPermissionHooks>()
    .LaunchAsync();
```

Or use a custom script:

```csharp
var config = new Configinfo
{
    Script = "/bin/bash chmod -R +x $InstallPath",
    // ...
};
```

**Linux publish commands:**

```bash
dotnet publish src/MyApp/MyApp.csproj -c Release -r linux-x64 --self-contained
dotnet publish src/MyApp.Upgrade/MyApp.Upgrade.csproj -c Release -r linux-x64 --self-contained
```

### macOS

**.app Bundle:**

macOS apps are typically packaged as `.app` directory structures. Ensure the upgrade helper is in the main app's `Contents/MacOS/` directory.

**Code Signing:**

macOS requires code signing for apps to run properly (especially on Apple Silicon):

```bash
codesign --deep --force --verify --verbose --sign "Developer ID" MyApp.app
```

**Notarization:**

Submit to Apple notarization before distributing:

```bash
xcrun notarytool submit MyApp.dmg --apple-id your@email.com --wait
```

**macOS publish commands:**

```bash
dotnet publish src/MyApp/MyApp.csproj -c Release -r osx-x64 --self-contained
dotnet publish src/MyApp.Upgrade/MyApp.Upgrade.csproj -c Release -r osx-x64 --self-contained
```

---

## 4. CI/CD Integration

### GitHub Actions Example

```yaml
name: Build and Package

on:
  push:
    tags:
      - 'v*'

jobs:
  build:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      
      - name: Publish Client
        run: dotnet publish src/MyApp/MyApp.csproj -c Release -r win-x64 -o publish/MyApp
      
      - name: Publish Upgrade
        run: dotnet publish src/MyApp.Upgrade/MyApp.Upgrade.csproj -c Release -r win-x64 -o publish/MyApp
      
      - name: Generate Patch
        run: |
          # Use GeneralUpdate.Tools CLI or other scripts to generate patches
      
      - name: Upload Artifacts
        uses: actions/upload-artifact@v4
        with:
          name: release-package
          path: publish/
```

---

## 5. Windows Permission Handling

### UAC and Installation Directories

![](imgs\UAC.png)

When using GeneralUpdate for automatic updates, permission issues may occur if the update directory is on the C: drive, especially when replacing files or applying patches. Windows 11 has stricter permission management for certain C: drive directories.

Directories that may trigger permission issues:

| Name | Directory |
| --- | --- |
| System Folder | C:\Windows |
| Registry Config | C:\Windows\System32\config |
| Driver Folder | C:\Windows\System32\drivers |
| Program Folder | C:\Program Files and C:\Program Files (x86) |

Recommended directories to avoid permission issues:

| Name | Directory |
| --- | --- |
| User Data Directory | AppData |
| System Temporary Directory | Temp |

### Lowering UAC Level

> **Warning:** The following method is not recommended for production use as it may pose security risks.

If you encounter UAC prompts or permission denied issues during updates, you may lower the UAC control level by modifying the registry:

| Registry Key | New Value | Default Value |
| --- | --- | --- |
| enableLUA | 0 | 1 |
| ConsentPromptBehaviorAdmin | 0 | 5 |

Modify the above registry settings before the update (effective after restart), and restore them after the update completes.

**C# registry modification:**

```csharp
using Microsoft.Win32;

public void UpdateRegistry()
{
    const string keyName = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System";

    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyName, true))
    {
        if (key != null)
        {
            key.SetValue("EnableLUA", 0, RegistryValueKind.DWord);
            key.SetValue("ConsentPromptBehaviorAdmin", 0, RegistryValueKind.DWord);
        }
    }
}
```

**Batch script:**

```bat
@echo off
REG ADD "HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System" /v EnableLUA /t REG_DWORD /d 0 /f
REG ADD "HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System" /v ConsentPromptBehaviorAdmin /t REG_DWORD /d 0 /f
```

**References:**
- [How User Account Control Works](https://learn.microsoft.com/en-us/windows/security/application-security/application-control/user-account-control/how-it-works)
- [Windows UAC Blog](https://blog.walterlv.com/post/windows-user-account-control.html)

---

## 6. File Occupancy Troubleshooting

### Windows Platform

Even when applications are closed before automatic upgrades, file occupancy can occur if background processes are still running. Use Microsoft's **handle.exe** tool to check for running processes in a specified directory.

`handle.exe` is a command-line tool from Microsoft Sysinternals that shows which processes have opened specific files. It can be invoked from C# via `System.Diagnostics.Process`.

```csharp
using System;
using System.Diagnostics;

class Program
{
    static void Main()
    {
        Process process = new Process();
        process.StartInfo.FileName = "handle.exe";
        process.StartInfo.Arguments = "filename"; // Replace with actual file or directory path
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.Start();

        string output = process.StandardOutput.ReadToEnd();
        Console.WriteLine(output);

        process.WaitForExit();
    }
}
```

**If file occupancy persists:**
1. Check for background services that haven't been shut down
2. Use `handle.exe` to identify the occupying process
3. Consider forced termination or post-reboot update strategies

**Reference:**
- [Handle - Sysinternals](https://learn.microsoft.com/en-us/sysinternals/downloads/handle)

---

## 7. Crash Dump Diagnostics

During automatic updates, if an update fails or the program crashes after updating, use the **ProcDump** tool to export dump files for analysis.

ProcDump is a command-line utility from Microsoft Sysinternals primarily used for monitoring CPU spikes in applications and generating crash dumps during those spikes. Administrators or developers can use these dumps to identify the cause of crashes. ProcDump also supports hung window monitoring, unhandled exception monitoring, and can generate dumps based on system performance counter values.

### C# Invocation of ProcDump

```csharp
using System;
using System.Diagnostics;

public class Program
{
    public static void Main()
    {
        var procDumpPath = @"C:\Path\To\procdump.exe";
        var processId = 1234; // The ID of the process to dump
        var dumpFilePath = @"C:\Path\To\dumpfile.dmp";

        var startInfo = new ProcessStartInfo
        {
            FileName = procDumpPath,
            Arguments = $"-ma {processId} {dumpFilePath}",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        var process = new Process { StartInfo = startInfo };
        process.OutputDataReceived += (sender, e) => Console.WriteLine(e.Data);
        process.ErrorDataReceived += (sender, e) => Console.Error.WriteLine(e.Data);

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        process.WaitForExit();
    }
}
```

**Reference:**
- [ProcDump - Sysinternals](https://learn.microsoft.com/en-us/sysinternals/downloads/procdump)

---

## 8. Version Number Management

### Recommended Strategies

| Strategy | Description | Use Case |
|----------|-------------|----------|
| **Manual** | Hardcode `<Version>` in `.csproj` | Small projects |
| **Git Tag** | Read version from Git tags | Projects with CI/CD |
| **MinVer** | Auto-calculate version from Git history | Recommended |
| **Nerdbank.GitVersioning** | Precise Git version management | Large projects |

### MinVer Configuration Example

```xml
<!-- .csproj -->
<PackageReference Include="MinVer" Version="5.*">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
</PackageReference>
```

---

## 9. Deployment Checklist

Before deploying, confirm:

- [ ] Main app references `GeneralUpdate.ClientCore`
- [ ] Upgrade helper is a standalone executable project referencing `GeneralUpdate.Core`
- [ ] Upgrade helper deployed in the same directory as the main app
- [ ] `generalupdate.manifest.json` can be auto-generated via Tools "Config Generator"
- [ ] Full update flow verified in test environment using Tools "Simulate Update"
- [ ] Blacklist correctly configured (protect user data from overwrite)
- [ ] Platform-specific permissions configured (Linux `chmod`, Windows registry)
- [ ] Version numbers follow SemVer format
- [ ] Server API deployed and configured with correct version info

---

## Related Resources

- **[GeneralUpdate.Tools](../quickstart/GeneralUpdate.PacketTool)** — Patch and config generation tool
- **[Beginner Cookbook](../quickstart/Beginner cookbook)** — Complete end-to-end update walkthrough
- **[GeneralUpdate.Core](../doc/GeneralUpdate.Core)** — Core update engine architecture
