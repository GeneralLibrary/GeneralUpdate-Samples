---
sidebar_position: 6
---

# GeneralUpdate.Differential

**Namespace:** `GeneralUpdate.Differential` | **Main Entry Points:** `IBinaryDiffer`, `BsdiffDiffer`, `StreamingHdiffDiffer` | **NuGet Package:** `GeneralUpdate.Differential`

## 1. Component Overview

### 1.1 Introduction

**GeneralUpdate.Differential** is the binary differential component of GeneralUpdate, focused on solving "one old file + one patch file = one new file". It provides pluggable file-level diff algorithms (BSDIFF 4.0 / Streaming HDiff), patch compression abstractions (BZip2 / Deflate / Brotli reserved), and BSDIFF-compatible patch read/write capabilities.

Directory-level comparison, batch patch generation, parallel scheduling, deleted file handling, and update workflow orchestration are handled by `GeneralUpdate.Core`'s `DiffPipeline` or `GeneralUpdate.Tools`.

**Core Capabilities:**

| Capability | Description |
| --- | --- |
| File-Level Diff Generation | `CleanAsync(oldFile, newFile, patchFile)` — compare old & new files to generate `.patch` |
| File-Level Diff Application | `DirtyAsync(oldFile, newFile, patchFile)` — old file + patch → new file |
| Pluggable Diff Algorithms | `BsdiffDiffer` (BSDIFF 4.0, suffix sort) and `StreamingHdiffDiffer` (block hash indexing) |
| Pluggable Compression | BZip2 (0x00), Deflate (0x01), .NET 6+ Brotli (0x02) available via conditional compilation |
| BSDIFF Compatible Format | 33-byte extended header (32-byte BSDIFF40 + 1-byte compression format), 32-byte legacy compatible |
| Thread Safety | Built-in differ and compression providers support concurrent calls |

**Business Problems Solved:**
- Full updates have high bandwidth costs; differential updates can reduce packages from GB to MB or KB
- Different file types and change patterns need different diff strategies
- Compression algorithm choice affects client decompression speed vs patch size trade-off

**Use Cases:**
- Incremental updates for large desktop apps (multiple DLLs, resource files)
- Binary differential distribution for firmware/driver packages
- Game resource hot updates
- Automated incremental patch generation in CI/CD pipelines

### 1.2 Environment & Dependencies

| Item | Description |
| --- | --- |
| **Version** | `10.5.0-beta.2` |
| **Target Framework** | `netstandard2.0` (.NET Framework 4.6.1+ / .NET Core 2.0+ / .NET 5+) |
| **Dependencies** | None (pure .NET BCL) |
| **Compatibility** | All .NET Standard 2.0 platforms |

---

## 2. Feature List

| Feature | Description | Type | Required | Notes |
| --- | --- | --- | --- | --- |
| BSDIFF 4.0 Diff Generation | Classic suffix-sort diff algorithm with stable patch sizes | Core | Optional | `BsdiffDiffer`, default BZip2 |
| BSDIFF 4.0 Patch Application | Apply BSDIFF format patches to old files | Core | Optional | Supports both 32/33 byte headers |
| Streaming HDiff Generation | FNV-1a block hash-based fast diff | Core | Optional | `StreamingHdiffDiffer`, default Deflate |
| BZip2 Compression | BZip2 compression for patch segments | Core | Optional | Format byte `0x00`, `BsdiffDiffer` default |
| Deflate Compression | Deflate compression for patch segments, faster decompression | Core | Optional | Format byte `0x01`, `StreamingHdiffDiffer` default |
| Custom Diff Algorithm | Implement `IBinaryDiffer` for proprietary algorithms | Extended | Optional | Must ensure Clean/Dirty consistency |
| Custom Compression Provider | Implement `ICompressionProvider` to swap compression | Extended | Optional | New format bytes require patch reader extension |

---

## 3. API Configuration Reference

### 3.1 Configuration Properties (Props)

Differential is a low-level library with no configuration classes. All parameters are passed via constructors.

**BsdiffDiffer Constructor Parameters:**

| Parameter | Type | Default | Required | Values | Description |
| --- | --- | --- | --- | --- | --- |
| `compressionProvider` | `ICompressionProvider` | `BZip2CompressionProvider` | Optional | `BZip2CompressionProvider` / `DeflateCompressionProvider` | Patch compression provider |

**StreamingHdiffDiffer Constructor Parameters:**

