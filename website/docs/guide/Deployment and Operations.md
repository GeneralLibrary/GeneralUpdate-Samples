---
sidebar_position: 1
---

# 部署与运维指南

本文档涵盖应用程序打包部署、平台适配、故障排查和运维诊断。

---

## 一、补丁包生成

### 使用 GeneralUpdate.Tools (推荐)

[GeneralUpdate.Tools](https://github.com/GeneralLibrary/GeneralUpdate.Tools) 是最推荐的补丁包生成工具，提供可视化界面和完整的验证能力。

**操作步骤：**

1. 下载并启动 GeneralUpdate.Tools
2. 切换到「补丁包」选项卡
3. 选择旧版本目录（源路径）
4. 选择新版本目录（目标路径）
5. 设置补丁包输出路径
6. 填写包名和版本号
7. 点击「构建」

详细说明请参考 [GeneralUpdate.Tools 文档](../quickstart/GeneralUpdate.PacketTool)。

### 使用 Differential API 编程生成

```csharp
using GeneralUpdate.Differential;

// 生成增量补丁
var sourcePath = @"C:\Builds\MyApp_v1.0.0";
var targetPath = @"C:\Builds\MyApp_v1.0.1";
var patchPath  = @"C:\Builds\patches\v1.0.1";

await DifferentialCore.Instance.Clean(sourcePath, targetPath, patchPath);

// 将 patchPath 目录打包为 .zip 上传到服务端
```

---

## 二、客户端打包

### 文件结构

客户端安装目录推荐的结构：

```
MyApp/
├── MyApp.exe                    ← 主程序
├── MyApp.Core.dll               ← 核心库
├── UpgradeSample.exe            ← 升级助手（与主程序同级）
├── GeneralUpdate.Core.dll       ← 升级助手依赖
├── GeneralUpdate.ClientCore.dll ← 客户端更新组件
├── GeneralUpdate.Differential.dll
├── generalupdate.manifest.json  ← 自动生成的清单文件
├── appsettings.json             ← 应用配置（建议加入黑名单）
└── resources/
    └── ...
```

### 客户端引用

```xml
<!-- 主程序 .csproj -->
<PackageReference Include="GeneralUpdate.ClientCore" Version="10.*" />
```

```xml
<!-- 升级助手 .csproj -->
<PackageReference Include="GeneralUpdate.Core" Version="10.*" />
```

### 升级助手独立项目

升级助手（Upgrade）必须是一个**独立的可执行项目**，与主程序分开编译：

```
MySolution/
├── src/
│   ├── MyApp/                    ← 主程序项目
│   │   ├── MyApp.csproj
│   │   └── Program.cs
│   └── MyApp.Upgrade/            ← 升级助手项目
│       ├── MyApp.Upgrade.csproj
│       └── Program.cs
└── MySolution.sln
```

**升级助手 Program.cs 最小实现：**

```csharp
using GeneralUpdate.Core;

await new GeneralUpdateBootstrap()
    .AddListenerException((_, args) =>
    {
        Console.WriteLine($"升级异常: {args.Exception.Message}");
    })
    .LaunchAsync();
```

---

## 三、平台特定打包

### Windows

**使用 NSIS 创建安装程序：**

- [NSIS 官网](https://nsis.sourceforge.io/Download)
- NSIS 脚本负责：创建安装目录、复制文件、注册快捷方式
- 将主程序和升级助手放在同一目录下
- 确保安装目录对当前用户有写入权限

**避免 C 盘权限问题：**

- 建议默认安装到 `%LOCALAPPDATA%` 而非 `C:\Program Files\`
- 如必须安装在 C 盘，参考下方 [Windows 权限处理](#五windows-权限处理) 章节

**Windows 发布命令：**

```bash
dotnet publish src/MyApp/MyApp.csproj -c Release -r win-x64 --self-contained
dotnet publish src/MyApp.Upgrade/MyApp.Upgrade.csproj -c Release -r win-x64 --self-contained
```

### Linux

**使用 .deb 包 (Debian/Ubuntu)：**

参考 [Avalonia 部署文档](https://docs.avaloniaui.net/docs/deployment/debian-ubuntu)

**使用 AppImage：**

通用的 Linux 可移植打包格式，无需安装即可运行。

**Linux 权限处理：**

```csharp
// 在升级助手中使用 UnixPermissionHooks
await new GeneralUpdateBootstrap()
    .Hooks<UnixPermissionHooks>()
    .LaunchAsync();
```

或使用自定义脚本：

```csharp
var config = new Configinfo
{
    Script = "/bin/bash chmod -R +x $InstallPath",
    // ...
};
```

**Linux 发布命令：**

```bash
dotnet publish src/MyApp/MyApp.csproj -c Release -r linux-x64 --self-contained
dotnet publish src/MyApp.Upgrade/MyApp.Upgrade.csproj -c Release -r linux-x64 --self-contained
```

### macOS

**使用 .app Bundle：**

macOS 应用程序通常打包为 `.app` 目录结构。确保升级助手在主程序的 `Contents/MacOS/` 目录中。

**代码签名：**

macOS 要求应用程序经过代码签名才能正常运行（尤其是 Apple Silicon 设备）：

```bash
codesign --deep --force --verify --verbose --sign "Developer ID" MyApp.app
```

**公证 (Notarization)：**

分发到 macOS 前，建议提交 Apple 公证：

```bash
xcrun notarytool submit MyApp.dmg --apple-id your@email.com --wait
```

**macOS 发布命令：**

```bash
dotnet publish src/MyApp/MyApp.csproj -c Release -r osx-x64 --self-contained
dotnet publish src/MyApp.Upgrade/MyApp.Upgrade.csproj -c Release -r osx-x64 --self-contained
```

---

## 四、CI/CD 集成

### GitHub Actions 示例

```yaml
name: Build and Package

on:
  push:
    tags:
      - 'v*'

jobs:
  build:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      
      - name: Publish Client
        run: dotnet publish src/MyApp/MyApp.csproj -c Release -r win-x64 -o publish/MyApp
      
      - name: Publish Upgrade
        run: dotnet publish src/MyApp.Upgrade/MyApp.Upgrade.csproj -c Release -r win-x64 -o publish/MyApp
      
      - name: Generate Patch
        run: |
          # 使用 GeneralUpdate.Tools CLI 或其他脚本生成补丁包
      
      - name: Upload Artifacts
        uses: actions/upload-artifact@v4
        with:
          name: release-package
          path: publish/
```

---

## 五、Windows 权限处理

### UAC 与安装目录

![](imgs\UAC.png)

使用 GeneralUpdate 进行自动更新时，如果更新目录位于 C 盘，尤其在替换文件或应用补丁时可能会遇到权限问题。Windows 11 对 C 盘某些目录的权限管理比以往更加严格。

以下目录可能触发权限问题：

| 名称 | 目录 |
| --- | --- |
| 系统文件夹 | C:\Windows |
| 注册表配置 | C:\Windows\System32\config |
| 驱动文件夹 | C:\Windows\System32\drivers |
| 程序文件夹 | C:\Program Files 和 C:\Program Files (x86) |

推荐的避免权限问题的目录：

| 名称 | 目录 |
| --- | --- |
| 用户数据目录 | AppData |
| 系统临时目录 | Temp |

### 降低 UAC 级别

> **警告：** 以下方法不建议在生产环境中使用，可能对用户造成安全风险。

如果在更新过程中遇到 UAC 弹窗或权限拒绝问题，可考虑通过修改注册表降低 UAC 控制级别：

| 注册表项 | 新值 | 默认值 |
| --- | --- | --- |
| enableLUA | 0 | 1 |
| ConsentPromptBehaviorAdmin | 0 | 5 |

更新前修改以上注册表（重启电脑后生效），更新完成后务必恢复原值。

**C# 修改注册表：**

```csharp
using Microsoft.Win32;

public void UpdateRegistry()
{
    const string keyName = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System";

    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyName, true))
    {
        if (key != null)
        {
            key.SetValue("EnableLUA", 0, RegistryValueKind.DWord);
            key.SetValue("ConsentPromptBehaviorAdmin", 0, RegistryValueKind.DWord);
        }
    }
}
```

**批处理脚本修改注册表：**

```bat
@echo off
REG ADD "HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System" /v EnableLUA /t REG_DWORD /d 0 /f
REG ADD "HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System" /v ConsentPromptBehaviorAdmin /t REG_DWORD /d 0 /f
```

**参考链接：**
- [User Account Control 工作原理](https://learn.microsoft.com/zh-cn/windows/security/application-security/application-control/user-account-control/how-it-works)
- [Windows 用户账户控制](https://blog.walterlv.com/post/windows-user-account-control.html)

---

## 六、文件占用排查

### Windows 平台

即使应用在自动升级前已关闭，在特殊情况下（如后台服务未退出）仍可能出现文件占用。此时可使用微软的 **handle.exe** 工具检查指定目录下是否有进程在运行。

`handle.exe` 是 Microsoft Sysinternals 提供的命令行工具，用于显示哪些进程打开了指定文件。在 C# 中可通过 `System.Diagnostics.Process` 调用。

```csharp
using System;
using System.Diagnostics;

