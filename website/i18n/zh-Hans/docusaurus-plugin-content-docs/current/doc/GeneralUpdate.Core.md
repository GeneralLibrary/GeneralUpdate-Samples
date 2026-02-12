---
sidebar_position: 5
---

# GeneralUpdate.Core

## 组件概览 | Component Overview

**GeneralUpdate.Core** 是 GeneralUpdate 框架最核心的组件之一，提供了完整的升级执行能力。与 ClientCore 不同，Core 组件作为独立的升级助手程序运行，负责在主程序关闭后执行实际的文件替换、版本升级和系统更新操作。通过进程启动和参数传递的方式，Core 接收来自 ClientCore 的更新指令，并安全地完成主程序的升级任务。

**GeneralUpdate.Core** is one of the most essential components of the GeneralUpdate framework, providing complete upgrade execution capabilities. Unlike ClientCore, the Core component runs as an independent upgrade assistant program and is responsible for performing actual file replacement, version upgrades, and system update operations after the main program closes. Through process startup and parameter passing, Core receives update instructions from ClientCore and safely completes the main program's upgrade tasks.

**命名空间 | Namespace:** `GeneralUpdate.Core`  
**程序集 | Assembly:** `GeneralUpdate.Core.dll`

```csharp
public class GeneralUpdateBootstrap : AbstractBootstrap<GeneralUpdateBootstrap, IStrategy>
```

---

## 核心特性 | Core Features

### 1. 文件替换与版本管理 | File Replacement and Version Management
- 安全的文件替换机制，避免文件占用问题 | Safe file replacement mechanism to avoid file locking issues
- 支持多版本增量升级 | Support multi-version incremental upgrades
- 自动处理文件依赖关系 | Automatic handling of file dependencies

### 2. 驱动升级支持 | Driver Upgrade Support
- 可选的驱动程序升级功能 | Optional driver upgrade functionality
- 字段映射表配置 | Field mapping table configuration
- 安全的驱动安装流程 | Safe driver installation process

### 3. 完整的事件通知 | Comprehensive Event Notifications
- 下载进度实时监控 | Real-time download progress monitoring
- 多版本下载管理 | Multi-version download management
- 异常和错误完整捕获 | Complete exception and error capture

### 4. 跨平台支持 | Cross-Platform Support
- Windows、Linux、macOS 平台全支持 | Full support for Windows, Linux, macOS platforms
- 自动平台检测和策略适配 | Automatic platform detection and strategy adaptation

![Multi Download](imgs/muti_donwload.png)

---

## 快速开始 | Quick Start

### 安装 | Installation

通过 NuGet 安装 GeneralUpdate.Core：

Install GeneralUpdate.Core via NuGet:

```bash
dotnet add package GeneralUpdate.Core
```

### 初始化与使用 | Initialization and Usage

以下示例展示了如何在升级助手程序中配置和启动升级流程：

The following example demonstrates how to configure and launch the upgrade process in the upgrade assistant program:

```csharp
using GeneralUpdate.Common.Download;
using GeneralUpdate.Common.Internal;
using GeneralUpdate.Common.Shared.Object;
using GeneralUpdate.Core;

try
{
    Console.WriteLine($"升级程序初始化，{DateTime.Now}！");
    Console.WriteLine("当前运行目录：" + Thread.GetDomain().BaseDirectory);
    
    // 启动升级流程 | Launch upgrade process
    await new GeneralUpdateBootstrap()
        // 监听下载统计信息 | Listen for download statistics
        .AddListenerMultiDownloadStatistics(OnMultiDownloadStatistics)
        // 监听单个下载完成 | Listen for single download completion
        .AddListenerMultiDownloadCompleted(OnMultiDownloadCompleted)
        // 监听所有下载完成 | Listen for all downloads completion
        .AddListenerMultiAllDownloadCompleted(OnMultiAllDownloadCompleted)
        // 监听下载错误 | Listen for download errors
        .AddListenerMultiDownloadError(OnMultiDownloadError)
        // 监听异常 | Listen for exceptions
        .AddListenerException(OnException)
        // 启动异步升级 | Launch async upgrade
        .LaunchAsync();
        
    Console.WriteLine($"升级程序已启动，{DateTime.Now}！");
    await Task.Delay(2000);
}
catch (Exception e)
{
    Console.WriteLine(e.Message + "\n" + e.StackTrace);
}

// 事件处理方法 | Event handler methods
void OnMultiDownloadStatistics(object arg1, MultiDownloadStatisticsEventArgs arg2)
{
    var version = arg2.Version as VersionInfo;
    Console.WriteLine($"当前下载版本：{version.Version}，下载速度：{arg2.Speed}，" +
                     $"剩余时间：{arg2.Remaining}，进度：{arg2.ProgressPercentage}%");
}

void OnMultiDownloadCompleted(object arg1, MultiDownloadCompletedEventArgs arg2)
{
    var version = arg2.Version as VersionInfo;
    Console.WriteLine(arg2.IsComplated ? 
        $"版本 {version.Version} 下载完成！" : 
        $"版本 {version.Version} 下载失败！");
}

void OnMultiAllDownloadCompleted(object arg1, MultiAllDownloadCompletedEventArgs arg2)
{
    Console.WriteLine(arg2.IsAllDownloadCompleted ? 
        "所有下载任务已完成！" : 
        $"下载任务失败！失败数量：{arg2.FailedVersions.Count}");
}

void OnMultiDownloadError(object arg1, MultiDownloadErrorEventArgs arg2)
{
    var version = arg2.Version as VersionInfo;
    Console.WriteLine($"版本 {version.Version} 下载错误：{arg2.Exception}");
}

void OnException(object arg1, ExceptionEventArgs arg2)
{
    Console.WriteLine($"升级异常：{arg2.Exception}");
}
```

