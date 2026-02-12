---
sidebar_position: 4
---

# GeneralUpdate.ClientCore

## 组件概览 | Component Overview

**GeneralUpdate.ClientCore** 是 GeneralUpdate 框架的核心组件之一，提供了丰富的客户端更新功能。该组件运行在主应用程序中，负责检查更新、下载更新包、验证完整性，并在完成后启动升级助手（GeneralUpdate.Core）来执行实际的文件替换操作。ClientCore 的设计理念是让主程序能够安全地检查和准备更新，而不影响当前运行状态。

**GeneralUpdate.ClientCore** is one of the core components of the GeneralUpdate framework, providing rich client-side update functionalities. This component runs in the main application and is responsible for checking updates, downloading update packages, validating integrity, and then launching the upgrade assistant (GeneralUpdate.Core) to perform actual file replacement operations upon completion. The design philosophy of ClientCore is to enable the main program to safely check and prepare updates without affecting the current running state.

**命名空间 | Namespace:** `GeneralUpdate.ClientCore`  
**程序集 | Assembly:** `GeneralUpdate.ClientCore.dll`

```csharp
public class GeneralClientBootstrap : AbstractBootstrap<GeneralClientBootstrap, IStrategy>
```

---

## 核心特性 | Core Features

### 1. 多版本下载管理 | Multi-Version Download Management
- 支持同时下载多个版本的更新包 | Support downloading multiple version update packages simultaneously
- 断点续传和下载速度限制 | Resume download and download speed limiting
- 实时下载进度和统计信息 | Real-time download progress and statistics

### 2. 灵活的配置选项 | Flexible Configuration Options
- 黑名单机制（文件、格式、目录）| Blacklist mechanism (files, formats, directories)
- 自定义更新策略和操作 | Custom update strategies and operations
- 支持二进制差异更新和全量更新 | Support binary differential updates and full updates

### 3. 完整的事件通知 | Comprehensive Event Notifications
- 下载进度、完成、错误事件 | Download progress, completion, and error events
- 支持用户自定义跳过更新选项 | Support user-defined skip update options
- 异常和错误全程监控 | Exception and error monitoring throughout the process

### 4. 多平台支持 | Multi-Platform Support
- Windows、Linux、macOS 平台支持 | Windows, Linux, macOS platform support
- 自动平台检测和策略选择 | Automatic platform detection and strategy selection

![Multi Download](imgs/muti_donwload.png)

---

## 快速开始 | Quick Start

### 安装 | Installation

通过 NuGet 安装 GeneralUpdate.ClientCore：

Install GeneralUpdate.ClientCore via NuGet:

```bash
dotnet add package GeneralUpdate.ClientCore
```

### 初始化与使用 | Initialization and Usage

以下示例展示了如何在主程序中配置和启动更新检查：

The following example demonstrates how to configure and launch update checking in the main program:

```csharp
using System.Text;
using GeneralUpdate.ClientCore;
using GeneralUpdate.Common.Download;
using GeneralUpdate.Common.Internal;
using GeneralUpdate.Common.Internal.Bootstrap;
using GeneralUpdate.Common.Shared.Object;

try
{
    Console.WriteLine($"主程序初始化，{DateTime.Now}！");
    
    // 配置更新参数 | Configure update parameters
    var configinfo = new Configinfo
    {
        // 更新验证 API 地址 | Update verification API address
        UpdateUrl = "http://127.0.0.1:5000/Upgrade/Verification",
        // 更新报告 API 地址 | Update report API address
        ReportUrl = "http://127.0.0.1:5000/Upgrade/Report",
        // 主应用程序名称 | Main application name
        MainAppName = "ClientSample.exe",
        // 升级程序名称 | Upgrade program name
        AppName = "UpgradeSample.exe",
        // 当前客户端版本 | Current client version
        ClientVersion = "1.0.0.0",
        // 升级端版本 | Upgrade client version
        UpgradeClientVersion = "1.0.0.0",
        // 安装路径 | Installation path
        InstallPath = Thread.GetDomain().BaseDirectory,
        // 产品 ID（用于多产品分支管理）| Product ID (for multi-product branch management)
        ProductId = "2d974e2a-31e6-4887-9bb1-b4689e98c77a",
        // 应用密钥（用于服务器验证）| App secret key (for server verification)
        AppSecretKey = "dfeb5833-975e-4afb-88f1-6278ee9aeff6"
    };
    
    // 启动更新流程 | Launch update process
    await new GeneralClientBootstrap()
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
        // 设置配置 | Set configuration
        .SetConfig(configinfo)
        // 设置选项 | Set options
        .Option(UpdateOption.DownloadTimeOut, 60)
        .Option(UpdateOption.Encoding, Encoding.Default)
        // 启动异步更新 | Launch async update
        .LaunchAsync();
        
    Console.WriteLine($"主程序已启动，{DateTime.Now}！");
}
catch (Exception e)
{
    Console.WriteLine(e.Message + "\n" + e.StackTrace);
}

// 事件处理方法 | Event handler methods
void OnMultiDownloadStatistics(object arg1, MultiDownloadStatisticsEventArgs arg2)
{
    var version = arg2.Version as VersionInfo;
    Console.WriteLine($"下载版本：{version.Version}，速度：{arg2.Speed}，" +
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
    Console.WriteLine($"更新异常：{arg2.Exception}");
}
```

