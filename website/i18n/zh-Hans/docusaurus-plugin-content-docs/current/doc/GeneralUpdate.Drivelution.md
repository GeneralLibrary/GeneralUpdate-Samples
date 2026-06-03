---
sidebar_position: 12
---

### 定义

命名空间：`GeneralUpdate.Drivelution`

程序集：`GeneralUpdate.Drivelution.dll`

```c#
public static class GeneralDrivelution
```

`GeneralUpdate.Drivelution` 是面向驱动更新场景的跨平台组件。它把驱动更新中容易出错的步骤拆成统一流水线：平台识别、权限检查、文件验证、备份、安装、结果验证、失败后的回滚入口，并在 Windows、Linux、macOS 上分别调用系统原生工具完成驱动安装。

驱动更新不是普通应用文件替换。应用文件通常只需要下载、解压、覆盖并重启进程；驱动更新会影响内核、设备节点、系统扩展或驱动仓库，因此必须额外关注管理员权限、签名可信度、目标系统和 CPU 架构、安装命令返回值、是否需要系统重启以及失败后的恢复路径。Drivelution 只负责操作系统驱动更新，不处理设备内部写入流程。

### 核心能力速览

| 能力 | 当前实现 |
| --- | --- |
| 平台适配 | `GeneralDrivelution.Create()` 自动选择 Windows、Linux 或 macOS 实现。 |
| 标准流水线 | Windows/Linux 会先做权限检查，然后执行 `Validate -> Backup -> Install -> Verify`。macOS 当前包含 `CheckSudo` 步骤并继续执行系统命令，实际安装仍取决于系统权限。 |
| 验证 | 文件存在检查、可选哈希校验、可选签名校验、目标 OS/架构兼容性检查。 |
| 备份 | `UpdateStrategy.RequireBackup` 默认为 `true`；备份路径来自 `UpdateStrategy.BackupPath`。 |
| 安装 | Windows 使用 `pnputil.exe`；Linux 使用 `insmod`/`modprobe`、`dpkg`、`rpm`/`dnf`；macOS 使用 `kextload`、`installer` 等系统工具。 |
| 回滚 | 暴露 `RollbackAsync(backupPath)`；Windows 会尝试重新安装备份中的 `.inf`，Linux 会尝试恢复 `.ko`，macOS 会尝试恢复 `.kext`。 |
| 批量/并行 | `BatchUpdateAsync` 支持 `BatchMode.Sequential` 和 `BatchMode.Parallel`，适合大型项目按驱动清单处理。 |
| 日志 | `GeneralTracer` 默认写入控制台和 `Logs\generalupdate-trace yyyy-MM-dd.log`，可通过 `SetTracingEnabled(false)` 关闭。 |

### 何时使用 Drivelution

适合使用 Drivelution 的场景：

- 硬件厂商客户端需要随应用一起交付网卡、采集卡、USB、虚拟设备等驱动。
- 企业或工业现场需要批量扫描驱动包并按清单更新。
- 安装器、维护工具、设备管理服务需要统一处理 Windows/Linux/macOS 驱动差异。
- 需要在更新前进行哈希、签名、系统架构校验，并在失败后保留可恢复的备份。

不适合把 Drivelution 当作普通文件更新器使用。如果只是更新应用自身的 exe、dll、资源文件或插件，请优先使用 `GeneralUpdate.Core` 的应用更新流程。

### 安装

或在项目文件中添加：

```xml
<PackageReference Include="GeneralUpdate.Drivelution" Version="*" />
```

### 快速开始：更新单个驱动

```c#
using GeneralUpdate.Drivelution;
using GeneralUpdate.Drivelution.Abstractions.Models;

var driver = new DriverInfo
{
    Name = "MyDevice Driver",
    Version = "1.2.0",
    FilePath = @"C:\Drivers\mydevice.inf",
    TargetOS = "Windows",
    Architecture = "x64",
    Hash = "driver-file-sha256",
    HashAlgorithm = "SHA256",
    TrustedPublishers = { "Contoso Hardware" }
};

var result = await GeneralDrivelution.QuickUpdateAsync(driver);

if (result.Success)
{
    Console.WriteLine($"Driver updated. Duration={result.DurationMs}ms");
}
else
{
    Console.WriteLine($"Driver update failed: {result.Error?.Message}");
    Console.WriteLine(string.Join(Environment.NewLine, result.StepLogs));
}
```

