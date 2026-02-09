---
sidebar_position: 12
---

### 定义

命名空间：GeneralUpdate.Drivelution

程序集：GeneralUpdate.Drivelution.dll



GeneralUpdate.Drivelution 是一个跨平台的驱动更新组件，提供设备驱动的自动检测、验证、备份、安装和回滚功能。它支持 Windows、Linux 和 MacOS 平台，并提供统一的 API 接口，简化驱动更新流程的同时确保安全性和可靠性。

```c#
public static class GeneralDrivelution
```



### 适用场景

GeneralUpdate.Drivelution 适用于以下场景：

- **企业设备管理**：在企业环境中批量更新多个设备的驱动程序
- **IoT 设备更新**：为嵌入式系统和物联网设备提供远程驱动更新
- **硬件厂商软件**：将驱动更新功能集成到硬件厂商的应用程序中
- **系统维护工具**：构建自动化的驱动更新和维护工具
- **跨平台应用**：需要在多个操作系统上管理驱动的应用程序



### 安装与配置

#### 安装

通过 NuGet 包管理器安装：

```bash
dotnet add package GeneralUpdate.Drivelution
```

或在项目文件中添加：

```xml
<PackageReference Include="GeneralUpdate.Drivelution" Version="*" />
```

#### 配置

创建配置选项对象（可选，不指定时使用默认值）：

```c#
var options = new DrivelutionOptions
{
    LogLevel = "Info",                          // 日志级别（Debug/Info/Warn/Error/Fatal）
    LogFilePath = "./Logs/drivelution-.log",    // 日志文件路径
    EnableConsoleLogging = true,                // 启用控制台日志
    EnableFileLogging = true,                   // 启用文件日志
    DefaultBackupPath = "./DriverBackups",      // 默认备份路径
    DefaultRetryCount = 3,                      // 默认重试次数
    DefaultRetryIntervalSeconds = 5,            // 默认重试间隔（秒）
    DefaultTimeoutSeconds = 300,                // 默认超时时间（秒）
    AutoCleanupBackups = true,                  // 自动清理旧备份
    BackupsToKeep = 5                           // 保留的备份数量
};
```



### 快速开始

#### 基础用法（Hello World）

最简单的驱动更新示例：

```c#
using GeneralUpdate.Drivelution;
using GeneralUpdate.Drivelution.Abstractions.Models;

// 1. 创建驱动信息
var driverInfo = new DriverInfo
{
    Name = "我的设备驱动",
    Version = "1.2.0",
    FilePath = "C:\\Drivers\\mydevice.inf",  // Windows: .inf, Linux: .ko, MacOS: .kext
    TargetOS = "Windows",
    Architecture = "x64",
    Hash = "abc123...",                       // 驱动文件的 SHA256 哈希值
    HashAlgorithm = "SHA256"
};

// 2. 使用默认设置快速更新
var result = await GeneralDrivelution.QuickUpdateAsync(driverInfo);

// 3. 检查结果
if (result.Success)
{
    Console.WriteLine("驱动更新成功！");
    Console.WriteLine($"耗时: {result.DurationMs}毫秒");
}
else
{
    Console.WriteLine($"更新失败: {result.Error?.Message}");
}
```

#### 高级用法（自定义策略）