---

## 核心 API 参考 | Core API Reference

### GeneralClientBootstrap 类方法 | GeneralClientBootstrap Class Methods

#### LaunchAsync 方法 | LaunchAsync Method

异步启动更新流程。

Launch the update process asynchronously.

```csharp
public async Task<GeneralClientBootstrap> LaunchAsync()
```

#### SetConfig 方法 | SetConfig Method

设置更新配置信息。

Set update configuration information.

```csharp
public GeneralClientBootstrap SetConfig(Configinfo configinfo)
```

#### Option 方法 | Option Method

设置更新选项。

Set update options.

```csharp
public GeneralClientBootstrap Option(UpdateOption option, object value)
```

#### SetBlacklist 方法 | SetBlacklist Method

设置更新黑名单，指定不需要更新的文件。

Set update blacklist to specify files that should not be updated.

```csharp
public GeneralClientBootstrap SetBlacklist(List<string> blackFiles = null, 
                                           List<string> blackFormats = null)
```

#### AddListenerMultiDownloadStatistics 方法 | AddListenerMultiDownloadStatistics Method

监听下载统计信息（速度、进度、剩余时间等）。

Listen for download statistics (speed, progress, remaining time, etc.).

```csharp
public GeneralClientBootstrap AddListenerMultiDownloadStatistics(
    Action<object, MultiDownloadStatisticsEventArgs> callbackAction)
```

#### AddListenerMultiDownloadCompleted 方法 | AddListenerMultiDownloadCompleted Method

监听单个更新包下载完成事件。

Listen for single update package download completion event.

```csharp
public GeneralClientBootstrap AddListenerMultiDownloadCompleted(
    Action<object, MultiDownloadCompletedEventArgs> callbackAction)
```

#### AddListenerMultiAllDownloadCompleted 方法 | AddListenerMultiAllDownloadCompleted Method

监听所有下载任务完成事件。

Listen for all download tasks completion event.

```csharp
public GeneralClientBootstrap AddListenerMultiAllDownloadCompleted(
    Action<object, MultiAllDownloadCompletedEventArgs> callbackAction)
```

#### AddListenerMultiDownloadError 方法 | AddListenerMultiDownloadError Method

监听下载错误事件。

Listen for download error events.

```csharp
public GeneralClientBootstrap AddListenerMultiDownloadError(
    Action<object, MultiDownloadErrorEventArgs> callbackAction)
```

#### AddListenerException 方法 | AddListenerException Method

监听更新过程中的所有异常。

Listen for all exceptions during the update process.

```csharp
public GeneralClientBootstrap AddListenerException(
    Action<object, ExceptionEventArgs> callbackAction)
```

#### AddCustomOption 方法 | AddCustomOption Method

添加自定义异步操作，可在更新前后执行自定义逻辑。

Add custom asynchronous operations that can execute custom logic before or after updates.

```csharp
public GeneralClientBootstrap AddCustomOption(Func<Task> customFunc)
```

#### SetCustomSkipOption 方法 | SetCustomSkipOption Method

设置自定义跳过选项，允许用户决定是否继续更新。

Set custom skip options, allowing users to decide whether to continue with the update.

```csharp
public GeneralClientBootstrap SetCustomSkipOption(Func<bool> customSkipFunc)
```

---

## 配置类详解 | Configuration Class Details

### Configinfo 类 | Configinfo Class

