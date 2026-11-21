---
sidebar_position: 11
---

# GeneralZip

**Namespace:** `GeneralZip`, `GeneralZip.Abstractions` | **Main Entry Points:** `GeneralZip.CompressAsync / ExtractAsync`, `GeneralZipReader`, `GeneralZipWriter` | **NuGet Package:** `GeneralZip`

## 1. Component Overview

### 1.1 Introduction

**GeneralZip** is a cross-format, encryption-capable (including Chinese national crypto), event-driven compression library for .NET. It covers a dozen-plus common archive/compression formats — ZIP, 7z, RAR, TAR, GZ, BZ2, XZ, Zstd, LZ4, Brotli, CPIO, ISO9660, and CAB — with one unified API, detecting formats automatically by magic bytes when opening an archive. The whole compress/extract pipeline is event-driven (progress, interactive password prompts, overwrite decisions, error policies), every API accepts a `CancellationToken`, and parallel compress/extract, stream-to-stream operations, and `IAsyncEnumerable` entry enumeration are supported. For encryption, it ships with WinZip AES (universal interoperability), SM4-GM (Chinese national crypto, implemented as a documented custom ZIP extension), and the SM2-GM national-crypto envelope (public-key sealing, no shared password), switchable via a single `EncryptionAlgorithm` switch. The core package has zero third-party NuGet dependencies: the SM3/SM4/SM2 cryptographic primitives and the TAR/CPIO containers are pure C# implementations locked by GB/T standard test vectors, while 7z/bzip2/xz/zstd/lz4/brotli come from an optional Rust native core that degrades gracefully when missing.

:::info v2.0.0 capability highlights
One-line `Append` / `Remove` / `Convert`, stream-to-stream overloads, parallel compress/extract, glob entry filters, ZIP extended compression methods (BZip2/Zstd/LZMA), SM2-GM envelope encryption, a pluggable KDF registry, split-volume ZIP, Unix attributes/symlinks, the CPIO/ISO9660/CAB/Brotli formats, and the `gz` command-line tool — see the [release log](../releaselog/GeneralZipReleaselog.md) for the full list.
:::

**Core Capabilities:**

| Capability | Description |
| --- | --- |
| One API, Many Formats | All formats map to the unified entry model `IArchiveEntry`; facade, reader, and writer behave identically across formats — learn once, use everywhere |
| Magic-Byte Detection | Formats are identified by file header signatures first, with extension fallback; content that disagrees with the extension still opens correctly |
| Encryption & National Crypto | WinZip AES-128/256 (AE-2, interoperable with WinZip/7-Zip/Windows Explorer) + SM4-GM (SM4-CTR + SM3-HMAC, documented container spec); legacy ZipCrypto is readable |
| Event-Driven | Entry lifecycle, per-entry and overall progress, interactive password prompts (`PasswordRequired`), overwrite queries (`OverwriteQuery`), error reports (`ErrorReport`) |
| Cancellation & Progress | Every API accepts a `CancellationToken`; dual progress channels (events and `IProgress<ArchiveProgress>`) with percentage, throughput, and ETA |
| Safe Extraction | Rejects absolute paths and path-traversal entries; wrong passwords fail fast via verifier bytes (constant-time comparison), never silently producing corrupt data |
| Zero Third-Party Dependencies | No third-party NuGet packages beyond Microsoft-maintained ones; SM3/SM4/TAR/CRC32 are pure C# implementations locked by GB/T 32905-2016 / GB/T 32907-2016 vectors |
| Graceful Degradation | When the Rust native core is missing, ZIP/TAR/TAR.GZ/GZ (fully managed) and all ZIP-family encryption keep working; only 7z/RAR and native codecs become unavailable |
| Open Extension | Dual extension points — format registry (`IArchiveFormat`) and encryption registry (`IEncryptionProvider`) — custom formats/algorithms register in one line |

**Business Problems Solved:**
- Applications receive archives from many sources (zip / tar.gz / 7z / rar / gz …); pulling in a dedicated library per format fragments APIs and bloats dependencies
- Regulated industries (government, finance) require archiving with the Chinese national SM4 algorithm, which mainstream compression libraries do not provide
- End-user applications need progress bars, password prompts, overwrite confirmation, and failure retry — yet most libraries only expose two functions: compress and extract
- Handling archives from untrusted sources requires protection against path traversal and other extraction attacks
- Third-party compression dependencies create security-audit and upgrade burden

:::info Where GeneralZip Fits in the Ecosystem
GeneralZip is a general-purpose compression component **independent of GeneralUpdate** within the GeneralLibrary family: it knows nothing about version numbers, update strategies, or server protocols — it only packs and unpacks. Patch-package generation, data backup, log archiving, and encrypted exports can all reuse its unified API and encryption capabilities directly.
:::

**Business Use Cases:**
- File backup and archiving for desktop/server applications (ZIP / TAR.GZ)
- Generating and unpacking update patch packages and data export packages
- Encrypted archiving for national-crypto compliance scenarios (SM4-GM)
- Single-file compression for logs and data (GZ / XZ / ZSTD / LZ4)
- Format detection, integrity verification, and safe extraction of externally sourced archives
- Backend support for interactive extraction UIs (password prompts, overwrite confirmation, error retry)

### 1.2 Environment & Dependencies

| Item | Description |
| --- | --- |
| **Version** | `2.0.0` |
| **Target Frameworks** | `netstandard2.0`, `net8.0` (netstandard2.0 supports .NET Framework 4.6.1+ / .NET Core 2.0+ / .NET 5+) |
| **NuGet Dependencies** | Only Microsoft-maintained packages: `Microsoft.Bcl.AsyncInterfaces`, `System.Text.Json` (both netstandard2.0 only); **zero third-party NuGet dependencies in the core package** |
| **Native Core** | Bundled Rust cdylib `generalzip_native` (provides 7z / RAR / BZ2 / XZ / ZSTD / LZ4 / Brotli); the NuGet package ships prebuilt binaries for win-x64, linux-x64, linux-musl-x64, linux-arm64, win-arm64, win-x86, osx-x64, osx-arm64 |
| **Degradation Policy** | Without the native core, 7z/RAR and native codecs are unavailable; everything else (ZIP/TAR/TAR.GZ/CPIO/GZ + all ZIP-family encryption) is unaffected |
| **License** | MIT |

---

## 2. Feature List