---

## 核心 API 参考 | Core API Reference

### GeneralUpdateBootstrap 类方法 | GeneralUpdateBootstrap Class Methods

#### LaunchAsync 方法 | LaunchAsync Method

异步启动升级流程。

Launch the upgrade process asynchronously.

```csharp
public async Task<GeneralUpdateBootstrap> LaunchAsync()
```

**返回值 | Return Value:**
- 返回当前 GeneralUpdateBootstrap 实例，支持链式调用
- Returns the current GeneralUpdateBootstrap instance, supporting method chaining

#### Option 方法 | Option Method

设置升级选项。

Set upgrade options.

```csharp
public GeneralUpdateBootstrap Option(UpdateOption option, object value)
```

**参数 | Parameters:**
- `option`: 升级选项枚举 | Upgrade option enum
- `value`: 选项值 | Option value

**示例 | Example:**
```csharp
.Option(UpdateOption.Drive, true)  // 启用驱动升级 | Enable driver upgrade
```

#### AddListenerMultiDownloadStatistics 方法 | AddListenerMultiDownloadStatistics Method

监听下载统计信息（速度、进度、剩余时间等）。

Listen for download statistics (speed, progress, remaining time, etc.).

```csharp
public GeneralUpdateBootstrap AddListenerMultiDownloadStatistics(
    Action<object, MultiDownloadStatisticsEventArgs> callbackAction)
```

#### AddListenerMultiDownloadCompleted 方法 | AddListenerMultiDownloadCompleted Method

监听单个更新包下载完成事件。

Listen for single update package download completion event.

```csharp
public GeneralUpdateBootstrap AddListenerMultiDownloadCompleted(
    Action<object, MultiDownloadCompletedEventArgs> callbackAction)
```

#### AddListenerMultiAllDownloadCompleted 方法 | AddListenerMultiAllDownloadCompleted Method

监听所有版本下载完成事件。

Listen for all version downloads completion event.

```csharp
public GeneralUpdateBootstrap AddListenerMultiAllDownloadCompleted(
    Action<object, MultiAllDownloadCompletedEventArgs> callbackAction)
```

#### AddListenerMultiDownloadError 方法 | AddListenerMultiDownloadError Method

监听每个版本下载错误事件。

Listen for download error events for each version.

```csharp
public GeneralUpdateBootstrap AddListenerMultiDownloadError(
    Action<object, MultiDownloadErrorEventArgs> callbackAction)
```

#### AddListenerException 方法 | AddListenerException Method

监听升级组件内部所有异常。

Listen for all internal exceptions in the upgrade component.

```csharp
public GeneralUpdateBootstrap AddListenerException(
    Action<object, ExceptionEventArgs> callbackAction)
```

#### SetFieldMappings 方法 | SetFieldMappings Method

设置字段映射表，用于解析驱动包信息。

Set field mapping table for parsing driver package information.

```csharp
public GeneralUpdateBootstrap SetFieldMappings(Dictionary<string, string> fieldMappings)
```

