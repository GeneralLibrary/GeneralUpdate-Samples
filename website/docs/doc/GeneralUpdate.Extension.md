---
sidebar_position: 7
---

# GeneralUpdate.Extension

**命名空间:** `GeneralUpdate.Extension` | **主要入口:** `GeneralExtensionHost`（实现 `IExtensionHost`） | **NuGet 包:** `GeneralUpdate.Extension`

## 1. 组件简介

### 1.1 组件概述

**GeneralUpdate.Extension** 是面向 .NET 应用的扩展管理组件，设计目标是让宿主程序具备类似 VS Code 的扩展生态能力：从远程服务查询扩展、下载扩展包、安装或更新到本地目录，并在这个过程中处理版本兼容、平台匹配、依赖扩展、SHA256 校验、失败回滚和事件通知。

它适合把主程序和可选能力拆开发布的场景，例如报表、认证、行业插件、客户定制模块、脚本执行器等。主程序只需要集成 `GeneralExtensionHost`，扩展包可以独立发布、独立更新。

**核心能力：**

| 能力 | 说明 |
| --- | --- |
| 扩展查询 | 通过服务端 API 分页查询可用扩展，支持名称、发布者、分类、平台等筛选条件 |
| 一键更新 | `UpdateExtensionAsync` 串起查询→兼容性检查→平台检查→依赖递归安装→下载→SHA256 校验→安装→catalog 更新 |
| 安全安装 | Zip Slip 路径穿越防护、安装前备份、失败自动回滚到旧版本 |
| 批量更新 | `UpdateExtensionsAsync` 按顺序批量处理多个扩展，返回每个扩展的成功/失败结果 |
| 版本兼容性 | `MinHostVersion` ≤ `HostVersion` ≤ `MaxHostVersion` 范围内才允许安装 |
| 平台匹配 | `[Flags] TargetPlatform` 位运算判断扩展是否支持当前 OS |
| 依赖解析 | 拓扑排序依赖树，检测循环依赖，递归安装未安装的依赖扩展 |
| 断点续传 | 下载支持 HTTP Range，已存在部分文件时从断点继续 |
| 本地 Catalog | 每个扩展独立 `manifest.json`，原子写入（`.tmp` → 重命名），支持持久化和加载 |
| 生命周期钩子 | 安装前后、激活/停用前后、卸载前后的业务逻辑注入 |
| 自动更新策略 | `SetGlobalAutoUpdate` / `SetAutoUpdate` 控制全局或单扩展的自动更新开关 |
| DI 集成 | `ExtensionHostBuilder` 注册默认服务，所有服务均可通过 DI 替换 |

**解决的业务痛点：**
- 主程序体积膨胀，需要把非核心功能拆成可独立更新的扩展
- 不同客户需要不同功能组合，扩展生态可以实现按需安装
- 扩展之间有依赖关系，需要自动管理依赖的安装和版本兼容性
- 需要统一的扩展管理框架减少重复开发

**业务使用场景：**
- IDE 类应用的插件市场
- 企业 ERP/CRM 的行业模块（报表模板、认证方式、数据导出等）
- 客户定制功能独立分发
- 脚本执行器/工具集的组件化发布

### 1.2 环境与依赖

| 项目 | 说明 |
| --- | --- |
| **版本** | `10.5.0-beta.2` |
| **目标框架** | `netstandard2.0`（兼容 .NET Framework 4.6.1+ / .NET Core 2.0+ / .NET 5+） |
| **依赖包** | `Microsoft.Extensions.DependencyInjection`、`Microsoft.Extensions.Logging.Abstractions`、`Microsoft.Extensions.Options`、`Newtonsoft.Json`、`System.Net.Http`、`System.IO.Compression`、`System.IO.Compression.ZipFile` |
| **兼容性** | 所有支持 .NET Standard 2.0 的平台 |

---

## 2. 组件功能列表

