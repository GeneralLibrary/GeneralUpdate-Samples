---
sidebar_position: 11
---

# GeneralUpdate.Tools

## 这是什么

GeneralUpdate.Tools 是一个基于 Avalonia 12 开发的跨平台桌面工具（Windows / Linux / macOS），用于在软件发布流程中生成和管理补丁包、扩展包、版本清单以及执行本地更新仿真。它不替代你的 CI/CD 系统，而是把”打包、校验、验证”这些重复劳动收敛到一个可视化工具中。

:::info Tools 与 CI/CD 的关系
Tools 是**桌面 GUI 工具**，适合开发者在本地交互式地生成补丁、验证更新。如果你需要在 CI/CD 流水线中自动化补丁生成，可以直接调用 `GeneralUpdate.Core.Pipeline.DiffPipeline.CleanAsync()` —— GUI 和脚本走的是同一条代码路径。
:::

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

> Simulation 模块内部会调用 `dotnet publish` 构建测试应用，因此使用仿真功能时必须安装 .NET SDK。Mobile 模块的项目模式（Build & Locate）也会调用 `dotnet publish`，同样需要 .NET SDK。仅运行 Patch / Extension / OSS / Config 模块以及 Mobile 的文件模式则不需要。

## 七个模块速览

| 模块 | 你提供 | 工具产出 | 下游消费者 |
|------|--------|----------|------------|
| **Patch** | 旧版本目录 + 新版本目录 | `{name}.zip`（含 `.patch` 文件、新文件、`generalupdate.delete.json`） | Server、OSS/CDN、Core Upgrade 进程 |
| **Extension** | 扩展源目录 + 元数据 | `{name}_{version}.zip`（含 `manifest.json`） | `GeneralUpdate.Extension` 组件 |
| **OSS Config** | 包名、版本、下载 URL、本地 ZIP（计算 Hash） | `oss_config.json`（版本数组） | OSS 客户端、对象存储发布流程 |
| **Config** | Client/Upgrade 的 `.csproj` | `generalupdate.manifest.json` + `sample_output/` 发布目录 | Client/Upgrade 启动引导 |
| **Simulation** | 旧版本目录 + 补丁 ZIP | 本地更新服务 + `simulation_report.md` | 发布前质量把关 |
| **Hash** | 本地文件（ZIP） | SHA256 小写十六进制字符串 | 完整性校验、服务端版本记录 |
| **Mobile** | APK/AAB 文件或 MAUI/Avalonia Android 的 `.csproj` | `mobile_version_{timestamp}.json` 版本记录 | 移动端更新服务端、GeneralUpdate.Avalonia/Maui 组件 |

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

1. **加密文件检测**：扫描 old/new 目录中的二进制文件，检测是否存在加壳（如 Themida、VMProtect）或加密签名。被检测到的文件将标记警告——此类文件无法生成有效差分补丁，将以全量文件形式打包。
2. 校验 old/new 目录存在、version 格式合法。
3. 创建临时目录 `gupatch_yyyyMMddHHmmss`。
4. 递归比较 old/new：**修改文件** → 生成 `.patch` 二进制差分；**新增文件** → 直接复制；**删除文件** → 记录 hash 到 `generalupdate.delete.json`。
5. 将临时目录压缩为 `{PackageName}.zip`。
6. 删除临时目录，ZIP 保留在 Output Directory。

> **关于加密文件**：加壳工具（Themida、VMProtect 等）、代码混淆或加密的二进制文件，因文件哈希在新旧版本间完全不同，差分算法对其无效。此类文件检测到后将直接以全量形式打包，而不会生成 `.patch` 差分。建议在发布前对原始文件进行去壳/解密处理，以获得最优的补丁体积。

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
- 详见 [GeneralUpdate.Extension](../doc/GeneralUpdate.Extension)

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
- 详见 [GeneralUpdate.Core](../doc/GeneralUpdate.Core) OSS 更新策略

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

## Mobile：移动端打包

### 解决什么问题

如果你使用 `GeneralUpdate.Avalonia.Android` 或 `GeneralUpdate.Maui.Android` 组件为 Android 应用提供自动更新能力，每次发布新版本时都需要获取 APK/AAB 文件的元数据（包名、版本号、SHA256 哈希、文件大小），并将其上传到服务端进行版本管理。Mobile 模块把"解析 → 打包 → 上传 → 生成版本记录"这四步收敛到一个界面中。

支持两种工作模式：

- **文件模式**：直接选择 APK 或 AAB 文件，自动解析 AndroidManifest.xml 提取元数据
- **项目模式**：选择 `.csproj` 项目文件，自动执行 `dotnet publish` 构建，然后定位产物并解析

### 输入

#### 文件模式

| 字段 | 必填 | 说明 |
|------|------|------|
| APK/AAB 文件 | ✅ | 选择 `.apk` 或 `.aab` 文件，工具自动识别格式类型 |
| Output Directory | ❌ | 版本记录 JSON 输出目录，为空时输出到桌面 |
| ProductId | ✅ | 产品标识 GUID，用于区分服务端产品线 |
| Platform | ✅ | 目标平台（默认 Android = 4） |
| Product Name | ❌ | 产品名称，写入版本记录 |
| Release Notes | ❌ | 发布说明 |
| Is Forcibly | ❌ | 是否强制更新 |

