---
slug: generalzip-releaselog
title: GeneralZip
authors: juster
tags: [log]
---

# 📒Release log

### 📍2026-11-20 — v0.0.2-beta 能力扩展（待发布）

*以下变更已合入 [GeneralZip 仓库](https://github.com/GeneralLibrary/GeneralZip) `main` 分支（PR #1、PR #2），对应版本为 `0.0.2-beta`；正式发布后此处将标注 NuGet 版本号。*

- **隐式引导（破坏性重构）**: Rust 原生核心改为在 `GeneralZipFormatRegistry.Default` 首次构造时自动装配——自动加载核心、按能力位注册 7z/RAR 提供者、注入原生编解码委托，自定义注册表场景由编解码路由 `EnsureBridge()` 兜底；`GeneralZip.Initialize()` 与 `GeneralZipNative.Initialize()` 已移除，改用 `NativeCoreLoader.TryLoad()` 探测核心可用性
- **一行式归档操作**: 门面新增 `Append/AppendAsync`（向已有 ZIP 归档追加条目）、`Remove/RemoveAsync`（按条目名删除）、`Convert/ConvertAsync`（跨格式转换）——读出→重写语义；加密归档转换时自动继承原加密算法与口令，杜绝静默产出明文包
- **流到流重载**: `Compress` / `Extract` / `Test` 新增 Stream→Stream 重载，无需落盘即可完成内存/网络流的压缩、解压与完整性校验
- **Glob 条目过滤**: 解压新增 `ExtractionOptions.IncludePatterns` / `ExcludePatterns`（`**` 跨层匹配、字符类），排除模式优先于包含模式
- **ZIP 扩展压缩方法**: `ArchiveWriteOptions.ZipCompressionMethod` 新增 BZip2（方法 12）/ Zstd（方法 93）/ LZMA（方法 14，method-14 EOPM 帧）三种扩展方法，与 7-Zip / Info-ZIP 互通，默认 `Auto`（Deflate）
- **原生编解码流式化**: Rust 核心重写为回调式 push-pull FFI（`gzp_codec_crun/drun`），bzip2/xz/zstd/lz4/brotli 全部零临时文件转码，彻底移除 v1 的"zstd 一次性内存 API"与"临时文件整段编解码"限制
- **并行压缩/解压**: 写侧分片并行压缩、读侧每 worker 重开归档 + 锁串行事件中继，`ArchiveWriteOptions.MaxDegreeOfParallelism` / `ExtractionOptions.MaxDegreeOfParallelism` 控制（默认 1 = 串行，行为不变）
- **异步枚举**: `GeneralZipAsyncExtensions.EnumerateEntriesAsync`（`IAsyncEnumerable<IArchiveEntry>`）与 `ExtractEntryAsync`
- **国密 SM2 信封加密**: 纯 C# SM2 实现（GB/T 32918.5 曲线，ECIES C1-C3-C2），与 python gmssl 双向互验并锁定互操作锚向量；`Sm2GmProvider`（AlgorithmId `"SM2-GM"`，extra field 0x4D54）为每条目生成随机数据密钥、以收件人 SM2 公钥封装——**无口令写路径**（`IPasswordlessWriteProvider`，私钥经口令管道下发），`ArchiveWriteOptions.RecipientPublicKey` 直接指定公钥即可加密
- **可插拔 KDF 注册表**: 新增 `IKdfProvider` 抽象与 `ArchiveWriteOptions.KdfAlgorithmId` 开关，内置 SM3-PBKDF2（SpecId 1，默认）与 PBKDF2-SHA256（SpecId 2）；SM4-GM 容器 extra field 携带 SpecId，旧包读取兼容
- **新格式**: CPIO（newc/odc/binary 读、newc 写，纯托管）、ISO9660（只读，+Joliet/Rock Ridge）、CAB（只读，NONE 存储文件夹；MSZIP/LZX 明确不支持）、Brotli（读写，原生核心）
- **分卷 ZIP**: `ArchiveWriteOptions.VolumeSize` 按字节上限切分 `.001/.002/…` 分卷写出；打开 `.001` 自动拼接后续分卷
- **Unix 属性与符号链接**: 新增 `IUnixEntry`（ZIP Unix host / ustar mode）与符号链接写入（ZIP `S_IFLNK` / TAR typeflag 2），配 `SymlinkPolicy`（默认 Skip，安全优先）与 `PreserveUnixAttributes` 解压保留 mode 位
- **命令行工具**: 新增 `GeneralZip.Cli`（dotnet global tool，命令名 `gz`）——`a` 压缩 / `x` 解压 / `t` 完整性测试 / `l` 列表 / `c` 转换 / `rm` 删除条目 / `i` 信息 / `detect` 格式探测 / `keygen` SM2 密钥对生成
- **发布矩阵扩展**: 原生核心构建矩阵从 5 个 RID 扩展到 8 个（新增 linux-arm64 / win-arm64 / win-x86）
- **健壮性**: 新增 SharpFuzz 模糊测试 harness 与 `RobustnessTests` 回归（随机/截断/位翻转输入永不崩溃读取器）；互操作 fixtures 由独立实现生成（py7zr、pycdlib、python zipfile、GNU cpio newc）作为读侧兼容锚点
- **修复**: `NativeTempDecodeStream` 不再在构造时耗尽并关闭调用方来源流（修复非可寻址流 TAR.BZ2 往返 `ObjectDisposedException`）；转码回调异常经 `ExceptionDispatchInfo` 重抛，保留原始堆栈
- **修复**: 无 UTF-8 标志的 ZIP 条目名/注释按 APPNOTE 约定回退 **IBM437** 解码——此前实现误回退 UTF-8，旧包 GBK/CP437 文件名会解出 U+FFFD 乱码；采用内置 256 项 CP437 单字节表，零第三方依赖、保持 AOT 兼容
- **文档**: 组件参考文档（中/英）同步更新格式矩阵、加密算法指南与功能列表，并新增本发布日志页

### 📍2026-11-02 — v0.0.1-beta（首个公开预览）

- **单包架构**: `GeneralZip.Native` 并入 `GeneralZip` 主包，捆绑 win-x64 / linux-x64 / linux-musl-x64 / osx-x64 / osx-arm64 五个平台的预编译 Rust 原生核心，首次使用自动加载、缺失时优雅降级
- **跨格式压缩引擎**: ZIP、7z、RAR、TAR、GZ、BZ2、XZ、Zstd、LZ4 统一 API 与魔数自动探测；ZipCrypto / WinZip AES / SM4-GM 三种加密方案，SM3/SM4/TAR/CRC32 纯 C# 实现（GB/T 标准向量锁定）
- **工程化**: CI + Release 流水线（RID 构建矩阵、NuGet/snupkg 发布冒烟测试）、运行时消息全量英文化、fixtures 跨平台确定性锚点、README 按 OSS 惯例重写、73 个测试（标准向量 + 互操作 oracle + 事件 + fixtures）
