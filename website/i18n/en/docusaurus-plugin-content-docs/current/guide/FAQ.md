---
sidebar_position: 8
---

# Frequently Asked Questions

## Basic Concepts

### Q1: What is GeneralUpdate?

GeneralUpdate is a cross-platform .NET Standard 2.0 automatic update framework. It provides a complete application update solution including version checking, download management, binary differential updates, driver updates, and plugin management.

### Q2: What's the difference between ClientCore and Core?

| | ClientCore | Core |
|---|---|---|
| **Runs in** | Main app process | Separate upgrade process |
| **Responsibility** | Check versions, download packages, launch upgrade | Decompress, apply patches, replace files, start new version |
| **Depends on** | References Core's shared types | Launched by ClientCore (via IPC) |
| **NuGet** | `GeneralUpdate.ClientCore` | `GeneralUpdate.Core` |

**Simply put:** ClientCore "checks and downloads", Core "installs and replaces".

### Q3: What is differential updating?

Differential updating downloads only the **changes** between two versions instead of the full package.

- **Full update:** Download 50MB → Decompress 50MB → Overwrite
- **Differential update:** Download 5MB (patch) → Decompress → BSDiff merge → Only 5MB downloaded

For frequently updated apps, differential updates save 80%-95% bandwidth.

### Q4: Which platforms are supported?

| OS | Status | Notes |
|----|--------|-------|
| Windows 10/11 | ✅ Full support | WPF/WinForms/WinUI/Avalonia/Console |
| Linux (Ubuntu/Debian/Fedora) | ✅ Full support | Avalonia/Console |
| macOS | ✅ Full support | Avalonia/Console |
| Android | ✅ Via MAUI | OSS mode only |
| Kylin V10 (Phytium/Kunpeng) | ✅ Verified | Domestic platforms |
| UOS | ✅ Verified | Domestic platforms |
| Loongson (LoongArch) | ✅ Verified | Domestic platforms |

---

## Installation & Configuration

### Q5: What's the minimum code needed?

Using the `ConfiginfoBuilder` zero-config mode:

```csharp
using GeneralUpdate.ClientCore;
using GeneralUpdate.Common.Shared.Object;

var config = ConfiginfoBuilder
    .Create("https://your-server.com/api/update/check", 
            "your-token", "Bearer")
    .Build();

await new GeneralClientBootstrap()
    .SetConfig(config)
    .LaunchAsync();
```

Only 3 parameters needed — the rest auto-extracted from `.csproj`.

### Q6: How to configure the blacklist?

```csharp
var config = new Configinfo
{
    BlackFiles = new List<string> { "appsettings.json", "userdata.db" },
    BlackFormats = new List<string> { ".log", ".cache", ".tmp" },
    SkipDirectorys = new List<string> { "logs", "temp", "userdata" }
};
```

Defaults already skip: `System.*.dll`, `.patch`, `.pdb`, `.rar`, `.tar`, `.json`, `.zip` files, and `app-`, `fail` directories.

### Q7: How to enable silent updates?

```csharp
await new GeneralClientBootstrap()
    .Option(UpdateOption.EnableSilentUpdate, true)
    .SetConfig(config)
    .LaunchAsync();
```

When enabled:
- Background polling every 20 minutes (configurable)
- Silent download when new version found
- Auto-triggers upgrade when main app exits
- No user interaction required

---

## Version Management

### Q8: What version format is required?

Semantic Versioning 2.0: `Major.Minor.Patch.Build`

- `1.0.0.0` ✓
- `2.1.3.5` ✓
- `1.0` ✗ (incomplete)
- `v1.0.0` ✗ (contains prefix)

### Q9: How are multi-version skip updates handled?

GeneralUpdate automatically handles sequential version updates. If client is `1.0.0.0` and server has `1.0.1.0`, `1.0.2.0`, `1.1.0.0`:

```
Client 1.0.0.0
  → Download patch_v1.0.1.zip → Update to 1.0.1.0
  → Download patch_v1.0.2.zip → Update to 1.0.2.0
  → Download patch_v1.1.0.zip → Update to 1.1.0.0
```

Each step uses complete verification before proceeding.

### Q10: How to force an update?

Set `IsForcibly: true` in the server response:

```json
{
    "Version": "2.0.0.0",
    "IsForcibly": true,
    "UpdateLog": "Critical security update, must install"
}
```

