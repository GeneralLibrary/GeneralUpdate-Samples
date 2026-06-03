---
sidebar_position: 11
---

### 简介

GeneralUpdate.Tools 是一款使用 Avalonia 开发的桌面应用程序，支持 Windows / Linux / Mac 跨平台。该工具为开发者提供了三个核心功能模块，用于管理软件更新和扩展。


| 仓库地址 |
| ----------------------------------------------------- |
|  https://github.com/GeneralLibrary/GeneralUpdate.Tools  |

| 功能 | 支持 | 说明 |
| ------------------------------- | -------------- | ------------------------------------------------------------ |
| 构建补丁包 | 是 | 比较前后版本，识别更新、新增或删除的文件 |
| 构建版本配置 | 是 | 轻松生成版本配置文件 |
| 扩展管理器 | 是 | 打包和管理应用程序扩展 |
| 模拟更新 | 是 | 在本地完整模拟客户端到服务端的更新流程 |
| 配置生成器 | 是 | 分析 .csproj 自动生成 manifest.json 和项目结构 |

![](imgs\tool.png)

![](imgs\tool2.png)

---

## 功能说明

### 1. 构建补丁包

#### 功能介绍

补丁包构建器用于创建差异更新包，通过对比旧版本和新版本，仅打包发生变化的文件，从而大幅减少更新包体积和下载时间。


#### 参数说明

| 名称 | 说明 |
| ---------------- | ------------------------------------------------------------ |
| 源路径 | 旧版本文件夹的路径 |
| 目标路径 | 新版本文件夹的路径 |
| 补丁路径 | 最终更新补丁包将生成的路径 |
| 构建 | 递归比较源路径和目标路径文件夹中的所有项目文件（DLL、exe 等），通过二进制差异检查和增量检查分析需要更新的文件列表，然后根据文件夹结构打包更新文件 |
| 清空 | 清除当前输入的内容 |

---

### 2. 构建 OSS 版本配置

#### 功能介绍

OSS 版本配置构建器用于生成 `version.json` 配置文件，该文件包含更新包的元数据信息，告知客户端应用程序如何获取和验证更新包。


#### 参数说明

| 名称 | 说明 |
| ---------------------- | ------------------------------------------------------------ |
| 发布日期时间 | 更新包的发布时间 |
| 包名 | 更新包的名称 |
| 哈希值 | 更新包的哈希值（用于完整性验证） |
| 版本号 | 更新包的版本号 |
| 下载地址 | 更新包的下载 |
| 获取哈希 | 获取更新包哈希值的功能 |
| 追加 | 将新的更新信息追加到现有版本详情中 |
| 清空 | 清除所有填写的内容 |
| 复制 | 将生成的内容复制到剪贴板 |
| 构建 | 将版本配置文件（.json）生成到本地磁盘 |

---

### 3. 扩展管理器

#### 功能介绍

扩展管理器（ExtensionView）是 GeneralUpdate.Tools 的核心功能之一，用于将应用程序扩展打包成可分发的标准格式。该工具自动创建包含所有必要元数据的扩展包，支持平台特定配置、依赖管理和自定义属性，非常适合构建插件系统和扩展市场。


#### 核心特性

- **完整的元数据管理 **：支持扩展名称、版本、描述、发布者、许可证等标准字段
- **平台支持 **：可指定目标平台（Windows / Linux / MacOS）
- **版本兼容性 **：定义最小和最大主机版本要求
- **依赖管理 **：声明扩展依赖的其他扩展
- **自动打包 **：自动压缩扩展目录并生成 manifest.json
- **自定义属性 **：支持添加额外的键值对元数据
- **分类标签 **：通过类别标签组织和发现扩展

---

## 扩展管理器使用指南

### 基本信息字段

#### 扩展名称

**描述**：扩展的唯一标识符，建议使用小写字母和连字符，不含空格。


**示例**：`my-awesome-plugin`、`data-exporter`

---

#### 显示名称

