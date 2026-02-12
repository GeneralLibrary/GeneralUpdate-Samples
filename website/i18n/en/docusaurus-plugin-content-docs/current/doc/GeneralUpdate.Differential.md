---
sidebar_position: 6
---

# GeneralUpdate.Differential

## Component Overview

**GeneralUpdate.Differential** is the core component responsible for binary differential updates in the GeneralUpdate framework. This component provides powerful differential algorithms that can accurately identify file changes between two versions, generate efficient incremental patch packages, and support patch restoration operations. By using differential updates, you can significantly reduce update package size and download time, making it particularly suitable for scenarios with frequent update releases.

**Namespace:** `GeneralUpdate.Differential`  
**Assembly:** `GeneralUpdate.Core.dll`

```csharp
public sealed class DifferentialCore
```

---

## Core Features

### 1. Incremental Identification
- Accurately identify added, modified, and deleted files
- Intelligent file version comparison
- Support skipping specified files and formats

### 2. Binary Patch Generation
- Efficient binary differential algorithm
- Minimize patch file size
- Fast patch generation speed

### 3. Patch Restoration
- Safe patch application process
- Automatic handling of file dependencies
- Integrity verification mechanism

### 4. Blacklist Support
- File-level blacklist
- Format-level blacklist
- Flexible filtering rules

---

## Quick Start

### Installation

Install GeneralUpdate.Differential via NuGet (included in Core package):

```bash
dotnet add package GeneralUpdate.Core
```

### Initialization and Usage

The following example demonstrates how to use DifferentialCore for incremental identification and patch operations:

```csharp
using GeneralUpdate.Differential;

// Identify increments and generate binary patches
var sourcePath = @"D:\packet\app";        // Old version path
var targetPath = @"D:\packet\release";    // New version path
var patchPath = @"D:\packet\patch";       // Patch output path

await DifferentialCore.Instance?.Clean(sourcePath, targetPath, patchPath);

// Apply patches (restoration)
await DifferentialCore.Instance?.Dirty(sourcePath, patchPath);
```

---

## Core API Reference

### DifferentialCore Class

#### Instance Property

Get the singleton instance of DifferentialCore.

```csharp
public static DifferentialCore Instance { get; }
```

#### Clean Method

Perform incremental identification, deleted file identification, and generate binary patch files.

**Method Signature:**

```csharp
public async Task Clean(string sourcePath, string targetPath, string patchPath = null)
```

**Parameters:**
- `sourcePath`: Path to the old version folder
- `targetPath`: Path to the new version folder
- `patchPath`: Directory to store discovered incremental update files (optional)

**Function Description:**
1. Compare sourcePath and targetPath directories
2. Identify added, modified, and deleted files
3. Generate binary differential patches for modified files
4. Save patches and added files to patchPath

**Example:**
```csharp
// Generate patch package from v1.0.0 to v1.1.0
var source = @"D:\MyApp\v1.0.0";
var target = @"D:\MyApp\v1.1.0";
var patch = @"D:\MyApp\patches\v1.1.0";

await DifferentialCore.Instance.Clean(source, target, patch);
// Result: patch directory contains all necessary incremental update files
```

#### Dirty Method

Apply patches to update old version files to the new version.

**Method Signature:**

```csharp
public async Task Dirty(string appPath, string patchPath)
```

**Parameters:**
- `appPath`: Client application directory (current version)
- `patchPath`: Path to the patch files

**Function Description:**
1. Read patch files from patchPath
2. Apply patches to corresponding files in appPath
3. Handle copying of added files
4. Handle removal of deleted files

**Example:**
```csharp
// Apply patches to current application
var appDir = AppDomain.CurrentDomain.BaseDirectory;
var patchDir = Path.Combine(appDir, "temp", "patches");

await DifferentialCore.Instance.Dirty(appDir, patchDir);
// Result: application updated to new version
```

---

## Practical Usage Examples

### Example 1: Basic Patch Generation

```csharp
using GeneralUpdate.Differential;

public async Task GeneratePatchAsync()
{
    try
    {
        // Version paths
        var oldVersion = @"D:\MyApp\1.0.0";
        var newVersion = @"D:\MyApp\1.0.1";
        var patchOutput = @"D:\MyApp\patches\1.0.1";
        
        Console.WriteLine("Starting patch generation...");
        
        // Generate patches
        await DifferentialCore.Instance.Clean(oldVersion, newVersion, patchOutput);
        
        Console.WriteLine($"Patch generation complete! Output directory: {patchOutput}");
        
        // Display patch information
        var patchFiles = Directory.GetFiles(patchOutput, "*.*", SearchOption.AllDirectories);
        Console.WriteLine($"Generated {patchFiles.Length} patch files");
        
        long totalSize = patchFiles.Sum(f => new FileInfo(f).Length);
        Console.WriteLine($"Total patch size: {totalSize / 1024.0:F2} KB");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Patch generation failed: {ex.Message}");
    }
}
```

### Example 2: Apply Patches

```csharp
using GeneralUpdate.Differential;

public async Task ApplyPatchAsync()
{
    try
    {
        // Application directory
        var appDirectory = @"D:\MyApp\current";
        // Patch directory
        var patchDirectory = @"D:\MyApp\patches\1.0.1";
        
        Console.WriteLine("Starting patch application...");
        
        // Verify patch exists
        if (!Directory.Exists(patchDirectory))
        {
            throw new DirectoryNotFoundException($"Patch directory not found: {patchDirectory}");
        }
        
        // Apply patches
        await DifferentialCore.Instance.Dirty(appDirectory, patchDirectory);
        
        Console.WriteLine("Patch applied successfully! Application updated to new version.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Patch application failed: {ex.Message}");
    }
}
```

