---
sidebar_position: 5
sidebar_label: 🔧 高级定制
title: 🔧 generalupdate-advanced — 高级定制参考
---

# 🔧 GeneralUpdate 高级定制参考

涵盖扩展点架构、Pipeline 管道、差分引擎、Bowl 崩溃守护、事件系统、文件系统工具等。

> ⚠️ **API 版本说明**：本指南基于 **NuGet v10.4.6 稳定版**。
> 
> 以下功能在稳定版中**不存在**（但在开发分支 v10.5.0-beta.2 中已有）：
> - `IUpdateHooks` 生命周期钩子
> - `IProcessInfoProvider` IPC 替换接口
> - `SilentPollOrchestrator` 静默轮询器
> - `Option` 可编程配置系统（v10.4.6 仅使用 `Configinfo` 属性）
> - `ISslValidationPolicy` SSL 策略接口
>
> 各功能的可用性在文中已标注。

---

## 1. Pipeline 管道系统（v10.4.6 可用）

GeneralUpdate 使用 Pipeline 管道模式处理更新包的校验、解压、补丁应用。

### PipelineBuilder API

```csharp
using GeneralUpdate.Common.Internal.Pipeline;
using GeneralUpdate.Common.Internal.Strategy;

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
    .UseMiddleware<PatchMiddleware>()     // 差分补丁（需安装 Differential 包）
    .Build();
```

| 中间件 | 类名 | 命名空间 | 功能 |
|--------|------|---------|------|
| 哈希校验 | `HashMiddleware` | `GeneralUpdate.Core.Pipeline` | SHA256 完整性校验 |
| 解压 | `CompressMiddleware` | `GeneralUpdate.Core.Pipeline` | 解压 ZIP 包 |
| 差分补丁 | `PatchMiddleware` | `GeneralUpdate.Core.Pipeline` | 应用 BSDIFF/HDiffPatch 补丁 |
| 驱动更新 | `DrivelutionMiddleware` | `GeneralUpdate.Core.Pipeline` | Windows 驱动安装 |

---

## 2. 策略系统（v10.4.6 可用）

GeneralUpdate 内置三种平台策略，通过 `AbstractStrategy` 模板方法模式实现：

| 策略 | 类名 | 平台 |
|------|------|------|
| Windows | `WindowsStrategy` | Windows |
| Linux | `LinuxStrategy` | Linux |
| OSS | `OSSStrategy` | 跨平台（对象存储） |

> ⚠️ 稳定版**不支持**通过 `bootstrap.Strategy<T>()` 注入自定义策略。
> 自定义策略需要继承 `AbstractStrategy` 并直接调用。

---

## 3. Bowl 崩溃守护（v10.4.6 存在但功能有限）

Bowl 是一个崩溃监控组件，通过 `MonitorParameter` 配置。

> ⚠️ **注意**：v10.4.6 的 Bowl 仅提供基础类型定义，`Bowl` 类没有公开的 `LaunchAsync` 方法。
> 完整功能在开发分支（v10.5.0-beta.2）中。

### MonitorParameter 配置

```csharp
using GeneralUpdate.Bowl;
using GeneralUpdate.Bowl.Strategys;

var param = new MonitorParameter
{
    ProcessNameOrId = "MyApp.exe",
    DumpFileName = "v1.0.0.0_fail.dmp",
    FailFileName = "v1.0.0.0_fail.json",
    TargetPath = @"C:\Program Files\MyApp",
    FailDirectory = @"C:\Program Files\MyApp\fail",
    BackupDirectory = @"C:\Program Files\MyApp\backup",
    WorkModel = "Upgrade",
};

// Bowl 实例（v10.4.6 无公开 LaunchAsync，此为占位）
var bowl = new Bowl();
```

完整 Bowl 崩溃守护功能请关注 GeneralUpdate 后续版本。

### Bowl 引用规则

Bowl 引用规则：只用 `GeneralUpdate.Bowl`（传递依赖 Core，两者不能同时引用）。

---

## 4. EventManager 事件系统（v10.4.6 可用）

EventManager 是一个全局单例，提供事件的发布和订阅：

