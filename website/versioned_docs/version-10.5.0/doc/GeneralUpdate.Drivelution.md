---
sidebar_position: 8
---

# GeneralUpdate.Drivelution

**命名空间:** `GeneralUpdate.Drivelution` | **主要入口:** `GeneralDrivelution`（静态类） | **NuGet 包:** `GeneralUpdate.Drivelution`

## 1. 组件简介

### 1.1 组件概述

**GeneralUpdate.Drivelution** 是面向驱动更新场景的跨平台组件。它把驱动更新中容易出错的步骤拆成统一流水线：平台识别 → 权限检查 → 文件验证（哈希/签名/兼容性） → 备份 → 安装 → 结果验证 → 失败后回滚，并在 Windows、Linux、macOS 上分别调用系统原生工具完成驱动安装。

驱动更新不是普通应用文件替换。应用文件通常只需要下载、解压、覆盖并重启进程；驱动更新会影响内核、设备节点、系统扩展或驱动仓库，因此必须额外关注管理员权限、签名可信度、目标系统和 CPU 架构、安装命令返回值、是否需要系统重启以及失败后的恢复路径。

**核心能力：**

| 能力 | 说明 |
| --- | --- |
| 跨平台抽象 | `GeneralDrivelution.Create()` 自动检测平台（Windows/Linux/macOS）创建对应实现 |
| 标准更新流水线 | 权限检查 → 文件验证（存在/哈希/签名/兼容性） → 备份 → 安装 → 验证 → 失败回滚 |
| 文件验证 | 文件存在检查、SHA256/MD5 哈希校验、Authenticode/GPG/codesign 签名校验、OS/架构兼容性检查 |
| 备份与回滚 | `RequireBackup` 默认为 `true`，失败后自动回滚并保留备份路径供业务显式调用 `RollbackAsync` |
| Windows 安装 | 通过 `pnputil.exe /add-driver /install` 安装 INF 驱动包 |
| Linux 安装 | 支持 `.ko`（`insmod`/`modprobe`）、`.deb`（`dpkg -i`）、`.rpm`（`rpm -ivh`/`dnf install`） |
| macOS 安装 | 支持 `.kext`（`kextload`）、`.dext`（SystemExtensions）、`.pkg`（`installer`） |
| 批量更新 | `BatchUpdateAsync` 支持 `BatchMode.Sequential` 和 `BatchMode.Parallel` |
| 进度报告 | 通过 `IProgress<UpdateProgress>` 上报各步骤的进度、状态和消息 |
| 日志追踪 | `GeneralTracer` 默认控制台 + 按日期轮转文件输出 |

**解决的业务痛点：**
- 硬件厂商客户端需要随应用交付驱动更新，但不同 OS 驱动安装方式差异大
- 驱动安装需要管理员权限、签名校验、架构兼容性检查等多重保障
- 驱动安装失败后需要可靠的回滚机制，避免设备不可用

:::danger 驱动更新 ≠ 普通文件替换
驱动更新会影响内核、设备节点、系统扩展或驱动仓库。**必须关注**：管理员权限、签名可信度、目标系统和 CPU 架构、安装命令返回值、是否需要系统重启以及失败后的恢复路径。生产环境中**永远不要**跳过哈希和签名校验。
:::

**业务使用场景：**
- 硬件厂商客户端：网卡、采集卡、USB 设备、虚拟设备驱动随应用一起交付
- 企业/工业现场：批量扫描驱动包并按清单更新
- 安装器/维护工具：统一处理 Windows/Linux/macOS 驱动差异

### 1.2 环境与依赖

| 项目 | 说明 |
| --- | --- |
| **版本** | `10.5.0-beta.7` |
| **目标框架** | `net8.0` / `net10.0`（多目标） |
| **依赖包** | `Microsoft.Extensions.DependencyInjection`、`Microsoft.Extensions.Logging.Abstractions`、`Microsoft.Extensions.Options` |
| **兼容性** | Windows（完整支持，需管理员权限）/ Linux（需 root/sudo）/ macOS（受 SIP 和系统扩展策略影响） |

---

## 2. 组件功能列表