```csharp
public class Configinfo
{
    /// <summary>
    /// 更新检查 API 地址
    /// Update check API address
    /// </summary>
    public string UpdateUrl { get; set; }
    
    /// <summary>
    /// 更新状态报告 API 地址
    /// Update status report API address
    /// </summary>
    public string ReportUrl { get; set; }
    
    /// <summary>
    /// 需要启动的应用程序名称（升级程序）
    /// Application name to be launched (upgrade program)
    /// </summary>
    public string AppName { get; set; }
    
    /// <summary>
    /// 需要启动的主应用程序名称
    /// Main application name to be launched
    /// </summary>
    public string MainAppName { get; set; }
    
    /// <summary>
    /// 更新日志网页地址
    /// Update log webpage address
    /// </summary>
    public string UpdateLogUrl { get; set; }
    
    /// <summary>
    /// 应用密钥，与服务器约定用于身份验证和产品分支
    /// Application secret key, agreed upon with server for authentication and product branching
    /// </summary>
    public string AppSecretKey { get; set; }
    
    /// <summary>
    /// 当前客户端版本号
    /// Current client version number
    /// </summary>
    public string ClientVersion { get; set; }
    
    /// <summary>
    /// 当前升级客户端版本号
    /// Current upgrade client version number
    /// </summary>
    public string UpgradeClientVersion { get; set; }
    
    /// <summary>
    /// 安装路径（用于更新文件逻辑）
    /// Installation path (used for update file logic)
    /// </summary>
    public string InstallPath { get; set; }
    
    /// <summary>
    /// 黑名单文件列表，这些文件在更新时会被跳过
    /// Blacklist file list, these files will be skipped during updates
    /// </summary>
    public List<string> BlackFiles { get; set; }
    
    /// <summary>
    /// 黑名单文件格式列表，这些格式的文件在更新时会被跳过
    /// Blacklist file format list, files with these formats will be skipped during updates
    /// </summary>
    public List<string> BlackFormats { get; set; }
    
    /// <summary>
    /// 需要跳过的目录路径列表，这些目录不需要更新
    /// Directory path list to be skipped that do not require updates
    /// </summary>
    public List<string> SkipDirectorys { get; set; }
    
    /// <summary>
    /// 当前产品分支的唯一 ID
    /// Unique ID of the current product branch
    /// </summary>
    public string ProductId { get; set; }
    
    /// <summary>
    /// Bowl 监控进程路径，更新完成后启动 Bowl 检查客户端是否正常启动
    /// Bowl monitoring process path, starts after updates to check if the client starts normally
    /// </summary>
    public string Bowl { get; set; }
    
    /// <summary>
    /// HTTP 请求中用于传递 token 的 Scheme（如 Bearer）
    /// Scheme used for passing token in HTTP requests (e.g., Bearer)
    /// </summary>
    public string Scheme { get; set; }
    
    /// <summary>
    /// HTTP 请求中用于身份验证的 Token
    /// Token used for authentication in HTTP requests
    /// </summary>
    public string Token { get; set; }
    
    /// <summary>
    /// Linux 平台下的脚本，用于在更新完成后为文件分配权限
    /// Script for Linux platform to assign permissions to files after update completion
    /// </summary>
    public string Script { get; set; }
}
```

### UpdateOption 枚举 | UpdateOption Enum

```csharp
public enum UpdateOption
{
    /// <summary>
    /// 更新包文件格式（默认为 Zip）
    /// Update package file format (default is Zip)
    /// </summary>
    Format,
    
    /// <summary>
    /// 压缩编码格式
    /// Compression encoding format
    /// </summary>
    Encoding,
    
    /// <summary>
    /// 下载超时时间（秒）。如果不指定，默认超时时间为 30 秒
    /// Download timeout duration (seconds). Default is 30 seconds if not specified
    /// </summary>
    DownloadTimeOut,
    
    /// <summary>
    /// 是否启用二进制差异更新功能，默认启用；设置为 false 则执行全量覆盖安装
    /// Whether to enable binary differential update function, enabled by default; 
    /// if set to false, full overwrite installation will be performed
    /// </summary>
    Patch,
    
    /// <summary>
    /// 是否在更新前启用备份功能，默认启用；设置为 false 则不进行备份
    /// Whether to enable backup function before update, enabled by default; 
    /// if set to false, no backup will be performed
    /// </summary>
    BackUp
}
```

---

## 实际使用示例 | Practical Usage Examples

### 示例 1：基本更新流程 | Example 1: Basic Update Process

