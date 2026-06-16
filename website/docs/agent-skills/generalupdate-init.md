---
sidebar_position: 2
sidebar_label: 🚀 集成指南
title: 🚀 generalupdate-init — 快速集成指南
---

# 🚀 GeneralUpdate 集成完全指南

帮助开发者在任意 .NET 应用中集成 GeneralUpdate 自动更新。从零开始，覆盖所有配置方式、部署场景和生产环境考量。

> ⚠️ **针对 NuGet v10.4.6 稳定版**。开发分支（v10.5.0-beta.2）有不同 API。

---

## 工作流程

```
1. 探测项目状态
   ├── 检查 .csproj → 目标框架、UI 类型
   └── 检查现有配置 → 已安装 NuGet？已有 manifest？
   
2. 选择集成模式
   ├── [Minimal] （new Configinfo + SetConfig + LaunchAsync）— 推荐新用户
   ├── [Standard]（Configinfo + 事件监听）— 需要精细控制
   └── [Scaffold]（完整双项目结构）— 从零开始的团队项目

3. 生成输出
   ├── NuGet 安装命令
   ├── Bootstrap 配置代码
   ├── manifest.json 模板
   └── 部署检查清单

4. 后续引导
   ├── 需要 UI → generalupdate-ui
   ├── 选择策略 → generalupdate-strategy
   └── 遇到问题 → generalupdate-troubleshoot
```

---

## 核心概念：4 大更新场景

GeneralUpdate 根据服务端返回的包类型决定更新策略：

| 场景 | 行为 |
|------|------|
| **None** | 无需更新，直接启动主程序 |
| **UpgradeOnly** | 只更新升级程序自身：Client 原地解压 Upgrade 包 |
| **MainOnly** | 只更新主程序：Client → IPC → 启动 Upgrade 进程 |
| **Both** | 两者都更新 |

### 双进程架构

```
App.exe (Client) 负责:
  ├── 版本验证（HTTP 请求服务端）
  ├── 下载所有更新包
  ├── IPC 写入（加密文件传递参数给 Upgrade）
  └── 启动 Upgrade.exe 然后自己退出

Upgrade.exe (Upgrade 进程) 负责:
  ├── 读取 IPC 文件
  ├── 应用更新（解压/补丁/替换文件）
  └── 启动主程序然后自己退出
```

---

## Configinfo 配置详解

### Configinfo 完整属性

```csharp
var config = new Configinfo
{
    // === 必需 ===
    UpdateUrl = "https://your-server.com/Upgrade/Verification",
    AppSecretKey = "your-secret-key",
    AppName = "MyApp.exe",
    MainAppName = "MyApp.exe",
    ClientVersion = "1.0.0.0",
    ProductId = "my-product-001",
    InstallPath = ".",
    
    // === 可选 ===
    ReportUrl = "https://your-server.com/Upgrade/Report",
    UpdateLogUrl = "https://your-server.com/Upgrade/Log",
    UpgradeClientVersion = "1.0.0.0",
    
    // === 安全认证 ===
    Scheme = "Bearer",           // HTTP 认证方案
    Token = "your-token",        // HTTP 认证令牌
    
    // === 黑名单（备份/复制时排除）===
    BlackFiles = new List<string> { "*.log", "*.tmp" },
    BlackFormats = new List<string> { ".pdb" },
    SkipDirectorys = new List<string> { "logs", "cache" },
};
```

### 应用角色（AppType）

`AppType` 是一个 class，包含两个静态字段：

| 字段 | 值 | 说明 |
|------|-----|------|
| `AppType.ClientApp` | 1 | 标准客户端（主程序） |
| `AppType.UpgradeApp` | 2 | 标准升级程序 |

> ⚠️ v10.4.6 不支持 OssClient（值 3-4），这些在开发分支中。

### 事件监听器完整清单

```csharp
// 全部 6 个事件
.AddListenerUpdateInfo((_, e) => {
    /* 版本验证结果（e.Info?.Body 含 VersionInfo 列表） */
})
.AddListenerMultiDownloadStatistics((_, e) => {
    /* 批量下载进度（e.ProgressPercentage, e.Speed, e.Remaining） */
})
.AddListenerMultiDownloadCompleted((_, e) => {
    /* 每版本下载完成（e.Version, e.IsComplated） */
})
.AddListenerMultiDownloadError((_, e) => {
    /* 下载错误（e.Exception, e.Version） */
})
.AddListenerMultiAllDownloadCompleted((_, e) => {
    /* 全部下载完成（e.IsAllDownloadCompleted, e.FailedVersions） */
})
.AddListenerException((_, e) => {
    /* 异常（e.Message, e.Exception） */
})
```

