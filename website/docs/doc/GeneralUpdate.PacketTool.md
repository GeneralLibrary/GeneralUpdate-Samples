---
sidebar_position: 11
---

### 简介 / Introduction

GeneralUpdate.Tools 是一款使用 Avalonia 开发的桌面应用程序，支持 Windows / Linux / Mac 跨平台。该工具为开发者提供了三个核心功能模块，用于管理软件更新和扩展。

GeneralUpdate.Tools is a desktop application developed using Avalonia that supports Windows / Linux / Mac cross-platform. This tool provides developers with three core functional modules for managing software updates and extensions.

| 仓库地址 Repository URL                               |
| ----------------------------------------------------- |
| https://github.com/GeneralLibrary/GeneralUpdate.Tools |

| 功能 Feature                    | 支持 Supported | 说明 Remarks                                                 |
| ------------------------------- | -------------- | ------------------------------------------------------------ |
| 构建补丁包 Build patch package            | 是 Yes         | 比较前后版本，识别更新、新增或删除的文件 Compares the previous version with the current version to identify updated, newly added, or deleted files. |
| 构建 OSS 版本配置 Build OSS version configuration | 是 Yes         | 轻松生成 OSS 版本配置文件 Easily generates OSS version configuration files. |
| 扩展管理器 Extension Manager              | 是 Yes         | 打包和管理应用程序扩展 Package and manage application extensions. |

![](imgs\tool.png)

![](imgs\tool2.png)

---

## 功能说明 / Feature Descriptions

### 1. 构建补丁包 / Build Patch Package

#### 功能介绍 / Function Introduction

补丁包构建器用于创建差异更新包，通过对比旧版本和新版本，仅打包发生变化的文件，从而大幅减少更新包体积和下载时间。

The Patch Package Builder is used to create differential update packages. By comparing the old version with the new version, it only packages the changed files, significantly reducing the update package size and download time.

#### 参数说明 / Parameter Description

| 名称 Name        | 说明 Remarks                                                 |
| ---------------- | ------------------------------------------------------------ |
| 源路径 Source path | 旧版本文件夹的路径 Path to the folder of the previous version. |
| 目标路径 Target path | 新版本文件夹的路径 Path to the folder of the current version. |
| 补丁路径 Patch path  | 最终更新补丁包将生成的路径 Path where the final update patch package will be generated. |
| 构建 Build       | 递归比较源路径和目标路径文件夹中的所有项目文件（DLL、exe 等），通过二进制差异检查和增量检查分析需要更新的文件列表，然后根据文件夹结构打包更新文件 Recursively compares all project files (DLL, exe, etc.) in the Source path and Target path folders. Analyzes the list of files to be updated through binary difference check and incremental check, then packages the update files according to the folder structure. |
| 清空 Clear       | 清除当前输入的内容 Clears the currently entered content.    |

---

### 2. 构建 OSS 版本配置 / Build OSS Version Configuration

#### 功能介绍 / Function Introduction

OSS 版本配置构建器用于生成 `version.json` 配置文件，该文件包含更新包的元数据信息，告知客户端应用程序如何获取和验证更新包。

The OSS Version Configuration Builder is used to generate `version.json` configuration files, which contain metadata information about the update package, informing the client application how to obtain and verify the update package.

#### 参数说明 / Parameter Description

| 名称 Name              | 说明 Remarks                                                 |
| ---------------------- | ------------------------------------------------------------ |
| 发布日期时间 Release date time | 更新包的发布时间 Release time of the update package.        |
| 包名 PacketName        | 更新包的名称 Name of the update package.                     |
| 哈希值 Hash            | 更新包的哈希值（用于完整性验证）Hash value of the update package (for integrity verification). |
| 版本号 Version         | 更新包的版本号 Version number of the update package.        |
| 下载地址 Download address | 更新包的下载 URL Download URL of the update package.         |
| 获取哈希 Get hash      | 获取更新包哈希值的功能 Function to retrieve the hash value of the update package. |
| 追加 Append            | 将新的更新信息追加到现有版本详情中 Appends new update information to the existing version details. |
| 清空 Clear             | 清除所有填写的内容 Clears all filled-in content.            |
| 复制 Copy              | 将生成的内容复制到剪贴板 Copies the generated content to the clipboard. |
| 构建 Build             | 将 OSS 版本配置文件（.json）生成到本地磁盘 Generates the OSS version configuration file (.json) to the local disk. |