**描述**：面向用户的友好显示名称，将显示在扩展列表和详情页面中。


**示例**：`My Awesome Plugin`、`Data Exporter Tool`

---

#### 版本号

**描述**：扩展的版本号，建议遵循语义化版本规范（SemVer），格式为 `主版本.次版本.修订号.构建号`。


**示例**：`1.0.0.0`、`2.1.3.0`

---

#### 描述

**描述**：扩展功能的详细说明，告诉用户该扩展的用途和特性。支持多行文本。


**示例**：
```
此扩展为您的应用程序添加强大的数据导出功能，
支持多种格式，包括 CSV、Excel 和 JSON。
```

---

#### 发布者

**描述**：扩展的发布者名称或组织标识符。


**示例**：`YourCompany`、`john-doe`、`awesome-dev-team`

---

#### 许可证

**描述**：扩展使用的开源许可证标识符。


**示例**：`MIT`、`Apache-2.0`、`GPL-3.0`、`BSD-3-Clause`

---

#### 分类

**描述**：扩展所属的分类标签，多个分类用逗号分隔。用于组织和搜索扩展。


**示例**：`Tools, Productivity`、`Data, Export, Utilities`

**常见分类 Common Categories**：
- `Tools` - 工具类
- `Productivity` - 效率提升
- `Data` - 数据处理
- `UI` - 用户界面
- `Security` - 安全
- `Development` - 开发

---

### 路径配置

#### 扩展目录

**描述**：包含扩展所有文件的源目录路径。该目录中的所有文件将被打包到最终的扩展包中。


**操作 Operation**：点击 "Pick" 按钮选择文件夹

**目录结构示例 Directory Structure**：
```
MyExtension/
  ├── bin/
  │   ├── MyExtension.dll
  │   └── dependencies/
  ├── resources/
  │   ├── icons/
  │   └── templates/
  └── README.md
```

---

#### 导出路径

**描述**：最终生成的扩展包（.zip 文件）的保存目录。


**操作 Operation**：点击 "Pick" 按钮选择保存位置

**输出格式 Output Format**：生成的文件名格式为 `{ExtensionName}_{Version}.zip`


**示例**：`my-awesome-plugin_1.0.0.0.zip`

---

### 依赖关系

#### 依赖项

**描述**：该扩展所依赖的其他扩展的 ID 列表，多个依赖项用逗号分隔。扩展系统会自动处理依赖关系的解析和安装顺序。


**格式 Format**：逗号分隔的扩展 GUID 列表

**示例**：
```
550e8400-e29b-41d4-a716-446655440001,
550e8400-e29b-41d4-a716-446655440002
```

**使用场景
- 扩展需要其他扩展提供的功能
- 共享公共库或资源
- 确保正确的加载顺序

---

### 版本兼容性

#### 最小主机版本

**描述**：扩展支持的最低主机应用程序版本。如果主机版本低于此值，扩展将被标记为不兼容。


**格式 Format**：语义化版本号（不含构建号）Semantic version number (without build number)

**示例**：`1.0.0`、`2.5.0`

---

#### 最大主机版本

**描述**：扩展支持的最高主机应用程序版本。如果主机版本高于此值，扩展将被标记为不兼容。


**格式 Format**：语义化版本号（不含构建号）Semantic version number (without build number)

**示例**：`3.0.0`、`2.9.9`

**兼容性检查逻辑 Compatibility Check Logic**：
```
MinHostVersion ≤ 主机版本 ≤ MaxHostVersion
MinHostVersion ≤ Host Version ≤ MaxHostVersion
```

**示例场景**：
- 主机版本：`2.0.0`
- 最小主机版本：`1.5.0`
- 最大主机版本：`2.5.0`
- 结果：✓ 兼容 Compatible

---

### 平台和格式

#### 格式

**描述**：扩展包的文件格式。目前固定为 `.zip` 格式，这是一个只读字段。


**值 Value**：`.zip` （只读 Read-only）