---

## 集成方式的完整代码

### 方式 A：Minimal — 使用 Configinfo

```csharp
using GeneralUpdate.Core;
using GeneralUpdate.Common.Shared.Object;

var config = new Configinfo
{
    UpdateUrl = "https://your-server.com/api",
    AppSecretKey = "your-32-char-secret-key-here!",
    AppName = "MyApp.exe",
    MainAppName = "MyApp.exe",
    ClientVersion = "1.0.0.0",
    ProductId = "my-product-001",
    InstallPath = "."
};

await new GeneralUpdateBootstrap()
    .SetConfig(config)
    .LaunchAsync();
```

### 方式 B：Standard — Configinfo + 事件 + 监听

```csharp
using GeneralUpdate.Core;
using GeneralUpdate.Common.Shared.Object;
using GeneralUpdate.Common.Download;

var config = new Configinfo
{
    UpdateUrl = "https://your-server.com/Upgrade/Verification",
    AppSecretKey = "your-secret-key",
    AppName = "MyApp.exe",
    MainAppName = "MyApp.exe",
    ClientVersion = "1.0.0.0",
    ProductId = "my-product-001",
    InstallPath = AppDomain.CurrentDomain.BaseDirectory,
    ReportUrl = "https://your-server.com/Upgrade/Report",
};

await new GeneralUpdateBootstrap()
    .SetConfig(config)
    .AddListenerUpdateInfo((_, e) =>
    {
        Console.WriteLine($"发现 {e.Info?.Body?.Count ?? 0} 个版本");
    })
    .AddListenerMultiDownloadStatistics((_, e) =>
    {
        Console.WriteLine($"进度: {e.ProgressPercentage}% | {e.Speed}");
    })
    .AddListenerMultiDownloadCompleted((_, e) =>
    {
        Console.WriteLine($"版本 {e.Version} 下载完成");
    })
    .AddListenerMultiAllDownloadCompleted((_, e) =>
    {
        Console.WriteLine($"全部完成 (IsAllDownloadCompleted={e.IsAllDownloadCompleted})");
    })
    .AddListenerMultiDownloadError((_, e) =>
    {
        Console.WriteLine($"下载失败: 版本 {e.Version} — {e.Exception?.Message}");
    })
    .AddListenerException((_, e) =>
    {
        Console.WriteLine($"异常: {e.Message}");
    })
    .LaunchAsync();
```

### Upgrade 进程配置

```csharp
using GeneralUpdate.Core;

// Upgrade 模式从 IPC 文件读取配置，无需 SetConfig
await new GeneralUpdateBootstrap()
    .AddListenerException((_, e) =>
        Console.WriteLine($"错误: {e.Message}"))
    .LaunchAsync();
```

---

## 生产环境部署检查清单

### 发布目录结构

```
publish/
├── MyApp.exe                  ← MainAppName（主程序）
├── generalupdate.manifest.json
└── update/
    └── UpgradeApp.exe         ← 升级程序，必须随首个版本发布
```

### 双进程验证

| 检查项 | 说明 |
|--------|------|
| UpgradeApp.exe 存在于发布目录 | 首个版本就必须有 |
| Client 和 Upgrade 使用相同 AppSecretKey | IPC 加密通信依赖此 Key |
| Client 和 Upgrade 使用相同 NuGet 版本号 | 版本不一致导致 "Method not found" |
| Upgrade 进程不需要网络 | 所有数据由 Client 预下载 |

---

## ⚠️ 已知问题

### NuGet 类型冲突

`GeneralUpdate.Core` 和 `GeneralUpdate.Bowl` **不能同时引用**（CS0433 类型冲突）。

请根据需求选择：
- 使用 Core：`dotnet add package GeneralUpdate.Core`
- 使用 Bowl：**只引用** `GeneralUpdate.Bowl`（它传递依赖 Core 所有功能）
- 差分类型已内嵌在 Core，**无需额外** `GeneralUpdate.Differential` 包

### 稳定版功能限制

v10.4.6 无 `IUpdateHooks`、无可编程 `Option`、无静默轮询器。这些功能在开发分支（v10.5.0-beta.2）中可用。

---

## 相关技能

- [🎨 generalupdate-ui — UI 框架自动检测 + 更新窗口代码生成](./generalupdate-ui.md)
- [⚙️ generalupdate-strategy — 6 种更新策略选择与配置](./generalupdate-strategy.md)
- [🔧 generalupdate-advanced — 高级定制](./generalupdate-advanced.md)
- [🩺 generalupdate-troubleshoot — 已知问题诊断](./generalupdate-troubleshoot.md)