```c#
using GeneralUpdate.Drivelution;
using GeneralUpdate.Drivelution.Abstractions.Models;
using GeneralUpdate.Drivelution.Abstractions.Configuration;

// 1. 创建配置选项
var options = new DrivelutionOptions
{
    LogLevel = "Debug",
    DefaultBackupPath = "C:\\DriverBackups"
};

// 2. 创建更新器实例
var updater = GeneralDrivelution.Create(options);

// 3. 定义驱动信息
var driverInfo = new DriverInfo
{
    Name = "显卡驱动",
    Version = "2.1.0",
    FilePath = "C:\\Drivers\\graphics.inf",
    TargetOS = "Windows",
    Architecture = "x64",
    Hash = "def456...",
    HashAlgorithm = "SHA256",
    HardwareId = "PCI\\VEN_10DE&DEV_1234",
    Description = "NVIDIA 显卡驱动"
};

// 4. 定义更新策略
var strategy = new UpdateStrategy
{
    RequireBackup = true,                      // 启用备份
    BackupPath = "C:\\DriverBackups\\graphics",
    RetryCount = 3,                            // 失败时重试 3 次
    RetryIntervalSeconds = 5,                  // 重试间隔 5 秒
    RestartMode = RestartMode.Prompt,          // 提示用户重启
    SkipSignatureValidation = false,           // 启用签名验证
    TimeoutSeconds = 600                       // 10 分钟超时
};

// 5. 执行更新
var result = await updater.UpdateAsync(driverInfo, strategy);

// 6. 处理结果
if (result.Success)
{
    Console.WriteLine($"✓ 更新成功，耗时 {result.DurationMs} 毫秒");
    if (!string.IsNullOrEmpty(result.BackupPath))
    {
        Console.WriteLine($"✓ 备份保存至: {result.BackupPath}");
    }
}
else
{
    Console.WriteLine($"✗ 更新失败: {result.Error?.Code}");
    Console.WriteLine($"  消息: {result.Error?.Message}");
    
    if (result.RolledBack)
    {
        Console.WriteLine("✓ 系统已回滚至之前状态");
    }
}
```



### 核心模块与 API 说明

#### 主入口类：GeneralDrivelution

静态入口类提供以下方法：

| 方法 | 说明 |
| ---- | ---- |
| `Create(options?)` | 创建驱动更新器实例（自动检测平台） |
| `Create(logger, options?)` | 使用自定义日志器创建驱动更新器实例 |
| `QuickUpdateAsync(driverInfo)` | 使用默认设置快速更新驱动 |
| `QuickUpdateAsync(driverInfo, strategy)` | 使用自定义策略快速更新驱动 |
| `ValidateAsync(driverInfo)` | 验证驱动文件 |
| `GetPlatformInfo()` | 获取当前平台信息 |
| `GetDriversFromDirectoryAsync(path)` | 从本地目录读取驱动信息 |

#### 核心接口：IGeneralDrivelution

更新器接口提供以下能力：

| 方法 | 说明 |
| ---- | ---- |
| `UpdateAsync(driverInfo, strategy, token?)` | 异步更新驱动 |
| `ValidateAsync(driverInfo, token?)` | 异步验证驱动 |
| `BackupAsync(driverInfo, backupPath, token?)` | 异步备份驱动 |
| `RollbackAsync(backupPath, token?)` | 异步回滚驱动 |
| `GetDriversFromDirectoryAsync(path, pattern?, token?)` | 从目录读取驱动信息 |

#### 数据模型

##### DriverInfo

驱动信息模型：

| 属性 | 类型 | 说明 |
| ---- | ---- | ---- |
| `Name` | string | 驱动名称 |
| `Version` | string | 驱动版本（遵循 SemVer 2.0） |
| `FilePath` | string | 驱动文件路径 |
| `TargetOS` | string | 目标操作系统 |
| `Architecture` | string | 系统架构（x86、x64、ARM、ARM64） |
| `HardwareId` | string | 硬件 ID（Windows 硬件 ID 或 Linux PCI/USB 设备 ID） |
| `Hash` | string | 文件哈希值（用于完整性验证） |
| `HashAlgorithm` | string | 哈希算法（SHA256、MD5） |
| `TrustedPublishers` | List\<string\> | 可信发布者列表 |
| `Description` | string | 驱动描述 |
| `ReleaseDate` | DateTime | 驱动发布日期 |
| `Metadata` | Dictionary | 附加元数据 |

##### UpdateStrategy

更新策略模型：

