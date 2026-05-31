---
sidebar_position: 6
---

# GeneralUpdate.Differential

`GeneralUpdate.Differential` 是 GeneralUpdate 的二进制差分组件，专注解决“一个旧文件 + 一个补丁文件 = 一个新文件”的问题。它提供可替换的文件级差分算法、补丁压缩抽象和 BSDIFF 兼容补丁读写能力；目录级对比、批量补丁生成、并行调度、删除文件处理和更新流程编排由 `GeneralUpdate.Core` 的 `DiffPipeline` 或 `GeneralUpdate.Tools` 承担。

**命名空间:** `GeneralUpdate.Differential`、`GeneralUpdate.Differential.Differ`、`GeneralUpdate.Differential.Abstractions`

**主要入口:** `IBinaryDiffer`、`BsdiffDiffer`、`StreamingHdiffDiffer`

**NuGet 包:** `GeneralUpdate.Differential`

```bash
dotnet add package GeneralUpdate.Differential
```

## 文档大纲与知识点导航 {#knowledge-map}

如果你第一次阅读 Differential 文档，可以先看这个导航，再跳到对应知识点。本文按照“能力边界 -> 文件级 API -> 算法选择 -> 压缩格式 -> 与 Core/Tools 集成 -> 性能与扩展”的顺序组织。