---

### 3. 扩展管理器 / Extension Manager

#### 功能介绍 / Function Introduction

扩展管理器（ExtensionView）是 GeneralUpdate.Tools 的核心功能之一，用于将应用程序扩展打包成可分发的标准格式。该工具自动创建包含所有必要元数据的扩展包，支持平台特定配置、依赖管理和自定义属性，非常适合构建插件系统和扩展市场。

The Extension Manager (ExtensionView) is one of the core features of GeneralUpdate.Tools, used to package application extensions into distributable standard formats. This tool automatically creates extension packages containing all necessary metadata, supports platform-specific configurations, dependency management, and custom properties, making it ideal for building plugin systems and extension marketplaces.

#### 核心特性 / Core Features

- **完整的元数据管理 / Complete Metadata Management**：支持扩展名称、版本、描述、发布者、许可证等标准字段 Supports standard fields such as extension name, version, description, publisher, license, etc.
- **平台支持 / Platform Support**：可指定目标平台（Windows / Linux / MacOS）Can specify target platforms (Windows / Linux / MacOS)
- **版本兼容性 / Version Compatibility**：定义最小和最大主机版本要求 Define minimum and maximum host version requirements
- **依赖管理 / Dependency Management**：声明扩展依赖的其他扩展 Declare dependencies on other extensions
- **自动打包 / Automatic Packaging**：自动压缩扩展目录并生成 manifest.json Automatically compress extension directory and generate manifest.json
- **自定义属性 / Custom Properties**：支持添加额外的键值对元数据 Supports adding additional key-value pair metadata
- **分类标签 / Category Tags**：通过类别标签组织和发现扩展 Organize and discover extensions through category tags

---

## 扩展管理器使用指南 / Extension Manager Usage Guide

### 基本信息字段 / Basic Information Fields

#### 扩展名称 / Extension Name

**描述 Description**：扩展的唯一标识符，建议使用小写字母和连字符，不含空格。

Unique identifier for the extension. It is recommended to use lowercase letters and hyphens without spaces.

**示例 Example**：`my-awesome-plugin`、`data-exporter`

---

#### 显示名称 / Display Name

**描述 Description**：面向用户的友好显示名称，将显示在扩展列表和详情页面中。

User-friendly display name that will be shown in extension lists and detail pages.

**示例 Example**：`My Awesome Plugin`、`Data Exporter Tool`

---

#### 版本号 / Version

**描述 Description**：扩展的版本号，建议遵循语义化版本规范（SemVer），格式为 `主版本.次版本.修订号.构建号`。

Extension version number. It is recommended to follow Semantic Versioning (SemVer) specification in the format `Major.Minor.Patch.Build`.

**示例 Example**：`1.0.0.0`、`2.1.3.0`

---

#### 描述 / Description

**描述 Description**：扩展功能的详细说明，告诉用户该扩展的用途和特性。支持多行文本。

Detailed description of the extension's functionality, telling users the purpose and features of the extension. Supports multi-line text.

**示例 Example**：
```
This extension adds powerful data export capabilities to your application,
supporting multiple formats including CSV, Excel, and JSON.
```

---

#### 发布者 / Publisher

**描述 Description**：扩展的发布者名称或组织标识符。

Publisher name or organization identifier of the extension.

**示例 Example**：`YourCompany`、`john-doe`、`awesome-dev-team`

---

#### 许可证 / License

