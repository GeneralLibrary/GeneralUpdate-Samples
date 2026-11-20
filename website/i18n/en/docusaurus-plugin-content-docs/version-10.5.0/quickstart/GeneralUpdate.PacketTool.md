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

> The Simulation module internally runs `dotnet publish` to build test apps, so you need the .NET SDK for that feature. The Mobile module's Project Mode (Build & Locate) also runs `dotnet publish`, which requires the .NET SDK too. Patch / Extension / OSS / Config modules and Mobile's File Mode work without the SDK.

## Seven modules at a glance

| Module | What you provide | What the tool produces | Downstream consumer |
|--------|-----------------|----------------------|---------------------|
| **Patch** | Old version directory + new version directory | `{name}.zip` (contains `.patch` files, new files, `generalupdate.delete.json`) | Server, OSS/CDN, Core Upgrade process |
| **Extension** | Extension source directory + metadata | `{name}_{version}.zip` (contains `manifest.json`) | `GeneralUpdate.Extension` component |
| **OSS Config** | Package name, version, download URL, local ZIP (for hash) | `oss_config.json` (version array) | OSS client, object storage publish flow |
| **Config** | Client/Upgrade `.csproj` files | `generalupdate.manifest.json` + `sample_output/` publish directory | Client/Upgrade bootstrap |
| **Simulation** | Old version directory + patch ZIP | Local update server + `simulation_report.md` | Pre-release QA gate |
| **Hash** | Local file (ZIP) | SHA256 lowercase hex string | Integrity check, server-side version records |
| **Mobile** | APK/AAB file or MAUI/Avalonia Android `.csproj` | `mobile_version_{timestamp}.json` version record | Mobile update server, GeneralUpdate.Avalonia/Maui components |

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

1. **Encrypted file detection**: Scans old/new directories for packed executables (e.g. Themida, VMProtect) or encryption signatures. Detected files are flagged with a warning — they cannot produce valid differential patches and will be bundled as full files.
2. Validates that old/new directories exist and version format is correct.
3. Creates a temp directory `gupatch_yyyyMMddHHmmss`.
4. Recursively compares old vs new: **changed files** → generate `.patch` binary diff; **new files** → copy directly; **deleted files** → record hash to `generalupdate.delete.json`.
5. Compresses the temp directory into `{PackageName}.zip`.
6. Deletes the temp directory; ZIP remains in Output Directory.

> **About encrypted files**: Packed executables (Themida, VMProtect, etc.), obfuscated code, or encrypted binaries will have completely different hashes between versions, making differential algorithms ineffective. Such files are detected and bundled as full files rather than `.patch` diffs. For optimal patch sizes, unpack/decrypt your binaries before publishing.

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
- See [GeneralUpdate.Extension](../doc/GeneralUpdate.Extension) for details

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
- See [GeneralUpdate.Core](../doc/GeneralUpdate.Core) OSS update strategy

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

## Mobile: Mobile packaging

### What problem it solves

If you use `GeneralUpdate.Avalonia.Android` or `GeneralUpdate.Maui.Android` to provide auto-update for your Android apps, each release requires extracting APK/AAB metadata (package name, version, SHA256 hash, file size) and uploading it to the server for version management. The Mobile module consolidates "parse → package → upload → generate version record" into a single interface.

<iframe
  src="//player.bilibili.com/player.html?bvid=BV1ctJF6XEJs&page=1"
  width="100%"
  height="480"
  style={{ borderRadius: '8px', border: 'none' }}
  allowFullScreen
  scrolling="no"
/>

Supports two modes:

- **File mode**: Select an APK or AAB file directly; the tool automatically parses AndroidManifest.xml to extract metadata.
- **Project mode**: Select a `.csproj`, automatically runs `dotnet publish`, then locates and parses the build artifact.

### Inputs

#### File mode

| Field | Required | Description |
|-------|----------|-------------|
| APK/AAB file | ✅ | Select a `.apk` or `.aab` file; format is auto-detected |
| Output Directory | ❌ | Version record JSON output directory; defaults to desktop |
| ProductId | ✅ | Product identifier GUID for server-side product differentiation |
| Platform | ✅ | Target platform (default Android = 4) |
| Product Name | ❌ | Product name, written into the version record |
| Release Notes | ❌ | Release notes |
| Is Forcibly | ❌ | Force update flag |

#### Project mode