**参数 | Parameters:**
- `fieldMappings`: 字段映射字典，键为英文字段名，值为本地化字段名
- Field mapping dictionary, key is English field name, value is localized field name

---

## 配置类详解 | Configuration Class Details

### UpdateOption 枚举 | UpdateOption Enum

```csharp
public enum UpdateOption
{
    /// <summary>
    /// 是否启用驱动升级功能
    /// Whether to enable driver upgrade functionality
    /// </summary>
    Drive
}
```

### Packet 类 | Packet Class

升级包信息类，由 ClientCore 通过参数传递给 Core：

Upgrade package information class, passed from ClientCore to Core via parameters:

```csharp
public class Packet
{
    /// <summary>
    /// 主更新检查 API 地址
    /// Main update check API address
    /// </summary>
    public string MainUpdateUrl { get; set; }
    
    /// <summary>
    /// 应用类型：1=客户端应用，2=更新应用
    /// Application type: 1=ClientApp, 2=UpdateApp
    /// </summary>
    public int AppType { get; set; }
    
    /// <summary>
    /// 更新检查 API 地址
    /// Update check API address
    /// </summary>
    public string UpdateUrl { get; set; }
    
    /// <summary>
    /// 需要启动的应用程序名称
    /// Name of the application to be launched
    /// </summary>
    public string AppName { get; set; }
    
    /// <summary>
    /// 主应用程序名称
    /// Main application name
    /// </summary>
    public string MainAppName { get; set; }
    
    /// <summary>
    /// 更新包文件格式（默认为 Zip）
    /// Update package file format (default is Zip)
    /// </summary>
    public string Format { get; set; }
    
    /// <summary>
    /// 是否需要升级更新应用
    /// Indicates if the update application needs to be upgraded
    /// </summary>
    public bool IsUpgradeUpdate { get; set; }
    
    /// <summary>
    /// 是否需要更新主应用
    /// Indicates if the main application needs to be updated
    /// </summary>
    public bool IsMainUpdate { get; set; }
    
    /// <summary>
    /// 更新日志网页 URL
    /// Update log webpage URL
    /// </summary>
    public string UpdateLogUrl { get; set; }
    
    /// <summary>
    /// 需要更新的版本信息列表
    /// List of version information that needs updating
    /// </summary>
    public List<VersionInfo> UpdateVersions { get; set; }
    
    /// <summary>
    /// 文件操作编码格式
    /// File operation encoding format
    /// </summary>
    public Encoding Encoding { get; set; }
    
    /// <summary>
    /// 下载超时时间（秒）
    /// Download timeout duration (seconds)
    /// </summary>
    public int DownloadTimeOut { get; set; }
    
    /// <summary>
    /// 应用密钥，与服务器约定
    /// Application secret key, agreed upon with the server
    /// </summary>
    public string AppSecretKey { get; set; }
    
    /// <summary>
    /// 当前客户端版本
    /// Current client version
    /// </summary>
    public string ClientVersion { get; set; }
    
    /// <summary>
    /// 最新版本
    /// Latest version
    /// </summary>
    public string LastVersion { get; set; }
    
    /// <summary>
    /// 安装路径（用于更新文件逻辑）
    /// Installation path (used for update file logic)
    /// </summary>
    public string InstallPath { get; set; }
    
    /// <summary>
    /// 下载文件的临时存储路径
    /// Temporary storage path for downloaded files
    /// </summary>
    public string TempPath { get; set; }
    
    /// <summary>
    /// 升级终端程序的配置参数（Base64 编码）
    /// Configuration parameters for the upgrade terminal program (Base64 encoded)
    /// </summary>
    public string ProcessBase64 { get; set; }
    
    /// <summary>
    /// 当前策略所属平台（Windows/Linux/Mac）
    /// Platform to which the current strategy belongs (Windows/Linux/Mac)
    /// </summary>
    public string Platform { get; set; }
    
    /// <summary>
    /// 黑名单文件列表
    /// Files in the blacklist
    /// </summary>
    public List<string> BlackFiles { get; set; }
    
    /// <summary>
    /// 黑名单文件格式列表
    /// File formats in the blacklist
    /// </summary>
    public List<string> BlackFormats { get; set; }
    
    /// <summary>
    /// 是否启用驱动升级功能
    /// Indicates if the driver upgrade feature is enabled
    /// </summary>
    public bool DriveEnabled { get; set; }
}
```

---