**描述 Description**：扩展使用的开源许可证标识符。

Open source license identifier used by the extension.

**示例 Example**：`MIT`、`Apache-2.0`、`GPL-3.0`、`BSD-3-Clause`

---

#### 分类 / Categories

**描述 Description**：扩展所属的分类标签，多个分类用逗号分隔。用于组织和搜索扩展。

Category tags to which the extension belongs. Multiple categories are separated by commas. Used for organizing and searching extensions.

**示例 Example**：`Tools, Productivity`、`Data, Export, Utilities`

**常见分类 Common Categories**：
- `Tools` - 工具类 Tools
- `Productivity` - 效率提升 Productivity enhancement
- `Data` - 数据处理 Data processing
- `UI` - 用户界面 User interface
- `Security` - 安全 Security
- `Development` - 开发 Development

---

### 路径配置 / Path Configuration

#### 扩展目录 / Extension Directory

**描述 Description**：包含扩展所有文件的源目录路径。该目录中的所有文件将被打包到最终的扩展包中。

Source directory path containing all files of the extension. All files in this directory will be packaged into the final extension package.

**操作 Operation**：点击 "Pick" 按钮选择文件夹 Click the "Pick" button to select a folder

**目录结构示例 Directory Structure Example**：
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

#### 导出路径 / Export Path

**描述 Description**：最终生成的扩展包（.zip 文件）的保存目录。

Save directory for the final generated extension package (.zip file).

**操作 Operation**：点击 "Pick" 按钮选择保存位置 Click the "Pick" button to select save location

**输出格式 Output Format**：生成的文件名格式为 `{ExtensionName}_{Version}.zip`

Generated file name format: `{ExtensionName}_{Version}.zip`

**示例 Example**：`my-awesome-plugin_1.0.0.0.zip`

---

### 依赖关系 / Dependencies

#### 依赖项 / Dependencies

**描述 Description**：该扩展所依赖的其他扩展的 ID 列表，多个依赖项用逗号分隔。扩展系统会自动处理依赖关系的解析和安装顺序。

List of IDs of other extensions that this extension depends on, separated by commas. The extension system will automatically handle dependency resolution and installation order.

**格式 Format**：逗号分隔的扩展 GUID 列表 Comma-separated list of extension GUIDs

**示例 Example**：
```
550e8400-e29b-41d4-a716-446655440001,
550e8400-e29b-41d4-a716-446655440002
```

**使用场景 Use Cases**：
- 扩展需要其他扩展提供的功能 Extension requires functionality provided by other extensions
- 共享公共库或资源 Sharing common libraries or resources
- 确保正确的加载顺序 Ensuring correct loading order

---

### 版本兼容性 / Version Compatibility

#### 最小主机版本 / Min Host Version

**描述 Description**：扩展支持的最低主机应用程序版本。如果主机版本低于此值，扩展将被标记为不兼容。

Minimum host application version supported by the extension. If the host version is lower than this value, the extension will be marked as incompatible.

**格式 Format**：语义化版本号（不含构建号）Semantic version number (without build number)

**示例 Example**：`1.0.0`、`2.5.0`

---

#### 最大主机版本 / Max Host Version

**描述 Description**：扩展支持的最高主机应用程序版本。如果主机版本高于此值，扩展将被标记为不兼容。

Maximum host application version supported by the extension. If the host version is higher than this value, the extension will be marked as incompatible.

**格式 Format**：语义化版本号（不含构建号）Semantic version number (without build number)

**示例 Example**：`3.0.0`、`2.9.9`

**兼容性检查逻辑 Compatibility Check Logic**：
```
MinHostVersion ≤ 主机版本 ≤ MaxHostVersion
MinHostVersion ≤ Host Version ≤ MaxHostVersion
```

**示例场景 Example Scenario**：
- 主机版本 Host Version：`2.0.0`
- 最小主机版本 Min Host Version：`1.5.0`
- 最大主机版本 Max Host Version：`2.5.0`
- 结果 Result：✓ 兼容 Compatible

