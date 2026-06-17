---
sidebar_position: 5
sidebar_label: 🔧 高级定制
title: 🔧 generalupdate-advanced — 高级定制参考
---

# 🔧 GeneralUpdate 高级定制参考

涵盖扩展点架构、Pipeline 管道、差分引擎、Bowl 崩溃守护、事件系统、文件系统工具等。

> ⚠️ **API 版本说明**：本指南基于 **NuGet v10.5.0-beta.4**。
> 以下功能在 v10.5.0-beta.4 中全部**可用**：
> - ✅ `IUpdateHooks` 生命周期钩子（`Hooks<T>()`）
> - ✅ `IStrategy` 自定义策略注入（`Strategy<T>()`）
> - ✅ `SilentPollOrchestrator` 静默轮询器（`Option.Silent`）
> - ✅ `Option` 可编程配置系统
> - ✅ `ISslValidationPolicy` SSL 策略接口
> - ✅ `IHttpAuthProvider` HTTP 认证提供者
> - ✅ `DiffPipelineBuilder` 差分管道配置
>
> 各功能的命名空间和用法在文中已标注。

---

## 📋 用户需求提取

```
### 定制目标（必需）
- 需要什么定制: ______（Bowl 崩溃守护 / IPC 替换 / Pipeline 定制 / 自定义策略 / AOT / Drivelution / 黑名单 / 认证提供者 / 差分引擎）
- 使用的 GeneralUpdate 版本: ______（v10.4.6 稳定版 / v10.5.0+ 开发分支）
- .NET 版本: ______（.NET 6/8/9/10）

### Bowl（如果选择）
- 被监控进程名: ______
- 工作模式: ______（Normal / Upgrade）
- 是否需要崩溃 Dump: ______（是/否）
- 备份目录路径: ______

### IPC 替换（如果选择）
- 替换方式: ______（NamedPipe / SharedMemory / 自定义）
- 目标平台: ______（Windows / Linux / macOS / 跨平台）
- 安全要求: ______（加密 / 签名 / 无额外安全）

### AOT（如果选择）
- 当前剪裁警告: ______（有/无）
- 是否使用反射: ______（是/否）
- JSON 序列化需求: ______（有/无）
```

---

## 1. Pipeline 管道系统（v10.5.0-beta.4 可用）

GeneralUpdate 使用 Pipeline 管道模式处理更新包的校验、解压、补丁应用。

### PipelineBuilder API

```csharp
using GeneralUpdate.Core.Pipeline;

// 创建管道上下文
var context = new PipelineContext();
context.Add("ZipFilePath", @"C:\temp\update.zip");
context.Add("Hash", "sha256-hex-value");
context.Add("Format", 0);  // 0=Zip
context.Add("Encoding", System.Text.Encoding.UTF8);
context.Add("SourcePath", @"C:\Program Files\MyApp");
context.Add("PatchEnabled", true);

// 构建并执行管道
await new PipelineBuilder(context)
    .UseMiddleware<HashMiddleware>()      // 哈希校验
    .UseMiddleware<CompressMiddleware>()  // 解压
    .UseMiddleware<PatchMiddleware>()     // 差分补丁
    .Build();
```

| 中间件 | 类名 | 命名空间 | 功能 |
|--------|------|---------|------|
| 哈希校验 | `HashMiddleware` | `GeneralUpdate.Core.Pipeline` | SHA256 完整性校验 |
| 解压 | `CompressMiddleware` | `GeneralUpdate.Core.Pipeline` | 解压 ZIP 包 |
| 差分补丁 | `PatchMiddleware` | `GeneralUpdate.Core.Pipeline` | 应用 BSDIFF/HDiffPatch 补丁 |
| 驱动更新 | `DrivelutionMiddleware` | `GeneralUpdate.Core.Pipeline` | Windows 驱动安装 |

---

## 2. 策略系统（v10.5.0-beta.4 可用）

GeneralUpdate 内置三种平台策略，通过 `IStrategy` 接口实现：

| 策略 | 类名 | 平台 |
|------|------|------|
| Windows | `WindowsStrategy` | Windows |
| Linux | `LinuxStrategy` | Linux |
| OSS | `OSSStrategy` | 跨平台（对象存储） |

> ✅ 支持通过 `bootstrap.Strategy<T>()` 注入自定义策略。
> 自定义策略需要实现 `IStrategy` 接口。

---

## 3. Bowl 崩溃守护（v10.5.0-beta.4）

Bowl 是一个崩溃监控组件，通过 `BowlContext` 配置。

```csharp
using GeneralUpdate.Bowl;

var context = new BowlContext
{
    ProcessNameOrId = "MyApp.exe",
    DumpFileName = "v1.0.0.0_fail.dmp",
    FailFileName = "v1.0.0.0_fail.json",
    TargetPath = @"C:\Program Files\MyApp",
    FailDirectory = @"C:\Program Files\MyApp\fail",
    BackupDirectory = @"C:\Program Files\MyApp\backup",
    WorkModel = "Upgrade",
    TimeoutMs = 30_000,
    AutoRestore = true,
    OnCrash = async (info, ct) => Console.WriteLine($"Crash: {info.DumpFilePath}"),
};

var bowl = new Bowl();
var result = await bowl.LaunchAsync(context);
Console.WriteLine($"Result: Success={result.Success}, Restored={result.Restored}");
```