| 功能名称 | 功能描述 | 类型 | 是否必填 | 备注限制 |
| --- | --- | --- | --- | --- |
| 扩展查询 | 从服务端 API 分页查询扩展列表 | 基础 | 推荐 | 支持多条件筛选 |
| 扩展下载 | 从服务端下载扩展 ZIP 包，支持断点续传 | 基础 | 自动 | 通过 `DownloadExtensionAsync` 或一键更新自动触发 |
| 扩展安装 | 安全解压 ZIP 到本地目录，支持 Zip Slip 防护和回滚 | 基础 | 自动 | 仅接受 `.zip` 格式 |
| 一键更新 | 自动串起查询→兼容性→依赖→下载→校验→安装全流程 | 基础 | 推荐 | `UpdateExtensionAsync` |
| 批量更新 | 按顺序批量更新多个扩展 | 拓展 | 可选 | `UpdateExtensionsAsync` |
| 扩展卸载 | 从本地 catalog 移除并删除扩展目录 | 基础 | 可选 | `UninstallExtensionAsync` |
| 版本兼容性检查 | 宿主版本必须在扩展的 Min/Max 范围内 | 基础 | 自动 | 更新流程中自动检查 |
| 平台匹配 | 自动识别当前 OS，匹配扩展支持的平台 | 基础 | 自动 | `PlatformMatcher` 通过 `RuntimeInformation` 检测 |
| 依赖递归安装 | 发现未安装依赖时递归调用更新 | 基础 | 自动 | 依赖必须能被同一服务端查询和下载 |
| 循环依赖检测 | 拓扑排序时检测依赖环 | 基础 | 自动 | `DependencyResolver` |
| SHA256 校验 | 下载后校验文件完整性 | 基础 | 自动 | 服务端 `Hash` 非空时校验 |
| 本地 Catalog 管理 | 每个扩展独立 `manifest.json`，原子写入 | 基础 | 自动 | 存储在扩展目录下 |
| 自动更新策略 | 全局/单扩展自动更新开关 | 拓展 | 可选 | 仅在内存中保存状态，不自动轮询 |
| 生命周期钩子 | 安装/激活/停用/卸载前后业务逻辑 | 拓展 | 可选 | 实现 `IExtensionLifecycleHooks` 或继承 `DefaultExtensionLifecycleHooks` |
| DI Builder | `ExtensionHostBuilder` 注册并替换所有服务 | 拓展 | 可选 | 支持自定义 `IExtensionServiceFactory` |
| 下载队列管理 | 并发下载控制（默认 3） | 拓展 | 可选 | `DownloadQueueManager` |

---

## 3. API 配置说明

### 3.1 配置字段（属性 Props）

**ExtensionHostOptions：**

| 字段名 | 数据类型 | 默认值 | 是否必填 | 枚举/取值范围 | 说明 |
| --- | --- | --- | --- | --- | --- |
| `ServerUrl` | `string` | — | 是 | 有效绝对 URL | 扩展服务根地址，客户端调用 `{ServerUrl}/Query` 和 `{ServerUrl}/Download/{extensionId}` |
| `Scheme` | `string` | `""` | 可选 | `"Bearer"` 等 | Authorization 认证方案，为空不设置认证头 |
| `Token` | `string` | `""` | 可选 | — | Authorization token，需和 `Scheme` 同时非空才生效 |
| `HostVersion` | `string` | — | 推荐 | SemVer 格式 | 宿主应用版本，用于兼容性判断 |
| `ExtensionsDirectory` | `string` | — | 是 | 有效目录路径 | 扩展包下载、安装和 `.backup` 目录所在位置 |
| `CatalogPath` | `string` | `null` | 可选 | 有效目录路径 | 本地扩展目录扫描路径，为空时使用 `ExtensionsDirectory` |

**ExtensionMetadata（本地模型）：**

