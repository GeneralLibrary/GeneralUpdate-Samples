---
sidebar_position: 6
---

# GeneralUpdate.Differential

**命名空间:** `GeneralUpdate.Differential` | **主要入口:** `IBinaryDiffer`、`BsdiffDiffer`、`StreamingHdiffDiffer` | **NuGet 包:** `GeneralUpdate.Differential`

## 1. 组件简介

### 1.1 组件概述

**GeneralUpdate.Differential** 是 GeneralUpdate 的二进制差分组件，专注解决"一个旧文件 + 一个补丁文件 = 一个新文件"的问题。它提供可替换的文件级差分算法（BSDIFF 4.0 / Streaming HDiff）、补丁压缩抽象（BZip2 / Deflate，源码中预留 .NET 6+ Brotli）和 BSDIFF 兼容补丁读写能力。

目录级对比、批量补丁生成、并行调度、删除文件处理和更新流程编排由 `GeneralUpdate.Core` 的 `DiffPipeline` 或 `GeneralUpdate.Tools` 承担。

**核心能力：**

| 能力 | 说明 |
| --- | --- |
| 文件级差分生成 | `CleanAsync(oldFile, newFile, patchFile)` — 对比新旧文件生成 `.patch` 补丁 |
| 文件级差分应用 | `DirtyAsync(oldFile, newFile, patchFile)` — 旧文件 + 补丁 → 新文件 |
| 可替换差分算法 | `BsdiffDiffer`（BSDIFF 4.0，后缀排序）和 `StreamingHdiffDiffer`（块哈希索引） |
| 可替换压缩格式 | BZip2 (0x00)、Deflate (0x01)，源码中通过 `#if NET6_0_OR_GREATER` 条件编译预留 Brotli (0x02) |
| BSDIFF 兼容格式 | 写入 33 字节扩展头（32 字节 BSDIFF40 + 1 字节压缩格式），兼容 32 字节旧头 |
| 线程安全 | 内置 differ 和压缩提供器均支持并发调用 |

**解决的业务痛点：**
- 全量更新带宽成本高，差分更新可将更新包从 GB 级降低到 MB 甚至 KB 级
- 不同文件类型和变化模式需要不同的差分策略（细粒度匹配 vs 快速块匹配）
- 压缩算法的选择影响客户端解压速度和补丁体积的平衡

:::note Differential 是底层库，通常不直接使用
大多数场景下你不需要直接调用 `IBinaryDiffer`。目录级差分、批量补丁生成、并行调度和更新流程编排由 `GeneralUpdate.Core` 的 `DiffPipeline` 或 `GeneralUpdate.Tools` 承担。Differential 只解决一个原子问题：**一个旧文件 + 一个补丁文件 = 一个新文件**。
:::

**业务使用场景：**
- 大型桌面应用（多 DLL、资源文件）的增量更新
- 固件/驱动包的二进制差分分发
- 游戏资源热更新
- CI/CD 发布流水线中自动生成增量补丁包

### 1.2 环境与依赖

| 项目 | 说明 |
| --- | --- |
| **版本** | `10.5.0-beta.6` |
| **目标框架** | `netstandard2.0`（兼容 .NET Framework 4.6.1+ / .NET Core 2.0+ / .NET 5+） |
| **依赖包** | 无外部依赖（纯 .NET BCL） |
| **兼容性** | 所有支持 .NET Standard 2.0 的平台 |

---

## 2. 组件功能列表

| 功能名称 | 功能描述 | 类型 | 是否必填 | 备注限制 |
| --- | --- | --- | --- | --- |
| BSDIFF 4.0 差分生成 | 基于后缀排序的经典差分算法，补丁体积稳定 | 基础 | 可选 | `BsdiffDiffer`，默认 BZip2 压缩 |
| BSDIFF 4.0 补丁应用 | 将 BSDIFF 格式补丁应用到旧文件 | 基础 | 可选 | 支持 32/33 字节两种头部格式 |
| Streaming HDiff 差分生成 | 基于 FNV-1a 块哈希索引的快速差分 | 基础 | 可选 | `StreamingHdiffDiffer`，默认 Deflate 压缩 |
| BZip2 压缩 | 补丁控制段/差异段/额外段的 BZip2 压缩 | 基础 | 可选 | 格式字节 `0x00`，`BsdiffDiffer` 默认 |
| Deflate 压缩 | 补丁段的 Deflate 压缩，解压更快 | 基础 | 可选 | 格式字节 `0x01`，`StreamingHdiffDiffer` 默认 |
| 自定义差分算法 | 实现 `IBinaryDiffer` 接入自研算法 | 拓展 | 可选 | 需保证 Clean/Dirty 一致性 |
| 自定义压缩提供器 | 实现 `ICompressionProvider` 替换压缩方式 | 拓展 | 可选 | 新格式字节需配合扩展补丁读取逻辑 |

