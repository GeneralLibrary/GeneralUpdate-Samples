---
sidebar_position: 11
---

# GeneralZip

**命名空间:** `GeneralZip`、`GeneralZip.Abstractions` | **主要入口:** `GeneralZip.CompressAsync / ExtractAsync`、`GeneralZipReader`、`GeneralZipWriter` | **NuGet 包:** `GeneralZip`

## 1. 组件简介

### 1.1 组件概述

**GeneralZip** 是一个跨格式、可加密（含国密）、事件驱动的 .NET 压缩组件。它用一套统一的 API 覆盖 ZIP、7z、RAR、TAR、GZ、BZ2、XZ、Zstd、LZ4、Brotli、CPIO、ISO9660、CAB 等十余种常见归档/压缩格式，打开归档时按魔数自动探测格式；压缩与解压全程提供事件系统（进度、交互式取密、覆盖决策、错误策略），全 API 支持 `CancellationToken`，并支持并行压缩/解压、流到流操作与 `IAsyncEnumerable` 条目枚举。加密方面内置 WinZip AES（通用互通）、SM4-GM 国密（文档化自定义 ZIP 扩展）与 SM2-GM 国密信封（公钥封装、无共享口令）三套方案，一个 `EncryptionAlgorithm` 开关切换。核心包零第三方 NuGet 依赖：SM3/SM4/SM2 密码学原语与 TAR/CPIO 容器为纯 C# 实现（GB/T 标准向量锁定），7z/bzip2/xz/zstd/lz4/brotli 由可选的 Rust 原生核心提供，核心缺失时优雅降级。

:::info v2.0.0 新增能力速览
一行式 `Append` / `Remove` / `Convert`、流到流重载、并行压缩/解压、Glob 条目过滤、ZIP 扩展压缩方法（BZip2/Zstd/LZMA）、SM2-GM 信封加密、可插拔 KDF 注册表、分卷 ZIP、Unix 属性/符号链接、CPIO/ISO9660/CAB/Brotli 格式与 `gz` 命令行工具——完整清单见[发布日志](../releaselog/GeneralZipReleaselog.md)。
:::

**核心能力：**

| 能力 | 说明 |
| --- | --- |
| 一套 API 多格式 | 所有格式映射到统一条目模型 `IArchiveEntry`，门面/读取器/写入器对全部格式行为一致，学一次即可处理所有格式 |
| 魔数自动探测 | 打开归档时按文件头魔数识别格式，扩展名兜底；内容与后缀不符也能正确处理 |
| 加密与国密 | WinZip AES-128/256（AE-2，与 WinZip/7-Zip/Windows 资源管理器互通）+ SM4-GM 国密（SM4-CTR + SM3-HMAC，容器规范文档化）；ZipCrypto 遗留格式可读 |
| 事件驱动 | 条目生命周期、条目级与整体进度、交互式口令（`PasswordRequired`）、覆盖询问（`OverwriteQuery`）、错误上报（`ErrorReport`） |
| 取消与进度 | 全 API 支持 `CancellationToken`；事件与 `IProgress<ArchiveProgress>` 双通道进度，含百分比、吞吐与 ETA |
| 安全解包 | 拒绝绝对路径与路径穿越条目；错误口令经校验值快速失败（常数时间比较），不会静默产生坏数据 |
| 零第三方依赖 | 除微软官方维护的包外不引用任何第三方 NuGet；SM3/SM4/TAR/CRC32 纯 C# 自研，经 GB/T 32905-2016 / GB/T 32907-2016 标准向量锁定 |
| 优雅降级 | Rust 原生核心缺失时，ZIP/TAR/TAR.GZ/GZ（纯托管）与全部 ZIP 系加密照常可用，仅 7z/RAR 与原生编解码不可用 |
| 开放扩展 | 格式注册表（`IArchiveFormat`）与加密注册表（`IEncryptionProvider`）双扩展点，自定义格式/算法一行注册 |

**解决的业务痛点：**
- 业务系统接收的压缩包来源多样（zip / tar.gz / 7z / rar / gz …），为每种格式引入专门库导致 API 碎片化、依赖膨胀
- 政务、金融等合规场景要求使用国密算法（SM4）加密归档，主流压缩库均不提供
- 面向终端用户的应用需要进度条、口令弹窗、覆盖确认、失败重试等交互能力，而多数压缩库只提供"压缩/解压"两个函数
- 处理不受信任来源的压缩包时，需要防范路径穿越等解包安全问题
- 第三方压缩依赖带来安全审计与版本升级负担

:::info GeneralZip 在生态中的定位
GeneralZip 是 GeneralLibrary 家族中**独立于 GeneralUpdate** 的通用压缩组件：它不感知版本号、更新策略或服务端协议，只负责"打包与解包"。补丁包生成、数据备份、日志归档、导出文件加密等场景都可以直接复用它的统一 API 与加密能力。
:::

**业务使用场景：**
- 桌面/服务端应用的文件备份与归档（ZIP / TAR.GZ）
- 更新补丁包、数据导出包的生成与解包
- 国密合规场景的加密归档（SM4-GM）
- 日志与数据的单文件压缩（GZ / XZ / ZSTD / LZ4）
- 外部来源压缩包的格式探测、完整性校验与安全解包
- 交互式解压 UI（口令输入、覆盖确认、错误重试）的后端支撑

### 1.2 环境与依赖

| 项目 | 说明 |
| --- | --- |
| **版本** | `2.0.0` |
| **目标框架** | `netstandard2.0`、`net8.0`（netstandard2.0 兼容 .NET Framework 4.6.1+ / .NET Core 2.0+ / .NET 5+） |
| **NuGet 依赖** | 仅微软官方维护的包：`Microsoft.Bcl.AsyncInterfaces`、`System.Text.Json`（均仅 netstandard2.0）；**核心包零第三方 NuGet 依赖** |
| **原生核心** | 内置 Rust cdylib `generalzip_native`（提供 7z / RAR / BZ2 / XZ / ZSTD / LZ4 / Brotli），NuGet 包捆绑 win-x64、linux-x64、linux-musl-x64、linux-arm64、win-arm64、win-x86、osx-x64、osx-arm64 八个平台的预编译产物 |
| **降级策略** | 原生核心缺失时 7z/RAR 与原生编解码不可用，其余能力（ZIP/TAR/TAR.GZ/CPIO/GZ + 全部 ZIP 系加密）不受影响 |
| **许可协议** | MIT |

---

## 2. 组件功能列表