| 字段名 | 数据类型 | 默认值 | 是否必填 | 说明 |
| --- | --- | --- | --- | --- |
| `Id` | `string` | — | 是 | 扩展唯一 ID，依赖、查询、更新、卸载都以它为关键标识 |
| `Name` | `string` | `null` | 推荐 | 扩展目录名和包名的稳定名称 |
| `DisplayName` | `string` | `null` | 可选 | 展示名称 |
| `Version` | `string` | `null` | 推荐 | 扩展版本，建议 `1.2.3` 格式 |
| `FileSize` | `long?` | `null` | 可选 | 包大小（字节） |
| `Format` | `string` | `null` | 推荐 | 包格式，当前安装要求 `.zip` |
| `Hash` | `string` | `null` | 推荐 | SHA256，非空时更新流程校验下载文件 |
| `Publisher` | `string` | `null` | 可选 | 发布者 |
| `Categories` | `string` | `null` | 可选 | 逗号分隔分类 |
| `SupportedPlatforms` | `TargetPlatform` | `All` | 推荐 | `[Flags]` 位标志：`Windows(1)`, `Linux(2)`, `MacOS(4)`, `All(7)` |
| `MinHostVersion` | `string` | `null` | 可选 | 最低宿主版本 |
| `MaxHostVersion` | `string` | `null` | 可选 | 最高宿主版本 |
| `Dependencies` | `string` | `null` | 可选 | 逗号分隔的依赖扩展 ID |
| `IsPreRelease` | `bool` | `false` | 可选 | 是否预发布 |
| `CustomProperties` | `string` | `null` | 可选 | JSON 字符串形式的自定义属性 |

**ExtensionQueryDTO（查询筛选）：**

| 字段名 | 数据类型 | 默认值 | 是否必填 | 说明 |
| --- | --- | --- | --- | --- |
| `Id` | `string?` | `null` | 可选 | 按 ID 精确查询 |
| `Name` | `string?` | `null` | 可选 | 按名称模糊匹配 |
| `Publisher` | `string?` | `null` | 可选 | 按发布者模糊匹配 |
| `Category` | `string?` | `null` | 可选 | 按分类筛选 |
| `Platform` | `TargetPlatform?` | `null` | 可选 | 按目标平台筛选 |
| `HostVersion` | `string?` | `null` | 可选 | 用于服务端兼容性判断 |
| `IsPreRelease` | `bool?` | `null` | 可选 | 是否包含预发布 |
| `Status` | `bool?` | `null` | 可选 | 按启用状态筛选 |
| `PageNumber` | `int` | `1` | 可选 | 页码（从 1 开始） |
| `PageSize` | `int` | `10` | 可选 | 每页大小 |

### 3.2 实例方法

**IExtensionHost：**

| 方法名 | 入参明细 | 返回值 | 使用场景 | 注意事项 |
| --- | --- | --- | --- | --- |
| `QueryExtensionsAsync(ExtensionQueryDTO)` | `query` — 查询条件 | `Task<HttpResponseDTO<PagedResultDTO<ExtensionDTO>>>` | 搜索/浏览可用扩展 | 响应数据在 `Body.Items` 中 |
| `DownloadExtensionAsync(string, string)` | `extensionId` — 扩展 ID；`savePath` — 保存路径 | `Task<bool>` | 单独下载扩展包 | 支持 HTTP Range 断点续传 |
| `UpdateExtensionAsync(string)` | `extensionId` — 扩展 ID | `Task<bool>` | 一键更新单个扩展（推荐入口） | 串起查询→兼容性→依赖→下载→校验→安装全流程 |
| `InstallExtensionAsync(string, bool)` | `extensionPath` — ZIP 包路径；`rollbackOnFailure` — 是否失败回滚 | `Task<bool>` | 手动安装本地扩展包 | 仅接受 `.zip` 格式 |
| `UpdateExtensionsAsync(IEnumerable<string>, CancellationToken)` | `extensionIds` — 扩展 ID 列表；`ct` — 取消令牌 | `Task<Dictionary<string, bool>>` | 批量更新 | 按传入顺序逐个处理 |
| `UninstallExtensionAsync(string, CancellationToken)` | `extensionId` — 扩展 ID；`ct` — 取消令牌 | `Task<bool>` | 卸载扩展 | 移除 catalog 记录并删除扩展目录 |
| `ActivateExtensionAsync(string, CancellationToken)` | `extensionId`；`ct` | `Task` | 激活扩展 | 调用生命周期钩子 |
| `DeactivateExtensionAsync(string, CancellationToken)` | `extensionId`；`ct` | `Task` | 停用扩展 | 调用生命周期钩子 |
| `IsExtensionCompatible(ExtensionMetadata)` | `extension` — 扩展元数据 | `bool` | 检查扩展兼容性 | 基于 `HostVersion` 与 `MinHostVersion`/`MaxHostVersion` 比较 |
| `SetAutoUpdate(string, bool)` | `extensionId` — 扩展 ID；`autoUpdate` — 是否自动更新 | `void` | 设置单扩展自动更新开关 | 仅内存状态，不自动后台轮询 |
| `SetGlobalAutoUpdate(bool)` | `enabled` — 是否启用 | `void` | 设置全局自动更新默认值 | 仅内存状态 |

