---
sidebar_position: 8
---

# 常见问题 (FAQ)

## 基础概念

### Q1: GeneralUpdate 是什么？

GeneralUpdate 是一个基于 .NET Standard 2.0 的跨平台自动更新框架。它提供了完整的应用程序更新解决方案，包括版本检查、下载管理、二进制差分更新、驱动更新、插件管理等功能。

### Q2: ClientCore 和 Core 有什么区别？

| | ClientCore | Core |
|---|---|---|
| **运行位置** | 主程序进程内 | 独立升级进程 |
| **职责** | 检查更新、下载包、启动升级助手 | 解压、打补丁、替换文件、启动新版本 |
| **依赖方向** | 引用 Core 的共享类型 | 被 ClientCore 启动（通过 IPC） |
| **NuGet 包** | `GeneralUpdate.ClientCore` | `GeneralUpdate.Core` |

**简单来说：** ClientCore "检查和下载"，Core "安装和替换"。

### Q3: 什么是差分更新？为什么需要它？

差分更新只下载两个版本之间的**变化部分**，而不是下载完整的安装包。

- **全量更新：** 下载 50MB → 解压 50MB → 覆盖安装
- **差分更新：** 下载 5MB (补丁) → 解压 → BSDiff 算法合并 → 仅 5MB 下载

对于频繁更新的应用，差分更新可节省 80%-95% 的带宽。

### Q4: GeneralUpdate 支持哪些平台？

| 操作系统 | 支持状态 | 备注 |
|----------|----------|------|
| Windows 10/11 | ✅ 完整支持 | 包括 WPF/WinForms/WinUI/Avalonia/Console |
| Linux (Ubuntu/Debian/Fedora) | ✅ 完整支持 | Avalonia/Console |
| macOS | ✅ 完整支持 | Avalonia/Console |
| Android | ✅ 通过 MAUI | 仅 OSS 模式 |
| 麒麟 V10 (飞腾/鲲鹏) | ✅ 已验证 | 国产化平台 |
| 统信 UOS | ✅ 已验证 | 国产化平台 |
| 龙芯 (LoongArch) | ✅ 已验证 | 国产化平台 |

---

## 安装与配置

### Q5: 最简配置需要多少代码？

使用 `ConfiginfoBuilder` 零配置模式：

```csharp
using GeneralUpdate.ClientCore;
using GeneralUpdate.Common.Shared.Object;

var config = ConfiginfoBuilder
    .Create("https://your-server.com/api/update/check", 
            "your-token", "Bearer")
    .Build();

await new GeneralClientBootstrap()
    .SetConfig(config)
    .LaunchAsync();
```

仅需 3 个参数，其它从 `.csproj` 自动提取。

### Q6: 如何配置黑名单？

```csharp
var config = new Configinfo
{
    // 跳过特定文件
    BlackFiles = new List<string> { "appsettings.json", "userdata.db" },
    
    // 跳过特定格式
    BlackFormats = new List<string> { ".log", ".cache", ".tmp" },
    
    // 跳过特定目录
    SkipDirectorys = new List<string> { "logs", "temp", "userdata" }
};
```

默认已跳过：`System.*.dll`、`.patch`、`.pdb`、`.rar`、`.tar`、`.json`、`.zip` 文件，以及 `app-`、`fail` 开头的目录。

### Q7: 静默更新如何配置？

```csharp
await new GeneralClientBootstrap()
    .Option(UpdateOption.EnableSilentUpdate, true)
    .SetConfig(config)
    .LaunchAsync();
```

启用后：
- 每 20 分钟（可配置）后台轮询检查新版本
- 发现新版本后静默下载
- 主程序退出时自动触发升级
- 无需用户交互

---

## 版本管理

### Q8: 版本号格式要求是什么？

使用语义化版本（SemVer 2.0）格式：`Major.Minor.Patch.Build`

- `1.0.0.0` ✓
- `2.1.3.5` ✓
- `1.0` ✗（不完整）
- `v1.0.0` ✗（含前缀）

### Q9: 如何处理多版本跳级更新？

GeneralUpdate 自动支持逐版本更新。如果客户端版本是 `1.0.0.0`，服务端有 `1.0.1.0`、`1.0.2.0`、`1.1.0.0` 三个版本：

```
客户端 1.0.0.0
  → 下载 patch_v1.0.1.zip → 更新到 1.0.1.0
  → 下载 patch_v1.0.2.zip → 更新到 1.0.2.0
  → 下载 patch_v1.1.0.zip → 更新到 1.1.0.0
```

按发布日期逐个升级，确保每步都经过完整校验。

### Q10: 可以实现强制更新吗？

可以。服务端在版本信息中设置 `IsForcibly: true`：

```json
{
    "Version": "2.0.0.0",
    "IsForcibly": true,
    "UpdateLog": "重要安全更新，必须安装"
}
```

强制更新时，客户端的 `AddListenerUpdatePrecheck` 回调返回值会被忽略，更新一定执行。

---

## 下载与网络

### Q11: 支持断点续传吗？

支持。下载中断后，下次启动会从断点继续下载。通过 `EnableResume` 选项控制（默认启用）。

```csharp
await new GeneralClientBootstrap()
    .Option(UpdateOption.EnableResume, true)
    .SetConfig(config)
    .LaunchAsync();
```

### Q12: 下载超时如何配置？

```csharp
await new GeneralClientBootstrap()
    .Option(UpdateOption.DownloadTimeOut, 120) // 120 秒超时
    .SetConfig(config)
    .LaunchAsync();
```

默认超时 30 秒。建议根据更新包大小和网络环境调整。

### Q13: 可以并发下载多个版本吗？