| Parameter | Type | Default | Required | Values | Description |
| --- | --- | --- | --- | --- | --- |
| `compressionProvider` | `ICompressionProvider` | `DeflateCompressionProvider` | Optional | `BZip2CompressionProvider` / `DeflateCompressionProvider` | Patch compression provider |
| `blockSize` | `int` | `65536` (64 KB) | Optional | Positive integer bytes | Block size for old file hash indexing |
| `maxWindowSize` | `int` | `134217728` (128 MB) | Optional | Positive integer bytes | Max memory window for computation |

**ICompressionProvider Format Identifiers:**

| Provider | Format Byte | Availability | Description |
| --- | --- | --- | --- |
| `BZip2CompressionProvider` | `0x00` | Fully available | Legacy BSDIFF compatible, higher decompression cost |
| `DeflateCompressionProvider` | `0x01` | Fully available | Faster decompression, better for client batch apply |
| `BrotliCompressionProvider` | `0x02` | .NET 6+ only (full implementation in source behind `#if NET6_0_OR_GREATER` conditional compilation) | Not included in current `netstandard2.0` package; not recommended for production |

### 3.2 Instance Methods

**IBinaryDiffer:**

| Method | Parameters | Returns | Use Case | Notes |
| --- | --- | --- | --- | --- |
| `CleanAsync(string, string, string, CancellationToken)` | `oldFilePath`, `newFilePath`, `patchFilePath`, `cancellationToken` | `Task` | Generate patch during build/release | Large file cancellation not immediate |
| `DirtyAsync(string, string, string, CancellationToken)` | `oldFilePath`, `newFilePath` (restored output), `patchFilePath`, `cancellationToken` | `Task` | Apply patch during client upgrade | Result written to `newFilePath`, not overwriting old file |

### 3.3 Callback Events

Differential does not publish events. Progress reporting and event notifications are implemented by Core's `DiffPipeline` via `DiffProgress` and `EventManager`.

---

## 4. Advanced Examples

### 4.1 Extension Points Overview

| Extension Interface | Description |
| --- | --- |
| `IBinaryDiffer` | Custom file-level diff algorithm; can integrate native libraries or proprietary algorithms |
| `ICompressionProvider` | Custom patch segment compression |

### 4.2 Examples by Scenario

#### Scenario 1: Custom Diff Algorithm

**Description:** Integrate an in-house high-compression-ratio diff algorithm.

```csharp
using GeneralUpdate.Differential.Abstractions;

public sealed class HighRatioDiffer : IBinaryDiffer
{
    public Task CleanAsync(
        string oldFilePath, string newFilePath, string patchFilePath,
        CancellationToken cancellationToken = default)
    {
        // Call proprietary algorithm to generate patch
        return Task.CompletedTask;
    }

    public Task DirtyAsync(
        string oldFilePath, string newFilePath, string patchFilePath,
        CancellationToken cancellationToken = default)
    {
        // Call proprietary algorithm to apply patch
        return Task.CompletedTask;
    }
}

// Use in Core DiffPipeline
var pipeline = new DiffPipelineBuilder()
    .UseDiffer(new HighRatioDiffer())
    .WithParallelism(4)
    .Build();
```

#### Scenario 2: Custom Compression Provider + BsdiffDiffer

**Description:** Use BsdiffDiffer's precise matching with Deflate's fast decompression for better client-side patch application speed.

```csharp
using GeneralUpdate.Differential.Abstractions;
using GeneralUpdate.Differential.Differ;

var differ = new BsdiffDiffer(
    new DeflateCompressionProvider(optimalLevel: false));

await differ.CleanAsync(oldFile, newFile, patchFile);
await differ.DirtyAsync(oldFile, outputFile, patchFile);
```

#### Scenario 3: StreamingHdiffDiffer Parameter Tuning

**Description:** Large single files (200MB+) need adjusted window budget to avoid OOM.

```csharp
var differ = new StreamingHdiffDiffer(
    compressionProvider: new DeflateCompressionProvider(optimalLevel: true),
    blockSize: 32 * 1024,           // 32 KB blocks for denser hash indexing
    maxWindowSize: 256 * 1024 * 1024);  // 256 MB for large files

await differ.CleanAsync(oldLargeFile, newLargeFile, patchFile);
```

---

## 5. Basic Usage Examples

### 5.1 Quick Start (Minimal Demo)

```csharp
using GeneralUpdate.Differential.Abstractions;
using GeneralUpdate.Differential.Differ;

IBinaryDiffer differ = new BsdiffDiffer();

var oldFile = @"D:\releases\1.0.0\app.dll";
var newFile = @"D:\releases\1.0.1\app.dll";
var patchFile = @"D:\patches\app.dll.patch";
var outputFile = @"D:\restore\app.dll";

// Generate patch: oldFile + newFile → patchFile
await differ.CleanAsync(oldFile, newFile, patchFile);

// Apply patch: oldFile + patchFile → outputFile
await differ.DirtyAsync(oldFile, outputFile, patchFile);
```