| 功能名称 | 功能描述 | 类型 | 是否必填 | 备注限制 |
| --- | --- | --- | --- | --- |
| 单驱动快速更新 | `QuickUpdateAsync` 使用默认策略快速更新单个驱动 | 基础 | 可选 | 推荐生产环境使用自定义策略 |
| 自定义策略更新 | `UpdateAsync` 配合 `UpdateStrategy` 和 `DrivelutionOptions` | 基础 | 推荐 | 可控制备份、重试、超时、重启策略 |
| 驱动验证 | 文件存在、SHA256 哈希、签名、OS/架构兼容性检查 | 基础 | 可选 | `ValidateAsync`，可通过策略跳过部分校验 |
| 驱动备份 | 更新前备份驱动文件到指定路径 | 基础 | 自动 | `RequireBackup` 默认为 `true` |
| 驱动回滚 | 从备份路径恢复驱动 | 基础 | 可选 | `RollbackAsync(backupPath)` |
| 目录扫描 | 从目录扫描并解析驱动信息（`.inf`/`.ko`/`.kext` 等） | 基础 | 可选 | `GetDriversFromDirectoryAsync` |
| 批量更新 | 按清单批量更新，支持顺序/并行模式 | 拓展 | 可选 | `BatchUpdateAsync` |
| 平台信息查询 | 获取当前 OS/架构/版本/是否支持 | 基础 | 可选 | `GetPlatformInfo()` |
| 重启行为控制 | `UpdateStrategy.RestartMode` 设定重启意图 | 拓展 | 可选 | 更新流水线不会自动重启，需业务层调用 `RestartHelper` |
| DI 注册 | `AddDrivelution` 扩展方法注册平台服务 | 拓展 | 可选 | 支持 Generic Host / ASP.NET Core |
| 日志追踪 | `GeneralTracer` 运行时诊断日志 | 拓展 | 可选 | 默认开启，可关闭 |

---

## 3. API 配置说明

### 3.1 配置字段（属性 Props）

**DriverInfo：**

| 字段名 | 数据类型 | 默认值 | 是否必填 | 枚举/取值范围 | 说明 |
| --- | --- | --- | --- | --- | --- |
| `Name` | `string` | `""` | 是 | — | 驱动名称 |
| `Version` | `string` | `""` | 推荐 | SemVer 格式 | 驱动版本，扫描目录时会尽量从元数据读取 |
| `FilePath` | `string` | `""` | 是 | 有效文件路径 | Windows: `.inf`；Linux: `.ko`/`.deb`/`.rpm`；macOS: `.kext`/`.dext`/`.pkg` |
| `TargetOS` | `string` | `""` | 可选 | `"Windows"`, `"Linux"`, `"MacOS"` | 为空时不限制 OS |
| `Architecture` | `string` | `""` | 可选 | `"x64"`/`"amd64"`/`"x86"`/`"arm64"`/`"arm"` | 支持常见别名归一化；为空不限制 |
| `HardwareId` | `string` | `""` | 可选 | — | 硬件 ID 或模块别名 |
| `Hash` | `string` | `""` | 可选 | SHA256/MD5 哈希值 | 非空且 `SkipHashValidation = false` 时执行哈希校验 |
| `HashAlgorithm` | `string` | `"SHA256"` | 可选 | `"SHA256"` / `"MD5"` | 哈希算法 |
| `TrustedPublishers` | `List<string>` | `new()` | 可选 | — | 可信发布者列表，非空且未跳过签名校验时验证签名 |
| `Description` | `string` | `""` | 可选 | — | 驱动描述 |
| `ReleaseDate` | `DateTime` | — | 可选 | — | 发布时间 |
| `Metadata` | `Dictionary<string, string>` | `new()` | 可选 | — | 扩展元数据 |

**UpdateStrategy：**

| 字段名 | 数据类型 | 默认值 | 是否必填 | 枚举/取值范围 | 说明 |
| --- | --- | --- | --- | --- | --- |
| `RequireBackup` | `bool` | `true` | 可选 | `true` / `false` | 是否执行备份步骤 |
| `BackupPath` | `string` | `""` | 推荐 | 有效目录路径 | 备份根路径，流水线在该路径下生成 `backup_{Name}_{yyyyMMddHHmmss}` |
| `RestartMode` | `RestartMode` | `Prompt` | 可选 | `None`, `Prompt`, `Delayed`, `Immediate` | 重启意图，流水线不会自动重启系统 |
| `SkipHashValidation` | `bool` | `false` | 可选 | `true` / `false` | 跳过哈希校验（仅调试/受控环境） |
| `SkipSignatureValidation` | `bool` | `false` | 可选 | `true` / `false` | 跳过签名校验（仅调试/受控环境） |
| `TimeoutSeconds` | `int` | `300` | 可选 | 正整数 | 单次更新超时，≤0 时使用 `DrivelutionOptions.DefaultTimeoutSeconds` |
| `RetryCount` | `int` | `3` | 可选 | 正整数 | 重试次数（策略字段，实际重试来自 `DrivelutionOptions`） |
| `RetryIntervalSeconds` | `int` | `5` | 可选 | 正整数 | 重试间隔秒数 |
| `Mode` | `UpdateMode` | `Full` | 可选 | `Full`, `Incremental` | 更新模式 |
| `ForceUpdate` | `bool` | `false` | 可选 | `true` / `false` | 是否强制更新 |
| `Priority` | `int` | `0` | 可选 | — | 优先级 |

