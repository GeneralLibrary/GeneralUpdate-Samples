---
sidebar_position: 3
---

# GeneralUpdate.Bowl

**命名空间:** `GeneralUpdate.Bowl` | **主要入口:** `new Bowl().LaunchAsync(BowlContext, CancellationToken)` | **NuGet 包:** `GeneralUpdate.Bowl`

## 1. 组件简介

### 1.1 组件概述

**GeneralUpdate.Bowl** 是升级完成后的启动守护组件。它不负责下载、解压或替换升级包，而是在新版本文件落地后、主程序即将启动或已启动时，监控目标进程是否在启动阶段崩溃。如果捕获到崩溃，它会生成 Dump 内存快照、写出失败报告 JSON、导出系统诊断信息，并在升级模式下自动将备份目录恢复回安装目录，避免用户一直停留在不可启动的新版本上。

**核心能力：**

| 能力 | 说明 |
| --- | --- |
| 进程崩溃监控 | 通过 ProcDump（Windows/Linux）或 lldb（macOS）附加到目标进程，捕获启动期未处理异常 |
| Dump 内存快照 | 支持 Full / Mini / Heap 三种 Dump 类型，可按需选择文件大小和完整度 |
| 崩溃报告生成 | 自动生成包含监控参数和工具输出的 `{version}_fail.json` 报告文件 |
| 系统诊断导出 | Windows 下自动导出驱动列表、系统信息和最近系统事件日志 |
| 自动回滚恢复 | 升级模式下将备份目录覆盖复制回安装目录，实现一键回退到旧版本 |
| 失败版本标记 | 写入 `UpgradeFail` 标记，Core 后续跳过该失败版本直到服务端提供更高版本 |
| 事件回调通知 | `OnCrash` 回调允许上传诊断包、通知用户或记录审计信息 |
| 独立监控模式 | Normal 模式只做崩溃捕获和报告输出，不自动恢复备份，适合通用进程监控 |

**解决的业务痛点：**
- 新版本升级后在启动阶段崩溃，用户无法使用应用且无法自行回退
- 开发者缺少崩溃现场信息（Dump、系统环境）来定位"升级后打不开"的问题
- 需要自动化回滚机制降低升级风险，避免人工介入

**业务使用场景：**
- 桌面应用升级后启动健康检查与自动回滚保护
- 通用进程启动崩溃监控与诊断信息采集
- CI/CD 冒烟测试失败后的自动诊断

:::warning Bowl 不是更新组件，是守护组件
Bowl **不负责**下载、解压或替换升级包。它的唯一职责是：在新版本文件落地后、主程序启动时，监控目标进程是否在启动阶段崩溃。如果发现崩溃 → 生成诊断信息 → 可选地自动回滚。它应该放在升级流程的**最后一步**——在文件替换完成后才启动守护。
:::

### 1.2 环境与依赖

| 项目 | 说明 |
| --- | --- |
| **版本** | `10.5.0-beta.6` |
| **目标框架** | `netstandard2.0`（兼容 .NET Framework 4.6.1+ / .NET Core 2.0+ / .NET 5+） |
| **依赖包** | `System.Collections.Immutable`, `System.Text.Json` |
| **内置工具** | Windows: `procdump.exe` / `procdump64.exe` / `procdump64a.exe`；Linux: `procdump` deb/rpm 包 + `install.sh`；macOS: `/usr/bin/lldb` |
| **兼容性** | Windows（完整支持）/ Linux（deb/rpm 发行版）/ macOS（基础支持，受 SIP 和调试权限限制） |

---

## 2. 组件功能列表