When forced, the `AddListenerUpdatePrecheck` callback return value is ignored.

---

## Download & Network

### Q11: Is download resume supported?

Yes. Downloads resume from the breakpoint on next startup. Controlled by `EnableResume` option (enabled by default).

### Q12: How to configure download timeout?

```csharp
.Option(UpdateOption.DownloadTimeOut, 120) // 120 seconds
```

Default is 30 seconds. Adjust based on package size and network conditions.

### Q13: Can I download multiple versions concurrently?

Yes. Configure via `MaxConcurrency`:

```csharp
Option.MaxConcurrency.SetValue(5); // Max 5 concurrent downloads
```

---

## Differential Updates

### Q14: Differential vs full update — when to use which?

| Scenario | Recommended Mode |
|----------|-----------------|
| Regular minor updates | Differential (default) |
| Major version jumps (1.x → 2.x) | Full update |
| < 20% files changed | Differential |
| > 80% files changed | Full update |
| First install | Full update |

### Q15: How are patches generated?

Use [GeneralUpdate.Tools](https://github.com/GeneralLibrary/GeneralUpdate.Tools) Patch Package tab:

1. Select old version directory
2. Select new version directory
3. Choose output directory
4. Click "Build"

The tool automatically generates BSDiff patches for changed files and packages them as `.zip`.

---

## Files & Permissions

### Q16: What if files are locked during update?

The upgrade process runs as a separate process (Core) after the main app exits, so file locks are rarely an issue. If still locked, check for background services and use `handle.exe` (Windows) to diagnose.

### Q17: How to handle Linux/macOS permissions?

```csharp
// Automatic chmod +x
await new GeneralClientBootstrap()
    .Hooks<UnixPermissionHooks>()
    .SetConfig(config)
    .LaunchAsync();
```

Or via custom script:

```csharp
var config = new Configinfo
{
    Script = "/path/to/permission-script.sh"
};
```

---

## Troubleshooting

### Q18: How to diagnose update failures?

1. **Check event listeners** — Register all exception and error listeners
2. **Check Bowl logs** — If Bowl enabled, inspect `fail/` directory
3. **Check server logs** — Confirm version info is returned correctly
4. **Check network** — Verify client can reach server API and download URLs
5. **Check versions** — Ensure consistent version format on both sides

### Q19: How does rollback work?

GeneralUpdate has automatic backup and rollback:

```csharp
.Option(UpdateOption.BackUp, true) // Enable backup
```

On update failure or Bowl crash detection, files are automatically restored from backup.

### Q20: How to test the update flow in development?

Use [GeneralUpdate.Tools](https://github.com/GeneralLibrary/GeneralUpdate.Tools) "Simulate Update" feature:

1. Select app directory and patch file
2. Set version numbers and platform
3. Click "Start Simulation"

The tool auto-launches a local mock server, publishes test apps, and runs the complete update flow.

---

## Server

### Q21: Do I need to implement the server API myself?

Sample projects provide a simple server example. For production, implement your own or use the commercial [GeneralSpacestation](https://www.justerzhu.cn/).

**Required APIs:**
1. `POST /Upgrade/Verification` — Version verification
2. `POST /Upgrade/Report` — Status reporting
3. `GET /patch/{filename}` — Patch download

---

## OSS Mode

### Q22: What's the difference between OSS and standard mode?

| | Standard Mode | OSS Mode |
|---|---|---|
| **Server** | HTTP API service required | Object storage only (S3/OSS/MinIO) |
| **Version check** | API call | Read `version.json` |
| **Use case** | Apps with backend services | Desktop tools, standalone apps |
| **Cost** | Server maintenance | Storage and bandwidth only |

### Q23: How to migrate from standard to OSS mode?

1. Generate `version.json` (use Tools OSS Config module)
2. Upload `version.json` and patches to object storage
3. Change client `AppType` to `OssClient`
4. Configure OSS URL

---

## Related Resources

- **[GeneralUpdate.Core](../doc/GeneralUpdate.Core.md)** — Core update engine
- **[Beginner Cookbook](../quickstart/Beginner%20cookbook.md)** — Run through the full update loop
- **[GeneralUpdate.Tools](../quickstart/GeneralUpdate.PacketTool.md)** — Packaging tools
- **[GeneralUpdate.Bowl](../doc/GeneralUpdate.Bowl.md)** — Crash monitoring & rollback