**DrivelutionOptions：**

| 字段名 | 数据类型 | 默认值 | 是否必填 | 说明 |
| --- | --- | --- | --- | --- |
| `DefaultBackupPath` | `string` | `"./DriverBackups"` | 可选 | 默认备份路径 |
| `DefaultRetryCount` | `int` | `3` | 可选 | 默认重试次数 |
| `DefaultRetryIntervalSeconds` | `int` | `5` | 可选 | 默认重试间隔 |
| `DefaultTimeoutSeconds` | `int` | `300` | 可选 | 默认超时时间 |
| `DebugModeSkipSignature` | `bool` | `false` | 可选 | 调试模式跳过签名 |
| `DebugModeSkipHash` | `bool` | `false` | 可选 | 调试模式跳过哈希 |
| `ForceTerminateOnPermissionFailure` | `bool` | `true` | 可选 | 权限失败时立即终止 |
| `AutoCleanupBackups` | `bool` | `true` | 可选 | 自动清理旧备份 |
| `BackupsToKeep` | `int` | `5` | 可选 | 保留备份数量 |
| `UseExponentialBackoff` | `bool` | `false` | 可选 | 是否使用指数退避 |

### 3.2 实例方法

**GeneralDrivelution（静态类）：**

| 方法名 | 入参明细 | 使用场景 | 注意事项 |
| --- | ---| --- | --- |
| `Create(DrivelutionOptions?)` | `options` — 全局选项 | 创建当前平台的驱动更新器 | 自动检测平台 |
| `Create(IServiceProvider)` | `serviceProvider` — DI 容器 | 从 DI 容器解析 | 未注册时回退到自动平台创建 |
| `QuickUpdateAsync(DriverInfo, UpdateStrategy?, IProgress<UpdateProgress>?, CancellationToken)` | `driverInfo` — 驱动信息；`strategy` — 可选策略（null 时使用默认）；`progress` — 进度报告；`ct` — 取消令牌 | 快速单驱动更新 | 使用安全默认策略 |
| `ValidateAsync(DriverInfo, CancellationToken)` | `driverInfo` — 驱动信息；`ct` — 取消令牌 | 单独验证驱动文件 | — |
| `GetPlatformInfo()` | 无 | 查询当前平台信息 | 返回 OS/架构/版本/是否支持 |
| `GetDriversFromDirectoryAsync(string, string?, CancellationToken)` | `directoryPath` — 目录路径；`searchPattern` — 搜索模式；`ct` — 取消令牌 | 扫描目录解析驱动信息 | 默认搜索模式与平台相关 |
| `BatchUpdateAsync(IEnumerable<DriverInfo>, UpdateStrategy, BatchMode, IProgress<UpdateProgress>?, CancellationToken)` | `drivers` — 驱动列表；`strategy` — 更新策略；`mode` — 顺序/并行；`progress` — 进度；`ct` — 取消令牌 | 批量更新多个驱动 | 并行模式下底层系统工具可能竞争资源 |

**IGeneralDrivelution：**

| 方法名 | 入参明细 | 使用场景 | 注意事项 |
| --- | ---| --- | --- |
| `UpdateAsync(...)` | 同 `QuickUpdateAsync` | 执行完整更新流水线 | — |
| `ValidateAsync(...)` | 同 `GeneralDrivelution.ValidateAsync` | 单独验证 | — |
| `BackupAsync(DriverInfo, string, CancellationToken)` | `driverInfo`, `backupPath`, `ct` | 单独备份驱动文件 | — |
| `RollbackAsync(string, CancellationToken)` | `backupPath` — 备份路径；`ct` — 取消令牌 | 从备份恢复驱动 | 不同平台恢复逻辑不同 |
| `GetDriversFromDirectoryAsync(...)` | 同 `GeneralDrivelution` | 扫描目录 | — |
| `BatchUpdateAsync(...)` | 同 `GeneralDrivelution` | 批量更新 | — |

