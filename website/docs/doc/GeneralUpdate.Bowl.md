---
sidebar_position: 3
---

# GeneralUpdate.Bowl

## 简介

**GeneralUpdate.Bowl** 是升级完成后的启动守护组件。它不负责下载、解压或替换升级包，而是在新版本文件落地、主程序即将启动或已经启动时，监控目标进程是否在启动阶段崩溃。如果捕获到崩溃，它会生成 Dump、写出失败报告、导出诊断信息，并在升级模式下把备份目录恢复回安装目录，避免用户一直停留在不可启动的新版本上。

**命名空间：** `GeneralUpdate.Bowl`

**程序集：** `GeneralUpdate.Bowl.dll`

**当前主要入口：** `new Bowl().LaunchAsync(BowlContext context, CancellationToken ct = default)`

## 阅读导航

| 主题 | 适合解决的问题 |
| --- | --- |
| [生命周期位置](#生命周期位置) | Bowl 应该在升级流程的哪个阶段运行 |
| [快速接入](#快速接入) | 用当前 `BowlContext` API 完成一次监控 |
| [崩溃检测与恢复流程](#崩溃检测与恢复流程) | 崩溃后组件具体做了什么 |
| [BowlContext 参数](#bowlcontext-参数) | 每个配置项的含义和推荐值 |
| [输出文件](#输出文件) | Dump、失败报告、系统诊断、追踪日志在哪里 |
| [事件回调](#事件回调) | 如何在崩溃时上传报告或通知用户 |
| [日志开关](#日志开关) | 如何为了性能关闭组件追踪日志 |
| [平台差异](#平台差异) | Windows、Linux、macOS 的监控能力差异 |
| [恢复场景](#恢复场景) | 一次真实升级失败回滚过程 |
| [旧 API 迁移](#旧-api-迁移) | 从 `MonitorParameter` 迁移到 `BowlContext` |

## 生命周期位置

在 GeneralUpdate 的完整升级链路中，Bowl 位于**文件替换完成之后、用户正式使用新版本之前**：

1. Core 获取更新信息、下载包、校验并应用更新。
2. Core/Upgrade 进程准备启动主程序。
3. Bowl 作为守护逻辑启动，附加到目标进程并等待启动期异常。
4. 主程序正常启动：没有 Dump 产生，Bowl 返回本次监控结果。
5. 主程序启动崩溃：Bowl 进入故障处理管线，生成诊断文件并按配置恢复备份。

在当前 Core 代码中，Windows 的 `UpdateStrategy` 会在更新完成后通过 OS 策略启动主程序，并在配置了 Bowl 进程名时一并启动 Bowl 辅助进程。Linux/macOS 侧 Core 策略没有同等的 Bowl helper 自动启动能力，通常需要由你的启动器、服务脚本或独立进程显式调用 `LaunchAsync`。

:::tip
Bowl 是“升级后健康检查与回滚保护”，不是固件恢复、系统还原或升级包安装器。它处理的是应用启动崩溃后的诊断与应用目录级备份恢复。
:::

## 快速接入

### 安装

```bash
dotnet add package GeneralUpdate.Bowl
```

### 升级模式监控

升级模式适合放在升级程序或 Bowl helper 中运行。关键点是：`BackupDirectory` 指向升级前保留的备份，`TargetPath` 指向当前安装目录，`ExtendedField` 填本次升级版本号。

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
    TimeoutMs = 30_000,
    DumpType = DumpType.Full,
    AutoRestore = true,
    OnCrash = (info, ct) =>
    {
        Console.WriteLine($"Crash dump: {info.DumpFilePath}");
        Console.WriteLine($"Crash report: {info.CrashReportPath}");
        return Task.CompletedTask;
    }
};

BowlResult result = await new Bowl().LaunchAsync(context);

if (result.DumpCaptured && result.Restored)
{
    Console.WriteLine("The upgraded version crashed and the backup was restored.");
}
```

### 独立监控模式

`Normal` 模式只做崩溃捕获、报告输出和回调通知，不会自动恢复备份，也不会写入 `UpgradeFail` 失败版本标记。

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
    AutoRestore = false
};

BowlResult result = await new Bowl().LaunchAsync(context);
```

## 崩溃检测与恢复流程

`LaunchAsync` 的核心判断非常直接：平台策略先启动监控工具，监控工具输出到 `FailDirectory`；Bowl 再检查 `{FailDirectory}/{DumpFileName}` 是否存在。存在 Dump 就认为启动阶段发生了崩溃。

| 阶段 | 当前实现 |
| --- | --- |
| 准备监控 | 根据操作系统选择 `WindowsBowlStrategy`、`LinuxBowlStrategy` 或 `MacBowlStrategy` |
| 捕获异常 | Windows 使用 ProcDump；Linux 尝试安装并调用 ProcDump；macOS 使用 `lldb` 基础能力 |
| 判断崩溃 | 检查 `FailDirectory` 中是否生成指定 Dump 文件 |
| 生成报告 | 写出 `{version}_fail.json`，包含监控参数和监控工具输出 |
| 导出诊断 | Windows 调用 `Applications/Windows/export.bat` 导出驱动、系统信息和最近系统日志 |
| 恢复备份 | 仅当 `WorkModel == "Upgrade"` 且 `AutoRestore == true` 时，把 `BackupDirectory` 覆盖复制回 `TargetPath` |
| 标记失败版本 | 升级模式下写入 `UpgradeFail = ExtendedField`，Core 后续会跳过小于等于该失败版本的更新 |
| 通知业务 | 如果配置了 `OnCrash`，传出 Dump 路径、报告路径、版本号和退出码 |

`TimeoutMs` 是监控子进程的等待上限。超时且没有 Dump 时，Bowl 不会执行恢复管线；此时更应该关注 `DumpCaptured` 是否为 `true`，而不是只看 `Success`。

## BowlContext 参数

| 参数 | 说明 | 建议 |
| --- | --- | --- |
| `ProcessNameOrId` | 要监控的进程名或 PID | Windows 可使用进程名；Linux 上更建议传 PID |
| `DumpFileName` | Dump 文件名 | 推荐包含版本号，例如 `2.0.0_fail.dmp` |
| `FailFileName` | 崩溃报告 JSON 文件名 | 推荐和 Dump 同版本，例如 `2.0.0_fail.json` |
| `TargetPath` | 当前应用安装根目录 | 恢复备份时会覆盖复制到这里 |
| `FailDirectory` | 故障文件输出目录 | 推荐 `Path.Combine(TargetPath, "fail", version)` |
| `BackupDirectory` | 升级前备份目录 | `AutoRestore` 打开时必须确保目录存在且内容完整 |
| `WorkModel` | `Upgrade` 或 `Normal` | 升级后回滚用 `Upgrade`；普通崩溃采集用 `Normal` |
| `ExtendedField` | 扩展字段，当前主要存版本号 | 升级模式下会写入 `UpgradeFail` |
| `TimeoutMs` | 监控子进程超时时间 | 默认归一化为 30000 ms，按应用启动耗时调大 |
| `DumpType` | `Full`、`Mini`、`Heap` | 生产环境可先用 `Mini` 降低体积；疑难问题用 `Full` |
| `AutoRestore` | 是否自动恢复备份 | 升级模式要显式设置为 `true` |
| `OnCrash` | 单次崩溃回调 | 适合上传报告、通知用户、写入业务日志 |

### DumpType 选择

| 类型 | Windows ProcDump 参数 | 特点 |
| --- | --- | --- |
| `Full` | `-ma` | 信息最完整，文件最大，适合难复现问题 |
| `Mini` | `-mm` | 文件更小，生成更快，适合生产默认采集 |
| `Heap` | `-mh` | 带堆信息的小型 Dump，介于 Mini 和 Full 之间 |

## 输出文件

一次升级失败后，推荐按版本存放所有故障文件：

```text
MyApp/
  fail/
    2.0.0/
      2.0.0_fail.dmp
      2.0.0_fail.json
      driverInfo.txt
      systeminfo.txt
      systemlog.evtx
  Logs/
    generalupdate-trace 2026-01-01.log
```

| 文件 | 来源 | 内容 |
| --- | --- | --- |
| `{version}_fail.dmp` | ProcDump 或 lldb | 崩溃现场内存快照 |
| `{version}_fail.json` | `CrashReporter` | `BowlContext` 映射参数和监控工具输出行 |
| `driverInfo.txt` | Windows `driverquery` | Windows 驱动列表 |
| `systeminfo.txt` | Windows `systeminfo` | OS、硬件、内存等系统信息 |
| `systemlog.evtx` | Windows `wevtutil` | 最近一天 Windows System 事件日志 |
| `Logs/generalupdate-trace yyyy-MM-dd.log` | `GeneralTracer` | Bowl 自身运行追踪日志 |

非 Windows 平台当前不会导出 `driverInfo.txt`、`systeminfo.txt`、`systemlog.evtx`，但仍会尽量生成 Dump 和失败 JSON。

失败 JSON 的结构来自当前 `CrashReporter`：

```json
{
  "Parameter": {
    "TargetPath": "C:\\Program Files\\MyApp",
    "FailDirectory": "C:\\Program Files\\MyApp\\fail\\2.0.0",
    "BackupDirectory": "C:\\Program Files\\MyApp\\2.0.0",
    "ProcessNameOrId": "MyApp.exe",
    "DumpFileName": "2.0.0_fail.dmp",
    "FailFileName": "2.0.0_fail.json",
    "WorkModel": "Upgrade",
    "ExtendedField": "2.0.0"
  },
  "ProcdumpOutPutLines": [
    "ProcDump v11.0 - Sysinternals process dump utility",
    "[10:00:03] Dump 1 initiated: C:\\Program Files\\MyApp\\fail\\2.0.0\\2.0.0_fail.dmp",
    "[10:00:03] Dump count reached."
  ]
}
```

## 事件回调

`OnCrash` 是单次崩溃事件回调，只在检测到 Dump 后触发。它拿到的是整理后的 `CrashInfo`：

```csharp
public readonly record struct CrashInfo
{
    public string DumpFilePath { get; init; }
    public string CrashReportPath { get; init; }
    public string Version { get; init; }
    public int ExitCode { get; init; }
}
```

常见用途：

| 场景 | 做法 |
| --- | --- |
| 上传诊断包 | 在回调中打包 Dump、JSON 和 Windows 诊断文件，上传到内部日志平台 |
| 提示用户 | 告知“新版本启动失败，已恢复上一版本”，并附带问题编号 |
| 记录业务审计 | 把 `Version`、`ExitCode`、报告路径写入你的业务日志 |

回调异常会被 Bowl 记录到追踪日志中，不会阻止 `LaunchAsync` 返回最终 `BowlResult`。取消操作请通过 `CancellationToken` 传递。

## 日志开关

Bowl 使用公开的 `GeneralTracer` 写运行追踪。默认会输出到控制台，并在运行目录下按日期写入：

```text
Logs/generalupdate-trace yyyy-MM-dd.log
```

如果你的场景对启动性能、磁盘写入或控制台输出非常敏感，可以关闭追踪：

```csharp
GeneralTracer.SetTracingEnabled(false);

var result = await new Bowl().LaunchAsync(context);

GeneralTracer.SetTracingEnabled(true);
```

关闭后，Bowl 自身的诊断追踪会减少，但崩溃 Dump 和失败 JSON 的生成逻辑不依赖该开关。排查升级失败时建议保持开启；稳定生产环境可按你的性能策略关闭。

## 平台差异

| 平台 | 监控工具 | 诊断导出 | 注意事项 |
| --- | --- | --- | --- |
| Windows | 内置 ProcDump：`procdump.exe`、`procdump64.exe`、`procdump64a.exe` | 支持 `driverInfo.txt`、`systeminfo.txt`、`systemlog.evtx` | 监控工具路径来自 `TargetPath/Applications/Windows`；需要足够权限生成 Dump |
| Linux | 内置 deb/rpm 包 + `install.sh` 安装 ProcDump 后调用 `procdump` | 当前为 no-op | 支持 Ubuntu、Debian、RHEL、CentOS、Fedora、ClearOS 映射包；脚本可能需要 `sudo` |
| macOS | `/usr/bin/lldb` | 当前为 no-op | 受 SIP、调试权限、签名策略影响；当前是基础实现 |

NuGet 包会把 `Applications/**/*` 作为内容输出到构建目录。自部署时请确认这些工具文件没有被裁剪，否则平台策略可能返回“监控工具不可用”或进程启动失败。

## 恢复场景

假设用户从 `1.0.0` 升级到 `2.0.0`，新版本启动后立即崩溃：

1. 升级流程先把旧版本备份到 `BackupDirectory`，例如 `C:\Program Files\MyApp\2.0.0`。
2. 新版本文件被复制到 `TargetPath`。
3. 主程序启动，同时 Bowl 使用 `ProcessNameOrId = "MyApp.exe"` 监控启动期异常。
4. ProcDump 捕获到未处理异常，写出 `fail\2.0.0\2.0.0_fail.dmp`。
5. Bowl 写出 `2.0.0_fail.json`，Windows 下继续导出驱动、系统信息和最近系统日志。
6. 因为 `WorkModel == "Upgrade"` 且 `AutoRestore == true`，Bowl 将 `BackupDirectory` 覆盖复制回 `TargetPath`。
7. Bowl 写入 `UpgradeFail = "2.0.0"`；Core 下次检测到服务端仍返回 `2.0.0` 或更低版本时，会跳过这个已知失败版本，直到服务端提供更高版本。
8. `OnCrash` 回调可以上传诊断包，或提示用户已经回退到可用版本。

这个机制的目标是降低“升级成功但新版本打不开”的风险：用户回到可启动版本，开发者拿到 Dump 和上下文继续修复。

## 旧 API 迁移

旧示例中的 `GeneralUpdate.Bowl.Strategys.MonitorParameter` 已标记为过时，推荐迁移到 `BowlContext` 和异步入口：

```csharp
var oldParameter = new GeneralUpdate.Bowl.Strategys.MonitorParameter
{
    ProcessNameOrId = "MyApp.exe",
    DumpFileName = "2.0.0_fail.dmp",
    FailFileName = "2.0.0_fail.json",
    TargetPath = installPath,
    FailDirectory = Path.Combine(installPath, "fail", "2.0.0"),
    BackupDirectory = Path.Combine(installPath, "2.0.0"),
    WorkModel = "Upgrade",
    ExtendedField = "2.0.0"
};

BowlContext context = Bowl.MapToContext(oldParameter);
BowlResult result = await new Bowl().LaunchAsync(context);
```

如果是新代码，直接创建 `BowlContext`，不要再依赖旧 `MonitorParameter`。

## 相关资源

- **示例代码**：[GeneralUpdate-Samples / Bowl](https://github.com/GeneralLibrary/GeneralUpdate-Samples/tree/main/src/Bowl)
- **主仓库**：[GeneralUpdate](https://github.com/GeneralLibrary/GeneralUpdate)
