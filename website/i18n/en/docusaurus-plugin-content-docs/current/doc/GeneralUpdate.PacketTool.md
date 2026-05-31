---
sidebar_position: 11
---

# GeneralUpdate.Tools

GeneralUpdate.Tools is the desktop toolbox for the GeneralUpdate release workflow. It brings patch packaging, extension packaging, OSS version manifests, minimal runtime configuration, and local update simulation into one Avalonia application, so developers and release operators can produce standard artifacts before shipping.

Repository: [https://github.com/GeneralLibrary/GeneralUpdate.Tools](https://github.com/GeneralLibrary/GeneralUpdate.Tools)

## Outline

| Topic | Tool page | Artifact | Main consumer |
| --- | --- | --- | --- |
| [Launch requirements](#launch-requirements) | `GeneralUpdate.Tools` | Desktop app | Release/test operators |
| [Patch Package](#patch-package) | Patch | `{packageName}.zip` | Server, OSS, Core/Upgrade |
| [Extension Package](#extension-package) | Extension | `{name}_{version}.zip`, `manifest.json` | GeneralUpdate.Extension |
| [OSS Config](#oss-config) | OSS | `oss_config.json` | OSS update clients and object-storage releases |
| [Config](#config-generalupdatemanifestjson) | Config | `generalupdate.manifest.json`, `sample_output` | Client/Upgrade startup configuration |
| [Simulation](#simulation) | Simulate | Local server, test apps, `simulation_report.md` | Pre-release validation |
| [Hash](#hash-sha256) | OSS / internal service | Lowercase SHA256 hex | Package integrity checks |

## Launch requirements

The current tool is built with Avalonia 12 and .NET 10. For development runs, install the .NET 10 SDK and run this from the Tools repository:

```powershell
dotnet run --project src\GeneralUpdate.Tools.csproj
```

If you use a published desktop build, start the executable directly. The Simulation page calls `dotnet publish` for the built-in test apps, so the machine must be able to run `dotnet --version`; the current source validates .NET 10 or a newer preview SDK.

## How artifacts enter the GeneralUpdate workflow

| Tools artifact | Contents | Where it goes | How it is consumed |
| --- | --- | --- | --- |
| Patch ZIP | Differential `.patch` files, new files, deletion manifest | Update server static directory, OSS/CDN, artifact storage | Server/OSS returns the URL; Core downloads it and the Upgrade process applies it |
| `generalupdate.delete.json` | Hash list for files deleted in the new version | Inside the Patch ZIP | Core `DiffPipeline.DirtyAsync` deletes matching old files by hash |
| `manifest.json` | Extension metadata | Inside the Extension ZIP | Extension Host reads it during install/update |
| `oss_config.json` | Package name, version, URL, SHA256 array | OSS configuration location or release-config repo | OSS clients discover and verify versions |
| `generalupdate.manifest.json` | Main app name, upgrade app name, version, ProductId, UpdatePath | Client output root | Reduces startup code; add sensitive values such as service URL and AppSecretKey separately |
| `simulation_report.md` | Simulation parameters, result, log timeline | Tested app directory | Release review and failure analysis |

:::info Deletion manifest name
Older material or task descriptions may call this `delete_files.json`. The current Core pipeline actually generates and consumes `generalupdate.delete.json`. They describe the same deletion-manifest concept; use the current source file name when publishing.
:::

## Patch Package

The Patch page compares an old release directory with a new release directory and outputs only changed content. The current implementation calls `GeneralUpdate.Core.Pipeline.DiffPipeline.CleanAsync(oldDir, newDir, patchDir)` and then zips the temporary patch directory.

### Inputs

| Field | Description |
| --- | --- |
| Old Directory | The version currently installed by users |
| New Directory | The target version to release |
| Package Name | Output ZIP name; if empty, the tool uses `patch_yyyyMMddHHmmss` |
| Version | Semantic version; must match `MAJOR.MINOR.PATCH`, with optional prerelease/build metadata |
| Output Directory | Output location; if empty, the desktop is used |

### Output rules

1. The tool creates a `gupatch_yyyyMMddHHmmss` temporary directory.
2. Changed files become binary `.patch` files.
3. New files are copied directly while preserving relative paths.
4. Deleted files are recorded in `generalupdate.delete.json`.
5. The temporary directory is compressed into `{PackageName}.zip` and then removed.

The Core differential pipeline supports parallel processing. The default `MaxDegreeOfParallelism` is `2`, which is safe for regular developer machines. Large products or CI servers can tune `DiffPipelineOptions` in code, but should avoid saturating disk I/O. The Tools Patch page uses the default pipeline to provide a stable one-click workflow.

## Extension Package

The Extension page compresses an extension directory into a standard ZIP and writes `manifest.json` into the ZIP root. GeneralUpdate.Core integrates Differential by default, but extension packages are consumed by [GeneralUpdate.Extension](./GeneralUpdate.Extension.md); developers do not need to handcraft the ZIP layout.

### Inputs and validation

| Field | Description |
| --- | --- |
| Name | Extension name, also used in the output file name |
| Version | Semantic version |
| Description / Publisher / License | Extension metadata |
| Extension Directory | Existing directory to package |
| Export Directory | Output directory; if empty, the desktop is used |
| Custom Properties | Key/value pairs written to `manifest.json.customProperties` |

### Output

The output file name is `{Sanitize(Name)}_{Version}.zip`. The ZIP keeps the extension files and adds:

```json
{
  "name": "MyExtension",
  "version": "1.0.0",
  "description": "...",
  "publisher": "...",
  "license": "MIT",
  "dependencies": "",
  "minHostVersion": "",
  "maxHostVersion": "",
  "isPreRelease": false,
  "customProperties": {
    "key": "value"
  }
}
```

Extension Host uses these fields for installation, compatibility checks, dependency handling, and local catalog maintenance. See [GeneralUpdate.Extension](./GeneralUpdate.Extension.md) for runtime behavior.

## OSS Config

The OSS page generates a version manifest for object storage or CDN releases. It does not upload files; it organizes the package version, download URL, and SHA256 hash.

| Field | Description |
| --- | --- |
| PacketName | Update package name |
| Version | Semantic version |
| Url | URL where the client can download the patch package |
| SHA256 | Calculated from a local file with ComputeHash, or filled by a release system |
| ReleaseDate | Reserved model field; the current page primarily shows package name, hash, and version |

After Add To List, the entry is added to the list. Export saves an `oss_config.json` array:

```json
[
  {
    "PacketName": "Packet",
    "Hash": "f2c7...",
    "Version": "1.0.0",
    "Url": "https://cdn.example.com/patch_1.0.0.zip",
    "ReleaseDate": ""
  }
]
```

The OSS client downloads the patch package from `Url` and verifies it with `Hash`. See [GeneralClient.OSS](./GeneralClient.OSS.md) for the OSS update path.

## Config: `generalupdate.manifest.json`

The Config page generates `generalupdate.manifest.json`. Its purpose is to save developer time: fixed values such as the main app name, upgrade app name, version, ProductId, and UpdatePath live in the file, so application startup only needs to provide sensitive values such as the service URL and AppSecretKey.

### Workflow

1. Select the Client `.csproj`.
2. Optionally select the Upgrade `.csproj`.
3. Click Analyze. The tool reads AssemblyName and TargetFramework from `.csproj`, then fills `MainAppName` and `UpdateAppName`.
4. Fill `ClientVersion`, `UpgradeClientVersion`, `AppType`, `ProductId`, and `UpdatePath`.
5. Click Generate. The tool writes `generalupdate.manifest.json` to the Tools runtime directory.

Example output:

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

Generate Sample also runs `dotnet publish`: it publishes Client into the `sample_output` root, Upgrade into the `UpdatePath` subdirectory, and writes the same manifest. This is useful for quickly creating a runnable sample layout.

## Simulation

The Simulation page turns “can this patch really update?” into a local closed-loop test. It publishes the built-in `ClientSample` and `UpgradeSample`, starts a local update server, runs Client, triggers Upgrade, and writes a report.

### Inputs

| Field | Description |
| --- | --- |
| Old App Directory | Directory used as the tested app root; it must already exist |
| Patch File | ZIP generated by the Patch page or CI |
| CurrentVersion / TargetVersion | Semantic versions |
| Platform | Current page provides Windows / Linux |
| AppType | ClientApp / UpgradeApp |
| AppSecretKey / ProductId / UpdatePath | Passed to the sample client and local server |
| ServerPort | Defaults to 5000; the started service records the actual port |

### Simulation flow

1. Validate directories, patch file, versions, and .NET SDK.
2. Publish `test_app\Client\ClientSample.csproj` to App Directory.
3. Publish `test_app\Upgrade\UpgradeSample.csproj` to `UpdatePath`.
4. Generate `generalupdate.manifest.json` in App Directory.
5. Copy the patch package to `.server` and compute SHA256.
6. Start the local endpoints: `POST /Upgrade/Verification`, `POST /Upgrade/Report`, `GET /patch/{filename}`.
7. Run `ClientSample.exe --server-url ... --app-secret ... --client-version ...`.
8. Stop the server, inspect the result, and generate `simulation_report.md`.

The report includes configuration, PASS/FAIL, notes, and the full timeline. It is suitable for release records or CI artifacts.

## Hash SHA256

The OSS page's ComputeHash action calculates SHA256 for a selected local file and outputs lowercase hexadecimal text. After uploading a Patch ZIP to CDN/OSS, write the hash of the final uploaded file into the version manifest or server response. Client-side verification catches corrupted downloads, wrong uploads, and stale cache content.

## Recommended release order

1. Produce old/new release directories through CI or manual publish.
2. Use Patch to generate the patch ZIP from old/new.
3. Use OSS to calculate SHA256, fill the download URL, and export `oss_config.json`; if you use a custom Server, put the same URL, Version, and Hash into the server version source.
4. Use Config to generate or check `generalupdate.manifest.json`.
5. Use Simulation with the old app directory and patch ZIP, and confirm PASS.
6. Publish the patch ZIP, OSS manifest, or server version configuration.

## Troubleshooting

| Symptom | Cause and fix |
| --- | --- |
| Version invalid | Tools validates SemVer. Use `1.0.0` or `1.2.0-beta.1`, not `1.0` |
| Patch output is empty | old/new have no detectable difference, or the directories are reversed |
| Cannot find `delete_files.json` | The current file name is `generalupdate.delete.json`; inspect the Patch ZIP |
| Simulation requires .NET SDK | Install .NET 10 SDK and ensure `dotnet --version` works |
| Simulation port is occupied | Change `ServerPort` or release local port 5000 |
| Hash verification fails | Recalculate the hash from the final uploaded ZIP, not an earlier local copy |
| Upgrade does not start | Check `updateAppName` and `updatePath` in the manifest against the published layout |

## Related docs

- [GeneralUpdate.Core](./GeneralUpdate.Core.md): Client/Upgrade workflow, events, silent updates, and manifest usage.
- [GeneralUpdate.Differential](./GeneralUpdate.Differential.md): Differential algorithms, Clean/Dirty modes, and parallel processing.
- [GeneralClient.OSS](./GeneralClient.OSS.md): OSS update workflow.
- [GeneralUpdate.Extension](./GeneralUpdate.Extension.md): Extension installation, update, compatibility, and dependencies.
- [GeneralUpdate.Bowl](./GeneralUpdate.Bowl.md): Reliable download and resume support.