**GeneralExtensionHost 附加方法：**

| 方法名 | 入参明细 | 返回值 | 使用场景 | 注意事项 |
| --- | --- | --- | --- | --- |
| `IsAutoUpdateEnabled(string)` | `extensionId` — 扩展 ID | `bool` | 查询指定扩展的自动更新开关 | 单扩展设置优先于全局设置 |

**ExtensionHostBuilder：**

| 方法名 | 入参明细 | 返回值 | 使用场景 | 注意事项 |
| --- | --- | --- | --- | --- |
| `ConfigureOptions(Action<ExtensionHostOptions>)` | `configure` — 配置委托 | `ExtensionHostBuilder` | 通过 Lambda 配置选项 | — |
| `WithOptions(ExtensionHostOptions)` | `options` — 选项对象 | `ExtensionHostBuilder` | 直接设置选项 | — |
| `ConfigureServices(Action<IServiceCollection>)` | `configure` — DI 注册委托 | `ExtensionHostBuilder` | 替换或添加服务 | 在 `Build()` 前调用 |
| `Build()` | 无 | `IExtensionHost` | 构建宿主实例 | 自动注册未覆盖的默认服务 |

### 3.3 回调事件

| 事件名称 | 回调参数 | 触发时机 | 使用说明 |
| --- | --- | --- | --- |
| `ExtensionUpdateStatusChanged` | `ExtensionUpdateEventArgs` — `ExtensionId`, `ExtensionName`, `Status`, `Progress`(0-100), `ErrorMessage` | 扩展更新流程各阶段 | `Status`: `Queued`→`Updating`（下载进度）→`UpdateSuccessful`/`UpdateFailed` |

**ExtensionUpdateStatus 枚举：**

| 值 | 说明 |
| --- | --- |
| `Queued` (0) | 已加入更新队列 |
| `Updating` (1) | 正在下载/更新中 |
| `UpdateSuccessful` (2) | 更新成功 |
| `UpdateFailed` (3) | 更新失败 |

---

## 4. 扩展示例（高阶用法）

### 4.1 组件可扩展能力总览

所有服务均可通过 `ExtensionHostBuilder.ConfigureServices()` 替换：

| 服务接口 | 默认实现 | 说明 |
| --- | --- | --- |
| `IExtensionHttpClient` | `ExtensionHttpClient` | HTTP 通信（查询/下载） |
| `IVersionCompatibilityChecker` | `VersionCompatibilityChecker` | 版本兼容性检查 |
| `IDownloadQueueManager` | `DownloadQueueManager` | 下载队列管理 |
| `IPlatformMatcher` | `PlatformMatcher` | 平台检测 |
| `IPlatformServices` | `RuntimePlatformServices` | 运行时平台信息 |
| `IExtensionMetadataMapper` | `DefaultExtensionMetadataMapper` | DTO→模型映射 |
| `IExtensionCatalog` | `ExtensionCatalog` | 本地扩展目录管理 |
| `IDependencyResolver` | `DependencyResolver` | 依赖解析 |
| `IExtensionLifecycleHooks` | `DefaultExtensionLifecycleHooks` | 生命周期钩子（所有方法 virtual） |
| `IExtensionServiceFactory` | `ExtensionServiceFactory` | 服务工厂 |

