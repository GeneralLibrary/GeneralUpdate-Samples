<div align="center">

# GeneralUpdate-Samples

**GeneralUpdate 的示例代码和快速入门仓库**

[![GitHub](https://img.shields.io/badge/GitHub-GeneralUpdate--Samples-blue?logo=github)](https://github.com/GeneralLibrary/GeneralUpdate-Samples)
[![Gitee](https://img.shields.io/badge/Gitee-GeneralUpdate--Samples-red?logo=gitee)](https://gitee.com/GeneralLibrary/GeneralUpdate-Samples)
[![License](https://img.shields.io/badge/License-Apache%202.0-green.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)

[English](README.md) | [中文文档](README_zh.md)

</div>

---

## 📖 目录

- [项目概述](#-项目概述)
- [核心功能](#-核心功能)
- [仓库结构](#-仓库结构)
- [快速开始](#-快速开始)
- [交互式示例浏览器](#-交互式示例浏览器)
- [示例目录](#-示例目录)
- [UI 框架示例](#-ui-框架示例)
- [相关仓库](#-相关仓库)
- [文档](#-文档)
- [环境要求](#-环境要求)
- [贡献指南](#-贡献指南)
- [技术支持](#-技术支持)
- [开源协议](#-开源协议)

---

## 🌟 项目概述

**GeneralUpdate-Samples** 是 [GeneralUpdate](https://github.com/GeneralLibrary/GeneralUpdate) 项目的官方示例仓库。GeneralUpdate 是一款基于 .NET Standard 2.0 Apache 协议开源的跨平台应用程序自动升级组件。本仓库提供了一个统一的**交互式示例浏览器（Hub）**，通过简洁的控制台菜单即可浏览和测试每个主要功能——无需手动配置多个项目。

**GeneralUpdate** 支持多种更新机制：
- ✅ 断点续传
- ✅ 逐版本更新
- ✅ 二进制差分更新
- ✅ 增量更新
- ✅ 强制更新
- ✅ 多分支更新
- ✅ OSS（对象存储服务）更新
- ✅ 回滚与备份
- ✅ AOT（提前编译）支持
- ✅ 驱动更新（通过 Drivelution）

---

## 🎯 核心功能

本仓库为 GeneralUpdate 的所有主要功能提供了可独立运行的示例：

| 功能 | 示例 | 描述 |
|------|------|------|
| **完整更新** | `CompleteUpdateSample` | 版本发现 → 下载 → 应用，经典的客户端-服务器升级流程 |
| **静默更新** | `SilentUpdateSample` | 后台轮询检查更新，支持退出前准备，实现零用户交互升级 |
| **OSS 更新** | `OssSample` | 使用文件服务器和 `versions.json` 的简化更新 —— 无需服务端应用程序 |
| **二进制差分** | `DifferentialSample` | 生成和应用版本间的二进制补丁，大幅减少下载量 |
| **推送通知** | `PushSample` | 基于 SignalR 的实时更新消息推送 |
| **进程守护** | `BowlSample` | 崩溃检测、Dump 文件导出和自动恢复 |
| **插件系统** | `ExtensionSample` | 自定义插件的安装、管理和兼容性检查 |
| **驱动更新** | `ImDiskQuickInstallSample` | 使用 ImDisk 进行驱动包的安装和更新 |
| **压缩工具** | `CompressSample` | 格式无关的压缩、解压和完整性校验 |

---

## 📁 仓库结构

```
GeneralUpdate-Samples/
├── src/                              # 源代码和示例
│   ├── Hub/                          # 交互式示例浏览器（主入口）
│   │   ├── Program.cs                # 菜单循环、Server 生命周期、示例发现
│   │   ├── AppConfig.cs              # 配置模型（appsettings.json）
│   │   └── Samples/                  # 各示例实现
│   │       ├── ISample.cs            # 示例接口（Hub 通过反射自动发现）
│   │       ├── CompleteUpdateSample.cs
│   │       ├── SilentUpdateSample.cs
│   │       ├── OssSample.cs
│   │       ├── DifferentialSample.cs
│   │       ├── PushSample.cs
│   │       ├── BowlSample.cs
│   │       ├── ExtensionSample.cs
│   │       ├── ImDiskQuickInstallSample.cs
│   │       └── CompressSample.cs
│   ├── Server/                       # Minimal API 更新服务器
│   │   ├── Program.cs                # 版本验证、状态上报和文件下载端点
│   │   ├── DTOs/                     # 请求/响应数据传输对象
│   │   └── wwwroot/packages/         # 更新包存储
│   ├── content/                      # 中间版本内容（v1.0.0.1, v1.0.0.2）
│   ├── content_client/               # 客户端基础内容（v1.0.0.0, v2.0.0.0）
│   ├── content_upgrade/              # 升级程序基础内容（v1.0.0.0, v2.0.0.0）
│   ├── ImDiskDriver/                 # ImDisk 驱动文件（用于驱动更新演示）
│   ├── gen_packages.ps1              # 生成测试更新包的脚本
│   ├── Run.cmd                       # Windows 启动器（双击即可运行）
│   ├── Run.ps1                       # PowerShell 启动器（支持 -BuildLibs）
│   └── GeneralUpdate-Samples.slnx    # 解决方案文件（新 .slnx 格式）
├── UI/                               # UI 框架集成示例
│   ├── AntdUI/                       # AntdUI 框架示例（WinForms）
│   ├── LayUI/                        # LayUI 框架示例（WPF）
│   ├── SemiUrsa/                     # Semi Ursa 示例（Avalonia，跨平台）
│   └── WPFDevelopers/                # WPF 开发者自定义控件示例
├── website/                          # 官方网站源码（Docusaurus）
├── .github/workflows/                # CI/CD 工作流
│   ├── dotnet.yml                    # .NET 构建工作流
│   └── az-swa-deploy.yml            # Azure Static Web App 部署（文档站点）
├── imgs/                             # 文档图片
├── global.json                       # .NET SDK 版本锁定
├── Directory.Build.props             # 共享 MSBuild 属性
├── LICENSE                           # Apache 2.0 许可证
└── README.md                         # 英文说明文档
```

---

## 🚀 快速开始

### 环境准备

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) 或更高版本
- Windows、Linux 或 macOS（驱动和 UI 示例建议使用 Windows）

### 一键启动

1. **克隆仓库**
   ```bash
   git clone https://github.com/GeneralLibrary/GeneralUpdate-Samples.git
   cd GeneralUpdate-Samples/src
   ```

2. **运行交互式示例浏览器**

   **Windows（双击或在终端中运行）：**
   ```cmd
   Run.cmd
   ```

   **任何平台（PowerShell）：**
   ```powershell
   .\Run.ps1
   ```

   如需从源码重新编译组件 DLL：
   ```powershell
   .\Run.ps1 -BuildLibs
   ```

3. **启动后：**
   - Hub 会自动构建并启动 Server
   - 显示带编号的交互式菜单
   - 输入数字选择要运行的示例
   - Hub 自动管理 Server 生命周期：依赖 Server 的示例运行前自动启动，结束后自动关闭
   - 输入 `0` 或 `q` 退出

### 生成测试更新包

首次运行需要 Server 的示例前，请先生成测试包：

```powershell
cd src
.\gen_packages.ps1
```

这将在 `src/Server/wwwroot/packages/` 中创建更新包和 `versions.json` 清单文件。

---

## 🖥️ 交互式示例浏览器

**Hub**（`src/Hub/`）是统一入口。无需手动启动多个项目，只需在一个控制台菜单中操作：

```
  ╔══════════════════════════════════════╗
  ║    GeneralUpdate 示例浏览器          ║
  ╚══════════════════════════════════════╝

  ═══════════════════════════════════
    1. 完整更新 — 版本发现→下载→应用
    2. 静默更新 — 后台轮询·退出前准备
    3. OSS 模式 — 对象存储更新
    4. 二进制差分 — 生成·应用·校验
    5. SignalR 推送 — 实时消息接收
    6. 进程守护 — 崩溃监控·Dump导出
    7. 插件系统 — 安装·管理·兼容性
    8. Driver Update
    9. 压缩工具 — 压缩·解压·校验
  ───────────────────────────────────
    0. 退出 (Exit)
  ═══════════════════════════════════
```

### 架构设计

- **示例实现 `ISample` 接口**，Hub 通过反射自动发现 —— 无需手动注册
- **Server 生命周期自动化**：`RequiresServer = true` 的示例会按需获得独立的 Server 进程
- **运行前自动清理**：每个示例运行前重置 `CleanPaths` 中指定的路径，确保演示可重现
- **配置集中管理**：所有配置统一在 `src/Hub/appsettings.json` 中

### 关键代码（完整更新示例）

```csharp
var request = new UpdateRequest
{
    UpdateUrl = $"{config.ServerUrl}/Upgrade/Verification",
    ReportUrl = $"{config.ServerUrl}/Upgrade/Report",
    AppSecretKey = config.AppSecretKey,
    InstallPath = mockAppDir,
    ClientVersion = config.ClientVersion,
    MainAppName = config.MainAppName,
    ProductId = config.ProductId
};

var bootstrap = new GeneralUpdateBootstrap()
    .SetConfig(request)
    .SetOption(Option.AppType, AppType.Client)
    .AddListenerUpdateInfo((_, e) => { /* 处理版本发现 */ })
    .AddListenerMultiDownloadStatistics((_, e) => { /* 处理下载进度 */ })
    .AddListenerMultiDownloadCompleted((_, e) => { /* 处理下载完成 */ })
    .AddListenerException((_, e) => { /* 处理异常 */ });

await bootstrap.LaunchAsync();
```

---

## 📦 示例目录

| 编号 | 示例 | 需要 Server | 说明 |
|------|------|:---:|------|
| 1 | **完整更新** | ✅ | 完整更新管线：版本检查 → 下载 → 应用。模拟 v1.0.0.0 应用升级到 v2.0.0.0，包含文件增删改 |
| 2 | **静默更新** | ✅ | 后台轮询检查更新，支持自定义间隔。无需用户交互，退出时自动准备更新 |
| 3 | **OSS 更新** | ✅ | 对象存储更新模式，使用文件服务器上的 `versions.json`。支持 AOT 编译场景 |
| 4 | **二进制差分** | ❌ | 两个版本目录间的补丁生成。识别变更、新增和删除的文件。使用 SHA256 验证补丁应用 |
| 5 | **SignalR 推送** | ❌ | 通过自托管 SignalR Hub 实现实时更新推送。演示客户端-服务器消息通信 |
| 6 | **进程守护** | ❌ | Bowl 集成：崩溃监控、自动 Dump 文件导出、系统信息收集和驱动枚举 |
| 7 | **插件系统** | ❌ | 扩展插件安装周期：下载 → 解压 → 验证 → 安装。演示兼容性检查和插件管理 |
| 8 | **驱动更新** | ❌ | ImDisk 虚拟磁盘驱动安装。演示 GeneralUpdate.Drivelution 驱动包部署 |
| 9 | **压缩工具** | ❌ | 格式无关的压缩和解压。使用随机内容验证所有支持压缩格式的数据完整性 |

---

## 🎨 UI 框架示例

`UI/` 目录包含主流 UI 框架的集成示例：

| 框架 | 路径 | 类型 | 平台 |
|------|------|------|------|
| **AntdUI** | `UI/AntdUI/Upgrade/` | WinForms + Ant Design 组件 | Windows |
| **LayUI** | `UI/LayUI/Upgrade/` | WPF + LayUI 样式 | Windows |
| **SemiUrsa** | `UI/SemiUrsa/` | Avalonia + Semi 设计系统 | 跨平台 |
| **WPFDevelopers** | `UI/WPFDevelopers/Upgrade/` | WPF + 自定义开发者控件 | Windows |

每个 UI 示例演示了如何将 GeneralUpdate 的升级工作流与特定的 UI 框架和设计模式集成。

---

## 🔗 相关仓库

GeneralUpdate 生态系统由多个仓库组成：

| 仓库 | 描述 | 链接 |
|------|------|------|
| **GeneralUpdate** | 核心自动更新组件 | [GitHub](https://github.com/GeneralLibrary/GeneralUpdate) • [Gitee](https://gitee.com/GeneralLibrary/GeneralUpdate) • [GitCode](https://gitcode.com/GeneralLibrary/GeneralUpdate) |
| **GeneralUpdate.Maui** | MAUI 更新（Android 平台） | [GitHub](https://github.com/GeneralLibrary/GeneralUpdate.Maui) • [Gitee](https://gitee.com/GeneralLibrary/GeneralUpdate.Maui) |
| **GeneralUpdate.Tools** | 补丁创建和打包工具 | [GitHub](https://github.com/GeneralLibrary/GeneralUpdate.Tools) • [Gitee](https://gitee.com/GeneralLibrary/GeneralUpdate.Tools) |
| **GeneralUpdate-Samples** | 使用示例（本仓库） | [GitHub](https://github.com/GeneralLibrary/GeneralUpdate-Samples) • [Gitee](https://gitee.com/GeneralLibrary/GeneralUpdate-Samples) |

---

## 📚 文档

### 官方文档
- 🌐 **官方网站**：[https://www.justerzhu.cn/](https://www.justerzhu.cn/)
- 📖 **快速入门指南**：[https://www.justerzhu.cn/docs/quickstart/quickstart](https://www.justerzhu.cn/docs/quickstart/quickstart)
- 🎥 **视频教程**：[哔哩哔哩](https://www.bilibili.com/video/BV1c8iyYZE7P)

### 本仓库中的文档

`website/` 目录包含使用 [Docusaurus](https://docusaurus.io/) 构建的完整文档源代码：

```bash
cd website

# 安装依赖
npm install

# 启动本地开发服务器
npm run start

# 构建生产版本
npm run build
```

**文档结构：**
- `website/docs/doc/` - 组件文档
- `website/docs/quickstart/` - 快速入门指南
- `website/docs/guide/` - 高级指南
- `website/docs/releaselog/` - 发布说明
- `website/i18n/` - 国际化（英文、中文）

---

## 💻 环境要求

### 运行时要求
- **.NET 10.0 运行时** 或更高版本
- **操作系统**：Windows、Linux、macOS、Android（使用 MAUI）
- **支持的平台**：x64、ARM64、LoongArch

### 支持的框架
- .NET 8.0+
- .NET Framework 4.6.1+

### 支持的 UI 框架
- WPF (Windows Presentation Foundation)
- WinForms (Windows Forms)
- Avalonia（跨平台）
- MAUI（Android）
- WinUI 3
- 控制台应用程序

### 已测试的操作系统
- ✅ Windows 10/11
- ✅ Windows Server 2016+
- ✅ Linux（Ubuntu、Debian、CentOS）
- ✅ macOS（包括 M1/M2）
- ✅ Android（通过 MAUI）
- ✅ 麒麟 V10（ARM 和 x64）
- ✅ UOS（统信操作系统）
- ✅ 华为 EulerOS
- ✅ 龙芯（LoongArch）

---

## 🤝 贡献指南

欢迎贡献！以下是参与方式：

1. **Fork 仓库**
2. **创建功能分支**
   ```bash
   git checkout -b feature/your-feature-name
   ```
3. **提交您的更改**
   ```bash
   git commit -am 'Add some feature'
   ```
4. **推送到分支**
   ```bash
   git push origin feature/your-feature-name
   ```
5. **开启 Pull Request**

### 贡献指南
- 通过实现 `ISample` 接口添加新示例（参见 `src/Hub/Samples/ISample.cs`）
- 为示例分配唯一的 `Index` 编号和描述性的 `Name`
- 正确设置 `RequiresServer` 属性
- 指定 `CleanPaths` 以确保每次运行可重现
- 遵循现有的代码风格和模式
- 根据需要更新文档

---

## 💬 技术支持

### 免费支持
- 🐛 **问题反馈**：[GitHub Issues](https://github.com/GeneralLibrary/GeneralUpdate/issues)
- 💬 **讨论组**：QQ 群 748744489（GeneralUpdate 讨论群）
- 🛠️ **技术交流**：QQ 群 341349660（.NET 技术交流群）

### 联系方式
- 📧 **电子邮件**：zhuzhen723723@outlook.com
- 🌐 **官方网站**：[https://www.justerzhu.cn/](https://www.justerzhu.cn/)

### 商业支持
如需定制开发、技术咨询或赞助合作，请通过电子邮件或官方网站联系作者。

---

## 📄 开源协议

本项目采用 **Apache License 2.0** 协议 - 详见 [LICENSE](LICENSE) 文件。

```
Copyright 2025 Juster Zhu

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
```

---

## 🌟 致谢

特别感谢所有[贡献者](https://github.com/GeneralLibrary/GeneralUpdate-Samples/graphs/contributors)和社区对 GeneralUpdate 项目的支持和贡献。

### 项目维护者
- **Juster Zhu** - *初始工作和维护* - [GitHub](https://github.com/JusterZhu)

---

<div align="center">

**⭐ 如果您觉得这个项目有用，请考虑给它一个 star！⭐**

**更新无限，升级无界。**

</div>
