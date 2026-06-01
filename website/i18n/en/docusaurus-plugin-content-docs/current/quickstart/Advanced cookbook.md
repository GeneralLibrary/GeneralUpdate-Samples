---
sidebar_position: 4
title: Advanced cookbook
---

# GeneralUpdate advanced cookbook

> **Target audience**: developers who have completed the beginner flow and are integrating GeneralUpdate into production products, CI/CD pipelines, and enterprise release workflows.

This cookbook complements the [beginner cookbook](./Beginner cookbook.md). The beginner cookbook gets you through your first update loop; this one assumes you've done that and jumps straight into production-grade design.

---

## 1. Production update architecture

### 1.1 Topology overview

```
┌──────────┐  GET version    ┌──────────┐
│  Client  │ ──────────────> │  Server  │
│ (main app)│ <── version.json │ (update)  │
└────┬─────┘                 └────┬─────┘
     │                            │
     │ download patch ZIP         │ reads packages/
     ▼                            ▼
┌──────────┐  start + pass  ┌──────────┐
│ Upgrade  │ <──────────── │  Client  │
│ (own proc)│               │          │
└────┬─────┘               └──────────┘
     │ apply patch, report status
     ▼
┌──────────┐
│  new      │
│  version  │
└──────────┘
```

### 1.2 Role responsibilities

| Role | Deployment | Responsibility | Key config |
|------|-----------|----------------|------------|
| Client | User endpoint | Version check, package download, launches Upgrade | `UpdateUrl`, `AppSecretKey`, `ClientVersion` |
| Upgrade | Same dir as Client | Independent updater, receives context and applies patch | Passed by Client via `GeneralUpdateBootstrap` |
| Server | Server | Version verification, package download, status reporting | `versions.json`, `/Upgrade/Verification`, `/File/Download` |
| Tools | Dev / CI | Generate patch packages, OSS manifests, hash computation | Input: old/new dirs, Output: `.zip` |
| Bowl | Same dir as Client | Process monitoring, crash detection, backup restore | `MonitorParameter`, dump path |

### 1.3 Minimum production topology

For most products:

- **Dev side**: CI runs `generate_packages.ps1` + Tools to produce patches
- **Server side**: One HTTP Server hosting `versions.json` and `.zip` files; optionally OSS mode with static storage
- **Client side**: Client references `GeneralUpdate.ClientCore`, Upgrade references `GeneralUpdate.Core`

```text
CI (GitHub Actions)                     Server (HTTP / OSS)
┌─────────────────┐                    ┌──────────────────┐
│ build + publish │ ── push ZIPs ────> │ versions.json    │
│ gen_packages.ps1│                    │ *.zip (packages)  │
│ Tools (optional) │                    │ static file host  │
└─────────────────┘                    └──────────────────┘
```

---

## 2. Standardized release artifacts

### 2.1 Artifact types

| Type | Generator | Format | Use case |
|------|-----------|--------|----------|
| Full package | `ZipFile.CreateFromDirectory` of full publish dir | `packet_{ts}_{version}.zip` | Cross-version upgrade, first install |
| Patch package | Tools Patch page or `DifferentialCore.Clean` comparing old/new | `patch_{ts}.zip` | Small change set to reduce download |
| Extension package | Tools Extension page | `.zip` (with manifest) | Plugin/extension standalone distribution & update |
| OSS manifest | Tools OSS page | `version.json` | OSS-mode update, suitable for static file hosting |

### 2.2 `versions.json` field reference

Each record in the Samples Server `versions.json` controls version matching and download:

```json
{
  "PacketName": "packet_20260529221936_client_2.0.0.0",
  "Hash": "a1b2c3...sha256",
  "Version": "2.0.0.0",
  "Url": "http://localhost:5000/File/Download/a1b2c3...",
  "PubTime": "2026-05-29T14:19:36.0000000",
  "AppType": 1,
  "Platform": 1,
  "ProductId": "2d974e2a-31e6-4887-9bb1-b4689e98c77a",
  "IsForcibly": false,
  "Format": ".zip",
  "Size": 123456,
  "IsFreeze": false,
  "IsCrossVersion": true,
  "FromVersion": "1.0.0.0",
  "ToVersion": "2.0.0.0"
}
```