可以。通过 `MaxConcurrency` 全局选项配置：

```csharp
Option.MaxConcurrency.SetValue(5); // 最多同时下载 5 个版本
```

---

## 差分更新

### Q14: 差分更新 vs 全量更新，如何选择？

| 场景 | 建议模式 |
|------|----------|
| 日常小版本更新 | 差分更新（默认） |
| 大版本跨越（如 1.x → 2.x） | 全量更新 |
| 文件变化 < 20% | 差分更新 |
| 文件变化 > 80% | 全量更新 |
| 首次安装 | 全量更新 |

```csharp
// 关闭差分更新
await new GeneralClientBootstrap()
    .Option(UpdateOption.Patch, false)
    .SetConfig(config)
    .LaunchAsync();
```

### Q15: 补丁包是如何生成的？

使用 [GeneralUpdate.Tools](https://github.com/GeneralLibrary/GeneralUpdate.Tools) 的「补丁包」功能：

1. 选择旧版本目录（如 `MyApp_v1.0.0`）
2. 选择新版本目录（如 `MyApp_v1.0.1`）
3. 设置输出目录
4. 点击「构建」

工具自动：
- 对比两个目录的文件差异
- 为修改的文件生成 BSDiff 补丁（`.patch`）
- 收集新增文件
- 记录需要删除的文件（`delete_files.json`）
- 打包为 `.zip`

---

## 文件与权限

### Q16: 更新时遇到文件被占用怎么办？

更新过程由独立的升级助手进程（Core）执行，主程序已退出，通常不会有文件占用问题。

如果仍有占用：
1. 检查是否有后台服务未关闭
2. 使用 [文件占用指南](./File occupancy) 中的 `handle.exe` 排查
3. 考虑使用强制重启后更新策略

### Q17: Linux/macOS 上文件权限怎么处理？

使用 `UnixPermissionHooks` 或 `CustomPermissionHooks`：

```csharp
// 自动 chmod +x
await new GeneralClientBootstrap()
    .Hooks<UnixPermissionHooks>()
    .SetConfig(config)
    .LaunchAsync();
```

或通过 `Configinfo.Script` 指定自定义脚本：

```csharp
var config = new Configinfo
{
    Script = "/path/to/permission-script.sh",
    // ...
};
```

---

## 故障排查

### Q18: 更新失败如何诊断？

1. **检查事件监听：** 确保注册了所有异常和错误监听器
2. **查看 Bowl 日志：** 如果启用了 Bowl，检查 `fail/` 目录下的 Dump 和诊断文件
3. **检查服务端日志：** 确认版本信息正确返回
4. **检查网络：** 确认客户端可以访问服务端 API 和下载地址
5. **检查版本号：** 确保客户端和服务端的版本号格式一致

### Q19: 如何实现回滚？

GeneralUpdate 有自动备份和回滚机制：

```csharp
// 确保启用备份
await new GeneralClientBootstrap()
    .Option(UpdateOption.BackUp, true)
    .SetConfig(config)
    .LaunchAsync();
```

更新失败或 Bowl 检测到崩溃时，自动从备份目录恢复文件。

### Q20: 如何在开发环境测试更新流程？

使用 [GeneralUpdate.Tools](https://github.com/GeneralLibrary/GeneralUpdate.Tools) 的「模拟更新」功能：

1. 选择应用程序目录和补丁包
2. 设置版本号和平台
3. 点击「开始模拟」

工具自动：
- 启动本地模拟服务端
- 发布并运行 ClientSample 和 UpgradeSample
- 执行完整更新流程
- 生成测试报告

---

## 服务端

### Q21: 服务端 API 需要自己实现吗？

示例项目中提供了简单的服务端示例。生产环境需要自行实现或使用商业版本 [GeneralSpacestation](https://www.justerzhu.cn/)。

**需要实现的 API：**
1. `POST /Upgrade/Verification` — 版本验证
2. `POST /Upgrade/Report` — 状态上报
3. `GET /patch/{filename}` — 补丁包下载

### Q22: 可以和 CI/CD 集成吗？

可以。推荐集成方式：

1. **CI 构建：** 编译新旧版本
2. **Tools 命令行：** 使用 GeneralUpdate.Tools 生成补丁包（CLI 模式）
3. **上传：** 将补丁包和 `version.json` 上传到服务器/OSS
4. **更新清单：** 更新服务端版本数据库

---

## OSS 模式

### Q23: OSS 模式和标准模式有什么区别？

| | 标准模式 | OSS 模式 |
|---|---|---|
| **服务端** | 需要 HTTP API 服务 | 仅需对象存储（S3/OSS/MinIO） |
| **版本检查** | API 调用 | 读取 `version.json` 文件 |
| **适用场景** | 有后端服务的应用 | 客户端工具、桌面软件 |
| **成本** | 需要维护服务 | 仅存储和流量费用 |

### Q24: 如何从标准模式迁移到 OSS 模式？

1. 生成 `version.json`（使用 GeneralUpdate.Tools OSS 配置模块）
2. 将 `version.json` 和补丁包上传到对象存储
3. 客户端修改 `AppType` 为 `OssClient`
4. 配置 OSS 地址

---

## 相关资源

- **[GeneralUpdate.Core](../doc/GeneralUpdate.Core)** — 核心更新引擎
- **[入门实战手册](../quickstart/Beginner%20cookbook)** — 从零跑通更新闭环
- **[GeneralUpdate.Tools](../quickstart/GeneralUpdate.PacketTool)** — 打包工具
- **[GeneralUpdate.Bowl](../doc/GeneralUpdate.Bowl)** — 崩溃监控与回滚