```csharp
using GeneralUpdate.Common.Internal.Event;

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

> ⚠️ EventManager 是全局单例，`Dispose()` 后 `Instance` 仍然可访问（代码审计发现）。

---

## 5. 文件系统工具（v10.4.6 可用）

### BlackList（黑名单）

`Configinfo` 支持通过以下属性排除文件：

```csharp
var config = new Configinfo
{
    // ...
    BlackFiles = new List<string> { "*.log", "*.tmp" },
    BlackFormats = new List<string> { ".pdb", ".vshost.exe" },
    SkipDirectorys = new List<string> { "logs", "cache", "temp" },
};
```

### FileTree（文件树对比）

```csharp
using GeneralUpdate.Common.FileBasic;

var tree = new FileTree();
var snapshot = tree.CreateSnapshot(@"C:\Program Files\MyApp");
// 或从 StorageManager 获取比较结果
```

---

## 6. 差分引擎（v10.4.6 可用，需安装 Differential 包）

安装 `GeneralUpdate.Differential` 包后可用：

```csharp
// DifferentialCore 提供核心差分能力
using GeneralUpdate.Differential;

// 清理模式（服务端）：对比新旧版本生成补丁
await DifferentialCore.CleanAsync(srcDir, tgtDir, patchDir);

// 脏模式（客户端）：应用补丁
await DifferentialCore.DirtyAsync(installDir, patchDir);
```

自定义匹配器（v10.4.6 可用）：

```csharp
using GeneralUpdate.Differential.Matchers;

// 自定义清理匹配器
var cleanMatcher = new DefaultCleanMatcher();  // 或实现 ICleanMatcher
var dirtyMatcher = new DefaultDirtyMatcher();  // 或实现 IDirtyMatcher
```

---

## 7. AOT / NativeAOT 兼容性

GeneralUpdate.Core v10.4.6 支持 .NET Native AOT：

```xml
<PropertyGroup>
  <IsAotCompatible>true</IsAotCompatible>
  <EnableTrimAnalyzer>true</EnableTrimAnalyzer>
</PropertyGroup>
```

JSON 序列化上下文（减少 AOT 大小）：

```csharp
using GeneralUpdate.Common.Internal.JsonContext;

// 使用内置的 JsonSerializerContext
// VersionRespJsonContext, PacketJsonContext, ProcessInfoJsonContext 等
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
| Pipeline 管道 | ✅ v10.4.6 | `GeneralUpdate.Common.Internal.Pipeline` |
| 策略系统 | ✅ v10.4.6 | `GeneralUpdate.Common.Internal.Strategy` |
| FileTree | ✅ v10.4.6 | `GeneralUpdate.Common.FileBasic` |
| BlackList | ✅ v10.4.6 | `Configinfo.BlackFiles` 等属性 |
| 差分引擎 | ✅ 需 `GeneralUpdate.Differential` | `DifferentialCore` |
| AOT | ✅ v10.4.6 | `JsonSerializerContext` 子类 |
| EventManager | ✅ v10.4.6 | `GeneralUpdate.Common.Internal.Event` |
| Bowl 崩溃守护 | ⚠️ 基础类型 | `GeneralUpdate.Bowl.Bowl` |
| IUpdateHooks | ❌ v10.4.6 不支持 | 开发分支 v10.5.0-beta.2 中 |
| 自定义 Strategy 注入 | ❌ v10.4.6 不支持 | 开发分支 v10.5.0-beta.2 中 |
| IPC 替换接口 | ❌ v10.4.6 不支持 | 开发分支 v10.5.0-beta.2 中 |
| SilentPollOrchestrator | ❌ v10.4.6 不支持 | 开发分支 v10.5.0-beta.2 中 |
| Option 系统 | ❌ v10.4.6 不支持 | 仅 Configinfo 属性 |

---

## 相关技能

- [🚀 generalupdate-init — Bootstrap 配置](./generalupdate-init.md)
- [⚙️ generalupdate-strategy — 更新策略选择](./generalupdate-strategy.md)
- [🩺 generalupdate-troubleshoot — 问题诊断](./generalupdate-troubleshoot.md)