| Field | Description |
|-------|-------------|
| `AppType` | `1` = Client (main app), `2` = Upgrade (updater) |
| `Platform` | `1` = Windows, `2` = Linux, `3` = Mac |
| `IsCrossVersion` | `true` = differential package (requires `FromVersion`/`ToVersion`), `false` = full package |
| `IsForcibly` | `true` = forced update, user cannot skip |
| `IsFreeze` | `true` = freeze this version, Server responds "already latest" |
| `ProductId` | Product identifier, must match Client config `ProductId` |

### 2.3 Packaging scripts

Samples includes complete PowerShell packaging scripts:

```powershell
# Generate both full and differential packages
.\src\Server\generate_packages.ps1

# Full-only mode (fast, skips Differential)
.\src\Server\generate_packages.ps1 -FullOnly

# Rebuild versions.json from existing ZIPs
.\src\Server\create_versions_json.ps1
```

Flow:
1. Scans `src/content_client/` and `src/content_upgrade/` for sample content
2. Produces full ZIPs for each version directory
3. Invokes `PatchGenerator` C# project to produce differential ZIPs
4. Computes SHA256 for each ZIP and writes `versions.json`

**Production recommendation**: adapt these scripts as CI steps, accepting old-version and new-version paths as inputs, outputting to an artifact repository.

---

## 3. CI/CD integration

### 3.1 GitHub Actions example

```yaml
name: Generate Update Packages

on:
  push:
    tags:
      - 'v*.*.*'

jobs:
  generate-packages:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      - name: Build release
        run: dotnet publish src/Client/ClientSample.csproj -c Release -o publish/v${{ github.ref_name }}

      - name: Generate packages
        shell: pwsh
        run: |
          $oldVersion = "publish/v1.0.0"  # download previous release from artifact store
          $newVersion = "publish/v${{ github.ref_name }}"
          dotnet run --project src/PatchGenerator -- $oldVersion $newVersion packages/

      - name: Upload artifacts
        uses: actions/upload-artifact@v4
        with:
          name: update-packages
          path: packages/
```

### 3.2 Version mapping strategies

**Key problem**: CI cannot assume every historical version lives on the build machine.

**Recommended approaches**:

| Approach | Description | Best for |
|----------|-------------|----------|
| Artifact repo cache | Upload `publish/v{version}` to an artifact store (GitHub Releases, Artifactory) on every release; CI downloads the versions it needs to diff against | Medium scale, manageable version count |
| Baseline + backfill | Publish a full package every N releases; diffs only compare against the last 2–3 versions | Fast-iterating products |
| On-demand download | Server downloads old-version packages from OSS on the fly for differential comparison | Products with many versions |

### 3.3 Release checklist

Confirm before every release:

- [ ] `Hash` in the new `versions.json` entry matches the uploaded ZIP
- [ ] Differential package `FromVersion` correctly points to the old version
- [ ] Full packages distinguish Client (`AppType=1`) from Upgrade (`AppType=2`)
- [ ] Old versions do not have `IsFreeze` set to `true` (unless intentionally frozen)
- [ ] All ZIPs are downloadable via `GET /File/Download/{hash}`
- [ ] Smoke-test: Client upgrades from the latest old version to the new version

---

## 4. Version strategy

### 4.1 Version number conventions

