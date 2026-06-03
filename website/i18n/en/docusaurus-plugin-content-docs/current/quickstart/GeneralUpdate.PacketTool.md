---
sidebar_position: 11
---

# GeneralUpdate.Tools

## What this is

GeneralUpdate.Tools is a cross-platform desktop application built on Avalonia 12 (Windows / Linux / macOS). It helps you generate and manage patch packages, extension packages, version manifests, and run local update simulations as part of your software release workflow. It does not replace your CI/CD system — it consolidates the repetitive tasks of "package, verify, validate" into a single visual tool.

Repository: [https://github.com/GeneralLibrary/GeneralUpdate.Tools](https://github.com/GeneralLibrary/GeneralUpdate.Tools)

## Download & Run

### Option 1: Run from source (recommended for developers)

The tool targets .NET 10. Make sure you have [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) installed:

```powershell
git clone https://github.com/GeneralLibrary/GeneralUpdate.Tools.git
cd GeneralUpdate.Tools\src
dotnet run --project GeneralUpdate.Tools.csproj
```

### Option 2: Download pre-built binaries

Go to [GeneralUpdate.Tools Releases](https://github.com/GeneralLibrary/GeneralUpdate.Tools/releases) and download the executable for your platform.

> The Simulation module internally runs `dotnet publish` to build test apps, so you need the .NET SDK for that feature. Patch / Extension / OSS / Config modules work without the SDK.

## Six modules at a glance

| Module | What you provide | What the tool produces | Downstream consumer |
|--------|-----------------|----------------------|---------------------|
| **Patch** | Old version directory + new version directory | `{name}.zip` (contains `.patch` files, new files, `generalupdate.delete.json`) | Server, OSS/CDN, Core Upgrade process |
| **Extension** | Extension source directory + metadata | `{name}_{version}.zip` (contains `manifest.json`) | `GeneralUpdate.Extension` component |
| **OSS Config** | Package name, version, download URL, local ZIP (for hash) | `oss_config.json` (version array) | OSS client, object storage publish flow |
| **Config** | Client/Upgrade `.csproj` files | `generalupdate.manifest.json` + `sample_output/` publish directory | Client/Upgrade bootstrap |
| **Simulation** | Old version directory + patch ZIP | Local update server + `simulation_report.md` | Pre-release QA gate |
| **Hash** | Local file (ZIP) | SHA256 lowercase hex string | Integrity check, server-side version records |

---

## Patch: Generate patch packages

### What problem it solves

You shipped a new version, but you don't want users to download the entire install package. The Patch module compares old and new version directories and outputs only what changed: modified files become binary `.patch` diffs, new files are copied directly, and deleted files are recorded in a manifest. The resulting differential package is typically much smaller than the full publish directory.

### Inputs

| Field | Required | Description |
|-------|----------|-------------|
| Old Directory | ✅ | The currently installed old version directory, e.g. `publish\v1.0.0` |
| New Directory | ✅ | The new version directory to release, e.g. `publish\v2.0.0` |
| Package Name | ❌ | Output ZIP name (without `.zip`); auto-generated as `patch_yyyyMMddHHmmss` if left empty |
| Version | ✅ | Target version number in `MAJOR.MINOR.PATCH` format (e.g. `2.0.0`) or `MAJOR.MINOR.PATCH-prerelease+build` |
| Output Directory | ❌ | Where to save the ZIP; defaults to desktop |

### What the tool does internally

1. Validates that old/new directories exist and version format is correct.
2. Creates a temp directory `gupatch_yyyyMMddHHmmss`.
3. Recursively compares old vs new: **changed files** → generate `.patch` binary diff; **new files** → copy directly; **deleted files** → record hash to `generalupdate.delete.json`.
4. Compresses the temp directory into `{PackageName}.zip`.
5. Deletes the temp directory; ZIP remains in Output Directory.

Under the hood it calls `GeneralUpdate.Core.Pipeline.DiffPipeline.CleanAsync(oldDir, newDir, patchDir)` — the same code path your CI scripts use.

### Output

```
{OutputDirectory}/{PackageName}.zip
  ├── changed.dll.patch           ← binary diff
  ├── new_file.dll                ← new file (preserving directory structure)
  └── generalupdate.delete.json    ← deletion manifest
```

### How downstream consumes it

- Place the ZIP in your Server's `wwwroot/packages/` directory and update `Url` and `Hash` in `versions.json`
- Or upload to OSS/CDN and record the download URL in `oss_config.json`
- Core Upgrade downloads the ZIP, then `DiffPipeline.DirtyAsync` applies patches and deletions from the manifest

---

## Extension: Package extensions

### What problem it solves

If you use the `GeneralUpdate.Extension` component to manage plugins/extensions, every extension release requires manually building a standard-format ZIP and generating `manifest.json`. The Extension module saves you from manual compression and hand-written JSON.

### Inputs

| Field | Required | Description |
|-------|----------|-------------|
| Name | ✅ | Unique extension identifier (lowercase + hyphens recommended), e.g. `my-data-exporter` |
| Version | ✅ | Semantic version, e.g. `1.0.0` |
| Description | ❌ | What the extension does |
| Publisher | ❌ | Publisher name |
| License | ❌ | License identifier, e.g. `MIT` |
| Extension Directory | ✅ | Source directory containing all extension files; click Pick to select |
| Export Directory | ❌ | Output directory; defaults to desktop |
| Custom Properties | ❌ | Key-value pairs written to `manifest.json.customProperties` |

### Output

```
{ExportDirectory}/{Name}_{Version}.zip
  ├── manifest.json               ← extension metadata
  ├── bin/
  │   └── MyExtension.dll
  ├── resources/
  └── ...
```

Example `manifest.json`:

```json
{
  "name": "my-data-exporter",
  "version": "1.0.0",
  "description": "Export data to CSV/Excel/JSON",
  "publisher": "MyCompany",
  "license": "MIT",
  "dependencies": "",
  "minHostVersion": "",
  "maxHostVersion": "",
  "isPreRelease": false,
  "platform": { "displayName": "Windows", "value": 1 },
  "format": ".zip",
  "releaseDate": "2026-06-01T00:00:00",
  "customProperties": { "maxConnections": "100" }
}
```

### How downstream consumes it

- Extension Host calls `ExtensionManager.QueryRemoteExtensionsAsync(...)` to list available extensions
- On install, downloads the ZIP, reads `manifest.json` for compatibility checks and dependency resolution
- See [GeneralUpdate.Extension](../doc/GeneralUpdate.Extension.md) for details

---

## OSS Config: Generate OSS version manifest

### What problem it solves

If you use OSS mode updates (static file server), you need to maintain a `versions.json` or `oss_config.json` so clients can discover available versions. The OSS Config module helps you organize version metadata and compute SHA256 hashes.

### Inputs

| Field | Required | Description |
|-------|----------|-------------|
| PacketName | ✅ | Update package name |
| Version | ✅ | Semantic version number |
| Url | ✅ | Full download URL clients can use |
| SHA256 | Recommended | Click ComputeHash and select a local ZIP to auto-fill |

### Workflow

1. Fill in PacketName, Version, Url.
2. Click **ComputeHash**, select the local patch ZIP, SHA256 auto-fills.
3. Click **Add To List** to add the entry.
4. Repeat for all versions.
5. Click **Export** to generate `oss_config.json`.

### Output

```json
[
  {
    "PacketName": "client_1.0.0_to_2.0.0",
    "Hash": "f2c7a8b3...",
    "Version": "2.0.0",
    "Url": "https://cdn.example.com/client_1.0.0_to_2.0.0.zip",
    "ReleaseDate": ""
  }
]
```

### How downstream consumes it

- Upload `oss_config.json` to your OSS bucket or static file server
- OSS client reads this file to discover versions and validates Hash after download
- See [GeneralUpdate.Core](../doc/GeneralUpdate.Core.md) OSS update strategy

---

## Config: Generate bootstrap manifest

### What problem it solves

Client and Upgrade need to know the main app name, updater app name, version, ProductId, and UpdatePath at startup. The Config module reads your `.csproj` files to auto-extract AssemblyName and TargetFramework, then generates `generalupdate.manifest.json` — saving you from manual configuration and typos.

### Inputs

| Field | Required | Description |
|-------|----------|-------------|
| Client .csproj | ✅ | Path to the main app's `.csproj` file |
| Upgrade .csproj | ❌ | Path to the updater app's `.csproj` |
| ClientVersion | ✅ | Current client version, e.g. `1.0.0` |
| UpgradeClientVersion | ✅ | Updater version, e.g. `1.0.0` |
| AppType | ✅ | Client / Upgrade / OssClient / OssUpgrade |
| ProductId | ✅ | Product identifier GUID |
| UpdatePath | ✅ | Relative subdirectory for Upgrade, e.g. `update/` |

### Workflow

1. Click Browse to select Client `.csproj` (required) and Upgrade `.csproj` (optional).
2. Click **Analyze** — the tool extracts AssemblyName and fills `MainAppName` / `UpdateAppName`.
3. Fill in Version, AppType, ProductId, UpdatePath manually.
4. Click **Generate** to produce `generalupdate.manifest.json` in the tool's run directory.
5. Click **Generate Sample** to additionally run `dotnet publish` and produce a runnable output directory.

### Output

`generalupdate.manifest.json`:

```json
{
  "mainAppName": "ClientSample.exe",
  "clientVersion": "1.0.0",
  "appType": "Client",
  "updateAppName": "UpgradeSample.exe",
  "upgradeClientVersion": "1.0.0",
  "productId": "2d974e2a-31e6-4887-9bb1-b4689e98c77a",
  "updatePath": "update/"
}
```

Generate Sample additionally outputs:

```
{tool-run-dir}/sample_output/
  ├── ClientSample.exe             ← Client dotnet publish output
  ├── generalupdate.manifest.json
  └── update/
      └── UpgradeSample.exe        ← Upgrade dotnet publish output
```

---

## Simulation: Local update dry-run

### What problem it solves

You just used the Patch module to generate a package — but you're not sure it actually works in a real update flow. The Simulation module starts a local update server, builds Client/Upgrade samples, and runs a complete "discover version → download patch → apply update" loop on your machine, then outputs a PASS/FAIL report.

### Prerequisites

- .NET 10 SDK (or newer) installed
- `dotnet --version` works
- Local port 5000 available (configurable)

### Inputs

| Field | Required | Description |
|-------|----------|-------------|
| Old App Directory | ✅ | The directory simulating the user's currently installed version |
| Patch File | ✅ | The patch ZIP produced by the Patch module |
| CurrentVersion | ✅ | Old version number (used by the simulated Client for version check) |
| TargetVersion | ✅ | New version number (used for the server-side version source) |
| AppType | ✅ | ClientApp = 1 / UpgradeApp = 2 |
| Platform | ✅ | Windows = 1 / Linux = 2 |
| AppSecretKey | ✅ | Client authentication key |
| ProductId | ✅ | Product identifier GUID |
| UpdatePath | ✅ | Upgrade subdirectory name, e.g. `update/` |
| ServerPort | ❌ | Local server port, defaults to 5000 |

### Simulation process

1. **Validate**: old directory exists, patch ZIP exists, version format valid, .NET SDK available.
2. **Publish Client**: `dotnet publish test_app/Client/ClientSample.csproj` to App Directory.
3. **Publish Upgrade**: `dotnet publish test_app/Upgrade/UpgradeSample.csproj` to `UpdatePath` subdirectory.
4. **Generate manifest**: write `generalupdate.manifest.json` in App Directory.
5. **Prepare patch**: copy patch ZIP to internal `.server` directory, compute SHA256, write server-side version source.
6. **Start server**: local server listens on `http://localhost:{ServerPort}`, serving `POST /Upgrade/Verification`, `POST /Upgrade/Report`, `GET /patch/{filename}`.
7. **Run Client**: launch `ClientSample.exe` with `--server-url http://localhost:5000 --app-secret ... --client-version 1.0.0`.
8. **Stop server**: stop after Client completes or times out.
9. **Generate report**: inspect directory changes and output `simulation_report.md`.

### Output

```
{AppDirectory}/simulation_report.md    ← simulation report (PASS/FAIL + timeline)
```

The report includes a config table, PASS/FAIL verdict, notes, and a full timeline — suitable for pasting into release notes or CI artifacts.

---

## Hash: SHA256 verification

### What problem it solves

After uploading a patch package to CDN/OSS, you must ensure clients download the exact same file. The Hash feature computes SHA256 of a local file and outputs a lowercase hex string. You should write this value into your version manifest or server response data.

### How to use

In the **OSS Config** module, click **ComputeHash**, select a local ZIP file, and the SHA256 field auto-fills. You can also use this standalone to verify any file.

---

## Recommended release workflow

This sequence chains the six modules into a complete release pipeline:

```
┌─────────┐    ┌──────────┐    ┌──────────┐    ┌──────────┐    ┌──────────┐
│ Build    │ -> │ Patch    │ -> │ OSS/Hash │ -> │ Config   │ -> │ Simulation│
│ old & new│    │ generate │    │ compute  │    │ generate │    │ verify    │
└─────────┘    └──────────┘    └──────────┘    └──────────┘    └──────────┘
```

1. **Build release directories**: run `dotnet publish` in CI or locally to get old/new directories.
2. **Patch**: select old/new, generate patch ZIP.
3. **OSS Config**: compute SHA256 on the patch ZIP, fill in download URL, export `oss_config.json` (or write the same info to `versions.json` for a self-hosted Server).
4. **Config**: select Client/Upgrade `.csproj` files, generate `generalupdate.manifest.json`.
5. **Simulation**: select the old version directory and patch ZIP, confirm PASS.
6. **Ship**: upload patch ZIP and OSS manifest to production.

---

## FAQ

| Symptom | Cause & fix |
|---------|------------|
| Version invalid | Use `1.0.0` or `2.0.0-beta.1` format. Do not use `1.0` or `v1.0.0` |
| Patch output empty or tiny | old/new directories may be swapped, or the two are identical. Confirm old is the installed version and new is the target version |
| Cannot find `delete_files.json` | The current filename is `generalupdate.delete.json`, located in the patch ZIP root |
| Simulation complains about missing SDK | Install .NET 10 SDK and verify `dotnet --version` works |
| Simulation port conflict | Change `ServerPort` or free up local port 5000 |
| Hash mismatch after client download | Re-compute hash on the file that was actually uploaded to CDN/OSS; check for proxy re-compression |
| Upgrade process doesn't start | Verify `updateAppName` and `updatePath` in `generalupdate.manifest.json` match the publish directory structure |

---

## Related docs

- [GeneralUpdate.Core](../doc/GeneralUpdate.Core.md): Client/Upgrade main update flow
- [GeneralUpdate.Differential](../doc/GeneralUpdate.Differential.md): Differential algorithm Clean/Dirty modes
- [GeneralUpdate.Extension](../doc/GeneralUpdate.Extension.md): Extension install and version management
- [Beginner cookbook](./Beginner%20cookbook.md): Complete end-to-end update walkthrough