### 4.2 分场景示例

#### 场景 1：自定义生命周期钩子

【场景说明】在扩展安装前后执行自定义逻辑：安装前检查许可证、安装后初始化扩展数据库。

【示例代码】

```csharp
using GeneralUpdate.Extension.Core;
using GeneralUpdate.Extension.Common.Models;

public sealed class LicensedLifecycleHooks : DefaultExtensionLifecycleHooks
{
    public override async Task<bool> OnBeforeInstallAsync(
        ExtensionMetadata extension,
        string? packagePath,
        CancellationToken cancellationToken = default)
    {
        // 检查许可证
        if (!LicenseManager.IsLicensed(extension.Id))
        {
            Console.WriteLine($"Extension '{extension.Id}' is not licensed.");
            return false;  // 阻止安装
        }
        return true;
    }

    public override async Task OnAfterInstallAsync(
        ExtensionMetadata extension,
        CancellationToken cancellationToken = default)
    {
        // 初始化扩展数据库
        if (extension.CustomProperties != null)
        {
            var props = Newtonsoft.Json.JsonConvert
                .DeserializeObject<Dictionary<string, string>>(extension.CustomProperties);
            if (props?.ContainsKey("DbInitScript") == true)
            {
                await DatabaseInitializer.RunAsync(props["DbInitScript"], cancellationToken);
            }
        }
        Console.WriteLine($"Extension '{extension.DisplayName}' installed successfully.");
    }

    public override async Task<bool> OnBeforeUninstallAsync(
        ExtensionMetadata extension,
        CancellationToken cancellationToken = default)
    {
        // 检查是否有关联数据
        var hasData = await DataService.HasExtensionDataAsync(extension.Id, cancellationToken);
        if (hasData)
        {
            Console.WriteLine($"Extension '{extension.Id}' has associated data. Clean up first.");
            return false;  // 阻止卸载
        }
        return true;
    }
}

// 使用 Builder 注册
var host = new ExtensionHostBuilder()
    .WithOptions(options)
    .ConfigureServices(services =>
    {
        services.AddSingleton<IExtensionLifecycleHooks, LicensedLifecycleHooks>();
    })
    .Build();
```

【效果&注意事项】
- 返回 `false` 会阻止操作继续执行
- 所有钩子方法都是 `virtual`，只需覆写需要的部分

#### 场景 2：自定义 HTTP 客户端 + 共享连接池

【场景说明】与主应用共享 `HttpClient` 连接池，避免 socket 耗尽；同时切换为 POST 查询。

【示例代码】

```csharp
using GeneralUpdate.Extension.Communication;

// 共享主应用的 HttpClient
var sharedClient = new HttpClient();  // 或从 IHttpClientFactory 获取

var httpClient = new ExtensionHttpClient(
    serverUrl: "https://extensions.mycompany.com/Extension",
    scheme: "Bearer",
    token: "jwt-token",
    httpClient: sharedClient,
    ownsHttpClient: false)   // 不拥有，不 Dispose
{
    UsePostForQuery = true    // 服务端要求 POST 查询
};

var host = new ExtensionHostBuilder()
    .WithOptions(options)
    .ConfigureServices(services =>
    {
        services.AddSingleton<IExtensionHttpClient>(httpClient);
    })
    .Build();
```

【效果&注意事项】
- `ownsHttpClient: false` 确保 Dispose 时不关闭共享连接
- `UsePostForQuery = true` 将默认 GET+JSON Body 改为 POST+JSON Body

#### 场景 3：依赖解析 + 条件批量更新

【场景说明】用户选择安装一个扩展时，自动解析依赖并一起安装。

【示例代码】