class Program
{
    static void Main()
    {
        Process process = new Process();
        process.StartInfo.FileName = "handle.exe";
        process.StartInfo.Arguments = "filename"; // 替换为实际的文件或目录路径
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.Start();

        string output = process.StandardOutput.ReadToEnd();
        Console.WriteLine(output);

        process.WaitForExit();
    }
}
```

**如果仍存在文件占用：**
1. 检查是否有后台服务未关闭
2. 使用 `handle.exe` 排查占用进程
3. 考虑使用强制终止或重启后更新策略

**参考链接：**
- [Handle - Sysinternals](https://learn.microsoft.com/zh-cn/sysinternals/downloads/handle)

---

## 七、崩溃转储诊断

在自动更新过程中，如果更新失败或更新后程序崩溃，可使用 **ProcDump** 工具导出 dump 文件进行分析。

ProcDump 是 Microsoft Sysinternals 的命令行实用工具，主要用于监控应用程序的 CPU 峰值并在峰值期间生成崩溃转储。管理员或开发人员可使用这些转储来确定崩溃原因。ProcDump 还支持挂起窗口监控、未处理异常监控，并可基于系统性能计数器值生成转储。

### C# 调用 ProcDump

```csharp
using System;
using System.Diagnostics;

public class Program
{
    public static void Main()
    {
        var procDumpPath = @"C:\Path\To\procdump.exe";
        var processId = 1234; // 需要 dump 的进程 ID
        var dumpFilePath = @"C:\Path\To\dumpfile.dmp";

        var startInfo = new ProcessStartInfo
        {
            FileName = procDumpPath,
            Arguments = $"-ma {processId} {dumpFilePath}",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        var process = new Process { StartInfo = startInfo };
        process.OutputDataReceived += (sender, e) => Console.WriteLine(e.Data);
        process.ErrorDataReceived += (sender, e) => Console.Error.WriteLine(e.Data);

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        process.WaitForExit();
    }
}
```

**参考链接：**
- [ProcDump - Sysinternals](https://learn.microsoft.com/zh-cn/sysinternals/downloads/procdump)

---

## 八、版本号管理

### 推荐策略

| 策略 | 说明 | 适用场景 |
|------|------|----------|
| **手动管理** | 在 `.csproj` 中硬编码 `<Version>` | 小型项目 |
| **Git Tag** | 从 Git Tag 读取版本号 | 有 CI/CD 的项目 |
| **MinVer** | 自动从 Git 历史计算版本号 | 推荐 |
| **Nerdbank.GitVersioning** | 精确的 Git 版本管理 | 大型项目 |

### MinVer 配置示例

```xml
<!-- .csproj -->
<PackageReference Include="MinVer" Version="5.*">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
</PackageReference>
```

---

## 九、部署检查清单

部署前确认：

- [ ] 主程序引用 `GeneralUpdate.ClientCore`
- [ ] 升级助手是独立的可执行项目，引用 `GeneralUpdate.Core`
- [ ] 升级助手与主程序部署在同一目录
- [ ] `generalupdate.manifest.json` 可通过 Tools 的「配置生成器」自动生成
- [ ] 已在测试环境使用 Tools 的「模拟更新」功能完整验证
- [ ] 黑名单配置正确（保护用户数据不被覆盖）
- [ ] 平台特定权限处理已配置（Linux `chmod`、Windows 注册表降权）
- [ ] 版本号符合 SemVer 格式
- [ ] 服务端 API 已部署并配置正确的版本信息

---

## 相关资源

- **[GeneralUpdate.Tools](../quickstart/GeneralUpdate.PacketTool)** — 补丁包和配置生成工具
- **[入门实战手册](../quickstart/Beginner cookbook)** — 从零跑通更新闭环
- **[GeneralUpdate.Core](../doc/GeneralUpdate.Core)** — 核心更新引擎架构