---

## 3. API 配置说明

### 3.1 配置字段（属性 Props）

Differential 本身是底层库，不提供配置类。所有参数通过构造函数传入。

**BsdiffDiffer 构造参数：**

| 字段名 | 数据类型 | 默认值 | 是否必填 | 枚举/取值范围 | 说明 |
| --- | --- | --- | --- | --- | --- |
| `compressionProvider` | `ICompressionProvider` | `BZip2CompressionProvider` | 可选 | `BZip2CompressionProvider` / `DeflateCompressionProvider` | 补丁压缩提供器 |

**StreamingHdiffDiffer 构造参数：**

| 字段名 | 数据类型 | 默认值 | 是否必填 | 枚举/取值范围 | 说明 |
| --- | --- | --- | --- | --- | --- |
| `compressionProvider` | `ICompressionProvider` | `DeflateCompressionProvider` | 可选 | `BZip2CompressionProvider` / `DeflateCompressionProvider` | 补丁压缩提供器 |
| `blockSize` | `int` | `65536`（64 KB） | 可选 | 正整数字节数 | 块大小，用于旧文件哈希索引 |
| `maxWindowSize` | `int` | `134217728`（128 MB） | 可选 | 正整数字节数 | 参与计算的最大内存窗口 |

**DeflateCompressionProvider 构造参数：**

| 字段名 | 数据类型 | 默认值 | 是否必填 | 枚举/取值范围 | 说明 |
| --- | --- | --- | --- | --- | --- |
| `optimalLevel` | `bool` | `true` | 可选 | `true` / `false` | `true` = `CompressionLevel.Optimal`，`false` = `CompressionLevel.Fastest` |

**ICompressionProvider 格式标识：**

| Provider | 格式字节 | 可用性 | 说明 |
| --- | --- | --- | --- |
| `BZip2CompressionProvider` | `0x00` | 完全可用 | BSDIFF 旧补丁兼容，解压成本较高 |
| `DeflateCompressionProvider` | `0x01` | 完全可用 | 解压速度更友好，适合客户端批量应用 |
| `BrotliCompressionProvider` | `0x02` | 仅 .NET 6+ 编译（源码中为完整实现，通过 `#if NET6_0_OR_GREATER` 条件编译） | 当前 `netstandard2.0` 包中不包含，生产不建议使用 |

### 3.2 实例方法

**IBinaryDiffer：**

| 方法名 | 入参明细 | 使用场景 | 注意事项 |
| --- | --- | --- | --- |
| `CleanAsync(string, string, string, CancellationToken)` | `oldFilePath` — 旧文件路径；`newFilePath` — 新文件路径；`patchFilePath` — 补丁输出路径；`cancellationToken` | 发布/构建阶段生成补丁 | 大文件取消不会立即响应，需等待当前文件处理完成 |
| `DirtyAsync(string, string, string, CancellationToken)` | `oldFilePath` — 旧文件路径；`newFilePath` — 补丁还原后文件输出路径；`patchFilePath` — 补丁文件路径；`cancellationToken` | 客户端升级阶段应用补丁 | 不会直接覆盖旧文件，结果写入 `newFilePath` |

### 3.3 回调事件

Differential 不发布事件。进度报告和事件通知由 Core 的 `DiffPipeline` 通过 `DiffProgress` 和 `EventManager` 实现。

---

## 4. 扩展示例（高阶用法）

### 4.1 组件可扩展能力总览

| 扩展接口 | 说明 |
| --- | --- |
| `IBinaryDiffer` | 自定义文件级差分算法，可接入原生库或自研算法 |
| `ICompressionProvider` | 自定义补丁段压缩方式 |

### 4.2 分场景示例

#### 场景 1：自定义差分算法

【场景说明】接入企业内部自研的高压缩率差分算法。

【示例代码】

```csharp
using GeneralUpdate.Differential.Abstractions;

public sealed class HighRatioDiffer : IBinaryDiffer
{
    public Task CleanAsync(
        string oldFilePath,
        string newFilePath,
        string patchFilePath,
        CancellationToken cancellationToken = default)
    {
        // 调用自研算法生成补丁
        // NativeMethods.GeneratePatch(oldFilePath, newFilePath, patchFilePath);
        return Task.CompletedTask;
    }

    public Task DirtyAsync(
        string oldFilePath,
        string newFilePath,
        string patchFilePath,
        CancellationToken cancellationToken = default)
    {
        // 调用自研算法应用补丁
        // NativeMethods.ApplyPatch(oldFilePath, patchFilePath, newFilePath);
        return Task.CompletedTask;
    }
}

// 在 Core DiffPipeline 中使用
using GeneralUpdate.Core.Pipeline;

var pipeline = new DiffPipelineBuilder()
    .UseDiffer(new HighRatioDiffer())
    .WithParallelism(4)
    .Build();

await pipeline.CleanAsync(oldDir, newDir, patchDir);
```