```csharp
using GeneralUpdate.Extension.Core;
using GeneralUpdate.Extension.Common.DTOs;
using GeneralUpdate.Extension.Common.Enums;

var host = new GeneralExtensionHost(options);

// 查询目标扩展
var response = await host.QueryExtensionsAsync(new ExtensionQueryDTO
{
    Id = "report-extension",
    PageSize = 1
});

if (response.Body?.Items.Any() != true)
{
    Console.WriteLine("Extension not found.");
    return;
}

var ext = response.Body.Items.First();

// 解析依赖
var catalog = host.ExtensionCatalog;
catalog.LoadInstalledExtensions();

var resolver = new GeneralUpdate.Extension.Dependencies.DependencyResolver(catalog);
var deps = resolver.ResolveDependencies(
    new ExtensionMetadata { Id = ext.Id, Dependencies = string.Join(",", ext.Dependencies ?? []) });

var missingDeps = resolver.GetMissingDependencies(
    new ExtensionMetadata { Id = ext.Id, Dependencies = string.Join(",", ext.Dependencies ?? []) });

Console.WriteLine($"Dependencies for {ext.DisplayName}: {deps.Count} total, {missingDeps.Count} missing.");

// 先安装缺失依赖
var updateOrder = new List<string>();
updateOrder.AddRange(missingDeps);
updateOrder.Add(ext.Id);

var results = await host.UpdateExtensionsAsync(updateOrder);

foreach (var (id, success) in results)
    Console.WriteLine($"  {id}: {(success ? "OK" : "FAILED")}");
```

【效果&注意事项】
- `DependencyResolver.ResolveDependencies` 返回拓扑排序后的完整依赖列表
- `GetMissingDependencies` 过滤出未安装在本地 catalog 中的依赖
- 循环依赖会被检测并抛出异常

---

## 5. 常规使用示例

### 5.1 快速入门示例（最简 demo）

```csharp
using GeneralUpdate.Extension.Core;
using GeneralUpdate.Extension.Common.DTOs;
using GeneralUpdate.Extension.Common.Models;

var options = new ExtensionHostOptions
{
    ServerUrl = "https://extensions.example.com/Extension",
    Scheme = "Bearer",
    Token = "your-token",
    HostVersion = "1.0.0",
    ExtensionsDirectory = "./extensions"
};

var host = new GeneralExtensionHost(options);

host.ExtensionUpdateStatusChanged += (sender, e) =>
{
    Console.WriteLine($"[{e.Status}] {e.ExtensionId}: {e.Progress}% {e.ErrorMessage}");
};

// 查询可用扩展
var response = await host.QueryExtensionsAsync(new ExtensionQueryDTO
{
    Platform = TargetPlatform.Windows,
    PageNumber = 1,
    PageSize = 20
});

if (response.Body != null)
{
    foreach (var ext in response.Body.Items)
        Console.WriteLine($"{ext.DisplayName} v{ext.Version} [{ext.Id}]");
}

// 更新指定扩展
var success = await host.UpdateExtensionAsync("report-extension");
Console.WriteLine(success ? "Extension updated." : "Update failed.");
```

### 5.2 基础参数组合示例

