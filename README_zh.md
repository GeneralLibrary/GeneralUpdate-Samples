<div align="center">

# GeneralUpdate-Samples

**GeneralUpdate 的示例代码和快速入门仓库**

[![GitHub](https://img.shields.io/badge/GitHub-GeneralUpdate--Samples-blue?logo=github)](https://github.com/GeneralLibrary/GeneralUpdate-Samples)
[![Gitee](https://img.shields.io/badge/Gitee-GeneralUpdate--Samples-red?logo=gitee)](https://gitee.com/GeneralLibrary/GeneralUpdate-Samples)
[![License](https://img.shields.io/badge/License-Apache%202.0-green.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)

[English](README.md) | [中文文档](README_zh.md)

</div>

---

## 📖 目录

- [项目概述](#-项目概述)
- [核心功能](#-核心功能)
- [仓库结构](#-仓库结构)
- [快速开始](#-快速开始)
- [示例项目](#-示例项目)
- [UI框架示例](#-ui框架示例)
- [相关仓库](#-相关仓库)
- [文档](#-文档)
- [环境要求](#-环境要求)
- [贡献指南](#-贡献指南)
- [技术支持](#-技术支持)
- [开源协议](#-开源协议)

---

## 🌟 项目概述

**GeneralUpdate-Samples** 是 [GeneralUpdate](https://github.com/GeneralLibrary/GeneralUpdate) 项目的官方示例仓库。GeneralUpdate 是一款基于 .NET Standard 2.0 Apache 协议开源的跨平台应用程序自动升级组件。本仓库提供了全面的代码示例、演示和快速入门指南，帮助开发者理解并集成自动更新功能到他们的应用程序中。

**GeneralUpdate** 支持多种更新机制，包括：
- ✅ 断点续传
- ✅ 逐版本更新
- ✅ 二进制差分更新
- ✅ 增量更新
- ✅ 强制更新
- ✅ 多分支更新
- ✅ OSS（对象存储服务）更新
- ✅ 回滚与备份
- ✅ AOT（提前编译）支持

---

## 🎯 核心功能

本仓库为 GeneralUpdate 的所有主要功能提供了可运行的示例：

| 功能 | 描述 | 示例位置 |
|------|------|---------|
| **标准更新** | 传统的客户端-服务器更新机制 | `src/Client`、`src/Server`、`src/Upgrade` |
| **OSS更新** | 使用文件服务器和 version.json 的简化更新 | `src/OSS` |
| **差分更新** | 二进制补丁生成和应用 | `src/Diff` |
| **进程监控** | 崩溃检测和诊断信息导出 | `src/Bowl` |
| **推送更新** | 使用 SignalR 的实时更新通知 | `src/Push` |
| **压缩功能** | 压缩能力测试和调试 | `src/Compress` |
| **驱动更新** | 驱动包更新功能 | `src/Drivelution` |
| **扩展功能** | 自定义扩展开发示例 | `src/Extension` |

---

## 📁 仓库结构

```
GeneralUpdate-Samples/
├── src/                          # 源代码和示例
│   ├── Client/                   # 主客户端应用程序示例
│   ├── Server/                   # 服务器应用程序（Minimal API）
│   ├── Upgrade/                  # 升级助手示例
│   ├── OSS/                      # OSS 更新示例
│   │   ├── OSSClientSample/      # OSS 客户端示例
│   │   └── OSSUpgradeSample/     # OSS 升级示例
│   ├── Bowl/                     # 进程崩溃监控示例
│   ├── Diff/                     # 差分补丁生成示例
│   ├── Compress/                 # 压缩功能示例
│   ├── Push/                     # 更新推送通知示例
│   ├── Drivelution/              # 驱动更新示例
│   ├── Extension/                # 扩展开发示例
│   ├── start.cmd                 # 标准更新演示启动器
│   └── oss_start.cmd             # OSS 更新演示启动器
├── UI/                           # UI 框架集成示例
│   ├── AntdUI/                   # AntdUI 框架示例
│   ├── LayUI/                    # LayUI 框架示例
│   ├── SemiUrsa/                 # Semi Ursa (Avalonia) 示例
│   └── WPFDevelopers/            # WPF 框架示例
├── website/                      # 官方网站源代码 (Docusaurus)
├── imgs/                         # 文档图片
├── LICENSE                       # Apache 2.0 许可证
└── README.md                     # 英文说明文档
```

---

## 🚀 快速开始

### 环境准备

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) 或更高版本
- Windows 操作系统（用于 .cmd 脚本）或使用 .NET 的跨平台支持

### 标准更新演示

1. **克隆仓库**
   ```bash
   git clone https://github.com/GeneralLibrary/GeneralUpdate-Samples.git
   cd GeneralUpdate-Samples/src
   ```

2. **运行标准更新演示**
   ```cmd
   start.cmd
   ```

   此脚本将会：
   - 构建 Client、Server 和 Upgrade 项目
   - 将编译后的文件复制到 `run` 目录
   - 启动 Server 应用程序
   - 启动 Client 应用程序
   - 自动触发更新过程

3. **验证更新**
   - Client 将从 Server 检测可用更新
   - 下载并应用更新包
   - 更新完成后自动重启
   - 检查 run 目录中的新文件 `Congratulations on the update.txt`

### OSS 更新演示

体验无需服务器端代码的简化更新机制：

```cmd
cd src
oss_start.cmd
```

这演示了基于 OSS 的更新，仅使用文件服务器上的 `version.json` 配置文件。

---

## 📦 示例项目

### 客户端示例 (`src/Client`)

演示需要更新的主应用程序。主要功能：
- 版本检测和验证
- 带进度跟踪的更新包下载
- 基于事件的通知系统
- 更新后自动重启

**关键代码：**
```csharp
var configinfo = new Configinfo
{
    UpdateUrl = "http://127.0.0.1:5000/Upgrade/Verification",
    MainAppName = "ClientSample.exe",
    ClientVersion = "1.0.0.0",
    ProductId = "2d974e2a-31e6-4887-9bb1-b4689e98c77a"
};

await new GeneralClientBootstrap()
    .AddListenerMultiDownloadStatistics(OnMultiDownloadStatistics)
    .AddListenerMultiDownloadCompleted(OnMultiDownloadCompleted)
    .AddListenerException(OnException)
    .SetConfig(configinfo)
    .LaunchAsync();
```

### 服务器示例 (`src/Server`)

提供更新信息和包分发的 Minimal API 服务器：
- 版本验证端点
- 更新包下载端点
- 更新状态报告

**关键代码：**
```csharp
app.MapPost("/Upgrade/Verification", (VerifyDTO request) =>
{
    var result = new List<VerificationResultDTO>
    {
        new VerificationResultDTO
        {
            Version = "1.0.0.1",
            Url = "http://localhost:5000/packages/packet.zip",
            Hash = "...",
            Size = packet.Length
        }
    };
    return HttpResponseDTO<IEnumerable<VerificationResultDTO>>.Success(result);
});
```

### 升级示例 (`src/Upgrade`)

独立的升级进程，用于更新主应用程序：
- 下载更新包
- 在主应用关闭时应用更新
- 更新后重启主应用程序
- 失败时提供回滚能力

### OSS 更新示例 (`src/OSS`)

简化的更新机制：
- 无需服务器端代码
- 使用文件服务器上的 `version.json` 配置
- 直接集成文件服务器（阿里云 OSS、AWS S3 等）
- 支持 AOT 编译

**version.json 示例：**
```json
[
  {
    "PacketName": "packet_20250102230201638_1.0.0.1",
    "Hash": "ad1a85a9169ca0083ab54ba390e085c56b9059efc3ca8aa1ec9ed857683cc4b1",
    "Version": "1.0.0.1",
    "Url": "http://localhost:5000/packages/packet_20250102230201638_1.0.0.1.zip"
  }
]
```

### Bowl 示例 (`src/Bowl`)

进程崩溃监控和诊断信息导出：
- 监控主应用程序健康状态
- 捕获崩溃转储文件
- 导出系统信息
- 崩溃时提供自动回滚

**崩溃时导出的文件：**
- 📒 转储文件 (.dmp)
- 📒 版本信息 (.json)
- 📒 驱动信息 (driverInfo.txt)
- 📒 系统信息 (systeminfo.txt)
- 📒 事件日志 (systemlog.evtx)

### 差分示例 (`src/Diff`)

二进制差分更新实现：
- 生成版本间的差分补丁
- 识别修改、新增和删除的文件
- 支持排除文件的黑名单
- 显著减少更新包大小

### 推送示例 (`src/Push`)

使用 SignalR 的实时更新通知：
- 向客户端推送最新版本信息
- 即时更新通知
- 支持强制更新触发

---

## 🎨 UI框架示例

`UI/` 目录包含各种 UI 框架的集成示例：

| 框架 | 路径 | 描述 |
|------|------|------|
| **AntdUI** | `UI/AntdUI` | Windows Forms 的现代 UI 组件 |
| **LayUI** | `UI/LayUI` | WPF UI 框架集成 |
| **SemiUrsa** | `UI/SemiUrsa` | Avalonia UI 框架（跨平台） |
| **WPFDevelopers** | `UI/WPFDevelopers` | 带自定义控件的 WPF |

每个 UI 示例演示了如何将 GeneralUpdate 与特定的 UI 框架和设计模式集成。

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
- 📖 **快速入门指南**：[https://www.justerzhu.cn/docs/quickstart/quikstart](https://www.justerzhu.cn/docs/quickstart/quikstart)
- 🎥 **视频教程**：[哔哩哔哩](https://www.bilibili.com/video/BV1c8iyYZE7P)

### 本仓库中的文档

`website/` 目录包含使用 [Docusaurus](https://docusaurus.io/) 构建的完整文档源代码：

**构建文档网站：**

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
- **.NET 8.0 运行时**或更高版本
- **操作系统**：Windows、Linux、macOS、Android（使用 MAUI）
- **支持的平台**：x64、ARM64、LoongArch

### 支持的框架
- .NET Core 2.0+
- .NET 5、6、7、8+
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

欢迎贡献！以下是您可以提供帮助的方式：

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

### 指南
- 遵循现有的代码风格和模式
- 为新功能添加示例
- 根据需要更新文档
- 彻底测试您的更改

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