```csharp
using GeneralUpdate.ClientCore;
using GeneralUpdate.Common.Internal;

var config = new Configinfo
{
    UpdateUrl = "http://your-server.com/api/update/check",
    ReportUrl = "http://your-server.com/api/update/report",
    MainAppName = "MyApp.exe",
    AppName = "Updater.exe",
    ClientVersion = "1.0.0.0",
    UpgradeClientVersion = "1.0.0.0",
    InstallPath = AppDomain.CurrentDomain.BaseDirectory,
    ProductId = "your-product-id",
    AppSecretKey = "your-secret-key"
};

await new GeneralClientBootstrap()
    .SetConfig(config)
    .AddListenerException((sender, args) => 
    {
        Console.WriteLine($"更新异常: {args.Exception.Message}");
    })
    .LaunchAsync();
```

### 示例 2：带黑名单的更新 | Example 2: Update with Blacklist

```csharp
using GeneralUpdate.ClientCore;
using GeneralUpdate.Common.Internal;

var config = new Configinfo
{
    UpdateUrl = "http://your-server.com/api/update/check",
    ClientVersion = "1.0.0.0",
    InstallPath = AppDomain.CurrentDomain.BaseDirectory,
    // 黑名单配置 | Blacklist configuration
    BlackFiles = new List<string> { "config.json", "userdata.db" },
    BlackFormats = new List<string> { ".log", ".cache" },
    SkipDirectorys = new List<string> { "logs", "temp" }
};

await new GeneralClientBootstrap()
    .SetConfig(config)
    .LaunchAsync();
```

### 示例 3：自定义更新选项 | Example 3: Custom Update Options

```csharp
using System.Text;
using GeneralUpdate.ClientCore;
using GeneralUpdate.Common.Internal;
using GeneralUpdate.Common.Internal.Bootstrap;

var config = new Configinfo
{
    UpdateUrl = "http://your-server.com/api/update/check",
    ClientVersion = "1.0.0.0",
    InstallPath = AppDomain.CurrentDomain.BaseDirectory
};

await new GeneralClientBootstrap()
    .SetConfig(config)
    // 设置下载超时 | Set download timeout
    .Option(UpdateOption.DownloadTimeOut, 120)
    // 设置编码格式 | Set encoding format
    .Option(UpdateOption.Encoding, Encoding.UTF8)
    // 启用二进制差异更新 | Enable binary differential update
    .Option(UpdateOption.Patch, true)
    // 启用备份 | Enable backup
    .Option(UpdateOption.BackUp, true)
    .LaunchAsync();
```

### 示例 4：完整的事件监听 | Example 4: Complete Event Listening

```csharp
using GeneralUpdate.ClientCore;
using GeneralUpdate.Common.Download;
using GeneralUpdate.Common.Internal;
using GeneralUpdate.Common.Shared.Object;

var config = new Configinfo
{
    UpdateUrl = "http://your-server.com/api/update/check",
    ClientVersion = "1.0.0.0",
    InstallPath = AppDomain.CurrentDomain.BaseDirectory
};

await new GeneralClientBootstrap()
    .SetConfig(config)
    // 下载统计 | Download statistics
    .AddListenerMultiDownloadStatistics((sender, args) =>
    {
        var version = args.Version as VersionInfo;
        Console.WriteLine($"[{version.Version}] 进度: {args.ProgressPercentage}% | " +
                         $"速度: {args.Speed} | 剩余: {args.Remaining}");
    })
    // 单个下载完成 | Single download completed
    .AddListenerMultiDownloadCompleted((sender, args) =>
    {
        var version = args.Version as VersionInfo;
        if (args.IsComplated)
            Console.WriteLine($"✓ 版本 {version.Version} 下载成功");
        else
            Console.WriteLine($"✗ 版本 {version.Version} 下载失败");
    })
    // 所有下载完成 | All downloads completed
    .AddListenerMultiAllDownloadCompleted((sender, args) =>
    {
        if (args.IsAllDownloadCompleted)
            Console.WriteLine("✓ 所有更新包下载完成，准备安装...");
        else
            Console.WriteLine($"✗ 下载失败，共 {args.FailedVersions.Count} 个版本失败");
    })
    // 下载错误 | Download error
    .AddListenerMultiDownloadError((sender, args) =>
    {
        var version = args.Version as VersionInfo;
        Console.WriteLine($"✗ 版本 {version.Version} 下载错误: {args.Exception.Message}");
    })
    // 异常处理 | Exception handling
    .AddListenerException((sender, args) =>
    {
        Console.WriteLine($"⚠ 更新异常: {args.Exception.Message}\n{args.Exception.StackTrace}");
    })
    .LaunchAsync();
```