`QuickUpdateAsync` 会创建当前平台的更新器，并使用安全默认策略：需要备份、失败可重试 3 次、重试间隔 5 秒。生产环境建议显式传入 `UpdateStrategy`，尤其是备份路径、超时时间和重启策略。

### 使用自定义策略

```c#
using GeneralUpdate.Drivelution;
using GeneralUpdate.Drivelution.Abstractions.Configuration;
using GeneralUpdate.Drivelution.Abstractions.Models;

var options = new DrivelutionOptions
{
    DefaultBackupPath = @"C:\DriverBackups",
    DefaultRetryCount = 3,
    DefaultRetryIntervalSeconds = 5,
    DefaultTimeoutSeconds = 600,
    UseExponentialBackoff = true,
    ForceTerminateOnPermissionFailure = true
};

var updater = GeneralDrivelution.Create(options);

var strategy = new UpdateStrategy
{
    RequireBackup = true,
    BackupPath = @"C:\DriverBackups\graphics",
    RetryCount = 3,
    RetryIntervalSeconds = 5,
    TimeoutSeconds = 600,
    RestartMode = RestartMode.Prompt,
    SkipHashValidation = false,
    SkipSignatureValidation = false
};

var progress = new Progress<UpdateProgress>(p =>
{
    Console.WriteLine($"{p.Percentage}% {p.StepName}: {p.Message}");
});

var result = await updater.UpdateAsync(driver, strategy, progress);

if (!result.Success && result.BackupPath is not null)
{
    await updater.RollbackAsync(result.BackupPath);
}
```

> 注意：`UpdateStrategy.RetryCount` 和 `RetryIntervalSeconds` 是策略模型字段；当前流水线实际重试策略来自 `DrivelutionOptions.DefaultRetryCount`、`DefaultRetryIntervalSeconds` 和 `UseExponentialBackoff`。如果需要统一控制重试行为，请在创建更新器时配置 `DrivelutionOptions`。

### DI 注册

在 Generic Host、ASP.NET Core 或自己的服务容器中，可以通过扩展方法注册当前平台实现：

```c#
using GeneralUpdate.Drivelution.Core;

builder.Services.AddDrivelution(options =>
{
    options.DefaultBackupPath = "./DriverBackups";
    options.DefaultTimeoutSeconds = 600;
});

var updater = GeneralDrivelution.Create(builder.Services.BuildServiceProvider());
```

`AddDrivelution` 会注册 `ICommandRunner`、平台对应的 `IDriverValidator`、`IDriverBackup` 和 `IGeneralDrivelution`。

### API 概览

#### `GeneralDrivelution`

| 方法 | 说明 |
| --- | --- |
| `Create(DrivelutionOptions? options = null)` | 自动检测当前系统并创建平台驱动更新器。 |
| `Create(IServiceProvider serviceProvider)` | 从 DI 容器解析 `IGeneralDrivelution`；未注册时回退到自动平台创建。 |
| `QuickUpdateAsync(driverInfo, strategy?, progress?, token?)` | 使用默认或自定义策略快速更新单个驱动。 |
| `ValidateAsync(driverInfo, token?)` | 使用当前平台验证器检查驱动文件。 |
| `GetPlatformInfo()` | 返回平台、系统、架构、系统版本和是否支持。 |
| `GetDriversFromDirectoryAsync(path, pattern?, token?)` | 从目录扫描并解析驱动信息。 |
| `BatchUpdateAsync(drivers, strategy, mode, progress?, token?)` | 批量更新驱动，可选择顺序或并行。 |

#### `IGeneralDrivelution`

| 方法 | 说明 |
| --- | --- |
| `UpdateAsync(driverInfo, strategy, progress?, token?)` | 执行完整更新流水线。 |
| `ValidateAsync(driverInfo, token?)` | 单独验证驱动。 |
| `BackupAsync(driverInfo, backupPath, token?)` | 单独备份驱动文件。 |
| `RollbackAsync(backupPath, token?)` | 按平台实现尝试从备份恢复。 |
| `GetDriversFromDirectoryAsync(path, pattern?, token?)` | 扫描目录。 |
| `BatchUpdateAsync(drivers, strategy, mode, progress?, token?)` | 批量处理多个驱动。 |

