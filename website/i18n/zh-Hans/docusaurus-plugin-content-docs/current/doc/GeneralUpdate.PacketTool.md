---
sidebar_position: 11
---

# GeneralUpdate.Tools

GeneralUpdate.Tools 是面向 GeneralUpdate 发布流程的桌面工具箱。它把补丁包、扩展包、OSS 版本清单、极简配置文件和本地更新仿真集中到一个 Avalonia 应用里，适合开发、测试、发布前验包和给运营/发布同学生成标准产物。

仓库地址：[https://github.com/GeneralLibrary/GeneralUpdate.Tools](https://github.com/GeneralLibrary/GeneralUpdate.Tools)

## 功能大纲

| 知识点 | 入口 | 产物 | 主要消费者 |
| --- | --- | --- | --- |
| [启动要求](#启动要求) | `GeneralUpdate.Tools` | 桌面工具 | 发布/测试人员 |
| [Patch Package](#patch-package-补丁包) | Patch | `{packageName}.zip` | Server、OSS、Core/Upgrade |
| [Extension Package](#extension-package-扩展包) | Extension | `{name}_{version}.zip`、`manifest.json` | GeneralUpdate.Extension |
| [OSS Config](#oss-config-oss-版本清单) | OSS | `oss_config.json` | OSS 更新客户端、对象存储发布流程 |
| [Config](#config-generalupdatemanifestjson) | Config | `generalupdate.manifest.json`、`sample_output` | Client/Upgrade 启动配置 |
| [Simulation](#simulation-本地更新仿真) | Simulate | 本地服务、测试应用、`simulation_report.md` | 发布前验证 |
| [Hash](#hash-sha256-校验) | OSS / 内部服务 | SHA256 小写十六进制 | 更新包完整性校验 |

## 启动要求

当前工具基于 Avalonia 12 和 .NET 10 构建。开发环境运行时建议安装 .NET 10 SDK，然后在 Tools 仓库执行：

```powershell
dotnet run --project src\GeneralUpdate.Tools.csproj
```

如果只使用已经发布的桌面程序，直接启动可执行文件即可。Simulation 模块会调用 `dotnet publish` 构建内置测试应用，因此本地必须能执行 `dotnet --version`，当前源码校验要求 .NET 10 或更新的预览 SDK。

## 产物如何进入 GeneralUpdate 流程

| Tools 产物 | 文件内容 | 放到哪里 | 后续如何使用 |
| --- | --- | --- | --- |
| Patch ZIP | 差分 `.patch` 文件、新增文件、删除清单 | 更新服务器静态目录、OSS/CDN、发布制品库 | Server/OSS 返回下载地址，Core 下载后由 Upgrade 进程应用 |
| `generalupdate.delete.json` | 新版本已删除文件的 hash 列表 | Patch ZIP 内 | Core `DiffPipeline.DirtyAsync` 根据 hash 删除旧文件 |
| `manifest.json` | 扩展元数据 | Extension ZIP 内 | Extension Host 安装/更新扩展时读取 |
| `oss_config.json` | 包名、版本、URL、SHA256 等数组 | OSS 配置文件位置或发布配置仓库 | OSS 客户端用它发现和校验版本 |
| `generalupdate.manifest.json` | 主程序名、升级程序名、版本、ProductId、UpdatePath | Client 输出目录根目录 | 让客户端少写初始化代码，配上敏感信息即可启动 |
| `simulation_report.md` | 仿真参数、结果、日志时间线 | 被测 App 目录 | 发布前审查和失败复盘 |

:::info 删除清单名称
早期资料或任务描述里可能写作 `delete_files.json`。当前最新 Core 管线实际生成并消费的文件名是 `generalupdate.delete.json`，两者表达的是同一个“删除清单”概念，落地时请以当前源码文件名为准。
:::

## Patch Package 补丁包

Patch 模块用于对比旧版本目录和新版本目录，只输出发生变化的内容。当前实现调用 `GeneralUpdate.Core.Pipeline.DiffPipeline.CleanAsync(oldDir, newDir, patchDir)`，再把临时补丁目录压缩成 ZIP。

### 输入

| 字段 | 说明 |
| --- | --- |
| Old Directory | 当前用户正在运行的旧版本发布目录 |
| New Directory | 准备发布的新版本发布目录 |
| Package Name | 输出 ZIP 名称；为空时使用 `patch_yyyyMMddHHmmss` |
| Version | 语义化版本号，必须符合 `MAJOR.MINOR.PATCH`，可带 prerelease/build |
| Output Directory | 输出目录；为空时输出到桌面 |

### 输出规则

1. 工具创建 `gupatch_yyyyMMddHHmmss` 临时目录。
2. changed files 生成 `.patch` 二进制差分文件。
3. new files 直接复制到补丁目录并保持目录结构。
4. deleted files 写入 `generalupdate.delete.json`。
5. 临时目录被压缩成 `{PackageName}.zip`，随后删除临时目录。

Core 的差分管线支持并行处理。默认 `MaxDegreeOfParallelism = 2`，适合普通开发机；大型项目或 CI 服务器可以在代码侧通过 `DiffPipelineOptions` 调高并行度，注意不要让磁盘 I/O 成为瓶颈。Tools 当前 Patch 页面使用默认管线，强调稳定的一键打包。

## Extension Package 扩展包

Extension 模块把一个扩展目录压缩为标准 ZIP，并向 ZIP 根目录写入 `manifest.json`。GeneralUpdate.Core 默认集成 Differential，但扩展能力由 [GeneralUpdate.Extension](./GeneralUpdate.Extension.md) 消费；扩展包本身不需要开发者手写 ZIP 结构。

### 输入与校验

| 字段 | 说明 |
| --- | --- |
| Name | 扩展名，参与输出文件名 |
| Version | 语义化版本号 |
| Description / Publisher / License | 扩展元数据 |
| Extension Directory | 要打包的扩展目录，必须存在 |
| Export Directory | 输出目录；为空时输出到桌面 |
| Custom Properties | 写入 `manifest.json.customProperties` 的键值对 |

### 输出

输出文件名为 `{Sanitize(Name)}_{Version}.zip`。ZIP 内会保留扩展目录内容，并额外包含：

```json
{
  "name": "MyExtension",
  "version": "1.0.0",
  "description": "...",
  "publisher": "...",
  "license": "MIT",
  "dependencies": "",
  "minHostVersion": "",
  "maxHostVersion": "",
  "isPreRelease": false,
  "customProperties": {
    "key": "value"
  }
}
```

这些字段会被 Extension Host 用于安装、兼容性判断、依赖处理和本地目录清单维护。扩展运行时细节见 [GeneralUpdate.Extension](./GeneralUpdate.Extension.md)。

## OSS Config OSS 版本清单

OSS 模块用于生成面向对象存储/CDN 的版本清单。它不上传文件，而是帮你整理“某个包的版本、下载地址和 SHA256”。

| 字段 | 说明 |
| --- | --- |
| PacketName | 更新包名称 |
| Version | 语义化版本号 |
| Url | 客户端可下载补丁包的地址 |
| SHA256 | 通过 ComputeHash 选择本地文件计算，或由发布系统填入 |
| ReleaseDate | 模型字段保留，当前页面主要展示包名、hash、版本 |

点击 Add To List 后，条目加入列表；点击 Export 会保存为 `oss_config.json`，内容是一个数组：

```json
[
  {
    "PacketName": "Packet",
    "Hash": "f2c7...",
    "Version": "1.0.0",
    "Url": "https://cdn.example.com/patch_1.0.0.zip",
    "ReleaseDate": ""
  }
]
```

OSS 客户端拿到 URL 下载补丁包，再用 Hash 做完整性校验。OSS 更新链路见 [GeneralClient.OSS](./GeneralClient.OSS.md)。

## Config: `generalupdate.manifest.json`

Config 模块用于生成 `generalupdate.manifest.json`。它的核心价值是节省开发者时间：主程序名、升级程序名、版本、ProductId、UpdatePath 这些固定信息写进文件后，应用启动时只需要再配置服务地址、AppSecretKey 等敏感信息即可进入更新流程。

### 工作流

1. 选择 Client `.csproj`。
2. 可选选择 Upgrade `.csproj`。
3. 点击 Analyze，工具从 `.csproj` 读取 AssemblyName、TargetFramework，并补全 `MainAppName`、`UpdateAppName`。
4. 填写 `ClientVersion`、`UpgradeClientVersion`、`AppType`、`ProductId`、`UpdatePath`。
5. 点击 Generate，在 Tools 运行目录生成 `generalupdate.manifest.json`。

输出示例：

```json
{
  "mainAppName": "ClientSample.exe",
  "clientVersion": "1.0.0",
  "appType": "Client",
  "updateAppName": "UpgradeSample.exe",
  "upgradeClientVersion": "1.0.0",
  "productId": "2d974e2a-31e6-4887-9bb1-b4689e98c77a",
  "updatePath": "update/"
}
```

Generate Sample 会额外执行 `dotnet publish`，把 Client 发布到 `sample_output` 根目录，把 Upgrade 发布到 `UpdatePath` 子目录，并写入同一份 manifest，适合快速搭建可运行样例。

## Simulation 本地更新仿真

Simulation 模块把“补丁包是否能跑通”变成本地闭环测试。它会发布内置 `ClientSample` 和 `UpgradeSample`，启动本地更新服务，运行 Client，触发 Upgrade，然后输出报告。

### 输入

| 字段 | 说明 |
| --- | --- |
| Old App Directory | 用作被测应用目录，必须存在 |
| Patch File | Patch 模块或 CI 生成的 ZIP |
| CurrentVersion / TargetVersion | 语义化版本 |
| Platform | 当前页面提供 Windows / Linux |
| AppType | ClientApp / UpgradeApp |
| AppSecretKey / ProductId / UpdatePath | 传给样例 Client 和本地服务的更新配置 |
| ServerPort | 默认 5000，服务启动后会记录实际端口 |

### 仿真过程

1. 校验目录、补丁包、版本号和 .NET SDK。
2. 发布 `test_app\Client\ClientSample.csproj` 到 App Directory。
3. 发布 `test_app\Upgrade\UpgradeSample.csproj` 到 `UpdatePath`。
4. 在 App Directory 生成 `generalupdate.manifest.json`。
5. 把补丁包复制到 `.server`，计算 SHA256。
6. 启动本地服务：`POST /Upgrade/Verification`、`POST /Upgrade/Report`、`GET /patch/{filename}`。
7. 运行 `ClientSample.exe --server-url ... --app-secret ... --client-version ...`。
8. 停止服务，检查更新结果，生成 `simulation_report.md`。

报告包含配置表、PASS/FAIL、Notes 和完整时间线，适合贴到发布记录或 CI artifact。

## Hash SHA256 校验

OSS 模块的 ComputeHash 会对选择的本地文件计算 SHA256，并输出小写十六进制字符串。Patch ZIP 上传到 CDN/OSS 后，应把同一个 Hash 写入版本清单或服务端返回值。客户端下载后校验 Hash，可以发现传输损坏、上传错包或缓存污染。

## 推荐发布顺序

1. 用 CI 或手工发布得到 old/new 两个目录。
2. 在 Patch 模块选择 old/new，生成 patch ZIP。
3. 在 OSS 模块对 ZIP 计算 SHA256，填写下载 URL，导出 `oss_config.json`；如果使用自建 Server，则把同样的 URL、Version、Hash 写入服务端版本源。
4. 用 Config 模块生成或检查 `generalupdate.manifest.json`。
5. 用 Simulation 选择旧版本目录和补丁 ZIP，确认 PASS。
6. 发布补丁 ZIP、OSS 清单或服务端版本配置。

## 常见问题

| 现象 | 原因与处理 |
| --- | --- |
| Version invalid | Tools 使用 SemVer 校验，使用 `1.0.0`、`1.2.0-beta.1` 这类格式，不要使用 `1.0` |
| Patch 输出为空 | old/new 没有可识别差异，或目录选反；确认 old 是用户已安装版本，new 是目标版本 |
| 找不到 `delete_files.json` | 当前文件名是 `generalupdate.delete.json`，在 Patch ZIP 根目录或对应目录中查找 |
| Simulation 提示 .NET SDK | 安装 .NET 10 SDK，并确认命令行可执行 `dotnet --version` |
| Simulation 端口占用 | 修改 `ServerPort` 或释放本地 5000 端口 |
| Hash 校验失败 | 重新对最终上传的 ZIP 计算 Hash，避免本地打包后又被重新压缩或替换 |
| Upgrade 没启动 | 检查 manifest 中 `updateAppName`、`updatePath` 是否与发布目录一致 |

## 关联文档

- [GeneralUpdate.Core](./GeneralUpdate.Core.md)：Client/Upgrade 更新主流程、事件、静默更新和 manifest。
- [GeneralUpdate.Differential](./GeneralUpdate.Differential.md)：差分算法、Clean/Dirty 模式和并行处理。
- [GeneralClient.OSS](./GeneralClient.OSS.md)：OSS 更新链路。
- [GeneralUpdate.Extension](./GeneralUpdate.Extension.md)：扩展包安装、更新、兼容性和依赖。
- [GeneralUpdate.Bowl](./GeneralUpdate.Bowl.md)：可靠下载与断点续传。