| Field | Required | Description |
|-------|----------|-------------|
| `.csproj` file | ✅ | Select a MAUI or Avalonia Android `.csproj`; tool parses `ApplicationId`, `ApplicationDisplayVersion`, `ApplicationVersion` |
| Output Directory | ❌ | Same as above |

### Workflow

1. **Select mode**: Toggle File Mode / Project Mode switch.
   - **File mode**: Click **Select File** to choose `.apk` or `.aab`.
   - **Project mode**: Click **Select Project** to choose a `.csproj`, then click **Build & Locate** to run `dotnet publish` and locate the artifact.
2. **Analyze**: Click Analyze — the tool automatically:
   - Detects file format (APK / Android App Bundle)
   - Extracts `PackageName`, `VersionName`, `VersionCode` from AndroidManifest.xml
   - Computes SHA256 hash and file size
3. **Configure upload**: Review or edit the auto-extracted metadata, fill in ProductId, Product Name, etc.
4. **Upload**: Click Upload to send to the server, automatically generating a version record JSON file.

### What the tool does internally

1. **Format detection**: Identifies package format via file extension (`.apk` / `.aab`) and ZIP internal structure (AndroidManifest.xml location).
   - APK detection: Opens the ZIP file and checks for `AndroidManifest.xml` at the root.
   - AAB detection: Opens the ZIP file and checks for `base/manifest/AndroidManifest.xml`.
   - If the format is neither `.apk` nor `.aab`, the detector returns Unknown.

2. **Metadata parsing**: Uses `AxmlParser` to parse the binary AXML (Android Binary XML) format from the ZIP's `AndroidManifest.xml`. The parsing process works as follows:
   - **String pool extraction**: Reads AXML chunk headers, locates the StringPool chunk (type `0x0001`), and extracts all UTF-16LE encoded strings.
   - **Attribute value extraction**: Walks through Start Element chunks (type `0x0102`), parses the attribute block (20 bytes per attribute), matches attribute names by their string pool index to find `package` and `versionName`, then resolves values via `rawValueIndex` from the string pool.
   - **versionCode extraction**: versionCode is stored as a typed integer attribute (type `0x10` = INT_DEC or `0x11` = INT_HEX), read directly from the `typedValueData` field (offset +16, 4 bytes) as a signed integer.
   - AXML chunk structure reference: https://justanapplication.wordpress.com/category/android/android-binary-xml/

3. **SHA256 computation**: Computes SHA256 hash of the full file, output as a lowercase hex string.

4. **File size**: Reads file length and formats it for human-readable display (B / KB / MB / GB).

5. **Project build** (Project mode only): Selecting a `.csproj` triggers:
   - `MobileCsprojParser.Parse()` reads the `.csproj` XML to extract `TargetFramework` (supports single and multi-TFM; auto-selects the one containing `-android`), `ApplicationId` (mapped to PackageName), `ApplicationDisplayVersion` (mapped to VersionName), `ApplicationVersion` (mapped to VersionCode), `UseMaui` (identifies MAUI projects), `AndroidPackageFormat`, and `AssemblyName`.
   - Runs `dotnet publish "{csprojPath}" -c Release -o "{publishDir}"`.
   - After build, searches `bin/Release/{tfm}/publish/` for `.apk` or `.aab` files, auto-locating the build artifact.
   - Project type display: MAUI (`<UseMaui>true</UseMaui>`) or Avalonia (no UseMaui).

6. **Upload**: Sends the file and form fields via HTTP multipart/form-data. The form fields and their data sources are:

   | Field | Source | Example |
   |-------|--------|---------|
   | `Name` | ProductName (user input) | `MyApp` |
   | `Version` | VersionName (auto-parsed) | `2.0.0` |
   | `Hash` | SHA256 (auto-computed) | `a1b2c3d4...` |
   | `Format` | Package format (auto-detected) | `.apk` / `.aab` |
   | `Size` | File size (auto-computed) | `50000000` |
   | `AppType` | Fixed value `"1"` | `1` |
   | `Platform` | Platform field (default 4=Android) | `4` |
   | `ProductId` | ProductId (user-entered GUID) | `2d974e2a-...` |
   | `IsForcibly` | Force update toggle | `true` / `false` |

   The upload service supports the following configuration:
   - **Server URL**: Server base address
   - **Upload Endpoint**: API path, defaults to `/Packet/Create`
   - **Timeout**: Timeout in seconds
   - **Retry Count**: Retry attempts on failure (exponential backoff)
   - **Auth**: Authentication configuration, supports Basic / Bearer Token / API Key modes (credentials stored encrypted via DPAPI)