| 属性 | 类型 | 说明 |
| ---- | ---- | ---- |
| `Mode` | UpdateMode | 更新模式（全量/增量） |
| `ForceUpdate` | bool | 是否强制更新 |
| `RequireBackup` | bool | 是否需要备份 |
| `BackupPath` | string | 备份路径 |
| `RetryCount` | int | 失败重试次数 |
| `RetryIntervalSeconds` | int | 重试间隔（秒） |
| `Priority` | int | 更新优先级（用于批量更新） |
| `RestartMode` | RestartMode | 更新后重启模式 |
| `SkipSignatureValidation` | bool | 跳过签名验证（仅调试模式） |
| `SkipHashValidation` | bool | 跳过哈希验证（仅调试模式） |
| `TimeoutSeconds` | int | 超时时间（秒） |

##### UpdateResult

更新结果模型：

| 属性 | 类型 | 说明 |
| ---- | ---- | ---- |
| `Success` | bool | 更新是否成功 |
| `Status` | UpdateStatus | 更新状态 |
| `Error` | ErrorInfo? | 错误信息 |
| `StartTime` | DateTime | 更新开始时间 |
| `EndTime` | DateTime | 更新结束时间 |
| `DurationMs` | long | 更新耗时（毫秒） |
| `BackupPath` | string? | 备份路径（如果有备份） |
| `RolledBack` | bool | 是否已回滚 |
| `Message` | string | 附加消息 |
| `StepLogs` | List\<string\> | 更新步骤日志 |

##### DrivelutionOptions

配置选项：

| 属性 | 类型 | 说明 |
| ---- | ---- | ---- |
| `DefaultBackupPath` | string | 默认备份路径 |
| `LogLevel` | string | 日志级别（Debug/Info/Warn/Error/Fatal） |
| `LogFilePath` | string | 日志文件路径 |
| `EnableConsoleLogging` | bool | 启用控制台日志 |
| `EnableFileLogging` | bool | 启用文件日志 |
| `DefaultRetryCount` | int | 默认重试次数 |
| `DefaultRetryIntervalSeconds` | int | 默认重试间隔（秒） |
| `DefaultTimeoutSeconds` | int | 默认超时时间（秒） |
| `DebugModeSkipSignature` | bool | 调试模式下跳过签名验证 |
| `DebugModeSkipHash` | bool | 调试模式下跳过哈希验证 |
| `ForceTerminateOnPermissionFailure` | bool | 权限检查失败时强制终止 |
| `AutoCleanupBackups` | bool | 自动清理旧备份 |
| `BackupsToKeep` | int | 保留的备份数量 |
| `TrustedCertificateThumbprints` | List\<string\> | 可信证书指纹列表（用于签名验证） |
| `TrustedGpgKeys` | List\<string\> | 可信 GPG 公钥列表（Linux 用） |



### 跨平台使用

GeneralUpdate.Drivelution 支持 Windows、Linux 和 MacOS 平台，具有自动平台检测和适配功能。

#### Windows 平台

**支持的驱动类型：**
- INF 驱动（*.inf）
- PnP 驱动
- 内核模式驱动
- 用户模式驱动

**示例：**

```c#
var driverInfo = new DriverInfo
{
    Name = "USB 设备驱动",
    Version = "3.0.1",
    FilePath = "C:\\Drivers\\usbdevice.inf",
    TargetOS = "Windows",
    Architecture = "x64",
    Hash = "sha256_hash_here",
    HardwareId = "USB\\VID_1234&PID_5678"
};

var strategy = new UpdateStrategy
{
    RequireBackup = true,
    RestartMode = RestartMode.Prompt
};

var updater = GeneralDrivelution.Create();
var result = await updater.UpdateAsync(driverInfo, strategy);
```

**Windows 平台特性：**
- 使用 Windows Authenticode 自动进行签名验证
- 设备驱动的硬件 ID 匹配
- 与 Windows 驱动存储集成
- 通过 PnPUtil 支持驱动安装

