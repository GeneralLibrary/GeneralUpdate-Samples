---
sidebar_position: 6
---

# GeneralUpdate.Differential

## 组件概览 | Component Overview

**GeneralUpdate.Differential** 是 GeneralUpdate 框架中负责二进制差异更新的核心组件。该组件提供了强大的差异算法，可以精确识别两个版本之间的文件变化，生成高效的增量补丁包，并支持补丁还原操作。通过使用差异更新，可以显著减少更新包的大小和下载时间，特别适合频繁发布更新的应用场景。

**GeneralUpdate.Differential** is the core component responsible for binary differential updates in the GeneralUpdate framework. This component provides powerful differential algorithms that can accurately identify file changes between two versions, generate efficient incremental patch packages, and support patch restoration operations. By using differential updates, you can significantly reduce update package size and download time, making it particularly suitable for scenarios with frequent update releases.

**命名空间 | Namespace:** `GeneralUpdate.Differential`  
**程序集 | Assembly:** `GeneralUpdate.Core.dll`

```csharp
public sealed class DifferentialCore
```

---

## 核心特性 | Core Features

### 1. 增量识别 | Incremental Identification
- 精确识别新增、修改、删除的文件 | Accurately identify added, modified, and deleted files
- 智能文件版本对比 | Intelligent file version comparison
- 支持跳过指定文件和格式 | Support skipping specified files and formats

### 2. 二进制补丁生成 | Binary Patch Generation
- 高效的二进制差异算法 | Efficient binary differential algorithm
- 最小化补丁文件大小 | Minimize patch file size
- 快速补丁生成速度 | Fast patch generation speed

### 3. 补丁还原 | Patch Restoration
- 安全的补丁应用流程 | Safe patch application process
- 自动处理文件依赖关系 | Automatic handling of file dependencies
- 完整性验证机制 | Integrity verification mechanism

### 4. 黑名单支持 | Blacklist Support
- 文件级黑名单 | File-level blacklist
- 格式级黑名单 | Format-level blacklist
- 灵活的过滤规则 | Flexible filtering rules

---

## 快速开始 | Quick Start

### 安装 | Installation

通过 NuGet 安装 GeneralUpdate.Differential（包含在 Core 包中）：

Install GeneralUpdate.Differential via NuGet (included in Core package):

```bash
dotnet add package GeneralUpdate.Core
```

### 初始化与使用 | Initialization and Usage

以下示例展示了如何使用 DifferentialCore 进行增量识别和补丁操作：

The following example demonstrates how to use DifferentialCore for incremental identification and patch operations:

```csharp
using GeneralUpdate.Differential;

// 增量识别并生成二进制补丁 | Identify increments and generate binary patches
var sourcePath = @"D:\packet\app";        // 旧版本路径 | Old version path
var targetPath = @"D:\packet\release";    // 新版本路径 | New version path
var patchPath = @"D:\packet\patch";       // 补丁输出路径 | Patch output path

await DifferentialCore.Instance?.Clean(sourcePath, targetPath, patchPath);

// 应用补丁（还原） | Apply patches (restoration)
await DifferentialCore.Instance?.Dirty(sourcePath, patchPath);
```

---

## 核心 API 参考 | Core API Reference

### DifferentialCore 类 | DifferentialCore Class

#### Instance 属性 | Instance Property

获取 DifferentialCore 的单例实例。

Get the singleton instance of DifferentialCore.

```csharp
public static DifferentialCore Instance { get; }
```

#### Clean 方法 | Clean Method

执行增量识别、删除文件识别，并生成二进制补丁文件。

Perform incremental identification, deleted file identification, and generate binary patch files.

**方法签名 | Method Signature:**

```csharp
public async Task Clean(string sourcePath, string targetPath, string patchPath = null)
```

**参数 | Parameters:**
- `sourcePath`: 旧版本文件夹路径 | Path to the old version folder
- `targetPath`: 新版本文件夹路径 | Path to the new version folder
- `patchPath`: 补丁文件输出目录（可选）| Directory to store discovered incremental update files (optional)

