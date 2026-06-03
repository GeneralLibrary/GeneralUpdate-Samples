---
sidebar_position: 12
---

# GeneralUpdate.Extension

## 组件概览

**GeneralUpdate.Extension** 是面向 .NET 应用的扩展管理组件，设计目标是让宿主程序具备类似 VS Code 的扩展生态能力：从远程服务查询扩展、下载扩展包、安装或更新到本地目录，并在这个过程中处理版本兼容、平台匹配、依赖扩展、SHA256 校验、失败回滚和事件通知。

它适合用于把主程序和可选能力拆开发布的场景，例如报表、认证、行业插件、客户定制模块、脚本执行器等。主程序只需要集成 `GeneralExtensionHost`，扩展包可以独立发布、独立更新，也可以通过 Tools 侧的打包流程生成标准 ZIP 包后交给 Extension 组件安装和管理。

**命名空间:** `GeneralUpdate.Extension`

**程序集:** `GeneralUpdate.Extension.dll`
**NuGet 包:** `GeneralUpdate.Extension`

```csharp
public interface IExtensionHost
{
    IExtensionCatalog ExtensionCatalog { get; }
    event EventHandler<ExtensionUpdateEventArgs>? ExtensionUpdateStatusChanged;

    Task<HttpResponseDTO<PagedResultDTO<ExtensionDTO>>> QueryExtensionsAsync(ExtensionQueryDTO query);
    Task<bool> DownloadExtensionAsync(string extensionId, string savePath);
    Task<bool> UpdateExtensionAsync(string extensionId);
    Task<bool> InstallExtensionAsync(string extensionPath, bool rollbackOnFailure = true);
    Task<Dictionary<string, bool>> UpdateExtensionsAsync(IEnumerable<string> extensionIds, CancellationToken cancellationToken = default);
    bool IsExtensionCompatible(ExtensionMetadata extension);
    void SetAutoUpdate(string extensionId, bool autoUpdate);
    void SetGlobalAutoUpdate(bool enabled);
}
```

---

## 阅读导航