| 功能名称 | 功能描述 | 类型 | 是否必填 | 备注限制 |
| --- | --- | --- | --- | --- |
| 升级模式监控 | 监控新版本启动崩溃，自动恢复备份、标记失败版本 | 基础 | 推荐 | `WorkModel = "Upgrade"` |
| 独立监控模式 | 仅捕获崩溃、生成报告，不自动恢复 | 基础 | 可选 | `WorkModel = "Normal"` |
| Full Dump | 完整内存快照，信息最完整 | 基础 | 可选 | `DumpType.Full`，文件最大 |
| Mini Dump | 小型内存快照，生成更快 | 基础 | 可选 | `DumpType.Mini`，生产环境推荐 |
| Heap Dump | 带堆信息的小型 Dump | 基础 | 可选 | `DumpType.Heap`，介于 Mini 和 Full 之间 |
| 崩溃报告 JSON | 自动生成结构化崩溃报告文件 | 基础 | 自动 | 输出到 `FailDirectory` |
| 系统诊断导出 | Windows 下导出驱动/系统信息/事件日志 | 拓展 | 自动 | Windows only |
| 自动备份恢复 | 崩溃后将备份目录覆盖回安装目录 | 基础 | 可选 | `AutoRestore = true` |
| 失败版本标记 | 写入升级失败版本，Core 后续跳过 | 基础 | 自动 | 升级模式下生效 |
| 崩溃回调通知 | 检测到崩溃后触发业务回调 | 拓展 | 可选 | `OnCrash` 回调函数 |
| 日志追踪 | `GeneralTracer` 运行时诊断日志 | 拓展 | 可选 | 默认开启，可关闭 |

---

## 3. API 配置说明

### 3.1 配置字段（属性 Props）

**BowlContext：**

| 字段名 | 数据类型 | 默认值 | 是否必填 | 枚举/取值范围 | 说明 |
| --- | --- | --- | --- | --- | --- |
| `ProcessNameOrId` | `string` | — | 是 | 进程名或 PID | 要监控的目标进程名称或进程 ID |
| `DumpFileName` | `string` | — | 是 | 有效文件名 | Dump 输出文件名，推荐 `"{version}_fail.dmp"` |
| `FailFileName` | `string` | — | 是 | 有效文件名 | 崩溃报告 JSON 文件名，推荐 `"{version}_fail.json"` |
| `TargetPath` | `string` | — | 是 | 有效目录路径 | 应用安装根目录，恢复备份时覆盖复制到这里 |
| `FailDirectory` | `string` | — | 是 | 有效目录路径 | 故障文件输出目录，推荐 `{TargetPath}/fail/{version}` |
| `BackupDirectory` | `string` | — | 推荐 | 有效目录路径 | 升级前备份目录，`AutoRestore` 打开时必须存在 |
| `WorkModel` | `string` | `"Upgrade"`（`Normalize()` 后） | 可选 | `"Upgrade"` / `"Normal"` | 工作模式：升级回滚 / 独立监控 |
| `ExtendedField` | `string` | `null` | 可选 | — | 扩展字段，通常存储版本号，升级模式下写入 `UpgradeFail` |
| `TimeoutMs` | `int` | `30000`（`Normalize()` 后） | 可选 | 正整数（毫秒） | 监控子进程超时时间，按应用启动耗时调整 |
| `DumpType` | `DumpType` | `DumpType.Full`（`Normalize()` 后） | 可选 | `Full(0)`, `Mini(1)`, `Heap(2)` | Dump 捕获类型 |
| `AutoRestore` | `bool` | `false` | 可选 | `true` / `false` | 是否自动恢复备份，升级模式需显式设为 `true` |
| `OnCrash` | `Func<CrashInfo, CancellationToken, Task>?` | `null` | 可选 | — | 崩溃事件回调，仅在检测到 Dump 后触发 |

**DumpType 枚举：**

| 枚举值 | 数值 | Windows ProcDump 参数 | 特点 |
| --- | --- | --- | --- |
| `Full` | `0` | `-ma` | 完整内存快照，信息最完整，文件最大 |
| `Mini` | `1` | `-mm` | 小型快照，生成快、文件小，适合生产默认采集 |
| `Heap` | `2` | `-mh` | 带堆信息小型快照，介于 Mini 和 Full 之间 |

### 3.2 实例方法

**Bowl：**

