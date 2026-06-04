<div align="center">

# GeneralUpdate-Samples

**Usage Examples and Quick Start Repository for GeneralUpdate**

[![GitHub](https://img.shields.io/badge/GitHub-GeneralUpdate--Samples-blue?logo=github)](https://github.com/GeneralLibrary/GeneralUpdate-Samples)
[![Gitee](https://img.shields.io/badge/Gitee-GeneralUpdate--Samples-red?logo=gitee)](https://gitee.com/GeneralLibrary/GeneralUpdate-Samples)
[![License](https://img.shields.io/badge/License-Apache%202.0-green.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)

[English](README.md) | [中文文档](README_zh.md)

</div>

---

## 📖 Table of Contents

- [Overview](#-overview)
- [Key Features](#-key-features)
- [Repository Structure](#-repository-structure)
- [Quick Start](#-quick-start)
- [Interactive Sample Browser](#-interactive-sample-browser)
- [Sample Catalog](#-sample-catalog)
- [UI Framework Samples](#-ui-framework-samples)
- [Related Repositories](#-related-repositories)
- [Documentation](#-documentation)
- [Requirements](#-requirements)
- [Contributing](#-contributing)
- [Support](#-support)
- [License](#-license)

---

## 🌟 Overview

**GeneralUpdate-Samples** is the official sample repository for the [GeneralUpdate](https://github.com/GeneralLibrary/GeneralUpdate) project, an Apache 2.0 licensed, cross-platform application automatic update component built on .NET. This repository provides a unified **interactive sample browser (Hub)** that lets you explore and test every major feature through a clean console menu — no manual project setup required.

**GeneralUpdate** supports a wide range of update mechanisms:
- ✅ Resume Download
- ✅ Version-by-Version Update
- ✅ Binary Differential Update
- ✅ Incremental Update
- ✅ Forced Update
- ✅ Multi-Branch Update
- ✅ OSS (Object Storage Service) Updates
- ✅ Rollback and Backup
- ✅ AOT (Ahead-of-Time) Compilation Support
- ✅ Driver Update (via Drivelution)

---

## 🎯 Key Features

This repository provides self-contained, runnable samples for all major features of GeneralUpdate:

| Feature | Sample | Description |
|---------|--------|-------------|
| **Full Update** | `CompleteUpdateSample` | Version discovery → download → apply. The classic client-server upgrade flow |
| **Silent Update** | `SilentUpdateSample` | Background polling with pre-exit preparation for zero-user-interaction upgrades |
| **OSS Update** | `OssSample` | Simplified update using a file server and `versions.json` — no server-side app needed |
| **Binary Differential** | `DifferentialSample` | Generate and apply binary patches between versions to minimize download size |
| **Push Notification** | `PushSample` | Real-time update notifications powered by SignalR |
| **Process Guardian** | `BowlSample` | Crash detection, dump file export, and automatic recovery |
| **Plugin System** | `ExtensionSample` | Custom plugin installation, management, and compatibility checking |
| **Driver Update** | `ImDiskQuickInstallSample` | Driver package installation and update using ImDisk |
| **Compression** | `CompressSample` | Format-agnostic compression, decompression, and integrity verification |

---

## 📁 Repository Structure

```
GeneralUpdate-Samples/
├── src/                              # Source code and samples
│   ├── Hub/                          # Interactive sample browser (main entry point)
│   │   ├── Program.cs                # Menu loop, server lifecycle, sample discovery
│   │   ├── AppConfig.cs              # Configuration model (appsettings.json)
│   │   └── Samples/                  # Individual sample implementations
│   │       ├── ISample.cs            # Sample interface (auto-discovered by Hub)
│   │       ├── CompleteUpdateSample.cs
│   │       ├── SilentUpdateSample.cs
│   │       ├── OssSample.cs
│   │       ├── DifferentialSample.cs
│   │       ├── PushSample.cs
│   │       ├── BowlSample.cs
│   │       ├── ExtensionSample.cs
│   │       ├── ImDiskQuickInstallSample.cs
│   │       └── CompressSample.cs
│   ├── Server/                       # Minimal API server for update delivery
│   │   ├── Program.cs                # Verification, report & file download endpoints
│   │   ├── DTOs/                     # Request/response data transfer objects
│   │   └── wwwroot/packages/         # Update package storage
│   ├── content/                      # Intermediate version content (v1.0.0.1, v1.0.0.2)
│   ├── content_client/               # Client app base content (v1.0.0.0, v2.0.0.0)
│   ├── content_upgrade/              # Upgrade app base content (v1.0.0.0, v2.0.0.0)
│   ├── ImDiskDriver/                 # ImDisk driver binaries (for driver update demo)
│   ├── gen_packages.ps1              # Script to generate test update packages
│   ├── Run.cmd                       # Windows launcher (double-click to start)
│   ├── Run.ps1                       # PowerShell launcher with -BuildLibs support
│   └── GeneralUpdate-Samples.slnx    # Solution file (new .slnx format)
├── UI/                               # UI framework integration samples
│   ├── AntdUI/                       # AntdUI framework sample (WinForms)
│   ├── LayUI/                        # LayUI framework sample (WPF)
│   ├── SemiUrsa/                     # Semi Ursa sample (Avalonia, cross-platform)
│   └── WPFDevelopers/                # WPF with custom developer controls
├── website/                          # Official website source (Docusaurus)
├── .github/workflows/                # CI/CD pipelines
│   ├── dotnet.yml                    # .NET build workflow
│   └── az-swa-deploy.yml            # Azure Static Web App deployment (docs site)
├── imgs/                             # Documentation images
├── global.json                       # .NET SDK version pinning
├── Directory.Build.props             # Shared MSBuild properties
├── LICENSE                           # Apache 2.0 License
└── README.md                         # This file
```

---

## 🚀 Quick Start

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later
- Windows, Linux, or macOS (Windows recommended for driver and UI samples)

### One-Click Launch

1. **Clone the repository**
   ```bash
   git clone https://github.com/GeneralLibrary/GeneralUpdate-Samples.git
   cd GeneralUpdate-Samples/src
   ```

2. **Run the interactive sample browser**

   **On Windows (double-click or terminal):**
   ```cmd
   Run.cmd
   ```

   **On any platform (PowerShell):**
   ```powershell
   .\Run.ps1
   ```

   For contributors who need to rebuild component DLLs from source:
   ```powershell
   .\Run.ps1 -BuildLibs
   ```

3. **What happens next:**
   - The Hub builds and starts the Server automatically
   - An interactive menu appears with numbered samples
   - Select a sample by number to run it
   - The Hub manages server lifecycle: starts before each server-dependent sample, stops afterward
   - Type `0` or `q` to exit

### Generating Test Packages

Before running update-dependent samples for the first time, generate test packages:

```powershell
cd src
.\gen_packages.ps1
```

This creates update packages in `src/Server/wwwroot/packages/` with a `versions.json` manifest.

---

## 🖥️ Interactive Sample Browser

The **Hub** (`src/Hub/`) is the central entry point. Instead of launching separate projects manually, you interact with a single console menu:

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

### Architecture

- **Samples implement `ISample`** and are auto-discovered via reflection — no wiring needed
- **Server lifecycle is automatic**: `RequiresServer = true` samples get an isolated Server process started/stopped on demand
- **Clean paths** are reset before each run, ensuring reproducible demos
- **Configuration** is centralized in `src/Hub/appsettings.json`

### Key Code (CompleteUpdateSample)

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
    .AddListenerUpdateInfo((_, e) => { /* handle version discovery */ })
    .AddListenerMultiDownloadStatistics((_, e) => { /* handle progress */ })
    .AddListenerMultiDownloadCompleted((_, e) => { /* handle completion */ })
    .AddListenerException((_, e) => { /* handle errors */ });

await bootstrap.LaunchAsync();
```

---

## 📦 Sample Catalog

| # | Sample | Requires Server | What It Demonstrates |
|---|--------|:---:|----------------------|
| 1 | **Full Update** | ✅ | Complete update pipeline: version check → download → apply. Simulates a v1.0.0.0 app upgrading to v2.0.0.0 with file additions, modifications, and deletions |
| 2 | **Silent Update** | ✅ | Background polling with configurable intervals. Polls for updates without user interaction, prepares on app exit |
| 3 | **OSS Update** | ✅ | Object storage update mode using `versions.json` on a file server. Supports AOT compilation scenarios |
| 4 | **Binary Differential** | ❌ | Patch generation between two version folders. Identifies changed, new, and deleted files. Validates patch application with SHA256 |
| 5 | **SignalR Push** | ❌ | Real-time update push via a self-hosted SignalR Hub. Demonstrates client-server messaging for instant update notifications |
| 6 | **Process Guardian** | ❌ | Bowl integration: crash monitoring, automatic dump file export, system information collection, and driver enumeration |
| 7 | **Plugin System** | ❌ | Extension plugin installation lifecycle: download → extract → validate → install. Demonstrates compatibility checking and plugin management |
| 8 | **Driver Update** | ❌ | ImDisk virtual disk driver installation. Demonstrates GeneralUpdate.Drivelution for driver package deployment |
| 9 | **Compression** | ❌ | Format-agnostic compression and decompression. Tests data integrity with random content verification across all supported compression formats |

---

## 🎨 UI Framework Samples

The `UI/` directory contains integration examples for popular UI frameworks:

| Framework | Path | Type | Platform |
|-----------|------|------|----------|
| **AntdUI** | `UI/AntdUI/Upgrade/` | WinForms with modern Ant Design components | Windows |
| **LayUI** | `UI/LayUI/Upgrade/` | WPF with LayUI styling | Windows |
| **SemiUrsa** | `UI/SemiUrsa/` | Avalonia with Semi design system | Cross-platform |
| **WPFDevelopers** | `UI/WPFDevelopers/Upgrade/` | WPF with custom developer controls | Windows |

Each UI sample demonstrates how to integrate GeneralUpdate's upgrade workflow with specific UI frameworks and design patterns.

---

## 🔗 Related Repositories

The GeneralUpdate ecosystem consists of multiple repositories:

| Repository | Description | Links |
|------------|-------------|-------|
| **GeneralUpdate** | Core automatic update component | [GitHub](https://github.com/GeneralLibrary/GeneralUpdate) • [Gitee](https://gitee.com/GeneralLibrary/GeneralUpdate) • [GitCode](https://gitcode.com/GeneralLibrary/GeneralUpdate) |
| **GeneralUpdate.Maui** | MAUI updates (Android platform) | [GitHub](https://github.com/GeneralLibrary/GeneralUpdate.Maui) • [Gitee](https://gitee.com/GeneralLibrary/GeneralUpdate.Maui) |
| **GeneralUpdate.Tools** | Patch creation and packaging tools | [GitHub](https://github.com/GeneralLibrary/GeneralUpdate.Tools) • [Gitee](https://gitee.com/GeneralLibrary/GeneralUpdate.Tools) |
| **GeneralUpdate-Samples** | Usage examples (this repository) | [GitHub](https://github.com/GeneralLibrary/GeneralUpdate-Samples) • [Gitee](https://gitee.com/GeneralLibrary/GeneralUpdate-Samples) |

---

## 📚 Documentation

### Official Documentation
- 🌐 **Official Website**: [https://www.justerzhu.cn/](https://www.justerzhu.cn/)
- 📖 **Quick Start Guide**: [https://www.justerzhu.cn/docs/quickstart/quickstart](https://www.justerzhu.cn/docs/quickstart/quickstart)
- 🎥 **Video Tutorial**: [Bilibili](https://www.bilibili.com/video/BV1c8iyYZE7P)

### Documentation in this Repository

The `website/` directory contains the full documentation source built with [Docusaurus](https://docusaurus.io/):

```bash
cd website

# Install dependencies
npm install

# Start local development server
npm run start

# Build for production
npm run build
```

**Documentation Structure:**
- `website/docs/doc/` - Component documentation
- `website/docs/quickstart/` - Quick start guides
- `website/docs/guide/` - Advanced guides
- `website/docs/releaselog/` - Release notes
- `website/i18n/` - Internationalization (English, Chinese)

---

## 💻 Requirements

### Runtime Requirements
- **.NET 10.0 Runtime** or later
- **Operating Systems**: Windows, Linux, macOS, Android (with MAUI)
- **Supported Platforms**: x64, ARM64, LoongArch

### Supported Frameworks
- .NET 8.0+
- .NET Framework 4.6.1+

### Supported UI Frameworks
- WPF (Windows Presentation Foundation)
- WinForms (Windows Forms)
- Avalonia (Cross-platform)
- MAUI (Android)
- WinUI 3
- Console Applications

### Tested Operating Systems
- ✅ Windows 10/11
- ✅ Windows Server 2016+
- ✅ Linux (Ubuntu, Debian, CentOS)
- ✅ macOS (including M1/M2)
- ✅ Android (via MAUI)
- ✅ Kylin V10 (ARM and x64)
- ✅ UOS (Union Operating System)
- ✅ Huawei EulerOS
- ✅ Loongson (LoongArch)

---

## 🤝 Contributing

Contributions are welcome! Here's how you can help:

1. **Fork the repository**
2. **Create a feature branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```
3. **Commit your changes**
   ```bash
   git commit -am 'Add some feature'
   ```
4. **Push to the branch**
   ```bash
   git push origin feature/your-feature-name
   ```
5. **Open a Pull Request**

### Guidelines
- Add new samples by implementing `ISample` (see `src/Hub/Samples/ISample.cs`)
- Give your sample a unique `Index` number and descriptive `Name`
- Set `RequiresServer` appropriately
- Specify `CleanPaths` to ensure reproducible runs
- Follow existing code style and patterns
- Update documentation as needed

---

## 💬 Support

### Free Support
- 🐛 **Issues**: [GitHub Issues](https://github.com/GeneralLibrary/GeneralUpdate/issues)
- 💬 **Discussion Group**: QQ Group 748744489 (GeneralUpdate Discussion)
- 🛠️ **Technical Exchange**: QQ Group 341349660 (.NET Technical Exchange)

### Contact
- 📧 **Email**: zhuzhen723723@outlook.com
- 🌐 **Official Website**: [https://www.justerzhu.cn/](https://www.justerzhu.cn/)

### Commercial Support
For customized development, technical consultation, or sponsorship opportunities, please contact the author via email or official website.

---

## 📄 License

This project is licensed under the **Apache License 2.0** - see the [LICENSE](LICENSE) file for details.

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

## 🌟 Acknowledgments

Special thanks to all [contributors](https://github.com/GeneralLibrary/GeneralUpdate-Samples/graphs/contributors) and the community for their support and contributions to the GeneralUpdate project.

### Project Maintainer
- **Juster Zhu** - *Initial work and maintenance* - [GitHub](https://github.com/JusterZhu)

---

<div align="center">

**⭐ If you find this project useful, please consider giving it a star! ⭐**

**Updates limitless, upgrades boundless.**

</div>