---

#### 发布日期

**描述**：扩展的发布日期。用于版本追踪和显示发布时间线。


**操作 Operation**：使用日历选择器选择日期

---

#### 平台

**描述**：扩展支持的目标操作系统平台。选择正确的平台可以确保扩展仅在兼容的系统上安装和运行。


**可选值 Available Options**：
- `Windows` - 仅支持
- `Linux` - 仅支持
- `MacOS` - 仅支持

**选择指南 Selection Guide**：
- 如果扩展使用了平台特定的 API 或库，选择对应的平台
- 跨平台扩展可以为每个平台创建单独的版本
- 平台选择影响扩展在不同操作系统上的可见性和可安装性

---

### 选项

#### 预发布版本

**描述**：标记该扩展版本是否为预发布版本（如 Alpha、Beta、RC）。预发布版本通常用于测试和早期访问。


**复选框 Checkbox**：勾选表示预发布

**使用场景
- 内部测试版本
- 公开测试版本
- 候选发布版本

**影响 Impact**：
- 预发布版本可能在扩展市场中单独显示或标记 Pre-release versions may be displayed or marked separately in extension marketplaces
- 用户可以选择是否接收预发布版本的自动更新

---

### 自定义属性

#### 启用自定义属性

**描述**：启用此选项后，可以为扩展添加额外的键值对元数据，用于存储扩展特定的配置或信息。


**复选框 Checkbox**：勾选以显示自定义属性输入区域

---

#### 添加自定义属性

**操作步骤 Operation Steps**：

1. **输入属性键 Enter Property Key**：在 "Property" 字段中输入属性名称"Property" field
   - 建议使用驼峰命名法或连字符
   - 示例：`maxConnections`、`default-theme`、`apiEndpoint`

2. **输入属性值 Enter Property Value**：在 "Value" 字段中输入对应的值"Value" field
   - 支持字符串、数字等各种值
   - 示例：`100`、`dark`、`https://api.example.com`

3. **点击添加 Click Add**：点击 "Add" 按钮将属性添加到列表 Click the "Add" button to add the property to the list

4. **管理属性 Manage Properties**：
   - 查看已添加的属性
   - 点击 "Remove" 删除不需要的属性 Click "Remove" to delete unwanted properties

**使用场景
- 存储扩展特定的配置选项 Store extension-specific configuration options
- 记录扩展的元数据信息
- 传递初始化参数
- 存储扩展的 API 端点或资源路径 Store extension API endpoints or resource paths

**示例**：
```json
{
  "maxConcurrentTasks": "5",
  "defaultLanguage": "en-US",
  "apiBaseUrl": "https://api.myextension.com",
  "enableDebugMode": "false"
}
```

---

## 操作流程

### 创建扩展包的完整步骤

#### 步骤 1：准备扩展文件

确保您的扩展目录包含所有必要的文件：
- 程序集文件（DLL、可执行文件）
- 资源文件（图标、模板、配置文件）
- 依赖库
- 文档文件（README、LICENSE）

---

#### 步骤 2：打开扩展管理器

1. 启动 GeneralUpdate.Tools Launch GeneralUpdate.Tools
2. 点击顶部的 "Extension" 选项卡 Click the "Extension" tab at the top
3. 进入扩展管理器界面---

#### 步骤 3：填写基本信息

按照上述字段说明，依次填写：
- Extension Name（扩展名称）
- Display Name（显示名称）
- 版本号
- 描述
- 发布者
- 许可证
- Categories（分类）

- 扩展名称
- 显示名称
- 版本
- 描述
- 发布者
- 许可证
- 分类

---

#### 步骤 4：配置路径

1. **选择扩展目录**：
   - 点击 "Extension Directory" 旁的 "Pick" 按钮
   - 浏览并选择包含扩展文件的文件夹

2. **选择导出路径**：
   - 点击 "Export Path" 旁的 "Pick" 按钮
   - 选择保存生成的扩展包的目录

