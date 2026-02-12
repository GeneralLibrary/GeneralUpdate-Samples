using GeneralUpdate.Drivelution;
using GeneralUpdate.Drivelution.Abstractions.Configuration;
using GeneralUpdate.Drivelution.Abstractions.Models;

try
{
    Console.WriteLine($"=== GeneralUpdate.Drivelution 示例程序 ===");
    Console.WriteLine($"=== GeneralUpdate.Drivelution Sample Program ===\n");
    Console.WriteLine($"初始化时间：{DateTime.Now}");
    Console.WriteLine($"Initialization Time: {DateTime.Now}\n");

    // ========================================
    // 1. 获取平台信息 / Get Platform Information
    // ========================================
    Console.WriteLine("=== 1. 获取平台信息 / Get Platform Information ===");
    var platformInfo = GeneralDrivelution.GetPlatformInfo();
    Console.WriteLine($"平台 / Platform: {platformInfo.Platform}");
    Console.WriteLine($"操作系统 / Operating System: {platformInfo.OperatingSystem}");
    Console.WriteLine($"架构 / Architecture: {platformInfo.Architecture}");
    Console.WriteLine($"系统版本 / System Version: {platformInfo.SystemVersion}");
    Console.WriteLine($"是否支持 / Is Supported: {(platformInfo.IsSupported ? "是/Yes" : "否/No")}");
    Console.WriteLine($"\n详细信息 / Full Info: {platformInfo}");
    Console.WriteLine("\n" + new string('=', 80) + "\n");

    // ========================================
    // 2. 创建驱动更新实例 / Create Driver Updater Instance
    // ========================================
    Console.WriteLine("=== 2. 创建驱动更新实例 / Create Driver Updater Instance ===");
    
    // 使用默认配置创建 / Create with default configuration
    var updater = GeneralDrivelution.Create();
    Console.WriteLine("✓ 默认实例创建成功 / Default instance created successfully");
    
    // 使用自定义配置创建 / Create with custom configuration
    var options = new DrivelutionOptions
    {
        DefaultBackupPath = "./driver_backups",
        DefaultRetryCount = 5,
        DefaultRetryIntervalSeconds = 3,
        AutoCleanupBackups = true,
        BackupsToKeep = 5
    };
    var customUpdater = GeneralDrivelution.Create(options);
    Console.WriteLine("✓ 自定义实例创建成功 / Custom instance created successfully");
    Console.WriteLine($"  备份路径 / Backup Path: {options.DefaultBackupPath}");
    Console.WriteLine($"  重试次数 / Retry Count: {options.DefaultRetryCount}");
    Console.WriteLine($"  自动清理备份 / Auto Cleanup Backups: {options.AutoCleanupBackups}");
    Console.WriteLine("\n" + new string('=', 80) + "\n");

    // ========================================
    // 3. 从目录读取驱动信息 / Get Drivers from Directory
    // ========================================
    Console.WriteLine("=== 3. 从目录读取驱动信息 / Get Drivers from Directory ===");
    
    // 创建测试驱动目录 / Create test driver directory
    var testDriverDir = Path.Combine(Path.GetTempPath(), "test_drivers");
    Directory.CreateDirectory(testDriverDir);
    Console.WriteLine($"测试目录 / Test Directory: {testDriverDir}");
    
    // 根据平台创建测试驱动文件 / Create test driver files based on platform
    if (platformInfo.Platform == "Windows")
    {
        // 创建模拟的 Windows INF 文件 / Create mock Windows INF file
        var infFile = Path.Combine(testDriverDir, "test_driver.inf");
        var infContent = @"
[Version]
Signature=""$Windows NT$""
Class=System
ClassGuid={4d36e97d-e325-11ce-bfc1-08002be10318}
Provider=%ManufacturerName%
DriverVer=02/12/2024,1.2.3.0
CatalogFile=test_driver.cat

[Manufacturer]
%ManufacturerName%=Standard,NTamd64

[Standard.NTamd64]
%DeviceDesc%=TestDriver_Install,PCI\VEN_8086&DEV_1234

[TestDriver_Install]
CopyFiles=Drivers_Dir

[Drivers_Dir]
test_driver.sys

[DestinationDirs]
Drivers_Dir=12

[Strings]
ManufacturerName=""Sample Driver Manufacturer""
DeviceDesc=""Sample Test Driver""
";
        File.WriteAllText(infFile, infContent);
        Console.WriteLine($"✓ 已创建测试 INF 文件 / Created test INF file: {Path.GetFileName(infFile)}");
    }
    else if (platformInfo.Platform == "Linux")
    {
        // 创建模拟的 Linux 内核模块文件 / Create mock Linux kernel module file
        var koFile = Path.Combine(testDriverDir, "test_driver.ko");
        File.WriteAllText(koFile, "Mock Linux kernel module content");
        Console.WriteLine($"✓ 已创建测试 KO 文件 / Created test KO file: {Path.GetFileName(koFile)}");
    }
    
    // 获取目录中的驱动信息 / Get drivers from directory
    var drivers = await GeneralDrivelution.GetDriversFromDirectoryAsync(testDriverDir);
    Console.WriteLine($"\n发现驱动数量 / Drivers Found: {drivers.Count}");
    
    if (drivers.Count > 0)
    {
        foreach (var driver in drivers)
        {
            Console.WriteLine($"\n驱动信息 / Driver Information:");
            Console.WriteLine($"  名称 / Name: {driver.Name}");
            Console.WriteLine($"  版本 / Version: {driver.Version}");
            Console.WriteLine($"  文件路径 / File Path: {driver.FilePath}");
            Console.WriteLine($"  目标操作系统 / Target OS: {driver.TargetOS}");
            Console.WriteLine($"  架构 / Architecture: {driver.Architecture}");
            Console.WriteLine($"  哈希值 / Hash: {driver.Hash}");
            Console.WriteLine($"  哈希算法 / Hash Algorithm: {driver.HashAlgorithm}");
        }
    }
    else
    {
        Console.WriteLine("未发现驱动文件 / No driver files found");
    }
    Console.WriteLine("\n" + new string('=', 80) + "\n");

    // ========================================
    // 4. 验证驱动文件 / Validate Driver File
    // ========================================
    Console.WriteLine("=== 4. 验证驱动文件 / Validate Driver File ===");
    
    // 创建测试驱动文件 / Create test driver file
    var testDriverFile = Path.Combine(testDriverDir, "sample_driver.sys");
    File.WriteAllText(testDriverFile, "Sample driver binary content");
    
    var driverInfo = new DriverInfo
    {
        Name = "Sample Driver",
        Version = "1.0.0",
        FilePath = testDriverFile,
        TargetOS = platformInfo.OperatingSystem,
        Architecture = platformInfo.Architecture,
        Description = "示例驱动程序 / Sample driver for demonstration",
        ReleaseDate = DateTime.Now
    };
    
    Console.WriteLine($"驱动名称 / Driver Name: {driverInfo.Name}");
    Console.WriteLine($"驱动版本 / Driver Version: {driverInfo.Version}");
    Console.WriteLine($"文件路径 / File Path: {driverInfo.FilePath}");
    
    var isValid = await GeneralDrivelution.ValidateAsync(driverInfo);
    Console.WriteLine($"\n验证结果 / Validation Result: {(isValid ? "✓ 通过/Passed" : "✗ 失败/Failed")}");
    Console.WriteLine("\n" + new string('=', 80) + "\n");

    // ========================================
    // 5. 快速更新驱动 / Quick Update Driver
    // ========================================
    Console.WriteLine("=== 5. 快速更新驱动 / Quick Update Driver ===");
    Console.WriteLine("注意：此示例仅演示API调用，不会实际安装驱动");
    Console.WriteLine("Note: This example only demonstrates API usage, will not actually install drivers\n");
    
    // 使用默认策略的快速更新 / Quick update with default strategy
    Console.WriteLine("使用默认策略 / Using default strategy:");
    Console.WriteLine("  • 需要备份 / Requires Backup: 是/Yes");
    Console.WriteLine("  • 重试次数 / Retry Count: 3");
    Console.WriteLine("  • 重试间隔 / Retry Interval: 5秒/seconds");
    
    var quickResult = await GeneralDrivelution.QuickUpdateAsync(driverInfo);
    
    Console.WriteLine($"\n更新结果 / Update Result:");
    Console.WriteLine($"  状态 / Status: {quickResult.Status}");
    Console.WriteLine($"  成功 / Success: {(quickResult.Success ? "是/Yes" : "否/No")}");
    Console.WriteLine($"  开始时间 / Start Time: {quickResult.StartTime:yyyy-MM-dd HH:mm:ss}");
    Console.WriteLine($"  结束时间 / End Time: {quickResult.EndTime:yyyy-MM-dd HH:mm:ss}");
    Console.WriteLine($"  耗时 / Duration: {quickResult.DurationMs} ms");
    Console.WriteLine($"  消息 / Message: {quickResult.Message}");
    
    if (quickResult.Error != null)
    {
        Console.WriteLine($"  错误信息 / Error: {quickResult.Error.Message}");
    }
    
    if (quickResult.StepLogs.Count > 0)
    {
        Console.WriteLine($"\n  执行步骤 / Execution Steps:");
        foreach (var log in quickResult.StepLogs)
        {
            Console.WriteLine($"    - {log}");
        }
    }
    Console.WriteLine("\n" + new string('=', 80) + "\n");

    // ========================================
    // 6. 使用自定义策略更新 / Update with Custom Strategy
    // ========================================
    Console.WriteLine("=== 6. 使用自定义策略更新 / Update with Custom Strategy ===");
    
    var strategy = new UpdateStrategy
    {
        Mode = UpdateMode.Full,
        RequireBackup = true,
        BackupPath = Path.Combine(testDriverDir, "backups"),
        RetryCount = 5,
        RetryIntervalSeconds = 3,
        RestartMode = RestartMode.Prompt,
        TimeoutSeconds = 120,
        SkipHashValidation = false,
        SkipSignatureValidation = false
    };
    
    Console.WriteLine("自定义策略配置 / Custom Strategy Configuration:");
    Console.WriteLine($"  更新模式 / Update Mode: {strategy.Mode}");
    Console.WriteLine($"  需要备份 / Require Backup: {(strategy.RequireBackup ? "是/Yes" : "否/No")}");
    Console.WriteLine($"  备份路径 / Backup Path: {strategy.BackupPath}");
    Console.WriteLine($"  重试次数 / Retry Count: {strategy.RetryCount}");
    Console.WriteLine($"  重试间隔 / Retry Interval: {strategy.RetryIntervalSeconds} 秒/seconds");
    Console.WriteLine($"  重启模式 / Restart Mode: {strategy.RestartMode}");
    Console.WriteLine($"  超时时间 / Timeout: {strategy.TimeoutSeconds} 秒/seconds");
    
    var customResult = await GeneralDrivelution.QuickUpdateAsync(driverInfo, strategy);
    
    Console.WriteLine($"\n更新结果 / Update Result:");
    Console.WriteLine($"  状态 / Status: {customResult.Status}");
    Console.WriteLine($"  成功 / Success: {(customResult.Success ? "是/Yes" : "否/No")}");
    Console.WriteLine($"  备份路径 / Backup Path: {customResult.BackupPath ?? "无/None"}");
    Console.WriteLine($"  已回滚 / Rolled Back: {(customResult.RolledBack ? "是/Yes" : "否/No")}");
    Console.WriteLine($"  耗时 / Duration: {customResult.DurationMs} ms");
    Console.WriteLine("\n" + new string('=', 80) + "\n");

    // ========================================
    // 7. 备份和回滚操作 / Backup and Rollback Operations
    // ========================================
    Console.WriteLine("=== 7. 备份和回滚操作 / Backup and Rollback Operations ===");
    
    // 备份驱动 / Backup driver
    var backupPath = Path.Combine(testDriverDir, "manual_backup");
    Directory.CreateDirectory(backupPath);
    
    Console.WriteLine($"备份驱动到 / Backing up driver to: {backupPath}");
    var backupSuccess = await updater.BackupAsync(driverInfo, backupPath);
    Console.WriteLine($"备份结果 / Backup Result: {(backupSuccess ? "✓ 成功/Success" : "✗ 失败/Failed")}");
    
    if (backupSuccess)
    {
        // 模拟回滚操作 / Simulate rollback operation
        Console.WriteLine($"\n从备份回滚驱动 / Rolling back driver from backup");
        var rollbackSuccess = await updater.RollbackAsync(backupPath);
        Console.WriteLine($"回滚结果 / Rollback Result: {(rollbackSuccess ? "✓ 成功/Success" : "✗ 失败/Failed")}");
    }
    
    Console.WriteLine("\n" + new string('=', 80) + "\n");

    // ========================================
    // 清理测试文件 / Cleanup Test Files
    // ========================================
    Console.WriteLine("=== 清理 / Cleanup ===");
    if (Directory.Exists(testDriverDir))
    {
        try
        {
            Directory.Delete(testDriverDir, true);
            Console.WriteLine("✓ 测试文件已清理 / Test files cleaned up");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ 清理失败 / Cleanup failed: {ex.Message}");
        }
    }

    Console.WriteLine("\n=== 示例程序执行完成 / Sample Program Completed ===");
    Console.WriteLine($"完成时间：{DateTime.Now}");
    Console.WriteLine($"Completion Time: {DateTime.Now}");
}
catch (Exception e)
{
    Console.WriteLine($"\n异常 / Exception: {e.Message}");
    Console.WriteLine($"堆栈跟踪 / Stack Trace:\n{e.StackTrace}");
}