#### 项目模式

| 字段 | 必填 | 说明 |
|------|------|------|
| `.csproj` 文件 | ✅ | 选择 MAUI 或 Avalonia Android 项目的 `.csproj`，工具自动解析 `ApplicationId`、`ApplicationDisplayVersion`、`ApplicationVersion` |
| Output Directory | ❌ | 同上 |

### 操作流程

1. **选择模式**：切换 File Mode / Project Mode 开关。
   - **文件模式**：点击 **Select File** 选择 `.apk` 或 `.aab` 文件。
   - **项目模式**：点击 **Select Project** 选择 `.csproj` 文件，然后点击 **Build & Locate** 自动执行 `dotnet publish` 并定位产物。
2. **Analyze**：点击 Analyze 按钮，工具自动：
   - 识别文件格式（APK / Android App Bundle）
   - 从 AndroidManifest.xml 中提取 `PackageName`、`VersionName`、`VersionCode`
   - 计算 SHA256 哈希和文件大小
3. **填写上传配置**：确认或编辑自动提取的元数据，填写 ProductId、Product Name 等发布信息。
4. **Upload**：点击 Upload 上传到服务端，并自动生成版本记录 JSON 文件。

### 工具内部做了什么

1. **格式检测**：通过文件扩展名（`.apk` / `.aab`）和 ZIP 内部结构（`AndroidManifest.xml` 位置）自动识别包格式。
   - APK 检测：打开 ZIP 文件，检查根目录是否存在 `AndroidManifest.xml`。
   - AAB 检测：打开 ZIP 文件，检查是否存在 `base/manifest/AndroidManifest.xml`。
   - 格式未知时（不是 .apk 也不是 .aab）会直接返回 Unknown。

2. **元数据解析**：使用 `AxmlParser` 解析二进制的 AXML（Android Binary XML）格式，从 ZIP 中读取 `AndroidManifest.xml`。解析流程如下：
   - **字符串池提取**：读取 AXML 文件的 Chunk 头，找到类型为 `0x0001` 的 StringPool Chunk，从中解析出所有 UTF-16LE 编码的字符串数组。
   - **属性值提取**：遍历 XML 的 Start Element Chunk（类型 `0x0102`），解析属性块（每个属性 20 字节），通过属性名在字符串池中的索引匹配 `package` 和 `versionName`，再通过 `rawValueIndex` 从字符串池中取出属性值。
   - **versionCode 提取**：versionCode 存储为整型属性（类型 `0x10` = INT_DEC 或 `0x11` = INT_HEX），直接从属性的 `typedValueData` 字段（偏移 +16，4 字节）读取有符号整数。
   - AXML 结构与参考：https://justanapplication.wordpress.com/category/android/android-binary-xml/

3. **SHA256 计算**：对完整文件计算 SHA256 哈希值，输出小写十六进制字符串。

4. **文件大小**：读取文件长度并格式化为人类可读的显示（B / KB / MB / GB）。

5. **项目构建**（仅项目模式）：选择 `.csproj` 后会触发：
   - `MobileCsprojParser.Parse()` 解析 `.csproj` 的 XML，提取 `TargetFramework`（支持单 TFM 和多 TFM，多 TFM 时自动选取包含 `-android` 的目标框架）、`ApplicationId`（映射为 PackageName）、`ApplicationDisplayVersion`（映射为 VersionName）、`ApplicationVersion`（映射为 VersionCode）、`UseMaui`（用于识别 MAUI 项目）、`AndroidPackageFormat`、`AssemblyName`。
   - 调用 `dotnet publish "{csprojPath}" -c Release -o "{publishDir}"`。
   - 构建完成后在 `bin/Release/{tfm}/publish/` 目录搜索 `.apk` 或 `.aab` 文件，自动定位构建产物。
   - 项目类型显示：MAUI（`<UseMaui>true</UseMaui>`）或 Avalonia（没有 UseMaui）。

6. **上传**：通过 HTTP multipart/form-data 将文件上传到服务端。上传表单字段和对应的数据源如下：

   | 字段 | 值来源 | 示例 |
   |------|--------|------|
   | `Name` | ProductName（用户填写） | `MyApp` |
   | `Version` | VersionName（自动解析） | `2.0.0` |
   | `Hash` | SHA256（自动计算） | `a1b2c3d4...` |
   | `Format` | 包格式（自动识别） | `.apk` / `.aab` |
   | `Size` | 文件大小（自动计算） | `50000000` |
   | `AppType` | 固定值 `"1"` | `1` |
   | `Platform` | Platform（默认 4=Android） | `4` |
   | `ProductId` | ProductId（用户填写 GUID） | `2d974e2a-...` |
   | `IsForcibly` | 强制更新开关 | `true` / `false` |

   上传服务支持以下配置：
   - **Server URL**：服务端地址
   - **Upload Endpoint**：API 路径，默认 `/Packet/Create`
   - **Timeout**：超时时间（秒）
   - **Retry Count**：失败重试次数（指数退避）
   - **Auth**：认证配置，支持 Basic / Bearer Token / API Key 三种模式（凭据通过 DPAPI 加密存储）

