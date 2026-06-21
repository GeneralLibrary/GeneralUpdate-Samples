using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;
using GeneralUpdate.Core.Download;
using GeneralUpdate.Core.Event;

namespace Hub.Samples;

public class SilentUpdateSample : ISample
{
    public int Index => 3;
    public string Name => "静默更新 — 后台轮询·退出前准备";
    public bool RequiresServer => true;
    public string[] CleanPaths => new[] {
        "{BaseDir}/fail",
        "{BaseDir}/backup",
        Path.Combine(Path.GetTempPath(), "upgrade_temp")
    };

    public async Task RunAsync(AppConfig config, CancellationToken ct)
    {
        Console.WriteLine();
        Console.WriteLine("══ 准备 — 静默模式配置 ══");

        var request = new UpdateRequest
        {
            UpdateUrl = $"{config.ServerUrl}/Upgrade/Verification",
            ReportUrl = $"{config.ServerUrl}/Upgrade/Report",
            AppSecretKey = config.AppSecretKey,
            InstallPath = AppDomain.CurrentDomain.BaseDirectory,
            ClientVersion = config.ClientVersion,
            MainAppName = config.MainAppName,
            UpdateAppName = config.UpgradeAppName,
            ProductId = config.ProductId
        };

        Console.WriteLine($"  版本: {config.ClientVersion}");
        Console.WriteLine($"  轮询: 每 1 分钟");
        Console.WriteLine($"  Server: {request.UpdateUrl}");
        Console.WriteLine();

        var bootstrap = new GeneralUpdateBootstrap()
            .SetConfig(request)
            .SetOption(Option.AppType, AppType.Client)
            .SetOption(Option.Silent, true)
            .SetOption(Option.SilentPollIntervalMinutes, 1)
            .AddListenerUpdateInfo((_, e) =>
            {
                var count = e.Info?.Body?.Count ?? 0;
                Console.WriteLine($"[轮询 {DateTime.Now:HH:mm:ss}] 发现 {count} 个可用更新");
            })
            .AddListenerException((_, e) =>
            {
                Console.WriteLine($"[异常] {e.Message}");
            });

        Console.Write("══ 启动静默模式... ");
        await bootstrap.LaunchAsync();
        Console.WriteLine("已启动 ✓");
        Console.WriteLine();

        // 模拟应用运行 — 同时等待后台轮询完成更新准备
        Console.WriteLine("══ 应用运行中 (后台静默轮询) ══");
        for (int i = 0; i < 5 && !ct.IsCancellationRequested; i++)
        {
            Console.WriteLine($"  [{DateTime.Now:HH:mm:ss}] 主程序工作中... (第 {i + 1} 秒)");
            await Task.Delay(1000, ct);
        }

        // 等待后台轮询完成（最多等 60 秒）
        Console.WriteLine();
        Console.WriteLine("══ 等待后台更新准备完成... ══");
        var waited = 0;
        while (bootstrap.SilentOrchestrator?.HasPreparedUpdate != true && waited < 60 && !ct.IsCancellationRequested)
        {
            await Task.Delay(500, ct);
            waited++;
        }

        Console.WriteLine();
        Console.WriteLine("══ 退出时自动准备更新包 ══");

        if (bootstrap.SilentOrchestrator?.HasPreparedUpdate == true)
        {
            Console.WriteLine("  更新包已准备完毕，正在启动升级...");
            var launched = bootstrap.SilentOrchestrator.TryLaunchUpgrade();
            if (launched)
            {
                Console.WriteLine("  升级进程已启动 ✓ (当前进程即将退出)");
                Console.WriteLine();
                Console.WriteLine("══ 完成 ✓ ══");
                Environment.Exit(0);
            }
            else
            {
                Console.WriteLine("  无待应用的客户端更新包");
            }
        }
        else
        {
            Console.WriteLine("  更新尚未准备完成（后台轮询可能仍在运行中）");
            Console.WriteLine("  提示：退出 Hub 时将自动触发升级");
        }

        Console.WriteLine();
        Console.WriteLine("══ 完成 ✓ ══");
    }
}