| 方法名 | 入参明细 | 使用场景 | 注意事项 |
| --- | --- | --- | --- |
| `LaunchAsync(BowlContext, CancellationToken)` | `context` — 执行上下文（建议先调 `Normalize()`）；`ct` — 取消令牌 | 启动崩溃监控守护流程 | 三阶段：准备监控 → 运行监控 → 检测到 Dump 则进入故障处理管线 |

**BowlContext：**

| 方法名 | 入参明细 | 使用场景 | 注意事项 |
| --- | --- | --- | --- |
| `Normalize()` | 无 | 应用默认值（`WorkModel` → `"Upgrade"`，`TimeoutMs` → `30000`，`DumpType` → `Full`） | 返回新实例，不修改原实例 |

### 3.3 回调事件

| 事件名称 | 回调参数 | 触发时机 | 使用说明 |
| --- | --- | --- | --- |
| `OnCrash` | `CrashInfo` — `DumpFilePath`, `CrashReportPath`, `Version`, `ExitCode`；`CancellationToken` | 检测到 Dump 文件后触发 | 适合上传诊断包、通知用户"新版本已回退"、记录业务审计。回调异常被 Bowl 记录到追踪日志，不会阻止 `LaunchAsync` 返回 |

**GeneralTracer 日志控制：**

| 方法 | 说明 |
| --- | --- |
| `GeneralTracer.SetTracingEnabled(false)` | 关闭 Bowl 日志输出 |
| `GeneralTracer.SetTracingEnabled(true)` | 重新开启日志输出 |
| `GeneralTracer.IsTracingEnabled()` | 查询当前日志开关状态 |
| `GeneralTracer.Dispose()` | 释放文件监听器并清空 Trace listeners |

---

## 4. 扩展示例（高阶用法）

### 4.1 组件可扩展能力总览

Bowl 的主要扩展点是 `BowlContext.OnCrash` 回调。内部策略接口（`IBowlStrategy`、`ICrashReporter`、`ISystemInfoProvider`）为 internal，如需新增平台支持或自定义报告逻辑，可在 GeneralUpdate 仓库贡献代码。

| 扩展点 | 类型 | 说明 |
| --- | --- | --- |
| `OnCrash` 回调 | `Func<CrashInfo, CancellationToken, Task>?` | `BowlContext` 中配置，崩溃时触发 |
| `GeneralTracer` 日志 | 静态类 | 可通过 `SetTracingEnabled` 开关日志 |

### 4.2 分场景示例

#### 场景 1：升级模式监控 + 崩溃告警上传

【场景说明】桌面应用升级到新版本后，Bowl 监控启动崩溃；发生崩溃时自动回退，并上传诊断包到内部日志平台。

【示例代码】

```csharp
using GeneralUpdate.Bowl;

var version = "2.0.0";
var installPath = AppDomain.CurrentDomain.BaseDirectory;

var context = new BowlContext
{
    ProcessNameOrId = "MyApp.exe",
    DumpFileName = $"{version}_fail.dmp",
    FailFileName = $"{version}_fail.json",
    TargetPath = installPath,
    FailDirectory = Path.Combine(installPath, "fail", version),
    BackupDirectory = Path.Combine(installPath, version),
    WorkModel = "Upgrade",
    ExtendedField = version,
    TimeoutMs = 60_000,     // 应用启动较慢，给 60 秒
    DumpType = DumpType.Mini,
    AutoRestore = true,
    OnCrash = async (info, ct) =>
    {
        // 打包诊断文件
        var zipPath = Path.Combine(
            Path.GetDirectoryName(info.DumpFilePath)!,
            $"crash_{info.Version}_{DateTimeOffset.Now:yyyyMMddHHmmss}.zip");

        System.IO.Compression.ZipFile.CreateFromDirectory(
            Path.GetDirectoryName(info.DumpFilePath)!, zipPath);

        // 上传到日志平台
        using var client = new HttpClient();
        var content = new MultipartFormDataContent();
        content.Add(new StreamContent(File.OpenRead(zipPath)), "file", Path.GetFileName(zipPath));
        content.Add(new StringContent(info.Version), "version");
        content.Add(new StringContent(info.ExitCode.ToString()), "exitCode");

        await client.PostAsync("https://logs.example.com/api/crash", content, ct);

        // 通知用户
        Console.WriteLine($"Version {info.Version} crashed (exit code {info.ExitCode}).");
        Console.WriteLine($"Diagnostics uploaded. Previous version restored.");
    }
};

BowlResult result = await new Bowl().LaunchAsync(context);

if (result.DumpCaptured && result.Restored)
    Console.WriteLine("Crash detected and backup restored.");
else if (!result.DumpCaptured)
    Console.WriteLine("Process started successfully.");
```