---

#### 步骤 5：配置依赖和兼容性（可选）

1. **添加依赖项
   - 在 "Dependencies" 字段中输入依赖扩展的 GUID Enter the GUID of dependent extensions in the "Dependencies" field
   - 多个依赖项用逗号分隔

2. **设置版本兼容性 Set Version Compatibility**：
   - 填写 "Min Host Version" Fill in "Min Host Version"
   - 填写 "Max Host Version" Fill in "Max Host Version"

---

#### 步骤 6：选择平台和选项

1. **选择目标平台**：
   - 从 "Platform" 下拉列表中选择 Windows、Linux 或 MacOS Select Windows, Linux, or MacOS from the "Platform" dropdown list

2. **设置发布日期 Set Release Date**：
   - 使用日历选择器选择发布日期

3. **标记预发布版本 Mark Pre-release Version**（如果适用 if applicable）：
   - 勾选 "Pre-release" 复选框 Check the "Pre-release" checkbox

---

#### 步骤 7：添加自定义属性（可选）

2. 在输入框中添加键值对
3. 点击 "Add" 按钮添加每个属性 Click the "Add" button to add each property
4. 根据需要添加多个自定义属性

---

#### 步骤 8：生成扩展包

1. **检查所有信息 Review All Information**：
   - 确保所有必填字段都已正确填写
   - 验证路径是否正确

2. **点击 Build 按钮 Click Build Button**：
   - 点击底部的 "Build" 按钮
   - 等待打包过程完成

3. **验证成功
   - 将显示成功消息，包含文件名和位置 A success message will be displayed with the file name and location
   - 生成的扩展包位于导出路径中

---

#### 步骤 9：验证扩展包

生成后，验证扩展包是否包含：
1. 所有源文件（从扩展目录压缩）
2. `manifest.json` 文件（自动生成）

可以使用解压缩工具打开 .zip 文件进行检查：
```
MyExtension_1.0.0.0.zip
  ├── manifest.json          ← 扩展元数据 Extension metadata
  ├── bin/
  │   └── MyExtension.dll
  ├── resources/
  └── README.md
```

---

### 清除表单

如果需要重新开始或清除所有输入的内容：
1. 点击底部的 "Clear" 按钮 Click the "Clear" button at the bottom
2. 所有字段将重置为默认值 All fields will be reset to default values
3. 自定义属性列表将被清空 Custom properties list will be cleared


---

## 输出结果

### 生成的扩展包结构

扩展管理器生成的 .zip 文件具有以下结构：


```
ExtensionName_Version.zip
  ├── manifest.json                    ← 扩展元数据 Extension metadata
  ├── [所有源目录中的文件和文件夹]     ← All files and folders from source directory
  └── ...
```

---

### manifest.json 文件内容

`manifest.json` 文件包含所有扩展元数据，示例结构：


```json
{
  "Name": "my-awesome-plugin",
  "DisplayName": "My Awesome Plugin",
  "Version": "1.0.0.0",
  "Description": "This extension adds powerful features to your application",
  "Publisher": "YourCompany",
  "License": "MIT",
  "Categories": ["Tools", "Productivity"],
  "Dependencies": "550e8400-e29b-41d4-a716-446655440001",
  "MinHostVersion": "1.0.0",
  "MaxHostVersion": "2.0.0",
  "Format": ".zip",
  "ReleaseDate": "2026-02-12T00:00:00",
  "IsPreRelease": false,
  "Platform": {
    "DisplayName": "Windows",
    "Value": 1
  },
  "FileSize": 1048576,
  "CustomProperties": {
    "maxConnections": "100",
    "apiEndpoint": "https://api.example.com"
  }
}
```

---

### 字段映射说明