### 数据模型

#### `DriverInfo`

| 属性 | 说明 |
| --- | --- |
| `Name` | 驱动名称。 |
| `Version` | 驱动版本。扫描目录时会尽量从 INF、modinfo、包元数据或 plist 中读取，读取不到时使用 `1.0.0`。 |
| `FilePath` | 驱动文件路径。Windows 通常为 `.inf`，Linux 为 `.ko`/`.deb`/`.rpm`，macOS 为 `.kext`/`.dext`/`.pkg`。 |
| `TargetOS` | 目标系统。为空时兼容性检查视为通过；不为空时需要包含当前系统名，例如 `Windows`、`Linux`、`MacOS`。 |
| `Architecture` | 目标架构。支持常见别名归一化：`x64/amd64/x86_64`、`x86/i386/i686`、`arm64/aarch64`、`arm/armv7`。 |
| `HardwareId` | 硬件 ID 或模块别名。Windows 解析 INF，Linux 可从 `modinfo alias` 读取。 |
| `Hash` / `HashAlgorithm` | 完整性校验。当前支持 `SHA256` 和兼容用 `MD5`。 |
| `TrustedPublishers` | 可信发布者列表。只有该列表非空且未跳过签名校验时才执行签名验证。 |
| `Description`、`ReleaseDate`、`Metadata` | 展示和扩展信息。 |

#### `UpdateStrategy`

| 属性 | 说明 |
| --- | --- |
| `RequireBackup` | 是否执行备份步骤，默认 `true`。 |
| `BackupPath` | 备份根路径。流水线会在该路径下生成 `backup_{Name}_{yyyyMMddHHmmss}`。 |
| `RestartMode` | 重启意图：`None`、`Prompt`、`Delayed`、`Immediate`。当前更新流水线不会自动重启系统，应用可在成功后调用 `RestartHelper.HandleRestartAsync(...)`。 |
| `SkipHashValidation` | 跳过哈希校验。仅建议调试或受控环境使用。 |
| `SkipSignatureValidation` | 跳过签名校验。仅建议调试或受控环境使用。 |
| `TimeoutSeconds` | 单次更新超时；小于等于 0 时使用 `DrivelutionOptions.DefaultTimeoutSeconds`。 |
| `Mode`、`ForceUpdate`、`Priority` | 策略模型保留字段，可供上层调度或 UI 使用。 |

#### `UpdateResult`

| 属性 | 说明 |
| --- | --- |
| `Success` / `Status` | 是否成功以及当前状态：`NotStarted`、`Validating`、`BackingUp`、`Updating`、`Verifying`、`Succeeded`、`Failed`、`RolledBack`。 |
| `Error` | 失败时的错误类型、错误码、消息、详情和堆栈。 |
| `BackupPath` | 本次备份路径。 |
| `RolledBack` | 流水线失败后是否进入回滚路径。若需要强制执行平台恢复，建议显式调用 `RollbackAsync(BackupPath)`。 |
| `StepLogs` | 每个步骤的文本日志，适合展示在安装结果页或上传诊断。 |
| `DurationMs` | 总耗时。 |

### 更新流水线

`BaseDriverUpdater.UpdateAsync` 会按顺序执行当前平台步骤：

1. 平台权限步骤：Windows 为 `CheckPermissions`，Linux 为 `CheckSudo`，macOS 为 `CheckSudo`。
2. `Validate`：检查文件存在、哈希、签名和兼容性。
3. `Backup`：当 `RequireBackup == true` 时执行。
4. `Install`：调用平台安装命令。
5. `Verify`：平台验证安装结果。Windows 会执行 `pnputil.exe /enum-drivers`，验证不确定时记录警告但不让整个更新失败。

每个步骤会通过 `IProgress<UpdateProgress>` 上报 `StepName`、`Percentage`、`Message`、`StepIndex` 和 `TotalSteps`。发生异常或步骤失败时，`UpdateResult.Error` 会映射为可展示的错误信息；如果有备份路径，流水线会进入回滚路径并在 `StepLogs` 中记录。

### 验证策略

