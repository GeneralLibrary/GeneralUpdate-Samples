---
sidebar_position: 12
---

### 定义

命名空间：GeneralUpdate.Extension

程序集：GeneralUpdate.ClientCore.dll、GeneralUpdate.Core.dll



GeneralUpdate.Extension 提供了一个全面的扩展框架，允许开发人员自定义和扩展更新过程。它使您能够注入自定义逻辑、处理事件，并根据特定应用程序需求定制更新行为。对于需要对更新工作流进行细粒度控制的开发人员来说，此组件至关重要。

```c#
// 扩展方法通过 GeneralClientBootstrap 和 GeneralUpdateBootstrap 提供
public class GeneralClientBootstrap : AbstractBootstrap<GeneralClientBootstrap, IStrategy>
```



### 示例

Extension 框架提供多种扩展功能的方法 [[查看示例]](https://github.com/GeneralLibrary/GeneralUpdate-Samples/blob/main/src/Client/Program.cs)。

```c#
var configinfo = new Configinfo
{
    UpdateUrl = "http://127.0.0.1:5000/Upgrade/Verification",
    AppName = "YourApp.exe",
    ClientVersion = "1.0.0.0",
    // ... 其他配置
};

await new GeneralClientBootstrap()
    // 添加自定义事件监听器
    .AddListenerMultiDownloadStatistics(OnDownloadProgress)
    .AddListenerMultiDownloadCompleted(OnDownloadCompleted)
    .AddListenerException(OnException)
    // 添加自定义操作
    .AddCustomOption(CheckEnvironment)
    .SetCustomSkipOption(ShowSkipDialog)
    .SetConfig(configinfo)
    .LaunchAsync();
```



### 核心扩展能力

GeneralUpdate.Extension 提供以下扩展功能。

#### 事件监听器

| 方法                                    | 说明                                                  |
| -------------------------------------- | ------------------------------------------------------------ |
| AddListenerMultiDownloadStatistics()   | 订阅下载进度通知，包括速度、剩余时间、已接收字节数和进度百分比。非常适合向用户显示实时下载信息。 |
| AddListenerMultiDownloadCompleted()    | 单个更新包下载完成时触发的事件（无论成功或失败）。允许您处理每个版本的下载完成情况。 |
| AddListenerMultiAllDownloadCompleted() | 所有更新包下载完成时的通知。这是更新安装开始前的最后检查点。 |
| AddListenerMultiDownloadError()        | 监听每个版本的下载错误。提供异常详情以帮助诊断网络或服务器问题。 |
| AddListenerException()                 | 全局异常处理程序，捕获整个更新过程中的任何错误。对于错误日志记录和用户通知至关重要。 |

#### 自定义操作

| 方法                | 说明                                                  |
| --------------------- | ------------------------------------------------------------ |
| AddCustomOption()     | 向更新工作流注入自定义异步操作。在更新开始前执行。非常适合环境检查、更新前备份或任何准备任务。返回 Task<bool>，返回 false 将取消更新。 |
| SetCustomSkipOption() | 允许用户决定是否跳过非强制更新。提供一种显示自定义 UI 以获取用户同意的方法。仅在服务器未强制更新时有效。 |
| SetBlacklist()        | 定义永远不应更新的文件或文件格式。对于保留用户数据、配置文件或第三方依赖项很有用。 |

#### 配置选项

| 方法        | 说明                                                  |
| ------------- | ------------------------------------------------------------ |
| Option()      | 配置更新行为，例如超时时间、编码格式、补丁启用和备份设置。提供对更新过程的细粒度控制。 |
| Config()      | 设置核心更新参数，包括服务器 URL、版本信息、身份验证密钥和安装路径。 |
| GetOption()   | 检索当前更新配置设置以供检查或修改。 |
| SetConfig()   | 应用完整的 Configinfo 对象来配置更新客户端。 |



### 事件参数

#### MultiDownloadStatisticsEventArgs

提供详细的下载进度信息。

| 属性               | 类型        | 说明                                          |
| ------------------ | ----------- | ---------------------------------------------------- |
| Version            | VersionInfo | 当前正在下载的版本                     |
| Speed              | string      | 下载速度（例如 "2.5 MB/s"）                    |
| Remaining          | string      | 预计剩余时间（例如 "00:02:30"）          |
| BytesReceived      | long        | 到目前为止已下载的字节数                    |
| TotalBytesToReceive| long        | 下载的总大小（字节）                  |
| ProgressPercentage | double      | 下载进度百分比（0-100）            |

#### MultiDownloadCompletedEventArgs

指示单个更新包的完成状态。

| 属性     | 类型        | 说明                                          |
| ------------ | ----------- | ---------------------------------------------------- |
| Version      | VersionInfo | 已完成下载的版本信息       |
| IsComplated  | bool        | 下载是否成功完成          |

#### MultiAllDownloadCompletedEventArgs

所有下载任务的摘要。

| 属性               | 类型            | 说明                                |
| ---------------------- | --------------- | ------------------------------------------ |
| IsAllDownloadCompleted | bool            | 是否所有下载都成功完成|
| FailedVersions         | List<VersionInfo>| 下载失败的版本列表   |

#### MultiDownloadErrorEventArgs

下载失败的错误信息。

| 属性  | 类型        | 说明                                          |
| --------- | ----------- | ---------------------------------------------------- |
| Version   | VersionInfo | 遇到错误的版本                    |
| Exception | Exception   | 下载期间发生的异常          |

#### ExceptionEventArgs

全局异常信息。

| 属性  | 类型      | 说明                                          |
| --------- | --------- | ---------------------------------------------------- |
| Exception | Exception | 更新过程中发生的异常    |



### 自定义操作示例

以下是使用自定义操作的完整示例：

```c#
private async Task<bool> CheckEnvironment()
{
    try
    {
        // 检查是否安装了所需的依赖项
        if (!IsDependencyInstalled("SomeLibrary"))
        {
            Console.WriteLine("缺少必需的依赖项！");
            return false; // 取消更新
        }
        
        // 验证磁盘空间
        var requiredSpace = 500 * 1024 * 1024; // 500 MB
        if (GetAvailableDiskSpace() < requiredSpace)
        {
            Console.WriteLine("磁盘空间不足！");
            return false; // 取消更新
        }
        
        // 创建关键文件的备份
        await BackupUserData();
        
        return true; // 继续更新
    }
    catch (Exception ex)
    {
        Console.WriteLine($"环境检查失败：{ex.Message}");
        return false;
    }
}

// 注册自定义操作
await new GeneralClientBootstrap()
    .AddCustomOption(CheckEnvironment)
    .SetConfig(configinfo)
    .LaunchAsync();
```



### 跳过选项示例

允许用户跳过非强制更新：

```c#
private async Task<bool> ShowSkipDialog()
{
    // 向用户显示自定义对话框
    var result = await ShowUpdateDialog(
        "有新的更新可用！",
        "您想现在更新吗？",
        new[] { "立即更新", "跳过" }
    );
    
    return result == "立即更新";
}

// 注册跳过选项处理程序
await new GeneralClientBootstrap()
    .SetCustomSkipOption(ShowSkipDialog)
    .SetConfig(configinfo)
    .LaunchAsync();
```



### 黑名单配置

从更新中排除特定文件或格式：

```c#
var configinfo = new Configinfo
{
    // ... 其他配置
    BlackFiles = new List<string> 
    { 
        "userconfig.json",
        "license.dat",
        "custom.db"
    },
    BlackFormats = new List<string> 
    { 
        ".log",
        ".temp",
        ".cache"
    },
    BlackDirectories = new List<string>
    {
        "UserData",
        "Plugins\\Custom"
    }
};

// 或使用 SetBlacklist 方法
await new GeneralClientBootstrap()
    .SetBlacklist(blackFiles: new List<string> { "config.ini" })
    .SetConfig(configinfo)
    .LaunchAsync();
```



### 完整扩展示例

以下是使用多个扩展功能的综合示例：

```c#
using GeneralUpdate.ClientCore;
using GeneralUpdate.Common.Shared.Object;

public class UpdateManager
{
    public async Task StartUpdateWithExtensions()
    {
        var configinfo = new Configinfo
        {
            UpdateUrl = "http://127.0.0.1:5000/Upgrade/Verification",
            ReportUrl = "http://127.0.0.1:5000/Upgrade/Report",
            AppName = "MyApp.exe",
            MainAppName = "MyApp.exe",
            ClientVersion = "1.0.0.0",
            ProductId = "your-product-id",
            AppSecretKey = "your-secret-key",
            InstallPath = AppDomain.CurrentDomain.BaseDirectory,
            BlackFiles = new List<string> { "userdata.db" }
        };

        try
        {
            await new GeneralClientBootstrap()
                // 注册事件监听器
                .AddListenerMultiDownloadStatistics(OnDownloadProgress)
                .AddListenerMultiDownloadCompleted(OnDownloadCompleted)
                .AddListenerMultiAllDownloadCompleted(OnAllDownloadsCompleted)
                .AddListenerMultiDownloadError(OnDownloadError)
                .AddListenerException(OnException)
                // 添加自定义操作
                .AddCustomOption(PerformPreUpdateChecks)
                .SetCustomSkipOption(AskUserForUpdatePermission)
                // 配置选项
                .SetConfig(configinfo)
                .Option(UpdateOption.DownloadTimeOut, 120)
                .Option(UpdateOption.Patch, true)
                .Option(UpdateOption.BackUp, true)
                // 启动更新
                .LaunchAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"更新失败：{ex.Message}");
        }
    }

    private void OnDownloadProgress(object sender, MultiDownloadStatisticsEventArgs e)
    {
        var version = e.Version as VersionInfo;
        Console.WriteLine($"正在下载 {version.Version}：{e.ProgressPercentage:F2}%，速度 {e.Speed}");
        // 在此更新 UI 进度条
    }

    private void OnDownloadCompleted(object sender, MultiDownloadCompletedEventArgs e)
    {
        var version = e.Version as VersionInfo;
        if (e.IsComplated)
        {
            Console.WriteLine($"版本 {version.Version} 下载成功！");
        }
        else
        {
            Console.WriteLine($"版本 {version.Version} 下载失败！");
        }
    }

    private void OnAllDownloadsCompleted(object sender, MultiAllDownloadCompletedEventArgs e)
    {
        if (e.IsAllDownloadCompleted)
        {
            Console.WriteLine("所有更新下载成功！");
        }
        else
        {
            Console.WriteLine($"更新失败。{e.FailedVersions.Count} 个版本失败。");
        }
    }

    private void OnDownloadError(object sender, MultiDownloadErrorEventArgs e)
    {
        var version = e.Version as VersionInfo;
        Console.WriteLine($"下载 {version.Version} 时出错：{e.Exception.Message}");
    }

    private void OnException(object sender, ExceptionEventArgs e)
    {
        Console.WriteLine($"更新异常：{e.Exception.Message}");
        // 将异常记录到文件或监控系统
    }

    private async Task<bool> PerformPreUpdateChecks()
    {
        // 检查系统要求
        Console.WriteLine("正在执行更新前检查...");
        
        // 验证网络连接
        if (!await CheckNetworkConnection())
        {
            Console.WriteLine("没有可用的网络连接！");
            return false;
        }
        
        // 检查磁盘空间
        if (!HasSufficientDiskSpace(500 * 1024 * 1024))
        {
            Console.WriteLine("磁盘空间不足！");
            return false;
        }
        
        Console.WriteLine("更新前检查通过！");
        return true;
    }

    private async Task<bool> AskUserForUpdatePermission()
    {
        Console.WriteLine("有新更新可用。立即更新？（y/n）");
        var response = Console.ReadLine();
        return response?.ToLower() == "y";
    }

    private async Task<bool> CheckNetworkConnection()
    {
        // 实现网络检查
        return true;
    }

    private bool HasSufficientDiskSpace(long requiredBytes)
    {
        // 实现磁盘空间检查
        return true;
    }
}
```



### 最佳实践

1. **始终处理异常**：使用 `AddListenerException()` 捕获并记录所有更新错误。

2. **更新前验证环境**：使用 `AddCustomOption()` 检查系统要求、依赖项和可用磁盘空间。

3. **提供用户反馈**：订阅下载进度事件以向用户显示实时更新。

4. **保护用户数据**：使用黑名单功能从更新中排除用户配置文件和数据。

5. **实现重试逻辑**：在错误处理程序中，为临时故障实现智能重试机制。

6. **测试自定义操作**：彻底测试所有自定义操作，以确保它们不会阻止合法更新。

7. **使用适当的超时**：根据预期文件大小和网络条件设置合理的超时值。

8. **记录所有内容**：维护更新过程的详细日志以便故障排除。



### 常见用例

#### 用例 1：环境验证

更新前，验证是否满足所有先决条件：

```c#
private async Task<bool> ValidateEnvironment()
{
    // 检查 .NET 运行时版本
    if (!IsRuntimeVersionSupported())
        return false;
    
    // 验证所需服务是否正在运行
    if (!AreServicesRunning(new[] { "ServiceA", "ServiceB" }))
        return false;
    
    // 检查文件权限
    if (!HasWritePermission(installPath))
        return false;
    
    return true;
}
```

#### 用例 2：自定义更新通知

在更新过程中向用户显示自定义通知：

```c#
private void NotifyUser(object sender, MultiDownloadStatisticsEventArgs e)
{
    // 显示系统托盘通知
    ShowNotification($"正在更新：{e.ProgressPercentage:F0}%");
    
    // 更新应用程序标题
    UpdateWindowTitle($"正在下载更新... {e.ProgressPercentage:F0}%");
}
```

#### 用例 3：条件更新

根据特定条件应用更新：

```c#
private async Task<bool> ShouldApplyUpdate()
{
    // 检查是否在营业时间内
    var now = DateTime.Now;
    if (now.Hour >= 9 && now.Hour <= 17)
    {
        // 在营业时间推迟更新
        return false;
    }
    
    // 检查是否有关键操作正在运行
    if (IsCriticalOperationInProgress())
    {
        return false;
    }
    
    return true;
}
```



### 故障排除

#### 问题：自定义操作无限期地阻止更新

**解决方案**：确保您的自定义操作具有超时和适当的错误处理：

```c#
private async Task<bool> CustomOperationWithTimeout()
{
    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
    try
    {
        await YourAsyncOperation(cts.Token);
        return true;
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("操作超时");
        return false;
    }
}
```

#### 问题：事件未触发

**解决方案**：确保在调用 `LaunchAsync()` 之前注册事件监听器：

```c#
// 正确的顺序
await new GeneralClientBootstrap()
    .AddListenerException(OnException)  // 先注册
    .SetConfig(configinfo)
    .LaunchAsync();  // 最后启动
```

#### 问题：跳过选项不起作用

**解决方案**：验证服务器是否未将更新设置为强制更新。跳过选项仅适用于非强制更新。



### 适用于

| 产品           | 版本          |
| -------------- | ---------------- |
| .NET           | 5, 6, 7, 8, 9, 10|
| .NET Framework | 4.6.1            |
| .NET Standard  | 2.0              |
| .NET Core      | 2.0              |

### 另请参阅

- [GeneralUpdate.ClientCore](./GeneralUpdate.ClientCore.md) - 主要客户端更新组件
- [GeneralUpdate.Core](./GeneralUpdate.Core.md) - 核心更新逻辑
- [快速入门指南](../quickstart/Quik%20start.md) - GeneralUpdate 入门