7. **Version record export**: After a successful upload, generates `mobile_version_{timestamp}.json` with full version metadata. You can also use **Export Record Only** to generate the record without uploading — the URL field will be `"manual"`.

### Output

```
{OutputDirectory}/mobile_version_20260614120000.json
```

```json
{
  "name": "MyApp",
  "version": "2.0.0",
  "hash": "a1b2c3d4e5f6...",
  "url": "https://server.example.com/packages/app-v2.0.0.apk",
  "packageName": "com.example.myapp",
  "fileSize": 50000000,
  "format": "apk",
  "platform": 4,
  "productId": "2d974e2a-31e6-4887-9bb1-b4689e98c77a",
  "isForcibly": false,
  "releaseDate": "2026-06-14T12:00:00.0000000Z"
}
```

### How downstream consumes it

- Import or upload the generated version record JSON to your update server (e.g. GeneralSpacestation).
- When the client (`GeneralUpdate.Avalonia.Android` or `GeneralUpdate.Maui.Android`) queries the server for version info, the returned data structure maps to the version record's fields.
- The client uses the `hash` field for SHA256 integrity verification after downloading the APK.

### Supported AndroidManifest fields

| Manifest attribute | Field | Description |
|-------------------|-------|-------------|
| `package` | `PackageName` | App package name (unique identifier) |
| `android:versionName` | `VersionName` | Display version (e.g. `2.0.0`) |
| `android:versionCode` | `VersionCode` | Internal version code (integer) |

---

## Recommended release workflow

### Desktop update

This sequence chains the Patch, OSS, Config, and Simulation modules together for desktop app releases:

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

### Mobile update

Use the Mobile module for Android app releases:

```
┌────────────┐    ┌──────────┐    ┌──────────┐
│ Build APK/ │ -> │ Mobile   │ -> │ Ship to  │
│ AAB        │    │ parse &  │    │ update   │
│ artifact   │    │ upload   │    │ server   │
└────────────┘    └──────────┘    └──────────┘
```

1. **Build artifact**: Build `.apk` / `.aab` in CI or locally.
2. **Mobile**: Select file or project, auto-parse metadata, compute SHA256, upload to server, export version record.
3. **Ship**: Confirm upload success; clients check for updates via `GeneralUpdate.Avalonia.Android` or `GeneralUpdate.Maui.Android`.

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
| Mobile analysis shows "Metadata extraction warning" | AXML parser could not find the expected attributes. Common causes: the APK uses resource ID references instead of direct string values; or the APK has been packed/obfuscated and the AndroidManifest.xml structure was modified. You can fill in PackageName, VersionName, and VersionCode manually |
| Mobile Project Mode Build & Locate fails | Verify that the .NET SDK is installed and supports the target framework. Ensure the `.csproj` contains an `-android` target framework. Make sure the output directory `bin/Release/{tfm}/publish/` is not write-protected |
| Mobile Project Mode "Build output not found" | `dotnet publish` succeeded but no `.apk`/`.aab` was found in the expected directory. Check the `AndroidPackageFormat` setting in `.csproj` (default `aab;apk` produces APK first), and verify that the TFM was resolved correctly |
| Mobile upload fails | Check that the server URL and endpoint path are configured correctly. Verify the server authentication method (Basic / Bearer / API Key) matches the tool configuration. Review the tool logs for the HTTP status code and error message |
| Mobile client update fails after upload | Confirm that the version record JSON returned by the server has `Platform` set to `4` (Android), and the `Format` field matches the APK/AAB format the client expects |

---

## Related docs

- [GeneralUpdate.Core](../doc/GeneralUpdate.Core): Client/Upgrade main update flow
- [GeneralUpdate.Differential](../doc/GeneralUpdate.Differential): Differential algorithm Clean/Dirty modes
- [GeneralUpdate.Extension](../doc/GeneralUpdate.Extension): Extension install and version management
- [GeneralUpdate.Avalonia.Android](../doc/GeneralUpdate.Avalonia.Android): Avalonia Android APK update component
- [GeneralUpdate.Maui.Android](../doc/GeneralUpdate.Maui.Android): MAUI Android APK update component
- [Beginner cookbook](./Beginner%20cookbook): Complete end-to-end update walkthrough
