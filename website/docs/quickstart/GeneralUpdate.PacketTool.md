---
sidebar_position: 11
---

# GeneralUpdate.Tools

## 这是什么

GeneralUpdate.Tools 是一个基于 Avalonia 12 开发的跨平台桌面工具（Windows / Linux / macOS），用于在软件发布流程中生成和管理补丁包、扩展包、版本清单以及执行本地更新仿真。它不替代你的 CI/CD 系统，而是把“打包、校验、验证”这些重复劳动收敛到一个可视化工具中。

仓库地址：[https://github.com/GeneralLibrary/GeneralUpdate.Tools](https://github.com/GeneralLibrary/GeneralUpdate.Tools)

## 下载与运行

### 方式一：从源码运行（推荐开发者）

当前工具基于 .NET 10 构建。确保本机安装了 [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)：

```powershell
git clone https://github.com/GeneralLibrary/GeneralUpdate.Tools.git
cd GeneralUpdate.Tools\src
dotnet run --project GeneralUpdate.Tools.csproj
```

### 方式二：下载预编译版本

前往 [GeneralUpdate.Tools Releases](https://github.com/GeneralLibrary/GeneralUpdate.Tools/releases) 下载对应平台的可执行文件，直接运行即可。

> Simulation 模块内部会调用 `dotnet publish` 构建测试应用，因此使用仿真功能时必须安装 .NET SDK，仅运行 Patch / Extension / OSS / Config 模块则不需要。

## 六个模块速览

| 模块 | 你提供 | 工具产出 | 下游消费者 |
|------|--------|----------|------------|
| **Patch** | 旧版本目录 + 新版本目录 | `{name}.zip`（含 `.patch` 文件、新文件、`generalupdate.delete.json`） | Server、OSS/CDN、Core Upgrade 进程 |
| **Extension** | 扩展源目录 + 元数据 | `{name}_{version}.zip`（含 `manifest.json`） | `GeneralUpdate.Extension` 组件 |
| **OSS Config** | 包名、版本、下载 URL、本地 ZIP（计算 Hash） | `oss_config.json`（版本数组） | OSS 客户端、对象存储发布流程 |
| **Config** | Client/Upgrade 的 `.csproj` | `generalupdate.manifest.json` + `sample_output/` 发布目录 | Client/Upgrade 启动引导 |
| **Simulation** | 旧版本目录 + 补丁 ZIP | 本地更新服务 + `simulation_report.md` | 发布前质量把关 |
| **Hash** | 本地文件（ZIP） | SHA256 小写十六进制字符串 | 完整性校验、服务端版本记录 |

---

## Patch：生成补丁包

### 解决什么问题

当你发布了一个新版本，用户不想下载整个安装包。Patch 模块比较旧版本目录和新版本目录，只输出变更内容：修改过的文件生成二进制差分 `.patch`，新增文件直接复制，删除的文件写入清单。差分包通常远小于完整发布目录。

### 输入

| 字段 | 必填 | 说明 |
|------|------|------|
| Old Directory | ✅ | 用户当前安装的旧版本发布目录，例如 `publish\v1.0.0` |
| New Directory | ✅ | 准备发布的新版本目录，例如 `publish\v2.0.0` |
| Package Name | ❌ | 输出 ZIP 文件名（不含 `.zip`），为空时自动生成 `patch_yyyyMMddHHmmss` |
| Version | ✅ | 目标版本号，格式 `MAJOR.MINOR.PATCH`（如 `2.0.0`）或 `MAJOR.MINOR.PATCH-prerelease+build` |
| Output Directory | ❌ | ZIP 输出目录，为空时输出到桌面 |

### 工具内部做了什么

1. 校验 old/new 目录存在、version 格式合法。
2. 创建临时目录 `gupatch_yyyyMMddHHmmss`。
3. 递归比较 old/new：**修改文件** → 生成 `.patch` 二进制差分；**新增文件** → 直接复制；**删除文件** → 记录 hash 到 `generalupdate.delete.json`。
4. 将临时目录压缩为 `{PackageName}.zip`。
5. 删除临时目录，ZIP 保留在 Output Directory。

底层调用的是 `GeneralUpdate.Core.Pipeline.DiffPipeline.CleanAsync(oldDir, newDir, patchDir)`，和你的 CI 脚本走的是同一条代码路径。

### 输出

```
{OutputDirectory}/{PackageName}.zip
  ├── changed.dll.patch           ← 二进制差分
  ├── new_file.dll                ← 新增文件（保持目录结构）
  └── generalupdate.delete.json    ← 删除清单
```

### 下游如何使用

- 把 ZIP 放到 Server 的 `wwwroot/packages/` 目录，更新 `versions.json` 中的 `Url` 和 `Hash`
- 或上传到 OSS/CDN，把下载链接写入 `oss_config.json`
- Core Upgrade 进程下载 ZIP 后，`DiffPipeline.DirtyAsync` 根据 `.patch` 和删除清单完成更新

---

## Extension：打包扩展

### 解决什么问题

如果你使用 `GeneralUpdate.Extension` 组件管理插件/扩展，每次发布新扩展都需要手动构建标准格式的 ZIP 包并生成 `manifest.json`。Extension 模块把你从手工压缩和手写 JSON 中解放出来。

### 输入

| 字段 | 必填 | 说明 |
|------|------|------|
| Name | ✅ | 扩展唯一标识，建议小写+连字符，如 `my-data-exporter` |
| Version | ✅ | 语义化版本，如 `1.0.0` |
| Description | ❌ | 扩展功能描述 |
| Publisher | ❌ | 发布者名称 |
| License | ❌ | 许可证标识，如 `MIT` |
| Extension Directory | ✅ | 扩展源文件目录，点击 Pick 选择文件夹 |
| Export Directory | ❌ | 输出目录，为空时输出到桌面 |
| Custom Properties | ❌ | 自定义键值对，写入 `manifest.json.customProperties` |

### 工具内部做了什么

1. 校验目录存在、version 格式合法。
2. 将 Extension Directory 中所有文件打包为 ZIP。
3. 在 ZIP 根目录写入 `manifest.json`。

### 输出

```
{ExportDirectory}/{Name}_{Version}.zip
  ├── manifest.json               ← 扩展元数据
  ├── bin/
  │   └── MyExtension.dll
  ├── resources/
  └── ...
```

`manifest.json` 示例：

```json
{
  "name": "my-data-exporter",
  "version": "1.0.0",
  "description": "Export data to CSV/Excel/JSON",
  "publisher": "MyCompany",
  "license": "MIT",
  "dependencies": "",
  "minHostVersion": "",
  "maxHostVersion": "",
  "isPreRelease": false,
  "platform": { "displayName": "Windows", "value": 1 },
  "format": ".zip",
  "releaseDate": "2026-06-01T00:00:00",
  "customProperties": { "maxConnections": "100" }
}
```

### 下游如何使用

- Extension Host 调用 `ExtensionManager.QueryRemoteExtensionsAsync(...)` 获取扩展列表
- 安装时下载 ZIP，读取 `manifest.json` 进行兼容性检查和依赖解析
- 详见 [GeneralUpdate.Extension](../doc/GeneralUpdate.Extension.md)

---

## OSS Config：生成 OSS 版本清单

### 解决什么问题

如果你使用 OSS 模式更新（静态文件服务器），你需要维护一个 `versions.json` 或 `oss_config.json`，让客户端知道有哪些版本可以下载。OSS Config 模块帮你整理版本信息并计算 SHA256。

### 输入

| 字段 | 必填 | 说明 |
|------|------|------|
| PacketName | ✅ | 更新包名称 |
| Version | ✅ | 语义化版本号 |
| Url | ✅ | 客户端可下载补丁包的完整地址 |
| SHA256 | 推荐 | 选择本地 ZIP 文件后点击 ComputeHash 自动计算 |

### 操作流程

1. 填写 PacketName、Version、Url。
2. 点击 **ComputeHash**，选择本地补丁 ZIP 文件，自动填入 SHA256。
3. 点击 **Add To List**，加入列表。
4. 重复以上步骤添加所有版本。
5. 点击 **Export**，生成 `oss_config.json`。

### 输出

```json
[
  {
    "PacketName": "client_1.0.0_to_2.0.0",
    "Hash": "f2c7a8b3...",
    "Version": "2.0.0",
    "Url": "https://cdn.example.com/client_1.0.0_to_2.0.0.zip",
    "ReleaseDate": ""
  }
]
```

### 下游如何使用

- 将 `oss_config.json` 上传到 OSS bucket 或静态文件服务器
- OSS 客户端读取此文件发现可用版本，下载后校验 Hash
- 详见 [GeneralUpdate.Core](../doc/GeneralUpdate.Core.md) OSS 更新策略

---

## Config：生成启动清单

### 解决什么问题

Client 和 Upgrade 启动时需要知道主程序名、升级程序名、版本号、ProductId、UpdatePath 等信息。Config 模块从 `.csproj` 中自动提取 AssemblyName 和 TargetFramework，帮你生成 `generalupdate.manifest.json`，省去手写和手误。

### 输入

| 字段 | 必填 | 说明 |
|------|------|------|
| Client .csproj | ✅ | 主程序的 `.csproj` 文件路径 |
| Upgrade .csproj | ❌ | 升级程序的 `.csproj` 路径 |
| ClientVersion | ✅ | 客户端当前版本，如 `1.0.0` |
| UpgradeClientVersion | ✅ | 升级程序版本，如 `1.0.0` |
| AppType | ✅ | Client / Upgrade / OssClient / OssUpgrade |
| ProductId | ✅ | 产品标识 GUID |
| UpdatePath | ✅ | 升级程序相对主程序的子目录，如 `update/` |

### 操作流程

1. 点击 Browse 选择 Client `.csproj`（必填）和 Upgrade `.csproj`（可选）。
2. 点击 **Analyze**，工具自动读取 AssemblyName 填入 `MainAppName`、`UpdateAppName`。
3. 手动填写 Version、AppType、ProductId、UpdatePath。
4. 点击 **Generate**，在工具运行目录生成 `generalupdate.manifest.json`。
5. 点击 **Generate Sample**，额外执行 `dotnet publish` 并输出可运行的发布目录。

### 输出

`generalupdate.manifest.json`：

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

Generate Sample 额外输出：

```
{工具运行目录}/sample_output/
  ├── ClientSample.exe             ← Client dotnet publish 输出
  ├── generalupdate.manifest.json
  └── update/
      └── UpgradeSample.exe        ← Upgrade dotnet publish 输出
```

### 下游如何使用

- Client 启动时读取 `generalupdate.manifest.json`，配合服务端地址和 AppSecretKey 即可进入更新流程
- 不需要手写 `Configinfo`，只需用 `GeneralUpdateBootstrap` 读取 manifest

---

## Simulation：本地更新仿真

### 解决什么问题

你刚用 Patch 模块生成了补丁包，但不确定这个包在实际更新流程中能不能跑通。Simulation 模块在你本机启动一个临时更新服务，构建 Client/Upgrade 样例，模拟一次完整的“发现版本 → 下载补丁 → 应用更新”闭环，最后输出 PASS/FAIL 报告。

### 前置条件

- 已安装 .NET 10 SDK（或更高版本）
- `dotnet --version` 可正常执行
- 本地 5000 端口未被占用（可配置）

### 输入

| 字段 | 必填 | 说明 |
|------|------|------|
| Old App Directory | ✅ | 模拟用户当前安装的旧版本目录 |
| Patch File | ✅ | Patch 模块生成的补丁 ZIP |
| CurrentVersion | ✅ | 旧版本号（Simulation 内部传给 Client，用于版本校验） |
| TargetVersion | ✅ | 新版本号（Simulation 内部赋值到补丁 ZIP，写入服务端版本源） |
| AppType | ✅ | ClientApp = 1 / UpgradeApp = 2 |
| Platform | ✅ | Windows = 1 / Linux = 2 |
| AppSecretKey | ✅ | 客户端验证密钥 |
| ProductId | ✅ | 产品标识 GUID |
| UpdatePath | ✅ | 升级程序子目录名称，如 `update/` |
| ServerPort | ❌ | 本地服务端口，默认 5000 |

### 仿真过程

1. **校验**：旧目录存在、Patch ZIP 存在、version 格式合法、.NET SDK 可用。
2. **发布 Client**：`dotnet publish test_app/Client/ClientSample.csproj` 到 App Directory。
3. **发布 Upgrade**：`dotnet publish test_app/Upgrade/UpgradeSample.csproj` 到 `UpdatePath` 子目录。
4. **生成 manifest**：在 App Directory 写入 `generalupdate.manifest.json`。
5. **准备补丁**：把 Patch ZIP 复制到内部 `.server` 目录，计算 SHA256，写入服务端版本源。
6. **启动服务**：本地服务监听 `http://localhost:{ServerPort}`，提供 `POST /Upgrade/Verification`、`POST /Upgrade/Report`、`GET /patch/{filename}`。
7. **运行 Client**：启动 `ClientSample.exe`，传入 `--server-url http://localhost:5000 --app-secret ... --client-version 1.0.0`。
8. **停止服务**：Client 完成更新或超时后停止服务。
9. **生成报告**：检查目录文件变化，输出 `simulation_report.md`。

### 输出

```
{AppDirectory}/simulation_report.md    ← 仿真报告（PASS/FAIL + 时间线）
```

报告包含配置表、PASS/FAIL 结论、Notes 和完整时间线，可以直接贴到发布记录或 CI artifact。

---

## Hash：SHA256 校验

### 解决什么问题

补丁包上传到 CDN/OSS 后，必须确保客户端下载到的文件和原始包一致。Hash 功能对本地文件计算 SHA256，输出小写十六进制字符串，你应该把这个值写入版本清单或服务端返回数据。

### 使用方式

在 **OSS Config** 模块中，点击 **ComputeHash**，选择本地 ZIP 文件，自动填入 SHA256 字段。你也可以单独使用这个功能来校验任何文件。

---

## 推荐发布工作流

这个顺序把六个模块串联成一个完整的发布流水线：

```
┌─────────┐    ┌──────────┐    ┌──────────┐    ┌──────────┐    ┌──────────┐
│ 发布新旧  │ -> │ Patch    │ -> │ OSS/Hash │ -> │ Config   │ -> │ Simulation│
│ 版本目录  │    │ 生成补丁  │    │ 算 Hash   │    │ 生成清单  │    │ 本地验证  │
└─────────┘    └──────────┘    └──────────┘    └──────────┘    └──────────┘
```

1. **构建发布目录**：在 CI 或本地 `dotnet publish` 得到 old/new 两个目录。
2. **Patch**：选择 old/new，生成补丁 ZIP。
3. **OSS Config**：对补丁 ZIP 计算 SHA256，填写下载 URL，导出 `oss_config.json`。（如果使用自建 Server，将相同信息写入 `versions.json`）。
4. **Config**：选择 Client/Upgrade 的 `.csproj`，生成 `generalupdate.manifest.json`。
5. **Simulation**：选择旧版本目录和补丁 ZIP，确认 PASS。
6. **发布上线**：上传补丁 ZIP、OSS 清单到生产环境。

---

## 常见问题

| 现象 | 原因与处理 |
|------|-----------|
| Version invalid | 使用 `1.0.0`、`2.0.0-beta.1` 格式。不要用 `1.0` 或 `v1.0.0` |
| Patch 输出为空或极小 | old/new 目录可能选反，或两个目录内容完全一致。确认 old 是用户已安装版本，new 是目标版本 |
| 找不到 `delete_files.json` | 当前文件名是 `generalupdate.delete.json`，位于 Patch ZIP 根目录 |
| Simulation 提示找不到 SDK | 安装 .NET 10 SDK 并确保命令行可执行 `dotnet --version` |
| Simulation 端口冲突 | 修改 `ServerPort` 或释放本地 5000 端口 |
| 客户端下载后 Hash 校验失败 | 对**最终上传到 CDN/OSS 的文件**重新计算 Hash，检查是否被中间代理重压缩 |
| Upgrade 进程没有启动 | 检查 `generalupdate.manifest.json` 中 `updateAppName` 和 `updatePath` 是否与发布目录结构一致 |

---

## 关联文档

- [GeneralUpdate.Core](../doc/GeneralUpdate.Core.md)：Client/Upgrade 更新主流程
- [GeneralUpdate.Differential](../doc/GeneralUpdate.Differential.md)：差分算法 Clean/Dirty 模式
- [GeneralUpdate.Extension](../doc/GeneralUpdate.Extension.md)：扩展包安装与版本管理
- [入门实战手册](./Beginner%20cookbook.md)：从零跑通完整更新闭环