---

### 平台和格式 / Platform and Format

#### 格式 / Format

**描述 Description**：扩展包的文件格式。目前固定为 `.zip` 格式，这是一个只读字段。

File format of the extension package. Currently fixed as `.zip` format, this is a read-only field.

**值 Value**：`.zip` （只读 Read-only）

---

#### 发布日期 / Release Date

**描述 Description**：扩展的发布日期。用于版本追踪和显示发布时间线。

Release date of the extension. Used for version tracking and displaying release timeline.

**操作 Operation**：使用日历选择器选择日期 Use the calendar picker to select a date

---

#### 平台 / Platform

**描述 Description**：扩展支持的目标操作系统平台。选择正确的平台可以确保扩展仅在兼容的系统上安装和运行。

Target operating system platform supported by the extension. Selecting the correct platform ensures that the extension is only installed and run on compatible systems.

**可选值 Available Options**：
- `Windows` - 仅支持 Windows 操作系统 Only supports Windows operating system
- `Linux` - 仅支持 Linux 操作系统 Only supports Linux operating system
- `MacOS` - 仅支持 MacOS 操作系统 Only supports MacOS operating system

**选择指南 Selection Guide**：
- 如果扩展使用了平台特定的 API 或库，选择对应的平台 If the extension uses platform-specific APIs or libraries, select the corresponding platform
- 跨平台扩展可以为每个平台创建单独的版本 Cross-platform extensions can create separate versions for each platform
- 平台选择影响扩展在不同操作系统上的可见性和可安装性 Platform selection affects the visibility and installability of extensions on different operating systems

---

### 选项 / Options

#### 预发布版本 / Pre-release

**描述 Description**：标记该扩展版本是否为预发布版本（如 Alpha、Beta、RC）。预发布版本通常用于测试和早期访问。

Mark whether this extension version is a pre-release version (such as Alpha, Beta, RC). Pre-release versions are typically used for testing and early access.

**复选框 Checkbox**：勾选表示预发布 Check to indicate pre-release

**使用场景 Use Cases**：
- 内部测试版本 Internal test version
- 公开测试版本 Public beta version
- 候选发布版本 Release candidate version

**影响 Impact**：
- 预发布版本可能在扩展市场中单独显示或标记 Pre-release versions may be displayed or marked separately in extension marketplaces
- 用户可以选择是否接收预发布版本的自动更新 Users can choose whether to receive automatic updates for pre-release versions

---

### 自定义属性 / Custom Properties

#### 启用自定义属性 / Enable Custom Properties

**描述 Description**：启用此选项后，可以为扩展添加额外的键值对元数据，用于存储扩展特定的配置或信息。

When enabled, you can add additional key-value pair metadata for the extension, used to store extension-specific configurations or information.

**复选框 Checkbox**：勾选以显示自定义属性输入区域 Check to show custom properties input area

---

#### 添加自定义属性 / Add Custom Properties

**操作步骤 Operation Steps**：

1. **输入属性键 Enter Property Key**：在 "Property" 字段中输入属性名称 Enter the property name in the "Property" field
   - 建议使用驼峰命名法或连字符 Recommended to use camelCase or hyphen-case
   - 示例 Examples：`maxConnections`、`default-theme`、`apiEndpoint`

2. **输入属性值 Enter Property Value**：在 "Value" 字段中输入对应的值 Enter the corresponding value in the "Value" field
   - 支持字符串、数字等各种值 Supports strings, numbers, and various other values
   - 示例 Examples：`100`、`dark`、`https://api.example.com`

3. **点击添加 Click Add**：点击 "Add" 按钮将属性添加到列表 Click the "Add" button to add the property to the list

4. **管理属性 Manage Properties**：
   - 查看已添加的属性 View added properties
   - 点击 "Remove" 删除不需要的属性 Click "Remove" to delete unwanted properties

