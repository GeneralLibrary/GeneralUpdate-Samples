---
sidebar_position: 2
---

# Configuration Reference

This document provides a complete listing of all configurable items, option enums, and configuration classes in the GeneralUpdate framework.

---

## 1. ClientCore Configuration

### Configinfo Class

Core configuration class for client updates.

**Namespace:** `GeneralUpdate.Common.Shared.Object`

| Property | Type | Description |
|----------|------|-------------|
| `UpdateUrl` | `string` | Update check API endpoint |
| `ReportUrl` | `string` | Status report API endpoint |
| `AppName` | `string` | Application name (upgrade program) |
| `MainAppName` | `string` | Main application name |
| `UpdateLogUrl` | `string` | Update changelog web URL |
| `AppSecretKey` | `string` | Application secret key for server auth & product branching |
| `ClientVersion` | `string` | Current client version |
| `UpgradeClientVersion` | `string` | Current upgrade client version |
| `InstallPath` | `string` | Installation path for file operations |
| `BlackFiles` | `List<string>` | Files to exclude from updates |
| `BlackFormats` | `List<string>` | File formats to exclude from updates |
| `SkipDirectorys` | `List<string>` | Directories to skip during update |
| `ProductId` | `string` | Product branch unique identifier |
| `Bowl` | `string` | Bowl process path for crash monitoring |
| `Scheme` | `string` | HTTP auth scheme (e.g., Bearer) |
| `Token` | `string` | HTTP auth token |
| `Script` | `string` | Linux permission script path |
| `DriverDirectory` | `string` | Driver files directory path |

### ConfiginfoBuilder (Zero-Config Builder)

**Namespace:** `GeneralUpdate.Common.Shared.Object`

Auto-extracts app name, version, and publisher from `.csproj`:

```csharp
// Minimal config: just UpdateUrl, Token, Scheme
var config = ConfiginfoBuilder
    .Create("https://api.example.com/updates", "your-token", "Bearer")
    .Build();
```

**Auto-extraction rules:**

| Config Item | Source | csproj Field | Maps To |
|-------------|--------|-------------|----------|
| App name | Project file | `<AssemblyName>` or filename | `AppName`, `MainAppName` |
| Version | Project file | `<Version>` | `ClientVersion`, `UpgradeClientVersion` |
| Publisher | Project file | `<Company>` or `<Authors>` | `ProductId` |
| Install path | Runtime directory | — | `InstallPath` |

**Optional overrides:** `SetAppName()`, `SetClientVersion()`, `SetInstallPath()`

---

## 2. UpdateOption Enum

**Namespace:** `GeneralUpdate.Common.Internal`

| Value | Description | Default |
|-------|-------------|---------|
| `Format` | Package file format | `Zip` |
| `Encoding` | Compression encoding | — |
| `DownloadTimeOut` | Download timeout (seconds) | 30 |
| `Patch` | Enable binary differential updates | `true` |
| `BackUp` | Enable pre-update backup | `true` |
| `EnableSilentUpdate` | Enable silent update mode | `false` |

```csharp
await new GeneralClientBootstrap()
    .Option(UpdateOption.DownloadTimeOut, 60)
    .Option(UpdateOption.Encoding, Encoding.UTF8)
    .Option(UpdateOption.Patch, true)
    .Option(UpdateOption.BackUp, true)
    .Option(UpdateOption.EnableSilentUpdate, true)
    .SetConfig(config)
    .LaunchAsync();
```

---

## 3. Global Option Class

**Namespace:** `GeneralUpdate.Common.Internal.Bootstrap`

Static properties for global configuration via `Option.ValueOf<T>(name, default)`:

| Property | Type | Description |
|----------|------|-------------|
| `AppType` | `Option<AppType>` | Application role |
| `DiffMode` | `Option<DiffMode>` | Serial or Parallel diff |
| `Encoding` | `Option<Encoding>` | Character encoding |
| `Format` | `Option<Format>` | Compression format |
| `DownloadTimeout` | `Option<int>` | Download timeout (seconds) |
| `PatchEnabled` | `Option<bool>` | Enable differential patches |
| `BackupEnabled` | `Option<bool>` | Enable file backup |
| `Silent` | `Option<bool>` | Silent update mode |
| `SilentPollIntervalMinutes` | `Option<int>` | Poll interval for silent updates (default 20) |
| `LaunchClientAfterUpdate` | `Option<bool>` | Auto-launch client after update |
| `MaxConcurrency` | `Option<int>` | Max concurrent downloads |
| `EnableResume` | `Option<bool>` | Enable download resume |
| `RetryCount` | `Option<int>` | Download retry count |
| `VerifyChecksum` | `Option<bool>` | Verify SHA256 checksum |
| `RetryInterval` | `Option<int>` | Retry interval (seconds) |

---

## 4. AppType Enum

| Value | Description |
|-------|-------------|
| `Client` | Standard client mode (requires server API) |
| `Upgrade` | Standard upgrade mode (requires server API) |
| `OssClient` | OSS client mode (uses object storage) |
| `OssUpgrade` | OSS upgrade mode (uses object storage) |

---

## 5. DiffMode Enum