#### Linux 平台

**支持的驱动类型：**
- 内核模块（*.ko）
- 设备树覆盖
- 固件文件

**示例：**

```c#
var driverInfo = new DriverInfo
{
    Name = "网卡驱动",
    Version = "1.5.0",
    FilePath = "/lib/modules/drivers/netadapter.ko",
    TargetOS = "Linux",
    Architecture = "x64",
    Hash = "sha256_hash_here"
};

var strategy = new UpdateStrategy
{
    RequireBackup = true,
    RestartMode = RestartMode.Delayed  // Linux 系统延迟重启
};

var updater = GeneralDrivelution.Create();
var result = await updater.UpdateAsync(driverInfo, strategy);
```

**Linux 平台特性：**
- GPG 签名验证
- 内核模块依赖检查
- 与 modprobe 和 insmod 集成
- 支持 DKMS（动态内核模块支持）

**所需权限：**
```bash
# 使用 sudo 或 root 权限运行
sudo dotnet run MyApp.dll
```

#### MacOS 平台

**支持的驱动类型：**
- 内核扩展（*.kext）
- DriverKit 扩展

**示例：**

```c#
var driverInfo = new DriverInfo
{
    Name = "音频设备驱动",
    Version = "2.3.0",
    FilePath = "/Library/Extensions/AudioDevice.kext",
    TargetOS = "MacOS",
    Architecture = "ARM64",
    Hash = "sha256_hash_here"
};

var strategy = new UpdateStrategy
{
    RequireBackup = true,
    RestartMode = RestartMode.Prompt
};

var updater = GeneralDrivelution.Create();
var result = await updater.UpdateAsync(driverInfo, strategy);
```

**MacOS 平台特性：**
- 使用 codesign 进行代码签名验证
- 系统完整性保护（SIP）感知
- 支持公证驱动
- 与 kextutil 集成

**所需权限：**
```bash
# 使用管理员权限运行
sudo dotnet run MyApp.dll
```

#### 平台检测

自动检测当前平台：

```c#
var platformInfo = GeneralDrivelution.GetPlatformInfo();

Console.WriteLine($"平台: {platformInfo.Platform}");
Console.WriteLine($"操作系统: {platformInfo.OperatingSystem}");
Console.WriteLine($"架构: {platformInfo.Architecture}");
Console.WriteLine($"是否支持: {platformInfo.IsSupported}");
```

#### 跨平台代码示例

编写一次，到处运行：

```c#
// 此代码在 Windows、Linux 和 MacOS 上均可运行
var updater = GeneralDrivelution.Create();

// 平台特定的驱动路径可以在运行时确定
var driverPath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) 
    ? "C:\\Drivers\\device.inf"
    : RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
        ? "/lib/modules/device.ko"
        : "/Library/Extensions/device.kext";

var driverInfo = new DriverInfo
{
    Name = "设备驱动",
    Version = "1.0.0",
    FilePath = driverPath,
    Hash = "calculated_hash"
};

var result = await updater.UpdateAsync(driverInfo, new UpdateStrategy());
```



### 常见问题（FAQ）

#### Q1: 支持哪些平台？

**A:** GeneralUpdate.Drivelution 支持 Windows（7/8/10/11/Server）、Linux（各种发行版）和 MacOS（10.13+）。组件会自动检测平台并使用适当的驱动更新策略。

#### Q2: 是否需要管理员/root 权限？

**A:** 是的，驱动更新在所有平台上都需要提升权限：
- **Windows**：以管理员身份运行
- **Linux**：使用 sudo 或以 root 身份运行
- **MacOS**：使用 sudo 或以 root 身份运行

#### Q3: 如何计算驱动文件哈希值？

**A:** 可以使用内置工具或以下代码：