**使用场景 Use Cases**：
- 存储扩展特定的配置选项 Store extension-specific configuration options
- 记录扩展的元数据信息 Record extension metadata information
- 传递初始化参数 Pass initialization parameters
- 存储扩展的 API 端点或资源路径 Store extension API endpoints or resource paths

**示例 Examples**：
```json
{
  "maxConcurrentTasks": "5",
  "defaultLanguage": "en-US",
  "apiBaseUrl": "https://api.myextension.com",
  "enableDebugMode": "false"
}
```

---

## 操作流程 / Operation Workflow

### 创建扩展包的完整步骤 / Complete Steps to Create an Extension Package

#### 步骤 1：准备扩展文件 / Step 1: Prepare Extension Files

确保您的扩展目录包含所有必要的文件：
- 程序集文件（DLL、可执行文件）
- 资源文件（图标、模板、配置文件）
- 依赖库
- 文档文件（README、LICENSE）

Ensure your extension directory contains all necessary files:
- Assembly files (DLLs, executables)
- Resource files (icons, templates, configuration files)
- Dependency libraries
- Documentation files (README, LICENSE)

---

#### 步骤 2：打开扩展管理器 / Step 2: Open Extension Manager

1. 启动 GeneralUpdate.Tools Launch GeneralUpdate.Tools
2. 点击顶部的 "Extension" 选项卡 Click the "Extension" tab at the top
3. 进入扩展管理器界面 Enter the Extension Manager interface

---

#### 步骤 3：填写基本信息 / Step 3: Fill in Basic Information

按照上述字段说明，依次填写：
- Extension Name（扩展名称）
- Display Name（显示名称）
- Version（版本号）
- Description（描述）
- Publisher（发布者）
- License（许可证）
- Categories（分类）

Fill in the following fields according to the field descriptions above:
- Extension Name
- Display Name
- Version
- Description
- Publisher
- License
- Categories

---

#### 步骤 4：配置路径 / Step 4: Configure Paths

1. **选择扩展目录 Select Extension Directory**：
   - 点击 "Extension Directory" 旁的 "Pick" 按钮 Click the "Pick" button next to "Extension Directory"
   - 浏览并选择包含扩展文件的文件夹 Browse and select the folder containing extension files

2. **选择导出路径 Select Export Path**：
   - 点击 "Export Path" 旁的 "Pick" 按钮 Click the "Pick" button next to "Export Path"
   - 选择保存生成的扩展包的目录 Select the directory to save the generated extension package

---

#### 步骤 5：配置依赖和兼容性（可选）/ Step 5: Configure Dependencies and Compatibility (Optional)

1. **添加依赖项 Add Dependencies**（如果适用 if applicable）：
   - 在 "Dependencies" 字段中输入依赖扩展的 GUID Enter the GUID of dependent extensions in the "Dependencies" field
   - 多个依赖项用逗号分隔 Separate multiple dependencies with commas

2. **设置版本兼容性 Set Version Compatibility**：
   - 填写 "Min Host Version" Fill in "Min Host Version"
   - 填写 "Max Host Version" Fill in "Max Host Version"

---

#### 步骤 6：选择平台和选项 / Step 6: Select Platform and Options

1. **选择目标平台 Select Target Platform**：
   - 从 "Platform" 下拉列表中选择 Windows、Linux 或 MacOS Select Windows, Linux, or MacOS from the "Platform" dropdown list

2. **设置发布日期 Set Release Date**：
   - 使用日历选择器选择发布日期 Use the calendar picker to select the release date

3. **标记预发布版本 Mark Pre-release Version**（如果适用 if applicable）：
   - 勾选 "Pre-release" 复选框 Check the "Pre-release" checkbox

---

#### 步骤 7：添加自定义属性（可选）/ Step 7: Add Custom Properties (Optional)

