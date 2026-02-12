using GeneralUpdate.Extension.Common.DTOs;
using GeneralUpdate.Extension.Common.Enums;
using GeneralUpdate.Extension.Common.Models;
using GeneralUpdate.Extension.Core;
using Newtonsoft.Json;
using System.IO.Compression;

try
{
    Console.WriteLine($"=== GeneralUpdate.Extension 示例程序 ===");
    Console.WriteLine($"=== GeneralUpdate.Extension Sample Program ===\n");
    Console.WriteLine($"初始化时间：{DateTime.Now}");
    Console.WriteLine($"Initialization Time: {DateTime.Now}\n");

    // ========================================
    // 1. 初始化扩展主机 / Initialize Extension Host
    // ========================================
    Console.WriteLine("=== 1. 初始化扩展主机 / Initialize Extension Host ===");
    
    var extensionsDir = Path.Combine(Directory.GetCurrentDirectory(), "extensions");
    Directory.CreateDirectory(extensionsDir);
    
    var options = new ExtensionHostOptions
    {
        ServerUrl = "http://127.0.0.1:7391/Extension",
        // Scheme = "Bearer",
        // Token = "your-token-here",
        HostVersion = "1.0.0",
        ExtensionsDirectory = extensionsDir,
        CatalogPath = Path.Combine(extensionsDir, "catalog.json")
    };
    
    Console.WriteLine("主机配置 / Host Configuration:");
    Console.WriteLine($"  服务器地址 / Server URL: {options.ServerUrl}");
    Console.WriteLine($"  主机版本 / Host Version: {options.HostVersion}");
    Console.WriteLine($"  扩展目录 / Extensions Directory: {options.ExtensionsDirectory}");
    Console.WriteLine($"  目录文件 / Catalog Path: {options.CatalogPath}");
    
    var host = new GeneralExtensionHost(options);
    Console.WriteLine("\n✓ 扩展主机创建成功 / Extension host created successfully");
    Console.WriteLine("\n" + new string('=', 80) + "\n");

    // ========================================
    // 2. 订阅扩展更新事件 / Subscribe to Extension Update Events
    // ========================================
    Console.WriteLine("=== 2. 订阅扩展更新事件 / Subscribe to Extension Update Events ===");
    
    host.ExtensionUpdateStatusChanged += (sender, e) =>
    {
        Console.WriteLine($"[事件/Event] 扩展/Extension: {e.ExtensionName ?? e.ExtensionId}");
        Console.WriteLine($"            状态/Status: {e.Status}");
        if (e.Status == ExtensionUpdateStatus.Updating)
        {
            Console.WriteLine($"            进度/Progress: {e.Progress}%");
        }
        if (e.Status == ExtensionUpdateStatus.UpdateFailed)
        {
            Console.WriteLine($"            错误/Error: {e.ErrorMessage}");
        }
        Console.WriteLine();
    };
    
    Console.WriteLine("✓ 事件订阅成功 / Event subscription successful");
    Console.WriteLine("\n" + new string('=', 80) + "\n");

    // ========================================
    // 3. 创建示例扩展包 / Create Sample Extension Package
    // ========================================
    Console.WriteLine("=== 3. 创建示例扩展包 / Create Sample Extension Package ===");
    
    var extensionId = Guid.NewGuid().ToString();
    var extensionMetadata = new ExtensionMetadata
    {
        Id = extensionId,
        Name = "sample-extension",
        DisplayName = "示例扩展 / Sample Extension",
        Version = "1.0.0",
        Description = "这是一个演示用的示例扩展 / This is a sample extension for demonstration",
        Publisher = "GeneralUpdate Team",
        Format = ".zip",
        FileSize = 0,
        Hash = "sample-hash-sha256",
        Status = true,
        SupportedPlatforms = TargetPlatform.All,
        MinHostVersion = "1.0.0",
        MaxHostVersion = "2.0.0",
        IsPreRelease = false,
        ReleaseDate = DateTime.UtcNow,
        UploadTime = DateTime.UtcNow,
        DownloadUrl = "http://127.0.0.1:7391/Extension/download",
        Categories = "Tools,Development",
        License = "MIT",
        Dependencies = null
    };
    
    Console.WriteLine("扩展元数据 / Extension Metadata:");
    Console.WriteLine($"  ID: {extensionMetadata.Id}");
    Console.WriteLine($"  名称 / Name: {extensionMetadata.Name}");
    Console.WriteLine($"  显示名称 / Display Name: {extensionMetadata.DisplayName}");
    Console.WriteLine($"  版本 / Version: {extensionMetadata.Version}");
    Console.WriteLine($"  发布者 / Publisher: {extensionMetadata.Publisher}");
    Console.WriteLine($"  支持平台 / Supported Platforms: {extensionMetadata.SupportedPlatforms}");
    Console.WriteLine($"  最小主机版本 / Min Host Version: {extensionMetadata.MinHostVersion}");
    Console.WriteLine($"  最大主机版本 / Max Host Version: {extensionMetadata.MaxHostVersion}");
    
    // 创建扩展包 / Create extension package
    var zipFileName = $"{extensionMetadata.Name}_{extensionMetadata.Version}.zip";
    var zipPath = Path.Combine(extensionsDir, zipFileName);
    
    await CreateExtensionPackage(zipPath, extensionMetadata);
    
    var fileInfo = new FileInfo(zipPath);
    extensionMetadata.FileSize = fileInfo.Length;
    
    Console.WriteLine($"\n✓ 扩展包已创建 / Extension package created: {zipPath}");
    Console.WriteLine($"  文件大小 / File Size: {extensionMetadata.FileSize} bytes ({extensionMetadata.FileSize / 1024.0:F2} KB)");
    Console.WriteLine("\n" + new string('=', 80) + "\n");

    // ========================================
    // 4. 安装扩展 / Install Extension
    // ========================================
    Console.WriteLine("=== 4. 安装扩展 / Install Extension ===");
    Console.WriteLine($"扩展路径 / Extension Path: {zipPath}");
    Console.WriteLine($"启用回滚 / Rollback Enabled: true\n");
    
    var installSuccess = await host.InstallExtensionAsync(zipPath, rollbackOnFailure: true);
    
    if (installSuccess)
    {
        Console.WriteLine("✓ 扩展安装成功 / Extension installed successfully");
        
        var extractedDir = Path.Combine(extensionsDir, extensionMetadata.Name);
        if (Directory.Exists(extractedDir))
        {
            Console.WriteLine($"  安装目录 / Installation Directory: {extractedDir}");
            var extractedFiles = Directory.GetFiles(extractedDir);
            Console.WriteLine($"  提取的文件 / Extracted Files ({extractedFiles.Length}):");
            foreach (var file in extractedFiles)
            {
                Console.WriteLine($"    • {Path.GetFileName(file)}");
            }
        }
    }
    else
    {
        Console.WriteLine("✗ 扩展安装失败 / Extension installation failed");
    }
    Console.WriteLine("\n" + new string('=', 80) + "\n");

    // ========================================
    // 5. 更新扩展目录 / Update Extension Catalog
    // ========================================
    Console.WriteLine("=== 5. 更新扩展目录 / Update Extension Catalog ===");
    
    host.ExtensionCatalog.AddOrUpdateInstalledExtension(extensionMetadata);
    Console.WriteLine("✓ 扩展已添加到目录 / Extension added to catalog");
    Console.WriteLine("  (目录会自动保存 / Catalog is saved automatically)");
    Console.WriteLine("\n" + new string('=', 80) + "\n");

    // ========================================
    // 6. 管理已安装的扩展 / Manage Installed Extensions
    // ========================================
    Console.WriteLine("=== 6. 管理已安装的扩展 / Manage Installed Extensions ===");
    
    // 加载已安装的扩展 / Load installed extensions
    host.ExtensionCatalog.LoadInstalledExtensions();
    Console.WriteLine("✓ 已加载已安装的扩展 / Installed extensions loaded");
    
    // 获取所有已安装的扩展 / Get all installed extensions
    var installedExtensions = host.ExtensionCatalog.GetInstalledExtensions();
    Console.WriteLine($"\n已安装扩展数量 / Total Installed Extensions: {installedExtensions.Count}");
    
    if (installedExtensions.Count > 0)
    {
        Console.WriteLine("\n已安装扩展列表 / Installed Extensions List:");
        foreach (var ext in installedExtensions)
        {
            Console.WriteLine($"\n  • {ext.DisplayName} v{ext.Version}");
            Console.WriteLine($"    ID: {ext.Id}");
            Console.WriteLine($"    名称 / Name: {ext.Name}");
            Console.WriteLine($"    状态 / Status: {(ext.Status == true ? "启用/Enabled" : "禁用/Disabled")}");
            Console.WriteLine($"    平台 / Platform: {ext.SupportedPlatforms}");
            Console.WriteLine($"    发布者 / Publisher: {ext.Publisher}");
            Console.WriteLine($"    描述 / Description: {ext.Description}");
        }
    }
    
    // 根据ID获取特定扩展 / Get specific extension by ID
    Console.WriteLine($"\n--- 根据ID获取扩展 / Get Extension by ID ---");
    var extension = host.ExtensionCatalog.GetInstalledExtensionById(extensionId);
    if (extension != null)
    {
        Console.WriteLine($"✓ 找到扩展 / Found extension:");
        Console.WriteLine($"  ID: {extension.Id}");
        Console.WriteLine($"  名称 / Name: {extension.DisplayName}");
        Console.WriteLine($"  版本 / Version: {extension.Version}");
    }
    else
    {
        Console.WriteLine($"✗ 未找到扩展 / Extension not found");
    }
    
    // 根据平台获取扩展 / Get extensions by platform
    Console.WriteLine($"\n--- 根据平台获取扩展 / Get Extensions by Platform ---");
    var windowsExtensions = host.ExtensionCatalog.GetInstalledExtensionsByPlatform(TargetPlatform.Windows);
    Console.WriteLine($"Windows 平台扩展数量 / Windows Platform Extensions: {windowsExtensions.Count}");
    
    var allPlatformExtensions = host.ExtensionCatalog.GetInstalledExtensionsByPlatform(TargetPlatform.All);
    Console.WriteLine($"所有平台扩展数量 / All Platform Extensions: {allPlatformExtensions.Count}");
    Console.WriteLine("\n" + new string('=', 80) + "\n");

    // ========================================
    // 7. 检查扩展兼容性 / Check Extension Compatibility
    // ========================================
    Console.WriteLine("=== 7. 检查扩展兼容性 / Check Extension Compatibility ===");
    
    if (extension != null)
    {
        var isCompatible = host.IsExtensionCompatible(extension);
        Console.WriteLine($"扩展名称 / Extension Name: {extension.DisplayName}");
        Console.WriteLine($"扩展版本 / Extension Version: {extension.Version}");
        Console.WriteLine($"最小主机版本 / Min Host Version: {extension.MinHostVersion}");
        Console.WriteLine($"最大主机版本 / Max Host Version: {extension.MaxHostVersion}");
        Console.WriteLine($"当前主机版本 / Current Host Version: {options.HostVersion}");
        Console.WriteLine($"兼容性 / Compatibility: {(isCompatible ? "✓ 兼容/Compatible" : "✗ 不兼容/Not Compatible")}");
    }
    Console.WriteLine("\n" + new string('=', 80) + "\n");

    // ========================================
    // 8. 自动更新设置 / Auto-Update Settings
    // ========================================
    Console.WriteLine("=== 8. 自动更新设置 / Auto-Update Settings ===");
    
    if (extension != null)
    {
        // 启用自动更新 / Enable auto-update
        host.SetAutoUpdate(extensionId, true);
        Console.WriteLine($"✓ 已为扩展启用自动更新 / Auto-update enabled for extension: {extension.DisplayName}");
        
        // 检查是否启用自动更新 / Check if auto-update is enabled
        var autoUpdateEnabled = host.IsAutoUpdateEnabled(extensionId);
        Console.WriteLine($"  自动更新状态 / Auto-Update Status: {(autoUpdateEnabled ? "启用/Enabled" : "禁用/Disabled")}");
        
        // 启用全局自动更新 / Enable global auto-update
        host.SetGlobalAutoUpdate(true);
        Console.WriteLine($"\n✓ 已启用全局自动更新 / Global auto-update enabled");
    }
    Console.WriteLine("\n" + new string('=', 80) + "\n");

    // ========================================
    // 9. 查询远程扩展 / Query Remote Extensions
    // ========================================
    Console.WriteLine("=== 9. 查询远程扩展 / Query Remote Extensions ===");
    Console.WriteLine("注意：此功能需要服务器运行，本示例仅演示API调用");
    Console.WriteLine("Note: This feature requires a running server, this example only demonstrates API usage\n");
    
    var query = new ExtensionQueryDTO
    {
        Name = "sample",
        Platform = TargetPlatform.All,
        HostVersion = "1.0.0",
        Status = true,
        PageNumber = 1,
        PageSize = 20,
        BeginDate = DateTime.Now.AddDays(-30),
        EndDate = DateTime.Now
    };
    
    Console.WriteLine("查询参数 / Query Parameters:");
    Console.WriteLine($"  名称 / Name: {query.Name}");
    Console.WriteLine($"  平台 / Platform: {query.Platform}");
    Console.WriteLine($"  主机版本 / Host Version: {query.HostVersion}");
    Console.WriteLine($"  页码 / Page Number: {query.PageNumber}");
    Console.WriteLine($"  每页大小 / Page Size: {query.PageSize}");
    
    try
    {
        var queryResult = await host.QueryExtensionsAsync(query);
        if (queryResult.Body != null)
        {
            Console.WriteLine($"\n✓ 查询成功 / Query successful");
            Console.WriteLine($"  总数 / Total Count: {queryResult.Body.TotalCount}");
            Console.WriteLine($"  当前页 / Current Page: {queryResult.Body.PageNumber}");
            Console.WriteLine($"  每页大小 / Page Size: {queryResult.Body.PageSize}");
            
            var itemsList = queryResult.Body.Items.ToList();
            if (itemsList.Count > 0)
            {
                Console.WriteLine($"\n  找到的扩展 / Found Extensions:");
                foreach (var ext in itemsList)
                {
                    Console.WriteLine($"\n    • {ext.DisplayName} v{ext.Version}");
                    Console.WriteLine($"      ID: {ext.Id}");
                    Console.WriteLine($"      发布者 / Publisher: {ext.Publisher}");
                    Console.WriteLine($"      兼容 / Compatible: {ext.IsCompatible}");
                    Console.WriteLine($"      平台 / Platform: {ext.SupportedPlatforms}");
                }
            }
        }
        else
        {
            Console.WriteLine($"\n✗ 查询失败 / Query failed: {queryResult.Message}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n✗ 查询错误 / Query error: {ex.Message}");
        Console.WriteLine($"   (这是预期的，因为服务器未运行)");
        Console.WriteLine($"   (This is expected as the server is not running)");
    }
    Console.WriteLine("\n" + new string('=', 80) + "\n");

    // ========================================
    // 10. 清理 / Cleanup
    // ========================================
    Console.WriteLine("=== 10. 清理 / Cleanup ===");
    Console.WriteLine("注意：示例创建的文件保留在extensions目录中供检查");
    Console.WriteLine("Note: Files created by this example are kept in the extensions directory for inspection");
    Console.WriteLine($"扩展目录 / Extensions Directory: {extensionsDir}");
    
    Console.WriteLine("\n=== 示例程序执行完成 / Sample Program Completed ===");
    Console.WriteLine($"完成时间：{DateTime.Now}");
    Console.WriteLine($"Completion Time: {DateTime.Now}");
}
catch (Exception e)
{
    Console.WriteLine($"\n异常 / Exception: {e.Message}");
    Console.WriteLine($"堆栈跟踪 / Stack Trace:\n{e.StackTrace}");
}

// 辅助方法：创建扩展包 / Helper method: Create extension package
static async Task CreateExtensionPackage(string zipPath, ExtensionMetadata metadata)
{
    await Task.Run(() =>
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"extension-temp-{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);

        try
        {
            // 1. 创建 extension.json 元数据文件 / Create extension.json metadata file
            var metadataJson = JsonConvert.SerializeObject(metadata, Formatting.Indented);
            File.WriteAllText(Path.Combine(tempDir, "extension.json"), metadataJson);

            // 2. 创建模拟的 extension.dll 文件 / Create mock extension.dll file
            var dllContent = $@"// Mock DLL content
// 模拟的DLL内容
// In production, this would be the actual compiled extension binary
// 在生产环境中，这将是实际编译的扩展二进制文件
namespace {metadata.Name}
{{
    public class ExtensionEntry
    {{
        public void Initialize()
        {{
            // Extension initialization code
            // 扩展初始化代码
        }}
    }}
}}";
            File.WriteAllText(Path.Combine(tempDir, "extension.dll"), dllContent);

            // 3. 创建 README.md
            var readmeContent = $@"# {metadata.DisplayName}

## 版本 / Version: {metadata.Version}

### 描述 / Description
{metadata.Description}

### 发布者 / Publisher
{metadata.Publisher}

### 许可证 / License
{metadata.License}

### 支持的平台 / Supported Platforms
{metadata.SupportedPlatforms}

### 兼容性 / Compatibility
- 最小主机版本 / Min Host Version: {metadata.MinHostVersion}
- 最大主机版本 / Max Host Version: {metadata.MaxHostVersion}

### 分类 / Categories
{metadata.Categories}

---

## 安装 / Installation
此扩展包与 GeneralUpdate.Extension 主机兼容。
This extension package is compatible with GeneralUpdate.Extension host.

生成时间 / Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss UTC}
";
            File.WriteAllText(Path.Combine(tempDir, "README.md"), readmeContent);

            // 4. 创建压缩包 / Create zip file
            if (File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }

            ZipFile.CreateFromDirectory(tempDir, zipPath);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    });
}