```csharp
using GeneralUpdate.Extension.Core;
using GeneralUpdate.Extension.Common.DTOs;
using GeneralUpdate.Extension.Common.Enums;

var host = new GeneralExtensionHost(new ExtensionHostOptions
{
    ServerUrl = "https://extensions.mycompany.com/Extension",
    Scheme = "Bearer",
    Token = Environment.GetEnvironmentVariable("EXTENSION_TOKEN") ?? "",
    HostVersion = "2.0.0",
    ExtensionsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "extensions")
});

// 事件监听
host.ExtensionUpdateStatusChanged += (_, e) =>
{
    switch (e.Status)
    {
        case ExtensionUpdateStatus.Queued:
            Console.WriteLine($"{e.ExtensionId}: queued");
            break;
        case ExtensionUpdateStatus.Updating:
            Console.WriteLine($"{e.ExtensionId}: downloading... {e.Progress}%");
            break;
        case ExtensionUpdateStatus.UpdateSuccessful:
            Console.WriteLine($"{e.ExtensionName ?? e.ExtensionId}: updated successfully");
            break;
        case ExtensionUpdateStatus.UpdateFailed:
            Console.WriteLine($"{e.ExtensionId}: failed — {e.ErrorMessage}");
            break;
    }
};

// 安装本地扩展包
var installed = await host.InstallExtensionAsync(
    "./downloads/report-extension_1.0.0.zip",
    rollbackOnFailure: true);
Console.WriteLine(installed ? "Installed." : "Installation failed.");

// 查询已安装扩展
host.ExtensionCatalog.LoadInstalledExtensions();
var installedExts = host.ExtensionCatalog.GetInstalledExtensions();
foreach (var ext in installedExts)
{
    var compat = host.IsExtensionCompatible(ext);
    Console.WriteLine($"{ext.DisplayName} v{ext.Version} — compatible: {compat}");
}

// 配置自动更新策略
host.SetGlobalAutoUpdate(true);
host.SetAutoUpdate("large-extension", false);  // 大型扩展关闭自动更新

var checkResult = host.IsAutoUpdateEnabled("large-extension");
Console.WriteLine($"Auto-update for large-extension: {checkResult}");
```

### 5.3 真实业务落地示例

包含异常处理、依赖管理、兼容性检查的完整工作流：

```csharp
using GeneralUpdate.Extension.Core;
using GeneralUpdate.Extension.Common.DTOs;
using GeneralUpdate.Extension.Common.Enums;
using GeneralUpdate.Extension.Common.Models;

// 1. 初始化
var options = new ExtensionHostOptions
{
    ServerUrl = "https://extensions.mycompany.com/Extension",
    Scheme = "Bearer",
    Token = Configuration.GetExtensionToken(),
    HostVersion = AppInfo.CurrentVersion.ToString(),
    ExtensionsDirectory = Path.Combine(AppInfo.DataDirectory, "extensions")
};

// 2. 使用 Builder 注册自定义服务
var host = new ExtensionHostBuilder()
    .WithOptions(options)
    .ConfigureServices(services =>
    {
        services.AddSingleton<IExtensionLifecycleHooks, AuditLifecycleHooks>();
    })
    .Build();

host.ExtensionUpdateStatusChanged += OnExtensionStatusChanged;

// 3. 加载本地已安装扩展
host.ExtensionCatalog.LoadInstalledExtensions();
var installed = host.ExtensionCatalog.GetInstalledExtensions();
Console.WriteLine($"Loaded {installed.Count} installed extension(s).");

// 4. 查询服务端可用扩展
HttpResponseDTO<PagedResultDTO<ExtensionDTO>>? response = null;
try
{
    response = await host.QueryExtensionsAsync(new ExtensionQueryDTO
    {
        Platform = TargetPlatform.Windows | TargetPlatform.Linux,
        HostVersion = options.HostVersion,
        Status = true,
        PageNumber = 1,
        PageSize = 100
    });
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"Failed to query extensions: {ex.Message}");
    return;
}

if (response?.Body == null)
{
    Console.WriteLine($"Server returned: {response?.Code} {response?.Message}");
    return;
}

// 5. 筛选可更新的扩展
var toUpdate = new List<string>();
foreach (var ext in response.Body.Items)
{
    var installedExt = host.ExtensionCatalog.GetInstalledExtensionById(ext.Id);
    if (installedExt == null)
    {
        Console.WriteLine($"[NEW] {ext.DisplayName} v{ext.Version}");
        continue;  // 新扩展，不自动安装
    }

    if (!host.IsExtensionCompatible(new ExtensionMetadata
    {
        MinHostVersion = ext.MinHostVersion,
        MaxHostVersion = ext.MaxHostVersion
    }))
    {
        Console.WriteLine($"[INCOMPATIBLE] {ext.DisplayName}: requires host {ext.MinHostVersion}-{ext.MaxHostVersion}");
        continue;
    }

    if (Version.TryParse(ext.Version, out var remoteVer) &&
        Version.TryParse(installedExt.Version, out var localVer) &&
        remoteVer > localVer)
    {
        if (host.IsAutoUpdateEnabled(ext.Id))
        {
            Console.WriteLine($"[UPDATE] {ext.DisplayName}: {installedExt.Version} → {ext.Version}");
            toUpdate.Add(ext.Id);
        }
        else
        {
            Console.WriteLine($"[SKIP] {ext.DisplayName}: auto-update disabled");
        }
    }
}

// 6. 执行批量更新
if (toUpdate.Any())
{
    Console.WriteLine($"\nUpdating {toUpdate.Count} extension(s)...");
    var results = await host.UpdateExtensionsAsync(toUpdate);

    var succeeded = results.Count(r => r.Value);
    var failed = results.Count(r => !r.Value);

    Console.WriteLine($"\nDone: {succeeded} succeeded, {failed} failed.");
    foreach (var (id, success) in results.Where(r => !r.Value))
        Console.WriteLine($"  Failed: {id}");
}
else
{
    Console.WriteLine("All extensions up to date.");
}

// 事件处理
void OnExtensionStatusChanged(object? sender, ExtensionUpdateEventArgs e)
{
    switch (e.Status)
    {
        case ExtensionUpdateStatus.Queued:
            break;
        case ExtensionUpdateStatus.Updating:
            UpdateProgressUI(e.ExtensionId, e.Progress);
            break;
        case ExtensionUpdateStatus.UpdateSuccessful:
            Log.Info($"Extension '{e.ExtensionName ?? e.ExtensionId}' updated.");
            RefreshUI();
            break;
        case ExtensionUpdateStatus.UpdateFailed:
            Log.Error($"Extension '{e.ExtensionId}' update failed: {e.ErrorMessage}");
            NotifyUser($"Failed to update {e.ExtensionName ?? e.ExtensionId}");
            break;
    }
}
```