1. 勾选 "Enable Custom Properties" Check "Enable Custom Properties"
2. 在输入框中添加键值对 Add key-value pairs in the input boxes
3. 点击 "Add" 按钮添加每个属性 Click the "Add" button to add each property
4. 根据需要添加多个自定义属性 Add multiple custom properties as needed

---

#### 步骤 8：生成扩展包 / Step 8: Build Extension Package

1. **检查所有信息 Review All Information**：
   - 确保所有必填字段都已正确填写 Ensure all required fields are correctly filled in
   - 验证路径是否正确 Verify that paths are correct

2. **点击 Build 按钮 Click Build Button**：
   - 点击底部的 "Build" 按钮 Click the "Build" button at the bottom
   - 等待打包过程完成 Wait for the packaging process to complete

3. **验证成功 Verify Success**：
   - 将显示成功消息，包含文件名和位置 A success message will be displayed with the file name and location
   - 生成的扩展包位于导出路径中 The generated extension package is located in the export path

---

#### 步骤 9：验证扩展包 / Step 9: Verify Extension Package

生成后，验证扩展包是否包含：
1. 所有源文件（从扩展目录压缩）All source files (compressed from extension directory)
2. `manifest.json` 文件（自动生成）`manifest.json` file (automatically generated)

After generation, verify that the extension package contains:
1. All source files (compressed from extension directory)
2. `manifest.json` file (automatically generated)

可以使用解压缩工具打开 .zip 文件进行检查：
You can open the .zip file with a decompression tool to inspect:
```
MyExtension_1.0.0.0.zip
  ├── manifest.json          ← 扩展元数据 Extension metadata
  ├── bin/
  │   └── MyExtension.dll
  ├── resources/
  └── README.md
```

---

### 清除表单 / Clear Form

如果需要重新开始或清除所有输入的内容：
1. 点击底部的 "Clear" 按钮 Click the "Clear" button at the bottom
2. 所有字段将重置为默认值 All fields will be reset to default values
3. 自定义属性列表将被清空 Custom properties list will be cleared

If you need to start over or clear all entered content:
1. Click the "Clear" button at the bottom
2. All fields will be reset to default values
3. Custom properties list will be cleared

---

## 输出结果 / Output Results

### 生成的扩展包结构 / Generated Extension Package Structure

扩展管理器生成的 .zip 文件具有以下结构：

The .zip file generated by the Extension Manager has the following structure:

```
ExtensionName_Version.zip
  ├── manifest.json                    ← 扩展元数据 Extension metadata
  ├── [所有源目录中的文件和文件夹]     ← All files and folders from source directory
  └── ...
```

---

### manifest.json 文件内容 / manifest.json File Content

`manifest.json` 文件包含所有扩展元数据，示例结构：

The `manifest.json` file contains all extension metadata. Example structure:

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

### 字段映射说明 / Field Mapping Description

| 界面字段 UI Field       | JSON 字段 JSON Field | 数据类型 Data Type | 说明 Description                         |
| ----------------------- | -------------------- | ------------------ | ---------------------------------------- |
| Extension Name          | Name                 | string             | 扩展唯一标识符 Extension unique identifier |
| Display Name            | DisplayName          | string             | 显示名称 Display name                    |
| Version                 | Version              | string             | 版本号 Version number                    |
| Description             | Description          | string             | 扩展描述 Extension description           |
| Publisher               | Publisher            | string             | 发布者 Publisher                         |
| License                 | License              | string             | 许可证 License                           |
| Categories              | Categories           | array              | 分类列表 Category list                   |
| Dependencies            | Dependencies         | string             | 依赖项（逗号分隔）Dependencies (comma-separated) |
| Min Host Version        | MinHostVersion       | string             | 最小主机版本 Minimum host version        |
| Max Host Version        | MaxHostVersion       | string             | 最大主机版本 Maximum host version        |
| Format                  | Format               | string             | 文件格式 File format                     |
| Release Date            | ReleaseDate          | DateTime           | 发布日期 Release date                    |
| Pre-release             | IsPreRelease         | boolean            | 是否预发布 Is pre-release                |
| Platform                | Platform             | object             | 平台信息 Platform information            |
| Custom Properties       | CustomProperties     | object             | 自定义属性字典 Custom properties dictionary |