GeneralUpdate follows [Semantic Versioning](https://semver.org/) (SemVer) core principles. On top of SemVer `MAJOR.MINOR.PATCH`, a fourth `Revision` segment is added for .NET `System.Version` compatibility, forming the `Major.Minor.Build.Revision` four-part scheme:

```text
2.0.0.0
│ │ │ └── Revision — .NET 4th segment, not in SemVer standard
│ │ └──── Build    — maps to SemVer PATCH
│ └────── Minor    — maps to SemVer MINOR
└──────── Major    — maps to SemVer MAJOR
```

Version rules:
- **Major**: incremented for incompatible API changes
- **Minor**: incremented for backwards-compatible new functionality
- **Build** (patch): incremented for backwards-compatible bug fixes
- **Revision**: internal revision, typically `0`; only distinguished in .NET assembly versions

Server compares with `new Version(string)` — keep the numeric format. See [Semantic Versioning 2.0.0](https://semver.org/) for details.

### 4.2 Update modes

| Mode | `UpgradeMode` | Behavior |
|------|---------------|----------|
| Version chain (full) | `1` | Matches only `IsCrossVersion = false` packages; upgrades sequentially from low to high version |
| Cross version (diff) | `2` | Matches only `IsCrossVersion = true` packages; `FromVersion` must exactly match the client's current version |

### 4.3 Staged rollout

```text
                     ┌──────────────┐
v1.0.0.0 ───────────>│ Full → 2.0.0.0 │  (cross-version jump)
                     └──────────────┘
                     ┌──────────────┐
v1.0.0.1 ───────────>│ Diff → 2.0.0.0 │  (exact FromVersion match)
                     └──────────────┘
                     ┌──────────────┐
v1.0.0.2 ───────────>│ Diff → 2.0.0.0 │
                     └──────────────┘
```

**Recommendations**:
- Major releases: provide both full and differential packages
- Provide differential upgrade paths for the 3 most recent active versions
- Users more than 3 versions behind go through full upgrade

### 4.4 Forced updates

Set `IsForcibly` to `true` in `versions.json`:

```json
{ "Version": "2.0.1.0", "IsForcibly": true }
```

Client-side handling with `AddListenerUpdatePrecheck`:

```csharp
bootstrap.AddListenerUpdatePrecheck(async (precheckArgs) =>
{
    var forcedCount = precheckArgs.Versions.Count(v => v.IsForcibly);
    if (forcedCount > 0)
    {
        // Forced update: don't show a "skip" button
        ShowDialog("A critical security update is required.");
        return false; // false = do not skip, proceed with update
    }
    // Optional update: ask the user
    return await AskUserWhetherToSkip();
});
```

### 4.5 Multi-version sequential upgrade

When the client is multiple versions behind:

1. Server returns all records above the current version (`IsCrossVersion = false` full packages)
2. Client downloads and applies them in ascending version order
3. After each version upgrade, Upgrade restarts Client, and Client re-queries for the next version

**Note**: to jump straight to the latest in one hop, provide a single full package (with the latest target version as its `Version`).

---

## 5. Differential strategy

### 5.1 When to use differential

| Scenario | Recommendation | Reason |
|----------|---------------|--------|
| User on a recent version | Differential | Only changed files transferred, saves bandwidth |
| User 3+ versions behind | Full | Accumulated changes may exceed full download |
| Emergency security patch | Differential | Fast distribution, minimizes attack window |
| Framework/runtime upgrade | Full | Wide file changes, low diff benefit |
| Large file update (>100MB) | Differential | Significant size reduction even with many changes |

### 5.2 Differential generation and `delete_files.json`

A patch package produced by Tools or `DifferentialCore.Clean` contains:

```
patch_20260529221936.zip
├── file1.dll.patch       ← binary diff file
├── file2_new.dll         ← new file (copied directly)
└── generalupdate.delete.json  ← files to delete
```

`generalupdate.delete.json` format:

```json
{
  "deleteFiles": [
    "old_plugin.dll",
    "deprecated/config.json"
  ]
}
```

Upgrade applies the patch automatically:
1. Restore `.patch` files via differential application
2. Copy new files
3. Delete files listed in the manifest

### 5.3 Blacklist configuration

Some files should never participate in updates (user data, local config, logs, etc.):

```csharp
new GeneralUpdateBootstrap()
    .AddBlacklist(new List<string>
    {
        "appsettings.json",    // user local config
        "logs/",               // log directory
        "*.user",              // user-specific files
        "cache.db"             // local database
    })
```

Custom strategy via `IBinaryDiffer` extension point:

```csharp
// Skip files larger than 100MB for differential — copy them directly instead
public class LargeFileAwareStrategy : DefaultCleanStrategy
{
    public override bool ShouldDiff(string relativePath, FileInfo oldFile, FileInfo newFile)
    {
        const long maxDiffSize = 100 * 1024 * 1024;
        if (newFile.Length > maxDiffSize)
            return false; // copy new file as-is, don't generate a .patch
        return base.ShouldDiff(relativePath, oldFile, newFile);
    }
}
```

### 5.4 Parallel processing

Differential uses parallel processing by default (`StreamingHdiffDiffer`). For large directories (thousands of files):

- Tune parallelism: `new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }`
- For very large files use bsdiff (lower memory, slightly slower)
- For typical files use hdiff (faster, higher memory)

---

## 6. Bowl reliability design

### 6.1 Bowl lifecycle placement

```text
┌──────────┐    ┌──────────┐    ┌──────────┐    ┌──────────┐
│ Update    │ -> │ Bowl     │ -> │ Start main│ -> │ Monitor  │
│ complete  │    │ launches │    │ app       │    │ process  │
└──────────┘    └──────────┘    └──────────┘    └────┬─────┘
                                                     │
                                            Process exits abnormally
                                                     │
                                                     ▼
                                              ┌──────────┐
                                              │ Dump file │
                                              │ Backup old│
                                              │ Fail JSON │
                                              └──────────┘
```

### 6.2 Production configuration

```csharp
public static async Task GuardAsync(string appPath, string installPath)
{
    var lastVersion = "2.0.0.0";
    var processInfo = new MonitorParameter
    {
        ProcessNameOrId = "MyProduct.App.exe",
        DumpFileName = $"{lastVersion}_fail.dmp",
        FailFileName = $"{lastVersion}_fail.json",
        TargetPath = installPath,
        FailDirectory = Path.Combine(installPath, "fail", lastVersion),
        BackupDirectory = Path.Combine(installPath, "backup", lastVersion),
        WorkModel = "Normal"
    };

    Bowl.Launch(processInfo);
}
```

### 6.3 Backup and rollback flow

1. **Before upgrade**: Bowl backs up current program files to `BackupDirectory`
2. **After upgrade**: Bowl starts the new version of the main app
3. **During monitoring**: Bowl continuously watches the process
4. **On failure**:
   - Writes `.dmp` file
   - Writes `fail.json` (version, timestamp, system info, driver info)
   - If `WorkModel = "Normal"`, auto-restores the old version from backup
5. **After restore**: Bowl relaunches the old version

### 6.4 Platform differences

| Feature | Windows | Linux | macOS |
|---------|---------|-------|-------|
| Dump export | MiniDumpWriteDump | Requires `dotnet-dump` or `createdump` | Requires `dotnet-dump` |
| Process monitoring | WMI / Win32 API | procfs (`/proc/{pid}`) | Similar to Linux |
| Backup/restore | File copy + overwrite | File copy + overwrite (watch permissions) | File copy + overwrite |
| Driver info | `driverquery` | `lsmod` / `lspci` | `kextstat` |

### 6.5 Log integration

Bowl outputs the following to `fail.json`:

```json
{
  "version": "2.0.0.0",
  "timestamp": "2026-05-29T14:30:00Z",
  "processName": "MyProduct.App.exe",
  "exitCode": -1073741819,
  "systemInfo": { "os": "Windows 11", "arch": "x64" },
  "driverInfo": [],
  "dumpPath": "fail/2.0.0.0/2.0.0.0_fail.dmp"
}
```

Add a log-upload step in your product to send `fail.json` and `.dmp` to your server for analysis.

---

## 7. Extension system

### 7.1 Extension lifecycle

```text
┌──────────┐   ┌──────────┐   ┌──────────┐   ┌──────────┐
│ Query     │ → │ Download │ → │ Install  │ → │ Load into│
│ available │   │ package  │   │ to dir   │   │ runtime  │
└──────────┘   └──────────┘   └──────────┘   └────┬─────┘
                                                   │
                                              ┌────▼─────┐
                                              │ Update /  │
                                              │ Rollback  │
                                              │ Uninstall │
                                              └──────────┘
```

### 7.2 Extension manifest specification

Each extension package should include `extension.json`:

```json
{
  "id": "com.mycompany.plugin.search",
  "name": "Search Plugin",
  "version": "1.2.0",
  "author": "MyCompany",
  "description": "Full-text search capability",
  "dependencies": {
    "com.mycompany.core": ">=1.0.0"
  },
  "platforms": ["win-x64", "linux-x64"],
  "minAppVersion": "2.0.0",
  "type": "plugin"
}
```

### 7.3 Query and install

```csharp
var manager = new ExtensionManager(options =>
{
    options.ExtensionsRootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "extensions");
    options.TempDownloadPath = Path.Combine(Path.GetTempPath(), "ext_downloads");
    options.ManifestFileName = "extension.json";
});

// Query remote available extensions
var available = await manager.QueryRemoteExtensionsAsync("https://extensions.example.com/api/v1/extensions");

// Install
await manager.InstallAsync("com.mycompany.plugin.search", "1.2.0",
    progress => Console.WriteLine($"Downloading: {progress.Percentage}%"));

// Update
await manager.UpdateAsync("com.mycompany.plugin.search", "1.3.0");

// Rollback
await manager.RollbackAsync("com.mycompany.plugin.search", "1.2.0");

// Uninstall
await manager.UninstallAsync("com.mycompany.plugin.search");
```

### 7.4 Tools extension package generation

Use the Extension Package page in Tools:

| Input | Description |
|-------|-------------|
| Extension Source Directory | Source directory of extension files |
| Manifest File | `extension.json` path |
| Output Directory | Extension package ZIP output directory |

The resulting ZIP contains the manifest and all extension files, ready to push to your extension server.

### 7.5 Server API contracts

The extension server should provide these endpoints:

```
GET  /api/v1/extensions                    → returns available extension list
GET  /api/v1/extensions/{id}               → returns extension detail and version list
GET  /api/v1/extensions/{id}/versions/{v}  → returns version metadata and download link
GET  /api/v1/extensions/download/{hash}    → downloads the extension package
POST /api/v1/extensions/check              → batch compatibility check
```

---

## 8. Driver updates

### 8.1 Driver updates vs. application file updates

| Dimension | App file update | Driver update |
|-----------|----------------|---------------|
| File location | Application directory | System driver directory |
| Permission | Normal user | Administrator / root |
| Activation | Restart application | May require system reboot |
| Signing | None required | Digital signature required (Windows WHQL / Linux DKMS) |
| Rollback complexity | Replace files directly | Uninstall current driver, install old version |
| Compatibility check | Framework version | Kernel version, hardware ID |

### 8.2 Drivelution update flow

```csharp
var updater = new DriverUpdater(options =>
{
    options.DriverStorePath = @"C:\ProgramData\MyProduct\Drivers";
    options.TempPath = Path.GetTempPath();
    options.RequireSignature = true;
    options.BackupBeforeInstall = true;
});

// 1. Validate driver package
var validation = await updater.ValidatePackageAsync(driverPackagePath);
if (!validation.IsValid)
{
    Console.WriteLine($"Driver validation failed: {validation.Errors}");
    return;
}

// 2. Backup current driver
await updater.BackupCurrentDriverAsync("MyDeviceDriver");

// 3. Install new driver
var result = await updater.InstallAsync(driverPackagePath);
if (!result.Success)
{
    // 4. Install failed, auto-rollback
    await updater.RollbackAsync("MyDeviceDriver");
    Console.WriteLine($"Install failed: {result.ErrorMessage}");
    return;
}

// 5. Verify new driver health
var health = await updater.HealthCheckAsync("MyDeviceDriver");
Console.WriteLine($"Driver health: {health.Status}");
```

### 8.3 Windows driver signing

Production Windows drivers must be signed:

1. Obtain an EV Code Signing certificate
2. Pass WHQL (Windows Hardware Quality Labs) certification
3. Enable `RequireSignature = true` in `DriverUpdater`

### 8.4 Linux driver considerations

- Use `dkms` (Dynamic Kernel Module Support) for kernel module management
- Check kernel version compatibility before updating
- `/lib/modules/$(uname -r)/` typically requires root permissions
- May need `depmod` and `modprobe` after update

### 8.5 Batch / parallel driver updates

```csharp
var updaters = new[]
{
    new DriverUpdateTask { DeviceId = "PCI\\VEN_8086", PackagePath = @"drivers\gpu_2.1.0.zip" },
    new DriverUpdateTask { DeviceId = "USB\\VID_0BDA", PackagePath = @"drivers\wifi_1.5.0.zip" },
};

var results = await Parallel.ForEachAsync(updaters,
    new ParallelOptions { MaxDegreeOfParallelism = 4 },
    async (task, ct) =>
    {
        var updater = new DriverUpdater(/* ... */);
        return await updater.InstallAsync(task.PackagePath);
    });

foreach (var r in results)
{
    Console.WriteLine($"Driver {r.DeviceId}: {(r.Success ? "OK" : "FAILED")}");
}
```

---

## 9. Security and compliance

### 9.1 Hash validation

GeneralUpdate automatically validates SHA256 after download:

```powershell
# Server-side: generate hash (PowerShell)
(Get-FileHash -Path .\packet.zip -Algorithm SHA256).Hash.ToLowerInvariant()

# Or use the Tools OSS page
```

Client-side auto-validation: after download, Core computes SHA256 on the downloaded file and compares it against the `Hash` field in `versions.json`. Mismatch triggers discard and retry (up to 3 times).

### 9.2 Authenticated downloads

Server supports passing a token in download requests:

```csharp
// Client-side: add auth header to requests
new GeneralUpdateBootstrap()
    .AddListenerDownloadConfig(config =>
    {
        config.Headers = new Dictionary<string, string>
        {
            ["Authorization"] = $"Bearer {GetAccessToken()}"
        };
    });
```

**Production recommendations**:
- Use short-lived JWT tokens
- Download endpoint validates token scope (only allow download for the specified `ProductId`)
- Log every download's token, IP, and timestamp to an audit log

### 9.3 OSS mode security

OSS mode uses static file services (Alibaba Cloud OSS, AWS S3, etc.):

```
# Recommended OSS bucket policy
{
  "Statement": [
    {
      "Effect": "Allow",
      "Principal": {"Anonymous": ["*"]},
      "Action": ["oss:GetObject"],
      "Resource": ["acs:oss:my-bucket:packages/*.zip"],
      "Condition": {
        "IpAddress": {"acs:SourceIp": ["your-server-ip-range"]}
      }
    },
    {
      "Effect": "Allow",
      "Principal": {"Anonymous": ["*"]},
      "Action": ["oss:GetObject"],
      "Resource": ["acs:oss:my-bucket:packages/versions.json"]
    }
  ]
}
```

### 9.4 Release audit checklist

Confirm before every production release:

- [ ] All `Hash` values in `versions.json` match the SHA256 of uploaded ZIPs
- [ ] Download endpoint uses HTTPS (HTTP is prohibited in production)
- [ ] `AppSecretKey` is correctly configured to prevent unauthorized version queries
- [ ] `IsFreeze` flag on old versions is correct (don't freeze versions still in active use)
- [ ] Differential package `FromVersion`/`ToVersion` paths are correct
- [ ] All files are CI-signed and untampered
- [ ] Production release record written to audit log

---

## 10. Failure drills

### 10.1 Scenario: download failure / network interruption

**How to simulate**: disconnect the network or stop the Server during Client download.

**Expected behavior**:
- Client throws `DownloadException`
- Resume-from-breakpoint kicks in automatically: download continues on next launch
- Download progress reported via `AddListenerMultiDownloadStatistics`

**Verification**:
```csharp
bootstrap.AddListenerMultiDownloadError((version, exception) =>
{
    Console.WriteLine($"[Download Error] {version.Version}: {exception.Message}");
    // Production integration: alerting
});
```

### 10.2 Scenario: file lock

**How to simulate**: before Upgrade applies the patch, manually open a DLL in the target directory (e.g., with Notepad).

**Expected behavior**:
- Upgrade encounters `IOException` when attempting to replace the file
- Retries 3 times (500ms intervals)
- If still failing, fires `AddListenerException` callback
- Other already-updated files are not corrupted

### 10.3 Scenario: post-upgrade startup crash

**How to simulate**: replace `UpgradeSample.exe` in `content_upgrade` with a fake exe that crashes immediately.

**Expected behavior**:
- Client completes update and launches the new version
- Bowl detects abnormal process exit
- Bowl writes `.dmp` and `fail.json` to `fail/{version}/`
- Bowl restores the old version from `backup/{version}/`
- Old version starts successfully

**Verification**:
1. Check `fail/` directory for dump and JSON files
2. Confirm old version was restored and launched
3. Server receives failure status via `/Upgrade/Report`

### 10.4 Scenario: corrupted patch package

**How to simulate**: manually modify a patch ZIP in `packages/` (e.g., open with a text editor and tweak a few bytes).

**Expected behavior**:
- Client validates SHA256 after download — mismatch detected
- Automatically discards the corrupted file and re-downloads (up to 3 times)
- After 3 failures, notifies via `AddListenerMultiDownloadError`
- Corrupted patch is never applied

### 10.5 Scenario: insufficient permissions

**How to simulate**: on Linux, run Upgrade as a non-root user with write permissions removed from the target directory.

**Expected behavior**:
- Upgrade encounters `UnauthorizedAccessException` when trying to write
- Notified via `AddListenerException`
- No partial update (atomicity preserved)

**Prevention**:
```csharp
// Client validates target directory permissions before launching Upgrade
if (!HasWritePermission(installDirectory))
{
    RequestAdminPrivilege();
    return;
}
```

### 10.6 Failure drill schedule

Run a full failure drill once per release cycle:

| Drill | Frequency | Owner | Verification |
|-------|-----------|-------|-------------|
| Download resume | Every release | QA | Check resume-from-breakpoint logs |
| File lock handling | Every release | QA | Check rollback behavior |
| Startup crash recovery | Monthly | DevOps | Bowl fail dir + Server report |
| Patch corruption validation | Every release | QA | Download log SHA256 mismatch |
| Permission handling | Every major version | DevOps | Linux platform test |
| Diff package integrity | Every release | CI | Automated tree comparison test |

---

## Appendix: related resources

- [GeneralUpdate.Core component docs](../doc/GeneralUpdate.Core.md)
- [GeneralUpdate.Differential component docs](../doc/GeneralUpdate.Differential.md)
- [GeneralUpdate.Bowl component docs](../doc/GeneralUpdate.Bowl.md)
- [GeneralUpdate.Drivelution component docs](../doc/GeneralUpdate.Drivelution.md)
- [GeneralUpdate.Extension component docs](../doc/GeneralUpdate.Extension.md)
- [GeneralUpdate.Tools reference](../doc/GeneralUpdate.PacketTool.md)
- [Beginner cookbook](./Beginner cookbook.md)
- [Samples repository](https://github.com/GeneralLibrary/GeneralUpdate-Samples)
- [Tools repository](https://github.com/GeneralLibrary/GeneralUpdate.Tools)

> **Contributing**: if you have production experience to share, PRs and issues are welcome at [GeneralUpdate-Samples](https://github.com/GeneralLibrary/GeneralUpdate-Samples/issues).