| Value | Description |
|-------|-------------|
| `Serial` | Process file diffs sequentially |
| `Parallel` | Process file diffs concurrently |

---

## 6. PlatformType Enum

| Value | Description |
|-------|-------------|
| `Unknown` | Unknown platform |
| `Windows` | Microsoft Windows |
| `Linux` | Linux distributions |
| `MacOS` | Apple macOS |

---

## 7. ReportType Constants

| Constant | Value | Description |
|----------|-------|-------------|
| `None` | 0 | Do not report |
| `Success` | 2 | Update succeeded |
| `Failure` | 3 | Update failed |

---

## 8. Server API Specification

### Version Verification

```
POST /Upgrade/Verification
Content-Type: application/json

Request:
{
    "AppSecretKey": "string",
    "ClientVersion": "string",
    "ProductId": "string",
    "ClientType": 1
}

Response:
{
    "Code": 200,
    "Body": [
        {
            "Version": "1.0.1.0",
            "Hash": "sha256:abc123...",
            "UpdateLog": "Update description...",
            "IsForcibly": false,
            "ReleaseDate": "2025-06-01T00:00:00",
            "Size": 1048576,
            "Url": "/patch/patch_v1.0.1.zip"
        }
    ]
}
```

### Status Reporting

```
POST /Upgrade/Report

Request:
{
    "AppSecretKey": "string",
    "ClientVersion": "string",
    "ReportType": 2,
    "ProductId": "string"
}
```

### Patch Download

```
GET /patch/{filename}
Response: application/octet-stream
```

---

## 9. Bowl Configuration

### MonitorParameter Class

| Property | Type | Description |
|----------|------|-------------|
| `TargetPath` | `string` | Monitored directory |
| `FailDirectory` | `string` | Crash data export directory |
| `BackupDirectory` | `string` | Backup directory |
| `ProcessNameOrId` | `string` | Monitored process name or ID |
| `DumpFileName` | `string` | Dump file name |
| `FailFileName` | `string` | Crash report file name |
| `WorkModel` | `string` | "Upgrade" or "Normal" |

### DumpType Enum

| Value | Description |
|-------|-------------|
| `Full` | Full memory dump |
| `Mini` | Mini dump |
| `Heap` | Heap dump |

---

## 10. Extension Configuration

### ExtensionHostOptions

| Property | Type | Description |
|----------|------|-------------|
| `ServerUrl` | `string` | Extension API server URL |
| `Scheme` | `string` | Auth scheme |
| `Token` | `string` | Auth token |
| `HostVersion` | `string` | Host application version |
| `ExtensionsDirectory` | `string` | Extension installation directory |
| `CatalogPath` | `string` | Extension catalog file path |

---

## 11. Drivelution Configuration

### DrivelutionOptions

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `DefaultBackupPath` | `string` | — | Default backup path |
| `DefaultRetryCount` | `int` | 3 | Default retry count |
| `DefaultRetryIntervalSeconds` | `int` | 5 | Retry interval |
| `DefaultTimeoutSeconds` | `int` | 300 | Operation timeout |
| `DebugModeSkipSignature` | `bool` | false | Skip signature validation (debug) |
| `DebugModeSkipHash` | `bool` | false | Skip hash validation (debug) |
| `ForceTerminateOnPermissionFailure` | `bool` | false | Force terminate on permission failure |
| `AutoCleanupBackups` | `bool` | true | Auto-cleanup old backups |
| `BackupsToKeep` | `int` | 5 | Number of backups to retain |
| `UseExponentialBackoff` | `bool` | true | Use exponential backoff for retries |
| `TrustedCertificateThumbprints` | `List<string>` | — | Trusted certificate thumbprints |

---

## 12. Blacklist Configuration

### BlackPolicy Record

| Property | Type | Description |
|----------|------|-------------|
| `Files` | `List<string>` | Blacklisted file names |
| `Formats` | `List<string>` | Blacklisted file extensions |
| `Directories` | `List<string>` | Blacklisted directory names |
| `HasRules` | `bool` | Whether any rules are configured |

### Default Blacklist

| Category | Default Values |
|----------|---------------|
| Files | `System.*.dll` |
| Formats | `.patch`, `.pdb`, `.rar`, `.tar`, `.json`, `.zip` |
| Directories | `app-`, `fail` |

---

## 13. manifest.json Format

Auto-generated configuration file `generalupdate.manifest.json`:

```json
{
    "MainAppName": "MyApp.exe",
    "ClientVersion": "1.0.0.0",
    "AppType": "Client",
    "UpdateAppName": "UpgradeSample.exe",
    "UpgradeClientVersion": "1.0.0.0",
    "ProductId": "2d974e2a-31e6-4887-9bb1-b4689e98c77a",
    "UpdatePath": ""
}
```

---

## Related Resources

- **[Architecture](./Architecture.md)** — Overall system architecture
- **[Pipeline Guide](./Pipeline.md)** — Middleware pipeline internals
- **[Security Guide](./Security.md)** — Security configuration
- **[GeneralUpdate.Tools](../doc/GeneralUpdate.PacketTool)** — Config generation tool