### Example 3: Complete Patch Workflow

```csharp
using GeneralUpdate.Differential;
using System.IO.Compression;

public class PatchManager
{
    // Generate and package patches
    public async Task<string> CreatePatchPackageAsync(
        string oldVersionPath, 
        string newVersionPath, 
        string outputPath)
    {
        try
        {
            // 1. Generate patch files
            var tempPatchDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempPatchDir);
            
            Console.WriteLine($"Generating patches...");
            await DifferentialCore.Instance.Clean(oldVersionPath, newVersionPath, tempPatchDir);
            
            // 2. Compress patch files
            var patchZipPath = Path.Combine(outputPath, "patch_1.0.1.zip");
            Console.WriteLine($"Packaging patches...");
            
            if (File.Exists(patchZipPath))
                File.Delete(patchZipPath);
                
            ZipFile.CreateFromDirectory(tempPatchDir, patchZipPath, 
                CompressionLevel.Optimal, false);
            
            // 3. Clean up temp files
            Directory.Delete(tempPatchDir, true);
            
            var patchSize = new FileInfo(patchZipPath).Length;
            Console.WriteLine($"Patch package created successfully: {patchZipPath}");
            Console.WriteLine($"Patch package size: {patchSize / 1024.0:F2} KB");
            
            return patchZipPath;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Patch package creation failed: {ex.Message}");
            throw;
        }
    }
    
    // Extract and apply patches
    public async Task ApplyPatchPackageAsync(string appPath, string patchZipPath)
    {
        try
        {
            // 1. Extract patch package
            var tempExtractDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempExtractDir);
            
            Console.WriteLine($"Extracting patch package...");
            ZipFile.ExtractToDirectory(patchZipPath, tempExtractDir);
            
            // 2. Apply patches
            Console.WriteLine($"Applying patches...");
            await DifferentialCore.Instance.Dirty(appPath, tempExtractDir);
            
            // 3. Clean up temp files
            Directory.Delete(tempExtractDir, true);
            
            Console.WriteLine($"Patch applied successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Patch package application failed: {ex.Message}");
            throw;
        }
    }
}

// Usage example
var manager = new PatchManager();

// Create patch package
var patchZip = await manager.CreatePatchPackageAsync(
    @"D:\MyApp\1.0.0",
    @"D:\MyApp\1.0.1",
    @"D:\MyApp\releases"
);

// Apply patch package
await manager.ApplyPatchPackageAsync(
    @"D:\MyApp\current",
    patchZip
);
```

### Example 4: Patch Operations with Progress Display

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
            progress?.Report("Starting file difference scan...");
            
            // In actual scenarios, you can add progress reports before and after Clean
            await DifferentialCore.Instance.Clean(sourcePath, targetPath, patchPath);
            
            progress?.Report("Patch generation complete!");
            
            // Statistics
            var files = Directory.GetFiles(patchPath, "*.*", SearchOption.AllDirectories);
            progress?.Report($"Generated {files.Length} patch files");
        }
        catch (Exception ex)
        {
            progress?.Report($"Error: {ex.Message}");
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
            progress?.Report("Starting patch application...");
            
            await DifferentialCore.Instance.Dirty(appPath, patchPath);
            
            progress?.Report("Patch applied successfully!");
        }
        catch (Exception ex)
        {
            progress?.Report($"Error: {ex.Message}");
            throw;
        }
    }
}

// Usage example
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

## Notes and Warnings

### ⚠️ Important Notes

1. **File Name Restrictions**
   - Cannot include files with the same name but different extensions (e.g., file.txt and file.log)
   - Recommended to use unique file naming conventions

2. **Directory Structure**
   - The relative structure of source and target directories should remain consistent
   - Directory hierarchy is preserved during patch generation

3. **Disk Space**
   - Ensure sufficient disk space for storing patch files
   - Binary differential patches are usually smaller than complete files, but still require temporary space

4. **File Locking**
   - When applying patches, ensure target files are not locked by other processes
   - Recommended to apply patches after the application is closed

5. **Backup Recommendations**
   - Recommended to backup original files before applying patches
   - Can use ClientCore's BackUp option for automatic backup

### 💡 Best Practices

- **Version Management**: Maintain separate patch packages for each version for easier version tracking and rollback
- **Patch Validation**: Perform validation testing after generating patches to ensure they can be applied correctly
- **Incremental Updates**: Prioritize differential updates over full updates, which can save 50%-90% of download size
- **Error Handling**: Implement complete exception capture and error recovery mechanisms
- **Performance Optimization**: For large files, the performance advantage of the differential algorithm is more pronounced

### 🔍 How It Works

**Clean Method Workflow:**
1. Scan all files in source and target directories
2. Compare MD5 hash values of files to identify changes
3. For modified files, use binary differential algorithm to generate patches
4. For added files, copy directly to patch directory
5. Record list of deleted files

**Dirty Method Workflow:**
1. Read all files in patch directory
2. For patch files, apply to corresponding original files
3. For added files, copy directly to application directory
4. Remove corresponding files according to deletion list
5. Verify update integrity

---

## Applicable Platforms

| Product        | Version       |
| -------------- | ------------- |
| .NET               | 5, 6, 7, 8, 9     |
| .NET Framework     | 4.6.1             |
| .NET Standard      | 2.0               |
| .NET Core          | 2.0               |

---

## Related Resources

- **Sample Code**: [View GitHub Examples](https://github.com/GeneralLibrary/GeneralUpdate-Samples/tree/main/src/Diff)
- **Main Repository**: [GeneralUpdate Project](https://github.com/GeneralLibrary/GeneralUpdate)
- **Packaging Tool**: The GeneralUpdate.PacketTool project depends on this component for differential packaging
