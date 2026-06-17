---
sidebar_position: 2
sidebar_label: 🚀 集成指南
title: 🚀 generalupdate-init — 快速集成指南
---

# 🚀 GeneralUpdate 集成完全指南

帮助开发者在任意 .NET 应用中集成 GeneralUpdate 自动更新。从零开始，覆盖所有配置方式、部署场景和生产环境考量。

> ⚠️ **针对 NuGet v10.5.0-beta.4**。`Configinfo` 已被 `UpdateRequest` 替代，命名空间已移至 `GeneralUpdate.Core.Configuration`。

---

## 📋 用户需求提取

在生成代码前，必须先提取以下信息。**不确定的必须追问：**

```
### 项目状态
- 现有项目类型: ______（新项目 / 已有项目 / 从旧版迁移）
- .NET 版本: ______
- UI 框架: ______（WPF/WinForms/Avalonia/MAUI/控制台/无）
- 目标平台: ______（Windows/Linux/macOS/多平台）

### 更新需求
- 是否需要显示进度 UI: ______（是/否）
- 是否有后端服务: ______（是/否）
- 更新策略倾向: ______（标准/OSS/静默/差分/跨版本/推送）
- 是否需要崩溃守护 Bowl: ______（是/否）

### 已有配置（如果存在）
- 是否已安装 NuGet: ______（是/否，版本号）
- 是否已有 UpdateRequest 配置: ______（是/否）
- 是否已有 manifest.json: ______（是/否）
```

---

## 工作流程（按顺序执行）

### Step 1：探测项目状态

```
├── 检查 .csproj → 目标框架、UI 类型、是否有 NuGet 引用
├── 检查是否存在 generalupdate.manifest.json
├── 检查是否存在 UpdateRequest/Bootstrap 配置代码
└── 检查项目结构 → 是否已有独立的 Upgrade 项目
```

### Step 2：选择集成模式

基于需求提取结果，选择以下模式之一：

| 模式 | 适用场景 | 产出 |
|------|---------|------|
| **[Minimal]** | 新用户快速上手，控制台/服务应用 | 3 行 Bootstrap 代码 |
| **[Standard]** | 需要精确控制更新过程 | UpdateRequest + 完整事件监听 |
| **[Scaffold]** | 团队项目，从零开始 | 完整 Client + Upgrade 双项目结构 |

### Step 3：生成输出

```
├── NuGet 安装命令（按平台选 Core/Bowl）
├── Bootstrap 配置代码（按模式）
├── manifest.json 模板
├── 部署检查清单
└── 已知问题预警（针对你的配置组合）
```

### Step 4：引导下一步

```
├── 需要 UI → /generalupdate-ui
├── 选择策略 → /generalupdate-strategy
├── 需要 Bowl 守护 → /generalupdate-advanced
└── 遇到问题 → /generalupdate-troubleshoot
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

**双进程架构**：
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

## UpdateRequest 配置详解

### UpdateRequest 完整属性

```csharp
// 方式 A：直接构造 UpdateRequest（推荐）
var config = new UpdateRequest
{
    // === 必需 ===
    UpdateUrl = "https://your-server.com/Upgrade/Verification",
    AppSecretKey = "your-secret-key",
    MainAppName = "MyApp.exe",
    ClientVersion = "1.0.0.0",
    ProductId = "my-product-001",
    InstallPath = ".",
    
    // === 可选 ===
    ReportUrl = "https://your-server.com/Upgrade/Report",
    UpdateLogUrl = "https://your-server.com/Upgrade/Log",
    UpgradeClientVersion = "1.0.0.0",
    
    // === 安全认证 ===
    AuthScheme = AuthScheme.Hmac,  // Hmac / Bearer / ApiKey / Basic
    Token = "your-token",
    BasicUsername = "user",
    BasicPassword = "pass",
    
    // === 黑名单（备份/复制时排除）===
    Files = new List<string> { "*.log", "*.tmp" },
    Formats = new List<string> { ".pdb" },
    Directories = new List<string> { "logs", "cache" },
};

// 方式 B：使用建造者模式
var config = UpdateRequestBuilder.Create()
    .SetUpdateUrl("https://your-server.com/api")
    .SetAppSecretKey("your-secret-key")
    .SetMainAppName("MyApp.exe")
    .SetClientVersion("1.0.0.0")
    .SetProductId("my-product-001")
    .SetInstallPath(".")
    .Build();

// 方式 C：零配置 — 从 manifest.json 自动发现
await new GeneralUpdateBootstrap()
    .SetSource(
        updateUrl: "https://your-server.com/api",
        appSecretKey: "your-secret-key")
    .AddListenerUpdateInfo(...)
    .LaunchAsync();