| 属性 | 类型 | 说明 |
|------|------|------|
| `ProcessNameOrId` | string | 被监控的进程名或 PID（必填） |
| `TargetPath` | string | 应用安装根目录（必填） |
| `DumpFileName` | string | Dump 文件名（必填） |
| `FailFileName` | string | 故障报告文件名（必填） |
| `FailDirectory` | string | 崩溃报告输出目录（必填） |
| `BackupDirectory` | string | 备份目录（必填） |
| `WorkModel` | string | "Upgrade" 或 "Normal" |
| `TimeoutMs` | int | 监控超时(毫秒)，默认 30000 |
| `AutoRestore` | bool | 崩溃后自动回滚 |
| `DumpType` | DumpType | Mini / Full |
| `OnCrash` | delegate | 崩溃回调 |

> ⚠️ NuGet v10.5.0-beta.4 中 Bowl 和 Core **无类型冲突**，可以同时引用。

---

## 4. EventManager 事件系统（v10.5.0-beta.4 可用）

EventManager 是一个全局单例，提供事件的发布和订阅：

```csharp
using GeneralUpdate.Core.Event;

// 添加监听
EventManager.Instance.AddListener((object? sender, UpdateInfoEventArgs e) =>
{
    // 处理版本发现事件
});

// 手动分发事件
EventManager.Instance.Dispatch(this, new ExceptionEventArgs(ex, "自定义错误"));

// 清空所有监听
EventManager.Instance.Clear();

// 释放
EventManager.Instance.Dispose();
```

> ⚠️ EventManager 是全局单例，`Dispose()` 后 `Instance` 仍然可访问。

---

## 5. 文件系统工具（v10.5.0-beta.4 可用）

### BlackList（黑名单）

`UpdateRequest` 支持通过以下属性排除文件：

```csharp
var config = new UpdateRequest
{
    // ...
    Files = new List<string> { "*.log", "*.tmp" },
    Formats = new List<string> { ".pdb", ".vshost.exe" },
    Directories = new List<string> { "logs", "cache", "temp" },
};
```

黑名单内部通过 `ToBlackPolicy()` 转换为 `BlackPolicy` 记录。

### FileTree（文件树对比）

```csharp
using GeneralUpdate.Core.FileSystem;

var tree = new FileTree();
var snapshot = tree.CreateSnapshot(@"C:\Program Files\MyApp");
```

---

## 6. 差分引擎（v10.5.0-beta.4 可用，无需额外安装包）

差分类型已内嵌在 `GeneralUpdate.Core` 中，**无需额外**安装 `GeneralUpdate.Differential` 包。

### DiffPipelineBuilder 方式（推荐）

```csharp
using GeneralUpdate.Core.Pipeline;

var pipeline = new DiffPipelineBuilder()
    .UseDiffer(new StreamingHdiffDiffer())     // 差分算法
    .UseCleanMatcher(new DefaultCleanMatcher()) // 文件匹配器（服务端）
    .UseDirtyMatcher(new DefaultDirtyMatcher()) // 文件匹配器（客户端）
    .WithParallelism(4)
    .WithStopOnFirstError(true)
    .WithProgress(new Progress<DiffProgress>(p =>
        Console.WriteLine($"[{p.Completed}/{p.Total}] {p.FileName}")))
    .Build();

// 服务端：生成补丁
await pipeline.CleanAsync(oldDir, newDir, patchDir);

// 客户端：应用补丁
await pipeline.DirtyAsync(appDir, patchDir);
```

### Bootstrap 集成方式

```csharp
new GeneralUpdateBootstrap()
    .SetConfig(config)
    .UseDiffPipeline(pipeline =>
    {
        pipeline.WithParallelism(2)
                .WithStopOnFirstError(true);
    })
    .LaunchAsync();
```

### 自定义匹配器

```csharp
using GeneralUpdate.Core.Differential;

// 自定义清理匹配器
var cleanMatcher = new DefaultCleanMatcher();  // 或实现 ICleanMatcher
var dirtyMatcher = new DefaultDirtyMatcher();  // 或实现 IDirtyMatcher
```

---

## 7. AOT / NativeAOT 兼容性

GeneralUpdate.Core v10.5.0-beta.4 支持 .NET Native AOT（`net8.0` 和 `net10.0`）：

```xml
<PropertyGroup>
  <IsAotCompatible>true</IsAotCompatible>
  <EnableTrimAnalyzer>true</EnableTrimAnalyzer>
</PropertyGroup>
```

JSON 序列化上下文（减少 AOT 大小）：