Drivelution 的验证逻辑是条件触发的：

- 文件存在是必做项。
- `DriverInfo.Hash` 不为空且 `SkipHashValidation == false` 时，计算文件哈希并与期望值比较。
- `DriverInfo.TrustedPublishers.Count > 0` 且 `SkipSignatureValidation == false` 时，执行签名校验。
- 兼容性校验始终执行；`TargetOS` 或 `Architecture` 为空表示不限制该项。

平台签名行为：

| 平台 | 签名验证 |
| --- | --- |
| Windows | 使用 Authenticode 相关逻辑验证文件签名，并检查可信发布者。 |
| Linux | 查找同名 `.sig` 或 `.asc` 文件并执行 GPG 签名验证；未提供可信发布者时允许无签名通过。 |
| macOS | 使用 `codesign -v`，失败后尝试 `codesign -v --deep`；指定可信发布者时通过 `codesign -dvv` 输出匹配。 |

### 平台差异

#### Windows

Windows 实现面向 INF 驱动包：

- 扫描默认模式：`*.inf`。
- 权限：必须以管理员身份运行，否则 `CheckPermissions` 会失败。
- 安装：`pnputil.exe /add-driver <path> /install`。
- 验证：`pnputil.exe /enum-drivers`，验证不确定时记录警告但不阻断更新。
- 元数据：解析 `DriverVer`、`DriverDesc`、`HardwareId`，并计算 SHA256。
- 回滚：`RollbackAsync` 会扫描备份目录中的 `.inf` 并重新调用 PnPUtil 安装。

#### Linux

Linux 实现支持内核模块和发行版包：

- 扫描默认包括 `.ko`，未指定搜索模式时还会扫描 `.deb` 和 `.rpm`。
- 权限：通过 sudo/root 检查，驱动安装通常需要 root。
- `.ko` 安装：先 `insmod <module.ko>`，失败后回退 `modprobe <moduleName>`。
- `.deb` 安装：`dpkg -i <package.deb>`。
- `.rpm` 安装：先 `rpm -ivh <package.rpm>`，失败后回退 `dnf install -y <package.rpm>`。
- 元数据：`.ko` 通过 `modinfo` 读取版本、描述和 alias；`.deb` 通过 `dpkg-deb -I`；`.rpm` 通过 `rpm -qip`。
- 回滚：当前主要恢复 `.ko`，先尝试 `modprobe -r <moduleName>` 卸载当前模块，再 `insmod <backup.ko>` 加载备份模块。

#### macOS

macOS 实现面向内核扩展、DriverKit 扩展和安装包：

- 扫描默认包括 `.kext`、`.dext`、`.pkg`。
- `.kext` 安装：复制到 `/Library/Extensions/`，设置 `root:wheel` 和 `755`，执行 `kextload`，再执行 `kextcache -i /`。
- `.dext` 安装：复制到 `/Library/SystemExtensions/`；DriverKit 扩展通常还需要用户在系统设置的安全隐私区域批准。
- `.pkg` 安装：`/usr/sbin/installer -pkg <pkg> -target /`。
- 签名：使用 `codesign` 验证。
- 限制：新版 macOS 对 kext、dext 有 SIP、用户批准和系统扩展策略限制；命令成功不代表用户批准流程已完成。
- 回滚：当前主要恢复 `.kext`，复制回 `/Library/Extensions/` 并尝试 `kextload`。

### 批量与并行更新

批量更新适合大型项目把驱动包拆成清单后统一处理：

```c#
var drivers = await GeneralDrivelution.GetDriversFromDirectoryAsync(@"C:\Drivers");

var batch = await GeneralDrivelution.BatchUpdateAsync(
    drivers,
    strategy,
    BatchMode.Parallel,
    progress);

Console.WriteLine(batch);
```

`BatchMode.Sequential` 会按顺序逐个更新，适合核心驱动、互相依赖的驱动或需要降低系统风险的场景。`BatchMode.Parallel` 使用 `Task.WhenAll` 并行处理多个驱动，适合互不依赖的驱动包扫描、验证和安装任务，但底层系统工具可能仍会竞争驱动仓库、包管理器锁或内核模块资源。大型项目建议先并行验证和扫描，再对高风险安装阶段做分组或顺序控制。