【效果&注意事项】
- `TimeoutMs` 需要大于应用正常启动时间
- `DumpType.Mini` 生成更快，适合生产环境；疑难问题再切换到 `Full`
- 回调异常不会阻断恢复流程

#### 场景 2：独立监控模式（非升级场景）

【场景说明】对 Worker 进程做通用启动崩溃监控，只采集诊断信息，不自动回滚。

【示例代码】

```csharp
var context = new BowlContext
{
    ProcessNameOrId = "MyWorker.exe",
    DumpFileName = "startup_fail.dmp",
    FailFileName = "startup_fail.json",
    TargetPath = AppDomain.CurrentDomain.BaseDirectory,
    FailDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fail", "startup"),
    BackupDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "backup"),
    WorkModel = "Normal",
    TimeoutMs = 15_000,
    DumpType = DumpType.Mini,
    AutoRestore = false,
    OnCrash = (info, ct) =>
    {
        Console.WriteLine($"Worker crashed: {info.DumpFilePath}");
        return Task.CompletedTask;
    }
};

BowlResult result = await new Bowl().LaunchAsync(context);

if (result.DumpCaptured)
{
    Console.WriteLine($"Dump captured at: {result.DumpFilePath}");
    Console.WriteLine($"Report at: {result.CrashReportPath}");
}
```

【效果&注意事项】
- `WorkModel = "Normal"` 不会恢复备份也不会标记 `UpgradeFail`
- 适合通用进程监控、CI 测试守护等非升级场景

---

## 5. 常规使用示例

### 5.1 快速入门示例（最简 demo）

```csharp
using GeneralUpdate.Bowl;

var context = new BowlContext
{
    ProcessNameOrId = "MyApp.exe",
    DumpFileName = "fail.dmp",
    FailFileName = "fail.json",
    TargetPath = AppDomain.CurrentDomain.BaseDirectory,
    FailDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fail"),
    BackupDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "backup"),
}.Normalize();  // 应用默认值

BowlResult result = await new Bowl().LaunchAsync(context);
Console.WriteLine($"Success: {result.Success}, Dump captured: {result.DumpCaptured}");
```

### 5.2 基础参数组合示例

```csharp
var version = "2.0.0";
var installPath = @"C:\Program Files\MyApp";

var context = new BowlContext
{
    ProcessNameOrId = "MyApp.exe",
    DumpFileName = $"{version}_fail.dmp",
    FailFileName = $"{version}_fail.json",
    TargetPath = installPath,
    FailDirectory = Path.Combine(installPath, "fail", version),
    BackupDirectory = Path.Combine(installPath, version),
    WorkModel = "Upgrade",
    ExtendedField = version,
    TimeoutMs = 30_000,
    DumpType = DumpType.Full,
    AutoRestore = true,
    OnCrash = (info, ct) =>
    {
        Console.WriteLine($"Crash: {info.DumpFilePath}");
        return Task.CompletedTask;
    }
};

BowlResult result = await new Bowl().LaunchAsync(context);

if (result.DumpCaptured && result.Restored)
    Console.WriteLine("The upgraded version crashed and the backup was restored.");
```

### 5.3 真实业务落地示例

完整升级后 Bowl 守护流程，包含升级程序侧集成：

