using GeneralUpdate.Core.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace Hub.Samples;

public class PushSample : ISample
{
    public int Index => 5;
    public string Name => "SignalR 推送 — 实时消息接收";
    public bool RequiresServer => false;
    public string[] CleanPaths => Array.Empty<string>();

    public async Task RunAsync(AppConfig config, CancellationToken ct)
    {
        var port = 5188;
        var hubUrl = $"http://localhost:{port}/UpgradeHub";

        // ════════════════════════════════════════════
        // 1. 启动内置 SignalR Hub 服务端
        // ════════════════════════════════════════════
        Console.WriteLine();
        Console.WriteLine("══ 准备 — 启动内置 SignalR Hub ══");
        Console.WriteLine($"  地址: {hubUrl}");

        var builder = WebApplication.CreateBuilder();
        builder.Services.AddSignalR();
        ((IWebHostBuilder)builder.WebHost).UseUrls($"http://localhost:{port}");
        var app = builder.Build();
        app.MapHub<DemoHub>("/UpgradeHub");

        await app.StartAsync(ct);
        var serverTask = Task.CompletedTask; // Server runs on a background host; stop via app.StopAsync
        await Task.Delay(500, ct);

        Console.WriteLine("  Hub 已启动 ✓");

        // ════════════════════════════════════════════
        // 2. 使用 GeneralUpdate.UpgradeHubService 连接
        // ════════════════════════════════════════════
        Console.WriteLine();
        Console.WriteLine("══ 连接 — UpgradeHubService 订阅 ══");

        var received = new List<string>();

        // UpgradeHubService 是 GeneralUpdate 内置的 SignalR 推送客户端
        var hubService = new UpgradeHubService(hubUrl, appkey: config.AppSecretKey);

        hubService.AddListenerReceive(msg =>
        {
            received.Add(msg);
            Console.WriteLine();
            Console.WriteLine($"  ← [ReceiveMessage] {msg}");
        });

        hubService.AddListenerOnline(msg =>
        {
            Console.WriteLine($"  ← [Online] {msg}");
        });

        hubService.AddListenerReconnected(_ =>
        {
            Console.WriteLine("  [重连] 已恢复连接");
            return Task.CompletedTask;
        });

        hubService.AddListenerClosed(ex =>
        {
            Console.WriteLine($"  [连接关闭] {(ex?.Message ?? "正常断开")}");
            return Task.CompletedTask;
        });

        Console.Write("  正在连接... ");
        await hubService.StartAsync();
        Console.WriteLine("已连接 ✓");

        // ════════════════════════════════════════════
        // 3. 服务端推送消息
        // ════════════════════════════════════════════
        Console.WriteLine();
        Console.WriteLine("══ 推送 — 模拟服务端下发更新 ══");

        var hubContext = app.Services.GetRequiredService<IHubContext<DemoHub>>();

        var messages = new[]
        {
            "{\"action\":\"update_available\",\"version\":\"v2.0.0.0\",\"desc\":\"新版本已发布 — 支持暗色模式、缓存优化\"}",
            "{\"action\":\"update_progress\",\"step\":\"prepare\",\"pct\":30}",
            "{\"action\":\"update_progress\",\"step\":\"prepare\",\"pct\":65}",
            "{\"action\":\"update_progress\",\"step\":\"prepare\",\"pct\":100}",
            "{\"action\":\"update_ready\",\"version\":\"v2.0.0.0\",\"msg\":\"更新包已就绪，请重启应用\"}"
        };

        foreach (var msg in messages)
        {
            await Task.Delay(800, ct);
            // 使用 UpgradeHubService 期望的方法名
            await hubContext.Clients.All.SendAsync("ReceiveMessage", msg, cancellationToken: ct);
        }

        await Task.Delay(500, ct);

        // ════════════════════════════════════════════
        // 4. 断开 & 结果
        // ════════════════════════════════════════════
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine("══ 结果 ══");
        Console.WriteLine($"  客户端收到 {received.Count} 条推送消息:");

        for (int i = 0; i < received.Count; i++)
        {
            Console.WriteLine($"    {i + 1}. {received[i]}");
        }

        Console.WriteLine();
        Console.Write("══ 断开连接... ");
        await hubService.StopAsync();
        await hubService.DisposeAsync();

        // 停止内置 Server
        await app.StopAsync(ct);

        Console.WriteLine("✓");
        Console.WriteLine();
        Console.WriteLine("══ UpgradeHubService 要点 ══");
        Console.WriteLine("  ① 服务器配置 /UpgradeHub SignalR Hub 端点");
        Console.WriteLine("  ② 客户端使用 UpgradeHubService 连接");
        Console.WriteLine("  ③ ReceiveMessage — 接收推送的更新消息");
        Console.WriteLine("  ④ Online — 在线状态通知");
        Console.WriteLine("  ⑤ 支持自动重连 (RandomRetryPolicy)");

        Console.WriteLine();
        Console.WriteLine("══ 完成 ✓ ══");
    }
}

/// <summary>
/// 演示用 SignalR Hub — 对应 GeneralUpdate Server 的 /UpgradeHub 端点。
/// 服务端通过 ReceiveMessage 方法向客户端推送更新通知。
/// </summary>
public class DemoHub : Microsoft.AspNetCore.SignalR.Hub
{
    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync("ReceiveMessage",
            "{\"action\":\"welcome\",\"msg\":\"已连接到 UpgradeHub, 等待更新推送...\"}");
        await base.OnConnectedAsync();
    }
}