**功能说明 | Function Description:**
1. 对比 sourcePath 和 targetPath 两个目录
2. 识别新增、修改、删除的文件
3. 为修改的文件生成二进制差异补丁
4. 将补丁和新增文件保存到 patchPath

**示例 | Example:**
```csharp
// 生成从 v1.0.0 到 v1.1.0 的补丁包
var source = @"D:\MyApp\v1.0.0";
var target = @"D:\MyApp\v1.1.0";
var patch = @"D:\MyApp\patches\v1.1.0";

await DifferentialCore.Instance.Clean(source, target, patch);
// 结果：patch 目录包含所有必要的增量更新文件
```

#### Dirty 方法 | Dirty Method

应用补丁，将旧版本文件更新到新版本。

Apply patches to update old version files to the new version.

**方法签名 | Method Signature:**

```csharp
public async Task Dirty(string appPath, string patchPath)
```

**参数 | Parameters:**
- `appPath`: 客户端应用程序目录（当前版本）| Client application directory (current version)
- `patchPath`: 补丁文件路径 | Path to the patch files

**功能说明 | Function Description:**
1. 读取 patchPath 中的补丁文件
2. 将补丁应用到 appPath 中的对应文件
3. 处理新增文件的复制
4. 处理删除文件的移除

**示例 | Example:**
```csharp
// 将补丁应用到当前应用程序
var appDir = AppDomain.CurrentDomain.BaseDirectory;
var patchDir = Path.Combine(appDir, "temp", "patches");

await DifferentialCore.Instance.Dirty(appDir, patchDir);
// 结果：应用程序更新到新版本
```

---

## 实际使用示例 | Practical Usage Examples

### 示例 1：基本补丁生成 | Example 1: Basic Patch Generation

```csharp
using GeneralUpdate.Differential;

public async Task GeneratePatchAsync()
{
    try
    {
        // 版本路径 | Version paths
        var oldVersion = @"D:\MyApp\1.0.0";
        var newVersion = @"D:\MyApp\1.0.1";
        var patchOutput = @"D:\MyApp\patches\1.0.1";
        
        Console.WriteLine("开始生成补丁...");
        
        // 生成补丁 | Generate patches
        await DifferentialCore.Instance.Clean(oldVersion, newVersion, patchOutput);
        
        Console.WriteLine($"补丁生成完成！输出目录：{patchOutput}");
        
        // 显示补丁信息 | Display patch information
        var patchFiles = Directory.GetFiles(patchOutput, "*.*", SearchOption.AllDirectories);
        Console.WriteLine($"生成了 {patchFiles.Length} 个补丁文件");
        
        long totalSize = patchFiles.Sum(f => new FileInfo(f).Length);
        Console.WriteLine($"总补丁大小：{totalSize / 1024.0:F2} KB");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"补丁生成失败：{ex.Message}");
    }
}
```

### 示例 2：应用补丁 | Example 2: Apply Patches

```csharp
using GeneralUpdate.Differential;

public async Task ApplyPatchAsync()
{
    try
    {
        // 应用程序目录 | Application directory
        var appDirectory = @"D:\MyApp\current";
        // 补丁目录 | Patch directory
        var patchDirectory = @"D:\MyApp\patches\1.0.1";
        
        Console.WriteLine("开始应用补丁...");
        
        // 验证补丁存在 | Verify patch exists
        if (!Directory.Exists(patchDirectory))
        {
            throw new DirectoryNotFoundException($"补丁目录不存在：{patchDirectory}");
        }
        
        // 应用补丁 | Apply patches
        await DifferentialCore.Instance.Dirty(appDirectory, patchDirectory);
        
        Console.WriteLine("补丁应用成功！应用程序已更新到新版本。");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"补丁应用失败：{ex.Message}");
    }
}
```

### 示例 3：完整的补丁流程 | Example 3: Complete Patch Workflow

