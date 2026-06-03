---
sidebar_position: 3
---

# Pipeline 中间件管道

## 概述

GeneralUpdate 采用**中间件管道模式 (Middleware Pipeline Pattern)** 来处理更新包。每个下载完成的更新包都会依次经过管道中的各个中间件处理——从哈希校验到解压、差分包应用到驱动安装。

该模式的核心优势在于**关注点分离**和**可扩展性**：每个中间件只负责一个独立任务，开发者可以轻松添加自定义中间件。

---

## 管道架构

```
下载完成 (.zip)
      │
      ▼
┌─────────────────┐
│  HashMiddleware  │  ← SHA256 哈希校验
└────────┬────────┘
         │ 通过
         ▼
┌─────────────────┐
│CompressMiddleware│  ← 解压 ZIP 到目标路径
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ PatchMiddleware  │  ← 应用 BSDiff 差分包 (可选)
└────────┬────────┘
         │
         ▼
┌─────────────────────┐
│DrivelutionMiddleware│  ← 安装驱动程序 (可选)
└────────┬────────────┘
         │
         ▼
     升级完成
```

---

## 核心接口

### IMiddleware

所有中间件必须实现的基础接口：

```csharp
public interface IMiddleware
{
    /// <summary>
    /// 执行中间件逻辑
    /// </summary>
    /// <param name="context">管道上下文，携带当前管道的所有状态</param>
    Task InvokeAsync(PipelineContext context);
}
```

### PipelineBuilder

不可变管道构建器，按 FIFO（先进先出）顺序执行中间件：

```csharp
public class PipelineBuilder
{
    /// <summary>注册中间件</summary>
    public PipelineBuilder UseMiddleware<T>() where T : IMiddleware;

    /// <summary>条件注册中间件</summary>
    public PipelineBuilder UseMiddlewareIf<T>(bool? condition) where T : IMiddleware;

    /// <summary>构建并执行管道</summary>
    public async Task Build(PipelineContext context);
}
```

### PipelineContext

线程安全的键值存储，在中间件之间传递状态：

```csharp
public class PipelineContext
{
    // 预定义的上下文键
    public const string Hash = "Hash";               // 期望的 SHA256 哈希值
    public const string Format = "Format";           // 压缩格式
    public const string Encoding = "Encoding";       // 编码
    public const string ZipFilePath = "ZipFilePath"; // 下载的 ZIP 文件路径
    public const string SourcePath = "SourcePath";   // 解压目标路径
    public const string PatchPath = "PatchPath";     // 差分解压路径
    public const string PatchEnabled = "PatchEnabled"; // 是否启用差分包
    public const string DiffPipeline = "DiffPipeline"; // 差分管道实例
}
```

---

## 内置中间件详解

### 1. HashMiddleware — SHA256 哈希校验

**位置：** `GeneralUpdate.Core.Pipeline.HashMiddleware`

**职责：** 对下载完成的 ZIP 更新包执行 SHA256 哈希校验，确保文件完整性。

**处理流程：**
```
1. 从 PipelineContext 读取 "Hash" 和 "ZipFilePath"
2. 计算 ZIP 文件的 SHA256 哈希
3. 与期望的哈希值比对
4. 不匹配 → 抛出 CryptographicException
5. 匹配 → 传递给下一个中间件
```

**错误处理：**
- 哈希不匹配时抛出 `CryptographicException`
- 上游调用方（`AbstractStrategy`）会捕获并触发错误回调

---

### 2. CompressMiddleware — 解压处理

**位置：** `GeneralUpdate.Core.Pipeline.CompressMiddleware`

**职责：** 将 ZIP 格式的更新包解压到目标目录。

**处理流程：**
```
1. 从 PipelineContext 读取压缩格式和路径信息
2. 如果 PatchEnabled = true：
   → 解压到 PatchPath（后续由 PatchMiddleware 处理）
3. 如果 PatchEnabled = false：
   → 解压到 SourcePath（直接覆盖安装）
4. 使用 System.IO.Compression.ZipFile 解压
```

**路径策略：**