### 5.2 Basic Parameter Combination

```csharp
// Option A: Classic BSDIFF + BZip2 → smallest patch size
var differA = new BsdiffDiffer();

// Option B: Classic BSDIFF + Deflate → small patch + faster apply
var differB = new BsdiffDiffer(new DeflateCompressionProvider(optimalLevel: true));

// Option C: Streaming HDiff + Deflate → fast generation + fastest apply
var differC = new StreamingHdiffDiffer(
    new DeflateCompressionProvider(optimalLevel: true),
    blockSize: 64 * 1024,
    maxWindowSize: 128 * 1024 * 1024);
```

### 5.3 Production Usage (via Core DiffPipeline)

Most scenarios use Differential indirectly through Core's `DiffPipeline`:

```csharp
using GeneralUpdate.Core;
using GeneralUpdate.Core.Pipeline;
using GeneralUpdate.Core.Models;
using GeneralUpdate.Differential.Differ;

// Build side: compare old/new version directories, generate patches
var pipeline = new DiffPipelineBuilder()
    .UseDiffer(new StreamingHdiffDiffer())
    .WithParallelism(8)
    .WithProgress(new Progress<DiffProgress>(p =>
        Console.WriteLine($"[Build] {p.Completed}/{p.Total} {p.CurrentFile}")))
    .Build();

await pipeline.CleanAsync(@"D:\builds\v1.0.0", @"D:\builds\v1.0.1", @"D:\patches\v1.0.0-to-v1.0.1");

// Client side: via GeneralUpdateBootstrap
await new GeneralUpdateBootstrap()
    .SetSource(updateUrl: "https://update.mycompany.com/api/upgrade/verification", appSecretKey: "prod-key")
    .SetOption(Option.AppType, AppType.Client)
    .SetOption(Option.PatchEnabled, true)
    .UseDiffPipeline(builder => builder
        .UseDiffer(new StreamingHdiffDiffer())
        .WithParallelism(4))
    .LaunchAsync();
```

---

## 6. Algorithm Selection Guide

### Clean vs Dirty Semantics

| Term | Method | Input | Output | Typical Location |
| --- | --- | --- | --- | --- |
| Clean | `CleanAsync` | Old file, new file, patch output path | `.patch` file | Build/release phase |
| Dirty | `DirtyAsync` | Old file, output new file path, patch path | Restored new file | Client upgrade phase |

### Algorithm Comparison

| Dimension | `BsdiffDiffer` | `StreamingHdiffDiffer` |
| --- | --- | --- |
| Core Approach | Classic BSDIFF 4.0, suffix sort + longest match | FNV-1a block hash indexing + byte-level extension |
| Default Compression | BZip2 (0x00) | Deflate (0x01) |
| Patch Application | Self-implemented BSDIFF Dirty | Delegates to `BsdiffDiffer` (BSDIFF compatible) |
| Patch Size | More stable, typically smaller | Sensitive to change distribution |
| Client Apply Speed | BZip2 decompression slower | Deflate decompression faster (~1.5-5x) |
| Generation Memory | Full read of old & new files | Budgeted by `maxWindowSize` |
| Compatibility | Compatible with legacy BSDIFF/BZip2 patches | Better for new projects |

### Recommendations by Scenario

| Scenario | Recommendation |
| --- | --- |
| Patch size priority | `BsdiffDiffer` + BZip2 |
| Client apply speed priority | `StreamingHdiffDiffer` (default Deflate) |
| Legacy patch format compatibility | `BsdiffDiffer` (32-byte legacy header auto-treated as BZip2) |
| Large files (>500MB) | Benchmark first; may need `StreamingHdiffDiffer` + larger `maxWindowSize` |
| Directory-level batch diff | Via Core `DiffPipeline` with `WithParallelism` |
| New project | Run baseline with defaults, adjust based on size/speed needs |

---

## Related Resources

- [GeneralUpdate Repository](https://github.com/GeneralLibrary/GeneralUpdate)
- [Differential Sample](https://github.com/GeneralLibrary/GeneralUpdate-Samples/tree/main/src/Hub/Samples/DifferentialSample.cs)
- [Core DiffPipeline Docs](GeneralUpdate.Core.md)
- [Packaging Guide](../guide/Packaging.md)