| Feature | Description | Type | Required | Notes & Constraints |
| --- | --- | --- | --- | --- |
| One-Line Compression | Directory or single file → archive; extension picks the format | Basic | Recommended | `CompressAsync`; every API cancellable |
| One-Line Extraction | Archive → directory, returns a result summary | Basic | Recommended | `ExtractAsync`, returns `ArchiveResult` |
| Format Detection | Magic bytes first, extension fallback | Basic | Optional | `DetectFormat`, returns `null` when unrecognized |
| Streaming Write | Add files/directories/streams entry by entry, `Finish` to finalize | Basic | Optional | `GeneralZipWriter`; `Dispose` finalizes automatically |
| Streaming Read | Entry enumeration, lazy decompression streams, single-entry extraction | Basic | Optional | `GeneralZipReader.OpenEntryStream` |
| Integrity Testing | CRC/HMAC/structure verification without touching disk | Basic | Optional | `Test` / `TestAll` |
| Encrypted Write | WinZip AES-128/256, SM4-GM (ZIP container) | Advanced | Optional | `EncryptionAlgorithm` + `Password` |
| Encrypted Read | ZipCrypto / WinZip AES / SM4-GM / 7z AES-256 / RAR | Advanced | Optional | Password preset or event-driven |
| Progress Reporting | Dual channel (events + `IProgress<T>`) with throughput and ETA | Advanced | Optional | Per-entry, roughly every 64 KB |
| Interactive Policies | Password prompts, overwrite decisions (5), error policies (3) | Advanced | Optional | `OverwritePolicy` / `ErrorPolicy` |
| Safe Extraction | Rejects absolute-path and path-traversal entries | Basic | Automatic | Recorded in `ArchiveResult.Failures` |
| Custom Formats | Implement `IArchiveFormat` and register | Advanced | Optional | `GeneralZip.Registry.Register` |
| Custom Encryption | Implement `IEncryptionProvider` and register | Advanced | Optional | `GeneralZip.Encryption.Register` |
| Native Formats | 7z (LZMA2, read/write + AES-256), RAR4/RAR5 (read-only) | Advanced | Optional | Requires the Rust native core |
| Native Codecs | BZ2 / XZ / ZSTD / LZ4 / Brotli, TAR.BZ2 | Advanced | Optional | Requires the Rust native core; callback-streaming transcode, zero temp files |
| One-Line Append/Remove/Convert | Append entries to an existing archive, delete entries by name/predicate, cross-format conversion | Advanced | Optional | `Append` / `Remove` / `Convert`, read→rewrite semantics |
| Stream-to-Stream Operations | Stream→Stream compress/extract/test without touching disk | Advanced | Optional | `Compress` / `Extract` / `Test` stream overloads |
| Parallel Compress/Extract | Entry-level parallelism: staged fragments on write, per-worker reopen on read | Advanced | Optional | `MaxDegreeOfParallelism`, default 1 (serial) |
| Glob Entry Filters | `**` cross-layer matching and character classes, include/exclude | Advanced | Optional | `IncludePatterns` / `ExcludePatterns` |
| Split-Volume ZIP | Split into `.001/.002/…` by byte cap; opening `.001` auto-concatenates | Advanced | Optional | `ArchiveWriteOptions.VolumeSize` |
| Unix Attributes/Symlinks | Unix mode read/write, symlink writing (skipped by default) | Advanced | Optional | `IUnixEntry` / `SymlinkPolicy` / `PreserveUnixAttributes` |
| Command-Line Tool | `gz` dotnet global tool: a/x/t/l/c/rm/i/detect/keygen | Advanced | Optional | `GeneralZip.Cli` |

### Format & Encryption Matrix

