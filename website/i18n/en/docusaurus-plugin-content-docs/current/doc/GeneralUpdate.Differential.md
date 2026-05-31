---
sidebar_position: 6
---

# GeneralUpdate.Differential

`GeneralUpdate.Differential` is the binary differential component in GeneralUpdate. It focuses on one low-level problem: old file + patch file = new file. It provides replaceable file-level differ algorithms, patch compression abstractions, and BSDIFF-compatible patch read/write support. Directory comparison, batch patch generation, parallel scheduling, deleted-file handling, and update orchestration are handled by `GeneralUpdate.Core` through `DiffPipeline`, or by `GeneralUpdate.Tools` on the publishing side.

**Namespaces:** `GeneralUpdate.Differential`, `GeneralUpdate.Differential.Differ`, `GeneralUpdate.Differential.Abstractions`

**Main entry points:** `IBinaryDiffer`, `BsdiffDiffer`, `StreamingHdiffDiffer`

**NuGet package:** `GeneralUpdate.Differential`

```bash
dotnet add package GeneralUpdate.Differential
```

## Documentation outline and topic navigation {#knowledge-map}

If this is your first time reading the Differential documentation, start with this map and jump to the topic you need. The page is organized as "boundaries -> file-level API -> algorithm selection -> compression format -> Core/Tools integration -> performance and extension points".