```csharp
using GeneralUpdate.Core.JsonContext;

// 使用内置的 JsonSerializerContext
// VersionRespJsonContext, ProcessContractJsonContext, HttpParameterJsonContext 等
```

---

## 8. Drivelution（Windows 驱动更新）

`GeneralUpdate.Drivelution` 包提供 Windows 驱动管理：

```csharp
using GeneralUpdate.Drivelution;

// 扫描驱动目录
var allDrivers = GeneralDrivelution.ScanDirectory(driverDir);

// 验证驱动
var isValid = GeneralDrivelution.ValidateDriver(driverPath);

// 安装驱动（DIFx → SetupAPI → PnPUtil 级联）
var result = GeneralDrivelution.InstallDriver(driverPath);
```

---

## 内容索引

| 主题 | 可用性 | 参考 |
|------|--------|------|
| Pipeline 管道 | ✅ v10.5.0-beta.4 | `GeneralUpdate.Core.Pipeline` |
| 策略系统 | ✅ v10.5.0-beta.4 | `GeneralUpdate.Core.Strategy` |
| FileTree | ✅ v10.5.0-beta.4 | `GeneralUpdate.Core.FileSystem` |
| BlackList | ✅ v10.5.0-beta.4 | `UpdateRequest.Files/Formats/Directories` → `ToBlackPolicy()` |
| 差分引擎 | ✅ 内嵌 Core | `DiffPipelineBuilder` / `DiffPipeline` |
| AOT | ✅ v10.5.0-beta.4 | `JsonSerializerContext` 子类 |
| EventManager | ✅ v10.5.0-beta.4 | `GeneralUpdate.Core.Event` |
| Bowl 崩溃守护 | ✅ v10.5.0-beta.4 | `GeneralUpdate.Bowl.Bowl` |
| IUpdateHooks | ✅ v10.5.0-beta.4 | `GeneralUpdate.Core.Hooks` — `Hooks<T>()` |
| 自定义 Strategy 注入 | ✅ v10.5.0-beta.4 | `Strategy<T>()` |
| IPC 替换接口 | ❌ 暂不支持 | 使用 NamedPipe 替代方案 |
| SilentPollOrchestrator | ✅ v10.5.0-beta.4 | `Option.Silent` + `SetOption()` |
| Option 系统 | ✅ v10.5.0-beta.4 | `SetOption<T>(Option<T>, T)` |

---

## ✅ 高级定制验证清单

### Bowl 崩溃守护
- [ ] 如果用 Bowl：项目中同时引用 `GeneralUpdate.Core` 和 `GeneralUpdate.Bowl`（v10.5.0-beta.4 无冲突）
- [ ] `BowlContext` 的 `ProcessNameOrId` 与实际进程名匹配
- [ ] `TargetPath` 设置为应用安装根目录，非子目录
- [ ] `WorkModel` 根据场景选择 Correct（Normal/Upgrade）
- [ ] `FailDirectory` 有写入权限
- [ ] Linux/macOS 无此功能（Bowl 仅 Windows）

### Pipeline 定制
- [ ] `PipelineContext` 中的 Key 名称使用字符串常量拼写正确
- [ ] 中间件注册顺序正确：Hash → Compress → Patch → Drivelution
- [ ] `Encoding` 设置为 `Encoding.UTF8`

### AOT/NativeAOT
- [ ] 启用了 `<IsAotCompatible>true</IsAotCompatible>`
- [ ] 对反射路径添加了 `[DynamicDependency]` 或 `[RequiresUnreferencedCode]`
- [ ] 使用了内置的 `JsonSerializerContext` 子类（减少裁剪）

---

## ⚠️ 反模式清单

| # | 反模式 | 后果 | 正确做法 |
|---|--------|------|---------|
| 1 | **在 v10.4.6 稳定版上使用开发分支 API（IUpdateHooks 等）** | 编译失败 / 运行时 MissingMethodException | 检查 API 可用性表 |
| 2 | **PipelineContext Key 拼写错误** | Pipeline 运行异常，值未传递 | 使用类库公开的常量或文档中的 Key 名 |
| 3 | **Bowl 的 WorkModel 设为 Upgrade 但进程是主程序** | 监控逻辑错误 | Normal=主线进程，Upgrade=升级进程 |
| 4 | **Windows 上 IPC 使用默认加密密钥** | 加密可被破解 | 使用强密钥（≥ 32 字符） |
| 5 | **差分包生成时使用不同版本的源文件结构** | 补丁应用失败 | 源和目标版本的文件结构必须一致 |
| 6 | **AOT 项目中使用了大量反射且未标记 DynamicDependency** | 运行时 TypeLoadException | 使用源代码生成器或显式标记保留 |
| 7 | **Pipeline 中 PatchMiddleware 排在 CompressMiddleware 前面** | 未解压就试图打补丁 | 顺序必须是 Compress→Patch |

---

## 相关技能

- `/generalupdate-init` — Bootstrap 配置
- `/generalupdate-strategy` — 更新策略选择
- `/generalupdate-troubleshoot` — 问题诊断