```c#
using System.Security.Cryptography;

string CalculateFileHash(string filePath)
{
    using var sha256 = SHA256.Create();
    using var stream = File.OpenRead(filePath);
    var hash = sha256.ComputeHash(stream);
    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
}
```

#### Q4: 如果更新失败会怎样？

**A:** 如果在更新策略中启用了 `RequireBackup`，系统将自动回滚到之前的驱动版本。`UpdateResult` 对象将包含详细的错误信息。

#### Q5: 可以同时更新多个驱动吗？

**A:** 可以，您可以顺序或并行更新多个驱动：

```c#
var drivers = new List<DriverInfo> { driver1, driver2, driver3 };
var tasks = drivers.Select(d => updater.UpdateAsync(d, strategy));
var results = await Task.WhenAll(tasks);
```

#### Q6: 如何处理驱动签名验证？

**A:** 默认情况下启用签名验证。要跳过验证（生产环境不推荐）：

```c#
var strategy = new UpdateStrategy
{
    SkipSignatureValidation = true  // 仅用于测试！
};
```

对于生产环境，请确保驱动已正确签名：
- **Windows**：使用 Microsoft Authenticode
- **Linux**：使用 GPG 签名
- **MacOS**：使用 Apple 代码签名

#### Q7: 更新驱动后系统是否需要重启？

**A:** 这取决于驱动类型和平台。您可以通过 `RestartMode` 控制重启行为：
- `RestartMode.None`：不需要重启
- `RestartMode.Prompt`：提示用户重启
- `RestartMode.Delayed`：稍后安排重启
- `RestartMode.Immediate`：立即重启

#### Q8: 如何在更新前验证驱动？

**A:** 使用 `ValidateAsync` 方法：

```c#
var isValid = await updater.ValidateAsync(driverInfo);
if (isValid)
{
    // 继续更新
    var result = await updater.UpdateAsync(driverInfo, strategy);
}
```

#### Q9: 可以自定义日志吗？

**A:** 可以，您可以提供自定义的 Serilog 日志器：

```c#
var logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/driver-update.log")
    .CreateLogger();

var updater = GeneralDrivelution.Create(logger, options);
```

#### Q10: 驱动备份存储在哪里？

**A:** 备份存储在以下路径（按优先级）：
1. `UpdateStrategy.BackupPath`（如果指定）
2. `DrivelutionOptions.DefaultBackupPath`（如果配置）
3. 默认：`./DriverBackups` 目录

如果启用了 `AutoCleanupBackups`，旧备份会自动清理。

#### Q11: 如何从目录获取驱动信息？

**A:** 使用 `GetDriversFromDirectoryAsync` 方法：

```c#
// 从目录获取所有驱动
var drivers = await updater.GetDriversFromDirectoryAsync("C:\\Drivers");

// 获取特定模式的驱动
var infDrivers = await updater.GetDriversFromDirectoryAsync(
    "C:\\Drivers", 
    "*.inf"
);
```

#### Q12: 组件是否线程安全？

**A:** 是的，您可以安全地在多个线程之间使用更新器实例。每个更新操作都是独立的，可以并发运行。

#### Q13: 支持哪些 .NET 版本？

**A:** GeneralUpdate.Drivelution 目标是 .NET 8.0，兼容：
- .NET 8.0+
- 支持 AOT（提前编译）
- 支持裁剪以实现更小的部署包



### 示例

包含错误处理和日志记录的完整示例：

