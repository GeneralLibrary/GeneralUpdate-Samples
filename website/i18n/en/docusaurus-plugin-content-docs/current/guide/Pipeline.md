---
sidebar_position: 3
---

# Pipeline Middleware

## Overview

GeneralUpdate uses a **Middleware Pipeline Pattern** to process update packages. Each downloaded package passes through middleware in sequence — from hash verification to decompression, patch application, and driver installation.

The core advantages are **separation of concerns** and **extensibility**: each middleware handles one task, and developers can easily add custom middleware.

---

## Pipeline Architecture

```
Download Complete (.zip)
      │
      ▼
┌─────────────────┐
│  HashMiddleware  │  ← SHA256 hash verification
└────────┬────────┘
         │ Pass
         ▼
┌─────────────────┐
│CompressMiddleware│  ← Decompress ZIP to target path
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ PatchMiddleware  │  ← Apply BSDiff patches (optional)
└────────┬────────┘
         │
         ▼
┌─────────────────────┐
│DrivelutionMiddleware│  ← Install drivers (optional)
└────────┬────────────┘
         │
         ▼
     Update Complete
```

---

## Core Interfaces

### IMiddleware

All middleware must implement:

```csharp
public interface IMiddleware
{
    Task InvokeAsync(PipelineContext context);
}
```

### PipelineBuilder

Immutable builder, executes middleware FIFO:

```csharp
public class PipelineBuilder
{
    public PipelineBuilder UseMiddleware<T>() where T : IMiddleware;
    public PipelineBuilder UseMiddlewareIf<T>(bool? condition) where T : IMiddleware;
    public async Task Build(PipelineContext context);
}
```

### PipelineContext

Thread-safe key-value store passed between middleware:

```csharp
public class PipelineContext
{
    public const string Hash = "Hash";
    public const string Format = "Format";
    public const string Encoding = "Encoding";
    public const string ZipFilePath = "ZipFilePath";
    public const string SourcePath = "SourcePath";
    public const string PatchPath = "PatchPath";
    public const string PatchEnabled = "PatchEnabled";
    public const string DiffPipeline = "DiffPipeline";
}
```

---

## Built-in Middleware

### 1. HashMiddleware — SHA256 Verification

**Location:** `GeneralUpdate.Core.Pipeline.HashMiddleware`

1. Reads `"Hash"` and `"ZipFilePath"` from context
2. Computes SHA256 of the ZIP file
3. Compares with expected hash
4. Mismatch → throws `CryptographicException`
5. Match → passes to next middleware

### 2. CompressMiddleware — Decompression

**Location:** `GeneralUpdate.Core.Pipeline.CompressMiddleware`

| `PatchEnabled` | Extract Target | Next Step |
|----------------|---------------|-----------|
| `true` | `PatchPath` (temp directory) | `PatchMiddleware` applies diffs |
| `false` | `SourcePath` (install directory) | Complete, full overwrite |

### 3. PatchMiddleware — Differential Patch Application

**Location:** `GeneralUpdate.Core.Pipeline.PatchMiddleware`

1. Checks for `"DiffPipeline"` in context
2. Not present → skip (full update mode)
3. Present → call `DiffPipeline.DirtyAsync()` to:
   - Apply `.patch` files via BSDiff/HDiffPatch
   - Copy new files to target
   - Delete files listed in `delete_files.json`

### 4. DrivelutionMiddleware — Driver Update

**Location:** `GeneralUpdate.Core.Pipeline.DrivelutionMiddleware`

Triggered when `DriverDirectory` is set in context. Runs the driver update pipeline: Validate → Backup → Install → Verify.

---

## Default Pipeline Construction

```csharp
protected override PipelineBuilder BuildPipeline(PipelineContext context)
{
    return new PipelineBuilder()
        .UseMiddleware<HashMiddleware>()
        .UseMiddleware<CompressMiddleware>()
        .UseMiddlewareIf<PatchMiddleware>(
            context.Get<bool>(PipelineContext.PatchEnabled))
        .UseMiddlewareIf<DrivelutionMiddleware>(
            !string.IsNullOrEmpty(context.Get<string>("DriverDirectory")));
}
```

---

## Differential Pipeline (DiffPipeline)

### Core Interface

```csharp
public interface IBinaryDiffer
{
    /// Apply patch (client): oldFile + patchFile → newFile
    Task DirtyAsync(string oldFilePath, string newFilePath, 
                    string patchFilePath, CancellationToken ct = default);

    /// Generate patch (server/tool): oldFile + newFile → patchFile
    Task CleanAsync(string oldFilePath, string newFilePath, 
                    string patchFilePath, CancellationToken ct = default);
}
```

### Algorithms

| Algorithm | Implementation | Characteristics |
|-----------|---------------|-----------------|
| **BSDiff 4.0** | `BsdiffDiffer` | O(n log n) suffix array, high compression |
| **Streaming Hdiff** | `StreamingHdiffDiffer` | FNV-1a hash pre-filtering, streaming for large files |

### Compression Providers

| Provider | FormatVersion | Compatibility |
|----------|---------------|---------------|
| **BZip2** | `0x00` | Legacy BSDiff compatible, all platforms |
| **Deflate** | `0x01` | .NET BCL DeflateStream, faster |
| **Brotli** | `0x02` | .NET 6+ BrotliStream, highest compression |

### Differential Matchers

```csharp
// Client side — determines which files need patching
public interface IDirtyMatcher
{
    Task<bool> ShouldDirty(string relativeFilePath);
}

// Server/Tool side — determines which files need diff generation
public interface ICleanMatcher
{
    Task<bool> ShouldClean(string relativeFilePath);
}
```

---

## Progress Reporting

```csharp
await new GeneralClientBootstrap()
    .AddListenerProgress((sender, args) =>
    {
        if (args.Progress.Diff != null)
        {
            Console.WriteLine($"Diff progress: {args.Progress.Diff.Percentage}%");
        }
    })
    .SetConfig(config)
    .LaunchAsync();
```

---

## Error Handling & Rollback

```
Middleware execution fails
  → AbstractStrategy.HandleExecuteException()
    → Fire Exception event
    → TryRollback()
      → Restore files from backup directory
      → Clean up temp files
    → Report failure (ReportType.Failure)
```

---

## Custom Middleware

### Example: File Scanner

```csharp
public class FileScanMiddleware : IMiddleware
{
    public async Task InvokeAsync(PipelineContext context)
    {
        var sourcePath = context.Get<string>(PipelineContext.SourcePath);
        var files = Directory.GetFiles(sourcePath, "*.*", 
            SearchOption.AllDirectories);
        Console.WriteLine($"Updated {files.Length} files");
        // Add virus scanning, code signing verification, etc.
    }
}
```

---

## Related Resources

- **[Architecture](./Architecture.md)** — Pipeline within overall architecture
- **[GeneralUpdate.Differential](../doc/GeneralUpdate.Differential)** — BSDiff differential algorithm
- **[Configuration Reference](./Configuration.md)** — Pipeline-related config options