| PatchEnabled | 解压目标 | 后续处理 |
|-------------|----------|----------|
| `true` | `PatchPath`（临时目录） | PatchMiddleware 应用差分 |
| `false` | `SourcePath`（安装目录） | 直接完成，全量覆盖 |

---

### 3. PatchMiddleware — 差分包应用

**位置：** `GeneralUpdate.Core.Pipeline.PatchMiddleware`

**职责：** 将二进制差分补丁应用到当前安装的文件上。

**处理流程：**
```
1. 检查 PipelineContext 中是否存在 "DiffPipeline"
2. 如果不存在 → 跳过（全量更新模式）
3. 如果存在 →
   a. 调用 DiffPipeline.DirtyAsync() 应用补丁
   b. 处理 .patch 文件 → BSDiff/HdiffPatch 还原
   c. 复制新增文件到目标目录
   d. 根据 delete_files.json 删除过期文件
4. 报告差分进度（通过 DiffProgressReporter）
```

**差分包目录结构：**
```
PatchPath/
├── MyApp.Core.dll.patch     ← BSDiff 格式的二进制补丁
├── MyApp.UI.exe.patch       ← BSDiff 格式的二进制补丁
├── NewFeature.dll            ← 新增文件（非补丁，直接复制）
├── config/new_config.json   ← 新增文件
└── delete_files.json         ← 待删除文件清单
```

**delete_files.json 格式：**
```json
{
  "DeletedFiles": [
    "old_module.dll",
    "deprecated/old_feature.exe"
  ]
}
```

---

### 4. DrivelutionMiddleware — 驱动更新

**位置：** `GeneralUpdate.Core.Pipeline.DrivelutionMiddleware`

**职责：** 在文件更新完成后，处理驱动程序更新。

**触发条件：**
- `PipelineContext` 中 `DriverDirectory` 不为空
- 平台为 Windows 或 Linux

**处理流程：**
```
1. 扫描 DriverDirectory 中的驱动文件
2. 调用 GeneralDrivelution 执行驱动更新管道：
   Validate → Backup → Install → Verify
3. 错误时自动回滚
```

---

## 管道构建

### 默认管道构建

在 `AbstractStrategy` 的子类中构建：

```csharp
// WindowsStrategy / LinuxStrategy / MacStrategy 中的 BuildPipeline
protected override PipelineBuilder BuildPipeline(PipelineContext context)
{
    return new PipelineBuilder()
        .UseMiddleware<HashMiddleware>()        // 步骤 1：哈希校验
        .UseMiddleware<CompressMiddleware>()    // 步骤 2：解压
        .UseMiddlewareIf<PatchMiddleware>(      // 步骤 3：差分包（条件）
            context.Get<bool>(PipelineContext.PatchEnabled))
        .UseMiddlewareIf<DrivelutionMiddleware>(// 步骤 4：驱动（条件）
            !string.IsNullOrEmpty(context.Get<string>("DriverDirectory")));
}
```

### 自定义管道

开发者可以通过 `AbstractBootstrap` 的方法注入自定义中间件：

```csharp
await new GeneralUpdateBootstrap()
    .SetConfig(config)
    // 注入自定义下载管道处理
    .DownloadPipeline<CustomPipeline>()
    .LaunchAsync();
```

---

## 差分管道 (DiffPipeline)

差分管道是在 `PatchMiddleware` 内部运行的子管道，专门处理二进制差分算法。

### 核心接口

```csharp
public interface IBinaryDiffer
{
    /// <summary>应用补丁（客户端）：oldFile + patchFile → newFile</summary>
    Task DirtyAsync(string oldFilePath, string newFilePath, 
                    string patchFilePath, CancellationToken ct = default);

    /// <summary>生成补丁（服务端/工具）：oldFile + newFile → patchFile</summary>
    Task CleanAsync(string oldFilePath, string newFilePath, 
                    string patchFilePath, CancellationToken ct = default);
}
```

### 差分算法

| 算法 | 实现类 | 特点 |
|------|--------|------|
| **BSDiff 4.0** | `BsdiffDiffer` | O(n log n) 后缀数组，高压缩比 |
| **Streaming Hdiff** | `StreamingHdiffDiffer` | FNV-1a 哈希预过滤，支持大文件流式处理 |