| 功能名称 | 功能描述 | 类型 | 是否必填 | 备注限制 |
| --- | --- | --- | --- | --- |
| 一行式压缩 | 目录或单文件 → 归档，扩展名决定格式 | 基础 | 推荐 | `CompressAsync`，全 API 可取消 |
| 一行式解压 | 归档 → 目录，返回结果摘要 | 基础 | 推荐 | `ExtractAsync`，返回 `ArchiveResult` |
| 格式探测 | 魔数优先、扩展名兜底 | 基础 | 可选 | `DetectFormat`，无法识别返回 `null` |
| 流式写入 | 逐条目添加文件/目录/流，`Finish` 收尾 | 基础 | 可选 | `GeneralZipWriter`，`Dispose` 自动收尾 |
| 流式读取 | 条目枚举、惰性解压流、单条目解压 | 基础 | 可选 | `GeneralZipReader.OpenEntryStream` |
| 完整性测试 | CRC/HMAC/结构校验，不落盘 | 基础 | 可选 | `Test` / `TestAll` |
| 加密写入 | WinZip AES-128/256、SM4-GM（ZIP 容器） | 拓展 | 可选 | `EncryptionAlgorithm` + `Password` |
| 加密读取 | ZipCrypto / WinZip AES / SM4-GM / 7z AES-256 / RAR | 拓展 | 可选 | 口令预置或事件交互 |
| 进度上报 | 事件与 `IProgress<T>` 双通道，含吞吐与 ETA | 拓展 | 可选 | 条目内约每 64KB 一次 |
| 交互策略 | 口令询问、覆盖决策（5 种）、错误策略（3 种） | 拓展 | 可选 | `OverwritePolicy` / `ErrorPolicy` |
| 安全解包 | 拒绝绝对路径与路径穿越条目 | 基础 | 自动 | 记入 `ArchiveResult.Failures` |
| 自定义格式 | 实现 `IArchiveFormat` 后注册 | 拓展 | 可选 | `GeneralZip.Registry.Register` |
| 自定义加密 | 实现 `IEncryptionProvider` 后注册 | 拓展 | 可选 | `GeneralZip.Encryption.Register` |
| 原生格式 | 7z（LZMA2，读写 + AES-256）、RAR4/RAR5（只读） | 拓展 | 可选 | 需 Rust 原生核心 |
| 原生编解码 | BZ2 / XZ / ZSTD / LZ4 / Brotli、TAR.BZ2 | 拓展 | 可选 | 需 Rust 原生核心；回调式流式转码零临时文件 |
| 一行式追加/删除/转换 | 向既有归档追加条目、按名称/谓词删除条目、跨格式转换 | 拓展 | 可选 | `Append` / `Remove` / `Convert`，读出→重写语义 |
| 流到流操作 | Stream→Stream 压缩/解压/测试，无需落盘 | 拓展 | 可选 | `Compress` / `Extract` / `Test` 流重载 |
| 并行压缩/解压 | 条目级并行，写侧分片、读侧多 worker | 拓展 | 可选 | `MaxDegreeOfParallelism`，默认 1 串行 |
| Glob 条目过滤 | `**` 跨层匹配、字符类的包含/排除模式 | 拓展 | 可选 | `IncludePatterns` / `ExcludePatterns` |
| 分卷 ZIP | 按字节上限切分 `.001/.002/…`，打开 `.001` 自动拼接 | 拓展 | 可选 | `ArchiveWriteOptions.VolumeSize` |
| Unix 属性/符号链接 | Unix mode 读写、符号链接写入（默认跳过） | 拓展 | 可选 | `IUnixEntry` / `SymlinkPolicy` / `PreserveUnixAttributes` |
| 命令行工具 | `gz` dotnet 全局工具：a/x/t/l/c/rm/i/detect/keygen | 拓展 | 可选 | `GeneralZip.Cli` |

### 格式与加密矩阵

| 格式 | 读 | 写 | 加密读 | 加密写 | 实现层 |
| --- | :---: | :---: | :---: | :---: | --- |
| ZIP（Store/Deflate/**BZip2/Zstd/LZMA**/Zip64） | ✓ | ✓ | ZipCrypto / WinZip AES / **SM4-GM** / **SM2-GM** | ✓ | 自研引擎（纯 C#） |
| 7z（LZMA2） | ✓ | ✓ | AES-256 | AES-256 | Rust 原生核心 |
| RAR4/RAR5 | ✓ | —（专有） | ✓ 密码 | — | Rust 原生核心 |
| TAR / TAR.GZ | ✓ | ✓ | — | — | 自研 ustar + .NET GZipStream |
| TAR.BZ2 | ✓ | ✓ | — | — | 自研 ustar + 原生核心 |
| GZ（.gz/.gzip） | ✓ | ✓ | — | — | .NET 内置 GZipStream |
| BZ2 / XZ / ZSTD / LZ4 / Brotli | ✓ | ✓ | — | — | Rust 原生核心（回调式流式转码） |
| CPIO（newc/odc/binary） | ✓ | ✓（newc） | — | — | 自研容器（纯 C#） |
| ISO9660（+Joliet/Rock Ridge） | ✓ | —（只读） | — | — | 自研引擎（纯 C#） |
| CAB | ✓（NONE 存储） | —（只读） | — | — | 自研引擎（纯 C#） |