| 界面字段 | JSON 字段 | 数据类型 | 说明 |
| ----------------------- | -------------------- | ------------------ | ---------------------------------------- |
|  Extension Name           |  Name                  |  string              | 扩展唯一标识符 |
|  Display Name             |  DisplayName           |  string              | 显示名称 |
|  Version                  |  Version               |  string              | 版本号 |
|  Description              |  Description           |  string              | 扩展描述 |
|  Publisher                |  Publisher             |  string              | 发布者 |
|  License                  |  License               |  string              | 许可证 |
|  Categories               |  Categories            |  array               | 分类列表 |
|  Dependencies             |  Dependencies          |  string              | 依赖项（逗号分隔）Dependencies (comma-separated) |
|  Min Host Version         |  MinHostVersion        |  string              | 最小主机版本 |
|  Max Host Version         |  MaxHostVersion        |  string              | 最大主机版本 |
|  Format                   |  Format                |  string              | 文件格式 |
|  Release Date             |  ReleaseDate           |  DateTime            | 发布日期 |
|  Pre-release              |  IsPreRelease          |  boolean             | 是否预发布 |
|  Platform                 |  Platform              |  object              | 平台信息 |
|  Custom Properties        |  CustomProperties      |  object              | 自定义属性字典 |

---

## 最佳实践

### 命名规范

1. **扩展名称 Extension Name**：
   - 使用小写字母和连字符
   - 避免空格和特殊字符
   - 保持简洁且具有描述性
   - ✓ 正确 Correct：`data-exporter`、`theme-customizer`
   - ✗ 错误 Incorrect：`Data Exporter`、`Theme@Customizer`

2. **版本号 Version Number**：
   - 遵循语义化版本规范 Follow Semantic Versioning
   - 格式：`主版本.次版本.修订号.构建号` Format: `Major.Minor.Patch.Build`
   - 示例：`1.0.0.0`、`2.1.3.5`

---

### 目录组织

保持扩展目录结构清晰：

Maintain a clear extension directory structure:

```
MyExtension/
  ├── bin/                    ← 二进制文件 Binary files
  │   ├── MyExtension.dll
  │   └── dependencies/
  ├── resources/              ← 资源文件 Resource files
  │   ├── icons/
  │   ├── templates/
  │   └── localization/
  ├── docs/                   ← 文档 Documentation
  │   └── README.md
  └── LICENSE                 ← 许可证 License
```

---

### 版本兼容性管理

1. **设置合理的版本范围 Set Reasonable Version Range**：
   - 不要设置过窄的版本范围 Don't set too narrow version range
   - 考虑向后兼容性
   - 示例：Min `1.0.0`、Max `2.0.0` (支持 1.x 所有版本)

2. **主版本更新 Major Version Updates**：
   - 主机应用程序发生重大变化时，更新 Min/Max Host Version
   - 为新的主版本创建新的扩展版本

---

### 依赖管理

1. **最小化依赖 Minimize Dependencies**：
   - 只声明必需的依赖项
   - 避免循环依赖

2. **文档化依赖 Document Dependencies**：
   - 在描述中说明为什么需要这些依赖
   - 提供依赖项的获取方式

---

### 自定义属性使用

1. **使用场景
   - 存储扩展特定的配置 Store extension-specific configurations
   - 记录扩展的技术要求
   - 传递初始化参数

2. **命名建议 Naming Recommendations**：
   - 使用驼峰命名法 Use camelCase
   - 保持键名简洁明了
   - 添加前缀避免冲突

---

### 测试和验证

1. **打包前检查 Pre-packaging Checks**：
   - ✓ 验证所有文件是否完整
   - ✓ 检查版本号是否正确
   - ✓ 确认依赖项是否准确
   - ✓ 测试扩展在目标平台上的运行

2. **打包后验证 Post-packaging Verification**：
   - ✓ 解压查看文件结构
   - ✓ 检查 manifest.json 内容 Check manifest.json content
   - ✓ 验证文件大小是否合理
   - ✓ 在目标环境中安装测试
---

### 发布管理

1. **预发布版本 Pre-release Versions**：
   - 使用预发布标记进行内部测试 Use pre-release flag for internal testing
   - 收集反馈后再发布正式版本