## 实际使用示例 | Practical Usage Examples

### 示例 1：基本升级流程 | Example 1: Basic Upgrade Process

```csharp
using GeneralUpdate.Common.Download;
using GeneralUpdate.Common.Internal;
using GeneralUpdate.Common.Shared.Object;
using GeneralUpdate.Core;

try
{
    Console.WriteLine("升级程序初始化...");
    
    // 启动升级流程 | Launch upgrade process
    await new GeneralUpdateBootstrap()
        .AddListenerMultiDownloadStatistics((sender, args) =>
        {
            var version = args.Version as VersionInfo;
            Console.WriteLine($"[{version.Version}] 下载进度: {args.ProgressPercentage}%");
        })
        .AddListenerException((sender, args) =>
        {
            Console.WriteLine($"升级异常: {args.Exception.Message}");
        })
        .LaunchAsync();
        
    Console.WriteLine("升级完成！");
}
catch (Exception e)
{
    Console.WriteLine($"升级失败: {e.Message}");
}
```

### 示例 2：启用驱动升级 | Example 2: Enable Driver Upgrade

```csharp
using GeneralUpdate.Core;
using GeneralUpdate.Common.Internal.Bootstrap;

// 中文字段映射表 | Chinese field mapping table
var fieldMappingsCN = new Dictionary<string, string>
{
    { "DriverName", "驱动名称" },
    { "DriverVersion", "驱动版本" },
    { "DriverDescription", "驱动描述" },
    { "InstallPath", "安装路径" }
};

await new GeneralUpdateBootstrap()
    // 设置字段映射表 | Set field mappings
    .SetFieldMappings(fieldMappingsCN)
    // 启用驱动更新 | Enable driver update
    .Option(UpdateOption.Drive, true)
    .AddListenerException((sender, args) =>
    {
        Console.WriteLine($"升级异常: {args.Exception.Message}");
    })
    .LaunchAsync();
```

### 示例 3：完整事件监听 | Example 3: Complete Event Listening

```csharp
using GeneralUpdate.Core;
using GeneralUpdate.Common.Download;
using GeneralUpdate.Common.Shared.Object;

await new GeneralUpdateBootstrap()
    // 下载统计 | Download statistics
    .AddListenerMultiDownloadStatistics((sender, args) =>
    {
        var version = args.Version as VersionInfo;
        Console.WriteLine($"[{version.Version}]");
        Console.WriteLine($"  速度: {args.Speed}");
        Console.WriteLine($"  进度: {args.ProgressPercentage}%");
        Console.WriteLine($"  已下载: {args.BytesReceived} / {args.TotalBytesToReceive}");
        Console.WriteLine($"  剩余时间: {args.Remaining}");
    })
    // 单个下载完成 | Single download completed
    .AddListenerMultiDownloadCompleted((sender, args) =>
    {
        var version = args.Version as VersionInfo;
        string status = args.IsComplated ? "✓ 成功" : "✗ 失败";
        Console.WriteLine($"版本 {version.Version} 下载{status}");
    })
    // 所有下载完成 | All downloads completed
    .AddListenerMultiAllDownloadCompleted((sender, args) =>
    {
        if (args.IsAllDownloadCompleted)
        {
            Console.WriteLine("✓ 所有版本下载完成，开始安装...");
        }
        else
        {
            Console.WriteLine($"✗ 下载失败，{args.FailedVersions.Count} 个版本失败：");
            foreach (var version in args.FailedVersions)
            {
                Console.WriteLine($"  - {version}");
            }
        }
    })
    // 下载错误 | Download error
    .AddListenerMultiDownloadError((sender, args) =>
    {
        var version = args.Version as VersionInfo;
        Console.WriteLine($"✗ 版本 {version.Version} 错误:");
        Console.WriteLine($"  {args.Exception.Message}");
    })
    // 异常处理 | Exception handling
    .AddListenerException((sender, args) =>
    {
        Console.WriteLine("⚠ 升级过程异常:");
        Console.WriteLine($"  错误: {args.Exception.Message}");
        Console.WriteLine($"  堆栈: {args.Exception.StackTrace}");
    })
    .LaunchAsync();
```

### 示例 4：自定义升级流程 | Example 4: Custom Upgrade Process