| 主题 | 说明 |
| --- | --- |
| [快速开始](#快速开始) | 最小配置、查询扩展、更新扩展 |
| [核心流程](#核心流程) | 查询、下载、安装、更新、回滚、卸载分别做什么 |
| [扩展元数据与 manifest](#扩展元数据与-manifest) | `ExtensionMetadata`、服务端 DTO、本地 `manifest.json` |
| [扩展包结构与 Tools 打包关系](#扩展包结构与-tools-打包关系) | ZIP 命名、包内文件、发布侧与消费侧的分工 |
| [兼容性、平台与依赖](#兼容性平台与依赖) | Host 版本范围、`TargetPlatform`、依赖递归安装 |
| [事件通知与自动更新开关](#事件通知与自动更新开关) | 状态事件、全局/单扩展自动更新配置 |
| [服务器 API 契约](#服务器-api-契约) | `/Query` 与 `/Download/{extensionId}` 的真实调用方式 |
| [高级扩展点](#高级扩展点) | DI Builder、自定义 HttpClient、生命周期钩子 |
| [最佳实践](#最佳实践) | 生产环境接入建议 |

---

## 快速开始

### 安装

```bash
dotnet add package GeneralUpdate.Extension
```

### 初始化扩展宿主

```csharp
using GeneralUpdate.Extension.Common.DTOs;
using GeneralUpdate.Extension.Common.Enums;
using GeneralUpdate.Extension.Common.Models;
using GeneralUpdate.Extension.Core;

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
    Console.WriteLine($"{e.ExtensionId} {e.Status} {e.Progress}% {e.ErrorMessage}");
};
```

`ExtensionHostOptions` 当前可配置项如下：

| 属性 | 说明 |
| --- | --- |
| `ServerUrl` | 扩展服务根地址。客户端会调用 `{ServerUrl}/Query` 和 `{ServerUrl}/Download/{extensionId}` |
| `Scheme` | Authorization 认证方案，例如 `Bearer`。为空时不设置认证头 |
| `Token` | Authorization token。需要和 `Scheme` 同时非空才会生效 |
| `HostVersion` | 宿主应用版本，用于 `MinHostVersion` / `MaxHostVersion` 兼容性判断 |
| `ExtensionsDirectory` | 扩展包下载、安装和 `.backup` 目录所在位置 |
| `CatalogPath` | 可选，本地扩展目录扫描路径；为空时使用 `ExtensionsDirectory` |

### 查询和更新扩展

```csharp
var query = new ExtensionQueryDTO
{
    Platform = TargetPlatform.Windows,
    HostVersion = options.HostVersion,
    Status = true,
    PageNumber = 1,
    PageSize = 20
};

var response = await host.QueryExtensionsAsync(query);
if (response.Body != null)
{
    foreach (var extension in response.Body.Items)
    {
        Console.WriteLine($"{extension.DisplayName} v{extension.Version}, compatible: {extension.IsCompatible}");
    }
}
else
{
    Console.WriteLine(response.Message);
}

var success = await host.UpdateExtensionAsync("extension-id");
```

---

## 核心流程

### 1. 查询远程扩展

`QueryExtensionsAsync` 直接把 `ExtensionQueryDTO` 交给 `ExtensionHttpClient`，返回 `HttpResponseDTO<PagedResultDTO<ExtensionDTO>>`。当前响应数据在 `Body` 属性中，不是 `Data`。

```csharp
var response = await host.QueryExtensionsAsync(new ExtensionQueryDTO
{
    Name = "report",
    Publisher = "general",
    Category = "Tools",
    Platform = TargetPlatform.Windows | TargetPlatform.Linux,
    HostVersion = "1.2.0",
    IsPreRelease = false,
    PageNumber = 1,
    PageSize = 10
});

if (response.Body == null)
{
    Console.WriteLine($"Query failed: {response.Code} {response.Message}");
    return;
}

Console.WriteLine($"Total: {response.Body.TotalCount}");
```

### 2. 下载扩展包

`DownloadExtensionAsync(extensionId, savePath)` 会调用远程下载接口并写入 `savePath`。底层下载器支持：

- 已存在部分文件时使用 HTTP Range 续传；
- 下载过程中通过 `ExtensionUpdateStatusChanged` 报告 `Updating` 和进度；
- `DownloadExtensionWithResultAsync` 在底层提供更细的错误分类，例如网络错误、4xx、5xx、取消、I/O 错误。

```csharp
var downloaded = await host.DownloadExtensionAsync(
    extensionId: "report-extension",
    savePath: "./extensions/report-extension_1.0.0.zip");
```

### 3. 安装扩展包

`InstallExtensionAsync` 只接受 `.zip` 包。安装时会根据文件名推导目录名：`name_version.zip` 会安装到 `{ExtensionsDirectory}/name`。

```csharp
var installed = await host.InstallExtensionAsync(
    extensionPath: "./extensions/report-extension_1.0.0.zip",
    rollbackOnFailure: true);
```

安装过程：

1. 检查文件是否存在，并确认扩展包是 `.zip`。
2. 调用 `IExtensionLifecycleHooks.OnBeforeInstallAsync`，返回 `false` 时取消安装。
3. 如果本地已有同名扩展且开启回滚，复制旧目录到 `{ExtensionsDirectory}/.backup`。
4. 删除旧目录，创建目标目录。
5. 安全解压 ZIP。解压时会校验目标路径，跳过 Zip Slip 路径穿越条目。
6. 安装成功后删除备份，并调用 `OnAfterInstallAsync`。
7. 安装失败时尝试从备份目录恢复。

### 4. 一键更新扩展

`UpdateExtensionAsync(extensionId)` 是推荐入口。它会串起查询、兼容性检查、平台检查、依赖递归安装、下载、SHA256 校验、安装和 catalog 更新。

```csharp
var success = await host.UpdateExtensionAsync("report-extension");
if (!success)
{
    Console.WriteLine("Update failed. Read ExtensionUpdateStatusChanged for details.");
}
```

完整流程：

1. 触发 `Queued` 事件。
2. 用 `Id = extensionId` 查询服务端扩展信息。
3. 将 `ExtensionDTO` 映射为 `ExtensionMetadata`。
4. 检查宿主版本是否落在 `MinHostVersion` / `MaxHostVersion` 范围内。
5. 检查当前 OS 是否包含在 `SupportedPlatforms`。
6. 遇到未安装依赖时递归调用 `UpdateExtensionAsync(depId)`。
7. 下载 `{Name}_{Version}{Format}` 到 `ExtensionsDirectory`。
8. 如果 `Hash` 非空，计算下载文件 SHA256 并对比。
9. 调用 `InstallExtensionAsync(..., rollbackOnFailure: true)`。
10. 写入或更新本地 catalog 的 `manifest.json`。
11. 成功触发 `UpdateSuccessful`，失败触发 `UpdateFailed`。

### 5. 批量更新

`UpdateExtensionsAsync` 会按传入顺序逐个更新扩展，并返回每个扩展的成功/失败结果。当前实现是顺序处理；如需并发策略，应在业务层控制并发数量后分别调用 `UpdateExtensionAsync`。

```csharp
var result = await host.UpdateExtensionsAsync(new[]
{
    "report-extension",
    "auth-extension",
    "theme-extension"
}, cancellationToken);

foreach (var item in result)
{
    Console.WriteLine($"{item.Key}: {item.Value}");
}
```

### 6. 回滚

回滚由 `InstallExtensionAsync` 负责，核心是备份旧目录、失败时恢复旧目录。它适合覆盖安装或更新失败场景；首次安装失败时因为没有旧目录，通常没有可恢复内容。

备份目录位于：

```text
{ExtensionsDirectory}/.backup/{extensionName}_{yyyyMMddHHmmss}
```

### 7. 卸载

`IExtensionHost` 当前没有暴露 `UninstallExtensionAsync`。卸载能力在 `IExtensionCatalog.RemoveInstalledExtension(extensionId)` 中，调用后会移除内存记录，并尝试删除对应扩展目录。

```csharp
host.ExtensionCatalog.RemoveInstalledExtension("report-extension");
```

如果业务需要审批、停用、卸载前检查或卸载后清理，可以在应用层封装卸载服务，并复用 `IExtensionLifecycleHooks.OnBeforeUninstallAsync` / `OnAfterUninstallAsync` 的语义保持一致。

---

## 扩展元数据与 manifest

### ExtensionMetadata

`ExtensionMetadata` 是 Extension 组件本地安装、catalog 持久化和兼容性判断使用的核心模型。

| 属性 | 说明 |
| --- | --- |
| `Id` | 扩展唯一 ID。依赖、查询、更新、卸载都以它为关键标识 |
| `Name` | 扩展目录名和包名建议使用的稳定名称，例如 `report-extension` |
| `DisplayName` | 展示名称 |
| `Version` | 扩展版本。兼容性比较使用 .NET `Version.TryParse`，建议使用 `1.2.3` 或 `1.2.3.0` |
| `FileSize` | 扩展包大小，单位字节 |
| `UploadTime` | 上传时间 |
| `Status` | 是否启用 |
| `Description` | 描述 |
| `Format` | 包格式。当前安装实现要求 `.zip` |
| `Hash` | 可选 SHA256。非空时更新流程会校验下载文件 |
| `Publisher` | 发布者 |
| `License` | 许可证 |
| `Categories` | 逗号分隔分类 |
| `SupportedPlatforms` | `TargetPlatform` 位标志 |
| `MinHostVersion` | 最低宿主版本 |
| `MaxHostVersion` | 最高宿主版本 |
| `ReleaseDate` | 发布时间 |
| `Dependencies` | 逗号分隔的依赖扩展 ID |
| `IsPreRelease` | 是否预发布 |
| `DownloadUrl` | 下载地址元数据；当前默认下载调用仍使用 `{ServerUrl}/Download/{extensionId}` |
| `CustomProperties` | JSON 字符串形式的自定义属性 |

### 服务端 DTO 与本地 manifest

服务端查询返回 `ExtensionDTO`，其中 `Categories` 和 `Dependencies` 是 `List<string>`；客户端会把它们映射为本地 `ExtensionMetadata` 的逗号分隔字符串。

本地安装 catalog 不再是单个 `catalog.json`。当前实现会扫描 `CatalogPath` 下的子目录，并读取每个扩展目录中的：

```text
manifest.json
```

`AddOrUpdateInstalledExtension` 会把每个扩展写入独立目录：

```text
{CatalogPath}/{safe-extension-name}/manifest.json
```

写入时使用 `manifest.json.tmp -> manifest.json` 的方式尽量保证原子替换；`LoadInstalledExtensions` 会清理遗留的 `.tmp` 文件，并跳过包含 `.backup` 的目录。

示例 manifest：

```json
{
  "Id": "report-extension",
  "Name": "report-extension",
  "DisplayName": "Report Extension",
  "Version": "1.0.0",
  "Status": true,
  "Description": "Adds PDF and Excel reports.",
  "Format": ".zip",
  "Hash": "6f5902ac237024bdd0c176cb93063dc4...",
  "Publisher": "GeneralLibrary",
  "License": "MIT",
  "Categories": "Reports,Tools",
  "SupportedPlatforms": 7,
  "MinHostVersion": "1.0.0",
  "MaxHostVersion": "2.0.0",
  "Dependencies": "base-extension",
  "IsPreRelease": false
}
```

`SupportedPlatforms` 是 `[Flags]` 枚举，`All = Windows | Linux | MacOS = 7`。

---

## 扩展包结构与 Tools 打包关系

Extension 组件负责“消费”扩展包：下载、校验、解压、安装、回滚和登记 manifest。Tools 或 CI/CD 负责“生产”扩展包：编译扩展、生成元数据、计算 SHA256、压缩为 ZIP、上传到扩展服务。

推荐包名：

```text
{Name}_{Version}.zip
```

例如：

```text
report-extension_1.0.0.zip
```

推荐 ZIP 内容：

```text
report-extension_1.0.0.zip
├─ manifest.json          # 推荐放入包内，供业务和 catalog 复用
├─ extension.dll          # 扩展主体程序集
├─ extension.deps.json    # .NET 依赖描述
├─ README.md
├─ CHANGELOG.md
└─ LICENSE.txt
```

当前 `InstallExtensionAsync` 本身不强制读取包内 `manifest.json`，它主要负责安全解压和回滚；`UpdateExtensionAsync` 会使用服务端返回的 `ExtensionDTO` 更新本地 catalog。因此生产侧必须保证服务端元数据与 ZIP 包内容一致。

发布侧建议流程：

1. 编译扩展项目。
2. 准备 `manifest.json`，字段对齐 `ExtensionMetadata`。
3. 生成 `{Name}_{Version}.zip`。
4. 计算 ZIP 的 SHA256，写入服务端 `Hash`。
5. 上传 ZIP 和 `ExtensionDTO` 元数据。
6. 宿主应用通过 `QueryExtensionsAsync` 查询，通过 `UpdateExtensionAsync` 消费。

打包基础可参考 [Packaging](../guide/Packaging.md)。高级 Cookbook 的扩展发布流水线会在任务 [#54](https://github.com/GeneralLibrary/GeneralUpdate-Samples/issues/54) 中展开，建议在那里把 Tools 打包、清单生成、哈希计算、上传和宿主灰度消费串成完整流水线。

---

## 兼容性、平台与依赖

### 版本兼容性

`VersionCompatibilityChecker.IsCompatible` 使用 `HostVersion` 与扩展的 `MinHostVersion`、`MaxHostVersion` 比较：

- `HostVersion` 为空：视为不限制，返回兼容；
- `HostVersion` 无法被 `Version.TryParse` 解析：不兼容；
- `MinHostVersion` 非空且无法解析：不兼容；
- `MaxHostVersion` 非空且无法解析：不兼容；
- 宿主版本必须满足 `MinHostVersion <= HostVersion <= MaxHostVersion`。

| HostVersion | MinHostVersion | MaxHostVersion | 结果 |
| --- | --- | --- | --- |
| `1.5.0` | `1.0.0` | `2.0.0` | 兼容 |
| `1.5.0` | `1.6.0` | `2.0.0` | 不兼容 |
| `1.5.0` | `1.0.0` | `1.4.0` | 不兼容 |
| `1.5.0` | 空 | 空 | 兼容 |

```csharp
var extension = host.ExtensionCatalog.GetInstalledExtensionById("report-extension");
if (extension != null && host.IsExtensionCompatible(extension))
{
    Console.WriteLine("Compatible");
}
```

### 平台匹配

```csharp
[Flags]
public enum TargetPlatform
{
    None = 0,
    Windows = 1,
    Linux = 2,
    MacOS = 4,
    All = Windows | Linux | MacOS
}
```

`PlatformMatcher` 通过 `RuntimeInformation` 自动识别当前系统，并用位运算判断扩展是否支持当前平台。

```csharp
var metadata = new ExtensionMetadata
{
    Id = "report-extension",
    Name = "report-extension",
    SupportedPlatforms = TargetPlatform.Windows | TargetPlatform.Linux
};
```

### 依赖处理

`ExtensionMetadata.Dependencies` 是逗号分隔的扩展 ID。`DependencyList` 会把它解析为列表。`UpdateExtensionAsync` 发现未安装依赖时，会先递归更新依赖，再安装当前扩展。

```csharp
var metadata = new ExtensionMetadata
{
    Id = "report-extension",
    Dependencies = "base-extension,chart-extension"
};
```

`DependencyResolver` 还提供依赖解析能力，可以基于本地 catalog 识别缺失依赖并检测循环依赖。需要注意：`UpdateExtensionAsync` 当前依赖安装是根据服务端返回的当前扩展元数据逐项递归处理；生产侧要确保依赖扩展也能通过同一个扩展服务查询和下载。

---

## 事件通知与自动更新开关

### ExtensionUpdateStatusChanged

扩展更新事件提供单个扩展的状态变化通知：

| 字段 | 说明 |
| --- | --- |
| `ExtensionId` | 扩展 ID |
| `ExtensionName` | 扩展名称，部分阶段可能为空 |
| `Status` | `Queued`、`Updating`、`UpdateSuccessful`、`UpdateFailed` |
| `Progress` | 0-100，下载中会更新 |
| `ErrorMessage` | 失败原因 |

```csharp
host.ExtensionUpdateStatusChanged += (sender, e) =>
{
    switch (e.Status)
    {
        case ExtensionUpdateStatus.Queued:
            Console.WriteLine($"{e.ExtensionId} queued");
            break;
        case ExtensionUpdateStatus.Updating:
            Console.WriteLine($"{e.ExtensionId} downloading {e.Progress}%");
            break;
        case ExtensionUpdateStatus.UpdateSuccessful:
            Console.WriteLine($"{e.ExtensionName ?? e.ExtensionId} updated");
            break;
        case ExtensionUpdateStatus.UpdateFailed:
            Console.WriteLine($"{e.ExtensionId} failed: {e.ErrorMessage}");
            break;
    }
};
```

### 自动更新开关

`SetGlobalAutoUpdate` 设置全局默认值，`SetAutoUpdate` 设置单个扩展的覆盖值。`IExtensionHost` 暴露了设置方法；`GeneralExtensionHost` 还提供 `IsAutoUpdateEnabled(extensionId)` 用于读取当前结果。

```csharp
var concreteHost = new GeneralExtensionHost(options);

concreteHost.SetGlobalAutoUpdate(true);
concreteHost.SetAutoUpdate("large-extension", false);

var enabled = concreteHost.IsAutoUpdateEnabled("large-extension");
```

这些开关只保存于当前 `GeneralExtensionHost` 实例内存中，组件不会自动启动后台轮询。应用层应自行决定何时扫描需要更新的扩展，并根据开关调用 `UpdateExtensionAsync`。

---

## 服务器 API 契约

当前 `ExtensionHttpClient` 使用两个端点。

### 查询

```http
GET {ServerUrl}/Query
Content-Type: application/json
Authorization: {Scheme} {Token}

ExtensionQueryDTO JSON body
```

这里是 **GET + JSON Body**。这不是常见 HTTP 风格，但当前客户端源码明确按这个服务端契约实现。如果经过代理、网关或 API 平台时出现兼容性问题，需要服务端和客户端一起改为 POST 或 query string。

响应：

```csharp
HttpResponseDTO<PagedResultDTO<ExtensionDTO>>
```

### 下载

```http
GET {ServerUrl}/Download/{extensionId}
Authorization: {Scheme} {Token}
Range: bytes={existingLength}-
```

服务端应支持普通文件流下载，最好同时支持 HTTP Range，便于客户端断点续传。客户端遇到 `416 RequestedRangeNotSatisfiable` 会视为文件已经完整下载。

---

## 高级扩展点

### 使用 ExtensionHostBuilder 和 DI

`ExtensionHostBuilder` 会注册默认服务，同时允许业务替换任意服务：

```csharp
var host = new ExtensionHostBuilder()
    .WithOptions(options)
    .ConfigureServices(services =>
    {
        services.AddSingleton<IExtensionLifecycleHooks, MyLifecycleHooks>();
        services.AddSingleton<IExtensionHttpClient>(sp =>
            new ExtensionHttpClient(options.ServerUrl, options.Scheme, options.Token, sharedHttpClient));
    })
    .Build();
```

默认注册包括：

- `IExtensionHttpClient -> ExtensionHttpClient`
- `IVersionCompatibilityChecker -> VersionCompatibilityChecker`
- `IDownloadQueueManager -> DownloadQueueManager`
- `IPlatformMatcher -> PlatformMatcher`
- `IPlatformServices -> RuntimePlatformServices`
- `IExtensionMetadataMapper -> DefaultExtensionMetadataMapper`
- `IExtensionCatalog -> ExtensionCatalog`
- `IDependencyResolver -> DependencyResolver`
- `IExtensionLifecycleHooks -> DefaultExtensionLifecycleHooks`
- `IExtensionHost -> GeneralExtensionHost`

### 生命周期钩子

`IExtensionLifecycleHooks` 用于在安装、激活、停用、卸载前后接入业务逻辑。当前 `GeneralExtensionHost` 已在安装前后调用 `OnBeforeInstallAsync` 和 `OnAfterInstallAsync`；激活、停用、卸载钩子可供业务层封装对应流程时复用。

```csharp
public sealed class MyLifecycleHooks : DefaultExtensionLifecycleHooks
{
    public override Task<bool> OnBeforeInstallAsync(
        ExtensionMetadata extension,
        string? packagePath,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Installing {packagePath}");
        return Task.FromResult(true);
    }
}
```

### 下载队列

`DownloadQueueManager` 提供独立队列类型，默认最大并发数为 3，支持 `Enqueue`、`GetTask`、`CancelTask`、`GetActiveTasks` 和 `DownloadStatusChanged` 事件。当前队列管理器本身只负责队列状态和并发槽位，真实下载逻辑由宿主更新流程中的 `ExtensionHttpClient` 完成；如果要做应用级并发下载，可以在业务层组合队列、HTTP 客户端和安装流程。

---

## 最佳实践

1. 生产环境始终使用 `.zip` 扩展包，并采用 `{Name}_{Version}.zip` 命名。
2. 服务端 `Hash` 建议填写 ZIP 的 SHA256，让 `UpdateExtensionAsync` 自动校验完整性。
3. `HostVersion`、`MinHostVersion`、`MaxHostVersion` 使用标准可解析版本号。
4. 本地 catalog 使用每个扩展独立 `manifest.json`，不要再按旧文档维护单个 `catalog.json`。
5. `SupportedPlatforms` 按实际 OS 能力填写，不要为了省事全部写 `All`。
6. 依赖扩展必须能被同一个服务端通过 ID 查询和下载，否则递归安装会失败。
7. 大型扩展建议服务端支持 HTTP Range，并在 UI 中展示 `ExtensionUpdateStatusChanged` 的进度。
8. 自动更新开关只是策略状态，不是后台任务调度器；扫描、定时、灰度和审批应由应用层控制。
