---
sidebar_position: 6
---

# Packaging & Deployment

This guide covers how to package your application with its update system for distribution to end users.

---

## 1. Patch Generation

### Using GeneralUpdate.Tools (Recommended)

[GeneralUpdate.Tools](https://github.com/GeneralLibrary/GeneralUpdate.Tools) is the recommended patch generation tool with a visual interface and comprehensive validation.

**Steps:**
1. Download and launch GeneralUpdate.Tools
2. Switch to the "Patch Package" tab
3. Select old version directory (source path)
4. Select new version directory (target path)
5. Set patch output path
6. Enter package name and version
7. Click "Build"

See the [GeneralUpdate.Tools documentation](../doc/GeneralUpdate.PacketTool) for details.

### Using the Differential API Programmatically

```csharp
using GeneralUpdate.Differential;

var sourcePath = @"C:\Builds\MyApp_v1.0.0";
var targetPath = @"C:\Builds\MyApp_v1.0.1";
var patchPath  = @"C:\Builds\patches\v1.0.1";

await DifferentialCore.Instance.Clean(sourcePath, targetPath, patchPath);
// Package patchPath as .zip and upload to server
```

---

## 2. Client Packaging

### Directory Structure

Recommended installation directory layout:

```
MyApp/
├── MyApp.exe                    ← Main application
├── MyApp.Core.dll               ← Core libraries
├── UpgradeSample.exe            ← Upgrade assistant (same directory)
├── GeneralUpdate.Core.dll       ← Upgrade dependencies
├── GeneralUpdate.ClientCore.dll ← Client update component
├── GeneralUpdate.Differential.dll
├── generalupdate.manifest.json  ← Auto-generated manifest
├── appsettings.json             ← App config (add to blacklist)
└── resources/
```

### Client References

```xml
<!-- Main app .csproj -->
<PackageReference Include="GeneralUpdate.ClientCore" Version="10.*" />
```

```xml
<!-- Upgrade assistant .csproj -->
<PackageReference Include="GeneralUpdate.Core" Version="10.*" />
```

### Separate Upgrade Project

The Upgrade assistant must be a **separate executable project**:

```
MySolution/
├── src/
│   ├── MyApp/                    ← Main app project
│   │   ├── MyApp.csproj
│   │   └── Program.cs
│   └── MyApp.Upgrade/            ← Upgrade project
│       ├── MyApp.Upgrade.csproj
│       └── Program.cs
└── MySolution.sln
```

**Minimal Upgrade Program.cs:**

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

**Using NSIS:**
- [NSIS Download](https://nsis.sourceforge.io/Download)
- Place main app and upgrade assistant in the same directory
- Default install to `%LOCALAPPDATA%` to avoid C drive permission issues
- For C drive installs, see [Permission Guide](./Permission.md)

```bash
dotnet publish src/MyApp/MyApp.csproj -c Release -r win-x64 --self-contained
dotnet publish src/MyApp.Upgrade/MyApp.Upgrade.csproj -c Release -r win-x64 --self-contained
```

### Linux

**Using .deb (Debian/Ubuntu):** See [Avalonia deployment docs](https://docs.avaloniaui.net/docs/deployment/debian-ubuntu)

**Permission handling:**

```csharp
await new GeneralUpdateBootstrap()
    .Hooks<UnixPermissionHooks>()
    .LaunchAsync();
```

```bash
dotnet publish src/MyApp/MyApp.csproj -c Release -r linux-x64 --self-contained
dotnet publish src/MyApp.Upgrade/MyApp.Upgrade.csproj -c Release -r linux-x64 --self-contained
```

### macOS

**Code signing:**
```bash
codesign --deep --force --verify --verbose --sign "Developer ID" MyApp.app
```

**Notarization:**
```bash
xcrun notarytool submit MyApp.dmg --apple-id your@email.com --wait
```

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
    tags: ['v*']
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
      - name: Upload Artifacts
        uses: actions/upload-artifact@v4
        with:
          name: release-package
          path: publish/
```

---

## 5. Version Management

### Recommended Strategies

| Strategy | Description | Best For |
|----------|-------------|----------|
| **Manual** | Hardcode `<Version>` in `.csproj` | Small projects |
| **Git Tag** | Read version from Git tags | CI/CD pipelines |
| **MinVer** | Auto-calculate version from Git history | Recommended |
| **Nerdbank.GitVersioning** | Precise Git-based version management | Large projects |

---

## 6. Deployment Checklist

Before deploying, verify:

- [ ] Main app references `GeneralUpdate.ClientCore`
- [ ] Upgrade assistant is a separate executable project referencing `GeneralUpdate.Core`
- [ ] Upgrade assistant deployed in the same directory as the main app
- [ ] `generalupdate.manifest.json` generated via Tools' Config Generator
- [ ] Full update flow tested via Tools' Simulate Update
- [ ] Blacklist configured correctly (protect user data from overwrites)
- [ ] Platform-specific permissions handled (Linux `chmod`, Windows registry)
- [ ] Version numbers follow SemVer format
- [ ] Server API deployed with correct version information

---

## Related Resources

- **[GeneralUpdate.Tools](../doc/GeneralUpdate.PacketTool)** — Patch and config generation tool
- **[Permission Guide](./Permission.md)** — Windows UAC permission handling
- **[Beginner Cookbook](../quickstart/Beginner%20cookbook.md)** — Complete update walkthrough
- **[GeneralUpdate.Core](../doc/GeneralUpdate.Core.md)** — Core update engine architecture