```csharp
using GeneralUpdate.Bowl;

// --- 升级程序（Update.exe）侧 ---
// 1. 主程序已停止，升级程序完成文件替换
// 2. 备份旧版本到 BackupDirectory
// 3. 启动 Bowl 监控新版本

var version = "2.0.0";
var installPath = @"C:\Program Files\MyApp";

var bowlContext = new BowlContext
{
    ProcessNameOrId = "MyApp.exe",
    DumpFileName = $"{version}_fail.dmp",
    FailFileName = $"{version}_fail.json",
    TargetPath = installPath,
    FailDirectory = Path.Combine(installPath, "fail", version),
    BackupDirectory = Path.Combine(installPath, "backups", version),
    WorkModel = "Upgrade",
    ExtendedField = version,
    TimeoutMs = 45_000,     // 应用冷启动约 30s，留 15s buffer
    DumpType = DumpType.Mini,
    AutoRestore = true,
    OnCrash = async (info, ct) =>
    {
        try
        {
            // 上传诊断信息
            using var client = new HttpClient();
            var crashData = new
            {
                version = info.Version,
                exitCode = info.ExitCode,
                dumpPath = info.DumpFilePath,
                reportPath = info.CrashReportPath,
                timestamp = DateTimeOffset.UtcNow
            };
            await client.PostAsJsonAsync(
                "https://monitor.mycompany.com/api/crash-report",
                crashData, ct);
        }
        catch (Exception ex)
        {
            // 上报失败不影响恢复流程
            Console.WriteLine($"Failed to upload crash report: {ex.Message}");
        }
    }
};

var result = await new Bowl().LaunchAsync(bowlContext);

if (result.Success)
{
    Console.WriteLine("New version started successfully.");
}
else if (result.DumpCaptured)
{
    Console.WriteLine($"New version crashed (exit code: {result.ExitCode}).");
    Console.WriteLine($"Backup restored: {result.Restored}");
    Console.WriteLine($"Dump: {result.DumpFilePath}");
    Console.WriteLine($"Report: {result.CrashReportPath}");
}
else
{
    Console.WriteLine($"Process exited abnormally (exit code: {result.ExitCode}), but no dump was captured.");
}
```

---

## 6. 全局配置

Bowl 不依赖全局配置文件。所有配置通过 `BowlContext` 传入。日志行为通过静态类 `GeneralTracer` 控制。

### 日志开关

```csharp
// 性能敏感场景关闭日志
GeneralTracer.SetTracingEnabled(false);

var result = await new Bowl().LaunchAsync(context);

// 排查问题时重新开启
GeneralTracer.SetTracingEnabled(true);
```

### 输出文件结构

一次升级失败后，推荐按版本存放所有故障文件：

```text
MyApp/
  fail/
    2.0.0/
      2.0.0_fail.dmp         # Dump 内存快照
      2.0.0_fail.json        # 崩溃报告 JSON
      driverInfo.txt          # Windows 驱动列表
      systeminfo.txt          # OS/硬件/内存信息
      systemlog.evtx          # Windows 系统事件日志
  Logs/
    generalupdate-trace 2026-01-01.log  # Bowl 自身追踪日志
```

### 平台差异

| 平台 | 监控工具 | 诊断导出 | 注意事项 |
| --- | --- | --- | --- |
| Windows | 内置 ProcDump（`procdump.exe`/`procdump64.exe`/`procdump64a.exe`） | 支持 `driverInfo.txt`、`systeminfo.txt`、`systemlog.evtx` | 需要足够权限生成 Dump |
| Linux | 内置 deb/rpm 包 + `install.sh` 安装 ProcDump | 当前为 no-op | 支持 Ubuntu/Debian/RHEL/CentOS/Fedora/ClearOS |
| macOS | `/usr/bin/lldb` | 当前为 no-op | 受 SIP、调试权限、签名策略影响，基础实现 |

---

## 相关资源

- [Bowl 示例代码](https://github.com/GeneralLibrary/GeneralUpdate-Samples/tree/main/src/Bowl)
- [GeneralUpdate 仓库](https://github.com/GeneralLibrary/GeneralUpdate)
