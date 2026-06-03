---
sidebar_position: 2
---

# 配置参考

本文档完整列出 GeneralUpdate 框架中所有可配置项、选项枚举和配置类。

---

## 一、ClientCore 配置

### Configinfo 类

客户端更新的核心配置类。

**命名空间：** `GeneralUpdate.Common.Shared.Object`

```csharp
public class Configinfo
{
    /// <summary>更新检查 API 地址</summary>
    public string UpdateUrl { get; set; }

    /// <summary>更新状态报告 API 地址</summary>
    public string ReportUrl { get; set; }

    /// <summary>需要启动的应用程序名称（升级程序）</summary>
    public string AppName { get; set; }

    /// <summary>需要启动的主应用程序名称</summary>
    public string MainAppName { get; set; }

    /// <summary>更新日志网页地址</summary>
    public string UpdateLogUrl { get; set; }

    /// <summary>应用密钥，与服务器约定用于身份验证和产品分支</summary>
    public string AppSecretKey { get; set; }

    /// <summary>当前客户端版本号</summary>
    public string ClientVersion { get; set; }

    /// <summary>当前升级客户端版本号</summary>
    public string UpgradeClientVersion { get; set; }

    /// <summary>安装路径（用于更新文件逻辑）</summary>
    public string InstallPath { get; set; }

    /// <summary>黑名单文件列表</summary>
    public List<string> BlackFiles { get; set; }

    /// <summary>黑名单文件格式列表</summary>
    public List<string> BlackFormats { get; set; }

    /// <summary>需要跳过的目录路径列表</summary>
    public List<string> SkipDirectorys { get; set; }

    /// <summary>当前产品分支的唯一 ID</summary>
    public string ProductId { get; set; }

    /// <summary>Bowl 监控进程路径</summary>
    public string Bowl { get; set; }

    /// <summary>HTTP 请求中用于传递 Token 的 Scheme（如 Bearer）</summary>
    public string Scheme { get; set; }

    /// <summary>HTTP 请求中用于身份验证的 Token</summary>
    public string Token { get; set; }

    /// <summary>Linux 平台下的权限脚本</summary>
    public string Script { get; set; }

    /// <summary>驱动程序目录路径</summary>
    public string DriverDirectory { get; set; }
}
```

### ConfiginfoBuilder 零配置构建器

**命名空间：** `GeneralUpdate.Common.Shared.Object`

从 `.csproj` 自动提取应用名称、版本号和发布者信息。

```csharp
// 最简配置：仅需 UpdateUrl、Token、Scheme
var config = ConfiginfoBuilder
    .Create("https://api.example.com/updates", "your-token", "Bearer")
    .Build();

// 自动提取规则
// AssemblyName → AppName / MainAppName
// Version     → ClientVersion / UpgradeClientVersion
// Company     → ProductId
```

**可选覆盖方法：**

| 方法 | 说明 |
|------|------|
| `SetAppName(string)` | 覆盖自动检测的 AppName |
| `SetClientVersion(string)` | 覆盖自动检测的 ClientVersion |
| `SetInstallPath(string)` | 覆盖自动检测的 InstallPath |

---

## 二、UpdateOption 枚举

**命名空间：** `GeneralUpdate.Common.Internal`

```csharp
public enum UpdateOption
{
    /// <summary>更新包文件格式（默认为 Zip）</summary>
    Format,

    /// <summary>压缩编码格式</summary>
    Encoding,

    /// <summary>下载超时时间（秒）。默认 30 秒</summary>
    DownloadTimeOut,

    /// <summary>是否启用二进制差异更新。默认开启</summary>
    Patch,

    /// <summary>是否在更新前启用备份功能。默认开启</summary>
    BackUp,

    /// <summary>是否启用静默更新模式</summary>
    EnableSilentUpdate
}
```

### 使用示例

```csharp
await new GeneralClientBootstrap()
    .Option(UpdateOption.DownloadTimeOut, 60)     // 下载超时 60 秒
    .Option(UpdateOption.Encoding, Encoding.UTF8) // UTF-8 编码
    .Option(UpdateOption.Patch, true)             // 启用差异更新
    .Option(UpdateOption.BackUp, true)            // 启用备份
    .Option(UpdateOption.EnableSilentUpdate, true)// 启用静默更新
    .SetConfig(config)
    .LaunchAsync();
```

---

## 三、全局 Option 配置

**命名空间：** `GeneralUpdate.Common.Internal.Bootstrap`

通过 `Option` 类的静态属性访问全局配置：