---

## 最佳实践 / Best Practices

### 命名规范 / Naming Conventions

1. **扩展名称 Extension Name**：
   - 使用小写字母和连字符 Use lowercase letters and hyphens
   - 避免空格和特殊字符 Avoid spaces and special characters
   - 保持简洁且具有描述性 Keep it concise and descriptive
   - ✓ 正确 Correct：`data-exporter`、`theme-customizer`
   - ✗ 错误 Incorrect：`Data Exporter`、`Theme@Customizer`

2. **版本号 Version Number**：
   - 遵循语义化版本规范 Follow Semantic Versioning
   - 格式：`主版本.次版本.修订号.构建号` Format: `Major.Minor.Patch.Build`
   - 示例 Example：`1.0.0.0`、`2.1.3.5`

---

### 目录组织 / Directory Organization

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

### 版本兼容性管理 / Version Compatibility Management

1. **设置合理的版本范围 Set Reasonable Version Range**：
   - 不要设置过窄的版本范围 Don't set too narrow version range
   - 考虑向后兼容性 Consider backward compatibility
   - 示例 Example：Min `1.0.0`、Max `2.0.0` (支持 1.x 所有版本 Support all 1.x versions)

2. **主版本更新 Major Version Updates**：
   - 主机应用程序发生重大变化时，更新 Min/Max Host Version When major changes occur in the host application, update Min/Max Host Version
   - 为新的主版本创建新的扩展版本 Create new extension versions for new major versions

---

### 依赖管理 / Dependency Management

1. **最小化依赖 Minimize Dependencies**：
   - 只声明必需的依赖项 Only declare necessary dependencies
   - 避免循环依赖 Avoid circular dependencies

2. **文档化依赖 Document Dependencies**：
   - 在描述中说明为什么需要这些依赖 Explain in the description why these dependencies are needed
   - 提供依赖项的获取方式 Provide ways to obtain dependencies

---

### 自定义属性使用 / Custom Properties Usage

1. **使用场景 Use Cases**：
   - 存储扩展特定的配置 Store extension-specific configurations
   - 记录扩展的技术要求 Record technical requirements of the extension
   - 传递初始化参数 Pass initialization parameters

2. **命名建议 Naming Recommendations**：
   - 使用驼峰命名法 Use camelCase
   - 保持键名简洁明了 Keep key names concise and clear
   - 添加前缀避免冲突 Add prefixes to avoid conflicts

---

### 测试和验证 / Testing and Validation

1. **打包前检查 Pre-packaging Checks**：
   - ✓ 验证所有文件是否完整 Verify all files are complete
   - ✓ 检查版本号是否正确 Check version number is correct
   - ✓ 确认依赖项是否准确 Confirm dependencies are accurate
   - ✓ 测试扩展在目标平台上的运行 Test extension runs on target platform

2. **打包后验证 Post-packaging Verification**：
   - ✓ 解压查看文件结构 Extract and view file structure
   - ✓ 检查 manifest.json 内容 Check manifest.json content
   - ✓ 验证文件大小是否合理 Verify file size is reasonable
   - ✓ 在目标环境中安装测试 Install and test in target environment

---

### 发布管理 / Release Management

1. **预发布版本 Pre-release Versions**：
   - 使用预发布标记进行内部测试 Use pre-release flag for internal testing
   - 收集反馈后再发布正式版本 Collect feedback before releasing stable version

2. **版本更新 Version Updates**：
   - 保持版本历史记录 Maintain version history
   - 在描述中记录更新日志 Record changelog in description
   - 提供从旧版本升级的指导 Provide upgrade guidance from old versions

---

## 故障排除 / Troubleshooting

### 常见问题 / Common Issues