2. **版本更新 Version Updates**：
   - 保持版本历史记录
   - 在描述中记录更新日志
   - 提供从旧版本升级的指导

---

## 故障排除

### 常见问题

#### 问题 1：构建失败

**可能原因 Possible Causes**：
- 扩展目录不存在或无法访问
- 导出路径无写入权限
- 磁盘空间不足
- 扩展目录为空

**解决方案 Solutions**：
1. 验证扩展目录路径是否正确
2. 检查导出路径的写入权限
3. 确保有足够的磁盘空间
4. 确认扩展目录包含文件

---

#### 问题 2：验证错误

**可能原因 Possible Causes**：
- 必填字段未填写
- 版本号格式不正确
- 路径格式无效

**解决方案 Solutions**：
1. 检查所有必填字段（名称、版本、目录、导出路径）Check all required fields (name, version, directory, export path)
2. 使用正确的版本号格式（如 `1.0.0.0`）Use correct version number format (e.g., `1.0.0.0`)
3. 确保路径使用正确的格式

---

#### 问题 3：生成的包无法解压

**可能原因 Possible Causes**：
- 打包过程被中断
- 磁盘空间在打包过程中耗尽
- 源文件被锁定或正在使用

**解决方案 Solutions**：
1. 重新生成扩展包
2. 确保有足够的磁盘空间
3. 关闭正在使用扩展文件的程序

---

#### 问题 4：自定义属性无法添加

**可能原因 Possible Causes**：
- 属性键或值为空
- 属性键已存在

**解决方案 Solutions**：
1. 确保键和值都已填写
2. 使用唯一的属性键名
3. 如需修改现有属性，先删除再重新添加 To modify existing properties, delete and re-add

---

### 4. 模拟更新

#### 功能介绍

模拟更新是 GeneralUpdate.Tools 最强大的功能之一，能够在本地完整模拟从客户端到服务端的整个更新流程。开发者无需部署真实服务端即可测试和验证更新逻辑。

**模拟更新会对以下流程进行端到端验证：**

1. **输入校验** — 验证 SemVer 版本格式、目录存在性、.NET SDK 版本
2. **环境准备** — 准备应用程序目录
3. **应用发布** — 使用 `dotnet publish` 发布内置的 ClientSample 和 UpgradeSample 测试应用
4. **清单生成** — 在应用目录生成 `generalupdate.manifest.json`
5. **Mock 服务端** — 启动本地 ASP.NET Core 模拟更新服务器
6. **执行更新** — 运行 ClientSample → 下载补丁 → 启动 UpgradeSample → 完成更新
7. **结果验证** — 检查更新是否成功并生成测试报告

#### 模拟架构

```
┌───────────────────────────────────────────┐
│           GeneralUpdate.Tools              │
│                                            │
│  ┌─────────────────────────────────────┐  │
│  │        SimulationService             │  │
│  │  Step1: Validate                     │  │
│  │  Step2: Prepare                      │  │
│  │  Step3: dotnet publish               │  │
│  │  Step4: Start LocalUpdateServer      │  │
│  │  Step5: Run ClientSample             │  │
│  │  Step6: Verify                       │  │
│  └──────────────┬──────────────────────┘  │
│                 │                          │
│  ┌──────────────▼──────────────────────┐  │
│  │     LocalUpdateServer (Mock API)     │  │
│  │  POST /Upgrade/Verification          │  │
│  │  POST /Upgrade/Report                │  │
│  │  GET  /patch/{filename}              │  │
│  └─────────────────────────────────────┘  │
└───────────────────────────────────────────┘
```

#### 参数说明