**部分 "—" 的原因：**
- **RAR 不可写** —— RAR 压缩算法为 RARLAB 专有，公开的 unrar 许可证明确禁止用于实现压缩器；读取（含密码解密）由原生核心完整支持。
- **TAR / GZ / BZ2 / XZ / ZSTD / LZ4 / Brotli / CPIO 无加密列** —— 这些格式的规范中不存在加密机制，没有存放 KDF 参数、IV 或认证标签的位置；需要加密时可在应用层"先加密后压缩"，或改用 ZIP + WinZip AES / SM4-GM / SM2-GM。
- **ISO9660 / CAB 只读** —— 写支持未实现；CAB 仅 NONE 存储文件夹可读（MSZIP/LZX 压缩文件夹明确不支持）。
- **SM4-GM 仅落地于 ZIP 是刻意设计** —— ZIP 的 extra field 机制天生支持第三方扩展，容器二进制规范已在 [docs/CRYPTO-SPEC.md](https://github.com/GeneralLibrary/GeneralZip/blob/main/docs/CRYPTO-SPEC.md) 完整文档化；向 7z 这类标准化容器注入自定义加密会产出其他工具无法打开的归档。
- **ZipCrypto 写入默认关闭** —— 56 位弱加密，仅限遗留兼容，需显式设置 `AllowWeakZipCrypto = true`。

---

## 3. API 配置说明

### 3.1 配置字段（属性 Props）

**ArchiveWriteOptions（创建归档）：**

| 字段名 | 数据类型 | 默认值 | 是否必填 | 枚举/取值范围 | 说明 |
| --- | --- | --- | --- | --- | --- |
| `Level` | `CompressionLevelChoice` | `Optimal` | 否 | `NoCompression(0)` / `Fastest(1)` / `Optimal(2)` / `SmallestSize(3)` | 压缩级别；ZIP 写侧 `SmallestSize` 映射为 Deflate Optimal，`NoCompression` 使用 Store（不压缩） |
| `EncryptionAlgorithm` | `string?` | `null`（不加密） | 加密时必填 | `"ZipCrypto"` / `"WinZipAES128"` / `"WinZipAES256"` / `"SM4-GM"` / 自定义 AlgorithmId | 加密算法标识，按名称查默认加密注册表 |
| `Password` | `string?` | `null` | `EncryptionAlgorithm` 非空时必填 | — | 加密口令，应从密钥管理服务获取，切勿硬编码 |
| `KdfIterations` | `int` | `10_000` | 否 | 正整数 | KDF 迭代次数（SM4-GM 等支持自定义 KDF 的算法） |
| `EncryptionProvider` | `IEncryptionProvider?` | `null` | 否 | — | 直接注入加密提供者，优先于按 `EncryptionAlgorithm` 查注册表 |
| `EntryNameEncoding` | `Encoding` | UTF-8（无 BOM） | 否 | — | 条目名编码；默认值同时设置 ZIP UTF-8 标志（0x0800），中文不乱码 |
| `ArchiveComment` | `string?` | `null` | 否 | — | 归档级注释（部分格式支持） |
| `DefaultLastWriteTime` | `DateTimeOffset?` | `null` | 否 | — | 条目未提供时间戳时使用；null 表示当前时间 |
| `Progress` | `IProgress<ArchiveProgress>?` | `null` | 否 | — | 进度接收器，与事件并行可用 |
| `ZipCompressionMethod` | `ZipCompressionMethodChoice` | `Auto`（Deflate） | 否 | `Auto(0)` / `Deflate(1)` / `BZip2(2)` / `Zstd(3)` / `Lzma(4)` | ZIP 条目压缩方法（其它格式忽略）；扩展方法与 7-Zip / Info-ZIP 互通 |
| `MaxDegreeOfParallelism` | `int` | `1`（串行） | 否 | ≥1 | 条目级并行压缩度（写侧分片并行） |
| `VolumeSize` | `long` | `0`（不分卷） | 否 | 字节数 | 分卷 ZIP：按上限切分 `.001/.002/…`，打开 `.001` 自动拼接 |
| `KdfAlgorithmId` | `string?` | `null`（算法默认） | 否 | 注册表中的 KdfId | KDF 提供者开关（内置 `SM3-PBKDF2` / `PBKDF2-SHA256`） |
| `RecipientPublicKey` | `string?` | `null` | SM2-GM 加密时必填 | hex 公钥（`04\|\|X\|\|Y` 130 字符或 `X\|\|Y` 128 字符） | SM2-GM 信封收件人公钥（无共享口令写路径） |
| `AllowWeakZipCrypto` | `bool` | `false` | 否 | `true` / `false` | 是否允许写入 ZipCrypto（弱加密，仅遗留兼容场景），显式开启才可写 |

**ArchiveOpenOptions（打开归档）：**

| 字段名 | 数据类型 | 默认值 | 是否必填 | 枚举/取值范围 | 说明 |
| --- | --- | --- | --- | --- | --- |
| `Password` | `string?` | `null` | 加密归档需要 | — | 预置口令（仅首次询问生效）；设置 `PasswordProvider` 后本字段失效 |
| `PasswordProvider` | `IPasswordProvider?` | `null` | 否 | — | 动态口令供给器（弹窗等交互场景）；返回 `null` 表示放弃解密 |
| `MaxPasswordAttempts` | `int` | `3` | 否 | 正整数 | 单条目/整包口令最大尝试次数，超过则抛出 |
| `EntryNameEncoding` | `Encoding?` | `null` | 否 | — | 条目名编码；null 时由格式自判（ZIP 优先 UTF-8 标志，否则回退 IBM437） |
| `Progress` | `IProgress<ArchiveProgress>?` | `null` | 否 | — | 进度接收器 |
| `SourceName` | `string?` | `null` | 否 | — | 来源文件名；单文件编解码格式用于推导条目名（门面层自动填充） |

**ExtractionOptions（解压）：**

| 字段名 | 数据类型 | 默认值 | 是否必填 | 枚举/取值范围 | 说明 |
| --- | --- | --- | --- | --- | --- |
| `OverwritePolicy` | `OverwritePolicy` | `Throw` | 否 | `Throw(0)` / `Overwrite(1)` / `Skip(2)` / `Ask(3)` / `Rename(4)` | 目标已存在时的策略；`Ask` 触发 `OverwriteQuery` 事件 |
| `ErrorPolicy` | `ErrorPolicy` | `Throw` | 否 | `Throw(0)` / `SkipAll(1)` / `RaiseEvent(2)` | 条目级错误策略；`RaiseEvent` 触发 `ErrorReport` 事件 |
| `MaxRenameAttempts` | `int` | `64` | 否 | 正整数 | 自动重命名上限（`name (2).ext` 形式），超出后抛出 |
| `Filter` | `Predicate<IArchiveEntry>?` | `null` | 否 | — | 只解压满足条件的条目（目录条目除外） |
| `Flatten` | `bool` | `false` | 否 | `true` / `false` | 忽略条目内部路径，全部落在目标根目录 |
| `PreserveTimestamps` | `bool` | `true` | 否 | `true` / `false` | 回写文件时间戳；持续集成等场景可关闭 |
| `PropagateEventExceptions` | `bool` | `false` | 否 | `true` / `false` | 事件处理器抛出的异常是否透传；默认隔离并按 Skip 处理 |
| `MaxRetryPerEntry` | `int` | `3` | 否 | 正整数 | `RaiseEvent` 策略下处理器连选 Retry 的上限 |
| `BufferSize` | `int` | `81920` | 否 | 正整数 | 单条目复制缓冲区大小（字节） |
| `MaxDegreeOfParallelism` | `int` | `1`（串行） | 否 | ≥1 | 条目级并行解压度（每 worker 重开归档） |
| `IncludePatterns` | `IReadOnlyList<string>?` | `null`（全部） | 否 | Glob 模式 | 只解压命中任一模式的条目（`**` 跨层、字符类） |
| `ExcludePatterns` | `IReadOnlyList<string>?` | `null` | 否 | Glob 模式 | 跳过命中任一模式的条目（优先于包含模式） |
| `SymlinkPolicy` | `SymlinkPolicy` | `Skip` | 否 | 枚举 | 符号链接条目处理策略（默认安全优先，不落盘链接） |
| `PreserveUnixAttributes` | `bool` | `false` | 否 | `true` / `false` | 解压保留 Unix mode 位（需条目提供 `IUnixEntry`） |

**事件应答枚举：**

| 枚举值 | 数值 | 说明 |
| --- | --- | --- |
| `OverwriteAction.Overwrite` | `0` | 直接覆盖 |
| `OverwriteAction.Skip` | `1` | 跳过该条目（事件参数默认值） |
| `OverwriteAction.Rename` | `2` | 自动重命名（`name (2).ext`） |
| `OverwriteAction.Abort` | `3` | 中止整个解压操作（等效取消） |
| `ErrorAction.Retry` | `0` | 重试该条目 |
| `ErrorAction.Skip` | `1` | 跳过该条目（事件参数默认值） |
| `ErrorAction.Abort` | `2` | 中止整个解压操作 |

### 3.2 静态门面与读写器方法

**静态门面 GeneralZip（零初始化，首次使用时隐式装配格式/加密/原生能力）：**

| 属性名 | 数据类型 | 说明 |
| --- | --- | --- |
| `Version` | `Version` | 组件版本（程序集版本） |
| `Registry` | `GeneralZipFormatRegistry` | 默认格式注册表（内置 13 种托管格式 + 原生 7z/RAR 按能力位自动注册） |
| `Encryption` | `GeneralZipEncryptionRegistry` | 默认加密注册表（内置 5 种算法） |

| 方法名 | 入参明细 | 使用场景 | 注意事项 |
| --- | --- | --- | --- |
| `CompressAsync(sourcePath, archivePath, configure, cancellationToken)` | 源路径（单文件或目录）、归档路径、选项回调、取消令牌 | 一行式压缩 | 格式由归档扩展名决定；压缩目录时以目录名为前缀（空目录无前缀）；来源不存在抛 `FileNotFoundException`；同步版为 `Compress` |
| `ExtractAsync(archivePath, destinationDirectory, configure, extraction, cancellationToken)` | 归档路径、目标目录、打开选项回调、解压选项、取消令牌 | 一行式解压 | 返回 `ArchiveResult`；同步版为 `Extract` |
| `Test(archivePath, configure, cancellationToken)` | 归档路径、打开选项回调、取消令牌 | 完整性测试 | 等价 `TestAll` + `ErrorPolicy.SkipAll`，不落盘；加密归档需在 `configure` 中提供口令 |
| `DetectFormat(path)` / `DetectFormat(stream)` | 文件路径或流 | 格式探测 | 魔数优先、扩展名兜底；流版本读取后恢复 `Position`；无法识别返回 `null` |
| `Append(archivePath, sourcePath, configure, cancellationToken)` / `AppendAsync` | 既有归档路径、追加的文件/目录、写入选项回调、取消令牌 | 向已有归档追加条目 | 读出→重写语义；加密归档自动继承原算法重新加密（盐/IV 不保留）；只读格式（RAR）不支持 |
| `Remove(archivePath, entryNames, predicate, configure, cancellationToken)` / `RemoveAsync` | 归档路径、条目名集合、条目谓词、打开选项回调、取消令牌 | 删除归档条目 | 精确名称（OrdinalIgnoreCase）与谓词可同时提供（并集）；加密归档同 Append 继承算法 |
| `Convert(sourceArchive, targetArchive, writeConfigure, openConfigure, cancellationToken)` / `ConvertAsync` | 源归档、目标路径（扩展名决定新格式）、写入/打开选项回调、取消令牌 | 跨格式转换 | 加密不跨格式保留，需重新加密时经 `writeConfigure` 指定 |

此外提供流到流重载：`Compress(Stream source, Stream output, formatName, entryName, …)`、`Extract(Stream input, destinationDirectory, …)`、`Test(Stream input, …)`，无需落盘即可完成内存/网络流操作。

:::note 门面类与命名空间同名
`namespace GeneralZip` 中的静态类也叫 `GeneralZip`。若需要与其它类型限定写法混用，可用 `using static GeneralZip.GeneralZip;` 裸调用 `CompressAsync()` 等方法。
:::

**GeneralZipReader（归档读取门面）：**

| 方法名 | 入参明细 | 使用场景 | 注意事项 |
| --- | --- | --- | --- |
| `Open(path, configure, initialize, registry)`（静态） | 归档路径、打开选项回调、事件订阅钩子、注册表 | 从文件打开归档 | 魔数探测 → 扩展名兜底；`initialize` 在格式解析前执行，**整包加密格式（如 7z 头部加密）必须在此订阅 `PasswordRequired`**；打开失败自动清理流 |
| `Open(stream, formatName, configure, initialize, registry)`（静态） | 流、显式格式名、选项回调、事件订阅钩子、注册表 | 从流打开归档 | `formatName` 给定时跳过签名探测；非可寻址流自动包装探测 |
| `OpenAsync(path, ...)`（静态） | 与文件版签名对齐 | 异步打开 | 内部为同步 IO，签名对齐便于调用方切换 |
| `OpenEntryStream(entry)` | 条目 | 读取条目内容 | 惰性解密 + 解压的顺序流；口令按预置/事件自动协商 |
| `ExtractToDirectory(destinationDirectory, options, cancellationToken)` | 目标目录、解压选项、取消令牌 | 批量解压 | 保留条目内部结构；返回 `ArchiveResult`；异步版 `ExtractToDirectoryAsync` 与同步版同实现 |
| `ExtractEntry(entry, destinationFilePath, options, cancellationToken)` | 条目、目标文件路径、解压选项、取消令牌 | 解压单个条目 | — |
| `TestAll(options, cancellationToken)` | 解压选项、取消令牌 | 完整性测试 | 校验 CRC/HMAC/结构，不落盘 |
| `Dispose()` | 无 | 释放读取器 | 关闭底层流 |

| 属性名 | 数据类型 | 说明 |
| --- | --- | --- |
| `Format` | `ArchiveFormatInfo` | 探测到的格式信息 |
| `Entries` | `IReadOnlyList<IArchiveEntry>` | 归档内全部条目 |
| `IsEncrypted` | `bool` | 归档是否包含加密内容 |
| `Password` | `string?` | 打开后补充的口令（优先级低于事件应答，下次询问生效） |

**GeneralZipWriter（归档写入门面）：**

| 方法名 | 入参明细 | 使用场景 | 注意事项 |
| --- | --- | --- | --- |
| `CreateNew(archivePath, configure, formatName, registry)`（静态） | 归档路径、写入选项回调、显式格式名、注册表 | 创建归档文件 | 扩展名决定格式（`.tar.gz` 等复合后缀可识别）；`formatName` 显式指定时覆盖扩展名推断 |
| `CreateNew(output, formatName, configure, registry)`（静态） | 输出流、格式名、写入选项回调、注册表 | 写入流 | 流无法从扩展名推断，`formatName` 必填 |
| `AddFile(filePath, entryName)` / `AddFileAsync(...)` | 源文件路径、条目名 | 添加文件 | `entryName` 缺省为文件名；异步版经线程池包装同步管道 |
| `AddDirectory(directoryPath, entryPrefix, filter)` / `AddDirectoryAsync(...)` | 目录路径、条目前缀、路径谓词 | 递归添加目录 | 自动生成目录占位条目（格式支持时）；`filter` 过滤文件路径 |
| `AddStream(entryName, content, size, lastWriteTime)` | 条目名、内容流、大小、时间戳 | 添加流内容 | `size` 已知时进度更准确 |
| `Finish(cancellationToken)` / `FinishAsync(cancellationToken)` | 取消令牌 | 收尾（写中央目录/索引） | 重复调用为无操作；`Dispose` 未 `Finish` 时自动调用 |
| `Dispose()` / `DisposeAsync()` | 无 | 释放写入器 | 未显式 `Finish` 时自动收尾 |

| 属性名 | 数据类型 | 说明 |
| --- | --- | --- |
| `Format` | `ArchiveFormatInfo` | 当前归档格式信息 |

:::warning 写入器非线程安全
`GeneralZipWriter` 不支持并发添加条目，请在单线程内按序调用 `AddFile` / `AddDirectory` / `AddStream`。
:::

**注册表（扩展点入口）：**

| 类型 | 方法 | 说明 |
| --- | --- | --- |
| `GeneralZipFormatRegistry` | `Register(format)` | 注册格式提供者；同名覆盖，线程安全 |
| | `Unregister(formatName)` | 按名称移除格式 |
| | `Snapshot()` / `FindByName(name)` | 枚举全部格式 / 按名称查找 |
| | `Detect(stream)` | 签名探测，首个命中者胜出；单个提供者异常不中断探测 |
| | `DetectByExtension(path)` | 扩展名匹配；`x.tar.gz` 命中 `.tar.gz` 而非 `.gz` |
| `GeneralZipEncryptionRegistry` | `Register(provider)` | 注册加密提供者；同 `AlgorithmId` 覆盖 |
| | `Find(algorithmId)` | 按算法标识查找 |
| | `FindByZipExtraFieldId(id)` / `FindAllByZipExtraFieldId(id)` | 按 ZIP extra field 标识查找；候选按注册逆序（新注册优先） |
| | `Snapshot()` | 枚举全部算法 |

**结果与条目模型：**

| 类型 | 成员 | 说明 |
| --- | --- | --- |
| `ArchiveResult` | `ProcessedCount` / `SkippedCount` / `FailedCount` / `Failures` / `Aborted` / `Success` | 批量操作结果摘要；`Success` 即 `FailedCount == 0` |
| `FailedEntry` | `EntryName` / `Exception` | 单条失败记录 |
| `IArchiveEntry` | `FullName` / `Name` / `Size` / `CompressedSize` / `LastWriteTime` / `IsDirectory` / `IsEncrypted` / `EncryptionAlgorithm` / `Crc32` / `Comment` | 所有格式统一条目视图；路径统一 `/` 分隔；未知大小为 `-1`；加密隐藏 CRC 时为 `null` |
| `ArchiveFormatInfo` | `Name` / `Extensions` / `Capabilities` / `Description` / `CanRead` 等便捷属性 | 格式元信息，如 `"ZIP"`、`.zip` |

### 3.3 回调事件与异常体系

**事件一览（GeneralZipReader 支持全部 8 种；GeneralZipWriter 支持前 5 种）：**

| 事件名称 | 回调参数 | 触发时机 | 使用说明 |
| --- | --- | --- | --- |
| `EntryStarted` / `EntryCompleted` | `EntryEventArgs` | 条目开始/完成 | `e.EntryName` 为条目完整路径 |
| `EntryProgress` | `EntryProgressEventArgs` | 条目内字节进度（约每 64KB 一次） | `e.Percent` 在大小未知时为 `null` |
| `EntryFailed` | `EntryFailedEventArgs` | 条目失败 | `e.Exception` 携带具体异常 |
| `OverallProgress` | `ArchiveProgress` | 整体进度快照 | 含阶段、百分比、吞吐、ETA，事件与 `IProgress<T>` 并行投递 |
| `PasswordRequired` | `PasswordRequiredEventArgs` | 遇到加密内容且需要口令 | 设置 `e.Password` 应答；`e.Cancel = true` 放弃（抛 `PasswordRequiredException`）；`e.Attempt` 从 1 起，口令错误后自动以 `Attempt+1` 重新询问 |
| `OverwriteQuery` | `OverwriteQueryEventArgs` | `OverwritePolicy.Ask` 且目标已存在 | `e.Action` 决策，默认 `Skip`（保守：不改磁盘） |
| `ErrorReport` | `ArchiveErrorEventArgs` | `ErrorPolicy.RaiseEvent` 且条目出错 | `e.Action` 决策（Retry/Skip/Abort），默认 `Skip`；`e.RetryCount` / `e.MaxRetry` 供重试判断 |

**ArchiveProgress 属性（不可变快照）：**

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `Phase` | `ArchivePhase` | `Analyzing`（解析结构）/ `Compressing`（压缩写入）/ `Decompressing`（解压读取）/ `Testing`（测试）/ `Finalizing`（收尾） |
| `CurrentEntry` | `string?` | 当前处理的条目名 |
| `EntriesCompleted` / `EntriesTotal?` | `long` / `long?` | 已完成条目数 / 条目总数（未知为 null） |
| `BytesProcessed` / `BytesTotal?` | `long` / `long?` | 已处理未压缩字节 / 未压缩总字节（未知为 null） |
| `Percent?` | `double?` | 整体百分比 0~100（无法计算为 null） |
| `BytesPerSecond?` / `Eta?` | `double?` / `TimeSpan?` | 吞吐（字节/秒）/ 预计剩余时间 |

:::note 交互事件默认行为
`OverwriteQuery` 与 `ErrorReport` 未订阅或未决策时，默认动作均为 **Skip**（保守：不改磁盘、不中断流程）。事件处理器抛出的异常默认被隔离并按 Skip 处理，需要"事件异常即失败"时设置 `ExtractionOptions.PropagateEventExceptions = true`。
:::

**异常体系：**

```text
ArchiveException（基类，携带 EntryName）
├─ UnsupportedFormatException       格式无法识别或本组件不支持
├─ NativeCoreMissingException       Rust 原生核心未部署（消息含部署指引）
├─ PasswordRequiredException        无口令可用（未预置且事件未应答/放弃）
├─ InvalidPasswordException         口令错误（Attempt 记录第几次尝试）
└─ ArchiveCorruptedException        CRC/HMAC 校验失败、结构非法、意外截断
```

| 异常类型 | 典型触发场景 | 处理建议 |
| --- | --- | --- |
| `UnsupportedFormatException` | 打开不识别的格式；`EncryptionAlgorithm` 未注册 | 提示用户格式不支持，或注册自定义提供者 |
| `NativeCoreMissingException` | 使用 7z/RAR/BZ2/XZ/ZSTD/LZ4/BROTLI 但核心缺失 | 按异常消息部署原生核心，或降级使用托管格式 |
| `PasswordRequiredException` | 加密归档未提供口令且事件放弃 | 引导用户输入口令后重试 |
| `InvalidPasswordException` | 口令错误（校验值不匹配） | 提示重输；事件模式下自动重新询问 |
| `ArchiveCorruptedException` | 传输损坏、篡改、截断 | 校验源完整性后重新获取归档 |
| `OperationCanceledException` | `CancellationToken` 触发取消 | 正常取消语义，按需清理临时文件 |

---

## 4. 扩展示例（高阶用法）

### 4.1 组件可扩展能力总览

GeneralZip 的格式与加密算法均为注册表驱动，内置实现与第三方实现地位相同——内置 ZIP/TAR 等也是通过 `IArchiveFormat` 注册进默认注册表的。

| 扩展点 | 类型 | 说明 | 注册入口 |
| --- | --- | --- | --- |
| 自定义归档格式 | `IArchiveFormat` | 提供格式元信息、魔数/扩展名探测与读写器工厂；实现须无状态、线程安全 | `GeneralZip.Registry.Register` |
| 自定义加密算法 | `IEncryptionProvider` | 提供算法标识、读写支持、extra field 布局与加解密流工厂；实现须无状态、线程安全 | `GeneralZip.Encryption.Register` |
| 动态口令供给 | `IPasswordProvider` | `GetPassword(PasswordRequest)` 返回口令；返回 `null` 表示放弃 | `ArchiveOpenOptions.PasswordProvider` |
| 进度接收 | `IProgress<ArchiveProgress>` | 与事件并行的进度通道 | 选项类 `Progress` 属性 |
| 覆盖/错误策略 | `OverwritePolicy` / `ErrorPolicy` | 策略枚举 + 事件应答决策 | `ExtractionOptions` |

内置注册清单：格式注册表预注册 ZIP、TAR、TAR.GZ、TAR.BZ2、GZIP、BZ2、XZ、ZSTD、LZ4、Brotli、CPIO、ISO9660、CAB 共 13 种托管格式，并按能力位自动挂接原生 7Z/RAR；加密注册表预注册 `ZipCrypto`、`WinZipAES128`、`WinZipAES256`、`SM4-GM`、`SM2-GM` 共 5 种算法。

### 4.2 分场景示例

#### 场景 1：国密 SM4-GM 加密备份与完整性校验

【场景说明】企业数据备份要求使用国密算法加密。压缩时选择 SM4-GM；恢复前先做完整性测试（HMAC 校验，不落盘），再按需解包或只读取单个条目。

【示例代码】

```csharp
using GeneralZip;
using GeneralZip.Abstractions;

var password = GetPasswordFromSecretStore();   // 从密钥管理服务获取，切勿硬编码

// 1. 压缩并加密（SM4-GM：SM4-CTR + SM3-HMAC，容器规范见 docs/CRYPTO-SPEC.md）
await GeneralZip.CompressAsync(@"C:\data", @"D:\backup.zip", o =>
{
    o.Level = CompressionLevelChoice.Optimal;
    o.EncryptionAlgorithm = "SM4-GM";
    o.Password = password;
    o.KdfIterations = 10_000;                  // SM3-PBKDF2 迭代次数，按安全要求调整
});

// 2. 恢复前先做完整性测试（校验 HMAC 与结构，不落盘）
using (var verifier = GeneralZipReader.Open(@"D:\backup.zip", o => o.Password = password))
{
    var test = verifier.TestAll();
    if (!test.Success)
        throw new InvalidOperationException($"备份已损坏：{test.FailedCount} 个条目校验失败");
}

// 3. 只取其中一个条目：惰性解密 + 解压流，无需解出整个归档
using var reader = GeneralZipReader.Open(@"D:\backup.zip", o => o.Password = password);
var entry = reader.Entries.Single(e => e.FullName == "data/config.json");
using var stream = reader.OpenEntryStream(entry);
using var output = File.Create(@"D:\restore\config.json");
stream.CopyTo(output);

Console.WriteLine($"条目加密算法：{entry.EncryptionAlgorithm}");   // "SM4-GM"
```

【效果&注意事项】
- 错误口令在首个条目上快速失败（4 字节校验值 + 常数时间比较），抛 `InvalidPasswordException`，不会静默产生坏数据
- SM4-GM 目前落地于 ZIP 容器（extra field 0x4D53）；与第三方工具互通需对方实现同一规范
- `KdfIterations` 只影响新建归档；读取时迭代次数从容器内恢复，无需配置

#### 场景 2：交互式解压（口令弹窗、覆盖确认、错误重试）

【场景说明】桌面应用解压一个来源不明的加密包：口令未知需弹窗询问；目标目录已有同名文件需决策；个别损坏条目允许重试或跳过；全程展示进度，结束后给出摘要。

【示例代码】

```csharp
using GeneralZip;
using GeneralZip.Abstractions;

using var reader = GeneralZipReader.Open(archivePath, configure: null, r =>
{
    // 口令询问：整包加密格式（如 7z 头部加密）在 Open 期间就会触发，务必在此订阅
    r.PasswordRequired += (_, e) =>
    {
        e.Password = PromptPassword(e.EntryName, e.Attempt);   // 你的 UI 逻辑
        if (e.Attempt >= 3) e.Cancel = true;                   // 3 次后放弃
    };

    r.OverwriteQuery += (_, e) =>
    {
        // 归档内版本更新时覆盖；否则自动重命名保留两份
        var targetTime = File.GetLastWriteTime(e.TargetPath);
        e.Action = e.LastWriteTime > targetTime
            ? OverwriteAction.Overwrite
            : OverwriteAction.Rename;
    };

    r.ErrorReport += (_, e) =>
    {
        Console.WriteLine($"条目 {e.EntryName} 第 {e.RetryCount}/{e.MaxRetry} 次失败：{e.Exception.Message}");
        e.Action = e.RetryCount < e.MaxRetry ? ErrorAction.Retry : ErrorAction.Skip;
    };

    r.OverallProgress += (_, p) => UpdateProgressBar(p.Percent, p.Eta);
});

var result = reader.ExtractToDirectory(@"D:\out", new ExtractionOptions
{
    OverwritePolicy = OverwritePolicy.Ask,      // 触发 OverwriteQuery 事件
    ErrorPolicy = ErrorPolicy.RaiseEvent,       // 触发 ErrorReport 事件
    MaxRetryPerEntry = 3,
    Filter = e => e.FullName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase),
    PreserveTimestamps = true,
});

Console.WriteLine($"处理 {result.ProcessedCount}，跳过 {result.SkippedCount}，失败 {result.FailedCount}");
foreach (var fail in result.Failures)
    Console.WriteLine($"  {fail.EntryName}: {fail.Exception.Message}");
```

【效果&注意事项】
- 事件处理器异常默认被隔离并按 Skip 处理；需要"异常即失败"时设置 `PropagateEventExceptions = true`
- 不安全的条目路径（绝对路径、路径穿越）无论何种策略都会被拒绝并记入 `Failures`
- `ArchiveResult.Aborted` 为 `true` 表示用户在覆盖询问中选择中止或操作被取消

#### 场景 3：注册自定义加密提供者

【场景说明】企业内部需要接入自研加密算法。GeneralZip 通过 `IEncryptionProvider` + ZIP extra field 机制开放加密扩展点：算法参数随条目持久化，读取端凭 extra field 恢复参数。

【示例代码】

以下为骨架示例（省略了密钥协商细节，完整实现可参考内置 `WinZipAesProvider` / `Sm4GmProvider` 源码）：

```csharp
using System.Collections.Generic;
using System.IO;
using GeneralZip;
using GeneralZip.Abstractions;
using GeneralZip.Cryptography;

public sealed class MyAesCtrProvider : IEncryptionProvider
{
    // 自定义 extra field id，避让 0x9901（WinZip AES）与 0x4D53（SM4-GM）
    private const ushort ExtraFieldId = 0x9F01;

    public string AlgorithmId => "MY-AES-CTR";
    public bool SupportsWrite => true;
    public bool SupportsRead => true;
    public IReadOnlyCollection<ushort> ZipExtraFieldIds => new ushort[] { ExtraFieldId };

    // 写入侧：生成盐/IV → 声明 extra field（携带参数）→ 给出加密流工厂
    public EntryEncryptionPlan PrepareWrite(EncryptionWriteContext context)
    {
        var salt = Kdf.RandomBytes(16);
        var key  = Kdf.Pbkdf2Sha1(context.Password, salt, 10_000, 32);
        var mac  = Kdf.CreateIncrementalHmacSha1(key);     // 示例简化，生产实现请使用完整 MAC 方案
        var ctr  = new CtrCipher(new AesBlockCipher(), key, Kdf.RandomBytes(16));

        return new EntryEncryptionPlan(
            extraFieldId: ExtraFieldId,
            extraFieldData: salt,                          // 随条目持久化，读取端凭它恢复参数
            cipherOverhead: 0,
            encryptionStreamFactory: plain =>
                new CtrWriteStream(plain, ctr, mac, trailerLength: 0));
    }

    // 读取侧：从 context.Parameters 恢复 extra field 参数 → 返回解密流
    // 口令校验失败必须抛 InvalidPasswordException（校验值用 Kdf.FixedTimeEquals 比较）
    public Stream OpenRead(Stream cipherSource, EncryptionReadContext context)
    {
        // ... 恢复盐值 → 派生密钥 → 构造解密流（参考 CRYPTO-SPEC.md 解密端伪码）
        throw new System.NotImplementedException();
    }
}

// 注册后即可按名称使用（同 AlgorithmId 覆盖旧实现，线程安全）
GeneralZip.Encryption.Register(new MyAesCtrProvider());

await GeneralZip.CompressAsync(@"C:\data", @"D:\sealed.zip", o =>
{
    o.EncryptionAlgorithm = "MY-AES-CTR";
    o.Password = password;
});
```

【效果&注意事项】
- 提供者实现必须**无状态、线程安全**，每次调用返回独立的流实例
- 读取端按 extra field id 匹配候选，候选顺序为注册逆序（新注册优先）
- 加密实现属于安全敏感代码，建议直接复用内置 `WinZipAES256`（通用互通）或 `SM4-GM`（国密合规）；本示例仅演示扩展点用法

---

## 5. 常规使用示例

### 5.1 快速入门示例（最简 demo）

```csharp
using GeneralZip;

// 目录 → 归档（格式由扩展名决定）
await GeneralZip.CompressAsync(@"C:\data", @"D:\backup.zip");

// 归档 → 目录
var result = await GeneralZip.ExtractAsync(@"D:\backup.zip", @"D:\restore");
Console.WriteLine($"解压 {result.ProcessedCount} 个条目，失败 {result.FailedCount}，成功：{result.Success}");

// 探测格式（魔数优先，扩展名兜底）
Console.WriteLine(GeneralZip.DetectFormat(@"D:\backup.zip")?.Name);   // "ZIP"
```

### 5.2 基础参数组合示例

```csharp
using GeneralZip;
using GeneralZip.Abstractions;

var progress = new Progress<ArchiveProgress>(p =>
    Console.Write($"\r{p.Percent:F0}% {p.CurrentEntry}"));

// 写入：压缩级别 + 归档注释 + 进度
await GeneralZip.CompressAsync(@"C:\photos", @"D:\photos.zip", o =>
{
    o.Level = CompressionLevelChoice.SmallestSize;
    o.ArchiveComment = "daily photos backup";
    o.Progress = progress;
});

// 读取：条目名编码 + 过滤 + 重命名策略 + 跳过错误
using var reader = GeneralZipReader.Open(@"D:\photos.zip");

var result = reader.ExtractToDirectory(@"D:\restore", new ExtractionOptions
{
    OverwritePolicy = OverwritePolicy.Rename,   // 冲突时生成 "name (2).jpg"
    ErrorPolicy = ErrorPolicy.SkipAll,          // 损坏条目跳过，不中断
    Filter = e => e.FullName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase),
});

foreach (var fail in result.Failures)
    Console.WriteLine($"失败：{fail.EntryName}: {fail.Exception.Message}");
```

### 5.3 真实业务落地示例（夜间日志归档）

完整业务流程：整目录归档为 TAR.BZ2（高压缩比）→ 完整性校验 → 按需抽取审计条目 → 失败产物自动清理。

```csharp
using GeneralZip;
using GeneralZip.Abstractions;

/// <summary>夜间日志归档：打包昨天的日志目录，失败条目隔离，校验不过自动清理产物。</summary>
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

        // 1. 整目录 → TAR.BZ2（BZ2 高压缩比由原生核心提供）
        await GeneralZip.CompressAsync(source, target, o =>
        {
            o.Level = CompressionLevelChoice.Optimal;
        }, _ct);

        // 2. 完整性校验（CRC + 结构，不落盘）；不过关则删除产物并告警
        using (var verifier = GeneralZipReader.Open(target))
        {
            var test = verifier.TestAll(cancellationToken: _ct);
            if (!test.Success)
            {
                File.Delete(target);
                throw new InvalidOperationException($"归档校验失败：{test.FailedCount} 个条目损坏");
            }
        }

        // 3. 只解出 error 级日志供审计，其余条目不触碰
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

## 6. 全局配置

GeneralZip 不依赖全局配置文件。所有行为通过选项对象（`ArchiveWriteOptions` / `ArchiveOpenOptions` / `ExtractionOptions`）、注册表与原生核心部署状态决定。

### 原生核心部署

NuGet 包已捆绑 8 个 RID（Windows x64/ARM64/x86、Linux x64/musl/ARM64、macOS x64/ARM64）的预编译 Rust 原生核心，常规安装**无需任何操作**：首次使用时自动探测并加载（幂等、线程安全），无需任何初始化调用。

手动部署时，将 `generalzip_native.dll`（`.so` / `.dylib`）放到以下任一位置：

1. 应用程序目录；
2. 应用目录下的 `runtimes/<RID>/native/`；
3. `GENERALZIP_NATIVE` 环境变量指向的路径。

检查加载状态与降级行为：

```csharp
using GeneralZip.Native;

Console.WriteLine(GeneralZipNative.CoreVersion);   // 原生核心版本；未加载为 null
Console.WriteLine(GeneralZipNative.CorePath);      // 加载路径

// 也可显式尝试加载（幂等）：
NativeCoreLoader.TryLoad();
```

| 场景 | 行为 |
| --- | --- |
| 原生核心可用 | ZIP/TAR/TAR.GZ/TAR.BZ2/GZ + 7z/RAR + BZ2/XZ/ZSTD/LZ4/BROTLI 全能力可用 |
| 原生核心缺失 | ZIP/TAR/TAR.GZ/GZ 与全部 ZIP 系加密照常可用；7z/RAR/BZ2/XZ/ZSTD/LZ4/BROTLI 抛 `NativeCoreMissingException`（消息含部署指引） |

### 条目名编码与中文文件名

- **写入**：默认 UTF-8（无 BOM）并设置 ZIP UTF-8 标志（0x0800），Windows/Linux/macOS 中文文件名不乱码
- **读取**：优先条目自带的 UTF-8 标志；无标志的旧包回退 `ArchiveOpenOptions.EntryNameEncoding`（null 则 ZIP 回退 IBM437）
- **读取非 UTF-8 编码的历史压缩包**（如 GBK 打包工具生成）：显式指定 `EntryNameEncoding`，例如 `Encoding.GetEncoding("GB18030")`（.NET Core/.NET 5+ 需先注册 `CodePagesEncodingProvider`）
- 条目路径内部统一 `/` 分隔；解压时拒绝绝对路径与路径穿越条目

### 加密算法选择指南

| AlgorithmId | 强度 | 互通性 | 适用场景 |
| --- | --- | --- | --- |
| `WinZipAES256` | AES-256-CTR + HMAC-SHA1（AE-2，extra field 0x9901） | WinZip / 7-Zip / Windows 资源管理器 | 通用互通首选 |
| `WinZipAES128` | AES-128-CTR + HMAC-SHA1（AE-2） | 同上 | 兼容老旧工具 |
| `SM4-GM` | SM4-CTR + SM3-HMAC（extra field 0x4D53） | 本组件及实现同一规范的第三方 | 国密合规（政务/金融等） |
| `SM2-GM` | SM2-ECIES 信封（每条目随机数据密钥，extra field 0x4D54） | 本组件及实现同一规范的第三方 | 公钥分发、无共享口令场景（`RecipientPublicKey`） |
| `ZipCrypto` | 56 位（弱） | 几乎所有工具 | 仅读取遗留包；写入需 `AllowWeakZipCrypto = true`，不推荐 |

- KDF：WinZip AES 固定 PBKDF2-SHA1 1000 轮；SM4-GM 默认 SM3-PBKDF2 10000 轮，`KdfIterations` 可调；KDF 注册表可插拔（`IKdfProvider`，内置 `SM3-PBKDF2` / `PBKDF2-SHA256`），SM4-GM 容器按 SpecId 选择、旧包兼容
- SM2-GM 信封为**无口令写路径**：每条目随机数据密钥以收件人 SM2 公钥封装（ECIES C1-C3-C2），私钥经口令管道下发
- WinZip AES-192 不受支持（抛 `NotSupportedException`）——该强度罕见且无实际增益
- 加密归档的错误口令经校验值快速失败（常数时间比较），不会静默产生坏数据
- 7z 加密（AES-256）由原生核心提供，`Password` 直接可用；RAR 加密仅支持读取

### 平台差异与已知限制

| 平台 | 原生核心 | 说明 |
| --- | --- | --- |
| Windows（win-x64 / win-arm64 / win-x86） | 捆绑 | 完整能力 |
| Linux（linux-x64 / linux-musl-x64 / linux-arm64） | 捆绑 | musl 适用于 Alpine 等发行版 |
| macOS（osx-x64 / osx-arm64） | 捆绑 | 完整能力（含 Apple Silicon） |
| 其它平台 | 无 | 优雅降级为纯托管能力（ZIP/TAR/TAR.GZ/GZ + ZIP 系加密） |

已知限制：
- RAR **仅读取**——压缩算法专有，任何合规实现均无法创建 RAR 归档
- 7z 归档不支持追加，每次写入生成完整新包
- ISO9660 / CAB 仅读取；CAB 仅支持 NONE 存储文件夹（MSZIP/LZX 明确不支持）
- SM4-GM 与 SM2-GM 信封为自定义规范（ZIP 无官方国密标准），互通限于本组件与实现同一规范的第三方

### 性能参考

全部格式的吞吐数据（含各加密算法的开销对比）见仓库 [docs/BENCHMARKS.md](https://github.com/GeneralLibrary/GeneralZip/blob/main/docs/BENCHMARKS.md)，要点：

- LZ4 / ZSTD 吞吐最高，适合日志与临时数据；LZMA2（XZ）/ BZ2 压缩比最高但耗时明显
- WinZip AES-256 压缩端约 15%~25% 开销；SM4-GM 对高可压数据可达约 104 MB/s
- GZ 解压可数 GB/s（.NET 内置实现）
- 压缩比与负载类型相关：文本（高可压）/ 混合 / 随机（不可压）差异显著，选型前建议以自己的真实数据实测

---

## 相关资源

- [GeneralZip 仓库](https://github.com/GeneralLibrary/GeneralZip)
- [示例代码（samples/GeneralZip.Samples）](https://github.com/GeneralLibrary/GeneralZip/tree/main/samples/GeneralZip.Samples)
- [架构设计文档（docs/ARCHITECTURE.md）](https://github.com/GeneralLibrary/GeneralZip/blob/main/docs/ARCHITECTURE.md)
- [SM4-GM 加密 ZIP 容器规范（docs/CRYPTO-SPEC.md）](https://github.com/GeneralLibrary/GeneralZip/blob/main/docs/CRYPTO-SPEC.md)
- [性能基准（docs/BENCHMARKS.md）](https://github.com/GeneralLibrary/GeneralZip/blob/main/docs/BENCHMARKS.md)
- [样例归档（samples/fixtures，含演示口令）](https://github.com/GeneralLibrary/GeneralZip/tree/main/samples/fixtures)