```c#
using GeneralUpdate.Drivelution;
using GeneralUpdate.Drivelution.Abstractions.Models;
using GeneralUpdate.Drivelution.Abstractions.Configuration;
using Serilog;

public class DriverUpdateExample
{
    public static async Task Main(string[] args)
    {
        // 配置日志
        var logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File("logs/driver-update.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        // 配置选项
        var options = new DrivelutionOptions
        {
            LogLevel = "Info",
            DefaultBackupPath = "C:\\DriverBackups",
            AutoCleanupBackups = true,
            BackupsToKeep = 5
        };

        try
        {
            // 创建更新器
            var updater = GeneralDrivelution.Create(logger, options);

            // 获取平台信息
            var platformInfo = GeneralDrivelution.GetPlatformInfo();
            Console.WriteLine($"平台: {platformInfo}");

            // 定义驱动信息
            var driverInfo = new DriverInfo
            {
                Name = "示例设备驱动",
                Version = "2.0.0",
                FilePath = "C:\\Drivers\\sample.inf",
                TargetOS = "Windows",
                Architecture = "x64",
                Hash = CalculateFileHash("C:\\Drivers\\sample.inf"),
                HashAlgorithm = "SHA256",
                HardwareId = "PCI\\VEN_1234&DEV_5678",
                Description = "用于演示的示例设备驱动"
            };

            // 先验证驱动
            Console.WriteLine("验证驱动中...");
            var isValid = await updater.ValidateAsync(driverInfo);
            if (!isValid)
            {
                Console.WriteLine("驱动验证失败！");
                return;
            }

            // 定义更新策略
            var strategy = new UpdateStrategy
            {
                RequireBackup = true,
                RetryCount = 3,
                RetryIntervalSeconds = 5,
                RestartMode = RestartMode.Prompt,
                TimeoutSeconds = 600
            };

            // 执行更新
            Console.WriteLine("开始驱动更新...");
            var result = await updater.UpdateAsync(driverInfo, strategy);

            // 处理结果
            if (result.Success)
            {
                Console.WriteLine($"✓ 驱动更新成功，耗时 {result.DurationMs} 毫秒");
                Console.WriteLine($"✓ 备份: {result.BackupPath}");
                
                foreach (var log in result.StepLogs)
                {
                    Console.WriteLine($"  - {log}");
                }
            }
            else
            {
                Console.WriteLine($"✗ 更新失败: {result.Error?.Message}");
                Console.WriteLine($"  错误代码: {result.Error?.Code}");
                
                if (result.RolledBack)
                {
                    Console.WriteLine("✓ 系统回滚成功");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"意外错误: {ex.Message}");
            logger.Error(ex, "驱动更新失败");
        }
    }

    private static string CalculateFileHash(string filePath)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        using var stream = File.OpenRead(filePath);
        var hash = sha256.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
```



### 注解

GeneralDrivelution 提供完整的驱动更新解决方案，具有以下关键特性：

#### 安全特性
- ✓ 驱动签名验证（平台特定）
- ✓ 文件哈希完整性检查
- ✓ 硬件 ID 匹配
- ✓ 发布者信任验证
- ✓ 操作前权限检查

#### 可靠性特性
- ✓ 更新前自动备份
- ✓ 失败时回滚
- ✓ 可配置尝试次数的重试机制
- ✓ 超时保护
- ✓ 全面的错误处理和日志记录

#### 平台特性
- ✓ Windows：INF 驱动安装、PnPUtil 集成
- ✓ Linux：内核模块管理、modprobe 集成
- ✓ MacOS：KEXT 安装、系统完整性保护感知

#### 开发者特性
- ✓ 简单直观的 API
- ✓ Async/await 支持
- ✓ 使用 Serilog 的全面日志记录
- ✓ 详细的错误信息
- ✓ 进度跟踪和步骤日志



### 适用于

| 产品           | 版本          |
| -------------- | ------------- |
| .NET           | 8、9          |
| .NET Standard  | 不适用        |
| .NET Core      | 不适用        |
| .NET Framework | 不适用        |

**平台支持：**

| 平台           | 支持级别      |
| -------------- | ------------- |
| Windows        | ✓ 完全支持    |
| Linux          | ✓ 完全支持    |
| MacOS          | ✓ 完全支持    |

**注意：** 在所有平台上进行驱动操作都需要提升权限（管理员/root）。