| 参数 | 说明 |
|------|------|
| 应用目录 | 存放 ClientSample 和 UpgradeSample 的根目录 |
| 补丁包路径 | 预先准备好的更新补丁包（.zip）路径 |
| 当前版本 | 模拟的客户端当前版本号 |
| 目标版本 | 模拟的更新目标版本号 |
| 平台 | 目标操作系统（Windows / Linux / macOS） |
| 应用类型 | Client / Upgrade |
| 应用密钥 | 服务端验证密钥（与 Configinfo.AppSecretKey 对应） |
| 产品 ID | 产品分支标识 |
| 更新路径 | 自定义更新路径（可选） |
| 服务端口 | 本地模拟服务端端口（默认自动选择） |

#### 使用步骤

**步骤 1：准备补丁包**

使用「补丁包」功能或手动准备一个更新补丁包（`.zip`），放到任意目录。

**步骤 2：配置模拟参数**

1. 打开 GeneralUpdate.Tools，切换到「模拟更新」选项卡
2. 选择应用目录（用于存放测试应用的位置）
3. 选择补丁包文件
4. 填写当前版本号（如 `1.0.0.0`）
5. 填写目标版本号（如 `1.0.1.0`）
6. 选择目标平台
7. 选择应用类型（Client）
8. 填写应用密钥和产品 ID

**步骤 3：启动模拟**

点击「开始模拟」按钮，观察实时日志输出：

```
[Step 1/6] 验证输入参数...
  ✓ 版本号格式正确
  ✓ 目录存在
  ✓ .NET SDK 10.0.0 已安装

[Step 2/6] 准备应用目录...
  ✓ 已创建应用目录

[Step 3/6] 发布测试应用...
  ✓ ClientSample 发布成功
  ✓ UpgradeSample 发布成功

[Step 4/6] 启动模拟服务器...
  ✓ 服务器已启动: http://127.0.0.1:50432

[Step 5/6] 运行更新流程...
  → 客户端连接服务器...
  → 发现新版本 1.0.1.0
  → 下载补丁包...
  → 解压补丁...
  → 应用差分包...
  → 启动升级助手...
  → 文件替换完成

[Step 6/6] 验证更新结果...
  ✓ 更新成功！
```

**步骤 4：查看报告**

模拟完成后，工具会生成 `simulation_report.md` 报告文件，包含：

- 模拟配置信息
- 完整时间线
- 各步骤详细日志
- 成功/失败状态

---

### 5. 配置生成器

#### 功能介绍

配置生成器是一个开发者效率工具，能够自动分析 `.csproj` 文件并生成 `generalupdate.manifest.json` 配置文件。它消除了手动编写配置文件的繁琐和出错风险。

**核心能力：**
- 解析 `.csproj` 文件提取 `AssemblyName`、`OutputType`、`TargetFramework`
- 自动填充应用名称、框架版本等字段
- 验证 SemVer 版本号格式
- 生成标准的 `generalupdate.manifest.json`
- 使用 `dotnet publish` 构建并组装示例项目结构

#### 配置管道

配置生成器内部使用 5 步管道处理：

```
CsprojParseStep → SemverValidateStep → ManifestBuildStep 
  → UserConfirmStep → FileEmitStep
```

| 步骤 | 说明 |
|------|------|
| **CsprojParseStep** | 解析 Client 和 Upgrade 的 .csproj 文件，提取项目元数据 |
| **SemverValidateStep** | 验证 ClientVersion 和 UpgradeClientVersion 是否为有效 SemVer |
| **ManifestBuildStep** | 将解析结果和用户输入合并，填充 Manifest 模型 |
| **UserConfirmStep** | 预留步骤，CLI 模式下交互确认；GUI 模式下为 no-op |
| **FileEmitStep** | 将 Manifest 序列化为 JSON 写入磁盘 |

#### 参数说明

| 参数 | 说明 |
|------|------|
| Client 路径 | 客户端项目的 `.csproj` 文件路径（必填） |
| Upgrade 路径 | 升级助手项目的 `.csproj` 文件路径（可选） |
| 主应用名称 | 自动从 AssemblyName 提取，可手动覆盖 |
| 客户端版本 | 客户端当前版本号（SemVer 格式） |
| 升级应用名称 | 自动从 Upgrade 项目的 AssemblyName 提取 |
| 升级端版本 | 升级助手当前版本号 |
| 应用类型 | Client / Upgrade |
| 产品 ID | 产品分支唯一标识 |
| 更新路径 | 自定义更新路径（可选） |