```csharp
public class Option
{
    /// <summary>应用类型：Client / Upgrade / OssClient / OssUpgrade</summary>
    public static Option<AppType> AppType { get; }

    /// <summary>差分模式：Serial / Parallel</summary>
    public static Option<DiffMode> DiffMode { get; }

    /// <summary>编码格式</summary>
    public static Option<Encoding> Encoding { get; }

    /// <summary>压缩格式（目前仅 Zip）</summary>
    public static Option<Format> Format { get; }

    /// <summary>下载超时时间（秒）</summary>
    public static Option<int> DownloadTimeout { get; }

    /// <summary>是否启用差分包</summary>
    public static Option<bool> PatchEnabled { get; }

    /// <summary>是否启用备份</summary>
    public static Option<bool> BackupEnabled { get; }

    /// <summary>是否启用静默更新</summary>
    public static Option<bool> Silent { get; }

    /// <summary>静默更新轮询间隔（分钟），默认 20</summary>
    public static Option<int> SilentPollIntervalMinutes { get; }

    /// <summary>更新完成后是否启动客户端</summary>
    public static Option<bool> LaunchClientAfterUpdate { get; }

    /// <summary>最大并发下载数</summary>
    public static Option<int> MaxConcurrency { get; }

    /// <summary>是否启用断点续传</summary>
    public static Option<bool> EnableResume { get; }

    /// <summary>下载重试次数</summary>
    public static Option<int> RetryCount { get; }

    /// <summary>是否校验 SHA256</summary>
    public static Option<bool> VerifyChecksum { get; }

    /// <summary>重试间隔（秒）</summary>
    public static Option<int> RetryInterval { get; }
}
```

### 全局配置使用

```csharp
// 设置全局配置
Option.AppType.SetValue(AppType.Client);
Option.DownloadTimeout.SetValue(120);
Option.PatchEnabled.SetValue(true);
Option.MaxConcurrency.SetValue(5);
Option.RetryCount.SetValue(3);
```

---

## 四、AppType 枚举

```csharp
public enum AppType
{
    /// <summary>标准客户端模式（需要服务端 API）</summary>
    Client,

    /// <summary>标准升级模式（需要服务端 API）</summary>
    Upgrade,

    /// <summary>OSS 客户端模式（使用对象存储）</summary>
    OssClient,

    /// <summary>OSS 升级模式（使用对象存储）</summary>
    OssUpgrade
}
```

---

## 五、DiffMode 枚举

```csharp
public enum DiffMode
{
    /// <summary>串行模式：逐个处理文件差分</summary>
    Serial,

    /// <summary>并行模式：并发处理文件差分</summary>
    Parallel
}
```

---

## 六、Format 枚举

```csharp
public enum Format
{
    /// <summary>ZIP 压缩格式（目前唯一支持）</summary>
    Zip
}
```

---

## 七、PlatformType 枚举

```csharp
public enum PlatformType
{
    Unknown,
    Windows,
    Linux,
    MacOS
}
```

---

## 八、ReportType 常量

```csharp
public static class ReportType
{
    public const int None = 0;     // 不报告
    public const int Success = 2;  // 更新成功
    public const int Failure = 3;  // 更新失败
}
```

---

## 九、服务端 API 规范

### 版本验证接口

```
POST /Upgrade/Verification
Content-Type: application/json

Request:
{
    "AppSecretKey": "string",   // 应用密钥
    "ClientVersion": "string",  // 客户端版本号
    "ProductId": "string",      // 产品分支 ID
    "ClientType": 1             // 客户端类型：1=Client, 2=Upgrade
}

Response:
{
    "Code": 200,
    "Body": [
        {
            "Version": "1.0.1.0",
            "Hash": "sha256:abc123...",
            "UpdateLog": "本次更新内容...",
            "IsForcibly": false,
            "ReleaseDate": "2025-06-01T00:00:00",
            "Size": 1048576,
            "Url": "/patch/patch_v1.0.1.zip"
        }
    ]
}
```

### 状态上报接口

```
POST /Upgrade/Report
Content-Type: application/json

Request:
{
    "AppSecretKey": "string",
    "ClientVersion": "string",
    "ReportType": 2,   // 2=成功, 3=失败
    "ProductId": "string"
}
```

### 补丁包下载

```
GET /patch/{filename}

Response: application/octet-stream (补丁包 .zip 文件)
```

---

## 十、Bowl 配置

### MonitorParameter 类