### 3.3 回调事件

Drivelution 通过 `IProgress<UpdateProgress>` 报告进度，不提供独立的事件系统。

| 进度字段 | 类型 | 说明 |
| --- | --- | --- |
| `CurrentStatus` | `UpdateStatus` | 当前状态（`Validating`/`BackingUp`/`Updating`/`Verifying`/`Succeeded`/`Failed`/`RolledBack`） |
| `StepName` | `string` | 当前步骤名称 |
| `Percentage` | `int` | 进度百分比 (0-100) |
| `Message` | `string` | 进度消息 |
| `StepIndex` | `int` | 当前步骤索引 |
| `TotalSteps` | `int` | 总步骤数 |

---

## 4. 扩展示例（高阶用法）

### 4.1 组件可扩展能力总览

| 扩展接口 | 说明 |
| --- | --- |
| `IGeneralDrivelution` | 完整替换驱动更新器的所有行为 |
| `IDriverValidator` | 自定义文件验证逻辑 |
| `IDriverBackup` | 自定义备份/恢复策略 |
| `ICommandRunner` | 自定义系统命令执行器 |
| `INetworkDownloader` | 预留接口（网络下载） |
| `BaseDriverUpdater` | 抽象基类，可继承创建新平台实现 |

### 4.2 分场景示例

#### 场景 1：自定义策略 + 回滚处理

【场景说明】生产环境驱动更新，启用所有安全检查，失败时显式回滚。

【示例代码】

```csharp
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
    TimeoutSeconds = 600,
    RestartMode = RestartMode.Prompt,
    SkipHashValidation = false,
    SkipSignatureValidation = false
};

var driver = new DriverInfo
{
    Name = "Graphics Driver",
    Version = "2.1.0",
    FilePath = @"C:\Drivers\graphics.inf",
    TargetOS = "Windows",
    Architecture = "x64",
    Hash = "expected-sha256...",
    HashAlgorithm = "SHA256",
    TrustedPublishers = { "Contoso Hardware Inc." }
};

var progress = new Progress<UpdateProgress>(p =>
{
    Console.WriteLine($"[{p.StepName}] {p.Percentage}%: {p.Message}");
});

var result = await updater.UpdateAsync(driver, strategy, progress);

if (!result.Success)
{
    Console.WriteLine($"Update failed: {result.Error?.Message}");

    if (result.BackupPath is not null)
    {
        Console.WriteLine("Rolling back...");
        await updater.RollbackAsync(result.BackupPath);
    }
}
else if (RestartHelper.IsRestartRequired(strategy.RestartMode))
{
    await RestartHelper.HandleRestartAsync(
        strategy.RestartMode,
        delaySeconds: 60,
        message: "Driver updated. Restart now?");
}
```

【效果&注意事项】
- 生产环境不要跳过哈希和签名校验
- 回滚后驱动恢复到安装前状态，但仍建议提示用户可能需要重启

#### 场景 2：DI 容器集成

【场景说明】在 ASP.NET Core / Generic Host 应用中通过 DI 注册驱动更新服务。

【示例代码】

```csharp
using GeneralUpdate.Drivelution.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDrivelution(options =>
{
    options.DefaultBackupPath = "./DriverBackups";
    options.DefaultTimeoutSeconds = 600;
    options.DefaultRetryCount = 3;
    options.AutoCleanupBackups = true;
    options.BackupsToKeep = 5;
});

var app = builder.Build();

// 在 Controller 或 Service 中使用
app.MapPost("/drivers/update", async (DriverInfo driver, IGeneralDrivelution updater) =>
{
    var result = await updater.UpdateAsync(driver, new UpdateStrategy());
    return result.Success ? Results.Ok(result) : Results.BadRequest(result.Error);
});
```

【效果&注意事项】
- `AddDrivelution` 自动注册平台对应的所有服务
- 支持通过 `IServiceProvider` 创建：`GeneralDrivelution.Create(serviceProvider)`

#### 场景 3：批量并行扫描 + 顺序安装

【场景说明】大型项目先并行扫描验证所有驱动，再按风险分组顺序安装。

【示例代码】