#### 问题 1：构建失败 / Issue 1: Build Failed

**可能原因 Possible Causes**：
- 扩展目录不存在或无法访问 Extension directory does not exist or is inaccessible
- 导出路径无写入权限 No write permission for export path
- 磁盘空间不足 Insufficient disk space
- 扩展目录为空 Extension directory is empty

**解决方案 Solutions**：
1. 验证扩展目录路径是否正确 Verify the extension directory path is correct
2. 检查导出路径的写入权限 Check write permissions for the export path
3. 确保有足够的磁盘空间 Ensure sufficient disk space
4. 确认扩展目录包含文件 Confirm the extension directory contains files

---

#### 问题 2：验证错误 / Issue 2: Validation Error

**可能原因 Possible Causes**：
- 必填字段未填写 Required fields not filled
- 版本号格式不正确 Version number format is incorrect
- 路径格式无效 Invalid path format

**解决方案 Solutions**：
1. 检查所有必填字段（名称、版本、目录、导出路径）Check all required fields (name, version, directory, export path)
2. 使用正确的版本号格式（如 `1.0.0.0`）Use correct version number format (e.g., `1.0.0.0`)
3. 确保路径使用正确的格式 Ensure paths use correct format

---

#### 问题 3：生成的包无法解压 / Issue 3: Generated Package Cannot Be Extracted

**可能原因 Possible Causes**：
- 打包过程被中断 Packaging process was interrupted
- 磁盘空间在打包过程中耗尽 Disk space exhausted during packaging
- 源文件被锁定或正在使用 Source files are locked or in use

**解决方案 Solutions**：
1. 重新生成扩展包 Regenerate the extension package
2. 确保有足够的磁盘空间 Ensure sufficient disk space
3. 关闭正在使用扩展文件的程序 Close programs using extension files

---

#### 问题 4：自定义属性无法添加 / Issue 4: Cannot Add Custom Properties

**可能原因 Possible Causes**：
- 属性键或值为空 Property key or value is empty
- 属性键已存在 Property key already exists

**解决方案 Solutions**：
1. 确保键和值都已填写 Ensure both key and value are filled
2. 使用唯一的属性键名 Use unique property key names
3. 如需修改现有属性，先删除再重新添加 To modify existing properties, delete and re-add

---

## 相关文档 / Related Documentation

### GeneralUpdate 生态系统 / GeneralUpdate Ecosystem

- **[GeneralUpdate.Core](./GeneralUpdate.Core.md)** - 核心更新组件 Core update component
- **[GeneralUpdate.Extension](./GeneralUpdate.Extension.md)** - 扩展管理系统 Extension management system
- **[GeneralUpdate.ClientCore](./GeneralUpdate.ClientCore.md)** - 客户端更新组件 Client update component
- **[快速入门指南](../quickstart/Quik%20start.md)** - GeneralUpdate 快速入门 GeneralUpdate quick start

### 外部资源 / External Resources

- **[GeneralUpdate.Tools 源代码](https://github.com/GeneralLibrary/GeneralUpdate.Tools)** - GitHub 仓库 GitHub repository
- **[GeneralUpdate 主项目](https://github.com/GeneralLibrary/GeneralUpdate)** - 主框架项目 Main framework project
- **[问题反馈](https://github.com/GeneralLibrary/GeneralUpdate.Tools/issues)** - 报告问题和建议 Report issues and suggestions

---

## 适用于 / Applicable To

| 产品 Product      | 版本 Versions                         |
| ----------------- | ------------------------------------- |
| .NET              | 5, 6, 7, 8, 9, 10                     |
| .NET Framework    | 4.6.1+                                |
| .NET Standard     | 2.0                                   |
| .NET Core         | 2.0+                                  |
| Windows           | 10+                                   |
| Linux             | Ubuntu 20.04+, Debian 10+, Fedora 35+ |
| macOS             | 10.15+ (Catalina and later)           |