```csharp
public class MonitorParameter
{
    /// <summary>被监控的目录</summary>
    public string TargetPath { get; set; }

    /// <summary>导出异常信息的目录</summary>
    public string FailDirectory { get; set; }

    /// <summary>备份目录</summary>
    public string BackupDirectory { get; set; }

    /// <summary>被监控进程的名称或 ID</summary>
    public string ProcessNameOrId { get; set; }

    /// <summary>Dump 文件名</summary>
    public string DumpFileName { get; set; }

    /// <summary>升级包版本信息（.json）文件名</summary>
    public string FailFileName { get; set; }

    /// <summary>工作模式："Upgrade" 或 "Normal"</summary>
    public string WorkModel { get; set; } = "Upgrade";
}
```

### DumpType 枚举

```csharp
public enum DumpType
{
    Full = 0,  // 完整转储
    Mini = 1,  // 迷你转储
    Heap = 2   // 堆转储
}
```

---

## 十一、Extension 配置

### ExtensionHostOptions

```csharp
public class ExtensionHostOptions
{
    /// <summary>扩展服务 API 地址</summary>
    public string ServerUrl { get; set; }

    /// <summary>认证 Scheme</summary>
    public string Scheme { get; set; }

    /// <summary>认证 Token</summary>
    public string Token { get; set; }

    /// <summary>主机应用版本号</summary>
    public string HostVersion { get; set; }

    /// <summary>扩展安装目录</summary>
    public string ExtensionsDirectory { get; set; }

    /// <summary>扩展编目文件路径</summary>
    public string CatalogPath { get; set; }
}
```

---

## 十二、Drivelution 配置

### DrivelutionOptions

```csharp
public class DrivelutionOptions
{
    /// <summary>默认备份路径</summary>
    public string DefaultBackupPath { get; set; }

    /// <summary>默认重试次数</summary>
    public int DefaultRetryCount { get; set; } = 3;

    /// <summary>默认重试间隔（秒）</summary>
    public int DefaultRetryIntervalSeconds { get; set; } = 5;

    /// <summary>默认超时时间（秒）</summary>
    public int DefaultTimeoutSeconds { get; set; } = 300;

    /// <summary>调试模式：跳过签名验证</summary>
    public bool DebugModeSkipSignature { get; set; }

    /// <summary>调试模式：跳过哈希验证</summary>
    public bool DebugModeSkipHash { get; set; }

    /// <summary>权限失败时强制终止</summary>
    public bool ForceTerminateOnPermissionFailure { get; set; }

    /// <summary>自动清理旧备份</summary>
    public bool AutoCleanupBackups { get; set; } = true;

    /// <summary>保留的备份数量</summary>
    public int BackupsToKeep { get; set; } = 5;

    /// <summary>使用指数退避重试</summary>
    public bool UseExponentialBackoff { get; set; } = true;

    /// <summary>受信任的证书指纹列表</summary>
    public List<string> TrustedCertificateThumbprints { get; set; }
}
```

---

## 十三、Blacklist (黑名单) 配置

### BlackPolicy

```csharp
public record BlackPolicy
{
    /// <summary>黑名单文件列表</summary>
    public List<string> Files { get; init; }

    /// <summary>黑名单格式列表（如 .log, .cache）</summary>
    public List<string> Formats { get; init; }

    /// <summary>黑名单目录列表</summary>
    public List<string> Directories { get; init; }

    /// <summary>是否有黑名单规则</summary>
    public bool HasRules { get; }
}
```

### 默认黑名单

| 类型 | 默认值 |
|------|--------|
| 文件 | `System.*.dll` |
| 格式 | `.patch`, `.pdb`, `.rar`, `.tar`, `.json`, `.zip` |
| 目录 | `app-`, `fail` |

---

## 十四、manifest.json 格式

客户端自动读取的清单文件 `generalupdate.manifest.json`：

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

| 字段 | 类型 | 说明 |
|------|------|------|
| `MainAppName` | string | 主应用名称 |
| `ClientVersion` | string | 客户端当前版本 |
| `AppType` | string | 应用类型 |
| `UpdateAppName` | string | 升级助手应用名称 |
| `UpgradeClientVersion` | string | 升级助手当前版本 |
| `ProductId` | string | 产品/分支 ID |
| `UpdatePath` | string | 自定义更新路径 |

---

## 相关资源

- **[系统架构](./Architecture.md)** — 了解整体架构
- **[Pipeline 详解](./Pipeline.md)** — 中间件管道工作原理
- **[安全指南](./Security.md)** — 安全配置
- **[GeneralUpdate.Tools](../doc/GeneralUpdate.PacketTool.md)** — 配置生成工具