| Format | Read | Write | Encrypted Read | Encrypted Write | Implementation |
| --- | :---: | :---: | :---: | :---: | --- |
| ZIP (Store/Deflate/**BZip2/Zstd/LZMA**/Zip64) | ✓ | ✓ | ZipCrypto / WinZip AES / **SM4-GM** / **SM2-GM** | ✓ | In-house engine (pure C#) |
| 7z (LZMA2) | ✓ | ✓ | AES-256 | AES-256 | Rust native core |
| RAR4/RAR5 | ✓ | — (proprietary) | ✓ password | — | Rust native core |
| TAR / TAR.GZ | ✓ | ✓ | — | — | In-house ustar + .NET GZipStream |
| TAR.BZ2 | ✓ | ✓ | — | — | In-house ustar + native core |
| GZ (.gz/.gzip) | ✓ | ✓ | — | — | .NET built-in GZipStream |
| BZ2 / XZ / ZSTD / LZ4 / Brotli | ✓ | ✓ | — | — | Rust native core (callback streaming, zero temp files) |
| CPIO (newc/odc/binary) | ✓ | ✓ (newc) | — | — | In-house container (pure C#) |
| ISO9660 (+Joliet/Rock Ridge) | ✓ | — (read-only) | — | — | In-house engine (pure C#) |
| CAB | ✓ (NONE-stored) | — (read-only) | — | — | In-house engine (pure C#) |

**Why some cells are "—":**
- **RAR cannot be written** — the RAR compression algorithm is proprietary to RARLAB, and the public unrar license explicitly forbids using it to implement a compressor. Reading (including password decryption) is fully supported via the native core.
- **TAR / GZ / BZ2 / XZ / ZSTD / LZ4 / Brotli / CPIO have no encryption column** — these format specifications have no encryption mechanism at all: there is nowhere to store KDF parameters, IVs, or authentication tags. When encryption is needed, encrypt-then-compress at the application layer, or switch to ZIP + WinZip AES / SM4-GM / SM2-GM.
- **ISO9660 / CAB are read-only** — write support is not implemented; only NONE-stored CAB folders are readable (MSZIP/LZX compressed folders are explicitly unsupported).
- **SM4-GM is ZIP-only by design** — ZIP's extra field mechanism naturally supports third-party extensions, and the container binary spec is fully documented in [docs/CRYPTO-SPEC.md](https://github.com/GeneralLibrary/GeneralZip/blob/main/docs/CRYPTO-SPEC.md). Injecting custom encryption into a standardized container like 7z would produce archives no other tool can open.
- **ZipCrypto writing is off by default** — legacy 56-bit weak encryption, legacy-compatibility only; enable explicitly with `AllowWeakZipCrypto = true`.

---

## 3. API Reference

### 3.1 Configuration Fields (Properties)

**ArchiveWriteOptions (creating archives):**

| Field | Type | Default | Required | Allowed Values | Description |
| --- | --- | --- | --- | --- | --- |
| `Level` | `CompressionLevelChoice` | `Optimal` | No | `NoCompression(0)` / `Fastest(1)` / `Optimal(2)` / `SmallestSize(3)` | Compression level; on the ZIP write side `SmallestSize` maps to Deflate Optimal and `NoCompression` uses the Store method (no compression) |
| `EncryptionAlgorithm` | `string?` | `null` (no encryption) | Required when encrypting | `"ZipCrypto"` / `"WinZipAES128"` / `"WinZipAES256"` / `"SM4-GM"` / custom AlgorithmId | Encryption algorithm identifier, resolved by name from the default encryption registry |
| `Password` | `string?` | `null` | Required when `EncryptionAlgorithm` is set | — | Encryption password; obtain it from your secret store, never hard-code |
| `KdfIterations` | `int` | `10_000` | No | Positive integer | KDF iteration count (algorithms that support custom KDFs, e.g. SM4-GM) |
| `EncryptionProvider` | `IEncryptionProvider?` | `null` | No | — | Directly injects an encryption provider; takes priority over registry lookup by `EncryptionAlgorithm` |
| `EntryNameEncoding` | `Encoding` | UTF-8 (no BOM) | No | — | Entry name encoding; the default value also sets the ZIP UTF-8 flag (0x0800) so Chinese names never garble |
| `ArchiveComment` | `string?` | `null` | No | — | Archive-level comment (supported by some formats) |
| `DefaultLastWriteTime` | `DateTimeOffset?` | `null` | No | — | Used when an entry provides no timestamp; null means current time |
| `Progress` | `IProgress<ArchiveProgress>?` | `null` | No | — | Progress sink; usable in parallel with events |
| `ZipCompressionMethod` | `ZipCompressionMethodChoice` | `Auto` (Deflate) | No | `Auto(0)` / `Deflate(1)` / `BZip2(2)` / `Zstd(3)` / `Lzma(4)` | ZIP entry compression method (ignored by other formats); extended methods interoperate with 7-Zip / Info-ZIP |
| `MaxDegreeOfParallelism` | `int` | `1` (serial) | No | ≥1 | Entry-level parallel compression degree (staged fragments on write) |
| `VolumeSize` | `long` | `0` (no split) | No | Bytes | Split-volume ZIP: split into `.001/.002/…` by cap; opening `.001` auto-concatenates |
| `KdfAlgorithmId` | `string?` | `null` (algorithm default) | No | KdfId from the registry | KDF provider switch (built-in `SM3-PBKDF2` / `PBKDF2-SHA256`) |
| `RecipientPublicKey` | `string?` | `null` | Required for SM2-GM encryption | hex public key (`04\|\|X\|\|Y` 130 chars, or `X\|\|Y` 128 chars) | SM2-GM envelope recipient public key (passwordless write path) |
| `AllowWeakZipCrypto` | `bool` | `false` | No | `true` / `false` | Whether writing ZipCrypto (weak legacy encryption) is allowed; must be enabled explicitly |

**ArchiveOpenOptions (opening archives):**

| Field | Type | Default | Required | Allowed Values | Description |
| --- | --- | --- | --- | --- | --- |
| `Password` | `string?` | `null` | Required for encrypted archives | — | Preset password (effective for the first prompt only); invalidated when `PasswordProvider` is set |
| `PasswordProvider` | `IPasswordProvider?` | `null` | No | — | Dynamic password supplier (prompts and other interactive scenarios); returning `null` means give up |
| `MaxPasswordAttempts` | `int` | `3` | No | Positive integer | Max password attempts per entry/whole archive; exceeding throws |
| `EntryNameEncoding` | `Encoding?` | `null` | No | — | Entry name encoding; when null the format decides (ZIP prefers the UTF-8 flag, otherwise falls back to IBM437) |
| `Progress` | `IProgress<ArchiveProgress>?` | `null` | No | — | Progress sink |
| `SourceName` | `string?` | `null` | No | — | Source file name; used by single-file codec formats to derive the entry name (filled automatically by the facade) |

**ExtractionOptions (extracting):**

| Field | Type | Default | Required | Allowed Values | Description |
| --- | --- | --- | --- | --- | --- |
| `OverwritePolicy` | `OverwritePolicy` | `Throw` | No | `Throw(0)` / `Overwrite(1)` / `Skip(2)` / `Ask(3)` / `Rename(4)` | Policy when the target already exists; `Ask` raises the `OverwriteQuery` event |
| `ErrorPolicy` | `ErrorPolicy` | `Throw` | No | `Throw(0)` / `SkipAll(1)` / `RaiseEvent(2)` | Per-entry error policy; `RaiseEvent` raises the `ErrorReport` event |
| `MaxRenameAttempts` | `int` | `64` | No | Positive integer | Auto-rename limit (`name (2).ext` style); exceeding throws |
| `Filter` | `Predicate<IArchiveEntry>?` | `null` | No | — | Extract only entries matching the predicate (directory entries excepted) |
| `Flatten` | `bool` | `false` | No | `true` / `false` | Ignore entry paths, extract everything into the destination root |
| `PreserveTimestamps` | `bool` | `true` | No | `true` / `false` | Write back file timestamps; disable in CI scenarios if desired |
| `PropagateEventExceptions` | `bool` | `false` | No | `true` / `false` | Whether exceptions thrown by event handlers propagate; by default they are isolated and treated as Skip |
| `MaxRetryPerEntry` | `int` | `3` | No | Positive integer | Upper limit on consecutive Retry decisions under the `RaiseEvent` policy |
| `BufferSize` | `int` | `81920` | No | Positive integer | Per-entry copy buffer size (bytes) |
| `MaxDegreeOfParallelism` | `int` | `1` (serial) | No | ≥1 | Entry-level parallel extraction degree (per-worker archive reopen) |
| `IncludePatterns` | `IReadOnlyList<string>?` | `null` (all) | No | Glob patterns | Extract only entries matching any pattern (`**` cross-layer, character classes) |
| `ExcludePatterns` | `IReadOnlyList<string>?` | `null` | No | Glob patterns | Skip entries matching any pattern (takes priority over include patterns) |
| `SymlinkPolicy` | `SymlinkPolicy` | `Skip` | No | Enum | Symbolic-link entry handling policy (safe-by-default) |
| `PreserveUnixAttributes` | `bool` | `false` | No | `true` / `false` | Preserve Unix mode bits on extraction (requires entries providing `IUnixEntry`) |

**Event Response Enums:**

| Enum Value | Numeric | Description |
| --- | --- | --- |
| `OverwriteAction.Overwrite` | `0` | Overwrite directly |
| `OverwriteAction.Skip` | `1` | Skip the entry (default response) |
| `OverwriteAction.Rename` | `2` | Auto-rename (`name (2).ext`) |
| `OverwriteAction.Abort` | `3` | Abort the entire extraction (equivalent to cancel) |
| `ErrorAction.Retry` | `0` | Retry the entry |
| `ErrorAction.Skip` | `1` | Skip the entry (default response) |
| `ErrorAction.Abort` | `2` | Abort the entire extraction |

### 3.2 Facade & Reader/Writer Methods

**Static facade GeneralZip (zero initialization; built-in formats/encryption and native capabilities are assembled implicitly on first use):**

| Property | Type | Description |
| --- | --- | --- |
| `Version` | `Version` | Component version (assembly version) |
| `Registry` | `GeneralZipFormatRegistry` | Default format registry (13 managed formats built in + native 7z/RAR registered by capability bits) |
| `Encryption` | `GeneralZipEncryptionRegistry` | Default encryption registry (5 algorithms built in) |

| Method | Parameters | Use Case | Notes |
| --- | --- | --- | --- |
| `CompressAsync(sourcePath, archivePath, configure, cancellationToken)` | Source path (single file or directory), archive path, options callback, cancellation token | One-line compression | Format is chosen by the archive extension; directories are added with the directory name as prefix (empty directories have none); throws `FileNotFoundException` when the source is missing; sync version is `Compress` |
| `ExtractAsync(archivePath, destinationDirectory, configure, extraction, cancellationToken)` | Archive path, destination directory, open-options callback, extraction options, cancellation token | One-line extraction | Returns `ArchiveResult`; sync version is `Extract` |
| `Test(archivePath, configure, cancellationToken)` | Archive path, open-options callback, cancellation token | Integrity testing | Equivalent to `TestAll` + `ErrorPolicy.SkipAll`, nothing written to disk; supply the password via `configure` for encrypted archives |
| `DetectFormat(path)` / `DetectFormat(stream)` | File path or stream | Format detection | Magic bytes first, extension fallback; the stream overload restores `Position` after reading; returns `null` when unrecognized |
| `Append(archivePath, sourcePath, configure, cancellationToken)` / `AppendAsync` | Existing archive path, file/directory to append, write-options callback, cancellation token | Append entries to an existing archive | Read→rewrite semantics; encrypted archives automatically re-encrypt with the inherited algorithm (salt/IV not preserved); read-only formats (RAR) unsupported |
| `Remove(archivePath, entryNames, predicate, configure, cancellationToken)` / `RemoveAsync` | Archive path, entry-name set, entry predicate, open-options callback, cancellation token | Delete archive entries | Exact names (OrdinalIgnoreCase) and predicate may be combined (union); encrypted archives inherit the algorithm like Append |
| `Convert(sourceArchive, targetArchive, writeConfigure, openConfigure, cancellationToken)` / `ConvertAsync` | Source archive, target path (extension picks the new format), write/open options callbacks, cancellation token | Cross-format conversion | Encryption does not carry across formats; re-encrypt via `writeConfigure` when needed |

Stream-to-stream overloads are also provided: `Compress(Stream source, Stream output, formatName, entryName, …)`, `Extract(Stream input, destinationDirectory, …)`, and `Test(Stream input, …)` — memory/network stream operations without touching disk.

:::note The facade class shares its name with the namespace
The static class inside `namespace GeneralZip` is also named `GeneralZip`. To mix with other qualified usages, add `using static GeneralZip.GeneralZip;` and call `CompressAsync()` etc. unqualified.
:::

**GeneralZipReader (archive reading facade):**

| Method | Parameters | Use Case | Notes |
| --- | --- | --- | --- |
| `Open(path, configure, initialize, registry)` (static) | Archive path, open-options callback, event-subscription hook, registry | Open an archive from a file | Magic-byte detection → extension fallback; `initialize` runs before format parsing — **whole-archive encrypted formats (e.g. 7z header encryption) must subscribe to `PasswordRequired` here**; the stream is cleaned up automatically if opening fails |
| `Open(stream, formatName, configure, initialize, registry)` (static) | Stream, explicit format name, options callback, event-subscription hook, registry | Open an archive from a stream | Signature probing is skipped when `formatName` is given; non-seekable streams are wrapped for probing automatically |
| `OpenAsync(path, ...)` (static) | Same signature as the file version | Asynchronous opening | Synchronous IO internally; the signature is aligned so callers can switch freely |
| `OpenEntryStream(entry)` | Entry | Read entry content | Sequential stream with lazy decryption + decompression; the password is negotiated automatically (preset or via events) |
| `ExtractToDirectory(destinationDirectory, options, cancellationToken)` | Destination directory, extraction options, cancellation token | Bulk extraction | Preserves entry structure; returns `ArchiveResult`; `ExtractToDirectoryAsync` shares the same implementation |
| `ExtractEntry(entry, destinationFilePath, options, cancellationToken)` | Entry, destination file path, extraction options, cancellation token | Extract a single entry | — |
| `TestAll(options, cancellationToken)` | Extraction options, cancellation token | Integrity testing | Verifies CRC/HMAC/structure without writing to disk |
| `Dispose()` | None | Release the reader | Closes the underlying stream |

| Property | Type | Description |
| --- | --- | --- |
| `Format` | `ArchiveFormatInfo` | Detected format information |
| `Entries` | `IReadOnlyList<IArchiveEntry>` | All entries in the archive |
| `IsEncrypted` | `bool` | Whether the archive contains encrypted content |
| `Password` | `string?` | Password supplied after opening (lower priority than event responses; effective for the next prompt) |

**GeneralZipWriter (archive writing facade):**

| Method | Parameters | Use Case | Notes |
| --- | --- | --- | --- |
| `CreateNew(archivePath, configure, formatName, registry)` (static) | Archive path, write-options callback, explicit format name, registry | Create an archive file | Extension picks the format (compound suffixes like `.tar.gz` are recognized); an explicit `formatName` overrides extension inference |
| `CreateNew(output, formatName, configure, registry)` (static) | Output stream, format name, write-options callback, registry | Write to a stream | `formatName` is required — a stream cannot be inferred from an extension |
| `AddFile(filePath, entryName)` / `AddFileAsync(...)` | Source file path, entry name | Add a file | `entryName` defaults to the file name; the async version wraps the synchronous pipeline on the thread pool |
| `AddDirectory(directoryPath, entryPrefix, filter)` / `AddDirectoryAsync(...)` | Directory path, entry prefix, path predicate | Add a directory recursively | Directory placeholder entries are generated when the format supports them; `filter` filters file paths |
| `AddStream(entryName, content, size, lastWriteTime)` | Entry name, content stream, size, timestamp | Add stream content | Progress is more accurate when `size` is known |
| `Finish(cancellationToken)` / `FinishAsync(cancellationToken)` | Cancellation token | Finalize (write central directory/index) | Repeated calls are no-ops; `Dispose` calls it automatically if `Finish` was not invoked |
| `Dispose()` / `DisposeAsync()` | None | Release the writer | Finalizes automatically when `Finish` was not called |

| Property | Type | Description |
| --- | --- | --- |
| `Format` | `ArchiveFormatInfo` | Current archive format information |

:::warning Writers are not thread-safe
`GeneralZipWriter` does not support adding entries concurrently; call `AddFile` / `AddDirectory` / `AddStream` sequentially from a single thread.
:::

**Registries (extension entry points):**

| Type | Method | Description |
| --- | --- | --- |
| `GeneralZipFormatRegistry` | `Register(format)` | Registers a format provider; same-name registration overwrites; thread safe |
| | `Unregister(formatName)` | Removes a format by name |
| | `Snapshot()` / `FindByName(name)` | Enumerate all formats / find by name |
| | `Detect(stream)` | Signature detection; the first provider that matches wins; a throwing provider never interrupts probing |
| | `DetectByExtension(path)` | Extension matching; `x.tar.gz` matches `.tar.gz`, not `.gz` |
| `GeneralZipEncryptionRegistry` | `Register(provider)` | Registers an encryption provider; same `AlgorithmId` overwrites |
| | `Find(algorithmId)` | Find by algorithm identifier |
| | `FindByZipExtraFieldId(id)` / `FindAllByZipExtraFieldId(id)` | Find by ZIP extra field id; candidates are ordered by registration (newest first) |
| | `Snapshot()` | Enumerate all algorithms |

**Result & Entry Models:**

| Type | Members | Description |
| --- | --- | --- |
| `ArchiveResult` | `ProcessedCount` / `SkippedCount` / `FailedCount` / `Failures` / `Aborted` / `Success` | Batch operation summary; `Success` means `FailedCount == 0` |
| `FailedEntry` | `EntryName` / `Exception` | A single failure record |
| `IArchiveEntry` | `FullName` / `Name` / `Size` / `CompressedSize` / `LastWriteTime` / `IsDirectory` / `IsEncrypted` / `EncryptionAlgorithm` / `Crc32` / `Comment` | Unified entry view across all formats; paths use `/` separators; unknown sizes are `-1`; CRC is `null` when hidden by encryption |
| `ArchiveFormatInfo` | `Name` / `Extensions` / `Capabilities` / `Description` / convenience properties like `CanRead` | Format metadata, e.g. `"ZIP"`, `.zip` |

### 3.3 Events & Exceptions

**Event list (GeneralZipReader supports all 8; GeneralZipWriter supports the first 5):**

| Event | Args | Raised When | Usage Notes |
| --- | --- | --- | --- |
| `EntryStarted` / `EntryCompleted` | `EntryEventArgs` | Entry start/finish | `e.EntryName` is the full entry path |
| `EntryProgress` | `EntryProgressEventArgs` | Per-entry byte progress (roughly every 64 KB) | `e.Percent` is `null` when the size is unknown |
| `EntryFailed` | `EntryFailedEventArgs` | Entry failed | `e.Exception` carries the concrete exception |
| `OverallProgress` | `ArchiveProgress` | Overall progress snapshot | Includes phase, percentage, throughput, ETA; delivered in parallel with `IProgress<T>` |
| `PasswordRequired` | `PasswordRequiredEventArgs` | Encrypted content needs a password | Set `e.Password` to answer; `e.Cancel = true` gives up (throws `PasswordRequiredException`); `e.Attempt` starts at 1 and the component re-prompts with `Attempt+1` after a wrong password |
| `OverwriteQuery` | `OverwriteQueryEventArgs` | `OverwritePolicy.Ask` and the target exists | Decide via `e.Action`, default `Skip` (conservative: no disk changes) |
| `ErrorReport` | `ArchiveErrorEventArgs` | `ErrorPolicy.RaiseEvent` and an entry failed | Decide via `e.Action` (Retry/Skip/Abort), default `Skip`; `e.RetryCount` / `e.MaxRetry` support retry logic |

**ArchiveProgress properties (immutable snapshot):**

| Property | Type | Description |
| --- | --- | --- |
| `Phase` | `ArchivePhase` | `Analyzing` (parse structure) / `Compressing` / `Decompressing` / `Testing` / `Finalizing` (write central directory, flush buffers) |
| `CurrentEntry` | `string?` | Entry currently being processed |
| `EntriesCompleted` / `EntriesTotal?` | `long` / `long?` | Completed entries / total entries (null when unknown) |
| `BytesProcessed` / `BytesTotal?` | `long` / `long?` | Uncompressed bytes processed / total uncompressed bytes (null when unknown) |
| `Percent?` | `double?` | Overall percentage 0~100 (null when not computable) |
| `BytesPerSecond?` / `Eta?` | `double?` / `TimeSpan?` | Throughput (bytes/sec) / estimated time remaining |

:::note Interactive event defaults
When `OverwriteQuery` and `ErrorReport` are unsubscribed or left unanswered, the default action is **Skip** (conservative: no disk changes, no interruption). Exceptions thrown by event handlers are isolated and treated as Skip by default; set `ExtractionOptions.PropagateEventExceptions = true` to make them fail the operation.
:::

**Exception hierarchy:**

```text
ArchiveException (base class, carries EntryName)
├─ UnsupportedFormatException       Format unrecognized or unsupported
├─ NativeCoreMissingException       Rust native core not deployed (message includes deployment guidance)
├─ PasswordRequiredException        No password available (not preset, event unanswered/abandoned)
├─ InvalidPasswordException         Wrong password (Attempt records the try count)
└─ ArchiveCorruptedException        CRC/HMAC verification failed, invalid structure, unexpected truncation
```

| Exception | Typical Trigger | Handling Advice |
| --- | --- | --- |
| `UnsupportedFormatException` | Opening an unrecognized format; `EncryptionAlgorithm` not registered | Tell the user the format is unsupported, or register a custom provider |
| `NativeCoreMissingException` | Using 7z/RAR/BZ2/XZ/ZSTD/LZ4/BROTLI without the native core | Deploy the native core per the exception message, or fall back to managed formats |
| `PasswordRequiredException` | Encrypted archive with no password supplied and the event gave up | Prompt the user for a password and retry |
| `InvalidPasswordException` | Wrong password (verifier mismatch) | Prompt again; in event mode the component re-prompts automatically |
| `ArchiveCorruptedException` | Corrupted transfer, tampering, truncation | Re-acquire the archive after verifying the source |
| `OperationCanceledException` | `CancellationToken` triggered cancellation | Normal cancellation semantics; clean up temp files as needed |

---

## 4. Advanced Usage

### 4.1 Extension Point Overview

GeneralZip's formats and encryption algorithms are registry-driven; built-in implementations and third-party implementations are peers — the built-in ZIP/TAR etc. are also registered into the default registry via `IArchiveFormat`.

| Extension Point | Type | Description | Registration |
| --- | --- | --- | --- |
| Custom archive format | `IArchiveFormat` | Provides format metadata, magic/extension detection, and reader/writer factories; implementations must be stateless and thread safe | `GeneralZip.Registry.Register` |
| Custom encryption algorithm | `IEncryptionProvider` | Provides the algorithm id, read/write support, extra field layout, and encryption stream factories; implementations must be stateless and thread safe | `GeneralZip.Encryption.Register` |
| Dynamic password supply | `IPasswordProvider` | `GetPassword(PasswordRequest)` returns a password; returning `null` means give up | `ArchiveOpenOptions.PasswordProvider` |
| Progress sink | `IProgress<ArchiveProgress>` | Progress channel in parallel with events | `Progress` property on the option classes |
| Overwrite/error policies | `OverwritePolicy` / `ErrorPolicy` | Policy enums + event response decisions | `ExtractionOptions` |

Built-in registry contents: the format registry pre-registers 13 managed formats — ZIP, TAR, TAR.GZ, TAR.BZ2, GZIP, BZ2, XZ, ZSTD, LZ4, Brotli, CPIO, ISO9660, CAB — and attaches native 7Z/RAR by capability bits; the encryption registry pre-registers 5 algorithms: `ZipCrypto`, `WinZipAES128`, `WinZipAES256`, `SM4-GM`, and `SM2-GM`.

### 4.2 Scenario Examples

#### Scenario 1: SM4-GM Encrypted Backup with Integrity Verification

[Scenario] Enterprise data backup requires encryption with the Chinese national algorithm. Choose SM4-GM when compressing; run an integrity test (HMAC verification, no disk writes) before restoring, then unpack everything or read a single entry as needed.

[Example]

```csharp
using GeneralZip;
using GeneralZip.Abstractions;

var password = GetPasswordFromSecretStore();   // obtain from your secret store, never hard-code

// 1. Compress with encryption (SM4-GM: SM4-CTR + SM3-HMAC; container spec in docs/CRYPTO-SPEC.md)
await GeneralZip.CompressAsync(@"C:\data", @"D:\backup.zip", o =>
{
    o.Level = CompressionLevelChoice.Optimal;
    o.EncryptionAlgorithm = "SM4-GM";
    o.Password = password;
    o.KdfIterations = 10_000;                  // SM3-PBKDF2 rounds, tune per your security requirements
});

// 2. Integrity test before restore (HMAC + structure, nothing written to disk)
using (var verifier = GeneralZipReader.Open(@"D:\backup.zip", o => o.Password = password))
{
    var test = verifier.TestAll();
    if (!test.Success)
        throw new InvalidOperationException($"Backup corrupted: {test.FailedCount} entries failed verification");
}

// 3. Read just one entry: lazy decryption + decompression stream, no full extraction needed
using var reader = GeneralZipReader.Open(@"D:\backup.zip", o => o.Password = password);
var entry = reader.Entries.Single(e => e.FullName == "data/config.json");
using var stream = reader.OpenEntryStream(entry);
using var output = File.Create(@"D:\restore\config.json");
stream.CopyTo(output);

Console.WriteLine($"Entry encryption: {entry.EncryptionAlgorithm}");   // "SM4-GM"
```

[Notes]
- Wrong passwords fail fast on the first entry (4-byte verifier + constant-time comparison), throwing `InvalidPasswordException` — no silent corruption
- SM4-GM currently lands in the ZIP container (extra field 0x4D53); interop with third-party tools requires them to implement the same spec
- `KdfIterations` only affects newly created archives; readers recover the iteration count from the container — no configuration needed

#### Scenario 2: Interactive Extraction (Password Prompt, Overwrite Confirmation, Error Retry)

[Scenario] A desktop app extracts an encrypted archive from an unknown source: the password is unknown and must be prompted; files already exist in the destination and need a decision; a few damaged entries may be retried or skipped; progress is shown throughout and a summary is printed at the end.

[Example]

```csharp
using GeneralZip;
using GeneralZip.Abstractions;

using var reader = GeneralZipReader.Open(archivePath, configure: null, r =>
{
    // Password prompt: whole-archive encrypted formats (e.g. 7z header encryption) trigger during Open — subscribe here
    r.PasswordRequired += (_, e) =>
    {
        e.Password = PromptPassword(e.EntryName, e.Attempt);   // your UI logic
        if (e.Attempt >= 3) e.Cancel = true;                   // give up after 3 tries
    };

    r.OverwriteQuery += (_, e) =>
    {
        // Overwrite when the archive copy is newer; otherwise auto-rename to keep both
        var targetTime = File.GetLastWriteTime(e.TargetPath);
        e.Action = e.LastWriteTime > targetTime
            ? OverwriteAction.Overwrite
            : OverwriteAction.Rename;
    };

    r.ErrorReport += (_, e) =>
    {
        Console.WriteLine($"Entry {e.EntryName} failed (attempt {e.RetryCount}/{e.MaxRetry}): {e.Exception.Message}");
        e.Action = e.RetryCount < e.MaxRetry ? ErrorAction.Retry : ErrorAction.Skip;
    };

    r.OverallProgress += (_, p) => UpdateProgressBar(p.Percent, p.Eta);
});

var result = reader.ExtractToDirectory(@"D:\out", new ExtractionOptions
{
    OverwritePolicy = OverwritePolicy.Ask,      // raises the OverwriteQuery event
    ErrorPolicy = ErrorPolicy.RaiseEvent,       // raises the ErrorReport event
    MaxRetryPerEntry = 3,
    Filter = e => e.FullName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase),
    PreserveTimestamps = true,
});

Console.WriteLine($"Processed {result.ProcessedCount}, skipped {result.SkippedCount}, failed {result.FailedCount}");
foreach (var fail in result.Failures)
    Console.WriteLine($"  {fail.EntryName}: {fail.Exception.Message}");
```

[Notes]
- Event handler exceptions are isolated and treated as Skip by default; set `PropagateEventExceptions = true` to make them fail the operation
- Unsafe entry paths (absolute paths, path traversal) are always rejected and recorded in `Failures`, regardless of policy
- `ArchiveResult.Aborted == true` means the user chose Abort in the overwrite query or the operation was cancelled

#### Scenario 3: Registering a Custom Encryption Provider

[Scenario] An organization needs to integrate its in-house encryption algorithm. GeneralZip exposes the encryption extension point via `IEncryptionProvider` + the ZIP extra field mechanism: algorithm parameters are persisted with each entry, and readers recover them from the extra field.

[Example]

The following is a skeleton (key-agreement details omitted; see the built-in `WinZipAesProvider` / `Sm4GmProvider` sources for complete implementations):

```csharp
using System.Collections.Generic;
using System.IO;
using GeneralZip;
using GeneralZip.Abstractions;
using GeneralZip.Cryptography;

public sealed class MyAesCtrProvider : IEncryptionProvider
{
    // Custom extra field id, avoiding 0x9901 (WinZip AES) and 0x4D53 (SM4-GM)
    private const ushort ExtraFieldId = 0x9F01;

    public string AlgorithmId => "MY-AES-CTR";
    public bool SupportsWrite => true;
    public bool SupportsRead => true;
    public IReadOnlyCollection<ushort> ZipExtraFieldIds => new ushort[] { ExtraFieldId };

    // Write side: generate salt/IV → declare the extra field (carries parameters) → provide the encryption stream factory
    public EntryEncryptionPlan PrepareWrite(EncryptionWriteContext context)
    {
        var salt = Kdf.RandomBytes(16);
        var key  = Kdf.Pbkdf2Sha1(context.Password, salt, 10_000, 32);
        var mac  = Kdf.CreateIncrementalHmacSha1(key);     // simplified for the example; use a full MAC scheme in production
        var ctr  = new CtrCipher(new AesBlockCipher(), key, Kdf.RandomBytes(16));

        return new EntryEncryptionPlan(
            extraFieldId: ExtraFieldId,
            extraFieldData: salt,                          // persisted with the entry; readers recover parameters from it
            cipherOverhead: 0,
            encryptionStreamFactory: plain =>
                new CtrWriteStream(plain, ctr, mac, trailerLength: 0));
    }

    // Read side: recover extra field parameters from context.Parameters → return the decryption stream
    // Password verification failure must throw InvalidPasswordException (compare verifiers with Kdf.FixedTimeEquals)
    public Stream OpenRead(Stream cipherSource, EncryptionReadContext context)
    {
        // ... recover salt → derive key → build the decryption stream (see the decryption pseudo-code in CRYPTO-SPEC.md)
        throw new System.NotImplementedException();
    }
}

// Register and use by name (same AlgorithmId overwrites the old implementation; thread safe)
GeneralZip.Encryption.Register(new MyAesCtrProvider());

await GeneralZip.CompressAsync(@"C:\data", @"D:\sealed.zip", o =>
{
    o.EncryptionAlgorithm = "MY-AES-CTR";
    o.Password = password;
});
```

[Notes]
- Provider implementations must be **stateless and thread safe**, returning independent stream instances per call
- Readers match candidates by extra field id, in registration order (newest registered wins)
- Encryption code is security-sensitive; prefer the built-in `WinZipAES256` (universal interop) or `SM4-GM` (national-crypto compliance) over in-house crypto — this example demonstrates the extension point only

---

## 5. Common Usage Examples

### 5.1 Quick Start (Minimal Demo)

```csharp
using GeneralZip;

// Directory → archive (format is chosen by the file extension)
await GeneralZip.CompressAsync(@"C:\data", @"D:\backup.zip");

// Archive → directory
var result = await GeneralZip.ExtractAsync(@"D:\backup.zip", @"D:\restore");
Console.WriteLine($"Extracted {result.ProcessedCount} entries, {result.FailedCount} failed, success: {result.Success}");

// Detect the format (magic bytes first, extension fallback)
Console.WriteLine(GeneralZip.DetectFormat(@"D:\backup.zip")?.Name);   // "ZIP"
```

### 5.2 Basic Parameter Combination

```csharp
using GeneralZip;
using GeneralZip.Abstractions;

var progress = new Progress<ArchiveProgress>(p =>
    Console.Write($"\r{p.Percent:F0}% {p.CurrentEntry}"));

// Write: compression level + archive comment + progress
await GeneralZip.CompressAsync(@"C:\photos", @"D:\photos.zip", o =>
{
    o.Level = CompressionLevelChoice.SmallestSize;
    o.ArchiveComment = "daily photos backup";
    o.Progress = progress;
});

// Read: entry name encoding + filter + rename policy + error skipping
using var reader = GeneralZipReader.Open(@"D:\photos.zip");

var result = reader.ExtractToDirectory(@"D:\restore", new ExtractionOptions
{
    OverwritePolicy = OverwritePolicy.Rename,   // conflicts produce "name (2).jpg"
    ErrorPolicy = ErrorPolicy.SkipAll,          // damaged entries are skipped, not fatal
    Filter = e => e.FullName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase),
});

foreach (var fail in result.Failures)
    Console.WriteLine($"Failed: {fail.EntryName}: {fail.Exception.Message}");
```

### 5.3 Real-World Scenario (Nightly Log Archiving)

A complete business flow: archive a whole directory as TAR.BZ2 (high compression ratio) → integrity verification → extract audit entries on demand → clean up the artifact automatically when verification fails.

```csharp
using GeneralZip;
using GeneralZip.Abstractions;

/// <summary>Nightly log archiver: packs yesterday's log directory, isolates failures,
/// and removes the artifact automatically when verification fails.</summary>
public sealed class NightlyLogArchiver
{
    private readonly string _logRoot;
    private readonly string _archiveRoot;
    private readonly CancellationToken _ct;

    public NightlyLogArchiver(string logRoot, string archiveRoot, CancellationToken ct)
    {
        _logRoot = logRoot;
        _archiveRoot = archiveRoot;
        _ct = ct;
    }

    public async Task<ArchiveResult> ArchiveYesterdayAsync(DateTimeOffset today)
    {
        var source = Path.Combine(_logRoot, today.AddDays(-1).ToString("yyyyMMdd"));
        var target = Path.Combine(_archiveRoot, $"{today:yyyyMMdd}_logs.tar.bz2");

        // 1. Directory → TAR.BZ2 (BZ2's high compression ratio comes from the native core)
        await GeneralZip.CompressAsync(source, target, o =>
        {
            o.Level = CompressionLevelChoice.Optimal;
        }, _ct);

        // 2. Integrity verification (CRC + structure, nothing on disk); delete the artifact and alert on failure
        using (var verifier = GeneralZipReader.Open(target))
        {
            var test = verifier.TestAll(cancellationToken: _ct);
            if (!test.Success)
            {
                File.Delete(target);
                throw new InvalidOperationException($"Archive verification failed: {test.FailedCount} entries corrupted");
            }
        }

        // 3. Extract only error-level logs for audit; other entries are untouched
        using var reader = GeneralZipReader.Open(target);
        return reader.ExtractToDirectory(
            Path.Combine(_archiveRoot, "audit"),
            new ExtractionOptions
            {
                OverwritePolicy = OverwritePolicy.Overwrite,
                ErrorPolicy = ErrorPolicy.SkipAll,
                Filter = e => e.FullName.Contains("/error/", StringComparison.OrdinalIgnoreCase),
            }, _ct);
    }
}
```

---

## 6. Global Configuration

GeneralZip does not rely on global configuration files. All behavior is determined by the option objects (`ArchiveWriteOptions` / `ArchiveOpenOptions` / `ExtractionOptions`), the registries, and the native core deployment state.

### Native Core Deployment

The NuGet package bundles prebuilt Rust native cores for 8 RIDs (Windows x64/ARM64/x86, Linux x64/musl/ARM64, macOS x64/ARM64); a regular install requires **no action at all**: the core is probed and loaded automatically on first use (idempotent, thread safe), with no initialization call needed.

For manual deployment, place `generalzip_native.dll` (`.so` / `.dylib`) in any of the following locations:

1. The application directory;
2. `runtimes/<RID>/native/` under the application directory;
3. The path pointed to by the `GENERALZIP_NATIVE` environment variable.

Checking load state and degradation behavior:

```csharp
using GeneralZip.Native;

Console.WriteLine(GeneralZipNative.CoreVersion);   // native core version; null when not loaded
Console.WriteLine(GeneralZipNative.CorePath);      // load path

// Or attempt loading explicitly (idempotent):
NativeCoreLoader.TryLoad();
```

| Scenario | Behavior |
| --- | --- |
| Native core available | ZIP/TAR/TAR.GZ/TAR.BZ2/GZ + 7z/RAR + BZ2/XZ/ZSTD/LZ4/BROTLI, all capabilities available |
| Native core missing | ZIP/TAR/TAR.GZ/GZ and all ZIP-family encryption keep working; 7z/RAR/BZ2/XZ/ZSTD/LZ4/BROTLI throw `NativeCoreMissingException` (message includes deployment guidance) |

### Entry Name Encoding & Chinese File Names

- **Writing**: UTF-8 (no BOM) by default, with the ZIP UTF-8 flag (0x0800) set — Chinese file names never garble across Windows/Linux/macOS
- **Reading**: the entry's own UTF-8 flag takes priority; legacy archives without the flag fall back to `ArchiveOpenOptions.EntryNameEncoding` (ZIP falls back to IBM437 when null)
- **Reading legacy archives in non-UTF-8 encodings** (e.g. produced by GBK packing tools): specify `EntryNameEncoding` explicitly, e.g. `Encoding.GetEncoding("GB18030")` (on .NET Core/.NET 5+ register `CodePagesEncodingProvider` first)
- Entry paths are normalized to `/` separators internally; absolute paths and path-traversal entries are rejected during extraction

### Encryption Algorithm Selection Guide

| AlgorithmId | Strength | Interoperability | Use Case |
| --- | --- | --- | --- |
| `WinZipAES256` | AES-256-CTR + HMAC-SHA1 (AE-2, extra field 0x9901) | WinZip / 7-Zip / Windows Explorer | First choice for universal interop |
| `WinZipAES128` | AES-128-CTR + HMAC-SHA1 (AE-2) | Same | Compatibility with older tools |
| `SM4-GM` | SM4-CTR + SM3-HMAC (extra field 0x4D53) | This component and third parties implementing the same spec | National-crypto compliance (government/finance etc.) |
| `SM2-GM` | SM2-ECIES envelope (random per-entry data key, extra field 0x4D54) | This component and third parties implementing the same spec | Public-key distribution, no shared password (`RecipientPublicKey`) |
| `ZipCrypto` | 56-bit (weak) | Nearly all tools | Read legacy archives only; writing requires `AllowWeakZipCrypto = true`, not recommended |

- KDF: WinZip AES uses a fixed PBKDF2-SHA1 1000 rounds; SM4-GM defaults to SM3-PBKDF2 10000 rounds, adjustable via `KdfIterations`; the KDF registry is pluggable (`IKdfProvider`, built-in `SM3-PBKDF2` / `PBKDF2-SHA256`), with the SM4-GM container selecting by SpecId and staying compatible with old archives
- The SM2-GM envelope is a **passwordless write path**: a random per-entry data key is sealed with the recipient's SM2 public key (ECIES C1-C3-C2), and the private key is delivered via the password pipeline
- WinZip AES-192 is not supported (throws `NotSupportedException`) — that strength is rare and offers no real benefit
- Wrong passwords fail fast via verifier bytes (constant-time comparison); corrupt data is never produced silently
- 7z encryption (AES-256) is provided by the native core; just set `Password`; RAR encryption is read-only

### Platform Differences & Known Limitations

| Platform | Native Core | Notes |
| --- | --- | --- |
| Windows (win-x64 / win-arm64 / win-x86) | Bundled | Full capabilities |
| Linux (linux-x64 / linux-musl-x64 / linux-arm64) | Bundled | musl suits Alpine and similar distributions |
| macOS (osx-x64 / osx-arm64) | Bundled | Full capabilities (including Apple Silicon) |
| Other platforms | None | Graceful degradation to managed capabilities (ZIP/TAR/TAR.GZ/GZ + ZIP-family encryption) |

Known limitations:
- RAR is **read-only** — the compression algorithm is proprietary; no compliant implementation can create RAR archives
- 7z archives do not support appending; each write produces a complete new archive
- ISO9660 / CAB are read-only; CAB supports only NONE-stored folders (MSZIP/LZX explicitly unsupported)
- SM4-GM and the SM2-GM envelope are custom specifications (ZIP has no official national-crypto standard); interop is limited to this component and third parties implementing the same specs

### Performance Reference

Throughput data for all formats (including per-encryption overhead) is available in the repository's [docs/BENCHMARKS.md](https://github.com/GeneralLibrary/GeneralZip/blob/main/docs/BENCHMARKS.md). Highlights:

- LZ4 / ZSTD have the highest throughput — ideal for logs and temporary data; LZMA2 (XZ) / BZ2 achieve the best ratios at a clear time cost
- WinZip AES-256 costs roughly 15%~25% on compression; SM4-GM reaches about 104 MB/s on highly compressible data
- GZ decompression can reach several GB/s (.NET built-in implementation)
- Compression ratios depend on workload: text (highly compressible) / mixed / random (incompressible) differ significantly — benchmark with your real data before choosing

---

## Related Resources

- [GeneralZip Repository](https://github.com/GeneralLibrary/GeneralZip)
- [Sample Code (samples/GeneralZip.Samples)](https://github.com/GeneralLibrary/GeneralZip/tree/main/samples/GeneralZip.Samples)
- [Architecture Document (docs/ARCHITECTURE.md)](https://github.com/GeneralLibrary/GeneralZip/blob/main/docs/ARCHITECTURE.md)
- [SM4-GM Encrypted ZIP Container Spec (docs/CRYPTO-SPEC.md)](https://github.com/GeneralLibrary/GeneralZip/blob/main/docs/CRYPTO-SPEC.md)
- [Benchmarks (docs/BENCHMARKS.md)](https://github.com/GeneralLibrary/GeneralZip/blob/main/docs/BENCHMARKS.md)
- [Sample Archives (samples/fixtures, demo password included)](https://github.com/GeneralLibrary/GeneralZip/tree/main/samples/fixtures)