```csharp
using GeneralUpdate.Drivelution;
using GeneralUpdate.Drivelution.Abstractions.Models;

// 1. 并行扫描和验证所有驱动
var drivers = await GeneralDrivelution.GetDriversFromDirectoryAsync(@"C:\DriverPackages");

var compatibleDrivers = new List<DriverInfo>();
foreach (var driver in drivers)
{
    var valid = await GeneralDrivelution.ValidateAsync(driver);
    if (valid)
    {
        Console.WriteLine($"{driver.Name} v{driver.Version}: valid");
        compatibleDrivers.Add(driver);
    }
    else
    {
        Console.WriteLine($"{driver.Name} v{driver.Version}: INVALID, skipping");
    }
}

// 2. 按风险分组
var coreDrivers = compatibleDrivers
    .Where(d => d.Metadata.ContainsKey("RiskLevel") && d.Metadata["RiskLevel"] == "Core")
    .ToList();

var optionalDrivers = compatibleDrivers
    .Except(coreDrivers)
    .ToList();

// 3. 核心驱动顺序安装
if (coreDrivers.Any())
{
    var coreResult = await GeneralDrivelution.BatchUpdateAsync(
        coreDrivers,
        new UpdateStrategy { RequireBackup = true },
        BatchMode.Sequential);  // 顺序安装，降低风险

    Console.WriteLine($"Core drivers: {coreResult.SucceededCount}/{coreResult.SucceededCount + coreResult.FailedCount}");
}

// 4. 可选驱动并行安装
if (optionalDrivers.Any())
{
    var optResult = await GeneralDrivelution.BatchUpdateAsync(
        optionalDrivers,
        new UpdateStrategy { RequireBackup = true },
        BatchMode.Parallel);    // 互不依赖，并行安装

    Console.WriteLine($"Optional drivers: {optResult.SucceededCount}/{optResult.SucceededCount + optResult.FailedCount}");
}
```

【效果&注意事项】
- `BatchMode.Parallel` 不一定更快，底层系统工具可能竞争驱动仓库锁
- 核心驱动建议顺序安装，降低系统风险

---

## 5. 常规使用示例

### 5.1 快速入门示例（最简 demo）

```csharp
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
    Console.WriteLine($"Driver updated successfully in {result.DurationMs}ms.");
else
{
    Console.WriteLine($"Update failed: {result.Error?.Message}");
    foreach (var log in result.StepLogs)
        Console.WriteLine($"  {log}");
}
```

### 5.2 基础参数组合示例

```csharp
using GeneralUpdate.Drivelution;
using GeneralUpdate.Drivelution.Abstractions.Configuration;
using GeneralUpdate.Drivelution.Abstractions.Models;

// 查询平台信息
var platform = GeneralDrivelution.GetPlatformInfo();
Console.WriteLine($"OS: {platform.OperatingSystem}, Arch: {platform.Architecture}");

// 扫描驱动目录
var drivers = await GeneralDrivelution.GetDriversFromDirectoryAsync(@"C:\Drivers");
Console.WriteLine($"Found {drivers.Count} driver(s).");

foreach (var d in drivers)
    Console.WriteLine($"  {d.Name} v{d.Version} ({d.FilePath})");

// 创建更新器
var updater = GeneralDrivelution.Create(new DrivelutionOptions
{
    DefaultBackupPath = @"C:\DriverBackups",
    DefaultRetryCount = 3,
    DefaultTimeoutSeconds = 600
});

// 更新，带进度
var progress = new Progress<UpdateProgress>(p =>
    Console.WriteLine($"{p.StepName}: {p.Percentage}%"));

var strategy = new UpdateStrategy
{
    RequireBackup = true,
    BackupPath = @"C:\DriverBackups\mydevice",
    TimeoutSeconds = 300
};

var result = await updater.UpdateAsync(drivers[0], strategy, progress);
Console.WriteLine($"Result: {(result.Success ? "Success" : "Failed")}");

if (result.Success && RestartHelper.IsRestartRequired(strategy.RestartMode))
    await RestartHelper.HandleRestartAsync(strategy.RestartMode, 60);
```

### 5.3 真实业务落地示例

完整驱动更新工作流，覆盖扫描、验证、安装、回滚、重启：