### 示例 5：自定义操作和跳过选项 | Example 5: Custom Operations and Skip Options

```csharp
using GeneralUpdate.ClientCore;
using GeneralUpdate.Common.Internal;

var config = new Configinfo
{
    UpdateUrl = "http://your-server.com/api/update/check",
    ClientVersion = "1.0.0.0",
    InstallPath = AppDomain.CurrentDomain.BaseDirectory
};

await new GeneralClientBootstrap()
    .SetConfig(config)
    // 添加自定义操作（更新前检查环境）| Add custom operation (check environment before update)
    .AddCustomOption(async () =>
    {
        Console.WriteLine("正在检查运行环境...");
        await Task.Delay(1000);
        // 检查磁盘空间、依赖项等 | Check disk space, dependencies, etc.
        Console.WriteLine("环境检查完成");
    })
    // 设置用户跳过选项 | Set user skip option
    .SetCustomSkipOption(() =>
    {
        Console.WriteLine("发现新版本，是否更新？(y/n)");
        var input = Console.ReadLine();
        return input?.ToLower() == "y";
    })
    .LaunchAsync();
```

---

## 注意事项与警告 | Notes and Warnings

### ⚠️ 重要提示 | Important Notes

1. **版本号格式 | Version Number Format**
   - 版本号必须遵循语义化版本规范（如 1.0.0.0）
   - 确保客户端和服务器端版本号格式一致
   - Version numbers must follow semantic versioning specification (e.g., 1.0.0.0)
   - Ensure version number format is consistent between client and server

2. **网络连接 | Network Connection**
   - 确保更新服务器地址可访问
   - 建议实现重试机制处理网络波动
   - Ensure update server address is accessible
   - Recommend implementing retry mechanism to handle network fluctuations

3. **进程管理 | Process Management**
   - 更新过程会关闭主程序并启动升级助手
   - 确保保存所有用户数据后再执行更新
   - The update process will close the main program and launch the upgrade assistant
   - Ensure all user data is saved before executing the update

4. **权限要求 | Permission Requirements**
   - 在 Windows 上可能需要管理员权限来替换文件
   - 在 Linux/macOS 上需要适当的文件系统权限
   - Administrator privileges may be required on Windows to replace files
   - Appropriate file system permissions are required on Linux/macOS

5. **黑名单使用 | Blacklist Usage**
   - 黑名单中的文件和目录不会被更新
   - 常用于保护配置文件、用户数据等
   - Files and directories in the blacklist will not be updated
   - Commonly used to protect configuration files, user data, etc.

### 💡 最佳实践 | Best Practices

- **备份策略**：始终启用 BackUp 选项，以便更新失败时可以回滚
- **差异更新**：启用 Patch 选项以减少下载量和更新时间
- **错误处理**：实现完整的异常监听和错误处理逻辑
- **用户体验**：在更新前提示用户并允许选择更新时机
- **测试验证**：在生产环境部署前充分测试更新流程
- **Backup Strategy**: Always enable the BackUp option to allow rollback in case of update failure
- **Differential Update**: Enable the Patch option to reduce download size and update time
- **Error Handling**: Implement complete exception listening and error handling logic
- **User Experience**: Prompt users before updating and allow them to choose update timing
- **Testing**: Thoroughly test the update process before production deployment

---

## 适用平台 | Applicable Platforms

| 产品 Product        | 版本 Version          |
| ------------------ | -------------------- |
| .NET               | 5, 6, 7, 8, 9, 10    |
| .NET Framework     | 4.6.1                |
| .NET Standard      | 2.0                  |
| .NET Core          | 2.0                  |

---

## 相关资源 | Related Resources

- **示例代码**：[查看 GitHub 示例](https://github.com/GeneralLibrary/GeneralUpdate-Samples/blob/main/src/Client/Program.cs) | [View GitHub Examples](https://github.com/GeneralLibrary/GeneralUpdate-Samples/blob/main/src/Client/Program.cs)
- **主仓库**：[GeneralUpdate 项目](https://github.com/GeneralLibrary/GeneralUpdate) | [GeneralUpdate Project](https://github.com/GeneralLibrary/GeneralUpdate)