### 压缩格式

差分补丁的元数据可以用不同算法压缩：

| 压缩器 | FormatVersion | 兼容性 |
|--------|---------------|--------|
| **BZip2** | `0x00` | 传统 BSDiff 兼容，所有平台 |
| **Deflate** | `0x01` | .NET BCL DeflateStream，更快 |
| **Brotli** | `0x02` | .NET 6+ BrotliStream，最高压缩比 |

### 差分匹配器

控制哪些文件需要差分处理：

```csharp
// 客户端（应用补丁）— 决定哪些文件需要差分
public interface IDirtyMatcher
{
    Task<bool> ShouldDirty(string relativeFilePath);
}

// 服务端（生成补丁）— 决定哪些文件需要生成差分
public interface ICleanMatcher
{
    Task<bool> ShouldClean(string relativeFilePath);
}
```

---

## 进度报告

### DiffProgressReporter

桥接差分管道进度到事件系统：

```csharp
public class DiffProgressReporter : IProgress<DiffProgress>
{
    public void Report(DiffProgress value)
    {
        EventManager.Instance.Dispatch(new ProgressEventArgs 
        { 
            Progress = new ProgressInfo { Diff = value }
        });
    }
}
```

客户端通过事件监听差分进度：

```csharp
await new GeneralClientBootstrap()
    .AddListenerProgress((sender, args) =>
    {
        if (args.Progress.Diff != null)
        {
            Console.WriteLine($"差分进度: {args.Progress.Diff.Percentage}%");
        }
    })
    .SetConfig(config)
    .LaunchAsync();
```

---

## 管道执行流程

### 完整执行代码路径

```
GeneralUpdateBootstrap.LaunchAsync()
  → IStrategy.ExecuteAsync()
    → AbstractStrategy.ExecuteAsync()
      → 遍历每个需要更新的版本
        → CreatePipelineContext(version)
        → BuildPipeline(context)
        → pipeline.Build(context)     ← 执行所有中间件
        → OnExecuteCompleteAsync()
      → 所有版本处理完成
    → StartAppAsync()
      → 启动新版本主程序
      → [可选] 启动 Bowl 监控
```

### 错误处理与回滚

```
中间件执行失败
  → AbstractStrategy.HandleExecuteException()
    → 触发 Exception 事件
    → TryRollback()
      → 从备份目录恢复文件
      → 清理临时文件
    → 上报失败状态 (ReportType.Failure)
```

---

## 自定义中间件示例

### 示例 1：文件扫描中间件

```csharp
public class FileScanMiddleware : IMiddleware
{
    public async Task InvokeAsync(PipelineContext context)
    {
        var sourcePath = context.Get<string>(PipelineContext.SourcePath);
        
        // 扫描更新后的文件
        var files = Directory.GetFiles(sourcePath, "*.*", 
            SearchOption.AllDirectories);
        
        Console.WriteLine($"已更新 {files.Length} 个文件");
        
        // 可在此添加病毒扫描、代码签名验证等逻辑
    }
}
```

### 示例 2：更新前后钩子中间件

```csharp
public class PrePostHookMiddleware : IMiddleware
{
    private readonly Action<string> _onBefore;
    private readonly Action<string> _onAfter;

    public PrePostHookMiddleware(Action<string> onBefore, Action<string> onAfter)
    {
        _onBefore = onBefore;
        _onAfter = onAfter;
    }

    public async Task InvokeAsync(PipelineContext context)
    {
        var version = context.Get<string>("Version");
        
        _onBefore?.Invoke(version);
        
        // 执行完毕后的处理
        _onAfter?.Invoke(version);
    }
}
```

---

## 相关资源

- **[系统架构](./Architecture.md)** — 了解整体架构与管道在其中的位置
- **[GeneralUpdate.Differential](../doc/GeneralUpdate.Differential)** — BSDiff 差分算法详解
- **[配置参考](./Configuration.md)** — 管道相关配置选项
