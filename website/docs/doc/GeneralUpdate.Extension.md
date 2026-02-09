---
sidebar_position: 12
---

### 定义

命名空间：GeneralUpdate.Extension

程序集：GeneralUpdate.Extension.dll

**NuGet 包**：GeneralUpdate.Extension



GeneralUpdate.Extension 是一个受 VS Code 启发的 .NET 应用程序扩展管理系统。它提供完整的插件/扩展管理功能，包括扩展下载、安装、更新、版本兼容性检查、平台支持、依赖关系解析和回滚机制。

```c#
public class GeneralExtensionHost : IExtensionHost
{
    public IExtensionCatalog ExtensionCatalog { get; }
    public event EventHandler<ExtensionUpdateEventArgs>? ExtensionUpdateStatusChanged;
}
```



### 快速开始

最基本的使用示例，初始化扩展主机并查询远程扩展 [[查看完整示例]](https://github.com/GeneralLibrary/GeneralUpdate/blob/master/src/c%23/GeneralUpdate.Extension/Examples/ExtensionExample.cs)。

```c#
using GeneralUpdate.Extension;
using GeneralUpdate.Extension.Core;
using GeneralUpdate.Extension.Common.Models;
using GeneralUpdate.Extension.Common.DTOs;

// 1. 初始化扩展主机
var options = new ExtensionHostOptions
{
    ServerUrl = "https://extensions.example.com/api",
    BearerToken = "your-bearer-token",          // 可选：用于身份验证
    HostVersion = "1.0.0",                      // 您的应用程序版本
    ExtensionsDirectory = "./extensions"         // 扩展安装目录
};

var host = new GeneralExtensionHost(options);

// 2. 订阅更新事件
host.ExtensionUpdateStatusChanged += (sender, e) =>
{
    Console.WriteLine($"扩展: {e.ExtensionName}");
    Console.WriteLine($"状态: {e.Status}");
    Console.WriteLine($"进度: {e.Progress}%");
};

// 3. 查询可用扩展
var query = new ExtensionQueryDTO
{
    Platform = TargetPlatform.Windows,
    HostVersion = "1.0.0",
    PageNumber = 1,
    PageSize = 20
};

var result = await host.QueryExtensionsAsync(query);
if (result.Success && result.Data != null)
{
    foreach (var ext in result.Data.Items)
    {
        Console.WriteLine($"{ext.DisplayName} v{ext.Version}");
        Console.WriteLine($"兼容: {ext.IsCompatible}");
    }
}

// 4. 更新扩展
bool success = await host.UpdateExtensionAsync("extension-guid");
```



### 核心功能

GeneralUpdate.Extension 提供以下核心功能。

#### 扩展主机 (GeneralExtensionHost)

主要的扩展管理容器，提供所有扩展操作的入口点。

| 方法                        | 说明                                                  |
| --------------------------- | ------------------------------------------------------------ |
| QueryExtensionsAsync()      | 从远程服务器查询可用扩展。支持按名称、平台、版本、状态等条件过滤。 |
| UpdateExtensionAsync()      | 更新指定扩展。自动处理下载、兼容性检查、平台验证和安装。 |
| DownloadExtensionAsync()    | 下载扩展包到指定路径。支持断点续传（HTTP Range 请求）。 |
| InstallExtensionAsync()     | 安装扩展包。支持自动回滚功能，安装失败时恢复到之前状态。 |
| IsExtensionCompatible()     | 检查扩展与当前主机版本是否兼容。                 |
| SetAutoUpdate()             | 为指定扩展启用/禁用自动更新。                     |
| SetGlobalAutoUpdate()       | 启用/禁用所有扩展的全局自动更新。                 |
| IsAutoUpdateEnabled()       | 检查扩展是否启用自动更新。                         |

#### 扩展目录 (ExtensionCatalog)

管理本地已安装扩展的目录，使用 JSON 文件持久化存储。

| 方法                              | 说明                                                  |
| --------------------------------- | ------------------------------------------------------------ |
| LoadInstalledExtensions()         | 从 catalog.json 加载已安装的扩展列表。               |
| GetInstalledExtensions()          | 获取所有已安装扩展的列表。                           |
| GetInstalledExtensionById()       | 根据扩展 ID（GUID）获取特定扩展。                     |
| GetInstalledExtensionsByPlatform()| 获取支持指定平台的扩展列表。                         |
| AddOrUpdateExtension()            | 添加新扩展或更新现有扩展的元数据。                   |
| RemoveExtension()                 | 从目录中删除扩展记录。                               |
| SaveCatalog()                     | 将目录保存到 JSON 文件。                             |

#### 下载队列 (DownloadQueueManager)

管理扩展下载队列，支持并发下载和状态跟踪。

| 方法                     | 说明                                                  |
| ------------------------ | ------------------------------------------------------------ |
| EnqueueDownload()        | 将下载任务添加到队列。任务将自动排队并处理。         |
| GetDownloadStatus()      | 获取指定下载任务的当前状态。                         |
| CancelDownload()         | 取消正在进行的下载任务。                             |

下载队列特性：
- 默认并发下载数：3 个
- 自动状态跟踪（排队、下载中、已完成、失败）
- 支持断点续传
- 事件通知下载状态变化



### 扩展元数据

每个扩展必须包含以下元数据信息：

| 属性               | 类型            | 说明                                          |
| ------------------ | --------------- | ---------------------------------------------------- |
| Id                 | string          | 扩展唯一标识符（GUID 格式）                   |
| Name               | string          | 扩展名称（唯一，小写，无空格）               |
| DisplayName        | string          | 人类可读的显示名称                           |
| Version            | string          | 语义化版本号（例如 "1.2.3"）                 |
| Publisher          | string          | 发布者标识符                                 |
| Description        | string          | 扩展描述                                     |
| SupportedPlatforms | TargetPlatform  | 支持的平台标志（Windows/Linux/MacOS/All）    |
| MinHostVersion     | string          | 最低兼容的主机版本                           |
| MaxHostVersion     | string          | 最高兼容的主机版本                           |
| Dependencies       | string          | 依赖的扩展 ID 列表（逗号分隔）               |
| Format             | string          | 文件格式（.dll、.zip 等）                    |
| Categories         | string          | 扩展分类（逗号分隔）                         |
| IsPreRelease       | bool            | 是否为预发布版本                             |
| License            | string          | 许可证标识符（例如 "MIT"、"Apache-2.0"）     |
| FileSize           | long            | 文件大小（字节）                             |
| Hash               | string          | 文件哈希值（SHA256）                         |
| DownloadUrl        | string          | 下载 URL                                     |



### 初始化和配置

#### 基本初始化

```c#
var options = new ExtensionHostOptions
{
    ServerUrl = "https://extensions.example.com/api",
    HostVersion = "1.0.0",
    ExtensionsDirectory = "./extensions"
};

var host = new GeneralExtensionHost(options);
```

#### 带身份验证的初始化

```c#
var options = new ExtensionHostOptions
{
    ServerUrl = "https://extensions.example.com/api",
    BearerToken = "your-bearer-token",
    HostVersion = "1.0.0",
    ExtensionsDirectory = "./extensions",
    CatalogPath = "./extensions/catalog.json"  // 可选：自定义目录文件路径
};

var host = new GeneralExtensionHost(options);
```

#### 订阅事件

```c#
host.ExtensionUpdateStatusChanged += (sender, e) =>
{
    switch (e.Status)
    {
        case ExtensionUpdateStatus.Queued:
            Console.WriteLine($"{e.ExtensionName} 已加入队列");
            break;
        case ExtensionUpdateStatus.Updating:
            Console.WriteLine($"{e.ExtensionName} 更新中... {e.Progress}%");
            break;
        case ExtensionUpdateStatus.UpdateSuccessful:
            Console.WriteLine($"{e.ExtensionName} 更新成功！");
            break;
        case ExtensionUpdateStatus.UpdateFailed:
            Console.WriteLine($"{e.ExtensionName} 更新失败: {e.ErrorMessage}");
            break;
    }
};
```



### 查询远程扩展

#### 基本查询

```c#
var query = new ExtensionQueryDTO
{
    PageNumber = 1,
    PageSize = 20
};

var result = await host.QueryExtensionsAsync(query);
if (result.Success && result.Data != null)
{
    Console.WriteLine($"找到 {result.Data.TotalCount} 个扩展");
    foreach (var ext in result.Data.Items)
    {
        Console.WriteLine($"• {ext.DisplayName} v{ext.Version}");
        Console.WriteLine($"  发布者: {ext.Publisher}");
        Console.WriteLine($"  兼容: {ext.IsCompatible}");
    }
}
```

#### 高级查询（带过滤条件）

```c#
var query = new ExtensionQueryDTO
{
    Name = "my-extension",                    // 按名称搜索
    Platform = TargetPlatform.Windows,        // 只查询 Windows 扩展
    HostVersion = "1.0.0",                    // 检查与此版本的兼容性
    Status = true,                            // 只查询已启用的扩展
    BeginDate = DateTime.Now.AddMonths(-1),   // 最近一个月的扩展
    EndDate = DateTime.Now,
    PageNumber = 1,
    PageSize = 50
};

var result = await host.QueryExtensionsAsync(query);
```



### 安装和更新扩展

#### 自动更新扩展

最简单的方式，一次调用完成所有操作：

```c#
// UpdateExtensionAsync 自动执行以下步骤：
// 1. 从服务器查询扩展信息
// 2. 检查版本兼容性
// 3. 检查平台支持
// 4. 下载扩展包
// 5. 安装扩展
// 6. 更新本地目录

string extensionId = "550e8400-e29b-41d4-a716-446655440000";
bool success = await host.UpdateExtensionAsync(extensionId);

if (success)
{
    Console.WriteLine("扩展更新成功！");
}
else
{
    Console.WriteLine("扩展更新失败，请检查事件获取详细信息");
}
```

#### 手动下载和安装

分步骤控制安装过程：

```c#
// 步骤 1：下载扩展
string extensionId = "550e8400-e29b-41d4-a716-446655440000";
string savePath = "./downloads/my-extension.zip";

bool downloaded = await host.DownloadExtensionAsync(extensionId, savePath);

if (downloaded)
{
    // 步骤 2：安装扩展（带回滚功能）
    bool installed = await host.InstallExtensionAsync(
        extensionPath: savePath,
        rollbackOnFailure: true  // 失败时自动回滚
    );
    
    if (installed)
    {
        Console.WriteLine("扩展安装成功！");
    }
    else
    {
        Console.WriteLine("安装失败，已自动回滚");
    }
}
```



### 管理已安装扩展

#### 列出所有扩展

```c#
var extensions = host.ExtensionCatalog.GetInstalledExtensions();

Console.WriteLine($"已安装 {extensions.Count} 个扩展：");
foreach (var ext in extensions)
{
    Console.WriteLine($"• {ext.DisplayName} v{ext.Version}");
    Console.WriteLine($"  ID: {ext.Id}");
    Console.WriteLine($"  状态: {(ext.Status == true ? "启用" : "禁用")}");
    Console.WriteLine($"  平台: {ext.SupportedPlatforms}");
}
```

#### 获取特定扩展

```c#
string extensionId = "550e8400-e29b-41d4-a716-446655440000";
var extension = host.ExtensionCatalog.GetInstalledExtensionById(extensionId);

if (extension != null)
{
    Console.WriteLine($"扩展名称: {extension.DisplayName}");
    Console.WriteLine($"版本: {extension.Version}");
    Console.WriteLine($"发布者: {extension.Publisher}");
    Console.WriteLine($"描述: {extension.Description}");
}
else
{
    Console.WriteLine("未找到扩展");
}
```

#### 按平台筛选扩展

```c#
var windowsExtensions = host.ExtensionCatalog
    .GetInstalledExtensionsByPlatform(TargetPlatform.Windows);

Console.WriteLine($"Windows 扩展: {windowsExtensions.Count} 个");

var linuxExtensions = host.ExtensionCatalog
    .GetInstalledExtensionsByPlatform(TargetPlatform.Linux);

Console.WriteLine($"Linux 扩展: {linuxExtensions.Count} 个");
```



### 版本兼容性

GeneralUpdate.Extension 自动检查版本兼容性，确保扩展与主机应用程序版本匹配。

#### 兼容性规则

扩展必须满足以下条件才被视为兼容：
1. 扩展的 MinHostVersion ≤ 主机版本
2. 扩展的 MaxHostVersion ≥ 主机版本
3. 两个条件必须同时满足

#### 示例

假设主机版本为 1.5.0：

| 扩展 MinHostVersion | 扩展 MaxHostVersion | 结果     |
| ------------------- | ------------------- | -------- |
| 1.0.0               | 2.0.0               | ✓ 兼容   |
| 1.6.0               | 2.0.0               | ✗ 不兼容 |
| 1.0.0               | 1.4.0               | ✗ 不兼容 |
| 1.5.0               | 1.5.0               | ✓ 兼容   |

#### 检查兼容性

```c#
var extension = host.ExtensionCatalog.GetInstalledExtensionById(extensionId);
if (extension != null)
{
    bool isCompatible = host.IsExtensionCompatible(extension);
    
    if (isCompatible)
    {
        Console.WriteLine("✓ 扩展与当前版本兼容");
    }
    else
    {
        Console.WriteLine("✗ 扩展与当前版本不兼容");
        Console.WriteLine($"  需要主机版本: {extension.MinHostVersion} - {extension.MaxHostVersion}");
        Console.WriteLine($"  当前主机版本: {options.HostVersion}");
    }
}
```



### 平台支持

GeneralUpdate.Extension 支持多平台扩展，可以为不同操作系统提供不同的扩展版本。

#### 平台标志

```c#
// 单个平台
TargetPlatform.Windows    // 仅 Windows
TargetPlatform.Linux      // 仅 Linux
TargetPlatform.MacOS      // 仅 macOS

// 多平台（使用位标志组合）
TargetPlatform.Windows | TargetPlatform.Linux  // Windows 和 Linux
TargetPlatform.All                              // 所有平台
```

#### 自动平台检测

系统自动检测当前运行平台，并筛选兼容的扩展：

```c#
// 查询时自动按平台过滤
var query = new ExtensionQueryDTO
{
    Platform = TargetPlatform.Windows  // 只返回支持 Windows 的扩展
};

var result = await host.QueryExtensionsAsync(query);
```

#### 平台特定扩展

```c#
// 示例：为不同平台设置扩展元数据
var extension = new ExtensionMetadata
{
    Name = "my-extension",
    DisplayName = "My Extension",
    Version = "1.0.0",
    // 仅支持 Windows 和 Linux
    SupportedPlatforms = TargetPlatform.Windows | TargetPlatform.Linux
};
```



### 依赖关系解析

GeneralUpdate.Extension 自动处理扩展之间的依赖关系。

#### 依赖关系特性

1. **自动解析传递依赖**：如果 A 依赖 B，B 依赖 C，系统会自动识别并安装 C。
2. **循环依赖检测**：自动检测并防止循环依赖。
3. **正确的安装顺序**：按依赖顺序安装（先安装被依赖项）。
4. **缺失依赖检查**：安装前检查所有依赖是否可用。

#### 定义依赖关系

在扩展元数据中使用逗号分隔的 GUID 列表：

```c#
var extension = new ExtensionMetadata
{
    Id = "550e8400-e29b-41d4-a716-446655440001",
    Name = "my-extension",
    DisplayName = "My Extension",
    Version = "1.0.0",
    // 依赖两个其他扩展
    Dependencies = "550e8400-e29b-41d4-a716-446655440002,550e8400-e29b-41d4-a716-446655440003"
};
```

#### 依赖自动处理

```c#
// UpdateExtensionAsync 自动处理依赖：
// 1. 识别所有依赖项
// 2. 检查是否已安装
// 3. 下载并安装缺失的依赖
// 4. 按正确顺序安装

bool success = await host.UpdateExtensionAsync(extensionId);
// 系统会自动安装所有必需的依赖扩展
```



### 回滚机制

InstallExtensionAsync 支持自动回滚功能，在安装失败时恢复到之前的状态。

#### 工作原理

1. **创建备份**：安装前，系统备份现有扩展（如果存在）。
2. **尝试安装**：执行扩展安装操作。
3. **成功时**：删除备份文件。
4. **失败时**：自动从备份恢复，撤销所有更改。

#### 使用回滚功能

```c#
bool success = await host.InstallExtensionAsync(
    extensionPath: "./downloads/extension.zip",
    rollbackOnFailure: true  // 启用自动回滚
);

if (!success)
{
    Console.WriteLine("安装失败，但已自动回滚到之前的版本");
    Console.WriteLine("您的扩展目录保持不变");
}
```

#### 最佳实践

- 生产环境中**始终启用回滚**（`rollbackOnFailure: true`）
- 确保有足够的磁盘空间用于备份
- 监控备份目录（默认为 `<ExtensionsDirectory>/.backup`）



### 自动更新设置

GeneralUpdate.Extension 支持扩展的自动更新功能。

#### 为单个扩展启用自动更新

```c#
string extensionId = "550e8400-e29b-41d4-a716-446655440000";

// 启用自动更新
host.SetAutoUpdate(extensionId, true);

// 禁用自动更新
host.SetAutoUpdate(extensionId, false);

// 检查状态
bool autoUpdateEnabled = host.IsAutoUpdateEnabled(extensionId);
Console.WriteLine($"自动更新: {(autoUpdateEnabled ? "启用" : "禁用")}");
```

#### 全局自动更新

```c#
// 为所有扩展启用自动更新
host.SetGlobalAutoUpdate(true);

// 禁用全局自动更新
host.SetGlobalAutoUpdate(false);
```



### 下载队列管理

下载队列管理器处理并发下载，提供状态跟踪和取消功能。

#### 队列特性

- 默认并发下载限制：3 个同时下载
- 自动状态跟踪
- 支持取消正在进行的下载
- 支持断点续传（HTTP Range 请求）

#### 下载状态

下载任务可能处于以下状态之一：

```c#
public enum ExtensionUpdateStatus
{
    Queued,            // 已加入队列，等待下载
    Updating,          // 正在下载/更新
    UpdateSuccessful,  // 下载/更新成功
    UpdateFailed       // 下载/更新失败
}
```

#### 监控下载

```c#
host.ExtensionUpdateStatusChanged += (sender, e) =>
{
    Console.WriteLine($"扩展: {e.ExtensionName ?? e.ExtensionId}");
    Console.WriteLine($"状态: {e.Status}");
    
    if (e.Status == ExtensionUpdateStatus.Updating)
    {
        Console.WriteLine($"进度: {e.Progress}%");
    }
    
    if (e.Status == ExtensionUpdateStatus.UpdateFailed)
    {
        Console.WriteLine($"错误: {e.ErrorMessage}");
    }
};
```



### 服务器 API 要求

GeneralUpdate.Extension 需要服务器提供以下 API 端点：

#### 查询扩展

```
POST {ServerUrl}/extensions
Content-Type: application/json
Authorization: Bearer {BearerToken}

请求体: ExtensionQueryDTO
响应: HttpResponseDTO<PagedResultDTO<ExtensionDTO>>
```

#### 下载扩展

```
GET {ServerUrl}/extensions/{id}
Authorization: Bearer {BearerToken}

响应: 文件流（支持 HTTP Range 请求以实现断点续传）
```



### 完整使用示例

以下是一个完整的示例，展示了扩展管理的主要操作：

```c#
using GeneralUpdate.Extension;
using GeneralUpdate.Extension.Core;
using GeneralUpdate.Extension.Common.Models;
using GeneralUpdate.Extension.Common.DTOs;
using GeneralUpdate.Extension.Common.Enums;
using System;
using System.Threading.Tasks;

public class ExtensionManagerExample
{
    public static async Task Main()
    {
        // 1. 初始化扩展主机
        var options = new ExtensionHostOptions
        {
            ServerUrl = "https://extensions.example.com/api",
            BearerToken = "your-bearer-token",
            HostVersion = "1.0.0",
            ExtensionsDirectory = "./extensions"
        };

        var host = new GeneralExtensionHost(options);

        // 2. 订阅更新事件
        host.ExtensionUpdateStatusChanged += (sender, e) =>
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {e.ExtensionName}");
            Console.WriteLine($"  状态: {e.Status}");
            
            if (e.Status == ExtensionUpdateStatus.Updating)
            {
                Console.WriteLine($"  进度: {e.Progress}%");
            }
            
            if (e.Status == ExtensionUpdateStatus.UpdateFailed)
            {
                Console.WriteLine($"  错误: {e.ErrorMessage}");
            }
        };

        // 3. 查询远程扩展
        Console.WriteLine("=== 查询远程扩展 ===\n");
        var query = new ExtensionQueryDTO
        {
            Platform = TargetPlatform.Windows,
            HostVersion = options.HostVersion,
            Status = true,
            PageNumber = 1,
            PageSize = 10
        };

        var queryResult = await host.QueryExtensionsAsync(query);
        if (queryResult.Success && queryResult.Data != null)
        {
            Console.WriteLine($"找到 {queryResult.Data.TotalCount} 个扩展:\n");
            
            foreach (var ext in queryResult.Data.Items)
            {
                Console.WriteLine($"• {ext.DisplayName} v{ext.Version}");
                Console.WriteLine($"  ID: {ext.Id}");
                Console.WriteLine($"  发布者: {ext.Publisher}");
                Console.WriteLine($"  兼容: {ext.IsCompatible}");
                Console.WriteLine($"  平台: {ext.SupportedPlatforms}");
                Console.WriteLine();
            }
        }

        // 4. 更新特定扩展
        Console.WriteLine("\n=== 更新扩展 ===\n");
        string extensionId = "550e8400-e29b-41d4-a716-446655440000";
        
        bool updateSuccess = await host.UpdateExtensionAsync(extensionId);
        if (updateSuccess)
        {
            Console.WriteLine("✓ 扩展更新成功！");
        }
        else
        {
            Console.WriteLine("✗ 扩展更新失败");
        }

        // 5. 列出已安装的扩展
        Console.WriteLine("\n=== 已安装的扩展 ===\n");
        var installed = host.ExtensionCatalog.GetInstalledExtensions();
        Console.WriteLine($"总共 {installed.Count} 个已安装扩展:\n");
        
        foreach (var ext in installed)
        {
            Console.WriteLine($"• {ext.DisplayName} v{ext.Version}");
            Console.WriteLine($"  状态: {(ext.Status == true ? "启用" : "禁用")}");
            Console.WriteLine($"  平台: {ext.SupportedPlatforms}");
            
            // 检查兼容性
            bool compatible = host.IsExtensionCompatible(ext);
            Console.WriteLine($"  兼容: {(compatible ? "是" : "否")}");
            Console.WriteLine();
        }

        // 6. 配置自动更新
        Console.WriteLine("\n=== 配置自动更新 ===\n");
        host.SetGlobalAutoUpdate(true);
        Console.WriteLine("✓ 已启用全局自动更新");
        
        // 为特定扩展禁用自动更新
        host.SetAutoUpdate(extensionId, false);
        Console.WriteLine($"✓ 已为扩展 {extensionId} 禁用自动更新");
    }
}
```



### 最佳实践

1. **始终订阅事件**：在执行任何操作前订阅 `ExtensionUpdateStatusChanged` 事件，以监控进度和错误。

2. **使用回滚功能**：生产环境中安装扩展时始终启用 `rollbackOnFailure: true`。

3. **检查兼容性**：安装前使用 `IsExtensionCompatible()` 检查扩展兼容性。

4. **合理分页**：查询扩展时使用适当的 `PageSize`（建议 10-50），避免一次加载过多数据。

5. **安全存储令牌**：Bearer Token 应安全存储，不要硬编码在代码中。

6. **监控磁盘空间**：确保有足够的磁盘空间用于下载和备份。

7. **验证扩展元数据**：安装前验证扩展的元数据完整性和哈希值。

8. **处理网络错误**：实现重试逻辑处理网络临时故障。

9. **日志记录**：记录所有扩展操作和错误，便于故障排查。

10. **定期清理**：定期清理下载缓存和备份目录，释放磁盘空间。



### 故障排除

#### 问题：扩展下载失败

**可能原因**：
- 网络连接问题
- 服务器不可用
- Bearer Token 无效或过期

**解决方案**：
```c#
// 检查事件参数中的错误信息
host.ExtensionUpdateStatusChanged += (sender, e) =>
{
    if (e.Status == ExtensionUpdateStatus.UpdateFailed)
    {
        Console.WriteLine($"下载失败: {e.ErrorMessage}");
        // 记录详细信息用于调试
    }
};

// 实现重试逻辑
int maxRetries = 3;
for (int i = 0; i < maxRetries; i++)
{
    bool success = await host.UpdateExtensionAsync(extensionId);
    if (success) break;
    
    await Task.Delay(TimeSpan.FromSeconds(5)); // 等待后重试
}
```

#### 问题：扩展显示不兼容

**解决方案**：
```c#
var extension = host.ExtensionCatalog.GetInstalledExtensionById(extensionId);
if (extension != null && !host.IsExtensionCompatible(extension))
{
    Console.WriteLine("扩展版本要求:");
    Console.WriteLine($"  最低主机版本: {extension.MinHostVersion}");
    Console.WriteLine($"  最高主机版本: {extension.MaxHostVersion}");
    Console.WriteLine($"  当前主机版本: {options.HostVersion}");
    
    // 检查是否有兼容的版本可用
    var query = new ExtensionQueryDTO
    {
        Name = extension.Name,
        HostVersion = options.HostVersion
    };
    var result = await host.QueryExtensionsAsync(query);
    // 检查是否有兼容版本
}
```

#### 问题：安装失败但未回滚

**解决方案**：
确保启用了回滚选项：
```c#
bool success = await host.InstallExtensionAsync(
    extensionPath: savePath,
    rollbackOnFailure: true  // 必须设置为 true
);
```

检查备份目录权限：
```c#
string backupDir = Path.Combine(options.ExtensionsDirectory, ".backup");
if (!Directory.Exists(backupDir))
{
    Directory.CreateDirectory(backupDir);
}
// 确保有写入权限
```



### 适用于

| 产品           | 版本                                  |
| -------------- | ------------------------------------- |
| .NET Standard  | 2.0                                   |
| .NET Framework | 4.6.1+                                |
| .NET Core      | 2.0+                                  |
| .NET           | 5, 6, 7, 8, 9, 10                     |
| Mono           | 5.4+                                  |
| Xamarin.iOS    | 10.14+                                |
| Xamarin.Android| 8.0+                                  |

### 另请参阅

- [GeneralUpdate.ClientCore](./GeneralUpdate.ClientCore.md) - 主客户端更新组件
- [GeneralUpdate.Core](./GeneralUpdate.Core.md) - 核心更新逻辑
- [GitHub 源代码](https://github.com/GeneralLibrary/GeneralUpdate/tree/master/src/c%23/GeneralUpdate.Extension) - 完整源代码和更多示例
- [快速入门指南](../quickstart/Quik%20start.md) - GeneralUpdate 入门
