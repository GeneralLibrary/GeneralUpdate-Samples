using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;
using GeneralUpdate.Core.Download;
using GeneralUpdate.Core.Event;

namespace Hub.Samples;

public class CompleteUpdateSample : ISample
{
    public int Index => 1;
    public string Name => "完整更新 — 版本发现→下载→应用";
    public bool RequiresServer => true;
    public string[] CleanPaths => new[] {
        "{BaseDir}/mock_app",
        "{BaseDir}/fail",
        "{BaseDir}/backup",
        Path.Combine(Path.GetTempPath(), "upgrade_temp")
    };

    public async Task RunAsync(AppConfig config, CancellationToken ct)
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var mockAppDir = Path.Combine(baseDir, "mock_app");
        var configDir = Path.Combine(mockAppDir, "config");

        // ════════════════════════════════════════════
        // 1. 准备 — 创建 v1.0.0.0 版本的模拟应用
        // ════════════════════════════════════════════
        SafeDelete(mockAppDir);
        Directory.CreateDirectory(configDir);

        var v1Files = new Dictionary<string, string>
        {
            ["version.txt"] = "App v1.0.0.0\nBuild: 2024-001\nStatus: stable",
            ["appdata.json"] = "{\"app\":\"DemoApp\",\"version\":\"1.0.0.0\",\"theme\":\"light\",\"cache\":false,\"maxConn\":5}",
            ["config/settings.ini"] = "[General]\nTheme=Light\nLang=zh-CN\nLogLevel=Info\n\n[Network]\nTimeout=30\nRetry=3",
            ["readme.txt"] = "DemoApp v1.0.0.0\n===============\nBasic features only.\nNo dark mode.\nNo caching.",
            ["old_module.dat"] = "DEPRECATED: This module will be removed in v2."
        };

        Console.WriteLine();
        Console.WriteLine("══ 步骤 1/3 — 当前版本文件 (v1.0.0.0) ══");
        foreach (var (name, content) in v1Files)
        {
            var fp = Path.Combine(mockAppDir, name);
            Directory.CreateDirectory(Path.GetDirectoryName(fp)!);
            File.WriteAllText(fp, content);
            Console.WriteLine($"  {name}");
        }
        Console.WriteLine($"  共 {v1Files.Count} 个文件");

        // ════════════════════════════════════════════
        // 2. 配置 GeneralUpdateBootstrap
        // ════════════════════════════════════════════
        Console.WriteLine();
        Console.WriteLine("══ 步骤 2/3 — 配置 GeneralUpdateBootstrap ══");

        var request = new UpdateRequest
        {
            UpdateUrl = $"{config.ServerUrl}/Upgrade/Verification",
            ReportUrl = $"{config.ServerUrl}/Upgrade/Report",
            AppSecretKey = config.AppSecretKey,
            InstallPath = mockAppDir,
            UpdatePath = baseDir,  // Hub.exe 所在目录，用于启动升级进程
            ClientVersion = config.ClientVersion,
            MainAppName = config.MainAppName,
            UpdateAppName = config.UpgradeAppName,
            ProductId = config.ProductId
        };

        Console.WriteLine($"  UpdateUrl: {request.UpdateUrl}");
        Console.WriteLine($"  ClientVersion: {request.ClientVersion}");
        Console.WriteLine($"  InstallPath: {mockAppDir}");

        var bootstrap = new GeneralUpdateBootstrap()
            .SetConfig(request)
            .SetOption(Option.AppType, AppType.Client)
            .AddListenerUpdateInfo((_, e) =>
            {
                var count = e.Info?.Body?.Count ?? 0;
                Console.WriteLine();
                Console.WriteLine($"  [版本发现] 找到 {count} 个可用更新:");
                if (e.Info?.Body != null)
                {
                    foreach (var v in e.Info.Body)
                    {
                        Console.WriteLine($"    v{v.Version} — {v.Name} ({v.Size} bytes)");
                        Console.WriteLine($"      Hash: {v.Hash?[..16]}...");
                        Console.WriteLine($"      IsCrossVersion: {v.IsCrossVersion}");
                    }
                }
            })
            .AddListenerMultiDownloadStatistics((_, e) =>
            {
                Console.Write($"\r  [下载] {e.BytesReceived}/{e.TotalBytesToReceive} bytes ({e.ProgressPercentage:F0}%)  {e.Speed}");
            })
            .AddListenerMultiDownloadCompleted((_, e) =>
            {
                Console.WriteLine();
                Console.WriteLine($"  [下载完成] {(e.IsCompleted ? "✓ 全部成功" : "✗ 存在失败")}");
            })
            .AddListenerProgress((_, e) =>
            {
                if (e.Progress != null)
                {
                    var p = e.Progress;
                    Console.WriteLine($"  [下载进度] {p.AssetName}: {p.Percentage:F0}% ({p.Status})");
                }
            })
            .AddListenerException((_, e) =>
            {
                Console.WriteLine();
                Console.WriteLine($"  [异常] {e.Message}");
            });

        Console.WriteLine();
        Console.WriteLine("══ 步骤 3/3 — 执行更新流程 ══");

        try
        {
            await bootstrap.LaunchAsync();

            Console.WriteLine();
            Console.WriteLine("══ 更新结果 ══");

            // 检查更新后的文件
            if (Directory.Exists(mockAppDir))
            {
                var files = Directory.GetFiles(mockAppDir, "*", SearchOption.AllDirectories);
                Console.WriteLine($"  已更新: {files.Length} 个文件");
                foreach (var f in files.OrderBy(x => x))
                {
                    var rel = Path.GetRelativePath(mockAppDir, f);
                    var content = File.Exists(f) ? File.ReadAllText(f) : "<<ERROR>>";
                    var truncated = content.Length > 80 ? content[..80] + "..." : content;
                    Console.WriteLine($"    {rel}: {truncated.Replace("\n", "\\n")}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine($"  [错误] 更新流程异常: {ex.Message}");
        }

        Console.WriteLine();
        Console.WriteLine("══ 完成 ✓ ══");
    }

    static void SafeDelete(string p) { try { if (Directory.Exists(p)) Directory.Delete(p, true); } catch { } }
}