```csharp
using GeneralUpdate.Differential;
using System.IO.Compression;

public class PatchManager
{
    // 生成并打包补丁 | Generate and package patches
    public async Task<string> CreatePatchPackageAsync(
        string oldVersionPath, 
        string newVersionPath, 
        string outputPath)
    {
        try
        {
            // 1. 生成补丁文件 | Generate patch files
            var tempPatchDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempPatchDir);
            
            Console.WriteLine($"正在生成补丁...");
            await DifferentialCore.Instance.Clean(oldVersionPath, newVersionPath, tempPatchDir);
            
            // 2. 压缩补丁文件 | Compress patch files
            var patchZipPath = Path.Combine(outputPath, "patch_1.0.1.zip");
            Console.WriteLine($"正在打包补丁...");
            
            if (File.Exists(patchZipPath))
                File.Delete(patchZipPath);
                
            ZipFile.CreateFromDirectory(tempPatchDir, patchZipPath, 
                CompressionLevel.Optimal, false);
            
            // 3. 清理临时文件 | Clean up temp files
            Directory.Delete(tempPatchDir, true);
            
            var patchSize = new FileInfo(patchZipPath).Length;
            Console.WriteLine($"补丁包创建成功：{patchZipPath}");
            Console.WriteLine($"补丁包大小：{patchSize / 1024.0:F2} KB");
            
            return patchZipPath;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"创建补丁包失败：{ex.Message}");
            throw;
        }
    }
    
    // 解压并应用补丁 | Extract and apply patches
    public async Task ApplyPatchPackageAsync(string appPath, string patchZipPath)
    {
        try
        {
            // 1. 解压补丁包 | Extract patch package
            var tempExtractDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempExtractDir);
            
            Console.WriteLine($"正在解压补丁包...");
            ZipFile.ExtractToDirectory(patchZipPath, tempExtractDir);
            
            // 2. 应用补丁 | Apply patches
            Console.WriteLine($"正在应用补丁...");
            await DifferentialCore.Instance.Dirty(appPath, tempExtractDir);
            
            // 3. 清理临时文件 | Clean up temp files
            Directory.Delete(tempExtractDir, true);
            
            Console.WriteLine($"补丁应用成功！");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"应用补丁包失败：{ex.Message}");
            throw;
        }
    }
}

// 使用示例 | Usage example
var manager = new PatchManager();

// 创建补丁包 | Create patch package
var patchZip = await manager.CreatePatchPackageAsync(
    @"D:\MyApp\1.0.0",
    @"D:\MyApp\1.0.1",
    @"D:\MyApp\releases"
);

// 应用补丁包 | Apply patch package
await manager.ApplyPatchPackageAsync(
    @"D:\MyApp\current",
    patchZip
);
```

### 示例 4：带进度显示的补丁操作 | Example 4: Patch Operations with Progress Display

```csharp
using GeneralUpdate.Differential;

public class ProgressivePatchManager
{
    public async Task GeneratePatchWithProgressAsync(
        string sourcePath, 
        string targetPath, 
        string patchPath,
        IProgress<string> progress)
    {
        try
        {
            progress?.Report("开始扫描文件差异...");
            
            // 在实际场景中，可以在 Clean 前后添加进度报告
            await DifferentialCore.Instance.Clean(sourcePath, targetPath, patchPath);
            
            progress?.Report("补丁生成完成！");
            
            // 统计信息 | Statistics
            var files = Directory.GetFiles(patchPath, "*.*", SearchOption.AllDirectories);
            progress?.Report($"共生成 {files.Length} 个补丁文件");
        }
        catch (Exception ex)
        {
            progress?.Report($"错误：{ex.Message}");
            throw;
        }
    }
    
    public async Task ApplyPatchWithProgressAsync(
        string appPath, 
        string patchPath,
        IProgress<string> progress)
    {
        try
        {
            progress?.Report("开始应用补丁...");
            
            await DifferentialCore.Instance.Dirty(appPath, patchPath);
            
            progress?.Report("补丁应用成功！");
        }
        catch (Exception ex)
        {
            progress?.Report($"错误：{ex.Message}");
            throw;
        }
    }
}

// 使用示例 | Usage example
var manager = new ProgressivePatchManager();
var progress = new Progress<string>(msg => Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {msg}"));

await manager.GeneratePatchWithProgressAsync(
    @"D:\MyApp\1.0.0",
    @"D:\MyApp\1.0.1",
    @"D:\MyApp\patches\1.0.1",
    progress
);
```
---