---

## 6. 全局配置

### 服务器 API 契约

**查询接口：**

```http
GET {ServerUrl}/Query
Content-Type: application/json
Authorization: {Scheme} {Token}

Body: ExtensionQueryDTO (JSON)
```

> 注意：当前实现使用 GET + JSON Body，非标准 HTTP 风格。经过代理/网关时可能需要调整为 POST 或 query string。

**下载接口：**

```http
GET {ServerUrl}/Download/{extensionId}
Authorization: {Scheme} {Token}
Range: bytes={existingLength}-
```

> 服务端应支持 HTTP Range 以启用断点续传。

### 扩展包结构与 Tools 关系

| 角色 | 说明 |
| --- | --- |
| Extension 组件 | **消费侧**：下载、校验、解压、安装、回滚、登记 manifest |
| Tools / CI/CD | **生产侧**：编译扩展、生成元数据、计算 SHA256、压缩为 ZIP、上传服务端 |

推荐包名格式：`{Name}_{Version}.zip`

推荐 ZIP 内容：

```text
report-extension_1.0.0.zip
├── manifest.json          # 推荐放入包内
├── extension.dll          # 扩展主体程序集
├── extension.deps.json    # .NET 依赖描述
├── README.md
├── CHANGELOG.md
└── LICENSE.txt
```

### 自动更新策略优先级

```
单扩展设置 > 全局设置 > 默认值 (false)
```

### 平台兼容性速查

| 枚举值 | 数值 | 说明 |
| --- | --- | --- |
| `TargetPlatform.None` | 0 | 不匹配任何平台 |
| `TargetPlatform.Windows` | 1 | Windows |
| `TargetPlatform.Linux` | 2 | Linux |
| `TargetPlatform.MacOS` | 4 | macOS |
| `TargetPlatform.All` | 7 | 所有平台 (Windows \| Linux \| MacOS) |

---

## 相关资源

- [扩展管理示例](https://github.com/GeneralLibrary/GeneralUpdate-Samples/tree/main/src/Hub/Samples/ExtensionSample.cs)
- [GeneralUpdate 仓库](https://github.com/GeneralLibrary/GeneralUpdate)
- [打包指南](../guide/Packaging)