7. **版本记录导出**：上传成功后自动生成 `mobile_version_{timestamp}.json`，包含完整的版本元数据。也可以在不上传的情况下单独点击 **Export Record Only** 导出记录，此时 URL 字段为 `"manual"`。

### 输出

```
{OutputDirectory}/mobile_version_20260614120000.json
```

```json
{
  "name": "MyApp",
  "version": "2.0.0",
  "hash": "a1b2c3d4e5f6...",
  "url": "https://server.example.com/packages/app-v2.0.0.apk",
  "packageName": "com.example.myapp",
  "fileSize": 50000000,
  "format": "apk",
  "platform": 4,
  "productId": "2d974e2a-31e6-4887-9bb1-b4689e98c77a",
  "isForcibly": false,
  "releaseDate": "2026-06-14T12:00:00.0000000Z"
}
```

### 下游如何使用

- 将生成的版本记录 JSON 导入或上传到你的更新服务端（如 GeneralSpacestation）
- 客户端（`GeneralUpdate.Avalonia.Android` 或 `GeneralUpdate.Maui.Android`）从服务端查询版本信息时，返回的数据结构与版本记录的内容对应
- 客户端下载 APK 后使用 `hash` 字段做 SHA256 完整性校验

### 支持的 AndroidManifest 字段提取

| 清单属性 | 字段名 | 说明 |
|---------|--------|------|
| `package` | `PackageName` | 应用包名（唯一标识） |
| `android:versionName` | `VersionName` | 展示版本号（如 `2.0.0`） |
| `android:versionCode` | `VersionCode` | 内部版本代码（整数） |

---

## 推荐发布工作流

### 桌面端更新

这个顺序把 Patch、OSS、Config、Simulation 四个模块串联成一个完整的桌面应用发布流水线：

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

### 移动端更新

使用 Mobile 模块串联移动端 Android 应用发布：

```
┌────────────┐    ┌──────────┐    ┌──────────┐
│ 构建 APK/  │ -> │ Mobile   │ -> │ 发布上线  │
│ AAB 产物   │    │ 解析+上传 │    │ 更新服务  │
└────────────┘    └──────────┘    └──────────┘
```

1. **构建产物**：在 CI 或本地构建出 `.apk` / `.aab` 文件。
2. **Mobile**：选择文件或项目，自动解析元数据、计算 SHA256、上传服务端、导出版本记录。
3. **发布上线**：确认上传成功，客户端通过 `GeneralUpdate.Avalonia.Android` 或 `GeneralUpdate.Maui.Android` 检查更新。

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
| Mobile 分析时提示 "Metadata extraction warning" | AXML 解析未找到预期属性，常见的两种原因：APK 使用资源 ID 引用而非直接文本存储属性值；或 APK 被加固/混淆后 AndroidManifest.xml 结构被修改。可以手动填写 PackageName、VersionName、VersionCode |
| Mobile 项目模式 Build & Locate 失败 | 确认已安装 .NET SDK 且版本支持目标框架（TFM），检查 `.csproj` 包含 `-android` 目标框架，输出目录 `bin/Release/{tfm}/publish/` 没有被写入保护 |
| Mobile 项目模式 "Build output not found" | `dotnet publish` 成功执行但未在预期目录找到 `.apk`/`.aab`。检查 `.csproj` 中 `AndroidPackageFormat` 设置是否正确（默认 `aab;apk` 优先产生 APK），以及 TFM 是否自动解析正确 |
| Mobile 上传失败 | 检查服务端地址和 Endpoint 配置是否正确，确认服务端认证方式（Basic / Bearer / API Key）与工具配置一致，查看工具日志中的 HTTP 状态码和错误消息 |
| Mobile 上传后客户端更新失败 | 确认服务端返回的版本记录 JSON 中 `Platform` 字段为 `4`（Android），`Format` 字段与客户端期望的 APK/AAB 格式一致 |

---

## 关联文档

- [GeneralUpdate.Core](../doc/GeneralUpdate.Core)：Client/Upgrade 更新主流程
- [GeneralUpdate.Differential](../doc/GeneralUpdate.Differential)：差分算法 Clean/Dirty 模式
- [GeneralUpdate.Extension](../doc/GeneralUpdate.Extension)：扩展包安装与版本管理
- [GeneralUpdate.Avalonia.Android](../doc/GeneralUpdate.Avalonia.Android)：Avalonia Android APK 更新组件
- [GeneralUpdate.Maui.Android](../doc/GeneralUpdate.Maui.Android)：MAUI Android APK 更新组件
- [入门实战手册](./Beginner%20cookbook)：从零跑通完整更新闭环