### 重启行为

`UpdateStrategy.RestartMode` 表示本次驱动更新完成后的重启意图：

| 值 | 含义 |
| --- | --- |
| `None` | 不需要重启。 |
| `Prompt` | 应用提示用户重启。当前 `RestartHelper.PromptUserForRestart` 只输出提示并返回 `false`，适合由 GUI 自行接管。 |
| `Delayed` | 延迟后调用系统重启命令。 |
| `Immediate` | 立即调用系统重启命令。 |

当前 `UpdateAsync` 不会自动调用 `RestartHelper`，因此不会在驱动安装后直接重启系统。推荐在业务层根据驱动类型和安装结果决定是否调用：

```c#
if (result.Success && RestartHelper.IsRestartRequired(strategy.RestartMode))
{
    await RestartHelper.HandleRestartAsync(
        strategy.RestartMode,
        delaySeconds: 60,
        message: "Driver update completed. Restart now?");
}
```

### 日志与性能开关

Drivelution 使用 `GeneralTracer` 输出内部诊断信息：

- 默认启用。
- 控制台输出：通过 `TextWriterTraceListener(Console.Out)`。
- 文件输出：应用基目录下的 `Logs\generalupdate-trace yyyy-MM-dd.log`，按日期切换。
- Windows 调试输出：Windows 下会额外添加 `WindowsOutputDebugListener`。
- 调试器附加时会添加 `DefaultTraceListener`。

驱动更新通常涉及外部命令和系统权限，日志对排查失败很重要。但 `GeneralTracer` 会生成时间戳、调用栈位置并写入 Trace Listener；在性能敏感、批量验证或大量并行处理场景中，可以关闭它降低额外开销：

```c#
GeneralTracer.SetTracingEnabled(false);

// 执行性能敏感的扫描或批量验证

GeneralTracer.SetTracingEnabled(true);
```

如果需要把日志桥接到自己的 UI 或日志系统，可以使用 `DrivelutionLogger` 的 `LogMessage` 事件自行包装；当前主更新流水线主要使用 `GeneralTracer`。

### 推荐实践

| 场景 | 建议 |
| --- | --- |
| 生产更新 | 保持 `RequireBackup = true`，设置明确的 `BackupPath`，不要跳过哈希和签名。 |
| 首次集成 | 先调用 `ValidateAsync` 和 `GetPlatformInfo()`，在 UI 中展示目标 OS、架构、版本和发布者。 |
| Windows | 以管理员启动进程，并优先使用厂商签名的 INF 包。 |
| Linux | 确认 root/sudo 权限、内核版本和包管理器锁；核心模块建议顺序更新。 |
| macOS | 提前告知用户可能需要批准系统扩展；kext 受 SIP 和系统策略影响较大。 |
| 大批量驱动 | 扫描和验证可并行，安装阶段按驱动风险分组；失败时保留 `StepLogs` 和 `BackupPath`。 |
| 高性能场景 | 批量扫描时可临时关闭 `GeneralTracer`，结束后再恢复。 |

### 常见问题

#### 为什么有时签名校验没有执行？

签名校验只在 `DriverInfo.TrustedPublishers` 非空且 `SkipSignatureValidation == false` 时执行。如果你希望强制校验签名，请提供可信发布者列表，并确保平台对应的签名文件或系统签名信息可用。

#### 为什么设置了 `RestartMode` 但系统没有重启？

`RestartMode` 当前是策略字段，更新流水线不会自动重启系统。应用需要在 `UpdateAsync` 成功后调用 `RestartHelper.HandleRestartAsync(...)`，或用自己的 GUI/服务逻辑接管重启。

#### `BatchMode.Parallel` 是否一定更快？

不一定。并行可以提升扫描、验证和互不依赖任务的吞吐，但驱动安装会调用系统工具，可能遇到驱动仓库锁、包管理器锁、模块依赖或重启要求。大型项目建议先并行验证，再对安装阶段分组控制并发。

#### 回滚应该如何设计？

更新前保留备份路径，失败时读取 `UpdateResult.BackupPath` 和 `StepLogs`。如果业务要求强恢复，显式调用 `RollbackAsync(backupPath)`，并在 UI 中提示用户可能仍需重启或重新插拔设备。