【效果&注意事项】
- 必须保证 `CleanAsync` 产出的补丁能被同一算法的 `DirtyAsync` 正确应用
- 发布侧和客户端必须使用同一套 differ 实现

#### 场景 2：自定义压缩提供器 + BsdiffDiffer

【场景说明】使用 BsdiffDiffer 算法 + Deflate 压缩，获得更快的客户端补丁应用速度。

【示例代码】

```csharp
using GeneralUpdate.Differential.Abstractions;
using GeneralUpdate.Differential.Differ;

// BsdiffDiffer 的精确匹配 + Deflate 的快速解压
var differ = new BsdiffDiffer(
    new DeflateCompressionProvider(optimalLevel: false));

await differ.CleanAsync(oldFile, newFile, patchFile);
await differ.DirtyAsync(oldFile, outputFile, patchFile);
```

【效果&注意事项】
- `optimalLevel: false` 生成更快，适合开发/CI 环境
- `optimalLevel: true` 补丁体积更小，适合生产环境
- 生成和消费两侧都需要能识别 Deflate 格式（`0x01`）

#### 场景 3：StreamingHdiffDiffer 参数调优

【场景说明】大型单文件（200MB+）的差分，调整窗口预算避免内存溢出。

【示例代码】

```csharp
using GeneralUpdate.Differential.Abstractions;
using GeneralUpdate.Differential.Differ;

// 大文件场景：增大窗口，减小块大小以获得更精细匹配
var differ = new StreamingHdiffDiffer(
    compressionProvider: new DeflateCompressionProvider(optimalLevel: true),
    blockSize: 32 * 1024,        // 32 KB 块，更密集的哈希索引
    maxWindowSize: 256 * 1024 * 1024);  // 256 MB，允许读入更大文件

await differ.CleanAsync(oldLargeFile, newLargeFile, patchFile);
```

【效果&注意事项】
- `blockSize` 越小，哈希索引越密集，匹配更精确但内存消耗更大
- `maxWindowSize` 决定能参与计算的最大数据量，超出部分不参与匹配
- 超大文件建议先在业务侧压测补丁体积和应用还原结果

---

## 5. 常规使用示例

### 5.1 快速入门示例（最简 demo）

```csharp
using GeneralUpdate.Differential.Abstractions;
using GeneralUpdate.Differential.Differ;

IBinaryDiffer differ = new BsdiffDiffer();

var oldFile = @"D:\releases\1.0.0\app.dll";
var newFile = @"D:\releases\1.0.1\app.dll";
var patchFile = @"D:\patches\app.dll.patch";
var outputFile = @"D:\restore\app.dll";

// 生成补丁：oldFile + newFile → patchFile
await differ.CleanAsync(oldFile, newFile, patchFile);

// 应用补丁：oldFile + patchFile → outputFile
await differ.DirtyAsync(oldFile, outputFile, patchFile);

// 验证还原结果
var newHash = ComputeSha256(newFile);
var outputHash = ComputeSha256(outputFile);
Console.WriteLine(newHash == outputHash ? "Patch verified." : "MISMATCH!");
```

### 5.2 基础参数组合示例

```csharp
using GeneralUpdate.Differential.Abstractions;
using GeneralUpdate.Differential.Differ;

// 方案 A：经典 BSDIFF + BZip2 → 补丁体积最小
var differA = new BsdiffDiffer();

// 方案 B：经典 BSDIFF + Deflate → 补丁体积小 + 应用更快
var differB = new BsdiffDiffer(new DeflateCompressionProvider(optimalLevel: true));

// 方案 C：Streaming HDiff + Deflate → 生成快 + 应用最快
var differC = new StreamingHdiffDiffer(
    new DeflateCompressionProvider(optimalLevel: true),
    blockSize: 64 * 1024,
    maxWindowSize: 128 * 1024 * 1024);

// 对同一组文件测试三种方案，选择最优
foreach (var differ in new IBinaryDiffer[] { differA, differB, differC })
{
    var sw = Stopwatch.StartNew();
    await differ.CleanAsync(oldFile, newFile, patchFile);
    sw.Stop();

    var patchSize = new FileInfo(patchFile).Length;
    Console.WriteLine($"{differ.GetType().Name}: {sw.ElapsedMilliseconds}ms, {patchSize} bytes");
}
```

