using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;
using GeneralUpdate.Core.Download;
using GeneralUpdate.Core.Event;

namespace Hub.Samples;

public class OssSample : ISample
{
    public int Index => 3;
    public string Name => "OSS 模式 — 对象存储更新";
    public bool RequiresServer => true;
    public string[] CleanPaths => new[] {
        "{BaseDir}/fail",
        "{BaseDir}/backup",
        Path.Combine(Path.GetTempPath(), "upgrade_temp"),
        Path.Combine(Path.GetTempPath(), "oss_demo")
    };

    public async Task RunAsync(AppConfig config, CancellationToken ct)
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var ossDemoDir = Path.Combine(Path.GetTempPath(), "oss_demo", Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(ossDemoDir);

        var versionsUrl = $"{config.ServerUrl}/packages/versions.json";

        // ════════════════════════════════════════════
        // 1. 说明 OSS 模式
        // ════════════════════════════════════════════
        Console.WriteLine();
        Console.WriteLine("══ OSS 模式说明 ══");
        Console.WriteLine("  OSS (Object Storage Service) 模式适用于:");
        Console.WriteLine("  · 阿里云 OSS / AWS S3 / MinIO 等对象存储");
        Console.WriteLine("  · 版本信息通过 versions.json 静态文件分发");
        Console.WriteLine("  · 客户端直接下载, 无需 Verification/Report API");
        Console.WriteLine();
        Console.WriteLine("══ 步骤 1/3 — 配置 ══");
        Console.WriteLine($"  版本清单: {versionsUrl}");
        Console.WriteLine($"  当前版本: {config.ClientVersion}");
        Console.WriteLine($"  安装路径: {ossDemoDir}");

        // ════════════════════════════════════════════
        // 2. 配置 GeneralUpdateBootstrap — OSS 模式
        // ════════════════════════════════════════════
        Console.WriteLine();
        Console.WriteLine("══ 步骤 2/3 — GeneralUpdateBootstrap (OssClient) ══");

        var request = new UpdateRequest
        {
            UpdateUrl = versionsUrl,
            AppSecretKey = config.AppSecretKey,
            InstallPath = ossDemoDir,
            UpdatePath = baseDir,
            UpdateAppName = config.UpgradeAppName,
            MainAppName = config.MainAppName,
            ClientVersion = config.ClientVersion
        };

        var bootstrap = new GeneralUpdateBootstrap()
            .SetConfig(request)
            .SetOption(Option.AppType, AppType.OssClient)
            .AddListenerUpdateInfo((_, e) =>
            {
                var count = e.Info?.Body?.Count ?? 0;
                Console.WriteLine();
                Console.WriteLine($"  [版本发现] 找到 {count} 个可用更新:");
                if (e.Info?.Body != null)
                {
                    foreach (var v in e.Info.Body)
                    {
                        var type = v.IsCrossVersion == true
                            ? $"[差分 {v.FromVersion}→{v.ToVersion}]"
                            : "[完整包]";
                        Console.WriteLine($"    v{v.Version}  {type}");
                        Console.WriteLine($"      Name: {v.Name}");
                        Console.WriteLine($"      Size: {v.Size} bytes");
                        Console.WriteLine($"      Hash: {v.Hash?[..16]}...");
                    }
                }
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
        Console.WriteLine("══ 步骤 3/3 — 执行 OSS 更新 ══");

        try
        {
            await bootstrap.LaunchAsync();

            Console.WriteLine();
            Console.WriteLine("══ OSS 更新结果 ══");

            // 检查更新后的文件
            if (Directory.Exists(ossDemoDir))
            {
                var files = Directory.GetFiles(ossDemoDir, "*", SearchOption.AllDirectories);
                if (files.Length > 0)
                {
                    Console.WriteLine($"  已写入 {files.Length} 个文件:");
                    foreach (var f in files.OrderBy(x => x).Take(10))
                    {
                        var rel = Path.GetRelativePath(ossDemoDir, f);
                        var fi = new FileInfo(f);
                        Console.WriteLine($"    {rel} ({fi.Length} bytes)");
                    }
                    if (files.Length > 10)
                        Console.WriteLine($"    ... 还有 {files.Length - 10} 个文件");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine($"  [错误] OSS 更新异常: {ex.Message}");
        }

        Console.WriteLine();
        Console.WriteLine("══ OSS 模式要点 ══");
        Console.WriteLine("  ① versions.json 放在 OSS bucket 中");
        Console.WriteLine("  ② UpdateUrl 指向该文件的公开 URL");
        Console.WriteLine("  ③ AppType = OssClient (主应用) / OssUpgrade (升级进程)");
        Console.WriteLine("  ④ 包文件也放在 OSS 上, 通过 hash 或 URL 定位");
        Console.WriteLine("  ⑤ 无需 Verification/Report 接口 — 直接下载即可");

        SafeDelete(ossDemoDir);
        Console.WriteLine();
        Console.WriteLine("══ 完成 ✓ ══");
    }

    static void SafeDelete(string p) { try { if (Directory.Exists(p)) Directory.Delete(p, true); } catch { } }
}