| 你想了解什么 | 推荐阅读 |
| --- | --- |
| Differential 到底负责什么、不负责什么 | [组件能力边界](#组件能力边界) |
| `Clean` / `Dirty` 是什么含义 | [Clean 与 Dirty 语义](#clean-与-dirty-语义) |
| 如何给单个文件生成并应用补丁 | [单文件快速开始](#单文件快速开始) |
| 使用 Core 时是否还要手动集成 Differential | [与 GeneralUpdate.Core 的关系](#与-generalupdatecore-的关系) |
| 当前有哪些差分算法，如何选择 | [差分算法选择](#差分算法选择) |
| BSDIFF 补丁格式和压缩字节怎么工作 | [补丁格式与压缩 Provider](#补丁格式与压缩-provider) |
| 如何在 Core 更新流程里启用目录级差分 | [与 GeneralUpdate.Core 的关系](#与-generalupdatecore-的关系) |
| Tools 构建差分包时用了什么能力 | [与 GeneralUpdate.Tools 的关系](#与-generalupdatetools-的关系) |
| 下载和差分是否可以多线程并行 | [并发模型与性能建议](#并发模型与性能建议) |
| 大型项目如何提升差分构建效率 | [大型项目并行差分](#大型项目并行差分) |
| 如何接入自定义差分算法或压缩方式 | [扩展点](#扩展点) |

## 组件能力边界

Differential 是底层文件补丁库，不是完整的更新编排器。理解这个边界可以避免把旧文档里的 `DifferentialCore`、黑名单、目录批量处理等概念误认为当前组件 API。

| 能力 | Differential 是否负责 | 说明 |
| --- | --- | --- |
| 单文件二进制补丁生成 | 是 | 通过 `IBinaryDiffer.CleanAsync(oldFile, newFile, patchFile)` 完成。 |
| 单文件二进制补丁应用 | 是 | 通过 `IBinaryDiffer.DirtyAsync(oldFile, outputNewFile, patchFile)` 完成。 |
| 差分算法实现 | 是 | 当前主要实现为 `BsdiffDiffer` 和 `StreamingHdiffDiffer`。 |
| 补丁数据压缩/解压 | 是 | 通过 `ICompressionProvider` 抽象，内置 BZip2、Deflate，源码中预留 .NET 6+ Brotli。 |
| 目录级新旧版本对比 | 否 | 由 `GeneralUpdate.Core.Pipeline.DiffPipeline` 的 matcher 负责。 |
| 新增文件复制、删除清单、批量 patch 命名 | 否 | 由 `DiffPipeline` 负责生成 `.patch` 文件、复制新增文件和写入 `generalupdate.delete.json`。 |
| 更新包生成工具 | 否 | 推荐由 `GeneralUpdate.Tools` 调用 Core 差分管道生成发布产物。 |
| 下载、校验、解压、版本回写、重启 | 否 | 这些属于 `GeneralUpdate.Core` 更新流程。 |

> 当前源码中没有旧文档提到的 `DifferentialCore` 单例。直接使用 Differential 组件时，请面向 `IBinaryDiffer` 和具体 differ 实现编程；需要目录级能力时使用 Core 的 `DiffPipeline`。

## Clean 与 Dirty 语义 {#clean-与-dirty-语义}

Differential 沿用了 GeneralUpdate 差分流程中的两个术语：

| 术语 | 方法 | 输入 | 输出 | 常用位置 |
| --- | --- | --- | --- | --- |
| `Clean` | `CleanAsync` | 旧文件、新文件、补丁路径 | `.patch` 补丁文件 | 构建/发布阶段 |
| `Dirty` | `DirtyAsync` | 旧文件、输出新文件路径、补丁路径 | 还原后的新文件 | 客户端升级阶段 |

文件级补丁应用不会直接覆盖旧文件，而是把还原结果写到你传入的 `newFilePath`。Core 的 `DiffPipeline` 在目录级更新时会先写临时文件，成功后再替换原文件，从而避免补丁应用失败时破坏原文件。

## 单文件快速开始

下面示例只演示 Differential 的底层单文件能力。如果你已经在使用 `GeneralUpdate.Core`，Core 默认已经集成 Differential，不需要为了正常更新流程再手动集成或直接调用本组件。如果你要比较两个目录、生成一批 `.patch`、复制新增文件或处理删除文件，请直接看 [与 GeneralUpdate.Core 的关系](#与-generalupdatecore-的关系)。

```csharp
using GeneralUpdate.Differential.Abstractions;
using GeneralUpdate.Differential.Differ;

IBinaryDiffer differ = new BsdiffDiffer();

var oldFile = @"D:\releases\1.0.0\app.dll";
var newFile = @"D:\releases\1.0.1\app.dll";
var patchFile = @"D:\patches\app.dll.patch";
var outputFile = @"D:\restore\app.dll";

// 生成补丁：oldFile + newFile -> patchFile
await differ.CleanAsync(oldFile, newFile, patchFile);

// 应用补丁：oldFile + patchFile -> outputFile
await differ.DirtyAsync(oldFile, outputFile, patchFile);
```

`CleanAsync` 和 `DirtyAsync` 都支持 `CancellationToken`。当前实现会在任务开始和 Core 管道调度点观察取消请求；单个算法内部不是每一个字节循环都检查取消，因此大文件取消可能会等到当前文件处理结束后才完全停下。

## 核心 API

### IBinaryDiffer

`IBinaryDiffer` 是所有文件级差分算法的统一抽象，也是 Core 差分管道接入自定义算法的关键接口。

```csharp
public interface IBinaryDiffer
{
    Task DirtyAsync(
        string oldFilePath,
        string newFilePath,
        string patchFilePath,
        CancellationToken cancellationToken = default);

    Task CleanAsync(
        string oldFilePath,
        string newFilePath,
        string patchFilePath,
        CancellationToken cancellationToken = default);
}
```

| 参数 | 含义 |
| --- | --- |
| `oldFilePath` | 旧版本文件路径。生成补丁和应用补丁时都需要。 |
| `newFilePath` | `CleanAsync` 中表示新版本源文件；`DirtyAsync` 中表示还原后的输出文件。 |
| `patchFilePath` | 补丁文件路径。`CleanAsync` 写入它，`DirtyAsync` 读取它。 |

### BsdiffDiffer

`BsdiffDiffer` 实现 BSDIFF 4.0 文件级二进制差分算法。它会把旧文件和新文件读入内存，通过后缀排序寻找匹配块，再输出控制段、差异段和额外段。

```csharp
using GeneralUpdate.Differential.Differ;

var differ = new BsdiffDiffer();
await differ.CleanAsync(oldFile, newFile, patchFile);
await differ.DirtyAsync(oldFile, outputFile, patchFile);
```

| 特性 | 说明 |
| --- | --- |
| 默认压缩 | `BZip2CompressionProvider`，兼容历史 BSDIFF 补丁。 |
| 可替换压缩 | 构造函数接受 `ICompressionProvider`。 |
| 补丁兼容 | 支持 32 字节旧 BSDIFF 头，也支持 33 字节扩展头。 |
| 适用场景 | 追求兼容性、补丁体积稳定、单文件体积可控的场景。 |
| 资源特征 | 生成补丁时会读入旧文件和新文件，单文件很大时需要关注内存占用。 |

`BsdiffDiffer` 也保留了 `Clean(...)` 和 `Dirty(...)` 方法；新代码建议优先面向 `IBinaryDiffer.CleanAsync` / `DirtyAsync`，便于切换算法。

### StreamingHdiffDiffer

`StreamingHdiffDiffer` 是当前源码中的另一种 differ 实现。它使用块级 FNV-1a 哈希索引预筛候选位置，再进行字节级扩展匹配，输出 BSDIFF 兼容的补丁结构。

```csharp
using GeneralUpdate.Differential.Abstractions;
using GeneralUpdate.Differential.Differ;

var differ = new StreamingHdiffDiffer(
    compressionProvider: new DeflateCompressionProvider(optimalLevel: true),
    blockSize: 64 * 1024,
    maxWindowSize: 128 * 1024 * 1024);

await differ.CleanAsync(oldFile, newFile, patchFile);
await differ.DirtyAsync(oldFile, outputFile, patchFile);
```

| 特性 | 说明 |
| --- | --- |
| 默认压缩 | `DeflateCompressionProvider`。 |
| 块大小 | `BlockSize` 默认 64 KB，用于建立旧文件块哈希索引。 |
| 窗口预算 | `MaxWindowSize` 默认 128 MB，影响生成补丁时参与计算的内存窗口。 |
| 应用补丁 | `DirtyAsync` 委托给 `BsdiffDiffer` 的补丁应用逻辑。 |
| 适用场景 | 需要更快候选匹配、希望与 Core `DiffPipeline` 默认算法保持一致的目录级差分构建。 |

需要注意的是，当前实现不是完整外存流式差分：当单个文件超过 `MaxWindowSize` 时，算法只会读取预算窗口参与计算。对超大单文件，请在业务侧验证补丁还原结果，或调大 `MaxWindowSize`，或改用 `BsdiffDiffer` 等更适合当前文件规模的实现。

## 差分算法选择

当前 Differential 内置两种文件级差分算法。它们都输出 BSDIFF 兼容补丁结构，但生成补丁时的匹配方式、默认压缩、性能侧重点不同。

| 对比项 | `BsdiffDiffer` | `StreamingHdiffDiffer` |
| --- | --- | --- |
| 核心思路 | 经典 BSDIFF 4.0，基于后缀排序寻找旧文件和新文件之间的最长匹配。 | 使用块级 FNV-1a 哈希建立旧文件索引，先用哈希快速筛选候选块，再做字节级扩展匹配。 |
| 默认压缩 | BZip2 (`0x00`)。 | Deflate (`0x01`)。 |
| 补丁应用 | 自己实现 BSDIFF Dirty 逻辑。 | `DirtyAsync` 委托给 `BsdiffDiffer`，因此应用阶段和 BSDIFF 补丁兼容。 |
| 生成效率 | 匹配更精细，局部或分散变化下补丁生成表现稳定；但后缀排序和全量读入会带来 CPU/内存开销。 | 块命中效果好时生成更快；如果变化分散、块哈希命中少，生成可能变慢。 |
| 客户端应用性能 | 默认 BZip2 解压成本更高，客户端应用大量补丁时耗时可能更明显。 | 默认 Deflate 解压更快，更适合客户端批量应用补丁。 |
| 补丁体积倾向 | 通常更追求细粒度匹配，补丁体积明显更稳定。 | 速度优先，补丁体积与文件变化分布、块大小、窗口预算强相关；块命中差时可能接近完整文件。 |
| 内存特征 | 生成阶段读取旧文件和新文件，单个大文件需要关注内存峰值。 | 通过 `BlockSize` 和 `MaxWindowSize` 控制匹配窗口，超大单文件需要额外验证或调参。 |
| 兼容性 | 最适合需要兼容旧 BSDIFF/BZip2 补丁的场景。 | 适合新项目、目录级批量差分和 Core `DiffPipeline` 默认构建。 |

可以简单理解为：`BsdiffDiffer` 更偏“兼容和补丁体积稳定”，`StreamingHdiffDiffer` 更偏“客户端应用速度和可调参数”。如果项目非常在意补丁体积或文件变化较分散，优先考虑 `BsdiffDiffer`；如果项目更在意客户端应用速度，并且经过压测确认补丁体积可接受，可以考虑 `StreamingHdiffDiffer`。

### 参考基准数据 {#benchmark-reference}

下面数据来自当前源码的一组本地微基准，用于给开发者判断量级，不是跨所有项目的性能承诺。测试环境为 Windows x64、.NET Release 构建，使用 2-4 MB 合成文件；真实结果会受 CPU、磁盘、文件类型、变化比例、压缩级别和并行度影响。

| 场景 | `BsdiffDiffer` 生成 | `StreamingHdiffDiffer` 生成 | `BsdiffDiffer` 应用 | `StreamingHdiffDiffer` 应用 | `BsdiffDiffer` 补丁体积 | `StreamingHdiffDiffer` 补丁体积 |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| 2 MB 文本，少量行变更/插入 | 484 ms | 2059 ms | 55 ms | 36 ms | 0.05% | 3.50% |
| 4 MB 二进制，连续局部块变更 | 1030 ms | 318 ms | 55 ms | 11 ms | 2.58% | 100.04% |
| 4 MB 二进制，随机分散字节变更 | 757 ms | 4176 ms | 70 ms | 30 ms | 2.18% | 100.27% |

从这组数据可以得到几个实用预估：

| 指标 | 参考结论 |
| --- | --- |
| 补丁体积 | `BsdiffDiffer` 在测试场景中约为新文件的 0.05%-2.58%；`StreamingHdiffDiffer` 约为 3.50%-100%。如果补丁包大小是第一优先级，优先测试 `BsdiffDiffer`。 |
| 客户端应用速度 | `StreamingHdiffDiffer` 默认 Deflate，在测试中应用补丁约快 1.5-5 倍。大量文件批量应用时，这个差距会更明显。 |
| 生成速度 | 没有绝对赢家：连续局部二进制变更中 `StreamingHdiffDiffer` 约快 3.2 倍；文本和随机分散变更中 `BsdiffDiffer` 约快 4.3-5.5 倍。 |
| 大型项目选择 | 大型项目建议同时看“补丁总体积 + 构建耗时 + 客户端应用耗时”。如果大量文件可以并行，`WithParallelism(...)` 往往比单个 differ 的微小差距更影响总体耗时。 |

> 这组数据的重点是帮助判断方向：`BsdiffDiffer` 通常更容易得到小补丁，`StreamingHdiffDiffer` 的应用阶段更快，但补丁体积和生成速度对文件变化形态非常敏感。正式发布前建议用自己项目的真实产物做一次压测。

推荐选择：

| 场景 | 建议 |
| --- | --- |
| 只需要低层单文件补丁，并希望最大兼容 | 使用 `new BsdiffDiffer()`。 |
| 通过 Core `DiffPipeline` 批量生成目录级补丁 | 先用默认配置跑基准；若补丁体积偏大，可显式切换到 `BsdiffDiffer`；再结合 `WithParallelism(...)` 提升吞吐。 |
| 客户端解压性能更敏感 | 优先选择 Deflate 补丁，即 `StreamingHdiffDiffer` 默认配置，或 `new BsdiffDiffer(new DeflateCompressionProvider())`。 |
| 历史补丁仍是旧 BSDIFF/BZip2 | 使用 `BsdiffDiffer` 应用；32 字节头会按 BZip2 处理。 |
| 大型项目包含大量 DLL、资源文件、插件文件 | 使用 Core `DiffPipeline` 做文件级并行，避免自己逐个文件串行调用 Differential。 |

## 补丁格式与压缩 Provider {#补丁格式与压缩-provider}

Differential 生成的是 BSDIFF 风格补丁。当前实现写入 33 字节扩展头：

| 偏移 | 长度 | 含义 |
| --- | --- | --- |
| `0` | 8 | 魔数 `"BSDIFF40"`。 |
| `8` | 8 | 压缩后控制段长度。 |
| `16` | 8 | 压缩后差异段长度。 |
| `24` | 8 | 新文件长度。 |
| `32` | 1 | 压缩格式版本。 |

应用补丁时也兼容 32 字节旧头：如果没有第 33 个格式字节，就按 BZip2 旧补丁处理。

### ICompressionProvider

`ICompressionProvider` 负责把控制段、差异段和额外段包装成压缩流。

```csharp
public interface ICompressionProvider
{
    byte FormatVersion { get; }

    Stream CreateCompressStream(
        Stream output,
        CancellationToken cancellationToken = default);

    Stream CreateDecompressStream(
        Stream input,
        CancellationToken cancellationToken = default);
}
```

| Provider | 格式字节 | 当前可用性 | 说明 |
| --- | --- | --- | --- |
| `BZip2CompressionProvider` | `0x00` | 可用 | `BsdiffDiffer` 默认值，兼容旧 BSDIFF 补丁。 |
| `DeflateCompressionProvider` | `0x01` | 可用 | BCL `DeflateStream`，解压速度更适合客户端更新。 |
| `BrotliCompressionProvider` | `0x02` | 源码中以 `NET6_0_OR_GREATER` 条件编译预留 | 当前 `GeneralUpdate.Differential` 项目目标为 `netstandard2.0`，并且补丁读取逻辑当前只识别 `0x00` / `0x01`，不要把 Brotli 作为生产更新包格式。 |

自定义压缩时，生成补丁和应用补丁必须使用能被补丁读取逻辑识别的格式字节。当前生产建议只使用 BZip2 或 Deflate。

```csharp
using GeneralUpdate.Differential.Abstractions;
using GeneralUpdate.Differential.Differ;

var differ = new BsdiffDiffer(
    new DeflateCompressionProvider(optimalLevel: false));

await differ.CleanAsync(oldFile, newFile, patchFile);
await differ.DirtyAsync(oldFile, outputFile, patchFile);
```

## 与 GeneralUpdate.Core 的关系 {#与-generalupdatecore-的关系}

`GeneralUpdate.Core` 在 Differential 之上提供目录级差分管道 `DiffPipeline`。它会负责：

1. 对比旧目录和新目录。
2. 找出发生变化的文件并调用 `IBinaryDiffer.CleanAsync` 生成 `.patch`。
3. 复制新增文件到补丁目录。
4. 生成 `generalupdate.delete.json` 记录删除文件。
5. 客户端应用补丁时并行调用 `IBinaryDiffer.DirtyAsync`，先写临时文件，成功后替换原文件。

如果你在应用更新流程中使用 `GeneralUpdate.Core`，Core 默认已经集成 Differential 并内置差分管道。也就是说，常规更新接入时不需要额外安装、初始化或手动调用 `GeneralUpdate.Differential`；只要使用 Core 的更新流程，并按业务需要启用补丁更新能力，Core 会在内部完成 differ 创建、补丁应用和目录级编排。

只有在你想替换默认差分算法、调整并行度、改变错误策略或接入自定义 matcher 时，才需要通过 `UseDiffPipeline` 做高级配置：

```csharp
using GeneralUpdate.Core;
using GeneralUpdate.Core.Models;
using GeneralUpdate.Differential.Differ;

await new GeneralUpdateBootstrap()
    .SetSource(
        updateUrl: "https://update.example.com/api/upgrade/verification",
        appSecretKey: "your-app-secret")
    .SetOption(Option.AppType, AppType.Client)
    .SetOption(Option.PatchEnabled, true)
    .UseDiffPipeline(builder => builder
        .UseDiffer(new StreamingHdiffDiffer())
        .WithParallelism(4)
        .WithStopOnFirstError(true))
    .LaunchAsync();
```

当前源码里有两个默认层级需要区分：

| 使用方式 | 默认 differ |
| --- | --- |
| 直接 `new DiffPipeline()` 或 `new DiffPipelineBuilder().Build()` | `StreamingHdiffDiffer` |
| `GeneralUpdateBootstrap` 未显式调用 `UseDiffPipeline(...)` 时内部构建 | `BsdiffDiffer`，并行度 2，带 `DiffProgressReporter` |

因此，普通用户可以把 Differential 看作 Core 已经带好的底层能力，不需要特地集成；只有希望 Core 更新流程明确使用某个算法或自定义差分行为时，才建议显式调用 `UseDiffPipeline(...)`。

## 与 GeneralUpdate.Tools 的关系 {#与-generalupdatetools-的关系}

`GeneralUpdate.Tools` 面向发布侧，帮助开发者构建更新产物。当前 `DiffService` 会创建 `new DiffPipeline()`，再调用：

```csharp
await pipeline.CleanAsync(oldDir, newDir, patchDir);
```

也就是说，Tools 生成目录级差分包时，本质上使用的是 Core 的 `DiffPipeline`，而 `DiffPipeline` 再调用 Differential 的 `IBinaryDiffer` 生成每个变更文件的补丁。对大多数开发者来说，推荐路径是：

1. 用 Tools 对比旧版本目录和新版本目录，生成补丁目录和清单产物。
2. 用 Core 在客户端检查版本、下载补丁包、应用补丁。
3. 只有在需要自定义差分算法、压缩格式或单文件补丁实验时，才直接使用 Differential。

这种分层可以让业务代码保持简单：Tools 负责构建，Core 负责更新，Differential 负责底层文件差分。

## 并发模型与性能建议

Differential 的单个 differ 实例没有保存某次补丁任务的可变共享状态。只要传入的 `ICompressionProvider` 是线程安全的，内置 differ 可以被 Core 管道并发调用；内置 BZip2、Deflate provider 都会为每次调用创建新的压缩流，适合并发使用。

真正的“多线程差分”通常发生在 Core `DiffPipeline` 层：

```csharp
var pipeline = new DiffPipelineBuilder()
    .UseDiffer(new StreamingHdiffDiffer())
    .WithParallelism(4)
    .Build();

await pipeline.CleanAsync(oldDir, newDir, patchDir);
```

### 大型项目并行差分 {#大型项目并行差分}

大型桌面项目通常不是“一个超大文件”，而是由主程序、多个 DLL、插件、资源文件、运行时文件和配置文件组成。Core `DiffPipeline` 会把目录对比结果拆成文件级任务，每个变更文件独立调用 `IBinaryDiffer.CleanAsync` 生成补丁，因此可以通过 `WithParallelism(...)` 同时处理多个文件。

这种并行模型对大型项目很重要：

1. 构建侧可以同时为多个变更文件生成 `.patch`，缩短发布包构建时间。
2. 客户端应用补丁时也可以并行处理多个文件，减少升级窗口。
3. 新增文件复制、删除清单处理和差分补丁生成由 Core 管道统一编排，开发者不需要手写多线程调度。
4. 并行度可以按机器能力调整，构建机可以设置更高，低配置客户端可以保持较低。

| 参数/策略 | 建议 |
| --- | --- |
| `WithParallelism(1)` | 资源敏感、机械硬盘、低内存环境。 |
| `WithParallelism(2)` | 默认平衡值，适合多数桌面应用。 |
| `WithParallelism(4-8)` | 多核 CPU、SSD、构建机或发布服务器。 |
| BZip2 | 补丁兼容性好，但客户端解压成本更高。 |
| Deflate | 解压速度更友好，适合客户端大批量应用补丁。 |
| 大文件 | 先压测补丁生成耗时、内存峰值和还原结果，不要只看补丁体积。 |

并行差分适合“文件数量多、每个文件可独立处理”的大型项目。需要注意的是，单个超大文件内部仍由具体 differ 算法处理，不会因为 `WithParallelism(8)` 就把一个文件拆成 8 份并行计算；并行度提升的是多个文件之间的吞吐。

下载与差分可以在上层更新流程中并行：Core 下载阶段可以并发拉取多个资源，差分应用阶段也可以按文件并行处理补丁。Differential 只负责单个文件的补丁计算，不直接管理网络下载线程。

## 扩展点

### 自定义差分算法

实现 `IBinaryDiffer` 后即可接入 Core 管道。适合接入其他算法、调用原生库，或对特定文件类型做特殊优化。

```csharp
using GeneralUpdate.Differential.Abstractions;

public sealed class MyBinaryDiffer : IBinaryDiffer
{
    public Task CleanAsync(
        string oldFilePath,
        string newFilePath,
        string patchFilePath,
        CancellationToken cancellationToken = default)
    {
        // Generate patchFilePath from oldFilePath and newFilePath.
        throw new NotImplementedException();
    }

    public Task DirtyAsync(
        string oldFilePath,
        string newFilePath,
        string patchFilePath,
        CancellationToken cancellationToken = default)
    {
        // Restore newFilePath from oldFilePath and patchFilePath.
        throw new NotImplementedException();
    }
}
```

```csharp
var pipeline = new DiffPipelineBuilder()
    .UseDiffer(new MyBinaryDiffer())
    .WithParallelism(4)
    .Build();
```

自定义算法需要保证 `CleanAsync` 产出的补丁能被同一算法的 `DirtyAsync` 正确应用；如果补丁要交给 Core 客户端使用，发布侧和客户端必须使用同一套 differ 实现。

### 自定义压缩 Provider

如果仍使用 BSDIFF 兼容补丁结构，只想替换控制段、差异段和额外段的压缩方式，可以实现 `ICompressionProvider`。

```csharp
using GeneralUpdate.Differential.Abstractions;

public sealed class MyCompressionProvider : ICompressionProvider
{
    public byte FormatVersion => 0x01;

    public Stream CreateCompressStream(
        Stream output,
        CancellationToken cancellationToken = default)
    {
        return new DeflateStream(
            output,
            CompressionLevel.Optimal,
            leaveOpen: true);
    }

    public Stream CreateDecompressStream(
        Stream input,
        CancellationToken cancellationToken = default)
    {
        return new DeflateStream(
            input,
            CompressionMode.Decompress,
            leaveOpen: true);
    }
}
```

不要随意分配新的 `FormatVersion`。当前 `BsdiffDiffer.DirtyAsync` 只识别 BZip2 (`0x00`) 和 Deflate (`0x01`)；如果你引入新格式，也需要同步扩展补丁读取逻辑，否则客户端无法应用补丁。

## 实战建议

| 场景 | 推荐做法 |
| --- | --- |
| 普通应用发布差分更新 | 使用 `GeneralUpdate.Tools` 生成产物，客户端使用 Core。 |
| 需要控制目录级并行、错误策略和进度 | 使用 Core `DiffPipelineBuilder`。 |
| 只验证某个文件的补丁效果 | 直接使用 `IBinaryDiffer`。 |
| 对补丁体积和应用速度都敏感 | 对同一组文件分别测试 BZip2、Deflate 和不同算法后再定默认策略。 |
| 更新包需要长期兼容旧客户端 | 保守使用 `BsdiffDiffer` + BZip2，或确保客户端已支持 Deflate 扩展头。 |

Differential 的价值在于把复杂的二进制差分能力收敛成稳定的文件级抽象。上层开发者可以把重点放在“什么时候更新、下载什么、如何提示用户”上，把具体补丁生成和应用交给 Core/Tools/Differential 的组合完成。
