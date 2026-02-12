<div align="center">

# GeneralUpdate-Samples

**Usage Examples and Quick Start Repository for GeneralUpdate**

[![GitHub](https://img.shields.io/badge/GitHub-GeneralUpdate--Samples-blue?logo=github)](https://github.com/GeneralLibrary/GeneralUpdate-Samples)
[![Gitee](https://img.shields.io/badge/Gitee-GeneralUpdate--Samples-red?logo=gitee)](https://gitee.com/GeneralLibrary/GeneralUpdate-Samples)
[![License](https://img.shields.io/badge/License-Apache%202.0-green.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)

[English](README.md) | [中文文档](README_zh.md)

</div>

---

## 📖 Table of Contents

- [Overview](#-overview)
- [Key Features](#-key-features)
- [Repository Structure](#-repository-structure)
- [Quick Start](#-quick-start)
- [Sample Projects](#-sample-projects)
- [UI Framework Samples](#-ui-framework-samples)
- [Related Repositories](#-related-repositories)
- [Documentation](#-documentation)
- [Requirements](#-requirements)
- [Contributing](#-contributing)
- [Support](#-support)
- [License](#-license)

---

## 🌟 Overview

**GeneralUpdate-Samples** is the official sample repository for the [GeneralUpdate](https://github.com/GeneralLibrary/GeneralUpdate) project, which is an open-source cross-platform application automatic update component based on .NET Standard 2.0. This repository provides comprehensive code examples, demonstrations, and quick start guides to help developers understand and integrate automatic update functionality into their applications.

**GeneralUpdate** supports various update mechanisms including:
- ✅ Resume Download
- ✅ Version-by-Version Update
- ✅ Binary Differential Update
- ✅ Incremental Update
- ✅ Forced Update
- ✅ Multi-Branch Update
- ✅ OSS (Object Storage Service) Updates
- ✅ Rollback and Backup
- ✅ AOT (Ahead-of-Time) Compilation Support

---

## 🎯 Key Features

This repository provides working examples for all major features of GeneralUpdate:

| Feature | Description | Sample Location |
|---------|-------------|----------------|
| **Standard Update** | Traditional client-server update mechanism | `src/Client`, `src/Server`, `src/Upgrade` |
| **OSS Update** | Simplified update using file server and version.json | `src/OSS` |
| **Differential Update** | Binary patch generation and application | `src/Diff` |
| **Process Monitoring** | Crash detection and diagnostic information export | `src/Bowl` |
| **Push Updates** | Real-time update notifications using SignalR | `src/Push` |
| **Compression** | Compression capability testing and debugging | `src/Compress` |
| **Driver Update** | Driver package update capabilities | `src/Drivelution` |
| **Extension** | Custom extension development examples | `src/Extension` |

---

## 📁 Repository Structure

```
GeneralUpdate-Samples/
├── src/                          # Source code and samples
│   ├── Client/                   # Main client application sample
│   ├── Server/                   # Server application (Minimal API)
│   ├── Upgrade/                  # Upgrade assistant sample
│   ├── OSS/                      # OSS update samples
│   │   ├── OSSClientSample/      # OSS client sample
│   │   └── OSSUpgradeSample/     # OSS upgrade sample
│   ├── Bowl/                     # Process crash monitoring sample
│   ├── Diff/                     # Differential patch generation sample
│   ├── Compress/                 # Compression capability sample
│   ├── Push/                     # Update push notification sample
│   ├── Drivelution/              # Driver update sample
│   ├── Extension/                # Extension development sample
│   ├── start.cmd                 # Standard update demo launcher
│   └── oss_start.cmd             # OSS update demo launcher
├── UI/                           # UI framework integration samples
│   ├── AntdUI/                   # AntdUI framework sample
│   ├── LayUI/                    # LayUI framework sample
│   ├── SemiUrsa/                 # Semi Ursa (Avalonia) sample
│   └── WPFDevelopers/            # WPF framework sample
├── website/                      # Official website source code (Docusaurus)
├── imgs/                         # Documentation images
├── LICENSE                       # Apache 2.0 License
└── README.md                     # This file
```

---

## 🚀 Quick Start

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- Windows OS (for .cmd scripts) or cross-platform support with .NET

### Standard Update Demo

1. **Clone the repository**
   ```bash
   git clone https://github.com/GeneralLibrary/GeneralUpdate-Samples.git
   cd GeneralUpdate-Samples/src
   ```

2. **Run the standard update demo**
   ```cmd
   start.cmd
   ```

   This script will:
   - Build the Client, Server, and Upgrade projects
   - Copy compiled files to the `run` directory
   - Start the Server application
   - Start the Client application
   - Automatically trigger the update process

3. **Verify the update**
   - The Client will detect available updates from the Server
   - Download and apply the update package
   - Restart automatically after update completion
   - Check for the new file `Congratulations on the update.txt` in the run directory

### OSS Update Demo

For a simplified update mechanism without server-side code:

```cmd
cd src
oss_start.cmd
```

This demonstrates OSS-based updates using only a `version.json` configuration file on a file server.

---

## 📦 Sample Projects

### Client Sample (`src/Client`)

Demonstrates the main application that requires updating. Key features:
- Version detection and verification
- Update package download with progress tracking
- Event-based notification system
- Automatic restart after update

**Key Code:**
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

### Server Sample (`src/Server`)

Minimal API server providing update information and package distribution:
- Version verification endpoint
- Update package download endpoint
- Update status reporting

**Key Code:**
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

### Upgrade Sample (`src/Upgrade`)

Independent upgrade process that updates the main application:
- Downloads update packages
- Applies updates while main app is closed
- Restarts main application after update
- Provides rollback capability on failure

### OSS Update Sample (`src/OSS`)

Simplified update mechanism:
- No server-side code required
- Uses `version.json` configuration on file server
- Direct file server integration (Alibaba Cloud OSS, AWS S3, etc.)
- Supports AOT compilation

**version.json Example:**
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

### Bowl Sample (`src/Bowl`)

Process crash monitoring and diagnostic information export:
- Monitors main application health
- Captures crash dump files
- Exports system information
- Provides automatic rollback on crash

**Exports on Crash:**
- 📒 Dump file (.dmp)
- 📒 Version information (.json)
- 📒 Driver information (driverInfo.txt)
- 📒 System information (systeminfo.txt)
- 📒 Event logs (systemlog.evtx)

### Diff Sample (`src/Diff`)

Binary differential update implementation:
- Generates differential patches between versions
- Identifies changed, new, and deleted files
- Supports blacklist for excluded files
- Reduces update package size significantly

### Push Sample (`src/Push`)

Real-time update notification using SignalR:
- Push latest version information to clients
- Immediate update notifications
- Supports forced update triggers

---

## 🎨 UI Framework Samples

The `UI/` directory contains integration examples for various UI frameworks:

| Framework | Path | Description |
|-----------|------|-------------|
| **AntdUI** | `UI/AntdUI` | Modern UI components for Windows Forms |
| **LayUI** | `UI/LayUI` | WPF UI framework integration |
| **SemiUrsa** | `UI/SemiUrsa` | Avalonia UI framework (cross-platform) |
| **WPFDevelopers** | `UI/WPFDevelopers` | WPF with custom controls |

Each UI sample demonstrates how to integrate GeneralUpdate with specific UI frameworks and design patterns.

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

**Building the Documentation Website:**

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
- **.NET 8.0 Runtime** or later
- **Operating Systems**: Windows, Linux, macOS, Android (with MAUI)
- **Supported Platforms**: x64, ARM64, LoongArch

### Supported Frameworks
- .NET Core 2.0+
- .NET 5, 6, 7, 8+
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
- Follow existing code style and patterns
- Add samples for new features
- Update documentation as needed
- Test your changes thoroughly

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