### 5.3 真实业务落地示例（通过 Core DiffPipeline 使用）

大多数情况下不直接使用 Differential，而是通过 Core 的 `DiffPipeline` 做目录级差分：

```csharp
using GeneralUpdate.Core;
using GeneralUpdate.Core.Pipeline;
using GeneralUpdate.Core.Models;
using GeneralUpdate.Differential.Differ;

// 构建端：对比新旧版本目录，生成补丁
var pipeline = new DiffPipelineBuilder()
    .UseDiffer(new StreamingHdiffDiffer())
    .WithParallelism(8)         // CI 构建机，高并行度
    .WithStopOnFirstError(true)
    .WithProgress(new Progress<DiffProgress>(p =>
    {
        Console.WriteLine($"[Build] {p.Completed}/{p.Total} {p.CurrentFile}");
    }))
    .Build();

await pipeline.CleanAsync(
    @"D:\builds\v1.0.0",
    @"D:\builds\v1.0.1",
    @"D:\patches\v1.0.0-to-v1.0.1");

// 客户端：通过 GeneralUpdateBootstrap 使用
await new GeneralUpdateBootstrap()
    .SetSource(
        updateUrl: "https://update.mycompany.com/api/upgrade/verification",
        appSecretKey: "prod-key")
    .SetOption(Option.AppType, AppType.Client)
    .SetOption(Option.PatchEnabled, true)
    .SetOption(Option.DiffMode, DiffMode.Parallel)
    .SetOption(Option.MaxConcurrency, 4)
    .UseDiffPipeline(builder => builder
        .UseDiffer(new StreamingHdiffDiffer())
        .WithParallelism(4))
    .LaunchAsync();
```

---

## 6. 算法选择指南

### Clean 与 Dirty 语义

| 术语 | 方法 | 输入 | 输出 | 常用位置 |
| --- | --- | --- | --- | --- |
| Clean | `CleanAsync` | 旧文件、新文件、补丁输出路径 | `.patch` 补丁文件 | 构建/发布阶段 |
| Dirty | `DirtyAsync` | 旧文件、输出新文件路径、补丁路径 | 还原后的新文件 | 客户端升级阶段 |

### 算法对比

| 对比维度 | `BsdiffDiffer` | `StreamingHdiffDiffer` |
| --- | --- | --- |
| 核心思路 | 经典 BSDIFF 4.0，后缀排序 + 最长匹配 | 块级 FNV-1a 哈希索引 + 字节级扩展匹配 |
| 默认压缩 | BZip2 (0x00) | Deflate (0x01) |
| 补丁应用 | 自实现 BSDIFF Dirty | 委托给 `BsdiffDiffer`（BSDIF 兼容） |
| 补丁体积 | 更稳定，通常更小 | 受文件变化分布影响大，块命中差时可能接近原文件大小 |
| 客户端应用速度 | BZip2 解压较慢 | Deflate 解压更快（约 1.5-5x） |
| 生成内存 | 全量读入新旧文件 | 按 `maxWindowSize` 预算读入 |
| 兼容性 | 兼容旧 BSDIFF/BZip2 补丁 | 适合新项目 |

### 场景推荐

| 场景 | 推荐 |
| --- | --- |
| 补丁体积优先 | `BsdiffDiffer` + BZip2 |
| 客户端应用速度优先 | `StreamingHdiffDiffer`（默认 Deflate） |
| 兼容旧补丁格式 | `BsdiffDiffer`（32 字节旧头自动按 BZip2 处理） |
| 大文件（>500MB） | 先压测，可能需要 `StreamingHdiffDiffer` + 调大 `maxWindowSize` |
| 目录级批量差分 | 通过 Core `DiffPipeline`，配合 `WithParallelism` 提升吞吐 |
| 新项目 | 先用默认配置跑基准，再根据体积和速度需求调整 |

### 并发模型

- 单个 `IBinaryDiffer` 实例线程安全（当 `ICompressionProvider` 线程安全时）
- 真正的多线程差分在 Core `DiffPipeline` 层，通过 `WithParallelism(n)` 控制
- BZip2 / Deflate 内置 provider 均为每次调用创建新流，支持并发

---

## 相关资源

- [GeneralUpdate 仓库](https://github.com/GeneralLibrary/GeneralUpdate)
- [Samples 差分示例](https://github.com/GeneralLibrary/GeneralUpdate-Samples/tree/main/src/Hub/Samples/DifferentialSample.cs)
- [Core DiffPipeline 文档](GeneralUpdate.Core)