```

### 应用角色（AppType）

`AppType` 是一个 enum（v10.5.0-beta.4）：

| 值 | 名称 | 说明 |
|----|------|------|
| 1 | `AppType.Client` | 标准客户端（主程序） |
| 2 | `AppType.Upgrade` | 标准升级程序 |
| 3 | `AppType.OssClient` | OSS 客户端模式（静默） |
| 4 | `AppType.OssUpgrade` | OSS 升级模式 |

### 事件监听器完整清单

```csharp
// 全部 7 个事件
.AddListenerUpdateInfo((_, e) => {
    /* 版本验证结果（e.Info?.Body 含 VersionEntry 列表） */
})
.AddListenerMultiDownloadStatistics((_, e) => {
    /* 批量下载进度（e.ProgressPercentage, e.Speed, e.Remaining） */
})
.AddListenerMultiDownloadCompleted((_, e) => {
    /* 每版本下载完成（e.Version, e.IsCompleted） */
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
.AddListenerProgress((_, e) => {
    /* 进度（e.Progress 或 e.DiffProgress，v10.5+） */
})
```

---

## 集成方式的完整代码

### 方式 A：Minimal — 使用 UpdateRequest

```csharp
using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;

var config = new UpdateRequest
{
    UpdateUrl = "https://your-server.com/api",
    AppSecretKey = "your-32-char-secret-key-here!",
    MainAppName = "MyApp.exe",
    ClientVersion = "1.0.0.0",
    ProductId = "my-product-001",
    InstallPath = "."
};

await new GeneralUpdateBootstrap()
    .SetConfig(config)
    .LaunchAsync();
```

### 方式 B：Standard — UpdateRequest + 事件监听

```csharp
using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;
using GeneralUpdate.Core.Download;

var config = new UpdateRequest
{
    UpdateUrl = "https://your-server.com/Upgrade/Verification",
    AppSecretKey = "your-secret-key",
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

### NuGet 注意事项（v10.5.0-beta.4）
`GeneralUpdate.Core` 和 `GeneralUpdate.Bowl` **可以同时引用**（v10.5.0-beta.4 中无 CS0433 冲突）。
- 使用 Core：`dotnet add package GeneralUpdate.Core`
- 使用 Bowl：`dotnet add package GeneralUpdate.Bowl`（它**不**传递依赖 Core，需要同时引用 Core）
- 差分类型已内嵌在 Core，**无需额外** `GeneralUpdate.Differential` 包

### 稳定版功能增强
v10.5.0-beta.4 新增以下功能：
- ✅ `IUpdateHooks` 生命周期钩子 — `Hooks<T>()`
- ✅ 可编程 `Option` 系统 — `SetOption(Option.Silent, true)`
- ✅ `SilentPollOrchestrator` 静默轮询
- ✅ `SetSource()` 零配置入口
- ✅ `UseDiffPipeline()` 差分管道配置
- ✅ `AddListenerProgress()` 第 7 个事件
- ✅ `IStrategy` 自定义策略注入 — `Strategy<T>()`
- ✅ `IUpdateReporter` / `IHttpAuthProvider` 等扩展点

---

## ✅ 集成验证清单（交付前逐项检查）

### Bootstrap 配置
- [ ] `UpdateRequest` 的 6 个必填字段都已设置（UpdateUrl, AppSecretKey, MainAppName, ClientVersion, ProductId, InstallPath）
- [ ] `UpdateUrl` 指向的服务端 API 可正常返回版本信息
- [ ] `AppSecretKey` 长度 ≥ 16 字符，与服务端一致
- [ ] `AppType` 设置正确（Client = 1, Upgrade = 2）
- [ ] 生产环境使用 `AppDomain.CurrentDomain.BaseDirectory` 作为 InstallPath

### NuGet & 编译
- [ ] Client 和 Upgrade 项目使用**完全相同**的 GeneralUpdate NuGet 版本
- [ ] 如果用 Bowl：项目中同时引用 `GeneralUpdate.Core` 和 `GeneralUpdate.Bowl`（v10.5.0-beta.4 无冲突）
- [ ] 项目能正常 `dotnet build`（0 errors）
- [ ] 无需额外引用 `GeneralUpdate.Differential`（已嵌入 Core）

### 部署结构
- [ ] UpgradeApp.exe 存在于发布目录 update/ 子目录中（首个版本就必须有）
- [ ] `generalupdate.manifest.json` 的 `UpdateAppName` 包含 `.exe`
- [ ] IPC 文件（`UpdateInfo.msg`）路径在 Client/Upgrade 间一致
- [ ] `Encoding` 设置为 `Encoding.UTF8`（防止 Linux/macOS 中文乱码）

---

## ⚠️ 反模式清单

| # | 反模式 | 后果 | 正确做法 |
|---|--------|------|---------|
| 1 | **Core 和 Bowl NuGet 版本不一致** | 运行时 MethodNotFoundException | 使用相同 NuGet 版本 |
| 2 | **Bowl 缺少 `GeneralUpdate.Core` 引用** | 编译失败，缺少 Core 类型 | Bowl 不传递依赖 Core，需同时引用 Core |
| 3 | **Bowl 传递依赖 Core 的误解** | 编译失败 | v10.5.0-beta.4 中 Bowl 是独立包，需单独引用 Core |
| 4 | **Client/Upgrade NuGet 版本号不一致** | 运行时 MethodNotFoundException | 锁定完全相同版本 |
| 5 | **事件监听中做耗时操作（网络 IO / 磁盘 IO）** | Update 进程 UI 卡死，超时被 Kill | 仅更新 UI 状态，耗时操作异步 |
| 6 | **IPC 文件编码未设置 UTF-8** | Linux/macOS 中文乱码 | `Encoding.UTF8` |
| 7 | **版本号不是 4 段式（如 1.0.0.0）** | 版本比较逻辑异常 | 始终用 `x.y.z.w` 格式 |
| 8 | **manifest.json 的 mainAppName 不匹配真实进程名** | 更新后主程序找不到 | 和实际 exe 名称一致 |
| 9 | **为 v9.x 编写的代码直接用在 v10** | API 不兼容，编译失败 | 对照 v10.4.6 稳定版 API 重写 |

---

## 相关技能

- `/generalupdate-ui` — UI 框架自动检测 + 更新窗口代码生成
- `/generalupdate-strategy` — 6 种更新策略选择与配置
- `/generalupdate-advanced` — 高级定制
- `/generalupdate-troubleshoot` — 已知问题诊断
