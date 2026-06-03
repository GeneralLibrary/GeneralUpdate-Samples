---
sidebar_position: 3
title: Beginner cookbook
---

# GeneralUpdate beginner cookbook

This cookbook is for first-time GeneralUpdate users. The goal is not to explain every API at once, but to help you complete one full loop: discover an update, download a package, start the upgrade, apply the patch, and return to the new version. After that, each component's responsibility becomes much easier to understand.

## Roles used in the flow

| Role | Sample location | Responsibility | Deep dive |
| --- | --- | --- | --- |
| Hub | `src\Hub` | Interactive sample browser; run update scenarios via menu | [GeneralUpdate.Core](../doc/GeneralUpdate.Core.md) |
| Server | `src\Server` | Returns version metadata, accepts reports, and serves package downloads | [GeneralUpdate.Core](../doc/GeneralUpdate.Core.md) |
| Packet | `src\Server\wwwroot\packages` | Downloadable `.zip` packages and `versions.json` metadata | [GeneralUpdate.Tools](./GeneralUpdate.PacketTool.md) |
| Tools | GeneralUpdate.Tools repository | Generates patch packages, hashes, OSS manifests, and simulation reports | [GeneralUpdate.Tools](./GeneralUpdate.PacketTool.md) |
| Bowl | Integrated in Hub Samples | Monitors process failures and exports failure data | [GeneralUpdate.Bowl](../doc/GeneralUpdate.Bowl.md) |
| Differential | Hub Samples + Core default integration | Generates old/new binary differences and applies them during updates | [GeneralUpdate.Differential](../doc/GeneralUpdate.Differential.md) |

## Step 1: Prepare the repository and runtime

Clone the Samples repository and confirm that `dotnet` works:

```powershell
git clone https://github.com/GeneralLibrary/GeneralUpdate-Samples.git
cd GeneralUpdate-Samples
dotnet --info
```

The Hub targets `net10.0`, so you need the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).

**Expected result**: `dotnet --info` prints the .NET 10 SDK, and the `src` directory contains `Hub`, `Server`, `ImDiskDriver`, `content_client`, `content_upgrade`, `gen_packages.ps1`, `Run.cmd`, `Run.ps1`, etc.

## Step 2: Run the built-in update sample

### 2.1 First run — build local component DLLs

The `src\Hub\libs\` directory contains pre-built DLLs. If you modified the GeneralUpdate component source code, rebuild and copy the DLLs:

```powershell
cd src
.\Run.ps1 -BuildLibs
```

`-BuildLibs` auto-detects the sibling `..\GeneralUpdate\src\c#` source tree, builds `GeneralUpdate.Differential`, `GeneralUpdate.Core`, `GeneralUpdate.Bowl`, `GeneralUpdate.Extension`, and `GeneralUpdate.Drivelution` in order, and copies the DLLs into `src\Hub\libs\`.

To try the samples quickly without building from source (using the pre-built DLLs), just run:

```powershell
cd src
.\Run.cmd
```

Or with PowerShell:

```powershell
cd src
.\Run.ps1
```

### 2.2 Sample menu

After startup you will see the interactive menu:

```text
  ╔══════════════════════════════════════╗
  ║    GeneralUpdate Sample Browser      ║
  ╚══════════════════════════════════════╝

  Config: http://localhost:5000

  ═══════════════════════════════════
    1. Complete Update — discover → download → apply
    2. OSS Update
    3. Silent Update
    4. Push Update
    5. Differential Algorithm
    6. Compression Demo
    7. Extension Management
    8. Bowl Process Guardian
    9. ImDisk Quick Install
  ───────────────────────────────────
    0. Exit
  ═══════════════════════════════════
```

### 2.3 Run the first sample

Enter `1` and press Enter to select "Complete Update". Hub will automatically:

