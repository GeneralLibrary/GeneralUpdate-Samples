---
sidebar_position: 3
title: Beginner cookbook
---

# GeneralUpdate beginner cookbook

This cookbook is for first-time GeneralUpdate users. The goal is not to explain every API at once, but to help you complete one full loop: discover an update, download a package, start Upgrade, apply the patch, and return to the new version. After that, each component's responsibility becomes much easier to understand.

## Roles used in the flow

| Role | Sample location | Responsibility | Deep dive |
| --- | --- | --- | --- |
| Client | `src\Client` | Main application; checks versions, downloads packages, and starts Upgrade | [GeneralUpdate.Core](../doc/GeneralUpdate.Core.md) |
| Upgrade | `src\Upgrade` | Independent updater; receives context from Client and applies the patch | [GeneralUpdate.Core](../doc/GeneralUpdate.Core.md) |
| Server | `src\Server` | Returns version metadata, accepts reports, and serves package downloads | [GeneralUpdate.Core](../doc/GeneralUpdate.Core.md) |
| Packet | `src\Server\wwwroot\packages` | Downloadable `.zip` packages and `versions.json` metadata | [GeneralUpdate.Tools](../doc/GeneralUpdate.PacketTool.md) |
| Tools | GeneralUpdate.Tools repository | Generates patch packages, hashes, OSS manifests, manifests, and simulation reports | [GeneralUpdate.Tools](../doc/GeneralUpdate.PacketTool.md) |
| Bowl | `src\Bowl` | Monitors process failures and exports failure data | [GeneralUpdate.Bowl](../doc/GeneralUpdate.Bowl.md) |
| Differential | `src\Diff` / integrated by Core | Generates old/new binary differences and applies them during Upgrade | [GeneralUpdate.Differential](../doc/GeneralUpdate.Differential.md) |

## Step 1: Prepare the repository and runtime

Clone the Samples repository and confirm that `dotnet` works:

```powershell
git clone https://github.com/GeneralLibrary/GeneralUpdate-Samples.git
cd GeneralUpdate-Samples\src
dotnet --info
```

The main Samples flow targets `net8.0`, so the machine must be able to build .NET 8 projects. The current GeneralUpdate.Tools source uses .NET 10; install the .NET 10 SDK as well if you plan to run Tools from source.

**Expected result**: `dotnet --info` prints SDK information, and the `src` directory contains `Client`, `Upgrade`, `Server`, `Bowl`, `Diff`, and related folders.

## Step 2: Run the built-in update sample first

Run this from the `src` directory:

```powershell
.\start.cmd
```

The current script runs `Server\build.bat`, `Upgrade\build.bat`, and `Client\build.bat`, deletes the old `src\run` directory, copies all three Release outputs into `src\run`, and starts:

```text
src\run\ServerSample.exe
src\run\ClientSample.exe
```

**Expected result**:

1. The Server console shows `GeneralUpdate Sample Upgrade Server` and listens on `http://localhost:5000/`.
2. Server loads package metadata from `src\Server\wwwroot\packages\versions.json`.
3. The Client console prints initialization, update request, download progress, and update-info callbacks.
4. After Client finishes downloading, it starts Upgrade; the Upgrade console prints initialization and patch progress.

## Step 3: Understand what Server returns

Client configuration lives in `src\Client\Program.cs`:

```csharp
ReportUrl = "http://127.0.0.1:5000/Upgrade/Report",
UpdateUrl = "http://127.0.0.1:5000/Upgrade/Verification",
AppName = "UpgradeSample.exe",
MainAppName = "ClientSample.exe",
ClientVersion = "1.0.0.0",
UpgradeClientVersion = "1.0.0.0",
ProductId = "2d974e2a-31e6-4887-9bb1-b4689e98c77a",
AppSecretKey = "dfeb5833-975e-4afb-88f1-6278ee9aeff6",
```

Server's `POST /Upgrade/Verification` reads client version, AppType, Platform, ProductId, and UpgradeMode, then filters higher versions from `versions.json`. Each returned item contains fields such as `RecordId`, `Name`, `Hash`, `Url`, `Version`, `AppType`, `Platform`, `Format`, and `Size`.

**Expected result**: when Client is at `1.0.0.0` and `versions.json` contains `2.0.0.0`, Client receives available updates and `AddListenerUpdateInfo` prints the number of returned versions.

## Step 4: Observe what Upgrade does

The Upgrade entry point is `src\Upgrade\Program.cs`. It does not hard-code the server URL; it starts from the update context passed by Client:

```csharp
await new GeneralUpdateBootstrap()
    .AddListenerMultiDownloadStatistics(OnMultiDownloadStatistics)
    .AddListenerMultiDownloadCompleted(OnMultiDownloadCompleted)
    .AddListenerMultiAllDownloadCompleted(OnMultiAllDownloadCompleted)
    .AddListenerMultiDownloadError(OnMultiDownloadError)
    .AddListenerException(OnException)
    .LaunchAsync();
```