## 注意事项与警告 | Notes and Warnings

### ⚠️ 重要提示 | Important Notes

1. **文件名限制 | File Name Restrictions**
   - 不能包含同名但扩展名不同的文件（如 file.txt 和 file.log）
   - 建议使用唯一的文件名命名规则
   - Cannot include files with the same name but different extensions (e.g., file.txt and file.log)
   - Recommended to use unique file naming conventions

2. **目录结构 | Directory Structure**
   - 源目录和目标目录的相对结构应保持一致
   - 补丁生成时会保留目录层次关系
   - The relative structure of source and target directories should remain consistent
   - Directory hierarchy is preserved during patch generation

3. **磁盘空间 | Disk Space**
   - 确保有足够的磁盘空间存储补丁文件
   - 二进制差异补丁通常比完整文件小，但仍需要临时空间
   - Ensure sufficient disk space for storing patch files
   - Binary differential patches are usually smaller than complete files, but still require temporary space

4. **文件占用 | File Locking**
   - 应用补丁时，确保目标文件没有被其他进程占用
   - 建议在应用程序关闭后应用补丁
   - When applying patches, ensure target files are not locked by other processes
   - Recommended to apply patches after the application is closed

5. **备份建议 | Backup Recommendations**
   - 在应用补丁前建议备份原始文件
   - 可以使用 ClientCore 的 BackUp 选项自动备份
   - Recommended to backup original files before applying patches
   - Can use ClientCore's BackUp option for automatic backup

### 💡 最佳实践 | Best Practices

- **版本管理**：为每个版本维护独立的补丁包，便于版本追踪和回滚
- **补丁验证**：生成补丁后进行验证测试，确保补丁可以正确应用
- **增量更新**：优先使用差异更新而非全量更新，可节省 50%-90% 的下载量
- **错误处理**：实现完整的异常捕获和错误恢复机制
- **性能优化**：对于大文件，差异算法的性能优势更加明显
- **Version Management**: Maintain separate patch packages for each version for easier version tracking and rollback
- **Patch Validation**: Perform validation testing after generating patches to ensure they can be applied correctly
- **Incremental Updates**: Prioritize differential updates over full updates, which can save 50%-90% of download size
- **Error Handling**: Implement complete exception capture and error recovery mechanisms
- **Performance Optimization**: For large files, the performance advantage of the differential algorithm is more pronounced

### 🔍 工作原理 | How It Works

**Clean 方法工作流程 | Clean Method Workflow:**
1. 扫描源目录和目标目录中的所有文件
2. 比较文件的MD5哈希值以识别变化
3. 对于修改的文件，使用二进制差异算法生成补丁
4. 对于新增文件，直接复制到补丁目录
5. 记录删除的文件列表

**Dirty 方法工作流程 | Dirty Method Workflow:**
1. 读取补丁目录中的所有文件
2. 对于补丁文件，应用到对应的原文件上
3. 对于新增文件，直接复制到应用目录
4. 根据删除列表移除相应文件
5. 验证更新完整性

---

## 适用平台 | Applicable Platforms

| 产品 Product        | 版本 Version       |
| ------------------ | ----------------- |
| .NET               | 5, 6, 7, 8, 9     |
| .NET Framework     | 4.6.1             |
| .NET Standard      | 2.0               |
| .NET Core          | 2.0               |

---

## 相关资源 | Related Resources

- **示例代码**：[查看 GitHub 示例](https://github.com/GeneralLibrary/GeneralUpdate-Samples/tree/main/src/Diff) | [View GitHub Examples](https://github.com/GeneralLibrary/GeneralUpdate-Samples/tree/main/src/Diff)
- **主仓库**：[GeneralUpdate 项目](https://github.com/GeneralLibrary/GeneralUpdate) | [GeneralUpdate Project](https://github.com/GeneralLibrary/GeneralUpdate)
- **打包工具**：GeneralUpdate.PacketTool 项目依赖此组件实现差异打包 | The GeneralUpdate.PacketTool project depends on this component for differential packaging