```csharp
using GeneralUpdate.Drivelution;
using GeneralUpdate.Drivelution.Abstractions.Configuration;
using GeneralUpdate.Drivelution.Abstractions.Models;

// 1. 全局配置
var options = new DrivelutionOptions
{
    DefaultBackupPath = @"C:\ProgramData\MyProduct\DriverBackups",
    DefaultRetryCount = 2,
    DefaultRetryIntervalSeconds = 10,
    DefaultTimeoutSeconds = 900,
    UseExponentialBackoff = true,
    ForceTerminateOnPermissionFailure = true,
    AutoCleanupBackups = true,
    BackupsToKeep = 3
};

// 2. 创建更新器
var updater = GeneralDrivelution.Create(options);

// 3. 扫描驱动目录
var platform = GeneralDrivelution.GetPlatformInfo();
Console.WriteLine($"Running on {platform.OperatingSystem} {platform.Architecture}");

var drivers = await GeneralDrivelution.GetDriversFromDirectoryAsync(
    @"C:\ProgramData\MyProduct\DriverPackages");

var compatible = drivers
    .Where(d => string.IsNullOrEmpty(d.TargetOS) || d.TargetOS == platform.OperatingSystem)
    .ToList();

Console.WriteLine($"Found {drivers.Count} driver(s), {compatible.Count} compatible.");

// 4. 逐驱动验证并更新
var results = new List<(DriverInfo Driver, UpdateResult Result)>();
foreach (var driver in compatible)
{
    // 先验证
    var valid = await GeneralDrivelution.ValidateAsync(driver);
    if (!valid)
    {
        Console.WriteLine($"[SKIP] {driver.Name}: validation failed");
        continue;
    }

    // 再更新
    var strategy = new UpdateStrategy
    {
        RequireBackup = true,
        BackupPath = Path.Combine(options.DefaultBackupPath, driver.Name),
        TimeoutSeconds = 600,
        RestartMode = RestartMode.Prompt,
        SkipHashValidation = false,
        SkipSignatureValidation = false
    };

    var progress = new Progress<UpdateProgress>(p =>
    {
        if (p.Percentage % 25 == 0 || p.CurrentStatus == UpdateStatus.Succeeded)
            Console.WriteLine($"[{driver.Name}] {p.StepName} {p.Percentage}%: {p.Message}");
    });

    var result = await updater.UpdateAsync(driver, strategy, progress);
    results.Add((driver, result));

    if (!result.Success)
    {
        Console.WriteLine($"[FAIL] {driver.Name}: {result.Error?.Message}");
        if (result.BackupPath != null)
        {
            Console.WriteLine($"  Rolling back {driver.Name}...");
            await updater.RollbackAsync(result.BackupPath);
        }
    }
    else
    {
        Console.WriteLine($"[OK] {driver.Name} v{driver.Version} ({result.DurationMs}ms)");
    }
}

// 5. 汇总并处理重启
var succeeded = results.Count(r => r.Result.Success);
var failed = results.Count(r => !r.Result.Success);
Console.WriteLine($"\nSummary: {succeeded} succeeded, {failed} failed.");

if (succeeded > 0 && results.Any(r => RestartHelper.IsRestartRequired(r.Result.Status == UpdateStatus.Succeeded
    ? RestartMode.Prompt : RestartMode.None)))
{
    Console.WriteLine("Some drivers may require a restart.");
    var userAccepted = RestartHelper.PromptUserForRestart("Driver update completed. Restart now?");
    if (userAccepted)
        await RestartHelper.RestartSystemAsync();
}
```

---

## 6. 全局配置

### 平台差异速查

| 平台 | 驱动格式 | 安装命令 | 签名验证 | 权限要求 |
| --- | --- | --- | --- | --- |
| Windows | `.inf` | `pnputil.exe /add-driver /install` | Authenticode | 管理员 |
| Linux | `.ko` / `.deb` / `.rpm` | `insmod`/`modprobe` / `dpkg -i` / `rpm -ivh` | GPG（`.sig`/`.asc`） | root/sudo |
| macOS | `.kext` / `.dext` / `.pkg` | `kextload` / SystemExtensions / `installer` | `codesign -v` | root（SIP 和用户批准策略影响） |

### 日志配置

```csharp
// 性能敏感场景关闭日志
GeneralTracer.SetTracingEnabled(false);

// 排查问题时重新开启
GeneralTracer.SetTracingEnabled(true);
```

---

## 相关资源

- [驱动更新示例](https://github.com/GeneralLibrary/GeneralUpdate-Samples/tree/main/src/Hub/Samples/ImDiskQuickInstallSample.cs)
- [GeneralUpdate 仓库](https://github.com/GeneralLibrary/GeneralUpdate)
- [GeneralUpdate.Drivelution 组件文档](../doc/GeneralUpdate.Drivelution)