1. **Start Server** (if not already running) — prints `[Server] Starting... ✓`
2. Create simulated v1.0.0.0 app files in the `mock_app` directory
3. Request updates from `http://localhost:5000/Upgrade/Verification` via `GeneralUpdateBootstrap`
4. Download the new version package and apply the update
5. Print the updated file list

**Expected result**:

- Server console shows `GeneralUpdate Sample Upgrade Server` listening on `http://localhost:5000/`
- Hub console shows version discovery, download progress, and update completion
- Files in `mock_app` are updated from v1.0.0.0 to v2.0.0.0

## Step 3: Understand what Server returns

Hub uses `appsettings.json` for shared configuration, and each Sample passes these values to `GeneralUpdateBootstrap`. See [src\Hub\appsettings.json](https://github.com/GeneralLibrary/GeneralUpdate-Samples/blob/main/src/Hub/appsettings.json):

```json
{
  "ServerUrl": "http://localhost:5000",
  "AppSecretKey": "dfeb5833-975e-4afb-88f1-6278ee9aeff6",
  "ProductId": "2d974e2a-31e6-4887-9bb1-b4689e98c77a",
  "ClientVersion": "1.0.0.0",
  "UpgradeClientVersion": "1.0.0.0",
  "MainAppName": "Hub.exe",
  "UpgradeAppName": "Hub.exe"
}
```

The "Complete Update" sample in [CompleteUpdateSample.cs](https://github.com/GeneralLibrary/GeneralUpdate-Samples/blob/main/src/Hub/Samples/CompleteUpdateSample.cs) uses `UpdateRequest` to configure the update:

```csharp
var request = new UpdateRequest
{
    UpdateUrl = $"{config.ServerUrl}/Upgrade/Verification",
    ReportUrl = $"{config.ServerUrl}/Upgrade/Report",
    AppSecretKey = config.AppSecretKey,
    InstallPath = mockAppDir,
    ClientVersion = config.ClientVersion,
    MainAppName = config.MainAppName,
    UpdateAppName = config.UpgradeAppName,
    ProductId = config.ProductId
};
```

Server's `POST /Upgrade/Verification` reads client version, AppType, Platform, ProductId, and UpgradeMode, then filters higher versions from `versions.json`. Each returned item contains `RecordId`, `Name`, `Hash`, `Url`, `Version`, `AppType`, `Platform`, `Format`, `Size`, and other fields.

**Expected result**: when Client is at `1.0.0.0` and `versions.json` contains `2.0.0.0`, Client receives available updates.

## Step 4: Understand the GeneralUpdateBootstrap flow

Each Sample is built around a `GeneralUpdateBootstrap` instance. Here's the "Complete Update" example:

```csharp
var bootstrap = new GeneralUpdateBootstrap()
    .SetConfig(request)
    .SetOption(Option.AppType, AppType.Client)
    .AddListenerUpdateInfo((_, e) => { /* handle version info */ })
    .AddListenerMultiDownloadStatistics((_, e) => { /* handle download progress */ })
    .AddListenerException((_, e) => { /* handle exceptions */ });

await bootstrap.LaunchAsync();
```

`GeneralUpdate.Core` now unifies the former `GeneralUpdate.ClientCore` and `GeneralUpdate.Common`. A single NuGet reference gives you all capabilities:

- **Client update management** (former ClientCore): version checks, package downloads, integrity validation, launching the upgrade process
- **Upgrade execution engine** (former Core): standalone upgrade process, file replacement, differential patch application, driver installation
- **Common infrastructure** (former Common): lifecycle tracing, download engine, serialization, and other low-level utilities

**Expected result**: after `LaunchAsync()` completes, the target directory files are updated to the new version.

## Step 5: Generate your own patch package with Tools

When you have two release directories, use the Patch page in GeneralUpdate.Tools:

| Tools field | What to select |
| --- | --- |
| Old Directory | The version users currently have, such as `publish\v1.0.0` |
| New Directory | The version you want to release, such as `publish\v2.0.0` |
| Package Name | Include versions, such as `client_1.0.0_to_2.0.0` |
| Version | Target version, such as `2.0.0` |
| Output Directory | Directory where the patch ZIP should be saved |

The Samples repo also provides `gen_packages.ps1` to quickly generate test packages:

```powershell
cd src
.\gen_packages.ps1
```

This script uses content from `src\content_client\v1.0.0.0\` and `src\content_client\v2.0.0.0\` to generate test patch packages and outputs them to `src\Server\wwwroot\packages\`.

Tools calls the Core differential pipeline: changed files become `.patch` files, new files are copied directly, deleted files are recorded in `generalupdate.delete.json`, and the result is compressed into a ZIP.

**Expected result**: update ZIP files appear in the output directory, and Server's `versions.json` is updated.

## Step 6: Publish the package to Server

Samples Server reads packages from:

```text
src\Server\wwwroot\packages\
```

Put the patch ZIP there and make sure `versions.json` includes the corresponding record. A version record needs enough data for Server to match and serve it:

```json
{
  "PacketName": "client_1.0.0_to_2.0.0",
  "Hash": "sha256-of-the-patch-zip",
  "Version": "2.0.0.0",
  "Url": "http://localhost:5000/File/Download/sha256-of-the-patch-zip",
  "AppType": 1,
  "Platform": 1,
  "ProductId": "2d974e2a-31e6-4887-9bb1-b4689e98c77a",
  "Format": ".zip",
  "IsCrossVersion": true,
  "FromVersion": "1.0.0.0",
  "ToVersion": "2.0.0.0"
}
```

You can calculate the hash with the OSS page in Tools or in CI. The Server download endpoint is `GET /File/Download/{hash}` and supports HTTP Range requests for resume.

**Expected result**: after restarting Hub and running a sample, the Server startup banner lists the loaded versions; the sample can discover your new version record.

## Step 7: Explore the other built-in samples

Hub provides 9 built-in samples covering GeneralUpdate's main use cases:

| # | Sample | Description |
| --- | --- | --- |
| 1 | Complete Update | Standard update loop: discover → download → apply |
| 2 | OSS Update | Object Storage-based update mode |
| 3 | Silent Update | Background download, no user disruption |
| 4 | Push Update | SignalR-based real-time push update |
| 5 | Differential Algorithm | Binary diff Clean/Dirty demo |
| 6 | Compression Demo | Compression middleware demo |
| 7 | Extension Management | Plugin/extension query, download, install |
| 8 | Bowl Process Guardian | Process crash monitoring and Dump collection |
| 9 | ImDisk Quick Install | Driver-level quick install demo |

Try running each sample and observe the results in the `mock_app` directory.

**Expected result**: each sample prints the key steps and results for its scenario.

## Next steps

After this flow works, read these pages in order:

1. [GeneralUpdate.Core](../doc/GeneralUpdate.Core.md): update strategies, event notifications, silent updates, and manifest-based minimal configuration.
2. [GeneralUpdate.Tools](./GeneralUpdate.PacketTool.md): patch packages, Hash, OSS Config, and Simulation.
3. [GeneralUpdate.Differential](../doc/GeneralUpdate.Differential.md): differential algorithms, parallel processing, and Clean/Dirty.
4. [GeneralUpdate.Bowl](../doc/GeneralUpdate.Bowl.md): crash monitoring, backup, and failure recovery.
5. [GeneralUpdate.Tools](./GeneralUpdate.PacketTool.md): patch packages, Hash, OSS Config, and Simulation.

## Sample UI

Sample application interface preview:

![](imgs\sampleclient.png)

![](imgs\sampleupgrade.png)

| Repository |
| --- |
| [ClientSample.sln](https://github.com/GeneralLibrary/GeneralUpdate-Samples/blob/main/src/Client/ClientSample.sln) |
| [UpgradeSample.sln](https://github.com/GeneralLibrary/GeneralUpdate-Samples/blob/main/src/Upgrade/UpgradeSample.sln) |