#### 使用步骤

**生成清单文件：**

1. 切换到「配置生成」选项卡
2. 点击「浏览」选择 Client `.csproj` 文件
3. （可选）选择 Upgrade `.csproj` 文件
4. 点击「分析」→ 自动提取 `AssemblyName`、`TargetFramework` 等信息
5. 手动填写版本号、应用类型、产品 ID
6. 点击「生成」→ `generalupdate.manifest.json` 写入磁盘
7. 点击「打开目录」查看生成的文件

**生成示例项目结构：**

1. 完成上述步骤 1-5
2. 点击「生成示例结构」
3. 工具自动执行 `dotnet publish` 编译 Client 和 Upgrade 项目
4. 将编译产物和 `manifest.json` 组装为完整的示例目录

#### 生成的 manifest.json 示例

```json
{
  "MainAppName": "MyApp.exe",
  "ClientVersion": "1.0.0.0",
  "AppType": "Client",
  "UpdateAppName": "UpgradeSample.exe",
  "UpgradeClientVersion": "1.0.0.0",
  "ProductId": "2d974e2a-31e6-4887-9bb1-b4689e98c77a",
  "UpdatePath": ""
}
```

#### CsprojInfo 提取规则

| .csproj 字段 | 提取目标 |
|-------------|----------|
| `<AssemblyName>` | 应用名称（MainAppName / UpdateAppName） |
| `<OutputType>` | 判断是否为可执行文件 |
| `<TargetFramework>` | 框架信息（如 net10.0） |
| 文件名 | fallback 应用名称 |

---

## 命令行模式 (CLI)

GeneralUpdate.Tools 的 Pipeline 架构支持命令行模式（开发中）。`UserConfirmStep` 即为 CLI 交互确认预留的步骤。

在 CLI 模式下可以集成到 CI/CD 流程：

```bash
# 计划中的 CLI 用法
GeneralUpdate.Tools config generate \
  --client ./src/MyApp/MyApp.csproj \
  --upgrade ./src/Upgrade/Upgrade.csproj \
  --client-version 1.0.0.0 \
  --product-id "your-product-id" \
  --output ./manifest.json
```

---

## 相关文档

### GeneralUpdate 生态系统

- **[GeneralUpdate.Core](./GeneralUpdate.Core.md)** - 核心更新组件
- **[GeneralUpdate.Extension](./GeneralUpdate.Extension.md)** - 扩展管理系统
- **[GeneralUpdate.ClientCore](./GeneralUpdate.ClientCore.md)** - 客户端更新组件
- **[快速入门指南](../quickstart/Quik%20start.md)** - GeneralUpdate 快速入门 GeneralUpdate quick start

### 外部资源

- **[GeneralUpdate.Tools 源代码](https://github.com/GeneralLibrary/GeneralUpdate.Tools)** - GitHub 仓库 GitHub 仓库
- **[GeneralUpdate 主项目](https://github.com/GeneralLibrary/GeneralUpdate)** - 主框架项目
- **[问题反馈](https://github.com/GeneralLibrary/GeneralUpdate.Tools/issues)** - 报告问题和建议

---

## 适用于

| 产品 | 版本 |
| ----------------- | ------------------------------------- |
|  .NET               |  5, 6, 7, 8, 9, 10                      |
|  .NET Framework     |  4.6.1+                                 |
|  .NET Standard      |  2.0                                    |
|  .NET Core          |  2.0+                                   |
|  Windows            |  10+                                    |
|  Linux              |  Ubuntu 20.04+, Debian 10+, Fedora 35+  |
|  macOS              |  10.15+ (Catalina and later)            |