Upgrade processes the downloaded package, applies differential patch files, copies new files, and removes obsolete files based on the deletion manifest.

**Expected result**: the Upgrade console prints download/apply progress; the Server console receives `/Upgrade/Report` or `/Update/Report`; files under `src\run` are updated to the target version.

## Step 5: Generate your own patch package with Tools

When you have two release directories, use the Patch page in GeneralUpdate.Tools:

| Tools field | What to select |
| --- | --- |
| Old Directory | The version users currently have, such as `publish\v1.0.0` |
| New Directory | The version you want to release, such as `publish\v2.0.0` |
| Package Name | Include versions, such as `client_1.0.0_to_2.0.0` |
| Version | Target version, such as `2.0.0` |
| Output Directory | Directory where the patch ZIP should be saved |

Tools calls the Core differential pipeline: changed files become `.patch` files, new files are copied directly, deleted files are recorded in `generalupdate.delete.json`, and the result is compressed into a ZIP.

**Expected result**: `{PackageName}.zip` appears in the output directory. If only a few files changed, the differential package is usually much smaller than the full publish directory.

## Step 6: Publish the package to Server

Samples Server reads packages from:

```text
src\Server\wwwroot\packages\
```

Put the patch ZIP there and update `versions.json` in the same directory. A version record needs enough data for Server to match and serve it:

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

**Expected result**: after restarting Server, the startup banner lists the loaded versions; the next Client request can discover your new version record.

## Step 7: Trigger your own update

Make sure `ClientVersion` in `src\Client\Program.cs` is lower than `Version` in `versions.json`, then run `.\start.cmd` again or manually start `ServerSample.exe` and `ClientSample.exe`.

If you want users to inspect release notes before updating, use `AddListenerUpdatePrecheck` in Client. This callback is not only for deciding whether to skip the update; it also receives all version information for the current update, so you can build a dialog that shows release notes, package size, and forced-update flags.

**Expected result**: Client receives update information, the precheck callback prints the version count, returning `false` continues the update, and returning `true` skips the current non-forced update.

## Step 8: Add Bowl reliability protection

Bowl is not a package-generation tool. It monitors processes and captures failure data. The sample is in `src\Bowl\BowlSample\Program.cs`:

```csharp
var processInfo = new MonitorParameter
{
    ProcessNameOrId = "ThrowApp.exe",
    DumpFileName = $"{lastVersion}_fail.dmp",
    FailFileName = $"{lastVersion}_fail.json",
    TargetPath = installPath,
    FailDirectory = Path.Combine(installPath, "fail", lastVersion),
    BackupDirectory = Path.Combine(installPath, lastVersion),
    WorkModel = "Normal"
};
Bowl.Launch(processInfo);
```

In a real product, Bowl usually monitors Client or a critical business process. When the monitored process exits abnormally, Bowl writes dump and failure metadata to `fail\{version}`, which helps diagnose “the app fails to start after update” problems.

**Expected result**: after the monitored process crashes, the `fail` directory contains `.dmp` and `.json` failure files.

## Step 9: Understand where Differential takes effect

You do not need to integrate Differential separately for the normal Core update flow: Core uses the differential pipeline by default. What matters is understanding the two phases:

| Phase | Concept | Who runs it | Result |
| --- | --- | --- | --- |
| Clean | Generate patch from old → new | Tools, CI, or release side | `.patch`, new files, `generalupdate.delete.json` |
| Dirty | Apply app + patch | Upgrade side | Local files become the target version |

`src\Diff\Program.cs` shows the low-level calls:

```csharp
await DifferentialCore.Clean(source, target, patch, new DefaultCleanStrategy(new DefaultCleanMatcher()));
await DifferentialCore.Dirty(source, patch, new DefaultDirtyStrategy(new DefaultDirtyMatcher()));
```

**Expected result**: when only a few files changed, Clean outputs only changed content; after Dirty, the old directory becomes the new version directory.

## Next steps

After this flow works, read these pages in order:

1. [GeneralUpdate.Core](../doc/GeneralUpdate.Core.md): update strategies, event notifications, silent updates, and manifest-based minimal configuration.
2. [GeneralUpdate.Tools](../doc/GeneralUpdate.PacketTool.md): patch packages, Hash, OSS Config, and Simulation.
3. [GeneralUpdate.Differential](../doc/GeneralUpdate.Differential.md): differential algorithms, parallel processing, and Clean/Dirty.
4. [GeneralUpdate.Bowl](../doc/GeneralUpdate.Bowl.md): crash monitoring, backup, and failure recovery.
