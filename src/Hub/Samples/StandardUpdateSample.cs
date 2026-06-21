using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;
using GeneralUpdate.Core.Download;
using GeneralUpdate.Core.Event;

namespace Hub.Samples;

/// <summary>
/// 标准更新流程 — 最简化的生产级集成方式。
/// 展示从配置到执行的完整链路，不包含模拟数据或文件操作。
/// </summary>
/// <remarks>
/// 相比 <see cref="CompleteUpdateSample"/>，本示例省略了模拟应用的创建和
/// 更新后的文件遍历比对，聚焦于 GeneralUpdateBootstrap 的核心配置模式。
/// </remarks>
public class StandardUpdateSample : ISample
{
    public int Index => 1;
    public string Name => "标准更新流程 — Bootstrap·配置·执行";
    public bool RequiresServer => true;
    public string[] CleanPaths => Array.Empty<string>();

    public async Task RunAsync(AppConfig config, CancellationToken ct)
    {
        Console.WriteLine();
        Console.WriteLine("══ 标准更新流程 (v10.5.0-beta.7) ══");

        // ════════════════════════════════════════════════════════════════
        // 1. 组装请求参数
        // ════════════════════════════════════════════════════════════════
        var request = new UpdateRequest
        {
            UpdateUrl = $"{config.ServerUrl}/Upgrade/Verification",
            ReportUrl = $"{config.ServerUrl}/Upgrade/Report",
            AppSecretKey = config.AppSecretKey,
            InstallPath = AppDomain.CurrentDomain.BaseDirectory,
            UpdatePath = AppDomain.CurrentDomain.BaseDirectory,
            ClientVersion = config.ClientVersion,
            MainAppName = config.MainAppName,
            UpdateAppName = config.UpgradeAppName,
            ProductId = config.ProductId
        };

        Console.WriteLine();
        Console.WriteLine("══ 配置 ══");
        Console.WriteLine($"  Server:  {request.UpdateUrl}");
        Console.WriteLine($"  Version: {request.ClientVersion}");
        Console.WriteLine($"  AppKey:  {request.AppSecretKey?[..Math.Min(8, request.AppSecretKey?.Length ?? 0)]}...");

        // ════════════════════════════════════════════════════════════════
        // 2. 构建 Bootstrap
        // ════════════════════════════════════════════════════════════════
        Console.WriteLine();
        Console.WriteLine("══ 初始化 GeneralUpdateBootstrap ══");

        var bootstrap = new GeneralUpdateBootstrap()

            // — 必要：请求配置 —
            .SetConfig(request)

            // — 必要：指定应用类型（Client=主应用, Upgrade=升级器） —
            .SetOption(Option.AppType, AppType.Client)

            // — 必要：版本发现事件 —
            .AddListenerUpdateInfo((_, e) =>
            {
                var count = e.Info?.Body?.Count ?? 0;
                // 服务端可能在多个版本中返回 full + chain 包供客户端选择
                // 客户端会按 PackageType 自动排序：full 优先
                Console.WriteLine($"  [版本发现] 找到 {count} 个可用更新");
            })

            // — 可选：下载统计（多文件并行） —
            .AddListenerMultiDownloadStatistics((_, e) =>
            {
                Console.Write($"\r  [下载] {e.BytesReceived}/{e.TotalBytesToReceive} bytes ({e.ProgressPercentage:F0}%)  {e.Speed}");
            })

            // — 可选：下载完成 —
            .AddListenerMultiDownloadCompleted((_, e) =>
            {
                Console.WriteLine();
                Console.WriteLine($"  [下载完成] {(e.IsCompleted ? "✓" : "✗ 存在失败")}");
            })

            // — 可选：单文件进度 —
            .AddListenerProgress((_, e) =>
            {
                if (e.Progress is { } p)
                    Console.WriteLine($"    {p.AssetName}: {p.Percentage:F0}% ({p.Status})");
            })

            // — 可选：异常通知 —
            .AddListenerException((_, e) =>
            {
                Console.WriteLine($"  [异常] {e.Message}");
            });

        // ════════════════════════════════════════════════════════════════
        // 3. 执行更新
        // ════════════════════════════════════════════════════════════════
        Console.WriteLine();
        Console.WriteLine("══ 启动更新 (LaunchAsync) ══");
        Console.WriteLine("  流程: 版本发现 → 下载 → 解压 → 备份旧版本 → 应用新版本");

        try
        {
            await bootstrap.LaunchAsync();
            Console.WriteLine();
            Console.WriteLine("══ 更新完成 ✓ ══");
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine($"══ 更新失败: {ex.Message} ══");
        }
    }
}