| What you want to learn | Recommended section |
| --- | --- |
| What Differential is responsible for | [Component boundaries](#component-boundaries) |
| What `Clean` and `Dirty` mean | [Clean and Dirty semantics](#clean-and-dirty-semantics) |
| How to generate and apply a patch for one file | [Single-file quick start](#single-file-quick-start) |
| Which algorithms exist and how to choose one | [Differ algorithm selection](#differ-algorithm-selection) |
| How the BSDIFF header and compression byte work | [Patch format and compression providers](#patch-format-and-compression-providers) |
| How directory-level differential updates are enabled in Core | [Relationship with GeneralUpdate.Core](#relationship-with-generalupdatecore) |
| What Tools uses when building differential packages | [Relationship with GeneralUpdate.Tools](#relationship-with-generalupdatetools) |
| Whether downloads and diff work can run in parallel | [Concurrency model and performance guidance](#concurrency-model-and-performance-guidance) |
| How to plug in a custom differ or compression provider | [Extension points](#extension-points) |

## Component boundaries

Differential is a low-level file patching library, not a complete update orchestrator. This boundary is important because older documentation mentioned concepts such as `DifferentialCore`, blacklists, and directory batch processing, but those are not the current public API of this component.

| Capability | Owned by Differential | Notes |
| --- | --- | --- |
| Generate a binary patch for one file | Yes | Done through `IBinaryDiffer.CleanAsync(oldFile, newFile, patchFile)`. |
| Apply a binary patch for one file | Yes | Done through `IBinaryDiffer.DirtyAsync(oldFile, outputNewFile, patchFile)`. |
| Differ algorithm implementations | Yes | Current main implementations are `BsdiffDiffer` and `StreamingHdiffDiffer`. |
| Compress and decompress patch data | Yes | Abstracted by `ICompressionProvider`; BZip2 and Deflate are available, with .NET 6+ Brotli reserved in source. |
| Compare old and new directories | No | Handled by matchers in `GeneralUpdate.Core.Pipeline.DiffPipeline`. |
| Copy new files, write delete manifests, name batch patches | No | Handled by `DiffPipeline`, which writes `.patch` files, copies new files, and writes `generalupdate.delete.json`. |
| Build update packages | No | Prefer `GeneralUpdate.Tools`, which calls the Core differential pipeline. |
| Download, verify, unzip, write back versions, restart apps | No | These belong to the `GeneralUpdate.Core` update flow. |

> The current source does not contain the old `DifferentialCore` singleton mentioned by previous docs. When using Differential directly, program against `IBinaryDiffer` and concrete differ implementations. For directory-level behavior, use Core's `DiffPipeline`.

## Clean and Dirty semantics {#clean-and-dirty-semantics}

Differential follows the two terms used by the GeneralUpdate differential flow:

| Term | Method | Input | Output | Common location |
| --- | --- | --- | --- | --- |
| `Clean` | `CleanAsync` | Old file, new file, patch path | `.patch` file | Build/publishing side |
| `Dirty` | `DirtyAsync` | Old file, output new-file path, patch path | Restored new file | Client upgrade side |

File-level patch application does not overwrite the old file directly. It writes the restored result to the `newFilePath` you pass in. At the directory level, Core's `DiffPipeline` writes to a temporary file first and replaces the original only after the patch is applied successfully, avoiding corruption if patch application fails.

## Single-file quick start

This example only demonstrates the low-level single-file capability. If you need to compare two directories, generate many `.patch` files, copy added files, or handle deleted files, go to [Relationship with GeneralUpdate.Core](#relationship-with-generalupdatecore).

```csharp
using GeneralUpdate.Differential.Abstractions;
using GeneralUpdate.Differential.Differ;

IBinaryDiffer differ = new BsdiffDiffer();

var oldFile = @"D:\releases\1.0.0\app.dll";
var newFile = @"D:\releases\1.0.1\app.dll";
var patchFile = @"D:\patches\app.dll.patch";
var outputFile = @"D:\restore\app.dll";

// Generate patch: oldFile + newFile -> patchFile
await differ.CleanAsync(oldFile, newFile, patchFile);

// Apply patch: oldFile + patchFile -> outputFile
await differ.DirtyAsync(oldFile, outputFile, patchFile);
```

Both `CleanAsync` and `DirtyAsync` accept a `CancellationToken`. The current implementations observe cancellation when work starts and at Core pipeline scheduling points; the inner algorithm loops do not check cancellation for every byte, so canceling a large file may complete only after the current file finishes processing.

## Core API

### IBinaryDiffer

`IBinaryDiffer` is the shared abstraction for all file-level differ algorithms, and it is the key interface used by Core's differential pipeline.

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

| Parameter | Meaning |
| --- | --- |
| `oldFilePath` | Path to the old-version file. Required when generating and applying patches. |
| `newFilePath` | In `CleanAsync`, the source new-version file; in `DirtyAsync`, the restored output file. |
| `patchFilePath` | Path to the patch file. `CleanAsync` writes it; `DirtyAsync` reads it. |

### BsdiffDiffer

`BsdiffDiffer` implements the BSDIFF 4.0 file-level binary diff algorithm. It reads the old and new files into memory, uses suffix sorting to find matching blocks, then writes control, diff, and extra sections.

```csharp
using GeneralUpdate.Differential.Differ;

var differ = new BsdiffDiffer();
await differ.CleanAsync(oldFile, newFile, patchFile);
await differ.DirtyAsync(oldFile, outputFile, patchFile);
```

| Feature | Notes |
| --- | --- |
| Default compression | `BZip2CompressionProvider`, for compatibility with legacy BSDIFF patches. |
| Replaceable compression | The constructor accepts an `ICompressionProvider`. |
| Patch compatibility | Supports legacy 32-byte BSDIFF headers and the current 33-byte extended header. |
| Best fit | Compatibility-focused scenarios where file size is manageable and stable patch size matters. |
| Resource profile | Patch generation reads both old and new files, so large single files require memory planning. |

`BsdiffDiffer` also keeps `Clean(...)` and `Dirty(...)` methods. New code should prefer `IBinaryDiffer.CleanAsync` and `DirtyAsync` so the algorithm can be swapped later.

### StreamingHdiffDiffer

`StreamingHdiffDiffer` is the other differ implementation in the current source. It builds a block-level FNV-1a hash index to pre-filter candidate positions, extends matches at byte level, and writes a BSDIFF-compatible patch structure.

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

| Feature | Notes |
| --- | --- |
| Default compression | `DeflateCompressionProvider`. |
| Block size | `BlockSize` defaults to 64 KB and is used to build the old-file block hash index. |
| Window budget | `MaxWindowSize` defaults to 128 MB and affects the memory window used during patch generation. |
| Patch application | `DirtyAsync` delegates to `BsdiffDiffer` patch application logic. |
| Best fit | Faster candidate matching and directory-level builds that should align with `DiffPipeline` defaults. |

The current implementation is not a full external-memory streaming differ. If a single file exceeds `MaxWindowSize`, only the budgeted window participates in the calculation. For very large files, validate restoration results in your publishing pipeline, increase `MaxWindowSize`, or choose `BsdiffDiffer` or another implementation that better fits the file size.

## Differ algorithm selection

| Algorithm | Default compression | Generation strategy | Strengths | Notes |
| --- | --- | --- | --- | --- |
| `BsdiffDiffer` | BZip2 | Suffix sort + BSDIFF control/diff/extra sections | Good historical compatibility and stable patch size | Reads old and new files during generation, so large files increase memory pressure |
| `StreamingHdiffDiffer` | Deflate | Block hash index + BSDIFF-compatible output | Faster candidate lookup, suitable for Core pipeline defaults | Single files larger than `MaxWindowSize` need validation or tuning |

Recommended choices:

| Scenario | Recommendation |
| --- | --- |
| Low-level single-file patching with maximum compatibility | Use `new BsdiffDiffer()`. |
| Directory-level batch patches through Core `DiffPipeline` | Use the default `StreamingHdiffDiffer`, or specify it explicitly in `UseDiffPipeline`. |
| Client-side decompression performance is more important | Prefer Deflate patches: `StreamingHdiffDiffer` defaults, or `new BsdiffDiffer(new DeflateCompressionProvider())`. |
| Existing patches are legacy BSDIFF/BZip2 | Apply them with `BsdiffDiffer`; 32-byte headers are treated as BZip2. |

## Patch format and compression providers {#patch-format-and-compression-providers}

Differential writes BSDIFF-style patches. The current implementation writes a 33-byte extended header:

| Offset | Length | Meaning |
| --- | --- | --- |
| `0` | 8 | Magic string `"BSDIFF40"`. |
| `8` | 8 | Compressed control-section length. |
| `16` | 8 | Compressed diff-section length. |
| `24` | 8 | New-file length. |
| `32` | 1 | Compression format version. |

Patch application is also compatible with legacy 32-byte headers. If the 33rd format byte is absent, the patch is treated as a BZip2 legacy patch.

### ICompressionProvider

`ICompressionProvider` wraps the control, diff, and extra sections in compression streams.

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

| Provider | Format byte | Current availability | Notes |
| --- | --- | --- | --- |
| `BZip2CompressionProvider` | `0x00` | Available | Default for `BsdiffDiffer`; compatible with legacy BSDIFF patches. |
| `DeflateCompressionProvider` | `0x01` | Available | BCL `DeflateStream`; friendlier decompression speed for client updates. |
| `BrotliCompressionProvider` | `0x02` | Reserved in source behind `NET6_0_OR_GREATER` | The current `GeneralUpdate.Differential` project targets `netstandard2.0`, and the patch reader currently recognizes only `0x00` and `0x01`, so do not use Brotli for production update packages. |

When customizing compression, generated patches must use a format byte that the patch reader can recognize. For production use today, prefer BZip2 or Deflate.

```csharp
using GeneralUpdate.Differential.Abstractions;
using GeneralUpdate.Differential.Differ;

var differ = new BsdiffDiffer(
    new DeflateCompressionProvider(optimalLevel: false));

await differ.CleanAsync(oldFile, newFile, patchFile);
await differ.DirtyAsync(oldFile, outputFile, patchFile);
```

## Relationship with GeneralUpdate.Core {#relationship-with-generalupdatecore}

`GeneralUpdate.Core` builds directory-level differential updates on top of Differential through `DiffPipeline`. It is responsible for:

1. Comparing the old and new directories.
2. Finding changed files and calling `IBinaryDiffer.CleanAsync` to generate `.patch` files.
3. Copying added files into the patch directory.
4. Writing `generalupdate.delete.json` for deleted files.
5. Applying patches on the client by calling `IBinaryDiffer.DirtyAsync` in parallel, writing temporary files first, then replacing originals after success.

If you are using Core in an application update flow, you usually do not need to call `BsdiffDiffer` or `StreamingHdiffDiffer` directly. Configure the algorithm and parallelism through `UseDiffPipeline`:

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

There are two default layers in the current source:

| Usage | Default differ |
| --- | --- |
| Direct `new DiffPipeline()` or `new DiffPipelineBuilder().Build()` | `StreamingHdiffDiffer` |
| `GeneralUpdateBootstrap` without an explicit `UseDiffPipeline(...)` | `BsdiffDiffer`, parallelism 2, with `DiffProgressReporter` |

If you want a specific algorithm in the Core update flow, call `UseDiffPipeline(...)` explicitly rather than relying on defaults.

## Relationship with GeneralUpdate.Tools {#relationship-with-generalupdatetools}

`GeneralUpdate.Tools` targets the publishing side and helps developers build update artifacts. The current `DiffService` creates `new DiffPipeline()` and calls:

```csharp
await pipeline.CleanAsync(oldDir, newDir, patchDir);
```

In other words, Tools uses Core's `DiffPipeline` to build directory-level differential packages, and `DiffPipeline` calls Differential's `IBinaryDiffer` for each changed file. For most developers, the recommended path is:

1. Use Tools to compare the old and new version directories and generate the patch directory plus manifest artifacts.
2. Use Core on the client to check versions, download packages, and apply patches.
3. Use Differential directly only when experimenting with custom algorithms, compression formats, or single-file patching.

This layering keeps business code simple: Tools builds artifacts, Core runs updates, and Differential handles low-level file diffs.

## Concurrency model and performance guidance

A Differential differ instance does not store mutable shared state for a specific patch operation. As long as the supplied `ICompressionProvider` is thread-safe, the built-in differ implementations can be called concurrently by Core's pipeline. The built-in BZip2 and Deflate providers create a new compression stream for each call and are suitable for concurrent use.

Real "multi-threaded diff" normally happens at the Core `DiffPipeline` layer:

```csharp
var pipeline = new DiffPipelineBuilder()
    .UseDiffer(new StreamingHdiffDiffer())
    .WithParallelism(4)
    .Build();

await pipeline.CleanAsync(oldDir, newDir, patchDir);
```

| Parameter/strategy | Guidance |
| --- | --- |
| `WithParallelism(1)` | Resource-sensitive devices, HDDs, or low-memory environments. |
| `WithParallelism(2)` | Balanced default for most desktop applications. |
| `WithParallelism(4-8)` | Multi-core CPUs, SSDs, build machines, or publishing servers. |
| BZip2 | Better compatibility, but higher client-side decompression cost. |
| Deflate | Friendlier decompression speed for applying many client patches. |
| Large files | Measure generation time, memory peak, and restoration correctness; do not look only at patch size. |

Downloads and differential work can run in parallel at the upper update-flow level: Core can download multiple resources concurrently, and the patch application phase can process files in parallel. Differential itself only computes the patch for one file and does not manage network download threads.

## Extension points

### Custom differ algorithm

Implement `IBinaryDiffer` to plug another algorithm into Core. This is useful when integrating a native library, optimizing specific file types, or replacing the built-in algorithms.

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

Your custom algorithm must ensure that patches produced by `CleanAsync` can be applied correctly by the same algorithm's `DirtyAsync`. If the patch will be consumed by Core clients, the publishing side and client side must use the same differ implementation.

### Custom compression provider

If you want to keep the BSDIFF-compatible patch structure but change how the control, diff, and extra sections are compressed, implement `ICompressionProvider`.

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

Do not allocate a new `FormatVersion` casually. Current `BsdiffDiffer.DirtyAsync` recognizes only BZip2 (`0x00`) and Deflate (`0x01`). If you introduce a new format, you must also extend the patch reader, otherwise clients cannot apply the patch.

## Practical guidance

| Scenario | Recommended approach |
| --- | --- |
| Regular application differential updates | Use `GeneralUpdate.Tools` to build artifacts and Core on the client. |
| Need control over directory-level parallelism, error handling, and progress | Use Core's `DiffPipelineBuilder`. |
| Need to validate one file's patch behavior | Use `IBinaryDiffer` directly. |
| Patch size and application speed both matter | Benchmark BZip2, Deflate, and different algorithms on the same file set before choosing a default. |
| Packages must remain compatible with older clients | Conservatively use `BsdiffDiffer` + BZip2, or ensure clients already support the Deflate extended header. |

Differential's value is that it compresses complex binary diff behavior into a stable file-level abstraction. Application developers can focus on when to update, what to download, and how to inform users, while Core, Tools, and Differential handle patch generation and application together.