```csharp
using GeneralUpdate.Core;
using GeneralUpdate.Common.Download;
using GeneralUpdate.Common.Shared.Object;

// 记录升级开始时间 | Record upgrade start time
var startTime = DateTime.Now;
var downloadedVersions = new List<string>();

await new GeneralUpdateBootstrap()
    .AddListenerMultiDownloadCompleted((sender, args) =>
    {
        if (args.IsComplated)
        {
            var version = args.Version as VersionInfo;
            downloadedVersions.Add(version.Version);
        }
    })
    .AddListenerMultiAllDownloadCompleted((sender, args) =>
    {
        if (args.IsAllDownloadCompleted)
        {
            var duration = DateTime.Now - startTime;
            Console.WriteLine($"升级完成！");
            Console.WriteLine($"总耗时: {duration.TotalSeconds:F2} 秒");
            Console.WriteLine($"已更新版本: {string.Join(", ", downloadedVersions)}");
        }
    })
    .AddListenerException((sender, args) =>
    {
        // 记录日志到文件 | Log to file
        File.AppendAllText("upgrade_error.log", 
            $"[{DateTime.Now}] {args.Exception}\n");
    })
    .LaunchAsync();
```

---

## 注意事项与警告 | Notes and Warnings

### ⚠️ 重要提示 | Important Notes

1. **进程隔离 | Process Isolation**
   - Core 必须作为独立进程运行，不能在主程序中直接调用
   - 升级时主程序必须完全关闭，否则文件替换会失败
   - Core must run as an independent process, cannot be called directly in the main program
   - The main program must be completely closed during upgrade, otherwise file replacement will fail

2. **参数传递 | Parameter Passing**
   - ClientCore 通过 Base64 编码的参数传递配置给 Core
   - 确保参数传递过程中不会被截断或损坏
   - ClientCore passes configuration to Core via Base64 encoded parameters
   - Ensure parameters are not truncated or corrupted during passing

3. **文件权限 | File Permissions**
   - 在 Windows 上可能需要管理员权限替换系统目录中的文件
   - 在 Linux/macOS 上需要适当的文件系统权限
   - Administrator privileges may be required on Windows to replace files in system directories
   - Appropriate file system permissions are required on Linux/macOS

4. **驱动升级 | Driver Upgrade**
   - 驱动升级功能需要系统级权限
   - 建议在测试环境充分验证后再使用
   - Driver upgrade functionality requires system-level permissions
   - Recommended to thoroughly validate in test environment before use

5. **回滚机制 | Rollback Mechanism**
   - Core 不直接提供回滚功能，但保留了备份文件
   - 如需回滚，可使用 ClientCore 的备份功能
   - Core does not directly provide rollback functionality, but backup files are preserved
   - For rollback, use ClientCore's backup functionality

### 💡 最佳实践 | Best Practices

- **日志记录**：实现完整的异常监听，记录升级过程中的所有问题
- **超时设置**：根据网络环境合理设置下载超时时间
- **进度反馈**：向用户显示升级进度，提升用户体验
- **错误处理**：升级失败时提供清晰的错误信息和解决方案
- **测试验证**：在各种网络条件下测试升级流程的稳定性
- **Logging**: Implement complete exception listening to record all issues during the upgrade process
- **Timeout Settings**: Set download timeout appropriately based on network environment
- **Progress Feedback**: Display upgrade progress to users to improve user experience
- **Error Handling**: Provide clear error messages and solutions when upgrade fails
- **Testing**: Test upgrade process stability under various network conditions

---

## 适用平台 | Applicable Platforms

| 产品 Product        | 版本 Version       |
| ------------------ | ----------------- |
| .NET               | 5, 6, 7, 8, 9     |
| .NET Framework     | 4.6.1             |
| .NET Standard      | 2.0               |
| .NET Core          | 2.0               |

---

## 相关资源 | Related Resources

- **示例代码**：[查看 GitHub 示例](https://github.com/GeneralLibrary/GeneralUpdate-Samples/blob/main/src/Upgrade/Program.cs) | [View GitHub Examples](https://github.com/GeneralLibrary/GeneralUpdate-Samples/blob/main/src/Upgrade/Program.cs)
- **主仓库**：[GeneralUpdate 项目](https://github.com/GeneralLibrary/GeneralUpdate) | [GeneralUpdate Project](https://github.com/GeneralLibrary/GeneralUpdate)
- **相关组件**：[GeneralUpdate.ClientCore](./GeneralUpdate.ClientCore.md) | [GeneralUpdate.Bowl](./GeneralUpdate.Bowl.md)
